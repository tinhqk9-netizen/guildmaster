using System.Collections;
using System.Collections.Generic;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Legacy;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Headquarters
{
    /// <summary>
    /// Phase 5F Market dialog. Reads IMerchantService only; never mutates SaveData itself.
    /// Sell duration, payout, and stock rotation are delegated to IMerchantService. No economy
    /// values are duplicated in this view.
    /// </summary>
    public sealed class MarketDialog : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _listingUpgradeInfoText;
        [SerializeField] private Button _listingUpgradeButton;
        [SerializeField] private Text _speedUpgradeInfoText;
        [SerializeField] private Button _speedUpgradeButton;
        [SerializeField] private Button _closeButton;

        [Header("List")]
        [SerializeField] private RectTransform _listContent;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private Sprite _rowBorderSprite;

        private ServiceContainer _services;
        private System.Action _onClose;
        private System.Action _onStateChanged;
        private Coroutine _refreshCoroutine;

        public void Setup(ServiceContainer services, System.Action onClose, System.Action onStateChanged)
        {
            _services = services;
            _onClose = onClose;
            _onStateChanged = onStateChanged;

            if (_titleText != null) _titleText.text = "Market";

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(() => _onClose?.Invoke());
            }

            if (_listingUpgradeButton != null)
            {
                _listingUpgradeButton.onClick.RemoveAllListeners();
                _listingUpgradeButton.onClick.AddListener(OnListingUpgradeClicked);
            }

            if (_speedUpgradeButton != null)
            {
                _speedUpgradeButton.onClick.RemoveAllListeners();
                _speedUpgradeButton.onClick.AddListener(OnSpeedUpgradeClicked);
            }

            Refresh();
            StartAutoRefresh();
        }

        private void OnEnable()
        {
            StartAutoRefresh();
        }

        private void OnDisable()
        {
            if (_refreshCoroutine != null)
            {
                StopCoroutine(_refreshCoroutine);
                _refreshCoroutine = null;
            }
        }

        private void StartAutoRefresh()
        {
            if (_refreshCoroutine == null && _services?.Merchant != null)
                _refreshCoroutine = StartCoroutine(AutoRefreshLoop());
        }

        private IEnumerator AutoRefreshLoop()
        {
            var wait = new WaitForSecondsRealtime(1f);
            while (isActiveAndEnabled)
            {
                yield return wait;
                if (_services?.Merchant != null) Refresh();
            }
        }

        public void Refresh()
        {
            if (_services?.Merchant == null || _listContent == null) return;

            var selling = _services.Merchant.GetMarketListings();
            var sold = _services.Merchant.GetSoldMarketItems();
            var regularStock = _services.Merchant.GetRegularStock();
            var specialStock = _services.Merchant.GetSpecialStock();

            RefreshUpgradeControls();

            for (int i = _listContent.childCount - 1; i >= 0; i--)
                Destroy(_listContent.GetChild(i).gameObject);

            bool allEmpty = selling.Count == 0 && sold.Count == 0 && regularStock.Count == 0 && specialStock.Count == 0;
            if (_emptyState != null) _emptyState.SetActive(allEmpty);
            if (allEmpty) return;

            if (selling.Count > 0)
            {
                AddDivider("Selling");
                for (int i = 0; i < selling.Count; i++)
                {
                    var item = selling[i];
                    bool isActive = i == 0;
                    long duration = _services.Merchant.GetSellDurationSeconds(item);
                    long remaining = isActive ? System.Math.Max(0, duration - item.SecondsPassed) : -1;
                    float progress = isActive && duration > 0 ? Mathf.Clamp01((float)item.SecondsPassed / duration) : 0f;
                    long payout = ComputeExpectedPayout(item.DefinitionId, item.StackCount);
                    string status = isActive
                        ? $"Selling... {remaining}s (est. {payout}g)"
                        : $"Waiting... (est. {payout}g)";
                    string instanceId = item.InstanceId;
                    WorkshopRowBuilder.CreateRow(_listContent, _rowBorderSprite, item.DefinitionId, item.StackCount,
                        status, showProgress: isActive, progress: progress, actionLabel: "Cancel",
                        onAction: () => OnCancelClicked(instanceId));
                }
            }

            if (sold.Count > 0)
            {
                AddDivider("Sold");
                foreach (var item in sold)
                {
                    string instanceId = item.InstanceId;
                    long payout = ComputeExpectedPayout(item.DefinitionId, item.StackCount);
                    WorkshopRowBuilder.CreateRow(_listContent, _rowBorderSprite, item.DefinitionId, item.StackCount,
                        $"Payout: {payout}g", showProgress: false, progress: 1f, actionLabel: "Claim",
                        onAction: () => OnClaimClicked(instanceId));
                }
            }

            if (regularStock.Count > 0 || specialStock.Count > 0)
            {
                AddDivider("Buy");
                foreach (var offer in regularStock) CreateStockRow(offer, isSpecial: false);
                foreach (var offer in specialStock) CreateStockRow(offer, isSpecial: true);
            }
        }

        private void RefreshUpgradeControls()
        {
            long money = _services.Save.CurrentData.Money;
            int listingLevel = _services.Merchant.GetMarketListingsLevel();
            long listingPrice = _services.Merchant.GetUpgradeMarketListingsPrice();
            bool listingMax = listingLevel >= 10;
            if (_listingUpgradeInfoText != null)
                _listingUpgradeInfoText.text = listingMax
                    ? $"Listing capacity • Level {listingLevel} • MAX"
                    : $"Listing capacity • Level {listingLevel} • Next: {listingPrice}g";
            if (_listingUpgradeButton != null)
            {
                _listingUpgradeButton.interactable = !listingMax && money >= listingPrice;
                var label = _listingUpgradeButton.GetComponentInChildren<Text>();
                if (label != null) label.text = listingMax ? "Max Listings" : "Upgrade Listings";
            }

            int speedLevel = _services.Merchant.GetMarketTimeLevel();
            long speedPrice = _services.Merchant.GetUpgradeMarketTimePrice();
            bool speedMax = speedLevel >= 25;
            if (_speedUpgradeInfoText != null)
                _speedUpgradeInfoText.text = speedMax
                    ? $"Market speed • Level {speedLevel} • MAX"
                    : $"Market speed • Level {speedLevel} • Next: {speedPrice}g";
            if (_speedUpgradeButton != null)
            {
                _speedUpgradeButton.interactable = !speedMax && money >= speedPrice;
                var label = _speedUpgradeButton.GetComponentInChildren<Text>();
                if (label != null) label.text = speedMax ? "Max Speed" : "Upgrade Speed";
            }
        }

        private void OnListingUpgradeClicked()
        {
            if (_services?.Merchant == null) return;
            if (_services.Merchant.UpgradeMarketListings()) OnStateChangedInternal();
            else RefreshUpgradeControls();
        }

        private void OnSpeedUpgradeClicked()
        {
            if (_services?.Merchant == null) return;
            if (_services.Merchant.UpgradeMarketTime()) OnStateChangedInternal();
            else RefreshUpgradeControls();
        }

        private void CreateStockRow(MerchantOfferSaveData offer, bool isSpecial)
        {
            long money = _services.Save.CurrentData.Money;
            long gems = _services.Save.CurrentData.Gems;
            bool canAfford = offer.IsGems ? gems >= offer.Price : money >= offer.Price;
            bool canAdd = _services.Inventory.CanAddItem(offer.DefinitionId);
            string currency = offer.IsGems ? "gem" : "g";

            var row = WorkshopRowBuilder.CreateRow(_listContent, _rowBorderSprite, offer.DefinitionId, offer.StackCount,
                $"Price: {offer.Price}{currency}", showProgress: false, progress: 0f,
                actionLabel: "Buy", onAction: () => OnBuyClicked(offer, isSpecial));

            var button = row.GetComponentInChildren<Button>();
            if (button != null) button.interactable = canAfford && canAdd;
        }

        private void OnClaimClicked(string instanceId)
        {
            if (_services?.Merchant == null) return;
            bool success = _services.Merchant.ClaimSoldItem(instanceId);
            if (success) OnStateChangedInternal();
        }

        private void OnCancelClicked(string instanceId)
        {
            if (_services?.Merchant == null) return;
            if (_services.Merchant.CancelListing(instanceId)) OnStateChangedInternal();
        }

        private void OnBuyClicked(MerchantOfferSaveData offer, bool isSpecial)
        {
            if (_services?.Merchant == null) return;
            bool success = _services.Merchant.BuyOffer(offer, isSpecial);
            if (success) OnStateChangedInternal();
        }

        private void OnStateChangedInternal()
        {
            Refresh();
            _onStateChanged?.Invoke();
        }

        /// <summary>
        /// Precomputes the same base item price MerchantService.ClaimSoldItem pays.
        /// </summary>
        private long ComputeExpectedPayout(string definitionId, int stackCount)
        {
            long unitPrice = 0;
            if (_services.Database.TryGet<ItemDefinition>(definitionId, out var def))
                unitPrice = GuildMaster.Runtime.Formulas.DecodeMath.TruncatePrice(def.Price);
            return unitPrice * stackCount;
        }

        private void AddDivider(string label)
        {
            var go = new GameObject($"Divider_{label}", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            go.transform.SetParent(_listContent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.fontStyle = FontStyle.Bold;
            text.color = LegacyUITheme.AscendedUnit;
            text.alignment = TextAnchor.MiddleLeft;
            text.text = label;
            go.GetComponent<LayoutElement>().preferredHeight = 46f;
        }
    }
}
