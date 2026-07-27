using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface IMerchantService
    {
        MerchantOfferData RollRegularOffer(string dungeonId);
        MerchantOfferData RollSpecialOffer(string dungeonId);
        MerchantResult BuyItem(string dungeonId, string itemId);
        MerchantResult SellItem(string instanceIdOrDefinitionId, int stackCount);
        void ProgressMarket(long deltaSeconds);
    }
}
