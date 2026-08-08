using System;
using System.Collections.Generic;
using UnityEngine;
using GuildMaster.Definitions;

namespace GuildMaster.Database
{
    /// <summary>
    /// Fills <see cref="ItemDefinition"/> stat fields (Constitution, Dexterity, Intelligence, Defense, MagicDefense, MaxHp)
    /// from the dynamic <c>"fields"</c> dictionary inside <c>items.json</c>.
    /// JsonUtility skips standard dictionaries during generic deserialization, so this class uses a typed DTO structure.
    /// </summary>
    public static class ItemFieldsLoader
    {
        [Serializable]
        private class ItemFileDto
        {
            public List<ItemDto> data;
        }

        [Serializable]
        private class ItemDto
        {
            public string id;
            public ItemFieldsDto fields;

            // Phase 0: these sit at the TOP LEVEL of each item record in items.json (not inside
            // "fields"), as lowercase JSON keys. ItemDefinition declares the equivalent members
            // as PascalCase (Price/Rarity/NotSellable), which JsonUtility's case-sensitive exact
            // match never binds — so they were always default (0/false) before Phase 0, entirely
            // independent of any missing data. Read here and assigned directly below instead.
            public long price;
            public string rarity;
            public bool notSellable;
        }

        [Serializable]
        private class ItemFieldsDto
        {
            public FieldValueDto constitution;
            public FieldValueDto dexterity;
            public FieldValueDto intelligence;
            public FieldValueDto defense;
            public FieldValueDto magicDefense;
            public FieldValueDto maxHp;
            public FieldValueDto feedPower;
            public FieldValueStringDto idImage;

            // Phase 2A: Equipment combat modifiers (verified present in items.json's "fields"
            // dict under these exact Java field names — see Docs/Backend_Audit/equipment_audit.md
            // and phase2a_audit_report.md). Previously entirely unparsed (data-loss bug).
            public FieldValueDto lifesteal;
            public FieldValueDto lifestealWithMinion;
            public FieldValueDto threat;
            public FieldValueDto bonusExperience;
            public FieldValueDto darknessReduction;
            public FieldValueDto regeneration;
            public FieldValueDto regenerationBonus;
            public FieldValueDto retaliationPhysicalDamage;
            public FieldValueDto retaliationMagicalDamage;
            public FieldValueDto onFireBonusDamage;
            public FieldValueDto freezeBonusDamage;
            public FieldValueDto poisonBonus;
            public FieldValueDto livingCompanionBonusDamage;
            public FieldValueDto decay;
            public FieldValueDto exaltInspireBonusTurns;

            public FieldValueDoubleDto counterattack;
            public FieldValueDoubleDto criticalChance;
            public FieldValueDoubleDto criticalDamage;
            public FieldValueDoubleDto flatDodgeChance;
            public FieldValueDoubleDto healingModifier;
            public FieldValueDoubleDto immunityToStatus;
            public FieldValueDoubleDto darknessDamageAmplification;

            public FieldValueBoolDto initiative;
            public FieldValueBoolDto alwaysHits;
        }

        [Serializable]
        private class FieldValueDto
        {
            public int value;
        }

        [Serializable]
        private class FieldValueStringDto
        {
            public string value;
        }

        [Serializable]
        private class FieldValueDoubleDto
        {
            public double value;
        }

        [Serializable]
        private class FieldValueBoolDto
        {
            public bool value;
        }

        public static int Apply(string rawJson, IEnumerable<ItemDefinition> definitions)
        {
            if (string.IsNullOrEmpty(rawJson) || definitions == null) return 0;

            ItemFileDto fileDto;
            try
            {
                fileDto = JsonUtility.FromJson<ItemFileDto>(rawJson);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemFieldsLoader] JsonUtility failed: {ex.Message}");
                return 0;
            }

            if (fileDto == null || fileDto.data == null) return 0;

            var map = new Dictionary<string, ItemDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in fileDto.data)
            {
                if (item != null && !string.IsNullOrEmpty(item.id))
                {
                    map[item.id] = item;
                }
            }

