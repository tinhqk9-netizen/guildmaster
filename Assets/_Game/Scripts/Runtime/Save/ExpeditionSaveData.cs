using System;

namespace GuildMaster.Runtime.Save
{
    [Serializable]
    public class ExpeditionSaveData
    {
        public int SlotIndex;
        public ActiveDungeonSaveData Dungeon;
    }
}
