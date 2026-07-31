using System;

namespace GuildMaster.Definitions
{
    [Serializable]
    public class ItemDefinition : DefinitionBase
    {
        public ItemCategory Category;
        
        // This is the specific type matched against Adventurer.WeaponType or ArmorType
        // Example: "Staff", "LightArmor", "Dagger", etc.
        public string ItemType;

        public long Price;
        public long SellPrice;
        public bool Consumable;
        public bool NotSellable;
        public int Rarity;
        
        // Stats for Equipment
        public int Constitution;
        public int Dexterity;
        public int Intelligence;
        public int Defense;
        public int MagicDefense;
        public int MaxHp;
        
        // Extended modifiers for later sprints
        // Added manualRuleRequired tag for complex fields like StatusEffect
        public string ManualRuleRequired_StatusEffects;

        public string GetStatSummary()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (Constitution != 0) parts.Add($"{(Constitution > 0 ? "+" : "")}{Constitution} CON");
            if (Dexterity != 0)    parts.Add($"{(Dexterity > 0 ? "+" : "")}{Dexterity} DEX");
            if (Intelligence != 0) parts.Add($"{(Intelligence > 0 ? "+" : "")}{Intelligence} INT");
            if (Defense != 0)      parts.Add($"{(Defense > 0 ? "+" : "")}{Defense} DEF");
            if (MagicDefense != 0) parts.Add($"{(MagicDefense > 0 ? "+" : "")}{MagicDefense} MDEF");
            if (MaxHp != 0)        parts.Add($"{(MaxHp > 0 ? "+" : "")}{MaxHp} HP");

            if (parts.Count > 0) return string.Join(", ", parts);
            return !string.IsNullOrEmpty(ItemType) ? ItemType : Category.ToString();
        }
    }
}
