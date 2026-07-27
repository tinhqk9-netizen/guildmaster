using System;
using System.Collections.Generic;

namespace GuildMaster.Loaders.DTOs
{
    [Serializable]
    public class DefinitionFileMetadata
    {
        public string category;
        public string sourceFile;
        public string generatedAt;
        public int recordCount;
    }

    [Serializable]
    public sealed class DefinitionFile<T>
    {
        public DefinitionFileMetadata metadata;
        public List<T> data;
    }
}
