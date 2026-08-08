using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    /// <summary>Legacy bestiary discovery boundary: only enemy ids are persisted.</summary>
    public interface IBestiaryService
    {
        IReadOnlyCollection<string> GetSeenEnemyIds();
        bool IsSeen(string enemyId);
        void MarkSeen(string enemyId);
    }

    public sealed class BestiaryService : IBestiaryService
    {
        private readonly ISaveService _saveService;
        private readonly GameDatabase _database;

        public BestiaryService(ISaveService saveService, GameDatabase database)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _saveService.CurrentData?.NormalizeAfterLoad();
        }

        public IReadOnlyCollection<string> GetSeenEnemyIds()
        {
            var ids = _saveService.CurrentData?.SeenEnemyIds;
            return ids == null
                ? Array.Empty<string>()
                : ids.Where(id => !string.IsNullOrEmpty(id)).Distinct(StringComparer.OrdinalIgnoreCase).ToList().AsReadOnly();
        }

        public bool IsSeen(string enemyId)
        {
            return !string.IsNullOrEmpty(enemyId) &&
                   _saveService.CurrentData?.SeenEnemyIds?.Any(id => string.Equals(id, enemyId, StringComparison.OrdinalIgnoreCase)) == true;
        }

        public void MarkSeen(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId) || !_database.TryGet<EnemyDefinition>(enemyId, out _)) return;
            var data = _saveService.CurrentData;
            if (data == null) return;
            if (data.SeenEnemyIds == null) data.SeenEnemyIds = new List<string>();
            if (!data.SeenEnemyIds.Any(id => string.Equals(id, enemyId, StringComparison.OrdinalIgnoreCase)))
                data.SeenEnemyIds.Add(enemyId);
        }
    }
}
