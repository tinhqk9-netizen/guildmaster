using System;

namespace GuildMaster.Runtime.Formulas
{
    /// <summary>
    /// Port of <c>Formulas.java</c>. Each method notes the source line it came from so the
    /// numbers can be re-checked against the decompiled original.
    ///
    /// Two decode details that are easy to get wrong and are deliberately preserved here:
    ///  * <c>GetStorageCapacityPrice</c> does NOT truncate; every other price does.
    ///  * The decompiler substituted androidx constants for plain literals of the same value
    ///    (MIN_BACKOFF_MILLIS = 10000, MIN_PERIODIC_FLEX_MILLIS = 300000,
    ///    DEFAULT_BACKOFF_DELAY_MILLIS = 30000). The literals are used below.
    ///
    /// Evidence: Reports/S6_5A/S6_5A_001_Rule_Extraction_Report.md
    /// </summary>
    public class FormulaService : IFormulaService
    {
        private const long IMPOSSIBLY_HIGH_PRICE = 99999999999999L;

        private const int BASE_QUARTERS_SPACES = 2;
        private const int BASE_STORAGE_SPACES = 35;
        private const int BASE_TAVERN_SPACES = 1;
        private const int BASE_MARKET_SPACES = 1;
        private const int BASE_WORKSHOP_SPACES = 1;
        private const double BASE_TAVERN_VISITOR_INTERVAL_SECONDS = 28800.0d;

        // --- Progression -----------------------------------------------------------------

        /// <summary>F-01 — Formulas.java:16. <c>(i * 3) + 4</c></summary>
        public int TotalStarsToNextLp(int lpLevel)
        {
            return (lpLevel * 3) + 4;
        }

        /// <summary>
        /// F-19 — Formulas.java:266. Note the trailing step-down: the result is floored to the
        /// nearest 1000 / 100 / 10 depending on magnitude.
        /// </summary>
        public int ExperienceToNextLevel(int currentLevel, bool isAdventurer)
        {
            double p = Math.Pow(currentLevel, 1.4d);
            int value = (int)((3.0d + p) * 10.0d * p);

            if (isAdventurer) value *= 2;

            if (value >= 10000) return (value / 1000) * 1000;
            if (value >= 1000) return (value / 100) * 100;
            return value >= 100 ? (value / 10) * 10 : value;
        }

        /// <summary>F-20 — Formulas.java:281. <c>(int)(pow(1.085, i) * 30.0)</c></summary>
        public int FoodToNextLevel(int currentLevel)
        {
            return (int)(Math.Pow(1.085d, currentLevel) * 30.0d);
        }

        // --- Upgrade prices --------------------------------------------------------------

        /// <summary>F-02 — Formulas.java:20. Lookup table of 23 tiers, then truncated.</summary>
        public long GetQuartersPrice(int levelQuarters)
        {
            long price;
            switch (levelQuarters)
            {
                case 0: price = 5L; break;
                case 1: price = 275L; break;
                case 2: price = 2000L; break;
                case 3: price = 10000L; break;      // WorkRequest.MIN_BACKOFF_MILLIS
                case 4: price = 40000L; break;
                case 5: price = 100000L; break;
                case 6: price = 200000L; break;
                case 7: price = 300000L; break;     // PeriodicWorkRequest.MIN_PERIODIC_FLEX_MILLIS
                case 8: price = 400000L; break;
                case 9: price = 500000L; break;
                case 10: price = 700000L; break;
                case 11: price = 1000000L; break;
                case 12: price = 1400000L; break;
                case 13: price = 1850000L; break;
                case 14: price = 2400000L; break;
                case 15: price = 3000000L; break;
                case 16: price = 4000000L; break;
                case 17: price = 5000000L; break;
                case 18: price = 6000000L; break;
                case 19: price = 7000000L; break;
                case 20: price = 8000000L; break;
                case 21: price = 9000000L; break;
                case 22: price = 10000000L; break;
                default: price = IMPOSSIBLY_HIGH_PRICE; break;
            }
            return DecodeMath.TruncatePrice(price);
        }

        /// <summary>F-03 — Formulas.java:99. <c>truncatePrice(pow(3.0, lvl) * 5000.0)</c></summary>
        public long GetTavernCapacityPrice(int levelTavernCapacity)
        {
            return DecodeMath.TruncatePrice((long)(Math.Pow(3.0d, levelTavernCapacity) * 5000.0d));
        }

        /// <summary>F-04 — Formulas.java:103. <c>truncatePrice(pow(1.7, lvl) * 200.0)</c></summary>
        public long GetTavernTimePrice(int levelTavernTime)
        {
            return DecodeMath.TruncatePrice((long)(Math.Pow(1.7d, levelTavernTime) * 200.0d));
        }

        /// <summary>
        /// F-05 — Formulas.java:107. Cumulative tiered cost.
        /// Unlike every other price method this one does NOT call truncatePrice.
        /// </summary>
        public long GetStorageCapacityPrice(int levelStorage)
        {
            int next = levelStorage + 1;
            if (next > 80) return IMPOSSIBLY_HIGH_PRICE;

            long total = next > 60 ? Math.Min(levelStorage - 59, 20) * 30000L : 0L;
            if (next > 50) total += Math.Min(levelStorage - 49, 10) * 22000L;
            if (next > 40) total += Math.Min(levelStorage - 39, 10) * 12000L;
            if (next > 30) total += Math.Min(levelStorage - 29, 10) * 4000L;
            if (next > 20) total += Math.Min(levelStorage - 19, 10) * 800L;
            if (next > 10) total += Math.Min(levelStorage - 9, 10) * 150L;

            return total + (Math.Min(next, 10) * 50L);
        }

