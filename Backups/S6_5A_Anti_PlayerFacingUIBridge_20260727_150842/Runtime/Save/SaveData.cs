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

        public bool IsAscended;
        public string Trait = string.Empty;
        public int[] PotionsDrank = new int[6];

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

    /// <summary>
    /// A merchant offer sitting in stock. Mirrors <c>MerchantOffer</c>: the item on sale, its
    /// price, and which currency pays for it — <c>IsGems</c> is what decides between money and
    /// gems at purchase time, so it must round-trip through the save.
    /// </summary>
    [Serializable]
    public class MerchantOfferSaveData
    {
        public string DefinitionId;
        public int StackCount;
        public long Price;
        public bool IsGems;
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

        // ---------------------------------------------------------------------------------
        // S6.5A Stage 1 — fields ported from Data.java that the core loop needs.
        // Names follow the existing PascalCase convention; the Java field each one maps to is
        // noted where the spelling differs. Everything defaults to the same value a fresh
        // Data instance has in the original, so an older save that lacks these keys still
        // loads with correct behaviour.
        // ---------------------------------------------------------------------------------

        // --- Purchase flags (Java: starterPackPurchased, adventurerPackPurchased, ...) ---
        // Not a store implementation — these are inputs to the capacity/price formulas.
        public bool StarterPackPurchased;
        public bool AdventurerPackPurchased;
        public bool MerchantPackPurchased;
        public bool ImperialVanguardPurchased;
        public bool UnholyCrusadePurchased;

        // --- Time markers (Java: lastAccess, lastHourTriggered, ...) ---
        // lastAccess drives offline progress; 0 means "never played", which the original
        // treats as a 1 second delta rather than a huge one.
        public long LastAccess;
        public long LastHourTriggered;
        public long Last24Triggered;
        public long LastWeekTriggered;

        // --- Tavern / quarters ---
        public long NextTavernVisit;
        public bool TavernLocked;
        public int TutorialStep;
        public List<CharacterSaveData> TavernGuests = new List<CharacterSaveData>();

        public int LevelQuarters;
        public int UpgradeQuarters;
        public int LevelTavernCapacity;
        public int UpgradeTavernCapacity;
        public int LevelTavernTime;
        public int UpgradeTavernTime;

        // --- Workshop / market / shelter levels ---
        // LevelStorage, UpgradeStorage, LevelWorkshopTime and LevelMarketTime already exist
        // above; these complete the set the formulas expect.
        public int UpgradeWorkshopTime;
        public int LevelWorkshopQueue;
        public int UpgradeWorkshopQueue;
        public int UpgradeMarketTime;
        public int LevelMarketListings;
        public int UpgradeMarketQueue;
        public int LevelShelter;
        public int UpgradeShelter;
        public int LevelShelterAutofeed;

        // --- Doctrine progression (Java: warLevel/warProgress, ruinLevel/ruinProgress, ...) ---
        // Quest rewards feed these, and they feed the combat stat bonuses.
        public int AfflictionLevel;
        public int AfflictionProgress;
        public int ControlLevel;
        public int ControlProgress;
        public int FortitudeLevel;
        public int FortitudeProgress;
        public int GraceLevel;
        public int GraceProgress;
        public int IllusionLevel;
        public int IllusionProgress;
        public int KnowledgeLevel;
        public int KnowledgeProgress;
        public int RuinLevel;
        public int RuinProgress;
        public int WarLevel;
        public int WarProgress;
        public bool DoctrineMaxed;

        // --- Merchant stock (Java: merchantRegularStockItems, merchantSpecialReserve) ---
        // Buying removes the offer from one of these two lists.
        public List<MerchantOfferSaveData> MerchantRegularStockItems = new List<MerchantOfferSaveData>();
        public List<MerchantOfferSaveData> MerchantSpecialReserve = new List<MerchantOfferSaveData>();
        public List<string> UniqueItemsLost = new List<string>();
        public bool NewMerchantRegularItems;
        public bool NewMerchantSpecialItems;

        // --- Quest bookkeeping ---
        public bool QuestsSeen;
        public bool QuestsRefreshed;
        public int QuestsCompleted;

        // --- Settings (Java: setting* / settingsLanguage) ---
        public bool SettingAutoOpenDungeonDetail;
        public bool SettingColorblindMode;
        public bool SettingConfirmRetreat;
        public bool SettingConfirmSwap;
        public bool SettingConfirmUpgrade;
        public bool SettingCraftMaxAmount;
        public bool SettingSellMaxAmount;
        public bool SettingVerboseLogs;
        public string SettingsLanguage = string.Empty;
        public bool SettingsNotifications;
        public bool SettingsCloud;
        public bool SettingsSound;
        public bool SettingsMusic;
        public bool SettingsVibration;

        // --- Statistics ---
        public long ItemsCrafted;
        public long ItemsSold;
        public long MaxWealth;
        public int MaxAdventurerTier;
        public int MaxAdventurersOwned;

        /// <summary>
        /// The purchase flags in the shape the formula service expects. Several capacity and
        /// price formulas read these directly in the original.
        /// </summary>
        public GuildMaster.Runtime.Formulas.PurchaseFlags GetPurchaseFlags()
        {
            return new GuildMaster.Runtime.Formulas.PurchaseFlags
            {
                StarterPack = StarterPackPurchased,
                AdventurerPack = AdventurerPackPurchased,
                MerchantPack = MerchantPackPurchased,
                ImperialVanguard = ImperialVanguardPurchased,
                UnholyCrusade = UnholyCrusadePurchased
            };
        }

        /// <summary>
        /// Repairs a deserialized instance in place.
        ///
        /// JsonUtility leaves any field absent from the JSON at its C# default, which turns
        /// every list added after a save was written into null. Calling this right after load
        /// keeps older saves working without touching the values they do carry.
        /// </summary>
        public void NormalizeAfterLoad()
        {
            if (Metadata == null) Metadata = new SaveMetadata();

            if (WorkshopQueue == null) WorkshopQueue = new List<ItemActionSaveData>();
            if (CompletedWorkshopItems == null) CompletedWorkshopItems = new List<ItemActionSaveData>();
            if (MarketListings == null) MarketListings = new List<ItemActionSaveData>();
            if (SoldMarketItems == null) SoldMarketItems = new List<ItemActionSaveData>();

            if (Items == null) Items = new List<ItemSaveData>();
            if (Characters == null) Characters = new List<CharacterSaveData>();
            if (Quests == null) Quests = new List<QuestSaveData>();
            if (Dungeons == null) Dungeons = new List<DungeonSaveData>();
            if (Skills == null) Skills = new List<SkillSaveData>();

            if (TavernGuests == null) TavernGuests = new List<CharacterSaveData>();
            if (MerchantRegularStockItems == null) MerchantRegularStockItems = new List<MerchantOfferSaveData>();
            if (MerchantSpecialReserve == null) MerchantSpecialReserve = new List<MerchantOfferSaveData>();
            if (UniqueItemsLost == null) UniqueItemsLost = new List<string>();

            if (SettingsLanguage == null) SettingsLanguage = string.Empty;

            foreach (CharacterSaveData c in Characters) NormalizeCharacter(c);
            foreach (CharacterSaveData c in TavernGuests) NormalizeCharacter(c);
        }

        private static void NormalizeCharacter(CharacterSaveData character)
        {
            if (character == null) return;
            if (character.PositiveStatusEffects == null)
                character.PositiveStatusEffects = new List<StatusEffectSaveData>();
            if (character.NegativeStatusEffects == null)
                character.NegativeStatusEffects = new List<StatusEffectSaveData>();
            if (character.PotionsDrank == null || character.PotionsDrank.Length != 6)
                character.PotionsDrank = new int[6];
            if (character.Trait == null)
                character.Trait = string.Empty;
        }
    }
}
