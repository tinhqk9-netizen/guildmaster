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
    /// Batch 1 grid-safe slicer for the FantasyDungeon character/enemy sheets.
    ///
    /// Only touches Assets/_Game/Art/Characters and .../Enemies. Every sheet is a
    /// 1024x1024, 4x4 grid of 256px cells where each ROW is a facing direction
    /// (row0=down, row1=up, row2=left, row3=right) and each COLUMN is an animation frame.
    ///
    /// Safety first: a file only becomes a candidate if it passes every guard
    /// (size, name pattern, no empty cells, no abnormal-coverage row). Anything that
    /// fails is skipped with a reason — never sliced blindly. Run the Dry Run menu to
    /// preview the candidate/skip split before applying.
    ///
    /// This does NOT crop/repack textures, create animation clips, or map anything.
    /// VFX, UI (ui_kit/ui_dialog), hero_skins and tilesets are out of scope for Batch 1.
    /// </summary>
    public static class SpriteSheetSlicer
    {
        private const int Cols = 4;
        private const int Rows = 4;
        private const int Cell = 256;
        private const int Expected = 1024;

        private static readonly string[] ScanFolders =
        {
            "Assets/_Game/Art/Characters",
            "Assets/_Game/Art/Enemies",
        };

        // row (top->bottom in the image) => facing direction
        private static readonly string[] RowDir = { "down", "up", "left", "right" };

        // Files flagged MANUAL_REVIEW_FIRST in the slice plan — excluded explicitly even
        // if a future edit made them pass the heuristics.
        private static readonly HashSet<string> ForceExclude = new HashSet<string>
        {
            "Characters/merchant/merchant_sheet.png",
            "Characters/villager/villager_sheet.png",
            "Characters/merchant/merchant_idle_sheet.png",
            "Characters/merchant/merchant_walk_sheet.png",
        };

        private enum Verdict { Candidate, Skip }

        private struct Item
        {
            public string Path;      // AssetDatabase path
            public string Rel;       // path under Art/
            public string Actor;
            public string Anim;
            public Vector2Int Size;
            public Verdict Verdict;
            public string Reason;
        }

        [MenuItem("GuildMaster/Assets/Slice Reference Sprite Sheets (Dry Run)")]
        public static void DryRun() => Run(apply: false);

        [MenuItem("GuildMaster/Assets/Slice Reference Sprite Sheets")]
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
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", ScanFolders);

            foreach (string guid in guids.Distinct())
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Replace("\\", "/").Contains("/hero_skins/"))
                {
                    continue; // hero_skins are single images, not sheets — out of scope
                }

                string rel = path.Substring("Assets/_Game/Art/".Length);
                var item = new Item { Path = path, Rel = rel, Verdict = Verdict.Skip };

                if (ForceExclude.Contains(rel))
                {
                    item.Reason = "SKIPPED_MANUAL_REVIEW (excluded by slice plan)";
                    results.Add(item);
                    continue;
                }

                // Decode source bytes into a readable temp texture for size + coverage.
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

                if (!ParseName(rel, out string actor, out string anim, out string nameReason))
                {
                    item.Reason = nameReason;
                    results.Add(item);
                    Object.DestroyImmediate(tex);
                    continue;
                }
                item.Actor = actor;
                item.Anim = anim;

                if (!CoverageOk(tex, out string covReason))
                {
                    item.Reason = covReason;
                    results.Add(item);
                    Object.DestroyImmediate(tex);
                    continue;
                }

                Object.DestroyImmediate(tex);
                item.Verdict = Verdict.Candidate;
                item.Reason = "grid 4x4 @256, all cells filled";
                results.Add(item);
            }

            return results.OrderBy(i => i.Rel).ToList();
        }

        /// <summary>Expects &lt;actor&gt;_&lt;anim&gt;_sheet.png where actor == parent folder name.</summary>
        private static bool ParseName(string rel, out string actor, out string anim, out string reason)
        {
            actor = anim = null;
            reason = null;

            string folder = Path.GetFileName(Path.GetDirectoryName(rel));
            string stem = Path.GetFileNameWithoutExtension(rel);

            if (!stem.StartsWith(folder + "_"))
            {
                reason = $"name '{stem}' does not start with actor '{folder}_'";
                return false;
            }

            string rest = stem.Substring(folder.Length + 1); // e.g. "walk_sheet"
            if (!rest.EndsWith("_sheet"))
            {
                reason = $"name '{stem}' not in <actor>_<anim>_sheet pattern";
                return false;
            }

            anim = rest.Substring(0, rest.Length - "_sheet".Length);
            if (string.IsNullOrEmpty(anim))
            {
                reason = $"name '{stem}' has no <anim> token (bare <actor>_sheet)";
                return false;
            }

            actor = folder;
            return true;
        }

        /// <summary>Per-row opaque coverage; flags a row whose coverage is under half the median.</summary>
        private static bool CoverageOk(Texture2D tex, out string reason)
        {
            reason = null;
            Color32[] px = tex.GetPixels32();
            int w = tex.width;

            var rowCov = new float[Rows];
            for (int r = 0; r < Rows; r++)
            {
                long opaque = 0, total = 0;
                // image row r (top->bottom) maps to texture y range bottom-up
                int yTop = Expected - r * Cell;
                for (int y = yTop - Cell; y < yTop; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (px[y * w + x].a > 8) opaque++;
                        total++;
                    }
                }
                rowCov[r] = total > 0 ? 100f * opaque / total : 0f;
            }

            float[] sorted = rowCov.OrderBy(v => v).ToArray();
            float median = (sorted[1] + sorted[2]) * 0.5f;

            for (int r = 0; r < Rows; r++)
            {
                if (rowCov[r] < median * 0.5f)
                {
                    reason = $"row{r} coverage {rowCov[r]:F1}% abnormal vs median {median:F1}%";
                    return false;
                }
            }
            return true;
        }

        private static bool Slice(Item item)
        {
            if (!(AssetImporter.GetAtPath(item.Path) is TextureImporter importer))
            {
                return false;
            }

            importer.spriteImportMode = SpriteImportMode.Multiple;

            var metas = new List<SpriteMetaData>(Cols * Rows);
            for (int r = 0; r < Rows; r++)
            {
                string dir = RowDir[r];
                // image row r (top) -> texture rect y (bottom-up)
                float rectY = Expected - (r + 1) * Cell;
                for (int c = 0; c < Cols; c++)
                {
                    metas.Add(new SpriteMetaData
                    {
                        name = $"{item.Actor}_{item.Anim}_{dir}_{c:00}",
                        rect = new Rect(c * Cell, rectY, Cell, Cell),
                        alignment = (int)SpriteAlignment.BottomCenter,
                        pivot = new Vector2(0.5f, 0f),
                    });
                }
            }

#pragma warning disable CS0618 // spritesheet API is legacy but the supported path for batch editor slicing
            importer.spritesheet = metas.ToArray();
#pragma warning restore CS0618
            importer.SaveAndReimport();
            return true;
        }

        private static void Log(bool apply, List<Item> candidates, List<Item> skipped, int slicedFiles, int slicedSprites)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[SpriteSheetSlicer] Batch 1 — {(apply ? "APPLY" : "DRY RUN")}");
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
                    sb.AppendLine($"    - {s.Rel} — {s.Reason}");
                }
            }

            sb.AppendLine("\n  CANDIDATES:");
            foreach (Item c in candidates)
            {
                sb.AppendLine($"    - {c.Rel}  ({c.Actor}/{c.Anim})");
            }

            Debug.Log(sb.ToString());
        }
    }
}
#endif
