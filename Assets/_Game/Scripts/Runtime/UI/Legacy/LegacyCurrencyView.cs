using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Shell; // For LegacyCurrencyAdapter

namespace GuildMaster.Runtime.UI.Legacy
{
    /// <summary>
    /// A reusable component that takes a raw money value (long) and formats it into
    /// Platinum, Gold, Silver, and Copper using the exact same breakdown logic as the HUD.
    /// Hides higher denominations if they are 0.
    /// </summary>
    public class LegacyCurrencyView : MonoBehaviour
    {
        [SerializeField] private GameObject _platinumGroup;
        [SerializeField] private Text _platinumText;
        
        [SerializeField] private GameObject _goldGroup;
        [SerializeField] private Text _goldText;
        
        [SerializeField] private GameObject _silverGroup;
        [SerializeField] private Text _silverText;
        
        [SerializeField] private GameObject _copperGroup;
        [SerializeField] private Text _copperText;

        public void SetMoney(long money)
        {
            var coins = LegacyCurrencyAdapter.FromMoney(money);

            // Platinum
            if (coins.Platinum > 0)
            {
                if (_platinumGroup != null) _platinumGroup.SetActive(true);
                if (_platinumText != null) _platinumText.text = coins.Platinum.ToString();
            }
            else
            {
                if (_platinumGroup != null) _platinumGroup.SetActive(false);
            }

            // Gold
            if (coins.Gold > 0 || coins.Platinum > 0)
            {
                if (_goldGroup != null) _goldGroup.SetActive(true);
                if (_goldText != null) _goldText.text = coins.Gold.ToString();
            }
            else
            {
                if (_goldGroup != null) _goldGroup.SetActive(false);
            }

            // Silver
            if (coins.Silver > 0 || coins.Gold > 0 || coins.Platinum > 0)
            {
                if (_silverGroup != null) _silverGroup.SetActive(true);
                if (_silverText != null) _silverText.text = coins.Silver.ToString();
            }
            else
            {
                if (_silverGroup != null) _silverGroup.SetActive(false);
            }

            // Copper is always shown even if 0
            if (_copperGroup != null) _copperGroup.SetActive(true);
            if (_copperText != null) _copperText.text = coins.Copper.ToString();
        }
    }
}