            int enriched = 0;
            foreach (var def in definitions)
            {
                if (def == null || string.IsNullOrEmpty(def.id)) continue;

                if (!map.TryGetValue(def.id, out var item)) continue;

                bool touched = false;

                // Top-level fields (Phase 0 fix — see ItemDto.price/rarity/notSellable comment).
                if (item.price != 0) { def.Price = item.price; touched = true; }
                if (!string.IsNullOrEmpty(item.rarity))
                {
                    def.RarityId = item.rarity;
                    def.Rarity = MapRarityToInt(item.rarity);
                    touched = true;
                }
                if (item.notSellable) { def.NotSellable = true; touched = true; }

                var fields = item.fields;
                if (fields != null)
                {
                    if (fields.constitution != null) { def.Constitution = fields.constitution.value; touched = true; }
                    if (fields.dexterity != null)    { def.Dexterity    = fields.dexterity.value; touched = true; }
                    if (fields.intelligence != null) { def.Intelligence = fields.intelligence.value; touched = true; }
                    if (fields.defense != null)      { def.Defense      = fields.defense.value; touched = true; }
                    if (fields.magicDefense != null) { def.MagicDefense = fields.magicDefense.value; touched = true; }
                    if (fields.maxHp != null)        { def.MaxHp        = fields.maxHp.value; touched = true; }
                    if (fields.feedPower != null)    { def.FeedPower    = fields.feedPower.value; touched = true; }
                    if (fields.idImage != null && !string.IsNullOrEmpty(fields.idImage.value))
                    {
                        def.IdImage = fields.idImage.value;
                        touched = true;
                    }

                    // Phase 2A: real Equipment combat modifiers (was 100% data loss before).
                    if (fields.lifesteal != null) { def.Lifesteal = fields.lifesteal.value; touched = true; }
                    if (fields.lifestealWithMinion != null) { def.LifestealWithMinion = fields.lifestealWithMinion.value; touched = true; }
                    if (fields.threat != null) { def.Threat = fields.threat.value; touched = true; }
                    if (fields.bonusExperience != null) { def.BonusExperience = fields.bonusExperience.value; touched = true; }
                    if (fields.darknessReduction != null) { def.DarknessReduction = fields.darknessReduction.value; touched = true; }
                    if (fields.regeneration != null) { def.Regeneration = fields.regeneration.value; touched = true; }
                    if (fields.regenerationBonus != null) { def.RegenerationBonus = fields.regenerationBonus.value; touched = true; }
                    if (fields.retaliationPhysicalDamage != null) { def.RetaliationPhysicalDamage = fields.retaliationPhysicalDamage.value; touched = true; }
                    if (fields.retaliationMagicalDamage != null) { def.RetaliationMagicalDamage = fields.retaliationMagicalDamage.value; touched = true; }
                    if (fields.onFireBonusDamage != null) { def.OnFireBonusDamage = fields.onFireBonusDamage.value; touched = true; }
                    if (fields.freezeBonusDamage != null) { def.FreezeBonusDamage = fields.freezeBonusDamage.value; touched = true; }
                    if (fields.poisonBonus != null) { def.PoisonBonus = fields.poisonBonus.value; touched = true; }
                    if (fields.livingCompanionBonusDamage != null) { def.LivingCompanionBonusDamage = fields.livingCompanionBonusDamage.value; touched = true; }
                    if (fields.decay != null) { def.Decay = fields.decay.value; touched = true; }
                    if (fields.exaltInspireBonusTurns != null) { def.ExaltInspireBonusTurns = fields.exaltInspireBonusTurns.value; touched = true; }

                    if (fields.counterattack != null) { def.Counterattack = fields.counterattack.value; touched = true; }
                    if (fields.criticalChance != null) { def.CriticalChance = fields.criticalChance.value; touched = true; }
                    if (fields.criticalDamage != null) { def.CriticalDamage = fields.criticalDamage.value; touched = true; }
                    if (fields.flatDodgeChance != null) { def.FlatDodgeChance = fields.flatDodgeChance.value; touched = true; }
                    if (fields.healingModifier != null) { def.HealingModifier = fields.healingModifier.value; touched = true; }
                    if (fields.immunityToStatus != null) { def.ImmunityToStatus = fields.immunityToStatus.value; touched = true; }
                    if (fields.darknessDamageAmplification != null) { def.DarknessDamageAmplification = fields.darknessDamageAmplification.value; touched = true; }

                    if (fields.initiative != null && fields.initiative.value) { def.Initiative = true; touched = true; }
                    if (fields.alwaysHits != null && fields.alwaysHits.value) { def.AlwaysHits = true; touched = true; }
                }

                if (touched) enriched++;
            }

            return enriched;
        }

        /// <summary>
        /// Best-effort mapping of Java's rarity enum string to the legacy int scale some
        /// existing UI/Quest code already reads (ItemDefinition.Rarity). Only known Legacy
        /// tiers are mapped; anything unrecognized leaves Rarity at its previous value so
        /// nothing regresses silently.
        /// </summary>
        private static int MapRarityToInt(string rarityId)
        {
            switch (rarityId.ToUpperInvariant())
            {
                case "COMMON": return 0;
                case "UNCOMMON": return 1;
                case "RARE": return 2;
                case "EPIC": return 3;
                case "LEGENDARY": return 4;
                default: return 0;
            }
        }
    }
}
