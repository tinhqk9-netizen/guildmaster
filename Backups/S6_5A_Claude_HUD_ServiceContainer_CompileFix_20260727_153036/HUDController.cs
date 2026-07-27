using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.UI.Core;
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

        public void Initialize(ServiceContainer services, IUIService uiService)
        {
            _saveService = services.Save;
            _uiService = uiService;
            BindButtons();
            RefreshHUD();
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
