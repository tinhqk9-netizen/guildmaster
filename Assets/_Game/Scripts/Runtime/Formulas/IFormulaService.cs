namespace GuildMaster.Runtime.Formulas
{
    /// <summary>
    /// Upgrade-purchase flags that several formulas read directly in the original
    /// (<c>MainActivity.data.isStarterPackPurchased()</c> and friends). They feed capacity and
    /// price maths, so they have to be supplied even though the store itself is out of scope.
    /// Everything defaults to false — the state of a player who has never bought anything.
    /// </summary>
    public struct PurchaseFlags
    {
        public bool StarterPack;
        public bool AdventurerPack;
        public bool MerchantPack;
        public bool ImperialVanguard;
        public bool UnholyCrusade;

        public static PurchaseFlags None => default;
    }

    /// <summary>
    /// Full port of <c>Formulas.java</c> (20 methods), including its rounding and truncation
    /// behaviour. The original reads inputs from a global <c>MainActivity.data</c>; here they
    /// are passed explicitly so the service stays testable and free of static state.
    ///
    /// Evidence: Reports/S6_5A/S6_5A_001_Rule_Extraction_Report.md (F-01 … F-20)
    /// </summary>
    public interface IFormulaService
    {
        // --- Progression ---
        int TotalStarsToNextLp(int lpLevel);
        int ExperienceToNextLevel(int currentLevel, bool isAdventurer);
        int FoodToNextLevel(int currentLevel);

        // --- Upgrade prices ---
        long GetQuartersPrice(int levelQuarters);
        long GetTavernCapacityPrice(int levelTavernCapacity);
        long GetTavernTimePrice(int levelTavernTime);
        long GetStorageCapacityPrice(int levelStorage);
        long GetMarketListingsPrice(int levelMarketListings);
        long GetMarketTimePrice(int levelMarketTime);
        long GetWorkshopQueuePrice(int levelWorkshopQueue);
        long GetWorkshopTimePrice(int levelWorkshopTime);
        long GetShelterPrice(int levelShelter);
        long GetShelterAutofeedPrice(int levelShelterAutofeed);

        // --- Capacities ---
        int GetQuartersCapacity(int levelQuarters, int upgradeQuarters, PurchaseFlags flags);
        int GetTavernCapacity(int levelTavernCapacity, int upgradeTavernCapacity, PurchaseFlags flags);
        int MarketListings(int levelMarketListings, int upgradeMarketQueue, PurchaseFlags flags);
        int WorkshopQueue(int levelWorkshopQueue, int upgradeWorkshopQueue, PurchaseFlags flags);
        int StorageSpaces(int levelStorage, int upgradeStorage, PurchaseFlags flags);
        int ShelterCapacity(int levelShelter, int upgradeShelter);

        // --- Timing ---
        /// <summary>Tavern visitor interval in MILLISECONDS — the original returns ms.</summary>
        long GetTavernVisitorInterval(int levelTavernTime, int upgradeTavernTime);
        long GetSecondsToCraft(long itemPrice, int itemStack, int levelWorkshopTime, int upgradeWorkshopTime, PurchaseFlags flags);
        long GetSecondsToSell(long itemPrice, int itemStack, int levelMarketTime, int upgradeMarketTime, PurchaseFlags flags);
    }
}
