using NUnit.Framework;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Save;
using System.Collections.Generic;

namespace GuildMaster.Tests.EditMode
{
    /// <summary>
    /// Stage 1 foundation tests. Expected values are computed from the decompiled originals
    /// (see Reports/S6_5A), not from this implementation, so a regression here means the port
    /// drifted away from the decode rather than "the test needs updating".
    /// </summary>
    public class S6_5A_Stage1_FoundationTests
    {
        private FormulaService _formulas;

        [SetUp]
        public void SetUp()
        {
            _formulas = new FormulaService();
        }

        // --- DecodeMath.Round --------------------------------------------------------------
        // Utils.round(d) = (int)(d + 0.0001) — truncation with an epsilon, NOT rounding.
        // These cases are exactly where Math.Round / Mathf.RoundToInt would disagree.

        [Test]
        public void Round_TruncatesRatherThanRounding()
        {
            Assert.AreEqual(2, DecodeMath.Round(2.5d), "2.5 must truncate to 2, not round to 2 or 3 by parity");
            Assert.AreEqual(3, DecodeMath.Round(3.5d), "3.5 must truncate to 3 — Math.Round would give 4");
            Assert.AreEqual(2, DecodeMath.Round(2.7d), "2.7 must truncate to 2 — Math.Round would give 3");
            Assert.AreEqual(2, DecodeMath.Round(2.9998d), "still below the epsilon reach, so it stays 2");
            // 2.9999 + 0.0001 lands exactly on 3.0, so the epsilon carries it over.
            Assert.AreEqual(3, DecodeMath.Round(2.9999d));
        }

        [Test]
        public void Round_EpsilonRescuesFloatingPointUndershoot()
        {
            // The epsilon exists so values that should be whole but land just below survive.
            Assert.AreEqual(1, DecodeMath.Round(0.9999d));
            Assert.AreEqual(5, DecodeMath.Round(4.99995d));
        }

        [Test]
        public void Round_HandlesZeroAndExactIntegers()
        {
            Assert.AreEqual(0, DecodeMath.Round(0d));
            Assert.AreEqual(7, DecodeMath.Round(7d));
        }

        // --- DecodeMath.TruncatePrice ------------------------------------------------------
        // if (j <= 10000) return j;
        // mod = (j <= 1000000) ? j % 100 : j % 10000;  return j - mod;

        [Test]
        public void TruncatePrice_LeavesSmallPricesUntouched()
        {
            Assert.AreEqual(0L, DecodeMath.TruncatePrice(0L));
            Assert.AreEqual(275L, DecodeMath.TruncatePrice(275L));
            Assert.AreEqual(9999L, DecodeMath.TruncatePrice(9999L));
            Assert.AreEqual(10000L, DecodeMath.TruncatePrice(10000L), "boundary is inclusive: <= 10000 is returned as-is");
        }

        [Test]
        public void TruncatePrice_DropsLastTwoDigitsInMidRange()
        {
            Assert.AreEqual(10000L, DecodeMath.TruncatePrice(10099L));
            Assert.AreEqual(12300L, DecodeMath.TruncatePrice(12345L));
            Assert.AreEqual(1000000L, DecodeMath.TruncatePrice(1000000L));
        }

        [Test]
        public void TruncatePrice_DropsLastFourDigitsAboveOneMillion()
        {
            Assert.AreEqual(1230000L, DecodeMath.TruncatePrice(1234567L));
            Assert.AreEqual(9990000L, DecodeMath.TruncatePrice(9999999L));
        }

        // --- DecodeMath.RollFromWeightedMap ------------------------------------------------
        // Weights are per-mille on a fixed 1000 scale; a table summing under 1000 keeps a
        // deliberate "nothing dropped" gap.

        [Test]
        public void RollFromWeightedMap_SelectsByCumulativePerMilleWeight()
        {
            var table = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("common", 700),
                new KeyValuePair<string, int>("rare", 200),
                new KeyValuePair<string, int>("epic", 100)
            };

