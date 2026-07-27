using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;

namespace GuildMaster.Runtime.UI.Popup
{
    public class PopupScreen : UIScreen
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _messageText;
        [SerializeField] private Button _okButton;

        private void Awake()
        {
            if (_okButton != null)
            {
                _okButton.onClick.AddListener(Hide);
            }
        }

        public void ShowMessage(string title, string message)
        {
            if (_titleText != null) _titleText.text = title;
            if (_messageText != null) _messageText.text = message;
            Show();
        }

        public void ShowDeferred()
        {
            ShowMessage("Deferred", "This feature is currently deferred and will be implemented in future sprints.");
        }
    }
}
