using System.Collections.Generic;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class OfflineProgressSummaryBuilder
    {
        private readonly ISaveService _saveService;

        public OfflineProgressSummaryBuilder(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public OfflineStateDiffSummary BuildSummary(System.Action applyAction)
        {
            if (_saveService.CurrentData == null)
            {
                return new OfflineStateDiffSummary();
            }

            long currentUnix = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long lastAccess = _saveService.CurrentData.LastAccess;
            long elapsedSeconds = 0;

            if (lastAccess > 0 && currentUnix > lastAccess)
            {
                elapsedSeconds = currentUnix - lastAccess;
            }
            long appliedSeconds = System.Math.Min(elapsedSeconds, 12 * 3600);

            // 1. Snapshot Before
            long beforeMoney = _saveService.CurrentData.Money;
            long beforeGems = _saveService.CurrentData.Gems;
            
            var beforeItems = new Dictionary<string, int>();
            foreach (var item in _saveService.CurrentData.Items)
            {
                if (beforeItems.ContainsKey(item.DefinitionId))
                    beforeItems[item.DefinitionId] += item.StackCount;
                else
                    beforeItems[item.DefinitionId] = item.StackCount;
            }

            int beforeWorkshop = _saveService.CurrentData.CompletedWorkshopItems.Count;
            int beforeMarket = _saveService.CurrentData.SoldMarketItems.Count;

            // 2. Apply Progress
            applyAction?.Invoke();

            if (appliedSeconds <= 0)
            {
                return new OfflineStateDiffSummary { AppliedSeconds = 0 };
            }

            // 3. Snapshot After
            long afterMoney = _saveService.CurrentData.Money;
            long afterGems = _saveService.CurrentData.Gems;

            var afterItems = new Dictionary<string, int>();
            foreach (var item in _saveService.CurrentData.Items)
            {
                if (afterItems.ContainsKey(item.DefinitionId))
                    afterItems[item.DefinitionId] += item.StackCount;
                else
                    afterItems[item.DefinitionId] = item.StackCount;
            }

            int afterWorkshop = _saveService.CurrentData.CompletedWorkshopItems.Count;
            int afterMarket = _saveService.CurrentData.SoldMarketItems.Count;

            // 4. Calculate Diff
            var summary = new OfflineStateDiffSummary
            {
                AppliedSeconds = appliedSeconds,
                WasCapped = elapsedSeconds > 12 * 3600,
                MoneyDelta = (int)(afterMoney - beforeMoney),
                GemsDelta = (int)(afterGems - beforeGems),
                ItemDeltas = new Dictionary<string, int>(),
                HasWorkshopUpdates = afterWorkshop > beforeWorkshop,
                HasMarketUpdates = afterMarket > beforeMarket,
                HasTavernUpdates = false,
                Warnings = new List<string>()
            };

            foreach (var kvp in afterItems)
            {
                string defId = kvp.Key;
                int afterAmount = kvp.Value;
                int beforeAmount = beforeItems.ContainsKey(defId) ? beforeItems[defId] : 0;
                
                int delta = afterAmount - beforeAmount;
                if (delta != 0)
                {
                    summary.ItemDeltas[defId] = delta;
                }
            }

            foreach (var kvp in beforeItems)
            {
                if (!afterItems.ContainsKey(kvp.Key))
                {
                    // Item was completely removed
                    summary.ItemDeltas[kvp.Key] = -kvp.Value;
                }
            }

            return summary;
        }
    }
}
