using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Runtime.Core;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;

namespace GuildMaster.Tests
{
    public class B2_TavernStarterWeaponTests
    {
        private class MockSaveService : ISaveService
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
        public void Setup()
        {
            _database = new GameDatabase();
            var provider = new EditorExternalGameDataProvider();
            var serializer = new UnityJsonSerializer();
            var builder = new DatabaseBuilder(provider, serializer, _database);
            builder.Build();
            
            // Provide a mock definition if none exists
            if (!_database.TryGet<AdventurerDefinition>("Footman", out _))
            {
                _database.Add(new AdventurerDefinition
                {
                    id = "Footman",
                    BaseMaxHp = 100,
                    StarterWeaponId = "test_sword"
                });
            }
            if (!_database.TryGet<ItemDefinition>("test_sword", out _))
            {
                _database.Add(new ItemDefinition
                {
                    id = "test_sword",
                    Category = ItemCategory.Weapon
                });
            }
        }

        private ServiceContainer BuildContainer(SaveData data = null)
        {
            if (data == null)
            {
                data = SaveData.CreateDefault();
                data.LevelTavernCapacity = 5; 
                data.LevelQuarters = 5; 
            }
            var saveService = new MockSaveService { CurrentData = data };
            
            var factory = new RuntimeFactory(new DefaultInstanceIdGenerator());
            // Need to mock formula service and others, but ServiceContainer does that internally
            // if we use its constructor with minimal args... wait, ServiceContainer constructor:
            // public ServiceContainer(ISaveService saveService, IFormulaService formulaService = null, RuntimeFactory factory = null, GameDatabase database = null)
            // But we can just use the public constructor that ServiceContainer provides.
            // Wait, in B1 it used:
            // var container = new ServiceContainer(save, null, new RuntimeFactory(new DefaultInstanceIdGenerator()), _database);
            
            var container = new ServiceContainer(_database, saveService, null, factory);
            return container;
        }

        [Test]
        public void B2_GenerateVisitor_DoesNotAddStarterWeaponToInventory()
        {
            var container = BuildContainer();
            Assert.AreEqual(0, container.Inventory.GetAllItems().Count, "Inventory should be empty initially");
            
            ((TavernService)container.Tavern).GenerateVisitorForDeveloper("archer");

            var guests = container.Tavern.GetGuests();
            Assert.AreEqual(1, guests.Count, "Guest should be generated");
            var guest = guests[0];

            Assert.IsFalse(string.IsNullOrEmpty(guest.WeaponInstanceId), "Visitor should carry a starter weapon instance id");
            Assert.IsFalse(container.Inventory.GetAllItems().Any(i => i.InstanceId == guest.WeaponInstanceId),
                "A visitor's starter weapon must not be visible in inventory before recruitment");
        }

        [Test]
        public void B2_RecruitGuest_WeaponStillInInventory()
        {
            var container = BuildContainer();
            ((TavernService)container.Tavern).GenerateVisitorForDeveloper("archer");

            var guest = container.Tavern.GetGuests()[0];
            string weaponId = guest.WeaponInstanceId;

            if (string.IsNullOrEmpty(weaponId))
                Assert.Inconclusive("Generated guest has no starter weapon.");

            bool recruited = container.Tavern.RecruitGuest(0, out var character);
            
            Assert.IsTrue(recruited, "Should successfully recruit guest");
            Assert.IsNotNull(character, "Character should not be null");
            Assert.AreEqual(weaponId, character.Weapon?.InstanceId, "Character should retain the weapon instance ID");
            
            Assert.IsNotNull(container.Inventory.GetItem(weaponId), "Weapon ownership record should exist after recruitment");
            Assert.IsFalse(container.Inventory.GetAllItems().Any(i => i.InstanceId == weaponId),
                "Equipped starter weapon must not remain in the visible inventory");
        }

        [Test]
        public void B2_TavernCapacityExceeded_DoesNotLeakVisitorWeapons()
        {
            var container = BuildContainer();
            var tavernService = container.Tavern;
            int maxCap = tavernService.GetTavernCapacity();

            for (int i = 0; i < maxCap + 1; i++)
                tavernService.GenerateVisitor();

            Assert.AreEqual(maxCap, tavernService.GetGuests().Count, "Guests should be trimmed to max capacity");
            Assert.AreEqual(0, container.Inventory.GetAllItems().Count,
                "Inventory must not contain starter weapons for un-recruited visitors.");
        }
    }
}
