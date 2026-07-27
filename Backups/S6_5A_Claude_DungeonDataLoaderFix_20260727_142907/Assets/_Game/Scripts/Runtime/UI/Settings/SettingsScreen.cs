using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Runtime.UI.Settings
{
    public class SettingsScreen : UIScreen
    {
        private ISettingsService _settingsService;
        [SerializeField] private Text _statusText;

        public void Initialize(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Refresh()
        {
            if (_settingsService == null || _statusText == null) return;

            string content = $"--- SETTINGS (v{_settingsService.GetGameVersion()}) ---\n";
            content += $"Language: {_settingsService.GetLanguage()}\n";
            content += $"Sound: {_settingsService.GetToggle("sound")}\n";
            content += $"Music: {_settingsService.GetToggle("music")}\n";
            content += $"Vibration: {_settingsService.GetToggle("vibration")}\n";
            content += $"Notifications: {_settingsService.GetToggle("notifications")}\n";
            content += $"Cloud: {_settingsService.GetToggle("cloud")}\n";

            _statusText.text = content;
        }

        public void OnClickSave()
        {
            _settingsService?.SaveCurrentState();
        }

        public void OnClickReset()
        {
            _settingsService?.ResetToDefault();
            Refresh();
        }
    }
}
