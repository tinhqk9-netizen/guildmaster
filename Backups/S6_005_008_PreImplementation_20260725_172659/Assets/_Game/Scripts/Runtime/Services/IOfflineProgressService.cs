using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface IOfflineProgressService
    {
        long CalculateOfflineDeltaSeconds(long lastSaveUnix, long currentUnix);
        OfflineProgressResult ApplyOfflineProgress(long currentUnix);
    }
}
