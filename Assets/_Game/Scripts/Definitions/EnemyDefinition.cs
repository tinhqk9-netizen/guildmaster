using System;
using System.Collections.Generic;

namespace GuildMaster.Definitions
{
    /// <summary>
    /// One weighted drop entry. Weights are per-mille against a fixed 1000 scale, matching
    /// <c>Utils.rollFromWeightedMap</c> — a table summing under 1000 keeps a deliberate
    /// "nothing dropped" gap.
    /// </summary>
    [Serializable]
    public class EnemyDropEntry
    {
        public string ItemId;
        public int Weight;
        public int StackCount = 1;
    }

    /// <summary>
    /// Enemy stats and drop table, recovered from the per-enemy Java classes in the APK —
    /// each unit overrides <c>configureStatistics()</c>, <c>getMinDamage()</c>,
    /// <c>getMaxDamage()</c> and <c>listDrops()</c>.
    ///
    /// All members are public FIELDS on purpose: Unity's JsonUtility ignores properties, so
    /// the earlier property-based version silently deserialized every stat as 0 — which is
    /// why combat could never do real damage.
    ///
    /// Evidence: Reports/S6_5A/S6_5A_Claude_DungeonCombatLoot_Audit_Report.md
    /// </summary>
    [Serializable]
    public class EnemyDefinition : DefinitionBase
    {
        public string nameKey;
        public string TrueClass;
        public string SourceClass;

        public int BaseMaxHp;
        public int BaseDefense;
        public int BaseMagicDefense;
        public int BaseConstitution;
        public int BaseIntelligence;
        public int BaseDexterity;
        public int BaseLifesteal;
        public int Counterattack;

        /// <summary>Attack range used by <c>rollAttackDamage()</c>.</summary>
        public int MinDamage;
        public int MaxDamage;

        public bool IsMagic;
        public bool IsRanged;

        public int ExpGiven;
        public int Rarity;

        public string ActiveSkillId;
        public string PassiveSkillId;

        /// <summary>
        /// Drop table, filled after deserialization from the raw JSON object because
        /// JsonUtility cannot read a dictionary. Empty for the three enemies whose
        /// <c>listDrops()</c> returns nothing.
        /// </summary>
        [NonSerialized] public List<EnemyDropEntry> DropTable = new List<EnemyDropEntry>();
    }
}
