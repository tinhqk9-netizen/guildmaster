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
        // Java Food.feedPower; populated from items.json fields at the data boundary.
        public int FeedPower;

        // Java: fields.rarity raw string value (e.g. "COMMON"). JSON's top-level `price`/
        // `rarity`/`notSellable` don't bind via JsonUtility (case-sensitive: "price" != "Price"),
        // so ItemFieldsLoader populates these fields directly from raw JSON. Rarity (int) above
        // is kept for existing UI/Quest consumers and is best-effort mapped from this string.
        public string RarityId;

        // Java: fields.idImage — per-item custom sprite override (11/607 items). Missing 100%
        // before Phase 0; populated by ItemFieldsLoader.
        public string IdImage;

        // Stats for Equipment
        public int Constitution;
        public int Dexterity;
        public int Intelligence;
        public int Defense;
        public int MagicDefense;
        public int MaxHp;

        // Phase 2A: Java `Equipment` combat modifiers. items.json's "fields" dictionary carries
        // these under their exact Java field names (verified against a live data sample —
        // e.g. "lifesteal":20, "threat":3, "counterattack":0.25, "criticalChance":0.1).
        // ItemFieldsLoader.cs was previously scoped to only 6 base stats and silently dropped
        // all of these (Docs/Backend_Audit/equipment_audit.md, §1.2) — real, non-fabricated
        // Legacy data that now actually populates them.
        public int Lifesteal;
        public int LifestealWithMinion;
        public int Threat;
        public int BonusExperience;
        public int DarknessReduction;
        public int Regeneration;
        public int RegenerationBonus;
        public int RetaliationPhysicalDamage;
        public int RetaliationMagicalDamage;
        public int OnFireBonusDamage;
        public int FreezeBonusDamage;
        public int PoisonBonus;
        public int LivingCompanionBonusDamage;
        public int Decay;
        public int ExaltInspireBonusTurns;

        // Java doubles (percentage-scale, e.g. 0.25 == 25%).
        public double Counterattack;
        public double CriticalChance;
        public double CriticalDamage;
        public double FlatDodgeChance;
        public double HealingModifier;
        public double ImmunityToStatus;
        public double DarknessDamageAmplification;

        public bool Initiative;
        public bool AlwaysHits;

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

            // Phase 2A: surface the previously-invisible combat modifiers so equipped items with
            // these stats actually show their effect (was silently dropped data before Phase 2A —
            // see Docs/Backend_Audit/equipment_audit.md §1.2). Base stat block above kept as-is.
            if (Lifesteal != 0)      parts.Add($"+{Lifesteal}% Lifesteal");
            if (Threat != 0)         parts.Add($"{(Threat > 0 ? "+" : "")}{Threat} Threat");
            if (Counterattack != 0)  parts.Add($"+{Counterattack * 100:0}% Counterattack");
            if (CriticalChance != 0) parts.Add($"+{CriticalChance * 100:0}% Crit Chance");
            if (CriticalDamage != 0) parts.Add($"+{CriticalDamage * 100:0}% Crit Damage");
            if (FlatDodgeChance != 0) parts.Add($"+{FlatDodgeChance * 100:0}% Dodge");
            if (Regeneration != 0)   parts.Add($"+{Regeneration} Regen");

            if (parts.Count > 0) return string.Join(", ", parts);
            return !string.IsNullOrEmpty(ItemType) ? ItemType : Category.ToString();
        }
    }
}
