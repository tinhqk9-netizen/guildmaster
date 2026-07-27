using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuildMaster.Runtime.Assets
{
    /// <summary>
    /// A design-time visual mapping from FantasyDungeon reference sprites to game concepts
    /// (currency, navigation, item categories, enemy groups, portraits).
    ///
    /// This holds direct Sprite references, so consumers get real sprites with no runtime
    /// path-loading. It carries NO gameplay data — only visual associations. Populate it via
    /// GuildMaster → Assets → Build Asset Catalog.
    /// </summary>
    [CreateAssetMenu(fileName = "AssetCatalog", menuName = "GuildMaster/Asset Catalog")]
    public class AssetCatalog : ScriptableObject
    {
        [Serializable]
        public struct NamedSprite
        {
            public string key;
            public Sprite sprite;
        }

        [Header("Currency (HUD)")]
        public Sprite coinIcon;
        public Sprite gemIcon;

        [Header("Navigation icons (key = UIScreenId name)")]
        public List<NamedSprite> navIcons = new List<NamedSprite>();

        [Header("Item category icons (key = ItemCategory name)")]
        public List<NamedSprite> categoryIcons = new List<NamedSprite>();

        [Header("Enemy group icons (key = visual group)")]
        public List<NamedSprite> enemyIcons = new List<NamedSprite>();

        [Header("Portraits (key = portrait name)")]
        public List<NamedSprite> portraits = new List<NamedSprite>();

        public Sprite Nav(string key) => Find(navIcons, key);
        public Sprite Category(string key) => Find(categoryIcons, key);
        public Sprite Enemy(string key) => Find(enemyIcons, key);
        public Sprite Portrait(string key) => Find(portraits, key);

        private static Sprite Find(List<NamedSprite> list, string key)
        {
            if (list == null || string.IsNullOrEmpty(key)) return null;
            foreach (NamedSprite e in list)
            {
                if (string.Equals(e.key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return e.sprite;
                }
            }
            return null;
        }
    }
}
