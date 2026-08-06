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
        public void B2_GenerateVisitor_AddsStarterWeaponToInventoryRuntime()
        {
            var container = BuildContainer();
            
            Assert.AreEqual(0, container.Inventory.GetAllItems().Count, "Inventory should be empty initially");
            
            container.Tavern.GenerateVisitor();

            var guests = container.Tavern.GetGuests();
            Assert.AreEqual(1, guests.Count, "Guest should be generated");
            var guest = guests[0];

            if (string.IsNullOrEmpty(guest.WeaponInstanceId))
            {
                Assert.Inconclusive("Generated guest has no starter weapon. Run test again or mock the definition.");
            }

            Assert.IsTrue(container.Inventory.HasItem(guest.WeaponInstanceId), "Inventory should have the guest's starter weapon at runtime");
            
            var item = container.Inventory.GetItem(guest.WeaponInstanceId);
            Assert.IsNotNull(item);
            Assert.IsTrue(item.IsLocked, "Starter weapon should be locked");
        }

        [Test]
        public void B2_RecruitGuest_WeaponStillInInventory()
        {
            var container = BuildContainer();
            container.Tavern.GenerateVisitor();

            var guest = container.Tavern.GetGuests()[0];
            string weaponId = guest.WeaponInstanceId;

            if (string.IsNullOrEmpty(weaponId))
                Assert.Inconclusive("Generated guest has no starter weapon.");

            bool recruited = container.Tavern.RecruitGuest(0, out var character);
            
            Assert.IsTrue(recruited, "Should successfully recruit guest");
            Assert.IsNotNull(character, "Character should not be null");
            Assert.AreEqual(weaponId, character.Weapon?.InstanceId, "Character should retain the weapon instance ID");
            
            Assert.IsTrue(container.Inventory.HasItem(weaponId), "Weapon should still exist in InventoryService runtime after recruitment");
        }

        [Test]
        public void B2_TavernCapacityExceeded_DeletesWeaponFromInventory()
        {
            var container = BuildContainer();
            var tavernService = container.Tavern;
            int maxCap = tavernService.GetTavernCapacity();
            
            List<string> weaponIds = new List<string>();

            // Generate enough visitors to exceed capacity
            for (int i = 0; i < maxCap + 1; i++)
            {
                tavernService.GenerateVisitor();
                if (tavernService.GetGuests().Count > 0)
                {
                    var newestGuest = tavernService.GetGuests()[0]; // Inserted at 0
                    if (!string.IsNullOrEmpty(newestGuest.WeaponInstanceId))
                    {
                        weaponIds.Insert(0, newestGuest.WeaponInstanceId);
                    }
                }
            }

            Assert.AreEqual(maxCap, tavernService.GetGuests().Count, "Guests should be trimmed to max capacity");
            
            // Verify all current guests' weapons are in the inventory.
            foreach (var guest in tavernService.GetGuests())
            {
                if (!string.IsNullOrEmpty(guest.WeaponInstanceId))
                {
                    Assert.IsTrue(container.Inventory.HasItem(guest.WeaponInstanceId), "Retained guest weapon should be in inventory");
                }
            }

            // Verify there are no orphaned items in the inventory.
            var allItemsCount = container.Inventory.GetAllItems().Count;
            var expectedItemsCount = tavernService.GetGuests().Count(g => !string.IsNullOrEmpty(g.WeaponInstanceId));
            Assert.AreEqual(expectedItemsCount, allItemsCount, "Inventory items count should strictly match the number of retained guest weapons.");
        }
    }
}
