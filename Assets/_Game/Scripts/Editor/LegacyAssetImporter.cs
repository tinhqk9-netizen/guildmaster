#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using GuildMaster.Runtime.UI.Legacy;

namespace GuildMaster.Editor.Assets
{
    /// <summary>
    /// Phase 1 (Legacy UI reconstruction): applies Unity sprite import settings to the legacy
    /// game art copied from the decompiled original
    /// (D:\Tinh\Guild Master - Idle Dungeons\resources\res\drawable\) into
    /// Assets/_Game/Art/Legacy/&lt;Category&gt;/. See Docs/Legacy_Audit/phase_1_asset_import_report.md.
    ///
    /// This tool only touches import settings on files already present under
    /// <see cref="LegacyArtRoot"/> — it does not copy files, does not rename anything, and does
    /// not touch any scene, prefab, backend service, or existing UI runtime code.
    ///
    /// Idempotent: safe to run repeatedly. It never creates or duplicates assets — it only
    /// (re)applies TextureImporter settings to whatever .png files already exist in the folder
    /// tree, and Unity's SaveAndReimport() is a no-op when settings are already identical.
    /// </summary>
    public static class LegacyAssetImporter
    {
        private const string LegacyArtRoot = "Assets/_Game/Art/Legacy";
        private const string ResourcesFolder = "Assets/Resources";
        private const string CatalogAssetPath = "Assets/Resources/LegacySpriteCatalog.asset";

        private static readonly string[] CategoryFolders =
        {
            "UI", "Navigation", "Currency", "Characters", "Enemies", "Items",
            "Skills", "Status", "Dungeons", "Pets", "Doctrines", "Quests", "Misc"
        };

        [MenuItem("Tools/Guild Master/Legacy UI/Import Legacy Assets")]
        public static void ImportLegacyAssets()
        {
            var perFolderCount = new Dictionary<string, int>();
            var skippedNoImporter = new List<string>();
            var nameToPath = new Dictionary<string, List<string>>();
            int applied = 0;

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (string folder in CategoryFolders)
                {
                    string folderPath = $"{LegacyArtRoot}/{folder}";
                    if (!AssetDatabase.IsValidFolder(folderPath))
                    {
                        perFolderCount[folder] = 0;
                        continue;
                    }

                    string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
                    int count = 0;

                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);

                        if (ApplyImportSettings(path))
                        {
                            count++;
                            applied++;
                        }
                        else
                        {
                            skippedNoImporter.Add(path);
                        }

                        string spriteName = System.IO.Path.GetFileNameWithoutExtension(path);
                        if (!nameToPath.TryGetValue(spriteName, out var paths))
                        {
                            paths = new List<string>();
                            nameToPath[spriteName] = paths;
                        }
                        paths.Add(path);
                    }

                    perFolderCount[folder] = count;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            var duplicates = nameToPath.Where(kv => kv.Value.Count > 1)
                .Select(kv => (kv.Key, kv.Value))
                .ToList();

            int catalogEntryCount = BuildCatalog(nameToPath, duplicates);

            Report(perFolderCount, applied, skippedNoImporter, duplicates, catalogEntryCount);
        }

