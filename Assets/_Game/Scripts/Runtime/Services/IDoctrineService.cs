using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public interface IDoctrineService
    {
        // --- Legacy: account-wide "doctrine mastery" level (Java: warLevel/warProgress, ...).
        // Drives Doctrine.bonusQuestPoints() in Java — unrelated to node levels below. Untouched
        // by Phase 2A (Phase 0/1 already restored this half correctly).
        int GetLevel(string doctrineName);
        int GetProgress(string doctrineName);
        void AddProgress(string doctrineName, int amount);
        bool IsMaxed();

        // --- Phase 2A: per-node progression (Java: Doctrine.l1..l6 / DoctrineAbility.level).
        // Each of the 8 doctrines has exactly 6 independent nodes ("l1".."l6"), each backed by a
        // DoctrineAbilityType (own maxLevel/cost/increasePerLevel). See DoctrineCatalog.cs.
        int GetNodeLevel(string doctrineId, string nodeId);
        bool CanUpgradeNode(string doctrineId, string nodeId);
        bool UpgradeNode(string doctrineId, string nodeId);

        /// <summary>Java: Doctrine.getValue(abilityType) == level * increasePerLevel for that node.</summary>
        int GetNodeEffectValue(string doctrineId, string nodeId);

        /// <summary>
        /// Sums <c>level * increasePerLevel</c> across every doctrine node whose AbilityType id
        /// matches, e.g. GetAggregateAbilityValue("CONDITIONED_REFLEXES") for War's counterattack
        /// node. NOTE: Java's Adventurer holds exactly ONE doctrine at a time (per-character
        /// assignment); the Rebuild save format has no per-character doctrine slot yet (flagged
        /// in phase1_completion_report.md as a full Doctrine-system rebuild, out of Phase 2A
        /// scope). This aggregate sums across ALL 8 doctrines' progress as an account-wide
        /// approximation until that per-character assignment exists — documented simplification,
        /// not a Java-parity claim.
        /// </summary>
        int GetAggregateAbilityValue(string abilityTypeId);
    }
}
