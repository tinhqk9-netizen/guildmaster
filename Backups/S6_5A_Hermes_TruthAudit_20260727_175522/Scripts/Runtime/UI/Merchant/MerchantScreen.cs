using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Runtime.UI.Merchant
{
    public class MerchantScreen : UIScreen
    {
        private IMerchantService _merchantService;
        [SerializeField] private Text _statusText;

        private IInventoryService _inventoryService;

        public void Initialize(ServiceContainer services)
        {
            _merchantService = services.Merchant;
            _inventoryService = services.Inventory;
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Refresh()
        {
            if (_merchantService == null || _statusText == null) return;

            var regularStock = _merchantService.GetRegularStock();
            var specialStock = _merchantService.GetSpecialStock();

            string content = $"--- MERCHANT / MARKET ---\n";
            content += $"Regular Offers ({regularStock.Count}):\n";
            foreach (var reg in regularStock)
            {
                string curr = reg.IsGems ? "Gems" : "Gold";
                content += $"- {reg.DefinitionId} x{reg.StackCount} Price: {reg.Price} {curr}\n";
            }

            content += $"\nSpecial Offers ({specialStock.Count}):\n";
            foreach (var sp in specialStock)
            {
                string curr = sp.IsGems ? "Gems" : "Gold";
                content += $"- {sp.DefinitionId} x{sp.StackCount} Price: {sp.Price} {curr}\n";
            }

            _statusText.text = content;
        }

        public void OnClickBuyFirstRegular()
        {
            var regularStock = _merchantService?.GetRegularStock();
            if (regularStock != null && regularStock.Count > 0)
            {
                _merchantService.BuyOffer(regularStock[0], false);
                Refresh();
            }
        }

        public void OnClickSellFirstItem()
        {
            var items = _inventoryService?.GetAllItems();
            if (items != null && items.Count > 0)
            {
                var item = items[0];
                if (item.Definition != null)
                {
                    _merchantService?.SellItem(item.Definition.id, 1);
                }
                Refresh();
            }
        }

        public void OnClickProgressTime()
        {
            _merchantService?.ProgressMarket(3600);
            Refresh();
        }
    }
}
