using System;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISaveService _saveService;

        public SettingsService(ISaveService saveService)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        }

        public bool GetToggle(string key)
        {
            if (string.IsNullOrEmpty(key)) return true;
            var data = _saveService.CurrentData;
            switch (key.ToLowerInvariant())
            {
                case "sound": return data.SettingsSound;
                case "music": return data.SettingsMusic;
                case "vibration": return data.SettingsVibration;
                case "notifications": return data.SettingsNotifications;
                case "cloud": return data.SettingsCloud;
                default: return true;
            }
        }

        public void SetToggle(string key, bool value)
        {
            if (string.IsNullOrEmpty(key)) return;
            var data = _saveService.CurrentData;
            switch (key.ToLowerInvariant())
            {
                case "sound": data.SettingsSound = value; break;
                case "music": data.SettingsMusic = value; break;
                case "vibration": data.SettingsVibration = value; break;
                case "notifications": data.SettingsNotifications = value; break;
                case "cloud": data.SettingsCloud = value; break;
            }
        }

        public string GetLanguage()
        {
            return _saveService.CurrentData.SettingsLanguage ?? "en";
        }

        public void SetLanguage(string lang)
        {
            if (string.IsNullOrEmpty(lang)) return;
            _saveService.CurrentData.SettingsLanguage = lang;
        }
    }
}
