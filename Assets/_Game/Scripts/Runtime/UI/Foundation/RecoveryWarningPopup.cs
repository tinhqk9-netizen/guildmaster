using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.UI.Foundation
{
    public class RecoveryWarningPopup : MonoBehaviour
    {
        [SerializeField] private Text _warningText;
        [SerializeField] private Button _acknowledgeButton;

        public void Initialize()
        {
            _acknowledgeButton.onClick.RemoveAllListeners();
            _acknowledgeButton.onClick.AddListener(CloseModal);
        }

        public void ShowWarning(SaveLoadResult loadStatus)
        {
            if (loadStatus == SaveLoadResult.BackupLoaded)
            {
                if (_warningText != null) 
                    _warningText.text = "Your main save file was corrupted. We have restored your progress from the backup file.";
            }
            else if (loadStatus == SaveLoadResult.FreshAfterCorruption)
            {
                if (_warningText != null) 
                    _warningText.text = "Both main and backup save files were corrupted or missing. A new game has been started.";
            }
            
            gameObject.SetActive(true);
        }

        private void CloseModal()
        {
            gameObject.SetActive(false);
        }
    }
}
