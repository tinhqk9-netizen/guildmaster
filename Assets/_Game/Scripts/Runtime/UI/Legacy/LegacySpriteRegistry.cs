using System.Collections.Generic;
using UnityEngine;

namespace GuildMaster.Runtime.UI.Legacy
{
    /// <summary>
    /// Phase 1 (Legacy UI reconstruction): loads legacy sprites by their original decompiled
    /// filename (e.g. "unit_footman", "doctrine_of_war", "gem", "bottom_nav_castle"). This is the
    /// ONLY job of this class — it does not touch gameplay services, models, save data, or any
    /// existing UI screen. Nothing in the game calls this yet; it is a foundational utility for
    /// the Phase 2+ UI rebuild.
    ///
    /// Backed by <see cref="LegacySpriteCatalog"/> (Assets/Resources/LegacySpriteCatalog.asset),
    /// built by "Tools/Guild Master/Legacy UI/Import Legacy Assets". Lazily loaded once, then
    /// cached in a dictionary for O(1) lookups.
    /// </summary>
    public static class LegacySpriteRegistry
    {
        private const string CatalogResourcePath = "LegacySpriteCatalog";

        private static Dictionary<string, Sprite> _cache;

        // Fallback index keyed by the catalog key with underscores stripped and lower-cased, e.g.
        // "absolute_zero" -> "absolutezero". Some legacy data sources (notably the decompiled
        // Recipes.java enum -> recipes.json pipeline) emit item identifiers with underscores
        // already removed (recipe.OutputItemId "absolutezero" vs. the real item id/sprite key
        // "absolute_zero"). Rather than guess word boundaries to reconstruct the "correct" id
        // (ambiguous and risky), this index lets a lookup match the *unique* catalog entry whose
        // normalized form equals the query — verified collision-free across the full legacy art set
        // at audit time (see Docs/Legacy_Audit/workshop_recipe_asset_audit.md). Entries that would
        // collide are intentionally omitted so this fallback never guesses between two candidates.
        private static Dictionary<string, Sprite> _normalizedCache;
        private static readonly HashSet<string> _loggedMissing = new HashSet<string>();

        /// <summary>True once the catalog has been loaded (successfully or not) at least once.</summary>
        public static bool IsInitialized => _cache != null;

        /// <summary>Number of sprites currently known to the registry (0 if the catalog failed to load).</summary>
        public static int Count => _cache?.Count ?? 0;

        private static Dictionary<string, Sprite> Cache
        {
            get
            {
                if (_cache != null) return _cache;

                _cache = new Dictionary<string, Sprite>();
                _normalizedCache = new Dictionary<string, Sprite>();
                var normalizedSeen = new HashSet<string>();
                var normalizedAmbiguous = new HashSet<string>();
                var catalog = Resources.Load<LegacySpriteCatalog>(CatalogResourcePath);

                if (catalog == null)
                {
                    Debug.LogWarning(
                        $"[LegacySpriteRegistry] Could not load '{CatalogResourcePath}' from Resources. " +
                        "Run Tools/Guild Master/Legacy UI/Import Legacy Assets in the Unity Editor first.");
                    return _cache;
                }

                foreach (var entry in catalog.entries)
                {
                    if (string.IsNullOrEmpty(entry.key))
                    {
                        continue;
                    }

                    if (_cache.ContainsKey(entry.key))
                    {
                        Debug.LogWarning($"[LegacySpriteRegistry] Duplicate key '{entry.key}' in catalog — keeping first entry.");
                        continue;
                    }

                    _cache[entry.key] = entry.sprite;

                    string normalized = Normalize(entry.key);
                    if (normalizedSeen.Contains(normalized))
                    {
                        // More than one catalog key normalizes to the same form — ambiguous, so
                        // never use it as a fallback match (avoids picking the wrong item's icon).
                        normalizedAmbiguous.Add(normalized);
                        continue;
                    }
                    normalizedSeen.Add(normalized);
                    _normalizedCache[normalized] = entry.sprite;
                }

                foreach (string ambiguous in normalizedAmbiguous)
                {
                    _normalizedCache.Remove(ambiguous);
                }

                return _cache;
            }
        }

        private static string Normalize(string key) => key.Replace("_", string.Empty).ToLowerInvariant();

