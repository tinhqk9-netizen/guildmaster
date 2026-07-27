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

        public void Initialize(ServiceContainer services)
        {
            _questService = services.Quest;
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

        public void OnClickClaimFirst()
        {
            var activeQuests = _questService?.GetActiveQuests();
            if (activeQuests != null && activeQuests.Count > 0)
            {
                var q = activeQuests[0];
                _questService.ClaimReward(q.InstanceId, "war");
                Refresh();
            }
        }

        public void OnClickTestIncrement()
        {
            var activeQuests = _questService?.GetActiveQuests();
            if (activeQuests != null && activeQuests.Count > 0)
            {
                var q = activeQuests[0];
                _questService.Increment(q.InstanceId, 1);
                Refresh();
            }
        }
    }
}
