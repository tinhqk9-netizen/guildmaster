using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuildMaster.Runtime.UI.Legacy
{
    /// <summary>
    /// Serialized data asset holding the procedurally generated bordered-card sprites
    /// (e.g. "object_border_dim_white") built by
    /// "Tools/Guild Master/Legacy UI/Build Legacy Theme Assets" from LegacyThemeBuilder.
    ///
    /// This is a separate catalog from <see cref="LegacySpriteCatalog"/> (which is rebuilt from
    /// scratch by the decompiled-asset importer) so the two generators never clobber each other.
    /// Lives at Assets/Resources/LegacyThemeSpriteCatalog.asset so
    /// <see cref="LegacyThemeSprites"/> can load it at runtime via Resources.Load.
    /// </summary>
    [CreateAssetMenu(fileName = "LegacyThemeSpriteCatalog", menuName = "GuildMaster/Legacy UI/Legacy Theme Sprite Catalog")]
    public class LegacyThemeSpriteCatalog : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string key;
            public Sprite sprite;
        }

        public List<Entry> entries = new List<Entry>();
    }
}
