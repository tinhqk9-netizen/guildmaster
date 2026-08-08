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
        private readonly IQuestService _questService;
        private readonly IOfflineProgressService _offlineProgressService;

        private int _checks = 0;

        public GameLoopService(
            ISaveService saveService, 
            ITavernService tavernService, 
            IMerchantService merchantService, 
            ICraftService craftService, 
            IDungeonService dungeonService,
            IQuestService questService,
            IOfflineProgressService offlineProgressService = null)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _tavernService = tavernService ?? throw new ArgumentNullException(nameof(tavernService));
            _merchantService = merchantService ?? throw new ArgumentNullException(nameof(merchantService));
            _craftService = craftService ?? throw new ArgumentNullException(nameof(craftService));
            _dungeonService = dungeonService ?? throw new ArgumentNullException(nameof(dungeonService));
            _questService = questService ?? throw new ArgumentNullException(nameof(questService));
            _offlineProgressService = offlineProgressService ?? new OfflineProgressService(saveService, craftService, merchantService, dungeonService);
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
            var offlineResult = _offlineProgressService.ApplyOfflineProgress(currentUnix);
            if (!offlineResult.Success) return;

            _tavernService.ProgressVisitorTime(offlineResult.DeltaSeconds);

            _questService.CheckAndTriggerWeeklyQuests(currentUnix);

            _merchantService.ProcessScheduledRefreshes(currentUnix);
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
            _merchantService.ProcessScheduledRefreshes(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        private void Tick60()
        {
            _checks++;
            if (_checks >= 60)
            {
                _checks = 0;
                long currentUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                _questService.CheckAndTriggerWeeklyQuests(currentUnix);
            }
        }
    }
}
