using System;
using GuildMaster.Definitions.Enums;

namespace GuildMaster.Definitions
{
    [Serializable]
    public class StatusEffectDefinition : DefinitionBase
    {
        public StatusEffectType Type { get; set; }
        public bool IsNegative { get; set; }
        public bool IsSerialized { get; set; }
    }
}
