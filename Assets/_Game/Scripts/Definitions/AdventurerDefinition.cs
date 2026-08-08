using System;

namespace GuildMaster.Definitions
{
    /// <summary>
    /// Adventurer class stats, recovered from each unit's <c>configureStatistics()</c>.
    ///
    /// Public FIELDS, not properties: Unity's JsonUtility ignores properties, so the previous
    /// property-based version deserialized every stat as 0 — which made adventurers spawn with
    /// no HP and no attack.
    /// </summary>
    [Serializable]
    public class AdventurerDefinition : DefinitionBase
    {
        public int MaxLevel;
        public int BaseMaxHp;
        public int BaseConstitution;
        public int BaseIntelligence;
        public int BaseDexterity;
        public int BaseDefense;
        public int BaseMagicDefense;

        public string StarterWeaponId;

        // Maps to ItemType in ItemDefinition for equip restrictions.
        public string WeaponType;
        public string ArmorType;

        public string[] NextClasses;
        public string PassiveSkill;
        public string ActiveSkill;

        // Portrait sprite id (Java: imageId, e.g. "unit_apprentice" from R.drawable.unit_apprentice).
        // Restored in Phase 1 via a narrow regex extraction of all 116
        // adventurers/units/*.java configureStatistics() bodies (Docs/Backend_Audit/phase1_audit_report.md).
        public string ImageId;

        // Java: idDescription (R.string.*). Raw resource key, not translated text — same
        // convention as SkillDefinition.NameKey/DescriptionKey (no localization table exists
        // in this project yet). "nameKey" itself lives on DefinitionBase.
        public string descriptionKey;

        // Java: potionDrinkerType (a real, simple enum field — not a complex mechanic).
        // Restored as its own field per Phase 0; the old field below is left in place
        // unreferenced elsewhere so nothing else needs to change.
        public string PotionDrinkerType;

        // Complex field from Java that has no port yet.
        public string ManualRuleRequired_PotionDrinkerType;
    }
}
