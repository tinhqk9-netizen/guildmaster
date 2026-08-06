using System.Collections.Generic;
using UnityEngine;

namespace GuildMaster.Runtime.UI.Legacy
{
    /// <summary>
    /// Runtime lookup for the procedurally generated bordered-card sprites
    /// (e.g. "object_border_dim_white") backed by
    /// <see cref="LegacyThemeSpriteCatalog"/> (Assets/Resources/LegacyThemeSpriteCatalog.asset).
    /// Mirrors the lazy-cache pattern used by <see cref="LegacySpriteRegistry"/>.
    /// </summary>
    public static class LegacyThemeSprites
    {
        private const string CatalogResourcePath = "LegacyThemeSpriteCatalog";

        private static Dictionary<string, Sprite> _cache;

        private static Dictionary<string, Sprite> Cache
        {
            get
            {
                if (_cache != null) return _cache;
                _cache = new Dictionary<string, Sprite>();
                var catalog = Resources.Load<LegacyThemeSpriteCatalog>(CatalogResourcePath);
                if (catalog == null)
                {
                    Debug.LogWarning($"[LegacyThemeSprites] Could not load '{CatalogResourcePath}' from Resources. " +
                        "Run Tools/Guild Master/Legacy UI/Build Legacy Theme Assets in the Unity Editor first.");
                    return _cache;
                }
                foreach (var entry in catalog.entries)
                {
                    if (string.IsNullOrEmpty(entry.key) || _cache.ContainsKey(entry.key)) continue;
                    _cache[entry.key] = entry.sprite;
                }
                return _cache;
            }
        }

        /// <summary>Bordered-card sprite by generated name, e.g. "object_border_dim_white". Null if not found.</summary>
        public static Sprite Get(string name) => !string.IsNullOrEmpty(name) && Cache.TryGetValue(name, out var s) ? s : null;

        public static void ClearCache() => _cache = null;
    }
}
