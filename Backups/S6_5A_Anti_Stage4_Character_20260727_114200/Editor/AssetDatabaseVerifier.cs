#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GuildMaster.Editor.Assets
{
    [InitializeOnLoad]
    public static class AssetDatabaseVerifier
    {
        static AssetDatabaseVerifier()
        {
            EditorApplication.delayCall += Verify;
        }

        [MenuItem("GuildMaster/Assets/Verify Sliced Assets (AssetDatabase)")]
        public static void Verify()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[AssetDatabaseVerifier] Starting AssetDatabase Verification...");
            bool allPass = true;

            // 1. Verify 8 Tileset files
            string[] tilesetFiles = new string[]
            {
                "Assets/_Game/Art/Tilesets/environment/props.png",
                "Assets/_Game/Art/Tilesets/environment/props2.png",
                "Assets/_Game/Art/Tilesets/environment/props2b.png",
                "Assets/_Game/Art/Tilesets/environment/traps.png",
                "Assets/_Game/Art/Tilesets/environment/animated_tiles.png",
                "Assets/_Game/Art/Tilesets/environment/water_anim.png",
                "Assets/_Game/Art/Tilesets/environment/brazier_anim.png",
                "Assets/_Game/Art/Tilesets/environment/deco_shadows.png"
            };

            sb.AppendLine("\n--- Verifying Tileset Batch 2 ---");
            foreach (string path in tilesetFiles)
            {
                bool pass = VerifyTileset(path, sb);
                if (!pass) allPass = false;
            }

            // 2. Verify Sample Batch 1 files
            string[] batch1Samples = new string[]
            {
                "Assets/_Game/Art/Characters/hero/hero_idle_sheet.png",
                "Assets/_Game/Art/Enemies/skeleton/skeleton_walk_sheet.png",
                "Assets/_Game/Art/Enemies/slime/slime_idle_sheet.png"
            };

            sb.AppendLine("\n--- Verifying Sample Batch 1 ---");
            foreach (string path in batch1Samples)
            {
                bool pass = VerifyBatch1Sample(path, sb);
                if (!pass) allPass = false;
            }

            if (allPass)
            {
                sb.AppendLine("\n>>> FINAL RESULT: ALL_VERIFIED_PASS <<<");
            }
            else
            {
                sb.AppendLine("\n>>> FINAL RESULT: VERIFICATION_FAILED <<<");
            }

            File.WriteAllText("verify-result.txt", sb.ToString());
            Debug.Log("[AssetDatabaseVerifier] Output written to verify-result.txt");
            EditorApplication.delayCall -= Verify;
        }

        private static bool VerifyTileset(string path, StringBuilder sb)
        {
            sb.AppendLine($"Checking: {path}");
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                sb.AppendLine($"  [FAIL] Importer is null for {path}");
                return false;
            }

            bool pass = true;

            if (importer.spriteImportMode != SpriteImportMode.Multiple) { sb.AppendLine($"  [FAIL] spriteImportMode != Multiple"); pass = false; }
            if (importer.filterMode != FilterMode.Point) { sb.AppendLine($"  [FAIL] filterMode != Point"); pass = false; }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed) { sb.AppendLine($"  [FAIL] textureCompression != Uncompressed"); pass = false; }
            if (importer.mipmapEnabled != false) { sb.AppendLine($"  [FAIL] mipmapEnabled != false"); pass = false; }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            Sprite[] sprites = assets.OfType<Sprite>().ToArray();

            if (sprites.Length != 64)
            {
                sb.AppendLine($"  [FAIL] Expected 64 sprites, got {sprites.Length}");
                pass = false;
            }
            else
            {
                sb.AppendLine($"  [PASS] LoadAllAssetsAtPath returned exactly 64 Sprites.");
            }

            string sheetName = System.IO.Path.GetFileNameWithoutExtension(path);
            bool allSpritesOk = true;

            // Just check a few to avoid log spam, but validate them all
            int checkedCount = 0;
            foreach (Sprite s in sprites)
            {
                // Check name
                if (!s.name.StartsWith(sheetName + "_"))
                {
                    sb.AppendLine($"  [FAIL] Sprite name '{s.name}' does not match pattern {sheetName}_XX_XX");
                    allSpritesOk = false;
                }

                // Check Rect
                if (s.rect.width != 128f || s.rect.height != 128f)
                {
                    sb.AppendLine($"  [FAIL] Sprite '{s.name}' rect size is {s.rect.width}x{s.rect.height}, expected 128x128");
                    allSpritesOk = false;
                }

                // Check Pivot
                Vector2 normPivot = new Vector2(s.pivot.x / s.rect.width, s.pivot.y / s.rect.height);
                if (Mathf.Abs(normPivot.x - 0.5f) > 0.01f || Mathf.Abs(normPivot.y - 0.5f) > 0.01f)
                {
                    sb.AppendLine($"  [FAIL] Sprite '{s.name}' normalized pivot is {normPivot}, expected (0.5, 0.5)");
                    allSpritesOk = false;
                }
                
                checkedCount++;
            }

            if (allSpritesOk && sprites.Length == 64)
            {
                sb.AppendLine($"  [PASS] 64 sprites validated: Rect=128x128, Pivot=Center, NamePattern={sheetName}_XX_XX");
            }

            return pass && allSpritesOk;
        }

        private static bool VerifyBatch1Sample(string path, StringBuilder sb)
        {
            sb.AppendLine($"Checking: {path}");
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                sb.AppendLine($"  [FAIL] Importer is null for {path}");
                return false;
            }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            Sprite[] sprites = assets.OfType<Sprite>().ToArray();

            if (sprites.Length != 16)
            {
                sb.AppendLine($"  [FAIL] Expected 16 sprites, got {sprites.Length}");
                return false;
            }

            sb.AppendLine($"  [PASS] LoadAllAssetsAtPath returned exactly 16 Sprites.");
            return true;
        }
    }
}
#endif
