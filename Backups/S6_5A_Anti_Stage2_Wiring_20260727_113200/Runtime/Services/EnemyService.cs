using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Core;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface IEnemyService
    {
        EnemyRuntime CreateEnemy(string definitionId);
    }

    public class EnemyService : IEnemyService
    {
        private readonly GameDatabase _gameDatabase;
        private readonly IInstanceIdGenerator _idGenerator;

        public EnemyService(GameDatabase gameDatabase, IInstanceIdGenerator idGenerator)
        {
            _gameDatabase = gameDatabase;
            _idGenerator = idGenerator;
        }

        public EnemyRuntime CreateEnemy(string definitionId)
        {
            if (!_gameDatabase.TryGet(definitionId, out EnemyDefinition definition))
            {
                return null;
            }

            string id = _idGenerator.GenerateInstanceId("enemy_");
            return new EnemyRuntime(id, definition);
        }
    }
}
