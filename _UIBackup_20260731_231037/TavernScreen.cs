using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.UI.Tavern
{
    public enum TavernTab
    {
        Tavern,
        Quarters
    }

    /// <summary>
    /// Tavern screen — 2 tabs matching decoded layout:
    /// Tab 1 (Tavern): View waiting visitors, inspect hero dossiers, recruit into guild roster.
    /// Tab 2 (Quarters): View and buy upgrades for Quarters capacity, Tavern visitor capacity, and Visitor speed.
    /// </summary>
    public class TavernScreen : UIScreen
    {
        private ITavernService _tavernService;
        private ISaveService   _saveService;

        // ── Serialized references (wired by Apply tool) ──────────────────────────
        [SerializeField] private Button        _tabTavernBtn;
        [SerializeField] private Button        _tabQuartersBtn;

        [SerializeField] private RectTransform _cardContainer;
        [SerializeField] private Text          _timerText;
        [SerializeField] private Text          _populationText;
        [SerializeField] private Text          _detailText;
        [SerializeField] private Text          _feedbackText;

        [SerializeField] private Button        _recruitButton;
        [SerializeField] private Button        _prevButton;
        [SerializeField] private Button        _nextButton;

        [SerializeField] private Button        _upgradeQuartersButton;
        [SerializeField] private Button        _upgradeCapacityButton;
        [SerializeField] private Button        _upgradeTimeButton;

        private TavernTab _activeTab = TavernTab.Tavern;
        private int       _selectedIndex = -1;

        public void Initialize(ServiceContainer services)
        {
            _tavernService = services.Tavern;
            _saveService   = services.Save;

            if (_tabTavernBtn != null)   _tabTavernBtn.onClick.AddListener(OnClickTabTavern);
            if (_tabQuartersBtn != null) _tabQuartersBtn.onClick.AddListener(OnClickTabQuarters);
        }

        public override void Show()
        {
            base.Show();
            _activeTab = TavernTab.Tavern;
            _selectedIndex = -1;
            Refresh();
        }

        public void Refresh()
        {
            if (_tavernService == null) return;

            UpdateTabButtons();

            var guests      = _tavernService.GetGuests();
            int tavernCap   = _tavernService.GetTavernCapacity();
            int quartersCap = _tavernService.GetQuartersCapacity();
            int rosterSize  = _saveService?.CurrentData?.Characters?.Count ?? 0;
            long timer      = _tavernService.GetNextVisitorTimerSeconds();

            // Centered 2-line summary matching user screenshot
            if (_populationText != null)
                _populationText.text = $"Guests: {guests.Count}/{tavernCap}  |  Quarters: {quartersCap} capacity";

            if (_timerText != null)
            {
                if (guests.Count >= tavernCap)
                    _timerText.text = "Tavern full";
                else if (timer > 0)
                    _timerText.text = $"Next visitor in: {FormatTimer(timer)}";
                else
                    _timerText.text = "Visitor arriving soon...";
            }

            if (_activeTab == TavernTab.Tavern)
            {
                ShowTavernView(guests);
            }
            else
            {
                ShowQuartersView();
            }

            RefreshActionButtons();
        }

        // ── Tab Switching ────────────────────────────────────────────────────────

        public void OnClickTabTavern()
        {
            _activeTab = TavernTab.Tavern;
            _selectedIndex = -1;
            Refresh();
        }

        public void OnClickTabQuarters()
        {
            _activeTab = TavernTab.Quarters;
            _selectedIndex = -1;
            Refresh();
        }

        private void UpdateTabButtons()
        {
            SetTabActive(_tabTavernBtn,   _activeTab == TavernTab.Tavern);
            SetTabActive(_tabQuartersBtn, _activeTab == TavernTab.Quarters);
        }

        private static void SetTabActive(Button btn, bool active)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = active ? UITemporaryTheme.TabActive : UITemporaryTheme.TabInactive;
        }

        // ── Tavern Tab View ──────────────────────────────────────────────────────

        private void ShowTavernView(IReadOnlyList<CharacterSaveData> guests)
        {
            if (_cardContainer == null) return;
            UICardFactory.ClearContainer(_cardContainer);

            UICardFactory.CreateDivider(_cardContainer, "TAVERN VISITORS");

            if (guests.Count == 0)
            {
                _selectedIndex = -1;
                UICardFactory.CreateCard(_cardContainer, "⏳ Waiting for Visitors...", "Upgrading Tavern Capacity or Speed brings new heroes faster!", false, false, null, preferredHeight: 70);
                
                if (_detailText != null)
                    _detailText.text = "No adventurers currently in the Tavern. Check back when the visitor timer completes!";
                return;
            }

            // Clamp selection
            if (_selectedIndex < 0) _selectedIndex = 0;
            else if (_selectedIndex >= guests.Count) _selectedIndex = guests.Count - 1;

            for (int i = 0; i < guests.Count; i++)
            {
                int captured = i;
                var g        = guests[i];
                bool sel     = (i == _selectedIndex);

                string traitInfo = !string.IsNullOrEmpty(g.Trait) ? $" • Trait: {g.Trait}" : "";
                string title    = GetHeroTitle(g.DefinitionId) + $" (Lv.{g.Level})";
                string subtitle = $"Role: {GetRoleDescription(g.DefinitionId)}{traitInfo} • Recruit Cost: Free";

                UICardFactory.CreateCard(_cardContainer, title, subtitle,
                    sel, true, () => SelectIndex(captured),
                    preferredHeight: 70);
            }

            RefreshTavernDetail(guests);
        }

        private void RefreshTavernDetail(IReadOnlyList<CharacterSaveData> guests)
        {
            if (_detailText == null) return;
            if (guests.Count == 0 || _selectedIndex < 0)
            {
                _detailText.text = "Select a guest from the list to see details.";
                return;
            }

            var g = guests[_selectedIndex];
            string title = GetHeroTitle(g.DefinitionId);
            string traitText = !string.IsNullOrEmpty(g.Trait) ? g.Trait : "None";

            int currentRoster = _saveService?.CurrentData?.Characters?.Count ?? 0;
            int quartersCap   = _tavernService?.GetQuartersCapacity() ?? 0;
            bool quartersFull = currentRoster >= quartersCap;

            string statusText = quartersFull 
                ? "❌ Quarters Full! Upgrade Quarters in Quarters tab to recruit more heroes."
                : "✓ Ready to Recruit into Guild Roster!";

            _detailText.text =
                $"⚔️ HERO DOSSIER: {title} (Lv.{g.Level})\n" +
                $"• Specialty: {GetRoleDescription(g.DefinitionId)}\n" +
                $"• Innate Trait: {traitText}\n" +
                $"• Status: {statusText}";
        }

        // ── Quarters Tab View ────────────────────────────────────────────────────

        private void ShowQuartersView()
        {
            if (_cardContainer == null) return;
            UICardFactory.ClearContainer(_cardContainer);

            if (_selectedIndex < 0) _selectedIndex = 0;
            if (_selectedIndex > 2) _selectedIndex = 2;

            long money = _saveService?.CurrentData?.Money ?? 0;

            // Upgrade 1: Quarters
            long priceQ = _tavernService.GetUpgradeQuartersPrice();
            int lvlQ    = _tavernService.GetQuartersLevel();
            int capQ    = _tavernService.GetQuartersCapacity();
            string subQ = $"Level {lvlQ} • Current Roster Space: {capQ} Heroes • Cost: {priceQ}g";
            UICardFactory.CreateCard(_cardContainer, "🏠 Upgrade Quarters Capacity", subQ,
                _selectedIndex == 0, money >= priceQ, () => SelectIndex(0), preferredHeight: 70);

            // Upgrade 2: Tavern Capacity
            long priceC = _tavernService.GetUpgradeTavernCapacityPrice();
            int lvlC    = _tavernService.GetTavernCapacityLevel();
            int capC    = _tavernService.GetTavernCapacity();
            string subC = $"Level {lvlC} • Current Visitor Slots: {capC} Guests • Cost: {priceC}g";
            UICardFactory.CreateCard(_cardContainer, "🍻 Upgrade Tavern Visitor Capacity", subC,
                _selectedIndex == 1, money >= priceC, () => SelectIndex(1), preferredHeight: 70);

            // Upgrade 3: Visitor Speed
            long priceT = _tavernService.GetUpgradeTavernTimePrice();
            int lvlT    = _tavernService.GetTavernTimeLevel();
            long secT   = _tavernService.GetVisitorIntervalSeconds();
            string subT = $"Level {lvlT} • Current Arrival Interval: {FormatTimer(secT)} • Cost: {priceT}g";
            UICardFactory.CreateCard(_cardContainer, "⚡ Upgrade Visitor Arrival Speed", subT,
                _selectedIndex == 2, money >= priceT, () => SelectIndex(2), preferredHeight: 70);

            RefreshQuartersDetail();
        }

        private void RefreshQuartersDetail()
        {
            if (_detailText == null) return;
            long money = _saveService?.CurrentData?.Money ?? 0;

            if (_selectedIndex == 0)
            {
                long price = _tavernService.GetUpgradeQuartersPrice();
                int lvl    = _tavernService.GetQuartersLevel();
                _detailText.text =
                    $"🏠 QUARTERS UPGRADE (Lv.{lvl})\n" +
                    $"• Increases your Guild's max hero roster capacity by +1.\n" +
                    $"• Upgrade Cost: {price}g (You have: {money}g)\n" +
                    $"• Status: {(money >= price ? "✓ Ready to Upgrade!" : "❌ Not enough gold.")}";
            }
            else if (_selectedIndex == 1)
            {
                long price = _tavernService.GetUpgradeTavernCapacityPrice();
                int lvl    = _tavernService.GetTavernCapacityLevel();
                _detailText.text =
                    $"🍻 TAVERN CAPACITY UPGRADE (Lv.{lvl})\n" +
                    $"• Allows more adventurer visitors to wait inside the Tavern at the same time.\n" +
                    $"• Upgrade Cost: {price}g (You have: {money}g)\n" +
                    $"• Status: {(money >= price ? "✓ Ready to Upgrade!" : "❌ Not enough gold.")}";
            }
            else
            {
                long price = _tavernService.GetUpgradeTavernTimePrice();
                int lvl    = _tavernService.GetTavernTimeLevel();
                _detailText.text =
                    $"⚡ VISITOR SPEED UPGRADE (Lv.{lvl})\n" +
                    $"• Reduces the time interval required for new adventurer visitors to arrive.\n" +
                    $"• Upgrade Cost: {price}g (You have: {money}g)\n" +
                    $"• Status: {(money >= price ? "✓ Ready to Upgrade!" : "❌ Not enough gold.")}";
            }
        }

        // ── Action Buttons Management ───────────────────────────────────────────

        private void RefreshActionButtons()
        {
            bool isTavern = (_activeTab == TavernTab.Tavern);

            if (_recruitButton)
            {
                _recruitButton.gameObject.SetActive(isTavern);
                var guests = _tavernService?.GetGuests();
                bool hasSelection = isTavern && guests != null && guests.Count > 0 && _selectedIndex >= 0;
                int currentRoster = _saveService?.CurrentData?.Characters?.Count ?? 0;
                int quartersCap   = _tavernService?.GetQuartersCapacity() ?? 0;
                _recruitButton.interactable = hasSelection && (currentRoster < quartersCap);
            }

            if (_prevButton) _prevButton.gameObject.SetActive(true);
            if (_nextButton) _nextButton.gameObject.SetActive(true);

            // Upgrades action buttons (Shown on Quarters tab or wired)
            if (_upgradeQuartersButton) _upgradeQuartersButton.gameObject.SetActive(!isTavern && _selectedIndex == 0);
            if (_upgradeCapacityButton) _upgradeCapacityButton.gameObject.SetActive(!isTavern && _selectedIndex == 1);
            if (_upgradeTimeButton)     _upgradeTimeButton.gameObject.SetActive(!isTavern && _selectedIndex == 2);

            long money = _saveService?.CurrentData?.Money ?? 0;
            if (_upgradeQuartersButton != null)
                _upgradeQuartersButton.interactable = (money >= _tavernService.GetUpgradeQuartersPrice());
            if (_upgradeCapacityButton != null)
                _upgradeCapacityButton.interactable = (money >= _tavernService.GetUpgradeTavernCapacityPrice());
            if (_upgradeTimeButton != null)
                _upgradeTimeButton.interactable = (money >= _tavernService.GetUpgradeTavernTimePrice());
        }

        public void SelectIndex(int index)
        {
            if (_activeTab == TavernTab.Tavern)
            {
                int count = _tavernService?.GetGuests().Count ?? 0;
                if (count == 0) { _selectedIndex = -1; Refresh(); return; }
                _selectedIndex = Mathf.Clamp(index, 0, count - 1);
            }
            else
            {
                _selectedIndex = Mathf.Clamp(index, 0, 2);
            }
            Refresh();
        }

        public void OnClickSelectNext()     => CycleSelection(1);
        public void OnClickSelectPrevious() => CycleSelection(-1);

        private void CycleSelection(int dir)
        {
            int count = _activeTab == TavernTab.Tavern ? (_tavernService?.GetGuests().Count ?? 0) : 3;
            if (count == 0) return;
            if (_selectedIndex < 0) { _selectedIndex = 0; Refresh(); return; }
            _selectedIndex = ((_selectedIndex + dir) % count + count) % count;
            Refresh();
        }

        // ── Button Handlers ─────────────────────────────────────────────────────

        public void OnClickRecruitSelected()
        {
            if (_tavernService == null) return;
            var guests = _tavernService.GetGuests();
            if (guests.Count == 0 || _selectedIndex < 0) return;

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, guests.Count - 1);
            string recruitedName = GetHeroTitle(guests[_selectedIndex].DefinitionId);

            int currentRoster = _saveService?.CurrentData?.Characters?.Count ?? 0;
            int capacity = _tavernService.GetQuartersCapacity();
            if (currentRoster >= capacity)
            {
                ShowFeedback($"Quarters full ({currentRoster}/{capacity})! Upgrade Quarters.", false);
                return;
            }

            if (_tavernService.RecruitGuest(_selectedIndex, out _))
            {
                ShowFeedback($"Recruited {recruitmentName(recruitedName)}!", success: true);
                _selectedIndex = -1;
            }
            else
            {
                ShowFeedback("Recruitment failed.", success: false);
            }
            Refresh();
        }

        private static string recruitmentName(string title) => title;

        public void OnClickUpgradeQuarters()
        {
            if (_tavernService == null) return;
            long price = _tavernService.GetUpgradeQuartersPrice();
            if (_tavernService.UpgradeQuarters())
            {
                ShowFeedback($"Upgraded Quarters capacity! (Paid {price}g)", true);
            }
            else
            {
                ShowFeedback("Not enough gold to upgrade Quarters.", false);
            }
            Refresh();
        }

        public void OnClickUpgradeTavernCapacity()
        {
            if (_tavernService == null) return;
            long price = _tavernService.GetUpgradeTavernCapacityPrice();
            if (_tavernService.UpgradeTavernCapacity())
            {
                ShowFeedback($"Upgraded Tavern visitor capacity! (Paid {price}g)", true);
            }
            else
            {
                ShowFeedback("Not enough gold to upgrade Capacity.", false);
            }
            Refresh();
        }

        public void OnClickUpgradeTavernTime()
        {
            if (_tavernService == null) return;
            long price = _tavernService.GetUpgradeTavernTimePrice();
            if (_tavernService.UpgradeTavernTime())
            {
                ShowFeedback($"Reduced Visitor arrival time! (Paid {price}g)", true);
            }
            else
            {
                ShowFeedback("Not enough gold to upgrade Speed.", false);
            }
            Refresh();
        }

        // ── Helper Utilities ────────────────────────────────────────────────────

        private static string GetHeroTitle(string defId)
        {
            if (string.IsNullOrEmpty(defId)) return "Adventurer";
            if (defId.Equals("Footman", StringComparison.OrdinalIgnoreCase)) return "⚔️ Footman Warrior";
            if (defId.Equals("Archer", StringComparison.OrdinalIgnoreCase)) return "🏹 Ranger Archer";
            if (defId.Equals("Rogue", StringComparison.OrdinalIgnoreCase)) return "🗡️ Shadow Rogue";
            if (defId.Equals("Apprentice", StringComparison.OrdinalIgnoreCase)) return "🔮 Apprentice Mage";
            if (defId.Equals("LightDisciple", StringComparison.OrdinalIgnoreCase)) return "✨ Light Disciple";
            return $"⚔️ {defId}";
        }

        private static string GetRoleDescription(string defId)
        {
            if (string.IsNullOrEmpty(defId)) return "Frontline Fighter";
            if (defId.Equals("Footman", StringComparison.OrdinalIgnoreCase)) return "Frontline Melee Tank / Defender";
            if (defId.Equals("Archer", StringComparison.OrdinalIgnoreCase)) return "Ranged Physical Damage Specialist";
            if (defId.Equals("Rogue", StringComparison.OrdinalIgnoreCase)) return "High Critical Speed Assassin";
            if (defId.Equals("Apprentice", StringComparison.OrdinalIgnoreCase)) return "Elemental Magic Burst Caster";
            if (defId.Equals("LightDisciple", StringComparison.OrdinalIgnoreCase)) return "Holy Support & Healer";
            return "Guild Adventurer";
        }

        private static string FormatTimer(long seconds)
        {
            if (seconds <= 0) return "00:00";
            long hours = seconds / 3600;
            long mins  = (seconds % 3600) / 60;
            long secs  = seconds % 60;

            if (hours > 0)
                return $"{hours}h {mins}m";
            return $"{mins}m {secs}s";
        }

        private void ShowFeedback(string msg, bool success)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = msg;
                _feedbackText.color = success ? UITemporaryTheme.SuccessColor : UITemporaryTheme.FailureColor;
            }
            Debug.Log($"[TavernScreen] {msg}");
        }
    }
}