        /// <summary>
        /// Phase 1 requirement 8 check: loads a fixed sample set (3 character, 3 item, 3 enemy,
        /// 4 currency, 3 navigation icon) through <see cref="LegacySpriteRegistry"/> — the exact
        /// same runtime API Phase 2+ screens will use — and logs pass/fail per sprite.
        /// </summary>
        [MenuItem("Tools/Guild Master/Legacy UI/Verify Sample Sprite Load")]
        public static void VerifySampleSpriteLoad()
        {
            LegacySpriteRegistry.ClearCache();

            var samples = new (string Group, string Name)[]
            {
                ("Character", "unit_cleric"),
                ("Character", "unit_knight"),
                ("Character", "unit_paladin"),
                ("Item", "abherrant_fabric"),
                ("Item", "absolute_zero"),
                ("Item", "abyssal_cutlass"),
                ("Enemy", "unit_abomination"),
                ("Enemy", "unit_adept"),
                ("Enemy", "unit_alchemist"),
                ("Currency", "coin_platinum"),
                ("Currency", "coin_gold"),
                ("Currency", "coin_silver"),
                ("Currency", "coin_copper"),
                ("Navigation", "bottom_nav_adventurers"),
                ("Navigation", "bottom_nav_dungeons"),
                ("Navigation", "bottom_nav_raids"),
            };

            var sb = new StringBuilder();
            sb.AppendLine("[LegacyAssetImporter] Sample sprite load verification:");
            int pass = 0;

            foreach (var (group, name) in samples)
            {
                Sprite sprite = LegacySpriteRegistry.GetSprite(name);
                bool ok = sprite != null;
                if (ok) pass++;
                sb.AppendLine($"  [{(ok ? "PASS" : "FAIL")}] {group,-10} {name,-24} -> {(ok ? sprite.name : "NULL")}");
            }

            sb.AppendLine($"\n  {pass}/{samples.Length} passed. Registry.Count = {LegacySpriteRegistry.Count}");
            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// (Re)builds Assets/Resources/LegacySpriteCatalog.asset from every sprite currently under
        /// Assets/_Game/Art/Legacy/. Idempotent: reuses the existing asset if present (does not
        /// create a second one), and simply replaces its entries list each run.
        /// </summary>
        private static int BuildCatalog(
            Dictionary<string, List<string>> nameToPath,
            List<(string Key, List<string> Value)> duplicates)
        {
            var duplicateKeys = new HashSet<string>(duplicates.Select(d => d.Key));

            if (!AssetDatabase.IsValidFolder(ResourcesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            var catalog = AssetDatabase.LoadAssetAtPath<LegacySpriteCatalog>(CatalogAssetPath);
            bool isNew = catalog == null;
            if (isNew)
            {
                catalog = ScriptableObject.CreateInstance<LegacySpriteCatalog>();
            }

            catalog.entries.Clear();

            foreach (var kv in nameToPath.OrderBy(k => k.Key))
            {
                // Duplicate names are ambiguous — skip them here rather than pick one arbitrarily.
                // They are already surfaced loudly in the report for a human to resolve.
                if (duplicateKeys.Contains(kv.Key))
                {
                    continue;
                }

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(kv.Value[0]);
                catalog.entries.Add(new LegacySpriteCatalog.Entry { key = kv.Key, sprite = sprite });
            }

            if (isNew)
            {
                AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
            }
            else
            {
                EditorUtility.SetDirty(catalog);
            }

            AssetDatabase.SaveAssets();
            return catalog.entries.Count;
        }

        /// <summary>
        /// Applies the Phase 1 spec exactly:
        /// Sprite Mode = Single, Texture Type = Sprite (2D and UI), Alpha Is Transparency = true,
        /// Compression = Uncompressed (None), Filter Mode = Bilinear (source art is painted/photo
        /// style mobile UI icons, not pixel art — Bilinear matches how Android rendered them),
        /// Max Size = smallest power-of-two >= the source image's real resolution.
        /// Never modifies the source file itself — only the .meta import settings.
        /// </summary>
        private static bool ApplyImportSettings(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
            {
                return false;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);

            var platform = importer.GetDefaultPlatformTextureSettings();
            platform.maxTextureSize = MaxSizeFor(path);
            platform.textureCompression = TextureImporterCompression.Uncompressed;
            platform.format = TextureImporterFormat.Automatic;
            importer.SetPlatformTextureSettings(platform);

            importer.SaveAndReimport();
            return true;
        }

        /// <summary>Smallest valid Unity max size that is still >= the source texture's real resolution.</summary>
        private static int MaxSizeFor(string path)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            int longest = texture != null ? Mathf.Max(texture.width, texture.height) : 2048;

            foreach (int size in new[] { 32, 64, 128, 256, 512, 1024, 2048, 4096 })
            {
                if (size >= longest)
                {
                    return size;
                }
            }

            return 8192;
        }

        private static void Report(
            Dictionary<string, int> perFolderCount,
            int applied,
            List<string> skippedNoImporter,
            List<(string Key, List<string> Value)> duplicates,
            int catalogEntryCount)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[LegacyAssetImporter] Phase 1 import settings applied:");

            foreach (string folder in CategoryFolders)
            {
                int n = perFolderCount.TryGetValue(folder, out int c) ? c : 0;
                sb.AppendLine($"  {folder,-12} {n,4} file(s)");
            }

            sb.AppendLine($"  {"TOTAL",-12} {applied,4} file(s)");
            sb.AppendLine($"\n  LegacySpriteCatalog: {catalogEntryCount} entries written to {CatalogAssetPath}");

            if (duplicates.Count > 0)
            {
                sb.AppendLine($"\n  DUPLICATE SPRITE NAMES ({duplicates.Count}) — must resolve before Phase 2:");
                foreach (var dup in duplicates)
                {
                    sb.AppendLine($"    - {dup.Key}: {string.Join(", ", dup.Value)}");
                }
            }
            else
            {
                sb.AppendLine("\n  No duplicate sprite names found.");
            }

            if (skippedNoImporter.Count > 0)
            {
                sb.AppendLine($"\n  Skipped (no TextureImporter) ({skippedNoImporter.Count}):");
                foreach (string s in skippedNoImporter)
                {
                    sb.AppendLine($"    - {s}");
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}
#endif