            Assert.AreEqual("common", DecodeMath.RollFromWeightedMap(table, 0.0d));
            Assert.AreEqual("common", DecodeMath.RollFromWeightedMap(table, 0.699d));
            Assert.AreEqual("rare", DecodeMath.RollFromWeightedMap(table, 0.70d), "700 is the exclusive upper bound of the first bucket");
            Assert.AreEqual("rare", DecodeMath.RollFromWeightedMap(table, 0.899d));
            Assert.AreEqual("epic", DecodeMath.RollFromWeightedMap(table, 0.90d));
            Assert.AreEqual("epic", DecodeMath.RollFromWeightedMap(table, 0.999d));
        }

        [Test]
        public void RollFromWeightedMap_ReturnsDefaultWhenWeightsUnderfillTheScale()
        {
            // Total weight 300/1000 — the remaining 70% is the "no drop" gap.
            var table = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("bone", 300)
            };

            Assert.AreEqual("bone", DecodeMath.RollFromWeightedMap(table, 0.0d));
            Assert.AreEqual("bone", DecodeMath.RollFromWeightedMap(table, 0.299d));
            Assert.IsNull(DecodeMath.RollFromWeightedMap(table, 0.30d), "past the filled range the roll yields nothing");
            Assert.IsNull(DecodeMath.RollFromWeightedMap(table, 0.99d));
        }

        [Test]
        public void RollFromWeightedMap_HandlesNullAndEmpty()
        {
            Assert.IsNull(DecodeMath.RollFromWeightedMap<string>(null, 0.5d));
            Assert.IsNull(DecodeMath.RollFromWeightedMap(new List<KeyValuePair<string, int>>(), 0.5d));
        }

        // --- Formulas ---------------------------------------------------------------------

        [Test]
        public void F01_TotalStarsToNextLp()
        {
            Assert.AreEqual(4, _formulas.TotalStarsToNextLp(0));
            Assert.AreEqual(7, _formulas.TotalStarsToNextLp(1));
            Assert.AreEqual(34, _formulas.TotalStarsToNextLp(10));
        }

        [Test]
        public void F02_QuartersPrice_MatchesLookupTable()
        {
            Assert.AreEqual(5L, _formulas.GetQuartersPrice(0));
            Assert.AreEqual(275L, _formulas.GetQuartersPrice(1));
            Assert.AreEqual(2000L, _formulas.GetQuartersPrice(2));
            // Level 3 is 10000 in the decode; the decompiler showed it as MIN_BACKOFF_MILLIS.
            Assert.AreEqual(10000L, _formulas.GetQuartersPrice(3));
            // Level 7 is 300000 (MIN_PERIODIC_FLEX_MILLIS), not some other work-manager value.
            Assert.AreEqual(300000L, _formulas.GetQuartersPrice(7));
            Assert.AreEqual(10000000L, _formulas.GetQuartersPrice(22));
        }

        [Test]
        public void F02_QuartersPrice_BeyondTableIsUnaffordable()
        {
            Assert.AreEqual(99999999999999L - (99999999999999L % 10000L), _formulas.GetQuartersPrice(23));
        }

        [Test]
        public void F03_TavernCapacityPrice()
        {
            Assert.AreEqual(5000L, _formulas.GetTavernCapacityPrice(0));
            Assert.AreEqual(15000L, _formulas.GetTavernCapacityPrice(1));
            Assert.AreEqual(45000L, _formulas.GetTavernCapacityPrice(2));
        }

        [Test]
        public void F04_TavernTimePrice()
        {
            Assert.AreEqual(200L, _formulas.GetTavernTimePrice(0));
            Assert.AreEqual(340L, _formulas.GetTavernTimePrice(1));
        }

        [Test]
        public void F05_StorageCapacityPrice_IsCumulativeAndNotTruncated()
        {
            // level 0 -> next = 1 -> min(1,10) * 50 = 50
            Assert.AreEqual(50L, _formulas.GetStorageCapacityPrice(0));
            // level 9 -> next = 10 -> min(10,10) * 50 = 500
            Assert.AreEqual(500L, _formulas.GetStorageCapacityPrice(9));
            // level 10 -> next = 11 -> 150*min(1,10) + 50*min(11,10) = 150 + 500 = 650
            Assert.AreEqual(650L, _formulas.GetStorageCapacityPrice(10));
        }

        [Test]
        public void F05_StorageCapacityPrice_StopsAtEighty()
        {
            Assert.AreEqual(99999999999999L, _formulas.GetStorageCapacityPrice(80),
                "next = 81 exceeds the cap and returns the raw unaffordable price without truncation");
        }

        [Test]
        public void F06_F08_MarketAndWorkshopQueuePricesShareShape()
        {
            Assert.AreEqual(20L, _formulas.GetMarketListingsPrice(0));
            Assert.AreEqual(90L, _formulas.GetMarketListingsPrice(1));
            Assert.AreEqual(_formulas.GetMarketListingsPrice(3), _formulas.GetWorkshopQueuePrice(3));
        }

        [Test]
        public void F07_F09_MarketAndWorkshopTimePricesShareShape()
        {
            Assert.AreEqual(10L, _formulas.GetMarketTimePrice(0));
            Assert.AreEqual(17L, _formulas.GetMarketTimePrice(1));
            Assert.AreEqual(_formulas.GetMarketTimePrice(5), _formulas.GetWorkshopTimePrice(5));
        }

        [Test]
        public void F10_ShelterPrice()
        {
            Assert.AreEqual(500L, _formulas.GetShelterPrice(0));
            Assert.AreEqual(32000L, _formulas.GetShelterPrice(3));
            Assert.AreEqual(4000000L, _formulas.GetShelterPrice(10));
        }

        [Test]
        public void F11_ShelterAutofeedIsAOneOffPurchase()
        {
            Assert.AreEqual(10000L, _formulas.GetShelterAutofeedPrice(0));
            Assert.AreEqual(99999999999999L - (99999999999999L % 10000L), _formulas.GetShelterAutofeedPrice(1));
        }

        [Test]
        public void F12_QuartersCapacity_AppliesPurchaseBonuses()
        {
            Assert.AreEqual(2, _formulas.GetQuartersCapacity(0, 0, PurchaseFlags.None));
            Assert.AreEqual(7, _formulas.GetQuartersCapacity(3, 2, PurchaseFlags.None));

            var all = new PurchaseFlags
            {
                StarterPack = true, AdventurerPack = true,
                ImperialVanguard = true, UnholyCrusade = true
            };
            // base 2 + starter 1 + adventurer 2 + vanguard 4 + crusade 4 = 13
            Assert.AreEqual(13, _formulas.GetQuartersCapacity(0, 0, all));
        }

        [Test]
        public void F13_TavernVisitorInterval_IsMillisecondsAndShrinksPerLevel()
        {
            Assert.AreEqual(28800L * 1000L, _formulas.GetTavernVisitorInterval(0, 0),
                "base interval is 8 hours expressed in milliseconds");

            long oneLevel = _formulas.GetTavernVisitorInterval(1, 0);
            Assert.AreEqual((long)(0.9d * 28800.0d * 1000.0d), oneLevel);

            Assert.AreEqual(oneLevel, _formulas.GetTavernVisitorInterval(0, 1),
                "level and upgrade are summed, so either one has the same effect");
        }

        [Test]
        public void F14_TavernCapacity()
        {
            Assert.AreEqual(1, _formulas.GetTavernCapacity(0, 0, PurchaseFlags.None));
            var packs = new PurchaseFlags { StarterPack = true, AdventurerPack = true };
            // base 1 + starter 1 + adventurer 2 = 4
            Assert.AreEqual(4, _formulas.GetTavernCapacity(0, 0, packs));
        }

        [Test]
        public void F15_F16_MarketAndWorkshopSlots()
        {
            Assert.AreEqual(1, _formulas.MarketListings(0, 0, PurchaseFlags.None));
            Assert.AreEqual(1, _formulas.WorkshopQueue(0, 0, PurchaseFlags.None));

            var merchant = new PurchaseFlags { StarterPack = true, MerchantPack = true };
            Assert.AreEqual(4, _formulas.MarketListings(0, 0, merchant));
            Assert.AreEqual(4, _formulas.WorkshopQueue(0, 0, merchant));
        }

        [Test]
        public void F17_StorageSpaces_UsesLargePackBonuses()
        {
            Assert.AreEqual(35, _formulas.StorageSpaces(0, 0, PurchaseFlags.None));

            var all = new PurchaseFlags { StarterPack = true, AdventurerPack = true, MerchantPack = true };
            // base 35 + starter 35 + adventurer 35 + merchant 70 = 175
            Assert.AreEqual(175, _formulas.StorageSpaces(0, 0, all));
        }

        [Test]
        public void F18_ShelterCapacityHasNoPurchaseBonus()
        {
            Assert.AreEqual(2, _formulas.ShelterCapacity(0, 0));
            Assert.AreEqual(7, _formulas.ShelterCapacity(3, 2));
        }

        [Test]
        public void F19_ExperienceToNextLevel_StepsDownByMagnitude()
        {
            // level 1: pow(1,1.4)=1 -> (3+1)*10*1 = 40 -> below 100, unchanged
            Assert.AreEqual(40, _formulas.ExperienceToNextLevel(1, false));
            // adventurers pay double
            Assert.AreEqual(80, _formulas.ExperienceToNextLevel(1, true));
        }

        [Test]
        public void F19_ExperienceToNextLevel_FlooringBuckets()
        {
            for (int level = 1; level <= 40; level++)
            {
                int value = _formulas.ExperienceToNextLevel(level, true);
                if (value >= 10000) Assert.AreEqual(0, value % 1000, $"level {level} should floor to 1000s");
                else if (value >= 1000) Assert.AreEqual(0, value % 100, $"level {level} should floor to 100s");
                else if (value >= 100) Assert.AreEqual(0, value % 10, $"level {level} should floor to 10s");
            }
        }

        [Test]
        public void F20_FoodToNextLevel()
        {
            Assert.AreEqual(30, _formulas.FoodToNextLevel(0));
            Assert.AreEqual(32, _formulas.FoodToNextLevel(1));
        }

        // --- SaveData ----------------------------------------------------------------------

        [Test]
        public void SaveData_NormalizeAfterLoad_ReplacesNullCollections()
        {
            var data = new SaveData
            {
                Items = null, Characters = null, Quests = null, Dungeons = null, Skills = null,
                WorkshopQueue = null, CompletedWorkshopItems = null,
                MarketListings = null, SoldMarketItems = null,
                TavernGuests = null, MerchantRegularStockItems = null,
                MerchantSpecialReserve = null, UniqueItemsLost = null,
                SettingsLanguage = null, Metadata = null
            };

            data.NormalizeAfterLoad();

            Assert.IsNotNull(data.Metadata);
            Assert.IsNotNull(data.Items);
            Assert.IsNotNull(data.Characters);
            Assert.IsNotNull(data.Quests);
            Assert.IsNotNull(data.Dungeons);
            Assert.IsNotNull(data.Skills);
            Assert.IsNotNull(data.WorkshopQueue);
            Assert.IsNotNull(data.CompletedWorkshopItems);
            Assert.IsNotNull(data.MarketListings);
            Assert.IsNotNull(data.SoldMarketItems);
            Assert.IsNotNull(data.TavernGuests);
            Assert.IsNotNull(data.MerchantRegularStockItems);
            Assert.IsNotNull(data.MerchantSpecialReserve);
            Assert.IsNotNull(data.UniqueItemsLost);
            Assert.IsNotNull(data.SettingsLanguage);
        }

        [Test]
        public void SaveData_NormalizeAfterLoad_PreservesExistingValues()
        {
            var data = new SaveData { Money = 1234L, Gems = 56L };
            data.Items.Add(new ItemSaveData { DefinitionId = "sword", InstanceId = "ITM-1", StackCount = 3 });
            data.Characters.Add(new CharacterSaveData { DefinitionId = "ADV_NOVICE", InstanceId = "CHR-1", Level = 4 });

            data.NormalizeAfterLoad();

            Assert.AreEqual(1234L, data.Money, "normalising must not touch currency");
            Assert.AreEqual(56L, data.Gems);
            Assert.AreEqual(1, data.Items.Count);
            Assert.AreEqual("sword", data.Items[0].DefinitionId);
            Assert.AreEqual(1, data.Characters.Count);
            Assert.AreEqual(4, data.Characters[0].Level);
        }

        [Test]
        public void SaveData_NormalizeAfterLoad_FixesCharacterStatusEffectLists()
        {
            var data = new SaveData();
            data.Characters.Add(new CharacterSaveData
            {
                DefinitionId = "ADV_NOVICE",
                PositiveStatusEffects = null,
                NegativeStatusEffects = null
            });
            data.TavernGuests.Add(new CharacterSaveData
            {
                DefinitionId = "Footman",
                PositiveStatusEffects = null,
                NegativeStatusEffects = null
            });

            data.NormalizeAfterLoad();

            Assert.IsNotNull(data.Characters[0].PositiveStatusEffects);
            Assert.IsNotNull(data.Characters[0].NegativeStatusEffects);
            Assert.IsNotNull(data.TavernGuests[0].PositiveStatusEffects);
            Assert.IsNotNull(data.TavernGuests[0].NegativeStatusEffects);
        }

        [Test]
        public void SaveData_NewFieldsDefaultToFreshGameValues()
        {
            var data = new SaveData();

            Assert.IsFalse(data.StarterPackPurchased, "no purchases by default");
            Assert.IsFalse(data.MerchantPackPurchased);
            Assert.AreEqual(0L, data.LastAccess, "0 means never played; offline progress treats it specially");
            Assert.AreEqual(0L, data.NextTavernVisit);
            Assert.IsFalse(data.TavernLocked);
            Assert.AreEqual(0, data.TutorialStep);
            Assert.AreEqual(0, data.WarLevel);
            Assert.AreEqual(0, data.QuestsCompleted);
        }

        [Test]
        public void SaveData_GetPurchaseFlags_MirrorsStoredFlags()
        {
            var data = new SaveData
            {
                StarterPackPurchased = true,
                MerchantPackPurchased = true,
                UnholyCrusadePurchased = true
            };

            PurchaseFlags flags = data.GetPurchaseFlags();

            Assert.IsTrue(flags.StarterPack);
            Assert.IsTrue(flags.MerchantPack);
            Assert.IsTrue(flags.UnholyCrusade);
            Assert.IsFalse(flags.AdventurerPack);
            Assert.IsFalse(flags.ImperialVanguard);
        }

        [Test]
        public void SaveData_SerializationRoundTripKeepsNewFields()
        {
            var original = new SaveData
            {
                Money = 999L,
                Gems = 42L,
                LastAccess = 1700000000L,
                NextTavernVisit = 12345L,
                TutorialStep = 7,
                LevelQuarters = 3,
                UpgradeTavernCapacity = 2,
                WarLevel = 5,
                WarProgress = 11,
                SettingVerboseLogs = true,
                StarterPackPurchased = true
            };
            original.TavernGuests.Add(new CharacterSaveData { DefinitionId = "Footman", InstanceId = "CHR-9", Level = 1 });
            original.MerchantRegularStockItems.Add(new MerchantOfferSaveData
            {
                DefinitionId = "PotionOfAgility", StackCount = 1, Price = 250L, IsGems = false
            });

            string json = UnityEngine.JsonUtility.ToJson(original);
            var restored = UnityEngine.JsonUtility.FromJson<SaveData>(json);
            restored.NormalizeAfterLoad();

            Assert.AreEqual(999L, restored.Money);
            Assert.AreEqual(42L, restored.Gems);
            Assert.AreEqual(1700000000L, restored.LastAccess);
            Assert.AreEqual(12345L, restored.NextTavernVisit);
            Assert.AreEqual(7, restored.TutorialStep);
            Assert.AreEqual(3, restored.LevelQuarters);
            Assert.AreEqual(2, restored.UpgradeTavernCapacity);
            Assert.AreEqual(5, restored.WarLevel);
            Assert.AreEqual(11, restored.WarProgress);
            Assert.IsTrue(restored.SettingVerboseLogs);
            Assert.IsTrue(restored.StarterPackPurchased);

            Assert.AreEqual(1, restored.TavernGuests.Count);
            Assert.AreEqual("Footman", restored.TavernGuests[0].DefinitionId);

            Assert.AreEqual(1, restored.MerchantRegularStockItems.Count);
            Assert.AreEqual(250L, restored.MerchantRegularStockItems[0].Price);
            Assert.IsFalse(restored.MerchantRegularStockItems[0].IsGems,
                "the currency flag has to survive the round trip — it decides money vs gems on purchase");
        }

        [Test]
        public void SaveData_OldSaveWithoutNewFieldsStillLoads()
        {
            // A save written before Stage 1: only the fields that existed back then.
            const string legacyJson = @"{
                ""Metadata"":{""SaveVersion"":1,""SaveTimeUnix"":1700000000,""GameVersion"":""1.0"",""DataVersion"":""""},
                ""LevelStorage"":4,
                ""UpgradeStorage"":1,
                ""Money"":5000,
                ""Gems"":25,
                ""LevelWorkshopTime"":2,
                ""LevelMarketTime"":1,
                ""Items"":[{""DefinitionId"":""sword_01"",""InstanceId"":""ITM-1"",""StackCount"":2,""IsLocked"":false}],
                ""Characters"":[{""DefinitionId"":""ADV_NOVICE"",""InstanceId"":""CHR-1"",""Level"":3,""Exp"":10,""CurrentHp"":40}]
            }";

            var restored = UnityEngine.JsonUtility.FromJson<SaveData>(legacyJson);
            restored.NormalizeAfterLoad();

            // Nothing that existed before may be lost.
            Assert.AreEqual(5000L, restored.Money);
            Assert.AreEqual(25L, restored.Gems);
            Assert.AreEqual(4, restored.LevelStorage);
            Assert.AreEqual(1, restored.UpgradeStorage);
            Assert.AreEqual(1, restored.Items.Count);
            Assert.AreEqual("sword_01", restored.Items[0].DefinitionId);
            Assert.AreEqual(1, restored.Characters.Count);
            Assert.AreEqual(3, restored.Characters[0].Level);

            // New fields come back as fresh-game defaults rather than null.
            Assert.IsNotNull(restored.TavernGuests);
            Assert.AreEqual(0, restored.TavernGuests.Count);
            Assert.IsNotNull(restored.MerchantRegularStockItems);
            Assert.IsNotNull(restored.UniqueItemsLost);
            Assert.AreEqual(0L, restored.LastAccess);
            Assert.IsFalse(restored.StarterPackPurchased);
        }

        [Test]
        public void InventoryCapacity_FormulaMatchesStorageSpaces()
        {
            var data = new SaveData { LevelStorage = 6, UpgradeStorage = 2 };

            int expected = _formulas.StorageSpaces(6, 2, data.GetPurchaseFlags());

            Assert.AreEqual(35 + 6 + 2, expected, "no packs owned, so no bonus applies");
        }
    }
}
