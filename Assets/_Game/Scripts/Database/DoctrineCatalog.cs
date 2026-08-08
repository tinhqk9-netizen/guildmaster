using System.Collections.Generic;
using GuildMaster.Definitions;

namespace GuildMaster.Database
{
    /// <summary>
    /// Phase 2A: hand-transcribed, 1:1 port of the Legacy Doctrine catalog. No doctrines.json
    /// exists anywhere in the decoded data (Phase 0 finding, unchanged) — Java hardcodes both the
    /// 40-value <c>DoctrineAbilityType</c> enum (name/cost/increasePerLevel/maxLevel/row per
    /// entry) and each of the 8 <c>Doctrine</c> subclasses' <c>setupAbilities()</c> (which 6 of the
    /// 40 ability types occupy that doctrine's l1..l6 slots). Every number below was read directly
    /// from:
    ///   - storage/data/entities/adventurers/doctrines/DoctrineAbilityType.java (cost, increasePerLevel, maxLevel)
    ///   - storage/data/entities/adventurers/doctrines/instances/DoctrineOf*.java (setupAbilities() ordering)
    /// This is data authored in C# (no JSON pipeline exists for it), not gameplay logic — it is
    /// registered into GameDatabase by DatabaseBuilder alongside the JSON-sourced categories so
    /// every other system (DoctrineService, UI) reads it the same way as any other Definition.
    /// </summary>
    public static class DoctrineCatalog
    {
        public struct AbilityTypeInfo
        {
            public string Id;
            public int Cost;
            public int IncreasePerLevel;
            public int MaxLevel;

            public AbilityTypeInfo(string id, int cost, int increasePerLevel, int maxLevel)
            {
                Id = id; Cost = cost; IncreasePerLevel = increasePerLevel; MaxLevel = maxLevel;
            }
        }

        // Java signature: DoctrineAbilityType(name, description, image, cost, increasePerLevel, formatMode, maxLevel, row)
        // Fields kept here: cost, increasePerLevel, maxLevel (formatMode/row are display-only, not needed for gameplay).
        public static readonly List<AbilityTypeInfo> AbilityTypes = new List<AbilityTypeInfo>
        {
            new AbilityTypeInfo("IMPROVED_HEALTH", 1, 15, 5),
            new AbilityTypeInfo("IMPROVED_CONSTITUTION", 1, 2, 5),
            new AbilityTypeInfo("IMPROVED_DEXTERITY", 1, 2, 5),
            new AbilityTypeInfo("IMPROVED_INTELLIGENCE", 1, 2, 5),
            new AbilityTypeInfo("EXALTED_CONSTITUTION", 1, 3, 5),
            new AbilityTypeInfo("EXALTED_DEXTERITY", 1, 3, 5),
            new AbilityTypeInfo("EXALTED_INTELLIGENCE", 1, 3, 5),
            new AbilityTypeInfo("EXALTED_HEALTH", 1, 25, 5),
            new AbilityTypeInfo("EXALTED_MANA", 3, 1, 3),
            new AbilityTypeInfo("LORE_MASTER", 10, 100, 1),
            new AbilityTypeInfo("SERVUS_SANGUINIS", 2, 8, 3),
            new AbilityTypeInfo("SERVUS_UMBRAE", 2, 2, 3),
            new AbilityTypeInfo("NECROSIS_PORPHYRICA", 3, 25, 3),
            new AbilityTypeInfo("GENUS_VAMPYRI", 5, 20, 1),
            new AbilityTypeInfo("IMPENETRABLE_WILLPOWER", 2, 20, 3),
            new AbilityTypeInfo("CHILLING_FLOW", 3, 30, 2),
            new AbilityTypeInfo("MIND_BENDER", 2, 5, 3),
            new AbilityTypeInfo("STAR_GAZE", 4, 5, 2),
            new AbilityTypeInfo("ARCANE_SUPPRESSION", 6, 150, 1),
            new AbilityTypeInfo("CONDITIONED_REFLEXES", 2, 10, 3),
            new AbilityTypeInfo("TACTICAL_KNOWLEDGE", 3, 20, 2),
            new AbilityTypeInfo("RELENTLESS_ASSAULT", 7, 1, 1),
            new AbilityTypeInfo("WEAPON_MASTER", 10, 1, 1),
            new AbilityTypeInfo("EPHEMERAL_PRESENCE", 2, 3, 3),
            new AbilityTypeInfo("BEAT_THE_ODDS", 3, 1, 1),
            new AbilityTypeInfo("FALSE_LIFE", 4, 4, 2),
            new AbilityTypeInfo("TRUE_AGONY", 3, 1500, 1),
            new AbilityTypeInfo("TROLL_RESISTANCE", 3, 1, 2),
            new AbilityTypeInfo("WARLOCK_RESILIENCE", 3, 1, 2),
            new AbilityTypeInfo("MANIFEST_DANGER", 4, 1, 1),
            new AbilityTypeInfo("MIRROR_OF_ANGUISH", 8, 1, 1),
            new AbilityTypeInfo("EXPOSE_WEAKNESS", 2, 8, 3),
            new AbilityTypeInfo("EXPLOIT_WEAKNESS", 2, 12, 3),
            new AbilityTypeInfo("LIGHTNING_SPEED", 3, 15, 3),
            new AbilityTypeInfo("EYE_FOR_AN_EYE", 4, 50, 1),
            new AbilityTypeInfo("RAGEBOUND", 4, 35, 1),
            new AbilityTypeInfo("DIVINE_INTERVENTION", 2, 1, 3),
            new AbilityTypeInfo("SELFLESS_SPIRIT", 2, 10, 4),
            new AbilityTypeInfo("OVERHEAL", 3, 5, 2),
            new AbilityTypeInfo("HEALING_NOVA", 5, 7, 1),
        };

