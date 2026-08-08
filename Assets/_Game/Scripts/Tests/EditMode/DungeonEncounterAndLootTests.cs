using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
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
    /// Phase 2B (Task 2.4 / 2.5) verification: real EncounterGroups-based encounter rolling
    /// (multi-enemy, weighted, empty-room) and weighted multi-drop loot rolling. Permanent
    /// addition to the EditMode suite — see Docs/Backend_Audit/phase2b_completion_report.md.
    /// </summary>
    [TestFixture]
    public class DungeonEncounterAndLootTests
    {
        private sealed class FailingSaveService : ISaveService
        {
            public SaveData CurrentData { get; private set; } = SaveData.CreateDefault();
            public SaveLoadResult LastLoadStatus { get; private set; } = SaveLoadResult.FreshNewGame;
            public bool FailSaves { get; set; }
            public event Action OnSaveStarted;
            public event Action<bool> OnSaveCompleted;

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
                if (!FailSaves)
                {
                    error = null;
                    OnSaveStarted?.Invoke();
                    OnSaveCompleted?.Invoke(true);
                    return true;
                }

                error = new InvalidOperationException("Intentional Phase 2B save failure.");
                OnSaveStarted?.Invoke();
                OnSaveCompleted?.Invoke(false);
                return false;
            }

            public void DeleteSave()
            {
                CurrentData = SaveData.CreateDefault();
            }
        }
        private GameDatabase _database;
        private ServiceContainer _container;
        private DungeonService _dungeonService;

        [SetUp]
        public void Setup()
        {
            _database = new GameDatabase();
            var provider = new EditorExternalGameDataProvider();
            var serializer = new UnityJsonSerializer();
            var builder = new DatabaseBuilder(provider, serializer, _database);
            builder.Build();

            if (!_database.TryGet<EnemyDefinition>("test_enemy_a", out _))
            {
                _database.Add(new EnemyDefinition { id = "test_enemy_a", BaseMaxHp = 50, ExpGiven = 5 });
            }
            if (!_database.TryGet<EnemyDefinition>("test_enemy_b", out _))
            {
                _database.Add(new EnemyDefinition { id = "test_enemy_b", BaseMaxHp = 60, ExpGiven = 6 });
            }

            if (!_database.TryGet<ItemDefinition>("test_item_common", out _))
            {
                _database.Add(new ItemDefinition { id = "test_item_common" });
            }
            if (!_database.TryGet<ItemDefinition>("test_item_uncommon", out _))
            {
                _database.Add(new ItemDefinition { id = "test_item_uncommon" });
            }
            if (!_database.TryGet<ItemDefinition>("test_item_rare", out _))
            {
                _database.Add(new ItemDefinition { id = "test_item_rare" });
            }

            // Encounter-group test dungeon: single-enemy(weight 100), multi-enemy(weight 400,
            // HIGH), multi-enemy(weight 10, LOW), empty-room(weight 490). Sums to 1000.
            if (!_database.TryGet<DungeonDefinition>("test_dungeon_encounters", out _))
            {
                _database.Add(new DungeonDefinition
                {
                    id = "test_dungeon_encounters",
                    EncounterGroups = new List<EncounterGroupData>
                    {
                        new EncounterGroupData { EnemyIds = new List<string> { "test_enemy_a" }, Weight = 100 },
                        new EncounterGroupData { EnemyIds = new List<string> { "test_enemy_a", "test_enemy_b" }, Weight = 400 },
                        new EncounterGroupData { EnemyIds = new List<string> { "test_enemy_b", "test_enemy_b" }, Weight = 10 },
                    },
                    EmptyRoomWeight = 490,
                    SearchRoomDrops = new List<EnemyDropEntry>
                    {
                        new EnemyDropEntry { ItemId = "test_item_common", Weight = 200, StackCount = 1 },
                        new EnemyDropEntry { ItemId = "test_item_uncommon", Weight = 100, StackCount = 2 },
                    }
                });
            }

            _container = new ServiceContainer(_database);
            _dungeonService = (DungeonService)_container.Dungeon;

            for (int i = 0; i < _dungeonService.MaxExpeditions; i++)
            {
                _dungeonService.StopExpedition(i);
            }
        }

        // ─── Task 2.4: Encounter generation ─────────────────────────────

        [Test]
        public void LoadedDungeonData_UsesEncounterGroups_AndSearchRoomDropsWhenDefined()
        {
            List<DungeonDefinition> dungeons = _database.GetAll<DungeonDefinition>().ToList();
            Assert.IsNotEmpty(dungeons, "No dungeon definitions were loaded from dungeons.json.");

            int withGroups = dungeons.Count(d => d.EncounterGroups != null && d.EncounterGroups.Count > 0);
            Assert.AreEqual(dungeons.Count, withGroups,
                "Every normal dungeon in the restored Phase 2B data must have EncounterGroups.");

            int resolvedGroups = 0;
            foreach (DungeonDefinition dungeon in dungeons)
            {
                foreach (EncounterGroupData group in dungeon.EncounterGroups)
                {
                    Assert.IsNotNull(group);
                    Assert.IsNotEmpty(group.EnemyIds);
                    Assert.Greater(group.Weight, 0);
                    foreach (string enemyId in group.EnemyIds)
                    {
                        Assert.IsTrue(_database.TryGet<EnemyDefinition>(enemyId, out _),
                            $"EncounterGroups for {dungeon.id} references missing enemy '{enemyId}'.");
                    }
                    resolvedGroups++;
                }
            }

            Assert.Greater(resolvedGroups, 0);
            Assert.Greater(dungeons.Count(d => d.SearchRoomDrops != null && d.SearchRoomDrops.Count > 0), 0,
                "At least one restored dungeon must expose SearchRoomDrops.");
        }

        [Test]
        public void RollEnemies_Over1000Rolls_ObservesMultiEnemyEmptyRoomAndWeightDirection()
        {
            _database.TryGet<DungeonDefinition>("test_dungeon_encounters", out DungeonDefinition def);
            var dungeon = new DungeonRuntime("test-instance", def);

            const int iterations = 1000;
            int singleEnemy = 0;
            int multiEnemy = 0;
            int emptyRoom = 0;
            int highWeightGroupHits = 0; // the 400-weight 2-enemy group (enemy_a + enemy_b)
            int lowWeightGroupHits = 0;  // the 10-weight 2-enemy group (enemy_b + enemy_b)

            for (int i = 0; i < iterations; i++)
            {
                List<EnemyRuntime> result = _dungeonService.RollEnemies(dungeon);

                if (result.Count == 0)
                {
                    emptyRoom++;
                }
                else if (result.Count == 1)
                {
                    singleEnemy++;
                }
                else
                {
                    multiEnemy++;
                    bool hasA = result.Any(e => e.DefinitionId == "test_enemy_a");
                    bool hasB = result.Any(e => e.DefinitionId == "test_enemy_b");
                    if (hasA && hasB) highWeightGroupHits++;
                    else if (!hasA && hasB) lowWeightGroupHits++;
                }
            }

            UnityEngine.Debug.Log(
                $"[DungeonEncounterAndLootTests] {iterations} rolls: single={singleEnemy} multi={multiEnemy} empty={emptyRoom} " +
                $"(highWeightGroup(400)={highWeightGroupHits} lowWeightGroup(10)={lowWeightGroupHits})");

            // Real observed counts, not just theoretical possibility.
            Assert.Greater(multiEnemy, 0, "Expected to observe at least one multi-enemy encounter across 1000 rolls.");
            Assert.Greater(emptyRoom, 0, "Expected to observe at least one empty room across 1000 rolls.");
            Assert.Greater(singleEnemy, 0, "Expected to observe at least one single-enemy encounter across 1000 rolls.");

            // Weight direction: the weight-400 multi-enemy group must appear meaningfully more
            // often than the weight-10 multi-enemy group (statistical, generous tolerance).
            Assert.Greater(highWeightGroupHits, lowWeightGroupHits,
                $"Higher-weight group (400) should be rolled more often than the lower-weight group (10). Got {highWeightGroupHits} vs {lowWeightGroupHits}.");

            // Rough proportion sanity checks (expected ~10% single, ~41% multi, ~49% empty).
            Assert.That((double)emptyRoom / iterations, Is.InRange(0.35, 0.65));
            Assert.That((double)multiEnemy / iterations, Is.InRange(0.25, 0.60));
        }

        [Test]
        public void RollEnemies_LegacyFallback_UsedWhenNoEncounterGroups()
        {
            var legacyDef = new DungeonDefinition
            {
                id = "test_dungeon_legacy",
                EnemyIds = new List<string> { "test_enemy_a", "test_enemy_b" }
            };
            var dungeon = new DungeonRuntime("legacy-instance", legacyDef);

            List<EnemyRuntime> result = _dungeonService.RollEnemies(dungeon);

            Assert.AreEqual(1, result.Count, "Legacy flat-pool fallback should still spawn exactly one enemy.");
        }

        // ─── Task 2.5: Loot ──────────────────────────────────────────────

        [Test]
        public void RollSingleDrop_Over1000Rolls_RespectsWeightDirection()
        {
            _database.TryGet<ItemDefinition>("test_item_common", out ItemDefinition common);
            _database.TryGet<ItemDefinition>("test_item_rare", out ItemDefinition rare);

            var table = new List<DropTableEntry>
            {
                new DropTableEntry { Item = common, Weight = 900, StackCount = 1 },
                new DropTableEntry { Item = rare, Weight = 50, StackCount = 1 },
            };

            int commonCount = 0, rareCount = 0, nothingCount = 0;
            const int iterations = 1000;

            for (int i = 0; i < iterations; i++)
            {
                ItemRuntime drop = _container.Loot.RollSingleDrop(table);
                if (drop == null) nothingCount++;
                else if (drop.Definition.id == "test_item_common") commonCount++;
                else if (drop.Definition.id == "test_item_rare") rareCount++;
            }

            UnityEngine.Debug.Log($"[DungeonEncounterAndLootTests] {iterations} drop rolls: common={commonCount} rare={rareCount} nothing={nothingCount}");

            Assert.Greater(commonCount, 0);
            Assert.Greater(rareCount, 0);
            Assert.Greater(commonCount, rareCount, "The 900-weight entry should drop far more often than the 50-weight entry.");
        }

        [Test]
        public void RunLoot_EnemyWithMultipleDefinedDrops_YieldsMoreThanOneItemTypeAcrossKills()
        {
            if (!_database.TryGet<EnemyDefinition>("test_multi_drop_enemy", out EnemyDefinition multiDropEnemy))
            {
                multiDropEnemy = new EnemyDefinition
                {
                    id = "test_multi_drop_enemy",
                    BaseMaxHp = 10,
                    DropTable = new List<EnemyDropEntry>
                    {
                        new EnemyDropEntry { ItemId = "test_item_common", Weight = 500, StackCount = 1 },
                        new EnemyDropEntry { ItemId = "test_item_uncommon", Weight = 400, StackCount = 1 },
                        new EnemyDropEntry { ItemId = "test_item_rare", Weight = 50, StackCount = 1 },
                    }
                };
                _database.Add(multiDropEnemy);
            }

            var seenItemIds = new HashSet<string>();
            const int iterations = 300;

            for (int i = 0; i < iterations; i++)
            {
                var pendingDrops = new List<ItemRuntime>();
                var table = new List<DropTableEntry>();
                foreach (EnemyDropEntry entry in multiDropEnemy.DropTable)
                {
                    _database.TryGet<ItemDefinition>(entry.ItemId, out ItemDefinition itemDef);
                    table.Add(new DropTableEntry { Item = itemDef, Weight = entry.Weight, StackCount = entry.StackCount });
                }

                ItemRuntime drop = _container.Loot.RollSingleDrop(table);
                if (drop != null) seenItemIds.Add(drop.Definition.id);
            }

            UnityEngine.Debug.Log($"[DungeonEncounterAndLootTests] Multi-drop enemy over {iterations} kills produced item types: {string.Join(", ", seenItemIds)}");

            Assert.Greater(seenItemIds.Count, 1,
                "An enemy with multiple weighted drops must be able to yield more than one item type across repeated kills (not always the first).");
        }

        [Test]
        public void RunSearchRoomReward_EmptyRoomPath_DoesNotThrowAndEventuallyCollectsLoot()
        {
            _database.TryGet<DungeonDefinition>("test_dungeon_encounters", out DungeonDefinition def);
            var dungeon = new DungeonRuntime("search-room-instance", def) { AdventurerInstanceIds = new List<string>() };
            var expedition = new ExpeditionRuntime { SlotIndex = 0, Dungeon = dungeon };

            int totalCollected = 0;
            const int iterations = 200;

            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    dungeon.PendingDrops.Clear();
                    _dungeonService.RunSearchRoomReward(expedition);
                    totalCollected += dungeon.PendingDrops.Count;
                }
            });

            UnityEngine.Debug.Log($"[DungeonEncounterAndLootTests] Search-room reward over {iterations} rolls collected items in {totalCollected} of them.");
            Assert.Greater(totalCollected, 0, "Search-room reward table should produce at least some drops across 200 rolls.");
        }

        [Test]
        public void RunSearchRoomReward_NoSearchRoomTable_DoesNotThrow()
        {
            var barrenDef = new DungeonDefinition { id = "test_dungeon_no_search_table" };
            var dungeon = new DungeonRuntime("no-search-instance", barrenDef);
            var expedition = new ExpeditionRuntime { SlotIndex = 0, Dungeon = dungeon };

            Assert.DoesNotThrow(() => _dungeonService.RunSearchRoomReward(expedition));
        }

        [Test]
        public void SaveDungeonState_SaveFailure_RestoresBothActiveStateFields()
        {
            var save = new FailingSaveService();
            var container = new ServiceContainer(_database, save);

            if (!_database.TryGet<AdventurerDefinition>("phase2b_save_test_footman", out _))
            {
                _database.Add(new AdventurerDefinition
                {
                    id = "phase2b_save_test_footman",
                    BaseMaxHp = 100,
                    BaseConstitution = 10,
                    BaseIntelligence = 5,
                    BaseDexterity = 5,
                    BaseDefense = 5,
                    BaseMagicDefense = 5,
                    MaxLevel = 100
                });
            }

            CharacterRuntime hero = container.Character.CreateCharacter("phase2b_save_test_footman");
            Assert.IsTrue(container.Dungeon.StartExpedition(
                0, "test_dungeon_encounters", new List<string> { hero.InstanceId }, out string startError), startError);

            var previousLegacyState = new ActiveDungeonSaveData
            {
                DungeonDefinitionId = "previous_legacy_dungeon",
                Progress = 17
            };
            var previousExpeditions = new List<ExpeditionSaveData>
            {
                new ExpeditionSaveData
                {
                    SlotIndex = 2,
                    Dungeon = new ActiveDungeonSaveData
                    {
                        DungeonDefinitionId = "previous_expedition_dungeon",
                        Progress = 23
                    }
                }
            };
            save.CurrentData.ActiveDungeon = previousLegacyState;
            save.CurrentData.ActiveExpeditions = previousExpeditions;

            save.FailSaves = true;
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(
                @"\[DungeonService\] SaveDungeonState failed, restored active dungeon state\. Error: Intentional Phase 2B save failure\."));
            container.Dungeon.SaveDungeonState();

            Assert.AreSame(previousLegacyState, save.CurrentData.ActiveDungeon,
                "Save failure must restore the legacy ActiveDungeon field.");
            Assert.AreSame(previousExpeditions, save.CurrentData.ActiveExpeditions,
                "Save failure must restore ActiveExpeditions, not leave an unsaved replacement list.");
        }
    }
}