        /// <summary>
        /// Looks up a sprite by its exact original legacy filename (no extension), e.g.
        /// "unit_footman", "gem", "doctrine_of_war", "bottom_nav_castle".
        /// Returns null and logs a warning (once per missing key) if not found.
        /// </summary>
        public static Sprite GetSprite(string legacyName)
        {
            if (string.IsNullOrEmpty(legacyName))
            {
                Debug.LogWarning("[LegacySpriteRegistry] GetSprite called with null/empty name.");
                return null;
            }

            if (Cache.TryGetValue(legacyName, out Sprite sprite))
            {
                if (sprite == null)
                {
                    LogMissingOnce(legacyName, "catalog entry exists but Sprite reference is null");
                }
                return sprite;
            }

            // Force Cache init (no-op if already loaded) so _normalizedCache is populated.
            _ = Cache;
            if (_normalizedCache != null && _normalizedCache.TryGetValue(Normalize(legacyName), out Sprite normalizedSprite))
            {
                return normalizedSprite;
            }

            LogMissingOnce(legacyName, "no catalog entry for this name");
            return null;
        }

        /// <summary>Convenience: character/enemy portrait — legacy filename pattern "unit_&lt;id&gt;".</summary>
        public static Sprite GetUnitSprite(string id) => GetSprite($"unit_{id}");

        /// <summary>
        /// Convenience: pet portrait. PetSaveData stores the canonical definition id (for
        /// example "beetle"), while the legacy drawable/catalog key is "pet_beetle". Accepts
        /// either form so callers do not have to duplicate the prefix rule.
        /// </summary>
        public static Sprite GetPetSprite(string idOrImageId)
        {
            if (string.IsNullOrEmpty(idOrImageId)) return null;
            string key = idOrImageId.StartsWith("pet_", System.StringComparison.OrdinalIgnoreCase)
                ? idOrImageId
                : "pet_" + idOrImageId;
            Sprite sprite = GetSprite(key);
            return sprite ?? (string.Equals(key, idOrImageId, System.StringComparison.Ordinal) ? null : GetSprite(idOrImageId));
        }

        /// <summary>Convenience: doctrine icon — legacy filename pattern "doctrine_&lt;id&gt;" (callers pass the raw id, without the "doctrine_" prefix).</summary>
        public static Sprite GetDoctrineSprite(string id) => GetSprite($"doctrine_{id}");

        /// <summary>Convenience: item sprite — legacy item sprite names are used as-is (from item.getIdImage()).</summary>
        public static Sprite GetItemSprite(string itemSpriteName) => GetSprite(itemSpriteName);

        /// <summary>
        /// Resolves a Legacy pet egg icon from either the item's image key (for example
        /// "egg_avian") or the canonical item definition id ("avian_egg"). Legacy resource
        /// names put the family before the suffix in the item id but after the prefix in the
        /// drawable key, so this keeps that naming rule in one generic resolver.
        /// </summary>
        public static Sprite GetEggSprite(string itemIdOrImageId)
        {
            if (string.IsNullOrEmpty(itemIdOrImageId)) return null;

            string legacyKey = itemIdOrImageId;
            const string suffix = "_egg";
            if (itemIdOrImageId.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase))
            {
                string family = itemIdOrImageId.Substring(0, itemIdOrImageId.Length - suffix.Length);
                legacyKey = "egg_" + family;
            }

            Sprite sprite = GetSprite(legacyKey);
            return sprite ?? (string.Equals(legacyKey, itemIdOrImageId, System.StringComparison.Ordinal)
                ? null
                : GetSprite(itemIdOrImageId));
        }

        /// <summary>Convenience: enemy sprite — legacy enemy sprite names are used as-is (from Enemy.getImageId()).</summary>
        public static Sprite GetEnemySprite(string enemySpriteName) => GetSprite(enemySpriteName);

        /// <summary>Convenience: currency icon — e.g. "gem", "coin_platinum", "coin_gold", "coin_silver", "coin_copper".</summary>
        public static Sprite GetCurrencySprite(string currencyName) => GetSprite(currencyName);

        /// <summary>Convenience: navigation drawer icon — e.g. "drawer_icon_settings", "bottom_nav_castle".</summary>
        public static Sprite GetDrawerIconSprite(string drawerIconName) => GetSprite(drawerIconName);

        /// <summary>Forces the catalog to reload from Resources on next access. Editor/testing use only.</summary>
        public static void ClearCache()
        {
            _cache = null;
            _normalizedCache = null;
            _loggedMissing.Clear();
        }

        private static void LogMissingOnce(string key, string reason)
        {
            if (_loggedMissing.Add(key))
            {
                Debug.LogWarning($"[LegacySpriteRegistry] Missing sprite '{key}' — {reason}.");
            }
        }
    }
}
