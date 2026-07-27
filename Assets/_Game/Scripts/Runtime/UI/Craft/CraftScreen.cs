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

        private GuildMaster.Database.GameDatabase _database;
        
        public void Initialize(ServiceContainer services)
        {
            _craftService = services.Craft;
            _database = services.Database;
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

        public void OnClickCraftFirstAvailable()
        {
            if (_craftService == null || _database == null) return;
            var recipes = _database.GetAll<GuildMaster.Definitions.RecipeDefinition>();
            if (recipes != null && recipes.Count > 0)
            {
                foreach (var r in recipes)
                {
                    if (_craftService.CanCraft(r.id).Success)
                    {
                        _craftService.TryStartCraft(r.id);
                        break;
                    }
                }
            }
            Refresh();
        }

        public void OnClickProgressTime()
        {
            _craftService?.ProgressWorkshop(3600); // 1 hour
            Refresh();
        }

        public void OnClickClaimFirst()
        {
            var completed = _craftService?.GetCompletedItems();
            if (completed != null && completed.Count > 0)
            {
                _craftService.ClaimCompletedCraft(completed[0].InstanceId);
                Refresh();
            }
        }
    }
}
