using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Runtime.UI.Settings
{
    /// <summary>
    /// Settings screen — toggles, version info, and a safe 2-step reset flow.
    /// Step 1: OnClickReset() → shows confirm/cancel buttons.
    /// Step 2: OnClickConfirmReset() → actually resets. OnClickCancelReset() → aborts.
    ///
    /// Source: DialogSettings.java
    /// </summary>
    public class SettingsScreen : UIScreen
    {
        private ISettingsService _settingsService;

        // ── Serialized references (wired by Apply tool) ──────────────────────────
        [SerializeField] private Text    _summaryText;
        [SerializeField] private Text    _detailText;
        [SerializeField] private Button  _toggleSoundButton;
        [SerializeField] private Button  _toggleMusicButton;
        [SerializeField] private Button  _toggleVibrationButton;
        [SerializeField] private Button  _toggleNotificationsButton;
        [SerializeField] private Button  _saveButton;
        [SerializeField] private Button  _resetButton;
        [SerializeField] private Button  _confirmResetButton;
        [SerializeField] private Button  _cancelResetButton;
        [SerializeField] private Text    _feedbackText;

        private bool _pendingReset;

        public void Initialize(ServiceContainer services)
        {
            _settingsService = services.Settings;
        }

        public override void Show()
        {
            base.Show();
            _pendingReset = false;
            Refresh();
        }

        public void Refresh()
        {
            if (_settingsService == null) return;

            if (_summaryText != null)
                _summaryText.text = $"Guild Master  v{_settingsService.GetGameVersion()}";

            if (_detailText != null)
                _detailText.text = BuildSettingsText();

            UpdateConfirmPanel();
        }

        private string BuildSettingsText()
        {
            bool sound         = _settingsService.GetToggle("sound");
            bool music         = _settingsService.GetToggle("music");
            bool vibration     = _settingsService.GetToggle("vibration");
            bool notifications = _settingsService.GetToggle("notifications");
            bool cloud         = _settingsService.GetToggle("cloud");

            return
                $"Language:      {_settingsService.GetLanguage()}\n\n" +
                $"Sound:         {BoolLabel(sound)}\n" +
                $"Music:         {BoolLabel(music)}\n" +
                $"Vibration:     {BoolLabel(vibration)}\n" +
                $"Notifications: {BoolLabel(notifications)}\n" +
                $"Cloud Backup:  {BoolLabel(cloud)}\n\n" +
                $"Version: {_settingsService.GetGameVersion()}";
        }

        private static string BoolLabel(bool value) => value ? "ON" : "OFF";

        private void UpdateConfirmPanel()
        {
            // The confirm/cancel buttons are only visible while pending reset
            if (_confirmResetButton != null)
                _confirmResetButton.gameObject.SetActive(_pendingReset);
            if (_cancelResetButton  != null)
                _cancelResetButton.gameObject.SetActive(_pendingReset);
            if (_resetButton        != null)
                _resetButton.gameObject.SetActive(!_pendingReset);

            if (_feedbackText != null && _pendingReset)
            {
                _feedbackText.text  = "⚠ Reset will ERASE all save data. Are you sure?";
                _feedbackText.color = UITemporaryTheme.WarningColor;
            }
        }

        // ── Toggle actions ───────────────────────────────────────────────────────

        public void OnClickToggleSound()
        {
            if (_settingsService == null) return;
            bool val = !_settingsService.GetToggle("sound");
            _settingsService.SetToggle("sound", val);
            ShowFeedback($"Sound: {BoolLabel(val)}", true);
            Refresh();
        }

        public void OnClickToggleMusic()
        {
            if (_settingsService == null) return;
            bool val = !_settingsService.GetToggle("music");
            _settingsService.SetToggle("music", val);
            ShowFeedback($"Music: {BoolLabel(val)}", true);
            Refresh();
        }

        public void OnClickToggleVibration()
        {
            if (_settingsService == null) return;
            bool val = !_settingsService.GetToggle("vibration");
            _settingsService.SetToggle("vibration", val);
            ShowFeedback($"Vibration: {BoolLabel(val)}", true);
            Refresh();
        }

        public void OnClickToggleNotifications()
        {
            if (_settingsService == null) return;
            bool val = !_settingsService.GetToggle("notifications");
            _settingsService.SetToggle("notifications", val);
            ShowFeedback($"Notifications: {BoolLabel(val)}", true);
            Refresh();
        }

        // ── Save / Reset ─────────────────────────────────────────────────────────

        public void OnClickSave()
        {
            _settingsService?.SaveCurrentState();
            ShowFeedback("Settings saved.", true);
            Refresh();
        }

        /// <summary>Step 1: arms the confirm panel. Does NOT reset anything yet.</summary>
        public void OnClickReset()
        {
            _pendingReset = true;
            Refresh();
        }

        /// <summary>Step 2a: user confirmed reset.</summary>
        public void OnClickConfirmReset()
        {
            if (!_pendingReset) return;
            _settingsService?.ResetToDefault();
            _pendingReset = false;
            ShowFeedback("Save data has been reset.", false);
            Refresh();
        }

        /// <summary>Step 2b: user cancelled reset.</summary>
        public void OnClickCancelReset()
        {
            _pendingReset = false;
            ShowFeedback("Reset cancelled.", true);
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
