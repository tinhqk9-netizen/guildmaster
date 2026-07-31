using System;
using System.Collections.Generic;
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
    [TestFixture]
    public class MultiDungeonExpeditionTests
    {
        private GameDatabase _database;
        private ServiceContainer _container;

        [SetUp]
        public void Setup()
        {
            _database = new GameDatabase();
            var provider = new EditorExternalGameDataProvider();
            var serializer = new UnityJsonSerializer();
            var builder = new DatabaseBuilder(provider, serializer, _database);
            builder.Build();

            // Register test adventurer definition if not already present
            if (!_database.TryGet<AdventurerDefinition>("test_footman", out _))
            {
                _database.Add(new AdventurerDefinition
                {
                    id = "test_footman",
                    BaseMaxHp = 100,
                    BaseConstitution = 10,
                    BaseIntelligence = 5,
                    BaseDexterity = 5,
                    BaseDefense = 5,
                    BaseMagicDefense = 5,
                    MaxLevel = 100
                });
            }

            // Register test dungeon definitions
            if (!_database.TryGet<DungeonDefinition>("test_dungeon_a", out _))
            {
                _database.Add(new DungeonDefinition { id = "test_dungeon_a" });
            }
            if (!_database.TryGet<DungeonDefinition>("test_dungeon_b", out _))
            {
                _database.Add(new DungeonDefinition { id = "test_dungeon_b" });
            }
            if (!_database.TryGet<DungeonDefinition>("test_dungeon_c", out _))
            {
                _database.Add(new DungeonDefinition { id = "test_dungeon_c" });
            }

            _container = new ServiceContainer(_database);

            // Clear any pre-existing expedition/dungeon state from saved data
            for (int i = 0; i < _container.Dungeon.MaxExpeditions; i++)
            {
                _container.Dungeon.StopExpedition(i);
            }
        }

        private CharacterRuntime CreateTestCharacter()
        {
            return _container.Character.CreateCharacter("test_footman");
        }

        // ─── Test 1: Migration ──────────────────────────────────────────

        [Test]
        public void Migration_OldActiveDungeon_MigratesToActiveExpeditionsSlot0()
        {
            var saveData = new SaveData();
            saveData.ActiveDungeon = new ActiveDungeonSaveData
            {
                DungeonDefinitionId = "test_dungeon_a",
                Progress = 42,
                AdventurerInstanceIds = new List<string> { "hero_1" }
            };

            saveData.NormalizeAfterLoad();

            Assert.AreEqual(1, saveData.ActiveExpeditions.Count);
            Assert.AreEqual(0, saveData.ActiveExpeditions[0].SlotIndex);
            Assert.AreEqual("test_dungeon_a", saveData.ActiveExpeditions[0].Dungeon.DungeonDefinitionId);
            Assert.AreEqual(42, saveData.ActiveExpeditions[0].Dungeon.Progress);
            // Legacy ActiveDungeon preserved in memory until save
            Assert.IsNotNull(saveData.ActiveDungeon);
        }

        [Test]
        public void Migration_Idempotent_DoesNotDuplicateOnSecondLoad()
        {
            var saveData = new SaveData();
            saveData.ActiveDungeon = new ActiveDungeonSaveData
            {
                DungeonDefinitionId = "test_dungeon_a",
                Progress = 10,
                AdventurerInstanceIds = new List<string> { "hero_1" }
            };

            saveData.NormalizeAfterLoad();
            Assert.AreEqual(1, saveData.ActiveExpeditions.Count);

            // Second normalize should not duplicate
            saveData.NormalizeAfterLoad();
            Assert.AreEqual(1, saveData.ActiveExpeditions.Count);
        }

        // ─── Test 2: Three Parallel Expeditions ─────────────────────────

        [Test]
        public void StartExpedition_ThreeParallelExpeditions_RunIndependently()
        {
            var dungeonService = _container.Dungeon;
            var c1 = CreateTestCharacter();
            var c2 = CreateTestCharacter();
            var c3 = CreateTestCharacter();

            bool s0 = dungeonService.StartExpedition(0, "test_dungeon_a", new List<string> { c1.InstanceId }, out string err0);
            bool s1 = dungeonService.StartExpedition(1, "test_dungeon_b", new List<string> { c2.InstanceId }, out string err1);
            bool s2 = dungeonService.StartExpedition(2, "test_dungeon_c", new List<string> { c3.InstanceId }, out string err2);

            Assert.IsTrue(s0, err0);
            Assert.IsTrue(s1, err1);
            Assert.IsTrue(s2, err2);

            var expeditions = dungeonService.GetAllExpeditions();
            Assert.AreEqual(3, expeditions.Count);

            Assert.IsNotNull(dungeonService.GetExpedition(0));
            Assert.IsNotNull(dungeonService.GetExpedition(1));
            Assert.IsNotNull(dungeonService.GetExpedition(2));
        }

        // ─── Test 3: Character Exclusivity ──────────────────────────────

        [Test]
        public void StartExpedition_CharacterExclusivity_RejectsAlreadyAssignedCharacter()
        {
            var dungeonService = _container.Dungeon;
            var c1 = CreateTestCharacter();

            bool s0 = dungeonService.StartExpedition(0, "test_dungeon_a", new List<string> { c1.InstanceId }, out string err0);
            Assert.IsTrue(s0, err0);

            bool s1 = dungeonService.StartExpedition(1, "test_dungeon_b", new List<string> { c1.InstanceId }, out string err1);
            Assert.IsFalse(s1);
            Assert.IsTrue(err1.Contains("already assigned"));
        }

        [Test]
        public void StartExpedition_RejectsDuplicateWithinTeam()
        {
            var dungeonService = _container.Dungeon;
            var c1 = CreateTestCharacter();

            bool result = dungeonService.StartExpedition(0, "test_dungeon_a",
                new List<string> { c1.InstanceId, c1.InstanceId }, out string err);
            Assert.IsFalse(result);
            Assert.IsTrue(err.Contains("Duplicate"));
        }

        [Test]
        public void StartExpedition_RejectsEmptyParty()
        {
            var dungeonService = _container.Dungeon;

            bool result = dungeonService.StartExpedition(0, "test_dungeon_a",
                new List<string>(), out string err);
            Assert.IsFalse(result);
            Assert.IsTrue(err.Contains("between 1 and 4"));
        }

        // ─── Test 4: Legacy API Compatibility ───────────────────────────

        [Test]
        public void LegacyApi_StrictlyMapsToSlot0()
        {
            var dungeonService = _container.Dungeon;
            var c1 = CreateTestCharacter();

            dungeonService.StartDungeon("test_dungeon_a", new List<string> { c1.InstanceId });

            Assert.IsTrue(dungeonService.IsDungeonActive());
            Assert.IsNotNull(dungeonService.GetActiveDungeon());
            Assert.AreEqual("test_dungeon_a", dungeonService.GetActiveDungeon().Definition.id);
            Assert.AreEqual("test_dungeon_a", dungeonService.GetExpedition(0).Dungeon.Definition.id);

            dungeonService.StopDungeon();
            Assert.IsFalse(dungeonService.IsDungeonActive());
            Assert.IsNull(dungeonService.GetActiveDungeon());
        }

        [Test]
        public void LegacyApi_IsDungeonActive_IgnoresOtherSlots()
        {
            var dungeonService = _container.Dungeon;
            var c1 = CreateTestCharacter();

            // Slot 0 empty, slot 2 active
            bool started = dungeonService.StartExpedition(2, "test_dungeon_a",
                new List<string> { c1.InstanceId }, out _);
            Assert.IsTrue(started);

            // Legacy API should report FALSE because slot 0 is empty
            Assert.IsFalse(dungeonService.IsDungeonActive());
            Assert.IsNull(dungeonService.GetActiveDungeon());
        }

        // ─── Test 5: Save/Load Roundtrip ────────────────────────────────

        [Test]
        public void SaveLoad_MultiExpeditionRoundtrip_RestoresAllSlots()
        {
            var c1 = CreateTestCharacter();
            var c2 = CreateTestCharacter();
            var dungeonService = _container.Dungeon;

            dungeonService.StartExpedition(0, "test_dungeon_a", new List<string> { c1.InstanceId }, out _);
            dungeonService.StartExpedition(2, "test_dungeon_b", new List<string> { c2.InstanceId }, out _);

            dungeonService.GetExpedition(0).Dungeon.Progress = 15;
            dungeonService.SaveDungeonState();

            // Reload
            dungeonService.LoadDungeonState();

            var exp0 = dungeonService.GetExpedition(0);
            var exp1 = dungeonService.GetExpedition(1);
            var exp2 = dungeonService.GetExpedition(2);

            Assert.IsNotNull(exp0);
            Assert.AreEqual(15, exp0.Dungeon.Progress);
            Assert.AreEqual("test_dungeon_a", exp0.Dungeon.Definition.id);

            Assert.IsNull(exp1);

            Assert.IsNotNull(exp2);
            Assert.AreEqual("test_dungeon_b", exp2.Dungeon.Definition.id);
        }

        // ─── Test 6: Dismiss Guard ──────────────────────────────────────

        [Test]
        public void DismissGuard_CharacterOnExpedition_CannotBeDismissed()
        {
            var c1 = CreateTestCharacter();
            var dungeonService = _container.Dungeon;

            dungeonService.StartExpedition(1, "test_dungeon_a", new List<string> { c1.InstanceId }, out _);

            bool canDismiss = _container.Character.CanDismissCharacter(c1.InstanceId, out string reason);
            Assert.IsFalse(canDismiss);
            Assert.IsTrue(reason.Contains("exploring a dungeon"));
        }

        // ─── Test 7: StopExpedition ─────────────────────────────────────

        [Test]
        public void StopExpedition_ClearsOnlyTargetSlot()
        {
            var dungeonService = _container.Dungeon;
            var c1 = CreateTestCharacter();
            var c2 = CreateTestCharacter();

            dungeonService.StartExpedition(0, "test_dungeon_a", new List<string> { c1.InstanceId }, out _);
            dungeonService.StartExpedition(1, "test_dungeon_b", new List<string> { c2.InstanceId }, out _);

            dungeonService.StopExpedition(1);

            Assert.IsNotNull(dungeonService.GetExpedition(0));
            Assert.IsNull(dungeonService.GetExpedition(1));

            // c2 should now be available
            Assert.IsFalse(dungeonService.IsCharacterOnExpedition(c2.InstanceId));
        }
    }
}
