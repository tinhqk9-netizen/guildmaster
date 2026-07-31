using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.UI.Inventory;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.UI.Merchant
{
    /// <summary>
    /// Merchant screen — 3 tabs: Buy (regular + special offers) | Sell | Listings.
    /// Supports listing items for sell, matching sold listings, and claiming gold.
    /// </summary>
    public class MerchantScreen : UIScreen
    {
        private IMerchantService _merchantService;
        private InventoryScreen  _inventoryScreen;

        // ── Tab state ────────────────────────────────────────────────────────────
        private enum MerchantTab { Buy, Sell, Listings }
        private MerchantTab _activeTab = MerchantTab.Buy;

        // ── Serialized references (wired by Apply tool) ──────────────────────────
        [SerializeField] private RectTransform _cardContainer;
        [SerializeField] private Text          _summaryText;
        [SerializeField] private Text          _detailText;
        [SerializeField] private Button        _tabBuyBtn;
        [SerializeField] private Button        _tabSellBtn;
        [SerializeField] private Button        _tabListingsBtn;
        [SerializeField] private Button        _buyRegularButton;
        [SerializeField] private Button        _buySpecialButton;
        [SerializeField] private Button        _sellButton;
        [SerializeField] private Button        _claimSoldButton;
        [SerializeField] private Text          _feedbackText;

        private int _selectedRegularIndex;
        private int _selectedSpecialIndex;
        private bool _buyingRegular = true;  // which section is active within Buy tab

        // Market selection
        private int _selectedMarketIndex = -1;
        private bool _selectedMarketIsSold = false;

        public void Initialize(ServiceContainer services, InventoryScreen inventoryScreen = null)
        {
            _merchantService = services.Merchant;
            _inventoryScreen = inventoryScreen;
        }

        public override void Show()
        {
            base.Show();
            _activeTab = MerchantTab.Buy;
            _selectedMarketIndex = -1;
            Refresh();
        }

        public void Refresh()
        {
            if (_merchantService == null) return;

            UpdateTabButtons();

            switch (_activeTab)
            {
                case MerchantTab.Buy:      ShowBuyTab();      break;
                case MerchantTab.Sell:     ShowSellTab();     break;
                case MerchantTab.Listings: ShowListingsTab(); break;
            }
        }

        // ── Tab switching ────────────────────────────────────────────────────────

        public void OnClickTabBuy()      { _activeTab = MerchantTab.Buy;      Refresh(); }
        public void OnClickTabSell()     { _activeTab = MerchantTab.Sell;     Refresh(); }
        public void OnClickTabListings() { _activeTab = MerchantTab.Listings; Refresh(); }

        private void UpdateTabButtons()
        {
            SetTabActive(_tabBuyBtn,      _activeTab == MerchantTab.Buy);
            SetTabActive(_tabSellBtn,     _activeTab == MerchantTab.Sell);
            SetTabActive(_tabListingsBtn, _activeTab == MerchantTab.Listings);

            // Show/hide relevant action buttons
            if (_buyRegularButton  != null) _buyRegularButton.gameObject.SetActive(_activeTab == MerchantTab.Buy);
            if (_buySpecialButton  != null) _buySpecialButton.gameObject.SetActive(_activeTab == MerchantTab.Buy);
            if (_sellButton        != null) _sellButton.gameObject.SetActive(_activeTab == MerchantTab.Sell);
            if (_claimSoldButton   != null) _claimSoldButton.gameObject.SetActive(_activeTab == MerchantTab.Listings);
        }

        private static void SetTabActive(Button btn, bool active)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = active ? UITemporaryTheme.TabActive : UITemporaryTheme.TabInactive;
        }

        // ── Buy tab ──────────────────────────────────────────────────────────────

        private void ShowBuyTab()
        {
            if (_cardContainer == null) return;
            UICardFactory.ClearContainer(_cardContainer);

            var regular = _merchantService.GetRegularStock();
            var special = _merchantService.GetSpecialStock();

            if (regular.Count == 0 && special.Count == 0)
            {
                UICardFactory.CreateDivider(_cardContainer, "MERCHANT SHOP");
                UICardFactory.CreateCard(_cardContainer, "🏪 Merchant Out of Stock", "Check back later for new inventory restocks!", false, false, null, preferredHeight: 65);
                if (_summaryText != null) _summaryText.text = "Stock: 0";
                if (_detailText  != null) _detailText.text  = "Merchant is currently out of stock.";
                return;
            }

            int totalStock = regular.Count + special.Count;
            if (_summaryText != null) _summaryText.text = $"Stock: {totalStock} item(s)";

            // Renders standard listings
            if (regular.Count > 0)
            {
                UICardFactory.CreateDivider(_cardContainer, "Regular Stock");
                for (int i = 0; i < regular.Count; i++)
                {
                    int captured = i;
                    bool sel     = _buyingRegular && (i == _selectedRegularIndex);
                    var offer    = regular[i];
                    string details = $"{offer.Price}g  |  Qty: {offer.StackCount}";

                    UICardFactory.CreateCard(_cardContainer, offer.DefinitionId, details,
                        sel, true, () => SelectRegularItem(captured));
                }
            }

            // Renders special offerings
            if (special.Count > 0)
            {
                UICardFactory.CreateDivider(_cardContainer, "Special Stock");
                for (int i = 0; i < special.Count; i++)
                {
                    int captured = i;
                    bool sel     = !_buyingRegular && (i == _selectedSpecialIndex);
                    var offer    = special[i];
                    string details = $"{offer.Price}g  |  Qty: {offer.StackCount}";

                    UICardFactory.CreateCard(_cardContainer, offer.DefinitionId, details,
                        sel, true, () => SelectSpecialItem(captured));
                }
            }

            RefreshBuyDetail(regular, special);
        }

        private void SelectRegularItem(int index)
        {
            _buyingRegular        = true;
            _selectedRegularIndex = index;
            Refresh();
        }

        private void SelectSpecialItem(int index)
        {
            _buyingRegular        = false;
            _selectedSpecialIndex = index;
            Refresh();
        }

        private void RefreshBuyDetail(IReadOnlyList<MerchantOfferSaveData> regular, IReadOnlyList<MerchantOfferSaveData> special)
        {
            if (_detailText == null) return;

            if (_buyingRegular)
            {
                if (regular.Count == 0) { _detailText.text = ""; return; }
                _selectedRegularIndex = Mathf.Clamp(_selectedRegularIndex, 0, regular.Count - 1);
                var offer = regular[_selectedRegularIndex];
                _detailText.text = $"Regular Offer: {offer.DefinitionId}\nCost: {offer.Price}g\nQuantity: x{offer.StackCount}";

                if (_buyRegularButton != null) _buyRegularButton.interactable = true;
                if (_buySpecialButton != null) _buySpecialButton.interactable = false;
            }
            else
            {
                if (special.Count == 0) { _detailText.text = ""; return; }
                _selectedSpecialIndex = Mathf.Clamp(_selectedSpecialIndex, 0, special.Count - 1);
                var offer = special[_selectedSpecialIndex];
                _detailText.text = $"Special Offer: {offer.DefinitionId}\nCost: {offer.Price}g\nQuantity: x{offer.StackCount}\n(Rare deals!)";

                if (_buyRegularButton != null) _buyRegularButton.interactable = false;
                if (_buySpecialButton != null) _buySpecialButton.interactable = true;
            }
        }

        // ── Sell tab ─────────────────────────────────────────────────────────────

        private void ShowSellTab()
        {
            if (_cardContainer == null) return;
            UICardFactory.ClearContainer(_cardContainer);

            if (_inventoryScreen == null)
            {
                if (_detailText  != null) _detailText.text  = "Inventory UI not attached.";
                if (_summaryText != null) _summaryText.text = "Error";
                if (_sellButton  != null) _sellButton.interactable = false;
                return;
            }

            var item = _inventoryScreen.GetSelectedItem();
            if (item == null)
            {
                if (_detailText  != null) _detailText.text  = "Select an item from the HUD's bag or character slot to sell.";
                if (_summaryText != null) _summaryText.text = "No item selected from Inventory";
                if (_sellButton  != null) _sellButton.interactable = false;
                return;
            }

            string defId = item.Definition != null ? item.Definition.id : item.InstanceId;
            UICardFactory.CreateCard(_cardContainer, defId,
                $"x{item.StackCount}  |  {(item.IsLocked ? "Locked" : "Available")}",
                true, !item.IsLocked, null);

            if (_detailText != null)
                _detailText.text = $"Selling: {defId}\nQuantity: x{item.StackCount}\n" +
                                   $"{(item.IsLocked ? "Item is locked. Unlock it in Inventory first." : "Ready to sell.")}";

            if (_summaryText != null)
                _summaryText.text = "Item from Inventory selected";

            if (_sellButton != null) _sellButton.interactable = !item.IsLocked;
        }

        // ── Listings tab ─────────────────────────────────────────────────────────

        private void ShowListingsTab()
        {
            if (_cardContainer == null) return;
            UICardFactory.ClearContainer(_cardContainer);

            var activeListings = _merchantService.GetMarketListings();
            var soldListings   = _merchantService.GetSoldMarketItems();

            int total = activeListings.Count + soldListings.Count;
            if (_summaryText != null)
                _summaryText.text = $"Active Listings: {activeListings.Count}  |  Sold Listings: {soldListings.Count}";

            if (total == 0)
            {
                UICardFactory.CreateDivider(_cardContainer, "Market Empty");
                UICardFactory.CreateDivider(_cardContainer, "List items from the Sell tab to see them here.");
                if (_detailText  != null) _detailText.text  = "";
                if (_claimSoldButton != null) _claimSoldButton.interactable = false;
                return;
            }

            // Draw Sold Listings first (highest priority to action)
            if (soldListings.Count > 0)
            {
                UICardFactory.CreateDivider(_cardContainer, "★ Sold Items (Claim Pouch) ★");
                for (int i = 0; i < soldListings.Count; i++)
                {
                    int captured = i;
                    var item = soldListings[i];
                    bool isSel = _selectedMarketIsSold && (_selectedMarketIndex == i);
                    UICardFactory.CreateCard(_cardContainer, item.DefinitionId, $"SOLD! x{item.StackCount}",
                        isSel, true, () => SelectMarketItem(captured, true));
                }
            }

            // Draw Active Listings
            if (activeListings.Count > 0)
            {
                UICardFactory.CreateDivider(_cardContainer, "Active Market Listings");
                for (int i = 0; i < activeListings.Count; i++)
                {
                    int captured = i;
                    var item = activeListings[i];
                    bool isSel = !_selectedMarketIsSold && (_selectedMarketIndex == i);
                    UICardFactory.CreateCard(_cardContainer, item.DefinitionId, $"Active x{item.StackCount}",
                        isSel, true, () => SelectMarketItem(captured, false));
                }
            }

            RefreshMarketDetail(activeListings, soldListings);
        }

        private void SelectMarketItem(int index, bool isSold)
        {
            _selectedMarketIndex  = index;
            _selectedMarketIsSold = isSold;
            Refresh();
        }

        private void RefreshMarketDetail(IReadOnlyList<ItemActionSaveData> active, IReadOnlyList<ItemActionSaveData> sold)
        {
            if (_detailText == null) return;

            if (_selectedMarketIndex < 0)
            {
                _detailText.text = "Select a listing to see details.";
                if (_claimSoldButton != null) _claimSoldButton.interactable = false;
                return;
            }

            if (_selectedMarketIsSold)
            {
                if (_selectedMarketIndex >= sold.Count) { _selectedMarketIndex = -1; Refresh(); return; }
                var item = sold[_selectedMarketIndex];
                _detailText.text = $"Item: {item.DefinitionId}\nQuantity: x{item.StackCount}\nStatus: SOLD!\n\nClick 'Claim Gold' to receive your gold bag.";
                if (_claimSoldButton != null) _claimSoldButton.interactable = true;
            }
            else
            {
                if (_selectedMarketIndex >= active.Count) { _selectedMarketIndex = -1; Refresh(); return; }
                var item = active[_selectedMarketIndex];
                _detailText.text = $"Item: {item.DefinitionId}\nQuantity: x{item.StackCount}\nStatus: Active (Listing)\n\nWaiting for a buyer to interact...";
                if (_claimSoldButton != null) _claimSoldButton.interactable = false;
            }
        }

        // ── Selection helpers ────────────────────────────────────────────────────────────

        public void SelectRegularIndex(int index)
        {
            var stock = _merchantService?.GetRegularStock();
            if (stock == null || stock.Count == 0) return;
            _selectedRegularIndex = Mathf.Clamp(index, 0, stock.Count - 1);
        }

        public void SelectSpecialIndex(int index)
        {
            var stock = _merchantService?.GetSpecialStock();
            if (stock == null || stock.Count == 0) return;
            _selectedSpecialIndex = Mathf.Clamp(index, 0, stock.Count - 1);
        }

        // ── Actions ──────────────────────────────────────────────────────────────

        public void OnClickBuySelectedRegular()
        {
            var stock = _merchantService?.GetRegularStock();
            if (stock == null || stock.Count == 0) return;
            _selectedRegularIndex = Mathf.Clamp(_selectedRegularIndex, 0, stock.Count - 1);
            bool ok = _merchantService.BuyOffer(stock[_selectedRegularIndex], false);
            ShowFeedback(ok ? "Purchased!" : "Cannot buy: insufficient funds.", ok);
            Refresh();
        }

        public void OnClickBuySelectedSpecial()
        {
            var stock = _merchantService?.GetSpecialStock();
            if (stock == null || stock.Count == 0) return;
            _selectedSpecialIndex = Mathf.Clamp(_selectedSpecialIndex, 0, stock.Count - 1);
            bool ok = _merchantService.BuyOffer(stock[_selectedSpecialIndex], true);
            ShowFeedback(ok ? "Purchased!" : "Cannot buy: insufficient funds.", ok);
            Refresh();
        }

        public void OnClickSellSelected()
        {
            if (_inventoryScreen == null || _merchantService == null) return;
            var item = _inventoryScreen.GetSelectedItem();
            if (item == null || item.IsLocked) return;

            string id = item.Definition != null ? item.Definition.id : item.InstanceId;
            int count = item.StackCount;

            var res = _merchantService.SellItem(id, count);
            if (res.Success)
            {
                ShowFeedback($"Listed {id} x{count} on the market!", true);
                _inventoryScreen.Refresh();
            }
            else
            {
                ShowFeedback($"Cannot sell: {res.FailureReason}", false);
            }
            Refresh();
        }

        public void OnClickClaimSold()
        {
            if (_merchantService == null) return;
            var sold = _merchantService.GetSoldMarketItems();
            if (sold.Count == 0 || _selectedMarketIndex < 0 || !_selectedMarketIsSold) return;

            _selectedMarketIndex = Mathf.Clamp(_selectedMarketIndex, 0, sold.Count - 1);
            var item = sold[_selectedMarketIndex];

            if (_merchantService.ClaimSoldItem(item.InstanceId))
            {
                ShowFeedback($"Claimed gold for sold {item.DefinitionId}!", true);
                _selectedMarketIndex = -1;
            }
            else
            {
                ShowFeedback("Claim failed.", false);
            }
            Refresh();
        }

        private void ShowFeedback(string msg, bool success)
        {
            if (_feedbackText == null) return;
            _feedbackText.text  = msg;
            _feedbackText.color = success ? UITemporaryTheme.SuccessColor : UITemporaryTheme.WarningColor;
        }
    }
}
