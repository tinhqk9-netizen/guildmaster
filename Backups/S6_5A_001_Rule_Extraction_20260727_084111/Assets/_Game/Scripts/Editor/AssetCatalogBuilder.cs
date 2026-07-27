#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GuildMaster.Runtime.Assets;
using UnityEditor;
using UnityEngine;

namespace GuildMaster.Editor.Assets
{
    /// <summary>
    /// Populates AssetCatalog.asset from the imported FantasyDungeon sprites.
    /// Every association below is grounded in the visual mapping report
    /// (Reports/S5/s5-asset-visual-mapping-report.md). Missing sprites are skipped and
    /// logged — the builder never fabricates a reference.
    /// </summary>
    public static class AssetCatalogBuilder
    {
        private const string Icons = "Assets/_Game/Art/Icons";
        private const string Enemies = "Assets/_Game/Art/Enemies";
        private const string Portraits = "Assets/_Game/Art/Portraits";
        private const string CatalogPath = "Assets/_Game/Data/AssetCatalog.asset";

        [MenuItem("GuildMaster/Assets/Build Asset Catalog")]
        public static void Build()
        {
            AssetCatalog catalog = AssetDatabase.LoadAssetAtPath<AssetCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<AssetCatalog>();
                string dir = System.IO.Path.GetDirectoryName(CatalogPath);
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    AssetDatabase.CreateFolder("Assets/_Game", "Data");
                }
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var missing = new List<string>();

            // ---- Currency ----
            catalog.coinIcon = Icon("coin", missing);
            catalog.gemIcon = Icon("gem_v1", missing);

            // ---- Navigation (UIScreenId name -> a sensible pack icon) ----
            catalog.navIcons = new List<AssetCatalog.NamedSprite>
            {
                Nav("Inventory", Icon("potion_red", missing)),
                Nav("Character", Icon("helmet", missing)),
                Nav("Dungeon",   Icon("icons_swords_0", missing)),
                Nav("Craft",     Icon("icons_blunt_0", missing)),
                Nav("Merchant",  Icon("coin", missing)),
                Nav("Settings",  Icon("gem_v1", missing)),
            };

            // ---- Item categories (ItemCategory name -> representative icon) ----
            catalog.categoryIcons = new List<AssetCatalog.NamedSprite>
            {
                Nav("Weapon",     Icon("icons_swords_0", missing)),
                Nav("Armor",      Icon("helmet", missing)),
                Nav("Accessory",  Icon("ring", missing)),
                Nav("Consumable", Icon("potion_red", missing)),
                Nav("Material",   Icon("gem_v1", missing)),
            };

            // ---- Enemy visual groups (idle, facing down, frame 0) ----
            catalog.enemyIcons = new List<AssetCatalog.NamedSprite>
            {
                Nav("undead", EnemySprite("skeleton", missing)),
                Nav("goblin", EnemySprite("goblin", missing)),
                Nav("slime",  EnemySprite("slime", missing)),
                Nav("bat",    EnemySprite("bat", missing)),
                Nav("orc",    EnemySprite("orc", missing)),
                Nav("boss",   EnemySprite("boss", missing)),
            };

            // ---- Portraits ----
            catalog.portraits = new List<AssetCatalog.NamedSprite>();
            foreach (string name in new[] { "hero", "boss", "goblin", "skeleton", "orc", "slime", "merchant", "villager" })
            {
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>($"{Portraits}/{name}.png");
                if (s == null) missing.Add($"{Portraits}/{name}.png");
                else catalog.portraits.Add(new AssetCatalog.NamedSprite { key = name, sprite = s });
            }

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var sb = new StringBuilder();
            sb.AppendLine("[AssetCatalogBuilder] Built AssetCatalog.asset");
            sb.AppendLine($"  coin={(catalog.coinIcon != null)} gem={(catalog.gemIcon != null)}");
            sb.AppendLine($"  nav={catalog.navIcons.Count(n => n.sprite != null)}/{catalog.navIcons.Count}"
                          + $"  category={catalog.categoryIcons.Count(n => n.sprite != null)}/{catalog.categoryIcons.Count}"
                          + $"  enemy={catalog.enemyIcons.Count(n => n.sprite != null)}/{catalog.enemyIcons.Count}"
                          + $"  portraits={catalog.portraits.Count}");
            if (missing.Count > 0)
            {
                sb.AppendLine($"  MISSING ({missing.Count}): " + string.Join(", ", missing));
            }
            Debug.Log(sb.ToString());
        }

        private static AssetCatalog.NamedSprite Nav(string key, Sprite sprite)
        {
            return new AssetCatalog.NamedSprite { key = key, sprite = sprite };
        }

        private static Sprite Icon(string name, List<string> missing)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>($"{Icons}/{name}.png");
            if (s == null) missing.Add($"{Icons}/{name}.png");
            return s;
        }

        /// <summary>Idle sheet, facing down, frame 0: sprite named &lt;group&gt;_idle_down_00.</summary>
        private static Sprite EnemySprite(string group, List<string> missing)
        {
            string path = $"{Enemies}/{group}/{group}_idle_sheet.png";
            Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
            Sprite s = all.OfType<Sprite>().FirstOrDefault(sp => sp.name == $"{group}_idle_down_00");
            if (s == null) missing.Add($"{path}#{group}_idle_down_00");
            return s;
        }
    }
}
#endif
