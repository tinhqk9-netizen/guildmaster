using System;
using System.Collections.Generic;

namespace GuildMaster.Runtime.Save
{
    [Serializable]
    public class ActiveDungeonSaveData
    {
        public string DungeonDefinitionId;
        public int Progress;
        public int MaxProgress;
        public int LocalDarkness;
        
        public List<string> AdventurerInstanceIds = new List<string>();
        public List<ItemSaveData> PendingDrops = new List<ItemSaveData>();
        
        public CombatEncounterSaveData EncounterState = new CombatEncounterSaveData();
        public DungeonActionState ActionState = new DungeonActionState();
    }
}
