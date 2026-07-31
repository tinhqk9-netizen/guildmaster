using System.Linq;
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
            _container.Save.CurrentData.Items.Clear();
            var inv = _container.Inventory;
            foreach (var item in inv.GetAllItems().ToList())
            {
                item.IsLocked = false;
                inv.RemoveItem(item.InstanceId, item.StackCount);
            }

            var matDef = new ItemDefinition { id = "m1", Category = ItemCategory.Material };
            var wpDef = new ItemDefinition { id = "w1", Category = ItemCategory.Weapon };

            inv.AddItem(new ItemRuntime("inst_test_mat", matDef, 5));
            inv.AddItem(new ItemRuntime("inst_test_wp", wpDef, 1));

            var materials = inv.GetItemsByCategory(ItemCategory.Material);
            var weapons = inv.GetItemsByCategory(ItemCategory.Weapon);

            Assert.AreEqual(1, materials.Count);
            Assert.AreEqual("m1", materials[0].Definition.id);

            Assert.AreEqual(1, weapons.Count);
            Assert.AreEqual("w1", weapons[0].Definition.id);
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

        [Test]
        public void DatabaseBuilder_ItemFields_PopulatesEquipmentStats()
        {
            Assert.IsTrue(_database.TryGet<ItemDefinition>("copper_sword", out var copperSword));
            Assert.AreEqual(3, copperSword.Constitution);
            Assert.AreEqual(1, copperSword.Dexterity);
            Assert.AreEqual("+3 CON, +1 DEX", copperSword.GetStatSummary());

            Assert.IsTrue(_database.TryGet<ItemDefinition>("cane", out var cane));
            Assert.AreEqual(1, cane.Intelligence);
            Assert.AreEqual("+1 INT", cane.GetStatSummary());
        }
    }
}
