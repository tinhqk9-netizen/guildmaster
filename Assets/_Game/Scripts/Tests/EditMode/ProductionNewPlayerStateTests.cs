using System;
using System.Linq;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Boot;
using GuildMaster.Runtime.Core;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using UnityEngine;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public class ProductionNewPlayerStateTests
    {
        private sealed class MockSaveService : ISaveService
        {
            public SaveData CurrentData { get; set; }
            public SaveLoadResult LastLoadStatus { get; private set; } = SaveLoadResult.FreshNewGame;
            public event Action OnSaveStarted;
            public event Action<bool> OnSaveCompleted;
            public bool HasSaveFile() => false;
            public bool Load(out Exception error) { error = null; return true; }
            public bool Save(out Exception error)
            {
                error = null;
                OnSaveStarted?.Invoke();
                OnSaveCompleted?.Invoke(true);
                return true;
            }
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

        private ServiceContainer CreateFreshServices(out MockSaveService save)
        {
            save = new MockSaveService { CurrentData = SaveData.CreateDefault() };
            return new ServiceContainer(_database, save);
        }

        [Test]
        public void FreshState_CreatesProductionStartingRoster()
        {
            var services = CreateFreshServices(out var save);

            Assert.IsTrue(NewPlayerStateInitializer.TryInitialize(services, out var error), error);

            Assert.AreEqual(100, save.CurrentData.Money);
            Assert.AreEqual(1, services.Character.GetAllCharacters().Count);
            Assert.AreEqual("footman", services.Character.GetAllCharacters()[0].Definition.id);
            Assert.AreEqual(1, save.CurrentData.TavernGuests.Count);
            Assert.IsFalse(string.Equals("footman", save.CurrentData.TavernGuests[0].DefinitionId,
                StringComparison.OrdinalIgnoreCase));
            Assert.IsEmpty(save.CurrentData.Pets);
            Assert.IsEmpty(save.CurrentData.WorkshopQueue);
            Assert.IsEmpty(save.CurrentData.MarketListings);
            Assert.IsEmpty(save.CurrentData.ActiveExpeditions);
            Assert.IsNull(save.CurrentData.ActiveDungeon);
            Assert.IsNull(save.CurrentData.ActiveRaid);
            Assert.AreEqual(1, services.Party.GetPartyMembers(0).Count);
            Assert.AreEqual(services.Character.GetAllCharacters()[0].InstanceId,
                services.Party.GetPartyMembers(0)[0]);
        }

        [Test]
        public void FreshState_EquipsFootmanWithoutVisibleInventoryDuplicate()
        {
            var services = CreateFreshServices(out var save);

            Assert.IsTrue(NewPlayerStateInitializer.TryInitialize(services, out var error), error);

            var hero = services.Character.GetAllCharacters().Single();
            Assert.IsNotNull(hero.Weapon);
            Assert.IsFalse(services.Inventory.GetAllItems().Any(item => item.InstanceId == hero.Weapon.InstanceId));
            Assert.AreEqual(1, save.CurrentData.Items.Count(item => item.InstanceId == hero.Weapon.InstanceId));
            Assert.IsTrue(save.CurrentData.Items.Single(item => item.InstanceId == hero.Weapon.InstanceId).IsLocked);
        }

        [Test]
        public void FreshState_SaveReloadPreservesStartingState()
        {
            var services = CreateFreshServices(out var save);
            Assert.IsTrue(NewPlayerStateInitializer.TryInitialize(services, out var error), error);

            var restoredData = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(save.CurrentData));
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
            Assert.IsFalse(string.Equals("footman", reloaded.Tavern.GetGuests()[0].DefinitionId,
                StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(reloaded.Character.GetAllCharacters()[0].Weapon);
            Assert.IsFalse(reloaded.Inventory.GetAllItems()
                .Any(item => item.InstanceId == reloaded.Character.GetAllCharacters()[0].Weapon.InstanceId));
            Assert.AreEqual(1, reloaded.Party.GetPartyMembers(0).Count);
        }
    }
}
