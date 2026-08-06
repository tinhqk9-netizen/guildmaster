using System;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Legacy;

namespace GuildMaster.Runtime.UI.Headquarters
{
    public class QuartersDialog : MonoBehaviour
    {
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Text _upgradeDescriptionText;
        [SerializeField] private LegacyCurrencyView _costCurrencyView;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Text _upgradeButtonText;
        [SerializeField] private Button _closeButton;

        private ServiceContainer _services;
        private Action _onClose;
        private Action _onStateChanged;
        
        // This is the literal value from FormulaService.cs used to denote max level.
        private const long MAX_LEVEL_PRICE_THRESHOLD = 99999999999999L;

        public void Setup(ServiceContainer services, Action onClose, Action onStateChanged)
        {
            _services = services;
            _onClose = onClose;
            _onStateChanged = onStateChanged;

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(() => _onClose?.Invoke());
            }

            if (_upgradeButton != null)
            {
                _upgradeButton.onClick.RemoveAllListeners();
                _upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }

            Refresh();
        }

        private void Refresh()
        {
            if (_services == null) return;

            int capacity = _services.Tavern.GetQuartersCapacity();
            long price = _services.Tavern.GetUpgradeQuartersPrice();
            long currentMoney = _services.Save.CurrentData.Money;

            bool isMaxLevel = price >= MAX_LEVEL_PRICE_THRESHOLD;
            bool canAfford = currentMoney >= price;

            if (_descriptionText != null)
            {
                _descriptionText.text = $"Current Capacity: {capacity}";
            }

            if (_upgradeDescriptionText != null)
            {
                if (isMaxLevel)
                {
                    _upgradeDescriptionText.text = "Maximum capacity reached.";
                    _upgradeDescriptionText.color = LegacyUITheme.DimWhite; // standard text
                }
                else
                {
                    // The "Upgrade" action already has its own button label — repeating the word
                    // here as a heading just duplicated it. One clear benefit sentence instead.
                    _upgradeDescriptionText.text = "Increases the maximum number of adventurers you can recruit.";
                    _upgradeDescriptionText.color = LegacyUITheme.DimWhite;
                }
            }

            if (_costCurrencyView != null)
            {
                _costCurrencyView.gameObject.SetActive(!isMaxLevel);
                if (!isMaxLevel)
                {
                    _costCurrencyView.SetMoney(price);
                }
            }

            if (_upgradeButton != null)
            {
                _upgradeButton.interactable = !isMaxLevel && canAfford;
                if (_upgradeButtonText != null)
                {
                    _upgradeButtonText.text = isMaxLevel ? "MAX" : "UPGRADE";
                    _upgradeButtonText.color = (!isMaxLevel && !canAfford) ? LegacyUITheme.Failure : LegacyUITheme.DimWhite;
                }
            }
        }

        private void OnUpgradeClicked()
        {
            if (_services == null) return;
            
            // Check affordability and not max level (Double check before doing anything)
            long price = _services.Tavern.GetUpgradeQuartersPrice();
            long currentMoney = _services.Save.CurrentData.Money;
            bool isMaxLevel = price >= MAX_LEVEL_PRICE_THRESHOLD;
            bool canAfford = currentMoney >= price;
            
            if (isMaxLevel || !canAfford) return;

            // Service method handles deducting money and increasing level
            bool success = _services.Tavern.UpgradeQuarters();
            if (success)
            {
                Refresh();
                _onStateChanged?.Invoke();
            }
        }
    }
}
