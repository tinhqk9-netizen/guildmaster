using System;

namespace GuildMaster.Definitions
{
    [Serializable]
    public class ItemDefinition : DefinitionBase
    {
        public ItemCategory Category { get; set; }
        
        // This is the specific type matched against Adventurer.WeaponType or ArmorType
        // Example: "Staff", "LightArmor", "Dagger", etc.
        public string ItemType { get; set; }

        public long Price { get; set; }
        public long SellPrice { get; set; }
        public bool Consumable { get; set; }
        public bool NotSellable { get; set; }
        public int Rarity { get; set; }
        
        // Stats for Equipment
        public int Constitution { get; set; }
        public int Dexterity { get; set; }
        public int Intelligence { get; set; }
        public int Defense { get; set; }
        public int MagicDefense { get; set; }
        public int MaxHp { get; set; }
        
        // Extended modifiers for later sprints
        // Added manualRuleRequired tag for complex fields like StatusEffect
        public string ManualRuleRequired_StatusEffects { get; set; }
    }
}
