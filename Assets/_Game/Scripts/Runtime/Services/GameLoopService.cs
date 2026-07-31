using System;
using System.Collections.Generic;
using UnityEngine;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class GameLoopService : IGameLoopService
    {
        private readonly ISaveService _saveService;
        private readonly ITavernService _tavernService;
        private readonly IMerchantService _merchantService;
        private readonly ICraftService _craftService;
        private readonly IDungeonService _dungeonService;

        private const long Cap12Hours = 12 * 3600;
        private int _checks = 0;

        public GameLoopService(
            ISaveService saveService, 
            ITavernService tavernService, 
            IMerchantService merchantService, 
            ICraftService craftService, 
            IDungeonService dungeonService)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _tavernService = tavernService ?? throw new ArgumentNullException(nameof(tavernService));
            _merchantService = merchantService ?? throw new ArgumentNullException(nameof(merchantService));
            _craftService = craftService ?? throw new ArgumentNullException(nameof(craftService));
            _dungeonService = dungeonService ?? throw new ArgumentNullException(nameof(dungeonService));
        }

        public void Initialize()
        {
            ProcessOfflineCatchup();
        }

        public void ProcessOfflineCatchup()
        {
            var data = _saveService.CurrentData;
            if (data == null) return;

            long currentUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long lastAccess = data.LastAccess;
            
            long elapsedSeconds = 1;
            if (lastAccess > 0 && currentUnix > lastAccess)
            {
                elapsedSeconds = currentUnix - lastAccess;
            }

            long jMax = Math.Max(1L, Math.Min(Cap12Hours, elapsedSeconds));

            _tavernService.ProgressVisitorTime(jMax);
            _merchantService.ProgressMarket(jMax);
            _craftService.ProgressWorkshop(jMax);

            // Runs the same per-second tick logic as calling TickAll() jMax times, but persists
            // dungeon state exactly once at the end instead of once per elapsed second (B1 fix).
            _dungeonService.FastForward(jMax);

            data.LastAccess = currentUnix;
            _saveService.Save(out _);
        }

        public void TickRuntime()
        {
            var data = _saveService.CurrentData;
            if (data != null)
            {
                data.LastAccess = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            _tavernService.ProgressVisitorTime(1);
            _merchantService.ProgressMarket(1);
            _craftService.ProgressWorkshop(1);
            
            Tick60();

            _dungeonService.TickAll();
        }

        private void Tick60()
        {
            _checks++;
            if (_checks >= 60)
            {
                _checks = 0;
                // Hourly/Daily resets would go here
            }
        }
    }
}
