using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuildMaster.Runtime.UI.Legacy
{
    /// <summary>
    /// Serialized data asset holding direct Sprite references for every legacy asset imported
    /// under Assets/_Game/Art/Legacy/. Built by the
    /// "Tools/Guild Master/Legacy UI/Import Legacy Assets" Editor tool — never edited by hand.
    ///
    /// Lives at Assets/Resources/LegacySpriteCatalog.asset so <see cref="LegacySpriteRegistry"/>
    /// can load it at runtime (in builds, not just the Editor) via Resources.Load. Only this small
    /// lookup asset is under Resources/ — the source PNGs themselves stay in Art/Legacy/ exactly
    /// where Phase 1 put them.
    /// </summary>
    [CreateAssetMenu(fileName = "LegacySpriteCatalog", menuName = "GuildMaster/Legacy UI/Legacy Sprite Catalog")]
    public class LegacySpriteCatalog : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            /// <summary>Original legacy filename stem, e.g. "unit_footman", "doctrine_of_war", "gem".</summary>
            public string key;
            public Sprite sprite;
        }

        public List<Entry> entries = new List<Entry>();
    }
}