        // Java: 8 concrete Doctrine subclasses, each setupAbilities() returning an ordered
        // 6-element list mapped to l1..l6. Order matters (it is the save-slot index).
        public static readonly Dictionary<string, string[]> DoctrineNodeAssignment = new Dictionary<string, string[]>
        {
            ["war"] = new[] { "IMPROVED_CONSTITUTION", "IMPROVED_DEXTERITY", "CONDITIONED_REFLEXES", "TACTICAL_KNOWLEDGE", "RELENTLESS_ASSAULT", "WEAPON_MASTER" },
            ["affliction"] = new[] { "IMPROVED_HEALTH", "IMPROVED_DEXTERITY", "NECROSIS_PORPHYRICA", "SERVUS_SANGUINIS", "SERVUS_UMBRAE", "GENUS_VAMPYRI" },
            ["control"] = new[] { "IMPROVED_INTELLIGENCE", "IMPENETRABLE_WILLPOWER", "MIND_BENDER", "CHILLING_FLOW", "STAR_GAZE", "ARCANE_SUPPRESSION" },
            ["fortitude"] = new[] { "IMPROVED_HEALTH", "IMPROVED_CONSTITUTION", "MANIFEST_DANGER", "TROLL_RESISTANCE", "WARLOCK_RESILIENCE", "MIRROR_OF_ANGUISH" },
            ["grace"] = new[] { "IMPROVED_HEALTH", "IMPROVED_INTELLIGENCE", "SELFLESS_SPIRIT", "DIVINE_INTERVENTION", "OVERHEAL", "HEALING_NOVA" },
            ["illusion"] = new[] { "IMPROVED_DEXTERITY", "IMPROVED_INTELLIGENCE", "EPHEMERAL_PRESENCE", "BEAT_THE_ODDS", "FALSE_LIFE", "TRUE_AGONY" },
            ["knowledge"] = new[] { "EXALTED_CONSTITUTION", "EXALTED_DEXTERITY", "EXALTED_INTELLIGENCE", "EXALTED_HEALTH", "EXALTED_MANA", "LORE_MASTER" },
            ["ruin"] = new[] { "IMPROVED_DEXTERITY", "EXPOSE_WEAKNESS", "EXPLOIT_WEAKNESS", "LIGHTNING_SPEED", "EYE_FOR_AN_EYE", "RAGEBOUND" },
        };

        public static AbilityTypeInfo GetAbilityInfo(string abilityTypeId)
        {
            foreach (var a in AbilityTypes)
                if (a.Id == abilityTypeId) return a;
            return default;
        }

        /// <summary>Builds the 8 DoctrineDefinition records (6 nodes each) from the tables above.</summary>
        public static List<DoctrineDefinition> BuildDefinitions()
        {
            var result = new List<DoctrineDefinition>();
            foreach (var kv in DoctrineNodeAssignment)
            {
                var def = new DoctrineDefinition { id = kv.Key, nameKey = "doctrine_" + kv.Key + "_name" };
                for (int i = 0; i < kv.Value.Length; i++)
                {
                    var abilityId = kv.Value[i];
                    var info = GetAbilityInfo(abilityId);
                    def.Nodes.Add(new DoctrineNodeDefinition
                    {
                        NodeId = "l" + (i + 1),
                        AbilityType = abilityId,
                        MaxLevel = info.MaxLevel,
                        Cost = info.Cost,
                        IncreasePerLevel = info.IncreasePerLevel
                    });
                }
                result.Add(def);
            }
            return result;
        }
    }
}
