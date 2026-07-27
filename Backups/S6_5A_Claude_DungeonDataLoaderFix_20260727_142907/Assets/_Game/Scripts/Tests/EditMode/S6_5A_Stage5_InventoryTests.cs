using System;
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
    public class S6_5A_Stage5_InventoryTests
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
        public void InventoryService_CategoryFiltering_ReturnsMatchingCategoryOnly()
        {
            var inv = _container.Inventory;

            var matDef = new ItemDefinition { id = "mat_iron", Category = ItemCategory.Material };
            var wpDef = new ItemDefinition { id = "wp_sword", Category = ItemCategory.Weapon };

            inv.AddItem(new ItemRuntime("inst_1", matDef, 5));
            inv.AddItem(new ItemRuntime("inst_2", wpDef, 1));

            var materials = inv.GetItemsByCategory(ItemCategory.Material);
            var weapons = inv.GetItemsByCategory(ItemCategory.Weapon);

            Assert.AreEqual(1, materials.Count);
            Assert.AreEqual("mat_iron", materials[0].Definition.id);

            Assert.AreEqual(1, weapons.Count);
            Assert.AreEqual("wp_sword", weapons[0].Definition.id);
        }

        [Test]
        public void InventoryService_ToggleLockItem_TogglesAndPersistsInSave()
        {
            var inv = _container.Inventory;
            var itemDef = new ItemDefinition { id = "mat_wood", Category = ItemCategory.Material };
            var item = new ItemRuntime("inst_wood", itemDef, 10);

            inv.AddItem(item);
            Assert.IsFalse(inv.GetItem("inst_wood").IsLocked);

            bool success = inv.ToggleLockItem("inst_wood");
            Assert.IsTrue(success);
            Assert.IsTrue(inv.GetItem("inst_wood").IsLocked);

            // Check save data
            Assert.IsTrue(_container.Save.CurrentData.Items[0].IsLocked);
        }

        [Test]
        public void InventoryService_UseConsumable_ConsumesItemAndHealsTarget()
        {
            var inv = _container.Inventory;
            var potDef = new ItemDefinition { id = "pot_hp", Category = ItemCategory.Consumable };
            var item = new ItemRuntime("inst_hp", potDef, 3);
            inv.AddItem(item);

            var charDef = new AdventurerDefinition { id = "adv_1", BaseMaxHp = 100 };
            var character = new CharacterRuntime("char_inst", charDef) { CurrentHp = 20 };

            bool used = inv.UseConsumable("inst_hp", character);

            Assert.IsTrue(used);
            Assert.AreEqual(70, character.CurrentHp); // 20 + 50
            Assert.AreEqual(2, inv.GetItem("inst_hp").StackCount);
        }
    }
}
