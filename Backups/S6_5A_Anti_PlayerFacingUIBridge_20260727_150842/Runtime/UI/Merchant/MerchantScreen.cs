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

        public void Initialize(IMerchantService merchantService)
        {
            _merchantService = merchantService;
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
    }
}
