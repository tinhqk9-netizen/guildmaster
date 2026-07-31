using System.Collections.Generic;

namespace GuildMaster.Runtime.Models
{
    public class ExpeditionRuntime
    {
        public int SlotIndex { get; set; }
        public DungeonRuntime Dungeon { get; set; }
        public List<CharacterRuntime> Party { get; } = new List<CharacterRuntime>();
    }
}
