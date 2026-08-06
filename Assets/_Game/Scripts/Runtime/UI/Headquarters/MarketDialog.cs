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
    /// Sell timer duration mirrors MerchantService.DEFAULT_SELL_TIME_SECONDS (private, 20s — not
    /// exposed on IMerchantService), the same "hard-coded, not exposed" situation already
    /// documented for Workshop's craft duration. Buy/Merchant Stock section reads the real
    /// GetRegularStock()/GetSpecialStock() APIs, but nothing in the runtime ever calls
    /// RollRegularOffer/RollSpecialOffer to populate them (verified — no caller exists anywhere),
    /// so the section is real but will show its empty state until a dungeon-completion flow wires
    /// that in a later phase. No stock is fabricated here.
    /// </summary>
    public sealed class MarketDialog : MonoBehaviour
    {
        public const long SellDurationSeconds = 20; // Mirrors MerchantService.DEFAULT_SELL_TIME_SECONDS (private, not exposed).

        [Header("Header")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Button _closeButton;

        [Header("List")]
        [SerializeField] private RectTransform _listContent;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private Sprite _rowBorderSprite;

        private ServiceContainer _services;
        private System.Action _onClose;
        private System.Action _onStateChanged;

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

            Refresh();
        }

        public void Refresh()
        {
            if (_services?.Merchant == null || _listContent == null) return;

            var selling = _services.Merchant.GetMarketListings();
            var sold = _services.Merchant.GetSoldMarketItems();
            var regularStock = _services.Merchant.GetRegularStock();
            var specialStock = _services.Merchant.GetSpecialStock();

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
                    long remaining = isActive ? System.Math.Max(0, SellDurationSeconds - item.SecondsPassed) : -1;
                    float progress = isActive ? Mathf.Clamp01((float)item.SecondsPassed / SellDurationSeconds) : 0f;
                    long payout = ComputeExpectedPayout(item.DefinitionId, item.StackCount);
                    string status = isActive
                        ? $"Selling... {remaining}s (est. {payout}g)"
                        : $"Waiting... (est. {payout}g)";
                    WorkshopRowBuilder.CreateRow(_listContent, _rowBorderSprite, item.DefinitionId, item.StackCount,
                        status, showProgress: isActive, progress: progress, actionLabel: null, onAction: null);
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
        /// Precomputes the same payout MerchantService.ClaimSoldItem will actually pay — mirrors
        /// its real fallback (ItemDefinition.SellPrice if &gt; 0, else 100) rather than inventing a
        /// new formula. Shown for information only; never used to credit currency itself.
        /// </summary>
        private long ComputeExpectedPayout(string definitionId, int stackCount)
        {
            long unitPrice = 100;
            if (_services.Database.TryGet<ItemDefinition>(definitionId, out var def) && def.SellPrice > 0)
                unitPrice = def.SellPrice;
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
