using System;
using System.Collections.Generic;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Save
{
    [Serializable]
    public class SaveMetadata
    {
        public int SaveVersion;
        public long SaveTimeUnix;
        public string GameVersion;
        public string DataVersion;
    }

    [Serializable]
    public class ItemSaveData
    {
        public string DefinitionId;
        public string InstanceId;
        public int StackCount;
        public bool IsLocked;
    }

    [Serializable]
    public class StatusEffectSaveData
    {
        public GuildMaster.Definitions.Enums.StatusEffectType Type;
        public string SourceInstanceId;
        public int TurnsLeft;
    }

    [Serializable]
    public class CharacterSaveData
    {
        public string DefinitionId;
        public string InstanceId;
        public int Level;
        public int Exp;
        public float CurrentHp;
        
        public string WeaponInstanceId;
        public string ArmorInstanceId;
        public string AccessoryInstanceId;

        public List<StatusEffectSaveData> PositiveStatusEffects = new List<StatusEffectSaveData>();
        public List<StatusEffectSaveData> NegativeStatusEffects = new List<StatusEffectSaveData>();
    }

    [Serializable]
    public class QuestSaveData
    {
        public string DefinitionId;
        public string InstanceId;
        public QuestState State;
        public long Progress;
    }

    [Serializable]
    public class DungeonSaveData
    {
        public string DefinitionId;
        public string InstanceId;
        public DungeonState State;
        public int ClearCount;
        public float BestTimeSeconds;
    }

    [Serializable]
    public class SkillSaveData
    {
        public string DefinitionId;
        public string InstanceId;
        public int Level;
        public float CurrentCooldown;
    }

    [Serializable]
    public class ItemActionSaveData
    {
        public string DefinitionId;
        public string InstanceId;
        public int StackCount;
        public long SecondsPassed;
    }

    [Serializable]
    public class SaveData
    {
        public SaveMetadata Metadata;
        
        // Storage progressions
        public int LevelStorage;
        public int UpgradeStorage;
        
        // S3 Batch 2 - Currency and Upgrades (Mapped from Java Data.java)
        // Java money -> Unity Money
        public long Money;
        public long Gems;
        
        // Java levelWorkshopTime -> Unity LevelWorkshopTime
        public int LevelWorkshopTime;
        // Java levelMarketTime -> Unity LevelMarketTime
        public int LevelMarketTime;
        
        // S3 Batch 2 - Craft & Merchant queues
        public List<ItemActionSaveData> WorkshopQueue = new List<ItemActionSaveData>();
        public List<ItemActionSaveData> CompletedWorkshopItems = new List<ItemActionSaveData>();
        public List<ItemActionSaveData> MarketListings = new List<ItemActionSaveData>();
        public List<ItemActionSaveData> SoldMarketItems = new List<ItemActionSaveData>();
        
        public List<ItemSaveData> Items = new List<ItemSaveData>();
        public List<CharacterSaveData> Characters = new List<CharacterSaveData>();
        public List<QuestSaveData> Quests = new List<QuestSaveData>();
        public List<DungeonSaveData> Dungeons = new List<DungeonSaveData>();
        public List<SkillSaveData> Skills = new List<SkillSaveData>();
        
        public ActiveDungeonSaveData ActiveDungeon = null;
    }
}
