using System;

namespace GuildMaster.Definitions
{
    [Serializable]
    public class QuestDefinition : DefinitionBase
    {
        public long TargetProgress { get; set; }
        public string TrueClass { get; set; }
    }
}
