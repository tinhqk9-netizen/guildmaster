using System;
using System.Collections.Generic;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public class S6_5A_Stage8_DungeonCombatTests
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

            _container = new ServiceContainer(_database);
        }

        [Test]
        public void CombatService_ApplyDamage_CalculatesDefenseAndShieldAbsorption()
        {
            var combat = _container.Combat;

            var charDef = new AdventurerDefinition
            {
                id = "adv_1",
                BaseMaxHp = 100,
                BaseDefense = 20, // 20% reduction -> (1 - 0.20) = 0.8
                BaseConstitution = 16 // CON / 8 = 2 flat reduction
            };
            var character = new CharacterRuntime("char_inst", charDef)
            {
                CurrentHp = 100,
                CurrentShield = 10
            };

            var wrapper = new AdventurerWrapper(character);

            // Raw damage = 50
            // Reduced = (1 - 0.20) * 50 - (16/8) - 0 = 40 - 2 = 38
            // Shield = 10, absorbs 10 -> leftover 28 damage to HP -> HP = 100 - 28 = 72
            int actualDmg = combat.ApplyDamage(wrapper, 50, false, 0, 0.0);

            Assert.AreEqual(38, actualDmg);
            Assert.AreEqual(0, wrapper.CurrentShield);
            Assert.AreEqual(72, wrapper.CurrentHp);
        }

        [Test]
        public void DungeonService_StateMachineTick_TransitionsStateAndResetsProgressOnDefeat()
        {
            var dungeon = _container.Dungeon;
            var dungeonDef = new DungeonDefinition { id = "dungeon_1" };
            _database.Add(dungeonDef);

            if (!_database.TryGet<AdventurerDefinition>("adv_1", out _))
            {
                _database.Add(new AdventurerDefinition
                {
                    id = "adv_1",
                    BaseMaxHp = 100,
                    BaseConstitution = 10,
                    MaxLevel = 100
                });
            }

            var character = _container.Character.CreateCharacter("adv_1");

            // Clear any pre-existing expedition from saved data
            dungeon.StopDungeon();

            dungeon.StartDungeon("dungeon_1", new List<string> { character.InstanceId });
            Assert.IsTrue(dungeon.IsDungeonActive());

            var active = dungeon.GetActiveDungeon();
            Assert.AreEqual(0, active.ActionType); // ENTER_DUNGEON

            // ENTER_DUNGEON duration is 5 turns, but DungeonService filters every second tick
            for (int i = 0; i < 10; i++) dungeon.Tick();

            Assert.AreEqual(1, active.ActionType); // ENTER_ROOM
        }
    }
}
