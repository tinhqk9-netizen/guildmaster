using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Runtime.UI.Quest
{
    public class QuestScreen : UIScreen
    {
        private IQuestService _questService;
        [SerializeField] private Text _statusText;

        public void Initialize(IQuestService questService)
        {
            _questService = questService;
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Refresh()
        {
            if (_questService == null || _statusText == null) return;

            var activeQuests = _questService.GetActiveQuests();

            string content = $"--- QUESTS ({activeQuests.Count}) ---\n\n";

            foreach (var q in activeQuests)
            {
                content += $"- {q.Definition?.id} ({q.Progress}/{q.Definition?.TargetProgress}) [{q.State}]\n";
            }

            _statusText.text = content;
        }

        public void OnClickClaim(string questInstanceId, string doctrineName)
        {
            if (_questService == null) return;
            if (_questService.ClaimReward(questInstanceId, doctrineName))
            {
                Refresh();
            }
        }
    }
}
