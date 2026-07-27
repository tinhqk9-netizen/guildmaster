using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Runtime.UI.Craft
{
    public class CraftScreen : UIScreen
    {
        private ICraftService _craftService;
        [SerializeField] private Text _statusText;

        public void Initialize(ICraftService craftService)
        {
            _craftService = craftService;
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Refresh()
        {
            if (_craftService == null || _statusText == null) return;

            var queue = _craftService.GetQueue();
            var completed = _craftService.GetCompletedItems();
            int queueCap = _craftService.GetQueueCapacity();

            string content = $"--- WORKSHOP / CRAFT ---\n";
            content += $"Queue: {queue.Count}/{queueCap}\n";
            content += $"Completed Items: {completed.Count}\n\n";

            content += "Active Queue:\n";
            foreach (var q in queue)
            {
                content += $"- {q.DefinitionId} x{q.StackCount} ({q.SecondsPassed}s passed)\n";
            }

            content += "\nCompleted Ready To Claim:\n";
            foreach (var c in completed)
            {
                content += $"- {c.DefinitionId} x{c.StackCount} [ID: {c.InstanceId}]\n";
            }

            _statusText.text = content;
        }

        public void OnClickClaim(string instanceId)
        {
            if (_craftService == null) return;
            if (_craftService.ClaimCompletedCraft(instanceId))
            {
                Refresh();
            }
        }
    }
}
