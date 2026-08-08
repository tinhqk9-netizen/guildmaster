using System;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class OfflineProgressService : IOfflineProgressService
    {
        private readonly ISaveService _saveService;
        private readonly ICraftService _craftService;
        private readonly IMerchantService _merchantService;
        private readonly IDungeonService _dungeonService;

        private const long Cap12Hours = 12 * 3600;

        public OfflineProgressService(
            ISaveService saveService,
            ICraftService craftService,
            IMerchantService merchantService,
            IDungeonService dungeonService = null)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _craftService = craftService ?? throw new ArgumentNullException(nameof(craftService));
            _merchantService = merchantService ?? throw new ArgumentNullException(nameof(merchantService));
            _dungeonService = dungeonService;
        }

        public long CalculateOfflineDeltaSeconds(long lastSaveUnix, long currentUnix)
        {
            if (lastSaveUnix <= 0) return 0;
            if (currentUnix <= lastSaveUnix) return 0;

            long delta = currentUnix - lastSaveUnix;
            return Math.Min(delta, Cap12Hours);
        }

        public OfflineProgressResult ApplyOfflineProgress(long currentUnix)
        {
            var data = _saveService.CurrentData;
            var metadata = data?.Metadata;
            if (data == null || metadata == null)
            {
                return new OfflineProgressResult { Success = false, DeltaSeconds = 0, DispatchDeferred = false };
            }

            // LastAccess is the Java runtime marker. Metadata.SaveTimeUnix is kept as a
            // compatibility fallback for older saves that predate LastAccess.
            long lastTimestamp = data.LastAccess > 0 ? data.LastAccess : metadata.SaveTimeUnix;
            long delta = CalculateOfflineDeltaSeconds(lastTimestamp, currentUnix);

            if (delta > 0)
            {
                _craftService.ProgressWorkshop(delta);
                _merchantService.ProgressMarket(delta);
                _dungeonService?.FastForward(delta);
            }

            // Advance timestamps only after all simulations have run. GameLoopService owns
            // the single final Save call; this service deliberately does not write to disk.
            data.LastAccess = currentUnix;
            metadata.SaveTimeUnix = currentUnix;

            return new OfflineProgressResult { Success = true, DeltaSeconds = delta, DispatchDeferred = false };
        }
    }
}
