using System.Collections.Generic;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public interface IMerchantService
    {
        MerchantOfferData RollRegularOffer(string dungeonId);
        MerchantOfferData RollSpecialOffer(string dungeonId);
        bool BuyOffer(MerchantOfferSaveData offer, bool isSpecial);
        MerchantResult SellItem(string definitionId, int stackCount);
        void ProgressMarket(long deltaSeconds);
        long GetSellDurationSeconds(ItemActionSaveData item);
        bool CancelListing(string instanceId);
        bool ClaimSoldItem(string instanceId);
        void ProcessScheduledRefreshes(long currentUnix);
        bool UpgradeMarketListings();
        bool UpgradeMarketTime();
        long GetUpgradeMarketListingsPrice();
        long GetUpgradeMarketTimePrice();
        int GetMarketListingsCapacity();
        int GetMarketListingsLevel();
        int GetMarketTimeLevel();
        IReadOnlyList<MerchantOfferSaveData> GetRegularStock();
        IReadOnlyList<MerchantOfferSaveData> GetSpecialStock();

        // UI Listings Helpers
        IReadOnlyList<ItemActionSaveData> GetMarketListings();
        IReadOnlyList<ItemActionSaveData> GetSoldMarketItems();
    }
}