        /// <summary>F-06 — Formulas.java:132. <c>truncatePrice(pow(4.5, lvl) * 20.0)</c></summary>
        public long GetMarketListingsPrice(int levelMarketListings)
        {
            return DecodeMath.TruncatePrice((long)(Math.Pow(4.5d, levelMarketListings) * 20.0d));
        }

        /// <summary>F-07 — Formulas.java:136. <c>truncatePrice(pow(1.7, lvl) * 10.0)</c></summary>
        public long GetMarketTimePrice(int levelMarketTime)
        {
            return DecodeMath.TruncatePrice((long)(Math.Pow(1.7d, levelMarketTime) * 10.0d));
        }

        /// <summary>F-08 — Formulas.java:140. Same shape as the market listings price.</summary>
        public long GetWorkshopQueuePrice(int levelWorkshopQueue)
        {
            return DecodeMath.TruncatePrice((long)(Math.Pow(4.5d, levelWorkshopQueue) * 20.0d));
        }

        /// <summary>F-09 — Formulas.java:144. Same shape as the market time price.</summary>
        public long GetWorkshopTimePrice(int levelWorkshopTime)
        {
            return DecodeMath.TruncatePrice((long)(Math.Pow(1.7d, levelWorkshopTime) * 10.0d));
        }

        /// <summary>F-10 — Formulas.java:148. Lookup table of 11 tiers, then truncated.</summary>
        public long GetShelterPrice(int levelShelter)
        {
            long price;
            switch (levelShelter)
            {
                case 0: price = 500L; break;
                case 1: price = 2000L; break;
                case 2: price = 8000L; break;
                case 3: price = 32000L; break;
                case 4: price = 64000L; break;
                case 5: price = 128000L; break;
                case 6: price = 256000L; break;
                case 7: price = 512000L; break;
                case 8: price = 1000000L; break;
                case 9: price = 2000000L; break;
                case 10: price = 4000000L; break;
                default: price = IMPOSSIBLY_HIGH_PRICE; break;
            }
            return DecodeMath.TruncatePrice(price);
        }

        /// <summary>
        /// F-11 — Formulas.java:191. A one-off purchase: once bought the price becomes
        /// unreachable.
        /// </summary>
        public long GetShelterAutofeedPrice(int levelShelterAutofeed)
        {
            return DecodeMath.TruncatePrice(levelShelterAutofeed > 0 ? IMPOSSIBLY_HIGH_PRICE : 10000L);
        }

        // --- Capacities ------------------------------------------------------------------

        /// <summary>F-12 — Formulas.java:197.</summary>
        public int GetQuartersCapacity(int levelQuarters, int upgradeQuarters, PurchaseFlags flags)
        {
            int bonus = flags.StarterPack ? 1 : 0;
            if (flags.AdventurerPack) bonus += 2;
            if (flags.ImperialVanguard) bonus += 4;
            if (flags.UnholyCrusade) bonus += 4;

            return levelQuarters + BASE_QUARTERS_SPACES + upgradeQuarters + bonus;
        }

        /// <summary>F-14 — Formulas.java:220.</summary>
        public int GetTavernCapacity(int levelTavernCapacity, int upgradeTavernCapacity, PurchaseFlags flags)
        {
            int bonus = flags.StarterPack ? 1 : 0;
            if (flags.AdventurerPack) bonus += 2;

            return levelTavernCapacity + BASE_TAVERN_SPACES + upgradeTavernCapacity + bonus;
        }

        /// <summary>F-15 — Formulas.java:231.</summary>
        public int MarketListings(int levelMarketListings, int upgradeMarketQueue, PurchaseFlags flags)
        {
            int bonus = flags.StarterPack ? 1 : 0;
            if (flags.MerchantPack) bonus += 2;

            return levelMarketListings + BASE_MARKET_SPACES + upgradeMarketQueue + bonus;
        }

        /// <summary>F-16 — Formulas.java:242.</summary>
        public int WorkshopQueue(int levelWorkshopQueue, int upgradeWorkshopQueue, PurchaseFlags flags)
        {
            int bonus = flags.StarterPack ? 1 : 0;
            if (flags.MerchantPack) bonus += 2;

            return levelWorkshopQueue + BASE_WORKSHOP_SPACES + upgradeWorkshopQueue + bonus;
        }

        /// <summary>
        /// F-17 — Formulas.java:251. The pack bonuses here are much larger than the other
        /// capacity formulas (35 / 35 / 70 rather than 1 / 2).
        /// </summary>
        public int StorageSpaces(int levelStorage, int upgradeStorage, PurchaseFlags flags)
        {
            int bonus = flags.StarterPack ? 35 : 0;
            if (flags.AdventurerPack) bonus += 35;
            if (flags.MerchantPack) bonus += 70;

            return levelStorage + BASE_STORAGE_SPACES + upgradeStorage + bonus;
        }

        /// <summary>F-18 — Formulas.java:262. No purchase bonus on this one.</summary>
        public int ShelterCapacity(int levelShelter, int upgradeShelter)
        {
            return levelShelter + upgradeShelter + 2;
        }

        // --- Timing ----------------------------------------------------------------------

        /// <summary>
        /// F-13 — Formulas.java:214. Returns MILLISECONDS: the 28800 second base is multiplied
        /// by 1000. Each level of tavern time shortens the wait by 10%.
        /// </summary>
        public long GetTavernVisitorInterval(int levelTavernTime, int upgradeTavernTime)
        {
            return (long)(Math.Pow(0.9d, levelTavernTime + upgradeTavernTime)
                          * BASE_TAVERN_VISITOR_INTERVAL_SECONDS
                          * 1000.0d);
        }
    }
}
