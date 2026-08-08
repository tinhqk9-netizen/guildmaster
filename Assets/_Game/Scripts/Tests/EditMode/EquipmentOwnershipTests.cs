using System;
using System.Linq;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Core;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Inventory;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public class EquipmentOwnershipTests
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

        private ServiceContainer CreateServices(out MockSaveService save, SaveData data = null)
        {
            data ??= SaveData.CreateDefault();
            data.LevelQuarters = Math.Max(data.LevelQuarters, 5);
            data.LevelTavernCapacity = Math.Max(data.LevelTavernCapacity, 5);
            // Step 7 deterministically creates the legacy archer visitor, which has
            // a real starter weapon in the loaded data. Tests that exercise visitor
            // ownership must not depend on the random class roll.
            save = new MockSaveService { CurrentData = data };
            return new ServiceContainer(_database, save, null,
                new RuntimeFactory(new DefaultInstanceIdGenerator()));
        }

        private CharacterRuntime CreateHero(ServiceContainer services)
        {
            var definition = _database.GetAll<AdventurerDefinition>().First(d => d != null);
            return services.Character.CreateCharacter(definition.id);
        }

        private ItemRuntime CreateCompatibleWeapon(CharacterRuntime hero, string id)
        {
            var definition = new ItemDefinition
            {
                id = id,
                Category = ItemCategory.Weapon,
                ItemType = string.IsNullOrEmpty(hero.Definition.WeaponType)
                    ? "Generic"
                    : hero.Definition.WeaponType
            };
            _database.Add(definition);
            return new ItemRuntime(id + "_instance", definition, 1);
        }

        [Test]
        public void VisitorSpawn_DoesNotCreateInventoryItem()
        {
            var services = CreateServices(out _);

            ((TavernService)services.Tavern).GenerateVisitorForDeveloper("archer");

            var guest = services.Tavern.GetGuests().First();
            Assert.IsFalse(string.IsNullOrEmpty(guest.WeaponInstanceId));
            Assert.IsFalse(services.Inventory.GetAllItems()
                .Any(i => i.InstanceId == guest.WeaponInstanceId));
        }

        [Test]
        public void RecruitVisitor_CreatesEquippedStarterWithoutInventoryDuplicate()
        {
            var services = CreateServices(out _);
            ((TavernService)services.Tavern).GenerateVisitorForDeveloper("archer");
            string weaponId = services.Tavern.GetGuests()[0].WeaponInstanceId;

            Assert.IsTrue(services.Tavern.RecruitGuest(0, out var hero));

            Assert.IsNotNull(hero.Weapon);
            Assert.AreEqual(weaponId, hero.Weapon.InstanceId);
            Assert.IsNotNull(services.Inventory.GetItem(weaponId));
            Assert.IsFalse(services.Inventory.GetAllItems()
                .Any(i => i.InstanceId == weaponId));
        }

        [Test]
        public void Equip_RemovesItemFromVisibleInventory()
        {
            var services = CreateServices(out _);
            var hero = CreateHero(services);
            var item = CreateCompatibleWeapon(hero, "ownership_equip_weapon");
            services.Inventory.AddItem(item);

            Assert.IsTrue(services.Equipment.Equip(hero, item.InstanceId, EquipmentSlot.Weapon));

            Assert.AreSame(item, hero.Weapon);
            Assert.IsFalse(services.Inventory.GetAllItems()
                .Any(i => i.InstanceId == item.InstanceId));
            Assert.IsTrue(item.IsLocked);
        }

        [Test]
        public void Unequip_RestoresItemToVisibleInventory()
        {
            var services = CreateServices(out _);
            var hero = CreateHero(services);
            var item = CreateCompatibleWeapon(hero, "ownership_unequip_weapon");
            services.Inventory.AddItem(item);
            Assert.IsTrue(services.Equipment.Equip(hero, item.InstanceId, EquipmentSlot.Weapon));

            Assert.IsTrue(services.Equipment.Unequip(hero, EquipmentSlot.Weapon));

            Assert.IsNull(hero.Weapon);
            Assert.IsTrue(services.Inventory.GetAllItems()
                .Any(i => i.InstanceId == item.InstanceId));
            Assert.IsFalse(item.IsLocked);
        }

        [Test]
        public void SaveLoad_PreservesEquipmentWithoutInventoryDuplicate()
        {
            var services = CreateServices(out var save);
            var hero = CreateHero(services);
            var item = CreateCompatibleWeapon(hero, "ownership_reload_weapon");
            services.Inventory.AddItem(item);
            Assert.IsTrue(services.Equipment.Equip(hero, item.InstanceId, EquipmentSlot.Weapon));
            save.Save(out _);

            var reloaded = CreateServices(out _, save.CurrentData);
            var loadedHero = reloaded.Character.GetAllCharacters()
                .Single(c => c.InstanceId == hero.InstanceId);

            Assert.IsNotNull(loadedHero.Weapon);
            Assert.AreEqual(item.InstanceId, loadedHero.Weapon.InstanceId);
            Assert.IsFalse(reloaded.Inventory.GetAllItems()
                .Any(i => i.InstanceId == item.InstanceId));
            Assert.AreEqual(1, reloaded.Save.CurrentData.Items
                .Count(i => i.InstanceId == item.InstanceId));
        }

        [Test]
        public void VisitorExpiration_DoesNotLeakStarterWeapons()
        {
            var services = CreateServices(out _);
            for (int i = 0; i < services.Tavern.GetTavernCapacity() + 4; i++)
                services.Tavern.GenerateVisitor();

            Assert.AreEqual(0, services.Inventory.GetAllItems().Count,
                "Visitor expiration must not leak starter weapons.");
        }

        [Test]
        public void SameDefinitionItems_SeparateAvailableAndEquippedCounts()
        {
            var services = CreateServices(out _);
            var heroes = new[] { CreateHero(services), CreateHero(services), CreateHero(services) };
            string definitionId = "ownership_same_definition_weapon";
            var definition = new ItemDefinition
            {
                id = definitionId,
                Category = ItemCategory.Weapon,
                ItemType = heroes[0].Definition.WeaponType
            };
            _database.Add(definition);

            for (int i = 0; i < heroes.Length; i++)
            {
                var equipped = new ItemRuntime($"{definitionId}_equipped_{i}", definition, 1);
                services.Inventory.AddItem(equipped);
                Assert.IsTrue(services.Equipment.Equip(heroes[i], equipped.InstanceId, EquipmentSlot.Weapon));
            }

            for (int i = 0; i < 3; i++)
                services.Inventory.AddItem(new ItemRuntime($"{definitionId}_available_{i}", definition, 1));

            var available = services.Inventory.GetAllItems();
            var counts = InventoryOwnershipPresentation.ForDefinition(
                definitionId, available, services.Character.GetAllCharacters());

            Assert.AreEqual(3, available.Count(i => i.Definition.id == definitionId));
            Assert.AreEqual(3, counts.Available);
            Assert.AreEqual(3, counts.Equipped);
            Assert.AreEqual(6, services.Save.CurrentData.Items.Count(i => i.DefinitionId == definitionId));
            Assert.IsTrue(available.All(i => !heroes.Any(h => h.Weapon?.InstanceId == i.InstanceId)));
        }
    }
}
