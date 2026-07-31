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

        // Maps to ItemType in ItemDefinition for equip restrictions.
        public string WeaponType;
        public string ArmorType;

        public string[] NextClasses;
        public string PassiveSkill;
        public string ActiveSkill;

        // Complex field from Java that has no port yet.
        public string ManualRuleRequired_PotionDrinkerType;
    }
}
