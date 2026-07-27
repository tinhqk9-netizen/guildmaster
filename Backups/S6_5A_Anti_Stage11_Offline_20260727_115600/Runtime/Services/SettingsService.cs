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
                case "colorblind": return data.SettingColorblindMode;
                case "autoopendetail": return data.SettingAutoOpenDungeonDetail;
                case "confirmretreat": return data.SettingConfirmRetreat;
                case "confirmswap": return data.SettingConfirmSwap;
                case "confirmupgrade": return data.SettingConfirmUpgrade;
                case "craftmax": return data.SettingCraftMaxAmount;
                case "sellmax": return data.SettingSellMaxAmount;
                case "verboselogs": return data.SettingVerboseLogs;
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
                case "colorblind": data.SettingColorblindMode = value; break;
                case "autoopendetail": data.SettingAutoOpenDungeonDetail = value; break;
                case "confirmretreat": data.SettingConfirmRetreat = value; break;
                case "confirmswap": data.SettingConfirmSwap = value; break;
                case "confirmupgrade": data.SettingConfirmUpgrade = value; break;
                case "craftmax": data.SettingCraftMaxAmount = value; break;
                case "sellmax": data.SettingSellMaxAmount = value; break;
                case "verboselogs": data.SettingVerboseLogs = value; break;
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

        public bool SaveCurrentState()
        {
            return _saveService.Save(out _);
        }

        public void ResetToDefault()
        {
            var data = _saveService.CurrentData;
            data.SettingsSound = true;
            data.SettingsMusic = true;
            data.SettingsVibration = true;
            data.SettingsNotifications = true;
            data.SettingsCloud = true;
            data.SettingColorblindMode = false;
            data.SettingAutoOpenDungeonDetail = true;
            data.SettingConfirmRetreat = true;
            data.SettingConfirmSwap = true;
            data.SettingConfirmUpgrade = true;
            data.SettingCraftMaxAmount = false;
            data.SettingSellMaxAmount = false;
            data.SettingVerboseLogs = false;
            data.SettingsLanguage = "en";
        }

        public string GetGameVersion()
        {
            return _saveService.CurrentData.Metadata?.GameVersion ?? "2.147";
        }
    }
}
