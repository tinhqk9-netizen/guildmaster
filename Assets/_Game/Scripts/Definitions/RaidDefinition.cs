using System;

namespace GuildMaster.Definitions
{
    [Serializable]
    public class RaidDefinition : DefinitionBase
    {
        public string RequiredClearDungeonId;
        public int RequiredClearProgress;
    }
}
