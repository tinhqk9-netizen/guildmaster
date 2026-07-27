using System;
using UnityEngine;

namespace GuildMaster.Runtime.Formulas
{
    public class FormulaService : IFormulaService
    {
        private const long IMPOSSIBLY_HIGH_PRICE = 99999999999999L;

        public int ExperienceToNextLevel(int currentLevel, bool isAdventurer)
        {
            double dPow = Math.Pow(currentLevel, 1.4d);
            int i2 = (int)((3.0d + dPow) * 10.0d * dPow);
            
            if (isAdventurer)
            {
                i2 *= 2;
            }
            
            if (i2 >= 10000)
            {
                return (i2 / 1000) * 1000;
            }
            if (i2 >= 1000)
            {
                return (i2 / 100) * 100;
            }
            return i2 >= 100 ? (i2 / 10) * 10 : i2;
        }

        public int FoodToNextLevel(int currentLevel)
        {
            return (int)(Math.Pow(1.085d, currentLevel) * 30.0d);
        }

        public long GetQuartersPrice(int level)
        {
            long j;
            switch (level)
            {
                case 0: j = 5; break;
                case 1: j = 275; break;
                case 2: j = 2000; break;
                case 3: j = 10000; break;
                case 4: j = 40000; break;
                case 5: j = 100000; break;
                case 6: j = 200000; break;
                case 7: j = 300000; break;
                case 8: j = 400000; break;
                case 9: j = 500000; break;
                case 10: j = 700000; break;
                case 11: j = 1000000; break;
                case 12: j = 1400000; break;
                case 13: j = 1850000; break;
                case 14: j = 2400000; break;
                case 15: j = 3000000; break;
                case 16: j = 4000000; break;
                case 17: j = 5000000; break;
                case 18: j = 6000000; break;
                case 19: j = 7000000; break;
                case 20: j = 8000000; break;
                case 21: j = 9000000; break;
                case 22: j = 10000000; break;
                default: j = IMPOSSIBLY_HIGH_PRICE; break;
            }
            return TruncatePrice(j);
        }

        public long GetTavernCapacityPrice(int level)
        {
            return TruncatePrice((long)(Math.Pow(3.0d, level) * 5000.0d));
        }

        public long GetStorageCapacityPrice(int level)
        {
            int i = level + 1;
            if (i > 80)
            {
                return IMPOSSIBLY_HIGH_PRICE;
            }
            long jMin = i > 60 ? Math.Min(level - 59, 20) * 10000L : 0L;
            if (i > 50) jMin += Math.Min(level - 49, 10) * 22000L;
            if (i > 40) jMin += Math.Min(level - 39, 10) * 12000L;
            if (i > 30) jMin += Math.Min(level - 29, 10) * 4000L;
            if (i > 20) jMin += Math.Min(level - 19, 10) * 800L;
            if (i > 10) jMin += Math.Min(level - 9, 10) * 150L;
            
            return jMin + (Math.Min(i, 10) * 50L);
        }

        public int GetStorageSpaces(int levelStorage, int upgradeStorage, int additionalBonus = 0)
        {
            // From Formulas.storageSpaces(): MainActivity.data.getLevelStorage() + 35 + MainActivity.data.getUpgradeStorage() + i
            return levelStorage + 35 + upgradeStorage + additionalBonus;
        }

        private long TruncatePrice(long price)
        {
            if (price >= 10000000) return (price / 10000) * 10000;
            if (price >= 100000) return (price / 1000) * 1000;
            if (price >= 10000) return (price / 100) * 100;
            if (price >= 1000) return (price / 10) * 10;
            return price;
        }

        public void CalculateDamage_ManualPortRequired()
        {
            Debug.LogWarning("[FormulaService] CalculateDamage logic relies on Combat context not yet ported. Manual port required in S2/S3.");
            throw new NotImplementedException("manualPortRequired");
        }
    }
}
