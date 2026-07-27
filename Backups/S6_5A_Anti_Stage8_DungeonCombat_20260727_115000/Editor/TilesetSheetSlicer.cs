#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GuildMaster.Editor.Assets
{
    /// <summary>
    /// Batch 2 grid-safe tileset slicer for 8x8 @128 environment sheets.
    ///
    /// Scope: Exactly 8 target environment sheets (1024x1024, 8x8 grid of 128px cells).
    /// Naming: <sheetname>_<row:00>_<col:00>
    /// Pivot: Center (0.5, 0.5)
    /// FilterMode: Point, Compression: None, MipMap: Off.
    /// </summary>
    public static class TilesetSheetSlicer
    {
        private const int Cols = 8;
        private const int Rows = 8;
        private const int Cell = 128;
        private const int Expected = 1024;

        private static readonly string[] TargetFiles = new string[]
        {
            "Assets/_Game/Art/Tilesets/environment/props.png",
            "Assets/_Game/Art/Tilesets/environment/props2.png",
            "Assets/_Game/Art/Tilesets/environment/props2b.png",
            "Assets/_Game/Art/Tilesets/environment/traps.png",
            "Assets/_Game/Art/Tilesets/environment/animated_tiles.png",
            "Assets/_Game/Art/Tilesets/environment/water_anim.png",
            "Assets/_Game/Art/Tilesets/environment/brazier_anim.png",
            "Assets/_Game/Art/Tilesets/environment/deco_shadows.png",
        };

        private enum Verdict { Candidate, Skip }

        private struct Item
        {
            public string Path;
            public string Name;
            public Vector2Int Size;
            public Verdict Verdict;
            public string Reason;
        }

        [MenuItem("GuildMaster/Assets/Slice Tileset Sprite Sheets (Dry Run)")]
        public static void DryRun() => Run(apply: false);

        [MenuItem("GuildMaster/Assets/Slice Tileset Sprite Sheets")]
        public static void Apply() => Run(apply: true);

        private static void Run(bool apply)
        {
            List<Item> items = Collect();
            var candidates = items.Where(i => i.Verdict == Verdict.Candidate).ToList();
            var skipped = items.Where(i => i.Verdict == Verdict.Skip).ToList();

            int slicedFiles = 0;
            int slicedSprites = 0;

            if (apply && candidates.Count > 0)
            {
                try
                {
                    AssetDatabase.StartAssetEditing();
                    foreach (Item item in candidates)
                    {
                        if (Slice(item))
                        {
                            slicedFiles++;
                            slicedSprites += Cols * Rows;
                        }
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.Refresh();
                }
            }

            Log(apply, candidates, skipped, slicedFiles, slicedSprites);
        }

        private static List<Item> Collect()
        {
            var results = new List<Item>();
            foreach (string path in TargetFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                var item = new Item { Path = path, Name = fileName, Verdict = Verdict.Skip };

                if (!File.Exists(path))
                {
                    item.Reason = "file missing";
                    results.Add(item);
                    continue;
                }

                var tex = new Texture2D(2, 2);
                bool loaded = tex.LoadImage(File.ReadAllBytes(path));
                if (!loaded)
                {
                    item.Reason = "cannot decode PNG";
                    results.Add(item);
                    Object.DestroyImmediate(tex);
                    continue;
                }

                item.Size = new Vector2Int(tex.width, tex.height);

                if (tex.width != Expected || tex.height != Expected)
                {
                    item.Reason = $"size {tex.width}x{tex.height} != {Expected}x{Expected}";
                    results.Add(item);
                    Object.DestroyImmediate(tex);
                    continue;
                }

                Object.DestroyImmediate(tex);
                item.Verdict = Verdict.Candidate;
                item.Reason = "grid 8x8 @128, size 1024x1024 valid";
                results.Add(item);
            }

            return results;
        }

        private static bool Slice(Item item)
        {
            if (!(AssetImporter.GetAtPath(item.Path) is TextureImporter importer))
            {
                return false;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;

            var metas = new List<SpriteMetaData>(Cols * Rows);
            for (int r = 0; r < Rows; r++)
            {
                float rectY = Expected - (r + 1) * Cell;
                for (int c = 0; c < Cols; c++)
                {
                    float rectX = c * Cell;
                    metas.Add(new SpriteMetaData
                    {
                        name = $"{item.Name}_{r:00}_{c:00}",
                        rect = new Rect(rectX, rectY, Cell, Cell),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f),
                    });
                }
            }

#pragma warning disable CS0618
            importer.spritesheet = metas.ToArray();
#pragma warning restore CS0618
            importer.SaveAndReimport();
            return true;
        }

        private static void Log(bool apply, List<Item> candidates, List<Item> skipped, int slicedFiles, int slicedSprites)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[TilesetSheetSlicer] Batch 2 — {(apply ? "APPLY" : "DRY RUN")}");
            sb.AppendLine($"  candidates : {candidates.Count}");
            sb.AppendLine($"  skipped    : {skipped.Count}");
            if (apply)
            {
                sb.AppendLine($"  sliced     : {slicedFiles} file(s) -> {slicedSprites} sprite(s)");
            }

            if (skipped.Count > 0)
            {
                sb.AppendLine("\n  SKIPPED:");
                foreach (Item s in skipped)
                {
                    sb.AppendLine($"    - {s.Name} ({s.Path}) — {s.Reason}");
                }
            }

            sb.AppendLine("\n  CANDIDATES:");
            foreach (Item c in candidates)
            {
                sb.AppendLine($"    - {c.Name} ({c.Path})");
            }

            Debug.Log(sb.ToString());
        }
    }
}
#endif
