using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Legacy;

namespace GuildMaster.Runtime.UI.Shell
{
    /// <summary>
    /// Phase 3: App Shell — top-level navigation replacing the flat 8-button HUD with a
    /// legacy-accurate shell: top HUD + currency bar, 4 main tabs (Headquarters / Adventurers /
    /// Dungeons / Raids), a slide-out navigation drawer, and a popup root that blocks input to
    /// everything below it.
    ///
    /// Only reads existing backend state (SaveService.CurrentData.Money / .Gems) for the HUD —
    /// never writes to it, never touches any other service/model. Tab content is a placeholder
    /// (<see cref="TabPlaceholderView"/>) in this phase; drawer items are inert placeholders that
    /// only close the drawer (no dialogs are wired yet — that is explicitly out of Phase 3 scope).
    /// </summary>
    public class AppShellController : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private Text _screenTitleText;
        [SerializeField] private Text _gemsText;
        [SerializeField] private Text _platinumText;
        [SerializeField] private Text _goldText;
        [SerializeField] private Text _silverText;
        [SerializeField] private Text _copperText;
        [SerializeField] private Button _menuButton;

        [Header("Tabs")]
        [SerializeField] private Button[] _tabButtons;      // 0=HQ 1=Adventurers 2=Dungeons 3=Raids
        [SerializeField] private Image[] _tabButtonIcons;   // parallel array, for active/inactive tint
        [SerializeField] private GameObject[] _tabPanels;   // parallel array, the 4 placeholder panels
        [SerializeField] private string[] _tabNames = { "Headquarters", "Adventurers", "Dungeons", "Raids" };

        [Header("Drawer")]
        [SerializeField] private GameObject _drawerRoot;
        [SerializeField] private Button _drawerBackdropButton;
        [SerializeField] private Button[] _drawerItemButtons;

        [Header("Popup")]
        [SerializeField] private RectTransform _popupRoot;
        [SerializeField] private GameObject _popupBackdrop;

        [Header("Phase 4: Headquarters Hub")]
        [SerializeField] private HeadquartersHubController _headquartersHub;

        private ServiceContainer _services;
        private int _activeTabIndex = -1;
        private GameObject _currentPopup;

