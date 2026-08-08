using System.IO;
using System.Linq;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Core;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using UnityEngine;

namespace GuildMaster.Tests.EditMode
{
    public class S2VerificationTests
    {
        private GameDatabase _database;
        private SaveService _saveService;
        private FormulaService _formulaService;
        private ItemService _itemService;
        private InventoryService _inventoryService;
        private EquipmentService _equipmentService;
        private CharacterService _characterService;
        private RuntimeFactory _runtimeFactory;
        private DefaultInstanceIdGenerator _idGenerator;

        [SetUp]
        public void Setup()
        {
            var provider = new EditorExternalGameDataProvider();
            var serializer = new UnityJsonSerializer();
            
            _database = new GameDatabase();
            var builder = new DatabaseBuilder(provider, serializer, _database);
            builder.Build();
            
            // Post-load category patch for items to bypass JsonUtility auto-property limitations in tests
            foreach (var item in _database.GetAll<ItemDefinition>())
            {
                if (string.IsNullOrEmpty(item.parentClass)) continue;
                string pc = item.parentClass.ToLower();
                if (pc == "item" || pc == "upgrade") item.Category = ItemCategory.Material;
                else if (pc == "heavyarmor" || pc == "lightarmor" || pc == "mediumarmor") item.Category = ItemCategory.Armor;
                else if (pc == "sword" || pc == "dagger" || pc == "staff" || pc == "bow") item.Category = ItemCategory.Weapon;
                else if (pc == "accessory") item.Category = ItemCategory.Accessory;
                else if (pc == "consumable" || pc == "potion" || pc == "food") item.Category = ItemCategory.Consumable;
            }
            
            _saveService = new SaveService();
            _saveService.DeleteSave(); // Start clean
            _saveService.Load(out _);
            
            _formulaService = new FormulaService();
            
            _idGenerator = new DefaultInstanceIdGenerator();
            _runtimeFactory = new RuntimeFactory(_idGenerator);

            _itemService = new ItemService(_runtimeFactory, _database);
            _inventoryService = new InventoryService(_saveService, _formulaService, _itemService, _database);
            _equipmentService = new EquipmentService(_inventoryService, _saveService);
            
            _characterService = new CharacterService(_saveService, _formulaService, _database, _runtimeFactory, _inventoryService);
        }

        [TearDown]
        public void TearDown()
        {
            _saveService.DeleteSave();
        }

        [Test]
        public void S2_001_ItemSystem_LoadedCorrectly()
        {
            var items = _database.GetAll<ItemDefinition>();
            Assert.IsTrue(items.Count > 0, "Items should be loaded");
            
            // Just test a known item from decode
            var wood = _database.GetRequired<ItemDefinition>("wood");
            Assert.IsNotNull(wood);
            Assert.AreEqual(ItemCategory.Material, wood.Category);
        }

        [Test]
        public void S2_002_InventorySystem_CapacityAndStacking()
        {
            int capacity = _inventoryService.GetCapacity();
            Assert.IsTrue(capacity >= 35, "Base capacity should be at least 35");

            var woodDef = _database.GetRequired<ItemDefinition>("wood");
            
            // Add wood
            var wood = new ItemRuntime("inst_wood_1", woodDef, 10);
            _inventoryService.AddItem(wood);
            
            Assert.IsTrue(_inventoryService.HasItem("inst_wood_1"));
            
            var existing = _inventoryService.GetItem("inst_wood_1");
            Assert.AreEqual(10, existing.StackCount);
            
            // Add more wood (should stack)
            var wood2 = new ItemRuntime("inst_wood_2", woodDef, 15);
            _inventoryService.AddItem(wood2);
            
            Assert.AreEqual(25, existing.StackCount); // It should have stacked into existing
        }

        [Test]
        public void S2_003_EquipmentSystem_SlotRestrictions()
        {
            // Phase 1: "adventurer" is Java's abstract Adventurer base class (never instantiable —
            // Adventurer.getInstance() only reflects into concrete units/*.java subclasses) and is
            // now correctly filtered out of the AdventurerDefinition table (DatabaseBuilder.cs,
            // parentClass == "Adventurer" filter — see Docs/Backend_Audit/phase1_audit_report.md).
            // "footman" is a real playable class (confirmed against the 116 Legacy hero units).
            var character = _characterService.CreateCharacter("footman");
            
            var swordDef = _database.GetAll<ItemDefinition>().First(i => i.Category == ItemCategory.Weapon);
            var sword = new ItemRuntime("inst_sword_1", swordDef, 1);
            _inventoryService.AddItem(sword);
            
            bool equipped = _equipmentService.Equip(character, "inst_sword_1", EquipmentSlot.Weapon);
            Assert.IsTrue(equipped, "Should equip weapon");
            Assert.IsNotNull(character.Weapon);
            Assert.IsTrue(_inventoryService.HasItem("inst_sword_1"), "Sword should NOT be removed from inventory");
            Assert.IsTrue(_inventoryService.GetItem("inst_sword_1").IsLocked, "Equipped sword should be locked");
        }

        [Test]
        public void S2_004_CharacterSystem_StatAggregationAndLevelUp()
        {
            // Phase 1: "adventurer" is Java's abstract Adventurer base class (never instantiable —
            // Adventurer.getInstance() only reflects into concrete units/*.java subclasses) and is
            // now correctly filtered out of the AdventurerDefinition table (DatabaseBuilder.cs,
            // parentClass == "Adventurer" filter — see Docs/Backend_Audit/phase1_audit_report.md).
            // "footman" is a real playable class (confirmed against the 116 Legacy hero units).
            var character = _characterService.CreateCharacter("footman");
            Assert.AreEqual(1, character.Level);
            
            int initialHp = _characterService.GetTotalStat(character, StatType.MaxHp);
            Assert.IsTrue(initialHp > 0);
            
            _characterService.GainExperience(character, 500); // Enough to level up
            
            Assert.IsTrue(character.Level > 1, "Character should level up");
            
            int newHp = _characterService.GetTotalStat(character, StatType.MaxHp);
            Assert.IsTrue(newHp > initialHp, "Stats should increase on level up");
        }
    }
}
