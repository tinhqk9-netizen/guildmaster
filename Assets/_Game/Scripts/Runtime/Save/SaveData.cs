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
        public bool IsHpInitialized;
        
        public string WeaponInstanceId;
        public string ArmorInstanceId;
        public string AccessoryInstanceId;

        // Java: Adventurer.ascended (real field, drives isAscended()/canPickDoctrine() and a
        // recovered +50% CON/INT/DEX/HP stat bonus — see S6_5A_Stage4_CharacterTests). Set true
        // only by PromotionService.Ascend() (reaching the final class tier at MaxLevel resets the
        // hero to its base class per Utils.getBaseClass()) or by the "Intercession" item shortcut
        // (DialogConsumeIntercession.java) — the latter is a Phase 4 (Tavern/Shop item) concern,
        // not implemented here.
        public bool IsAscended;

        // Phase 1: AscensionLevel (removed) was a fabricated "promotion tier counter" that never
        // matched Java — Java has no PromotionDefinition/tier data table at all; promotion is
        // driven purely by AdventurerDefinition.NextClasses + MaxLevel, and DefinitionId now
        // actually changes on promotion (see PromotionService.cs). Old saves may still contain an
        // "AscensionLevel" JSON key; JsonUtility silently ignores fields with no matching member,
        // so old saves load cleanly with no explicit migration step needed for this removal.

        // Phase 0: Java's Adventurer has two independent trait slots — traitCommon (rolled via
        // TavernService.RollCommonTrait()) and traitRare (RollRareTrait()) — an adventurer can
        // hold both at once. The single Trait field below collapsed them into one, which is
        // structurally impossible to represent both traits with. TraitCommon/TraitRare restore
        // the real shape; Trait is kept and migrated in NormalizeAfterLoad() so older saves and
        // any other existing reader of Trait keep working unchanged.
        public string TraitCommon = string.Empty;
        public string TraitRare = string.Empty;
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
        public int Rarity;
        public long TargetProgress;
        // Legacy selection context. A quest definition may be present in both the general
        // and doctrine pools, so this must round-trip independently of DefinitionId.
        public string RewardPoolType;
        public string RewardDoctrineId;
    }

    [Serializable]
    public class DungeonSaveData
    {
        public string DefinitionId;
        public string InstanceId;
        public DungeonState State;
        public int ClearCount;
        public int MaxProgress;
        public float BestTimeSeconds;
    }

    [Serializable]
    public class RaidPartyMemberSaveData
    {
        public string InstanceId;
        public int CurrentHp;
        public int CurrentMana;
        public int CurrentShield;
    }

    /// <summary>
    /// Persisted Legacy raid state. Older saves simply deserialize this as null and continue
    /// with no active raid, so the schema addition is backward compatible.
    /// </summary>
    [Serializable]
    public class RaidSaveData
    {
        public string DefinitionId;
        public int RoomIndex;
        public int LegacyProgress;
        public string EventKey;
        public int EventProgress;
        public string EventOutcome;
        public bool IsComplete;
        public bool IsFailed;
        public List<RaidPartyMemberSaveData> Party = new List<RaidPartyMemberSaveData>();
        public List<EnemySaveData> Enemies = new List<EnemySaveData>();
        public List<ItemSaveData> PendingRewards = new List<ItemSaveData>();
        public List<string> Log = new List<string>();
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
    public class PetSaveData
    {
        public string DefinitionId;
        public string InstanceId;
        public int Level;
        public long Exp;
        public int Food;
        public bool Favourite;
        public string Ability1;
        public string Ability2;
        public string Ability3;
        public string Ability4;
        public string AssignedDungeonId;
        // Legacy-incompatible compatibility field retained for old saves; PetService no longer
        // uses hero equipment semantics.
        public string EquippedToCharacterId;
    }

    /// <summary>
    /// Phase 0: per-node doctrine progression. Java's Doctrine has 6 independent nodes (l1..l6),
    /// but SaveData previously stored only one summed Level/Progress pair per doctrine (see
    /// AfflictionLevel/WarLevel/... below, left untouched — DoctrineService.cs, which reads
    /// them, is gameplay logic and out of scope for Phase 0). JsonUtility can't serialize
    /// Dictionary, so this uses the same id-keyed list pattern as MerchantOfferSaveData.
    /// Nothing currently reads/writes this list — it's a save-data slot only, per
    /// Docs/Backend_Audit/phase0_schema_mapping.md §10.
    /// </summary>
    [Serializable]
    public class DoctrineNodeSaveData
    {
        public string DoctrineId; // "war", "affliction", ...
        public string NodeId;     // "l1".."l6"
        public int Level;
    }

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
        public List<PetSaveData> Pets = new List<PetSaveData>();
        
        public List<string> CurrentParty = new List<string>();
        public List<List<string>> ExpeditionParties = new List<List<string>>();

        public ActiveDungeonSaveData ActiveDungeon = null;
        public List<ExpeditionSaveData> ActiveExpeditions = new List<ExpeditionSaveData>();
        public RaidSaveData ActiveRaid = null;

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

        // Phase 0: per-node progression slot — see DoctrineNodeSaveData above.
        public List<DoctrineNodeSaveData> DoctrineNodes = new List<DoctrineNodeSaveData>();

        // Phase 0: Bestiary "seen enemy" tracking. No discovery logic is wired up yet — this is
        // just the save-data slot for a later phase, per phase0_schema_mapping.md §10.
        public List<string> SeenEnemyIds = new List<string>();

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
        public static SaveData CreateDefault()
        {
            var data = new SaveData();
            // Production fresh-save currency. The first Footman and Tavern visitor are
            // created by NewPlayerStateInitializer after the service graph is ready.
            data.Money = 100;
            data.LevelStorage = 1;
            data.LevelQuarters = 1;
            data.SettingsSound = true;
            data.SettingsMusic = true;

            data.NormalizeAfterLoad();

            // Fresh save starts with no characters or visitors until the production
            // initialization pipeline creates the Footman and first Tavern visitor.
            data.NextTavernVisit = 0;

            return data;
        }

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
            if (Pets == null) Pets = new List<PetSaveData>();
            if (MerchantRegularStockItems == null) MerchantRegularStockItems = new List<MerchantOfferSaveData>();
            if (MerchantSpecialReserve == null) MerchantSpecialReserve = new List<MerchantOfferSaveData>();
            if (UniqueItemsLost == null) UniqueItemsLost = new List<string>();
            if (CurrentParty == null) CurrentParty = new List<string>();

            if (DoctrineNodes == null) DoctrineNodes = new List<DoctrineNodeSaveData>();
            if (SeenEnemyIds == null) SeenEnemyIds = new List<string>();

            // Multi-Party migration: migrate CurrentParty → ExpeditionParties[0]
            if (ExpeditionParties == null) ExpeditionParties = new List<List<string>>();
            while (ExpeditionParties.Count < 3) ExpeditionParties.Add(new List<string>());
            if (CurrentParty.Count > 0 && ExpeditionParties[0].Count == 0)
            {
                ExpeditionParties[0].AddRange(CurrentParty);
            }

            if (SettingsLanguage == null) SettingsLanguage = string.Empty;

            if (ActiveExpeditions == null) ActiveExpeditions = new List<ExpeditionSaveData>();
            if (ActiveDungeon != null && string.IsNullOrEmpty(ActiveDungeon.DungeonDefinitionId))
                ActiveDungeon = null;
            if (ActiveRaid != null && string.IsNullOrEmpty(ActiveRaid.DefinitionId))
                ActiveRaid = null;
            if (ActiveRaid != null)
            {
                if (ActiveRaid.Party == null) ActiveRaid.Party = new List<RaidPartyMemberSaveData>();
                if (ActiveRaid.Enemies == null) ActiveRaid.Enemies = new List<EnemySaveData>();
                if (ActiveRaid.PendingRewards == null) ActiveRaid.PendingRewards = new List<ItemSaveData>();
                if (ActiveRaid.Log == null) ActiveRaid.Log = new List<string>();
            }
            if (ActiveDungeon != null &&
                !string.IsNullOrEmpty(ActiveDungeon.DungeonDefinitionId) &&
                ActiveExpeditions.Count == 0)
            {
                ActiveExpeditions.Add(new ExpeditionSaveData
                {
                    SlotIndex = 0,
                    Dungeon = ActiveDungeon
                });
            }

            foreach (CharacterSaveData c in Characters) NormalizeCharacter(c);
            foreach (PetSaveData p in Pets) NormalizePet(p);
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
            if (character.TraitCommon == null)
                character.TraitCommon = string.Empty;
            if (character.TraitRare == null)
                character.TraitRare = string.Empty;

            // Phase 0 migration: older saves only have the single Trait field. Route it into
            // whichever slot it actually belongs to (Trait was only ever assigned one of
            // TavernService's RollCommonTrait()/RollRareTrait() outputs, never both at once,
            // so this is a lossless one-time classification, not a guess).
            if (string.IsNullOrEmpty(character.TraitCommon) && string.IsNullOrEmpty(character.TraitRare) &&
                !string.IsNullOrEmpty(character.Trait))
            {
                if (IsKnownCommonTrait(character.Trait))
                    character.TraitCommon = character.Trait;
                else
                    character.TraitRare = character.Trait;
            }
        }

        private static readonly HashSet<string> KnownCommonTraits = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BOOKWORM", "BRUTE", "FERAL", "BOOKWORM_PLUS", "BRUTE_PLUS", "FERAL_PLUS"
        };

        private static bool IsKnownCommonTrait(string trait) => KnownCommonTraits.Contains(trait);

        private static void NormalizePet(PetSaveData pet)
        {
            if (pet == null) return;
            if (pet.Level < 1) pet.Level = 1;
            if (string.IsNullOrEmpty(pet.InstanceId))
                pet.InstanceId = System.Guid.NewGuid().ToString();
        }
    }
}
