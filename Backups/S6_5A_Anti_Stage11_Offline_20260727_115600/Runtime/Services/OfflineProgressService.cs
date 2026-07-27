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

        private const long Cap12Hours = 12 * 3600;

        public OfflineProgressService(ISaveService saveService, ICraftService craftService, IMerchantService merchantService)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _craftService = craftService ?? throw new ArgumentNullException(nameof(craftService));
            _merchantService = merchantService ?? throw new ArgumentNullException(nameof(merchantService));
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
            var metadata = _saveService.CurrentData?.Metadata;
            if (metadata == null) 
            {
                return new OfflineProgressResult { Success = false, DeltaSeconds = 0, DispatchDeferred = false };
            }

            long delta = CalculateOfflineDeltaSeconds(metadata.SaveTimeUnix, currentUnix);
            
            // Save new last access time
            metadata.SaveTimeUnix = currentUnix;

            if (delta > 0)
            {
                // Dispatch to safe services (Craft and Merchant queues)
                _craftService.ProgressWorkshop(delta);
                _merchantService.ProgressMarket(delta);

                // Dungeon tick deferred: No safe background dungeon loop implemented yet.
                // Combat / Quest offline logic deferred.
                
                return new OfflineProgressResult 
                { 
                    Success = true, 
                    DeltaSeconds = delta, 
                    DispatchDeferred = true 
                };
            }

            return new OfflineProgressResult { Success = true, DeltaSeconds = 0, DispatchDeferred = false };
        }
    }
}
