using System;
using System.Collections.Generic;

namespace GuildMaster.Definitions
{
    [Serializable]
    public abstract class DefinitionBase
    {
        public string id;
        public string className;
        public string parentClass;
        public string recordHash;
        public string parseStatus;
        public bool manualRuleRequired;
        public string sourcePath;
        public List<string> parseReasons;

        // Phase 0: shared localization keys that appear at the top level of every category's
        // JSON (items/adventurers/dungeons/...), currently always null because the Python
        // parser doesn't resolve Java's R.string.* references. Schema slot only — see
        // Docs/Backend_Audit/phase0_schema_mapping.md §9.
        public string nameKey;
        public string iconKey;
    }
}
