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
    }
}
