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

namespace GuildMaster.Tests
{
    [TestFixture]
    public class CharacterDismissTests
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
        private ServiceContainer _container;
        private MockSaveService _saveService;

        [SetUp]
        public void SetUp()
        {
            _database = new GameDatabase();
            var provider = new EditorExternalGameDataProvider();
            var serializer = new UnityJsonSerializer();
            var builder = new DatabaseBuilder(provider, serializer, _database);
            builder.Build();

            _saveService = new MockSaveService
            {
                CurrentData = new SaveData()
            };

            _container = new ServiceContainer(_database, _saveService);
        }

        [Test]
        public void DismissCharacter_ValidHero_RemovesFromRosterAndSaveData()
        {
            var hero = _container.Character.CreateCharacter("footman");
            Assert.AreEqual(1, _container.Character.GetAllCharacters().Count);
            Assert.AreEqual(1, _saveService.CurrentData.Characters.Count);

            bool success = _container.Character.DismissCharacter(hero.InstanceId, out string errorReason);
            Assert.IsTrue(success, $"Dismiss should succeed. Error: {errorReason}");
            Assert.AreEqual(0, _container.Character.GetAllCharacters().Count, "Hero should be removed from runtime list.");
            Assert.AreEqual(0, _saveService.CurrentData.Characters.Count, "Hero should be removed from SaveData.");
        }

        [Test]
        public void DismissCharacter_ActiveParty_FailsWithReason()
        {
            var hero = _container.Character.CreateCharacter("footman");
            _saveService.CurrentData.CurrentParty.Add(hero.InstanceId);

            bool canDismiss = _container.Character.CanDismissCharacter(hero.InstanceId, out string reason);
            Assert.IsFalse(canDismiss, "Should not be able to dismiss a hero in active party.");
            Assert.IsTrue(reason.Contains("active party"), $"Reason should mention active party: {reason}");

            bool success = _container.Character.DismissCharacter(hero.InstanceId, out string errorReason);
            Assert.IsFalse(success, "DismissCharacter should fail for party member.");
            Assert.AreEqual(1, _container.Character.GetAllCharacters().Count, "Hero should remain in roster.");
        }

        [Test]
        public void DismissCharacter_EquippedItems_UnlocksItems()
        {
            var hero = _container.Character.CreateCharacter("footman");
            
            // Add and equip a weapon
            if (!_database.TryGet<ItemDefinition>("ironsword", out var weaponDef))
            {
                _database.TryGet<AdventurerDefinition>("footman", out var footmanDef);
                weaponDef = new ItemDefinition { id = "ironsword", Category = ItemCategory.Weapon, ItemType = footmanDef?.WeaponType };
                _database.Add(weaponDef);
            }

            var item = new ItemRuntime("TEST_WEAPON_1", weaponDef, 1);
            _container.Inventory.AddItem(item);
            _container.Equipment.Equip(hero, item.InstanceId, EquipmentSlot.Weapon);
            Assert.IsTrue(item.IsLocked, "Equipped item should be locked.");

            bool success = _container.Character.DismissCharacter(hero.InstanceId, out string error);
            Assert.IsTrue(success, $"Dismiss should succeed: {error}");
            Assert.IsFalse(item.IsLocked, "Item should be unlocked after character dismissal.");
        }
    }
}
