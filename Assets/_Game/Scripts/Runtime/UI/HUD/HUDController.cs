using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.UI.Foundation;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.HUD
{
    public class HUDController : UIScreen
    {
        private ISaveService _saveService;
        private IUIService _uiService;

        [Header("Currency Displays")]
        [SerializeField] private Text _moneyText;
        [SerializeField] private Text _gemsText;

        [Header("Navigation Buttons")]
        [SerializeField] private Button _inventoryButton;
        [SerializeField] private Button _characterButton;
        [SerializeField] private Button _dungeonButton;
        [SerializeField] private Button _craftButton;
        [SerializeField] private Button _merchantButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _tavernButton;
        [SerializeField] private Button _questButton;

        [Header("Foundation UI Components")]
        [SerializeField] private WelcomeModal _welcomeModal;
        [SerializeField] private RecoveryWarningPopup _recoveryPopup;
        [SerializeField] private SaveStatusIndicator _saveIndicator;
        [SerializeField] private OfflineSummaryPopup _offlinePopup;

        public void Initialize(ISaveService saveService, IUIService uiService, OfflineStateDiffSummary offlineSummary)
        {
            _saveService = saveService;
            _uiService = uiService;
            BindButtons();
            RefreshHUD();

            if (_saveIndicator != null) _saveIndicator.Initialize(saveService);
            if (_welcomeModal != null) _welcomeModal.Initialize();
            if (_recoveryPopup != null) _recoveryPopup.Initialize();
            if (_offlinePopup != null) _offlinePopup.Initialize();

            if (saveService.LastLoadStatus == SaveLoadResult.FreshNewGame)
            {
                if (_welcomeModal != null) _welcomeModal.Show();
            }
            else if (saveService.LastLoadStatus == SaveLoadResult.FreshAfterCorruption || saveService.LastLoadStatus == SaveLoadResult.BackupLoaded)
            {
                if (_recoveryPopup != null) _recoveryPopup.ShowWarning(saveService.LastLoadStatus);
            }
            else if (offlineSummary.AppliedSeconds > 0)
            {
                if (_offlinePopup != null) _offlinePopup.ShowSummary(offlineSummary);
            }
        }

        private void BindButtons()
        {
            if (_inventoryButton) _inventoryButton.onClick.AddListener(() => _uiService.ShowScreen(UIScreenId.Inventory));
            if (_characterButton) _characterButton.onClick.AddListener(() => _uiService.ShowScreen(UIScreenId.Character));
            if (_dungeonButton) _dungeonButton.onClick.AddListener(() => _uiService.ShowScreen(UIScreenId.Dungeon));
            if (_craftButton) _craftButton.onClick.AddListener(() => _uiService.ShowScreen(UIScreenId.Craft));
            if (_merchantButton) _merchantButton.onClick.AddListener(() => _uiService.ShowScreen(UIScreenId.Merchant));
            if (_settingsButton) _settingsButton.onClick.AddListener(() => _uiService.ShowScreen(UIScreenId.Settings));
            if (_tavernButton) _tavernButton.onClick.AddListener(() => _uiService.ShowScreen(UIScreenId.Tavern));
            if (_questButton) _questButton.onClick.AddListener(() => _uiService.ShowScreen(UIScreenId.Quest));
        }

        public void RefreshHUD()
        {
            if (_saveService == null) return;
            
            var data = _saveService.CurrentData;
            if (data != null)
            {
                if (_moneyText != null) _moneyText.text = data.Money.ToString();
                if (_gemsText != null) _gemsText.text = data.Gems.ToString();
            }
            else
            {
                // Save is null -> display safe default
                if (_moneyText != null) _moneyText.text = "0";
                if (_gemsText != null) _gemsText.text = "0";
            }
        }

        public override void Show()
        {
            base.Show();
            RefreshHUD();
        }
    }
}
