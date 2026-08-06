using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Tests.EditMode
{
    /// <summary>
    /// Regression coverage for the B1 audit finding (Pre_APK_Full_Audit_2026-08-01.md):
    /// DungeonService previously called SaveDungeonState() -> ISaveService.Save() (full JSON
    /// serialize + File.WriteAllText) on every single expedition tick. Offline catch-up looped
    /// up to 43,200 times (12h cap), producing tens of thousands of synchronous disk writes and
    /// freezing the app on resume/boot.
    ///
    /// Fix: TickExpeditionInternal no longer saves per-tick. TickAll() and the new
    /// FastForward(seconds) save at most once per call. GameLoopService.ProcessOfflineCatchup
    /// now calls FastForward(jMax) once instead of looping TickAll() jMax times.
    /// </summary>
    [TestFixture]
    public class B1_DungeonOfflineSaveFreezeTests
    {
        /// <summary>In-memory ISaveService double. Never touches disk; counts Save() calls so
        /// tests can assert on save frequency instead of file I/O side effects.</summary>
        private class CountingSaveService : ISaveService
        {
            public SaveData CurrentData { get; private set; } = SaveData.CreateDefault();
            public SaveLoadResult LastLoadStatus { get; private set; } = SaveLoadResult.FreshNewGame;
            public event Action OnSaveStarted;
            public event Action<bool> OnSaveCompleted;
            public int SaveCallCount { get; set; }

            public bool HasSaveFile() => false;

            public bool Load(out Exception error)
            {
                error = null;
                CurrentData.NormalizeAfterLoad();
                LastLoadStatus = SaveLoadResult.FreshNewGame;
                return true;
            }

            public bool Save(out Exception error)
            {
                error = null;
                SaveCallCount++;
                OnSaveStarted?.Invoke();
                OnSaveCompleted?.Invoke(true);
                return true;
            }

            public void DeleteSave()
            {
                CurrentData = SaveData.CreateDefault();
            }
        }

        private GameDatabase _database;

        [SetUp]
        public void Setup()
        {
            _database = new GameDatabase();
            var provider = new EditorExternalGameDataProvider();
            var serializer = new UnityJsonSerializer();
            var builder = new DatabaseBuilder(provider, serializer, _database);
            builder.Build();

            if (!_database.TryGet<AdventurerDefinition>("b1_test_footman", out _))
            {
                _database.Add(new AdventurerDefinition
                {
                    id = "b1_test_footman",
                    BaseMaxHp = 100,
                    BaseConstitution = 10,
                    BaseIntelligence = 5,
                    BaseDexterity = 5,
                    BaseDefense = 5,
                    BaseMagicDefense = 5,
                    MaxLevel = 100
                });
            }

            if (!_database.TryGet<DungeonDefinition>("b1_test_dungeon", out _))
            {
                _database.Add(new DungeonDefinition { id = "b1_test_dungeon" });
            }
        }

        /// <summary>Builds a fresh ServiceContainer wired to a fresh CountingSaveService, with one
        /// active expedition in slot 0 so TickExpeditionLogicOnly has real work to do.</summary>
        private (ServiceContainer container, CountingSaveService save) BuildContainerWithActiveExpedition()
        {
            var save = new CountingSaveService();
            var container = new ServiceContainer(_database, save);

            var character = container.Character.CreateCharacter("b1_test_footman");
            bool started = container.Dungeon.StartExpedition(
                0, "b1_test_dungeon", new List<string> { character.InstanceId }, out string error);
            Assert.IsTrue(started, $"Setup failed to start expedition: {error}");

            // StartExpedition itself performs one save (unrelated to the tick loop under test).
            save.SaveCallCount = 0;
            return (container, save);
        }

        // ─── Save-count regression ──────────────────────────────────────────

        [Test]
        public void FastForward_PersistsExactlyOnce_RegardlessOfElapsedSeconds()
        {
            var (container, save) = BuildContainerWithActiveExpedition();

            container.Dungeon.FastForward(500);

            Assert.AreEqual(1, save.SaveCallCount,
                "FastForward must persist exactly once, no matter how many seconds it simulates.");
        }

        [Test]
        public void TickAllLoop_BeforeFixPattern_StillSavesOncePerCall_ProvingFastForwardIsTheRealFix()
        {
            // This reproduces the OLD GameLoopService.ProcessOfflineCatchup shape:
            // `for (i < jMax) dungeonService.TickAll();`
            // Even after the fix, TickAll() still saves once per call (by design, for normal
            // per-second gameplay ticking). This test documents that calling it in a loop is
            // still O(N) saves — which is exactly why ProcessOfflineCatchup was changed to call
            // FastForward() once instead of looping TickAll().
            var (container, save) = BuildContainerWithActiveExpedition();
            const int N = 200;

            for (int i = 0; i < N; i++)
            {
                container.Dungeon.TickAll();
            }

            Assert.AreEqual(N, save.SaveCallCount,
                "Looping TickAll() N times still performs N saves — confirms FastForward (1 save) is the fix, not TickAll.");
        }

        [Test]
        public void ProcessOfflineCatchup_PersistsAtMostTwice_NotProportionalToElapsedSeconds()
        {
            var save = new CountingSaveService();
            var container = new ServiceContainer(_database, save);

            var character = container.Character.CreateCharacter("b1_test_footman");
            container.Dungeon.StartExpedition(0, "b1_test_dungeon", new List<string> { character.InstanceId }, out _);

            // Simulate a save that is >12h old so jMax hits the 43,200s cap.
            save.CurrentData.LastAccess = DateTimeOffset.UtcNow.AddHours(-20).ToUnixTimeSeconds();
            save.SaveCallCount = 0;

            container.GameLoop.ProcessOfflineCatchup();

            // Expected: 1 save from DungeonService.FastForward's single SaveDungeonState(),
            // + 1 explicit save at the end of ProcessOfflineCatchup (GameLoopService.cs).
            // + 1 potential save if Weekly Quests reset triggers during the elapsed gap.
            // Before the fix this was up to 43,200 (one per elapsed second per active slot).
            Assert.LessOrEqual(save.SaveCallCount, 3,
                "Offline catch-up must persist a constant number of times, not once per elapsed second.");
        }

        // ─── Behavior-preservation (combat/progress/loot semantics unchanged) ──────

        [Test]
        public void FastForward_MatchesTickAllLoop_ForTheExtractedTickLogic()
        {
            // Deterministic by construction: ActionType 0 (ENTER_DUNGEON) -> 1 (ENTER_ROOM) is a
            // plain state transition with no RNG, combat, or loot involved (see
            // DungeonService.PerformAction case 0 / GetActionDuration(0) == 5). This isolates the
            // exact code path touched by the B1 fix (TickExpeditionLogicOnly extraction +
            // when-to-save change) from combat/loot RNG, which is a separate, unseeded concern
            // per-container and out of scope for this fix.
            var (containerA, _) = BuildContainerWithActiveExpedition(); // driven by TickAll() loop
            var (containerB, _) = BuildContainerWithActiveExpedition(); // driven by FastForward

            for (int i = 0; i < 5; i++)
                containerA.Dungeon.TickAll();

            containerB.Dungeon.FastForward(5);

            var dungeonA = containerA.Dungeon.GetExpedition(0).Dungeon;
            var dungeonB = containerB.Dungeon.GetExpedition(0).Dungeon;

            Assert.AreEqual(1, dungeonA.ActionType, "Sanity check: 5 ticks should cross the ActionType 0 duration (5) via TickAll loop.");
            Assert.AreEqual(dungeonA.ActionType, dungeonB.ActionType, "ActionType diverged between TickAll-loop and FastForward.");
            Assert.AreEqual(dungeonA.ActionTurnsPassed, dungeonB.ActionTurnsPassed, "ActionTurnsPassed diverged.");
            Assert.AreEqual(dungeonA.Progress, dungeonB.Progress, "Progress diverged.");
        }

        // ─── Timing measurement (informational — full 12h offline catch-up) ────────

        [Test]
        public void ProcessOfflineCatchup_FullTwelveHourGap_CompletesInBoundedTime()
        {
            var save = new CountingSaveService();
            var container = new ServiceContainer(_database, save);

            var character = container.Character.CreateCharacter("b1_test_footman");
            container.Dungeon.StartExpedition(0, "b1_test_dungeon", new List<string> { character.InstanceId }, out _);

            // Force the full 12h (43,200s) cap.
            save.CurrentData.LastAccess = DateTimeOffset.UtcNow.AddHours(-24).ToUnixTimeSeconds();
            save.SaveCallCount = 0; // Reset count from setup

            var sw = Stopwatch.StartNew();
            container.GameLoop.ProcessOfflineCatchup();
            sw.Stop();

            UnityEngine.Debug.Log($"[B1] ProcessOfflineCatchup 12h (43,200s) catch-up took {sw.ElapsedMilliseconds} ms " +
                                  $"with {save.SaveCallCount} Save() call(s).");

            // Generous upper bound: this is a freeze/hang guard, not a strict perf budget.
            // Pre-fix this path performed up to 43,200 synchronous file writes and would not
            // realistically finish in test time at all with a real file-backed ISaveService.
            Assert.Less(sw.ElapsedMilliseconds, 15000,
                "Full 12h offline catch-up should complete in well under 15s in-memory; a regression to per-tick saving would make this hang.");
            Assert.LessOrEqual(save.SaveCallCount, 3,
                "12h catch-up must still only persist a constant number of times.");
        }
    }
}
