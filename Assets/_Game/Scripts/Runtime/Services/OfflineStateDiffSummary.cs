using System.Collections.Generic;

namespace GuildMaster.Runtime.Services
{
    public struct OfflineStateDiffSummary
    {
        public long AppliedSeconds;
        public bool WasCapped;
        public int MoneyDelta;
        public int GemsDelta;
        public Dictionary<string, int> ItemDeltas;
        public bool HasWorkshopUpdates;
        public bool HasMarketUpdates;
        public bool HasTavernUpdates;
        public List<string> Warnings;
    }
}
