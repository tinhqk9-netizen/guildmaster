using System;
using System.Collections.Generic;

namespace GuildMaster.Definitions
{
    /// <summary>
    /// One of a doctrine's 6 fixed node slots (Java: l1..l6), backed by a
    /// DoctrineAbilityType entry (own maxLevel/cost/increasePerLevel).
    /// </summary>
    [Serializable]
    public class DoctrineNodeDefinition
    {
        public string NodeId;       // "l1".."l6"
        public string AbilityType;  // DoctrineAbilityType id, e.g. "IMPROVED_HEALTH"
        public int MaxLevel;
        public int Cost;
        public float IncreasePerLevel;
    }

    /// <summary>
    /// Java: abstract Doctrine.java + 8 concrete subclasses (Affliction, Control, Fortitude,
    /// Grace, Illusion, Knowledge, Ruin, War), each with exactly 6 independent nodes. This class
    /// did not exist before Phase 0 — DoctrineService.cs hardcodes the 8 doctrine names in
    /// string switches, and SaveData previously stored only one summed Level/Progress pair per
    /// doctrine (see SaveData.cs DoctrineNodes for the save-side half of this restore).
    ///
    /// No doctrines.json exists yet; the 8x6 catalog is hand-documented in
    /// Reports/Decode_Final_Systems_Deep_Audit_20260729/05_Doctrine_Exact_Catalog.md for
    /// whoever authors the JSON in a later phase. See
    /// Docs/Backend_Audit/phase0_schema_mapping.md §5.
    /// </summary>
    [Serializable]
    public class DoctrineDefinition : DefinitionBase
    {
        public List<DoctrineNodeDefinition> Nodes = new List<DoctrineNodeDefinition>();
    }
}
