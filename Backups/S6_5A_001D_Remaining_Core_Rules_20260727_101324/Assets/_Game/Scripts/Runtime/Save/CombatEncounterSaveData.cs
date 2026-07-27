using System;
using System.Collections.Generic;

namespace GuildMaster.Runtime.Save
{
    [Serializable]
    public class EnemySaveData
    {
        public string DefinitionId;
        public int CurrentHp;
        public int CurrentMana;
        public int CurrentShield;
        public List<StatusEffectSaveData> PositiveStatusEffects = new List<StatusEffectSaveData>();
        public List<StatusEffectSaveData> NegativeStatusEffects = new List<StatusEffectSaveData>();
    }

    [Serializable]
    public class CombatEncounterSaveData
    {
        public List<EnemySaveData> Enemies = new List<EnemySaveData>();
        public List<EnemySaveData> Corpses = new List<EnemySaveData>();
        public int TurnsFighting;
        public string SavedActingEntityId;
    }
}
