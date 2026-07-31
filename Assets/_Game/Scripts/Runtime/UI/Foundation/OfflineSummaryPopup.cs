using System.Text;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Runtime.UI.Foundation
{
    public class OfflineSummaryPopup : MonoBehaviour
    {
        [SerializeField] private Text _timeText;
        [SerializeField] private Text _detailsText;
        [SerializeField] private Button _continueButton;

        public void Initialize()
        {
            _continueButton.onClick.RemoveAllListeners();
            _continueButton.onClick.AddListener(CloseModal);
        }

        public void ShowSummary(OfflineStateDiffSummary summary)
        {
            if (summary.AppliedSeconds <= 0) return;

            if (_timeText != null)
            {
                int hours = (int)(summary.AppliedSeconds / 3600);
                int minutes = (int)((summary.AppliedSeconds % 3600) / 60);
                
                string timeStr = $"You were away for {hours} hours and {minutes} minutes.";
                if (summary.WasCapped) timeStr += " (Max 12 hours reached)";
                _timeText.text = timeStr;
            }

            if (_detailsText != null)
            {
                StringBuilder sb = new StringBuilder();
                if (summary.MoneyDelta > 0) sb.AppendLine($"+{summary.MoneyDelta} Gold");
                if (summary.GemsDelta > 0) sb.AppendLine($"+{summary.GemsDelta} Gems");
                
                foreach (var item in summary.ItemDeltas)
                {
                    if (item.Value > 0)
                        sb.AppendLine($"+{item.Value} {item.Key}");
                    else
                        sb.AppendLine($"{item.Value} {item.Key}");
                }

                if (summary.HasWorkshopUpdates || summary.HasMarketUpdates || summary.HasTavernUpdates)
                {
                    sb.AppendLine("\nCheck your Workshop and Market for pending updates!");
                }

                _detailsText.text = sb.ToString();
            }

            gameObject.SetActive(true);
        }

        private void CloseModal()
        {
            gameObject.SetActive(false);
        }
    }
}