        public void Initialize(ServiceContainer services)
        {
            _services = services;

            for (int i = 0; i < _tabPanels.Length; i++)
            {
                var view = _tabPanels[i].GetComponent<TabPlaceholderView>();
                view?.SetLabel(_tabNames[i]);
            }

            for (int i = 0; i < _tabButtons.Length; i++)
            {
                int index = i; // capture
                if (_tabButtons[i] != null)
                {
                    _tabButtons[i].onClick.RemoveAllListeners();
                    _tabButtons[i].onClick.AddListener(() => SwitchTab(index));
                }
            }

            if (_menuButton != null)
            {
                _menuButton.onClick.RemoveAllListeners();
                _menuButton.onClick.AddListener(OpenDrawer);
            }

            if (_drawerBackdropButton != null)
            {
                _drawerBackdropButton.onClick.RemoveAllListeners();
                _drawerBackdropButton.onClick.AddListener(CloseDrawer);
            }

            if (_drawerItemButtons != null)
            {
                foreach (var btn in _drawerItemButtons)
                {
                    if (btn == null) continue;
                    btn.onClick.RemoveAllListeners();
                    // Phase 3: drawer items are inert placeholders — no dialogs exist yet for
                    // Shop/Settings/Recall/Messages/FAQ/Bestiary/Achievements/Cloud/Redeem/Community.
                    // Wiring real destinations is Phase 4+ scope.
                    btn.onClick.AddListener(CloseDrawer);
                }
            }

            CloseDrawer();
            ClosePopup();
            SwitchTab(0);
            RefreshHud();

            if (_headquartersHub != null)
            {
                _headquartersHub.Initialize(services, this);
            }

            // Phase 6 is isolated from the shell bootstrap. A roster/UI failure must never
            // prevent Headquarters, HUD, drawer, or bottom navigation from initializing.
            try
            {
                if (_tabPanels != null && _tabPanels.Length > 1 && _tabPanels[1] != null)
                {
                    _tabPanels[1].GetComponent<GuildMaster.Runtime.UI.Character.AdventurersTabController>()?.Setup(services);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Phase6] Adventurers setup isolated from App Shell: {ex}");
            }

            // Phase 7 is isolated in the same way: a dungeon UI failure must not break
            // Headquarters, the shell, or the already-working Adventurers tab.
            try
            {
                if (_tabPanels != null && _tabPanels.Length > 2 && _tabPanels[2] != null)
                {
                    _tabPanels[2].GetComponent<GuildMaster.Runtime.UI.Dungeon.DungeonsTabController>()?.Setup(services);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Phase7] Dungeons setup isolated from App Shell: {ex}");
            }

            // Phase 8 is isolated the same way: a raids UI failure must not break Headquarters,
            // Adventurers, Dungeons, or the shell itself.
            try
            {
                if (_tabPanels != null && _tabPanels.Length > 3 && _tabPanels[3] != null)
                {
                    _tabPanels[3].GetComponent<GuildMaster.Runtime.UI.Raid.RaidsTabController>()?.Setup(services);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Phase8] Raids setup isolated from App Shell: {ex}");
            }

            // Phase 9 is isolated the same way: an auxiliary-screen failure must not break
            // Headquarters, Adventurers, Dungeons, Raids, the drawer, or the shell itself. Unlike
            // Phase 6-8 (which own a pre-existing tab panel), Phase 9 has no scene-authored host,
            // so it creates its own small controller GameObject under the shell at runtime.
            try
            {
                var auxGo = new GameObject("AuxiliaryController");
                auxGo.transform.SetParent(transform, false);
                var aux = auxGo.AddComponent<GuildMaster.Runtime.UI.Auxiliary.AuxiliaryController>();
                aux.Setup(services, this, _drawerRoot);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Phase9] Auxiliary setup isolated from App Shell: {ex}");
            }
        }

        /// <summary>Call after any action that might change Money/Gems to refresh the HUD display.</summary>
        public void RefreshHud()
        {
            if (_services?.Save?.CurrentData == null) return;

            var data = _services.Save.CurrentData;

            if (_gemsText != null) _gemsText.text = data.Gems.ToString();

            var coins = LegacyCurrencyAdapter.FromMoney(data.Money);
            if (_platinumText != null) _platinumText.text = coins.Platinum.ToString();
            if (_goldText != null) _goldText.text = coins.Gold.ToString();
            if (_silverText != null) _silverText.text = coins.Silver.ToString();
            if (_copperText != null) _copperText.text = coins.Copper.ToString();
        }

        // ─── Tabs ────────────────────────────────────────────────────────────

        public void SwitchTab(int index)
        {
            if (index < 0 || index >= _tabPanels.Length || index == _activeTabIndex) return;

            for (int i = 0; i < _tabPanels.Length; i++)
            {
                bool active = i == index;
                if (_tabPanels[i] != null) _tabPanels[i].SetActive(active);

                if (_tabButtonIcons != null && i < _tabButtonIcons.Length && _tabButtonIcons[i] != null)
                {
                    _tabButtonIcons[i].color = active ? LegacyUITheme.BrassBorder : LegacyUITheme.DimWhite;
                }
            }

            _activeTabIndex = index;

            if (_screenTitleText != null && index < _tabNames.Length)
            {
                _screenTitleText.text = _tabNames[index];
            }
        }

        // ─── Drawer ──────────────────────────────────────────────────────────

        public void OpenDrawer()
        {
            if (_drawerRoot != null) _drawerRoot.SetActive(true);
        }

        public void CloseDrawer()
        {
            if (_drawerRoot != null) _drawerRoot.SetActive(false);
        }

        // ─── Popup (blocks interaction below it; only one at a time) ───────────

        /// <summary>
        /// Shows <paramref name="popupInstance"/> under the popup root and blocks input to
        /// everything below. Returns false (and leaves the currently open popup untouched) if a
        /// popup is already open — callers must close the current one first.
        /// </summary>
        public bool OpenPopup(GameObject popupInstance)
        {
            if (popupInstance == null) return false;

            if (_currentPopup != null)
            {
                Debug.LogWarning($"[AppShellController] Refused to open '{popupInstance.name}' — " +
                                  $"'{_currentPopup.name}' is already open. Close it first.");
                return false;
            }

            popupInstance.transform.SetParent(_popupRoot, false);
            popupInstance.SetActive(true);

            // Order matters: push the backdrop to the end FIRST, then push the popup to the end.
            // That guarantees the popup always ends up as the topmost sibling (rendered above,
            // and hit-tested before, the backdrop) — the previous code instead moved the backdrop
            // to the popup's index, which shifted the popup down a slot and left the backdrop on
            // top, silently eating every click (including Close) meant for the popup. See
            // phase_4_headquarters_hub_report.md "Popup Close Fix" for the full root-cause note.
            if (_popupBackdrop != null)
            {
                _popupBackdrop.SetActive(true);
                _popupBackdrop.transform.SetAsLastSibling();
            }
            popupInstance.transform.SetAsLastSibling();

            _currentPopup = popupInstance;
            return true;
        }

        public void ClosePopup()
        {
            if (_currentPopup != null)
            {
                // Every popup instance is freshly Instantiate()'d by its caller on open — destroy
                // it here rather than just deactivating, so closed popups never accumulate as
                // inactive orphans under PopupRoot.
                Destroy(_currentPopup);
                _currentPopup = null;
            }

            if (_popupBackdrop != null) _popupBackdrop.SetActive(false);
        }

        public bool IsPopupOpen => _currentPopup != null;
    }
}
