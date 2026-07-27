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
        bool ClaimSoldItem(string instanceId);
        IReadOnlyList<MerchantOfferSaveData> GetRegularStock();
        IReadOnlyList<MerchantOfferSaveData> GetSpecialStock();
    }
}
