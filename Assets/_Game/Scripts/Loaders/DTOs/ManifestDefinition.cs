using System;
using System.Collections.Generic;

namespace GuildMaster.Loaders.DTOs
{
    [Serializable]
    public class ManifestFileEntry
    {
        public string filename;
        public string category;
        public int recordCount;
        public string hash;
        public int loadOrder;
        public List<string> dependencies;
    }

    [Serializable]
    public class ManifestDefinition
    {
        public string schemaVersion;
        public string converterVersion;
        public string generatedAt;
        public string sourceHash;
        public string runId;
        public bool deterministic;
        public List<ManifestFileEntry> files;
    }
}
