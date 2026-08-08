using System;
using System.Linq;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Core;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Tools.Developer;
using UnityEngine;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public class NewPlayerStateResetTests
    {
        private sealed class MockSaveService : ISaveService
        {
            public SaveData CurrentData { get; set; }
            public SaveLoadResult LastLoadStatus { get; private set; } = SaveLoadResult.FreshNewGame;
            public event Action OnSaveStarted;
            public event Action<bool> OnSaveCompleted;
            public bool HasSaveFile() => false;
            public bool Load(out Exception error) { error = null; return true; }
            public bool Save(out Exception error) { error = null; return true; }
            public void DeleteSave() { }
        }

        private GameDatabase _database;

        [SetUp]
        public void SetUp()
        {
            _database = new GameDatabase();
            new DatabaseBuilder(
                new EditorExternalGameDataProvider(),
                new UnityJsonSerializer(),
                _database).Build();
        }

        private ServiceContainer Reset(out MockSaveService save)
        {
            var data = SaveData.CreateDefault();
            data.Money = 999999;
            data.TutorialStep = 7;
            data.Items.Add(new ItemSaveData { InstanceId = "DEV_TEST_ITEM", DefinitionId = "copper_sword", StackCount = 4 });
            data.WorkshopQueue.Add(new ItemActionSaveData { InstanceId = "DEV_TEST_CRAFT", DefinitionId = "copper_sword" });
            data.MarketListings.Add(new ItemActionSaveData { InstanceId = "DEV_TEST_MARKET", DefinitionId = "copper_sword" });
            data.ActiveDungeon = new ActiveDungeonSaveData { DungeonDefinitionId = "DEV_TEST_DUNGEON" };
            data.ActiveRaid = new RaidSaveData { DefinitionId = "DEV_TEST_RAID" };
            save = new MockSaveService { CurrentData = data };
            return NewPlayerStateResetter.ResetToNewPlayerState(
                _database,
                save,
                new System.Random(17));
        }

        [Test]
        public void ResetCreatesExactlyOneFootmanAndOneNonFootmanVisitor()
        {
            var services = Reset(out var save);
            var data = save.CurrentData;

            Assert.AreEqual(100, data.Money);
            Assert.AreEqual(1, services.Character.GetAllCharacters().Count);
            Assert.AreEqual("footman", services.Character.GetAllCharacters()[0].Definition.id);
            Assert.AreEqual(1, data.TavernGuests.Count);
            Assert.AreNotEqual("footman", data.TavernGuests[0].DefinitionId);
            Assert.AreEqual(0, data.TutorialStep);
            Assert.IsNull(data.ActiveDungeon);
            Assert.IsNull(data.ActiveRaid);
            Assert.AreEqual(0, data.WorkshopQueue.Count);
            Assert.AreEqual(0, data.MarketListings.Count);
        }

        [Test]
        public void ResetKeepsStarterWeaponEquippedWithoutVisibleInventoryDuplicate()
        {
            var services = Reset(out var save);
            var hero = services.Character.GetAllCharacters().Single();

            Assert.IsNotNull(hero.Weapon);
            Assert.IsNotNull(services.Inventory.GetItem(hero.Weapon.InstanceId));
            Assert.IsFalse(services.Inventory.GetAllItems().Any(item => item.InstanceId == hero.Weapon.InstanceId));
            Assert.AreEqual(1, save.CurrentData.Items.Count(item => item.InstanceId == hero.Weapon.InstanceId));
        }

        [Test]
        public void ResetStateSurvivesSaveLoad()
        {
            var services = Reset(out var save);
            string json = JsonUtility.ToJson(save.CurrentData);
            var restoredData = JsonUtility.FromJson<SaveData>(json);
            restoredData.NormalizeAfterLoad();
            var reloaded = new ServiceContainer(
                _database,
                new MockSaveService { CurrentData = restoredData },
                null,
                new RuntimeFactory(new DefaultInstanceIdGenerator()));

            Assert.AreEqual(100, reloaded.Save.CurrentData.Money);
            Assert.AreEqual(1, reloaded.Character.GetAllCharacters().Count);
            Assert.AreEqual("footman", reloaded.Character.GetAllCharacters()[0].Definition.id);
            Assert.AreEqual(1, reloaded.Tavern.GetGuests().Count);
            Assert.AreNotEqual("footman", reloaded.Tavern.GetGuests()[0].DefinitionId);
            Assert.IsNotNull(reloaded.Character.GetAllCharacters()[0].Weapon);
            Assert.IsFalse(reloaded.Inventory.GetAllItems()
                .Any(item => item.InstanceId == reloaded.Character.GetAllCharacters()[0].Weapon.InstanceId));
            Assert.AreEqual(0, reloaded.Save.CurrentData.ActiveExpeditions.Count);
            Assert.IsNull(reloaded.Save.CurrentData.ActiveDungeon);
            Assert.IsNull(reloaded.Save.CurrentData.ActiveRaid);
        }
    }
}
