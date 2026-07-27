#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GuildMaster.Editor.Assets
{
    /// <summary>
    /// Applies pixel-art safe import settings to the reference art copied from
    /// Assets-tham-khao (FantasyDungeon pack) into Assets/_Game/Art.
    ///
    /// The pack's README_IMPORT.txt requires "NO filtering, NO compression" — Unity's
    /// Bilinear + Compressed defaults would blur and band every sprite, so these presets
    /// are mandatory before any of the art is usable.
    ///
    /// Sheets are intentionally left as Single sprites here; switching them to Multiple
    /// belongs to the slice plan, not to this import pass.
    /// </summary>
    public static class AssetImportPresets
    {
        private const string ArtRoot = "Assets/_Game/Art";

        /// <summary>Per-folder preset. PPU matches the pack's native cell size so 1 unit == 1 cell.</summary>
        private readonly struct Preset
        {
            public readonly string Folder;
            public readonly float PixelsPerUnit;
            public readonly string Note;

            public Preset(string folder, float pixelsPerUnit, string note)
            {
                Folder = folder;
                PixelsPerUnit = pixelsPerUnit;
                Note = note;
            }
        }

        private static readonly Preset[] Presets =
        {
            new Preset("Icons",      64f,  "icons are 64x64 native"),
            new Preset("Tilesets",   128f, "tiles are 128x128 native"),
            new Preset("Characters", 256f, "1024x1024 sheet = 4x4 grid of 256px cells"),
            new Preset("Enemies",    256f, "1024x1024 sheet = 4x4 grid of 256px cells"),
            new Preset("Portraits",  256f, "portraits are 256x256 native"),
            new Preset("UI",         100f, "UI is scaled by CanvasScaler, not PPU; 100 = Unity default"),
            new Preset("VFX",        256f, "provisional - frames are not uniform, revisit with the slice plan"),
        };

        /// <summary>Assets that import fine but still carry a defect a human must resolve.</summary>
        private static readonly Dictionary<string, string> ManualReview = new Dictionary<string, string>
        {
            { "UI/ui_kit.png",                    "atlas - needs manual element cutting + 9-slice borders" },
            { "UI/ui_dialog.png",                 "mockup with baked English text - harvest elements only" },
            { "Characters/hero_skins/crimson.png",    "magenta chroma-key background not removed" },
            { "Characters/hero_skins/mage.png",       "magenta chroma-key background not removed" },
            { "Characters/hero_skins/darkknight.png", "magenta chroma-key background not removed" },
            { "Characters/hero_skins/paladin.png",    "white halo fringe around the sprite" },
            { "VFX/vfx_heal.png",                 "stone tile baked under the effect frames" },
            { "VFX/vfx_levelup.png",              "stone tile + baked 'LEVEL UP!' text" },
        };

        [MenuItem("GuildMaster/Assets/Apply Reference Asset Import Presets")]
        public static void ApplyPresets()
        {
            var counts = new Dictionary<string, int>();
            var skipped = new List<string>();
            var flagged = new List<string>();

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var preset in Presets)
                {
                    string folder = $"{ArtRoot}/{preset.Folder}";
                    if (!AssetDatabase.IsValidFolder(folder))
                    {
                        skipped.Add($"{preset.Folder} (folder not found)");
                        continue;
                    }

                    string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
                    int applied = 0;

                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (Apply(path, preset.PixelsPerUnit))
                        {
                            applied++;
                        }
                        else
                        {
                            skipped.Add($"{path} (no TextureImporter)");
                        }

                        string rel = path.Substring(ArtRoot.Length + 1);
                        if (ManualReview.TryGetValue(rel, out string reason))
                        {
                            flagged.Add($"{rel} — {reason}");
                        }
                    }

                    counts[preset.Folder] = applied;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            Report(counts, skipped, flagged);
        }

        private static bool Apply(string path, float pixelsPerUnit)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
            {
                return false;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = importer.DoesSourceTextureHaveAlpha();

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);

            // Never clamp below the source resolution.
            var platform = importer.GetDefaultPlatformTextureSettings();
            platform.maxTextureSize = MaxSizeFor(path);
            platform.textureCompression = TextureImporterCompression.Uncompressed;
            platform.format = TextureImporterFormat.Automatic;
            importer.SetPlatformTextureSettings(platform);

            importer.SaveAndReimport();
            return true;
        }

        /// <summary>Smallest valid Unity max size that is still >= the source texture.</summary>
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

        private static void Report(Dictionary<string, int> counts, List<string> skipped, List<string> flagged)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[AssetImportPresets] Applied pixel-art import presets:");

            foreach (var preset in Presets)
            {
                int n = counts.TryGetValue(preset.Folder, out int c) ? c : 0;
                sb.AppendLine($"  {preset.Folder,-12} {n,4} file(s)   PPU {preset.PixelsPerUnit,-5} — {preset.Note}");
            }

            sb.AppendLine($"  {"TOTAL",-12} {counts.Values.Sum(),4} file(s)");

            if (flagged.Count > 0)
            {
                sb.AppendLine($"\n  Needs manual review ({flagged.Count}):");
                foreach (string f in flagged)
                {
                    sb.AppendLine($"    - {f}");
                }
            }

            if (skipped.Count > 0)
            {
                sb.AppendLine($"\n  Skipped ({skipped.Count}):");
                foreach (string s in skipped)
                {
                    sb.AppendLine($"    - {s}");
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}
#endif
