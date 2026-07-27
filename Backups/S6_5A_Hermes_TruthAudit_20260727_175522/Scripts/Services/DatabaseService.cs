using GuildMaster.Database;
using GuildMaster.Definitions;
using System.Collections.Generic;

namespace GuildMaster.Services
{
    public interface IDatabaseService
    {
        bool TryGet<T>(string id, out T definition) where T : DefinitionBase;
        T GetRequired<T>(string id) where T : DefinitionBase;
        IReadOnlyCollection<T> GetAll<T>() where T : DefinitionBase;
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly GameDatabase _database;

        public DatabaseService(GameDatabase database)
        {
            _database = database ?? throw new System.ArgumentNullException(nameof(database));
        }

        public bool TryGet<T>(string id, out T definition) where T : DefinitionBase
        {
            return _database.TryGet(id, out definition);
        }

        public T GetRequired<T>(string id) where T : DefinitionBase
        {
            return _database.GetRequired<T>(id);
        }

        public IReadOnlyCollection<T> GetAll<T>() where T : DefinitionBase
        {
            return _database.GetAll<T>();
        }
    }
}
