using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Definitions;

namespace GuildMaster.Database
{
    public class GameDatabase
    {
        private readonly Dictionary<Type, object> _registries = new Dictionary<Type, object>();

        // Register a full collection of typed definitions
        public void RegisterCollection<T>(IEnumerable<T> definitions) where T : DefinitionBase
        {
            var type = typeof(T);
            var dict = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var def in definitions)
            {
                if (string.IsNullOrEmpty(def.id))
                {
                    UnityEngine.Debug.LogWarning($"[GameDatabase] Skipping {type.Name} with null or empty ID.");
                    continue;
                }

                if (!dict.ContainsKey(def.id))
                {
                    dict[def.id] = def;
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[GameDatabase] Duplicate ID found in {type.Name}: {def.id}");
                }
            }

            _registries[type] = dict;
        }

        public bool TryGet<T>(string id, out T definition) where T : DefinitionBase
        {
            definition = null;
            if (string.IsNullOrEmpty(id)) return false;

            if (_registries.TryGetValue(typeof(T), out var registryObj))
            {
                var dict = (Dictionary<string, T>)registryObj;
                return dict.TryGetValue(id, out definition);
            }
            return false;
        }

        public T GetRequired<T>(string id) where T : DefinitionBase
        {
            if (TryGet<T>(id, out var def))
            {
                return def;
            }
            throw new KeyNotFoundException($"Definition of type {typeof(T).Name} with ID '{id}' was not found in GameDatabase.");
        }

        public IReadOnlyCollection<T> GetAll<T>() where T : DefinitionBase
        {
            if (_registries.TryGetValue(typeof(T), out var registryObj))
            {
                var dict = (Dictionary<string, T>)registryObj;
                return dict.Values;
            }
            return Array.Empty<T>();
        }
    }
}
