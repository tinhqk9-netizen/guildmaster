namespace GuildMaster.Runtime.UI.Shell
{
    /// <summary>
    /// Phase 3 (App Shell): UI-only display adapter that breaks the backend's single
    /// <c>SaveData.Money</c> (long) value into the legacy game's 4-tier coin display
    /// (Platinum/Gold/Silver/Copper), matching the recovered legacy formula exactly:
    /// <c>UIUtils.populateMoneyContainer()</c> —
    /// Docs/Legacy_Audit (Decode_Audit) 06_Decode_Formula_Ledger.md.
    ///
    /// This does not read/write any save field beyond the existing <c>Money</c> long, and does
    /// not modify SaveData, CharacterService, or any other backend type — it is purely a
    /// presentation-layer breakdown for the HUD currency bar.
    /// </summary>
    public static class LegacyCurrencyAdapter
    {
        public readonly struct Breakdown
        {
            public readonly long Platinum;
            public readonly long Gold;
            public readonly long Silver;
            public readonly long Copper;

            public Breakdown(long platinum, long gold, long silver, long copper)
            {
                Platinum = platinum;
                Gold = gold;
                Silver = silver;
                Copper = copper;
            }
        }

        /// <summary>
        /// Platinum = money / 1,000,000; Gold = (money % 1,000,000) / 10,000;
        /// Silver = (money % 10,000) / 100; Copper = money % 100.
        /// </summary>
        public static Breakdown FromMoney(long money)
        {
            if (money < 0) money = 0;

            long platinum = money / 1_000_000L;
            long gold = (money % 1_000_000L) / 10_000L;
            long silver = (money % 10_000L) / 100L;
            long copper = money % 100L;

            return new Breakdown(platinum, gold, silver, copper);
        }
    }
}
