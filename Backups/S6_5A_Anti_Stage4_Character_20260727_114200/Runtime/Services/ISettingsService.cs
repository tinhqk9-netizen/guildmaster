namespace GuildMaster.Runtime.Services
{
    public interface ISettingsService
    {
        bool GetToggle(string key);
        void SetToggle(string key, bool value);
        string GetLanguage();
        void SetLanguage(string lang);
    }
}
