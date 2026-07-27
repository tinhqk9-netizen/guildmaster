using System.Collections.Generic;

namespace GuildMaster.Runtime.Formulas
{
    /// <summary>
    /// Arithmetic primitives copied from the decompiled game so ported formulas produce
    /// bit-identical results. These deliberately do NOT use System.Math / UnityEngine.Mathf
    /// rounding, because the original uses its own conventions.
    ///
    /// Evidence: Reports/S6_5A/S6_5A_001E_Rule_Cleanup_Report.md
    ///           Reports/S6_5A/S6_5A_001E_AreaHelpers_DAD.md
    /// </summary>
    public static class DecodeMath
    {
        /// <summary>
        /// Port of <c>Utils.round(double)</c>: <c>(int)(d + 0.0001)</c>.
        ///
        /// This is a truncation with an epsilon nudge, NOT a rounding function — 3.5 becomes 3,
        /// not 4. Using Math.Round (banker's rounding) or Mathf.RoundToInt here would skew
        /// damage, stats, experience and prices across the whole game, because the original
        /// funnels all of those through this one call.
        ///
        /// Evidence: Utils.round smali/DAD — <c>return ((int) (p2 + 4547007122018943789));</c>
        /// where the long bit pattern 4547007122018943789 decodes to the double 0.0001.
        /// </summary>
        public static int Round(double value)
        {
            return (int)(value + 0.0001d);
        }

        /// <summary>
        /// Port of <c>Utils.truncatePrice(long)</c>: drops the low digits of large prices.
        /// Values at or below 10000 are returned untouched.
        ///
        /// Evidence: Utils.java:889 (JADX) and Merchant DAD dump — both agree.
        /// </summary>
        public static long TruncatePrice(long price)
        {
            if (price <= 10000L) return price;
            long remainder = price <= 1000000L ? price % 100L : price % 10000L;
            return price - remainder;
        }

        /// <summary>
        /// Port of <c>Utils.rollFromWeightedMap(Map)</c>. Weights are per-mille on a fixed
        /// 1000 scale — they are NOT normalised against their own sum, so a table whose
        /// weights add up to less than 1000 intentionally has a chance of returning nothing.
        /// That gap is how the original expresses "no drop".
        ///
        /// Evidence: Utils.rollFromWeightedMap DAD — <c>random() * 1000.0</c>, running sum,
        /// returns the first key whose cumulative weight exceeds the roll.
        /// </summary>
        /// <param name="weightedEntries">Candidate to per-mille weight. Enumeration order matters.</param>
        /// <param name="roll01">A uniform value in [0,1), i.e. the result of Utils.random().</param>
        /// <returns>The selected key, or <c>default</c> when the roll falls past every entry.</returns>
        public static T RollFromWeightedMap<T>(IEnumerable<KeyValuePair<T, int>> weightedEntries, double roll01)
        {
            if (weightedEntries == null) return default;

            double target = roll01 * 1000.0d;
            int cumulative = 0;

            foreach (KeyValuePair<T, int> entry in weightedEntries)
            {
                cumulative += entry.Value;
                if (target < cumulative) return entry.Key;
            }

            return default;
        }
    }
}
