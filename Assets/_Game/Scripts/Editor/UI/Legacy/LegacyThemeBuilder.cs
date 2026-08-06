using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Legacy;

namespace GuildMaster.Editor.UI.Legacy
{
    /// <summary>
    /// Idempotent Editor tool to generate foundational 9-slice sprites and prefabs for the Legacy UI.
    /// Runs via "Tools/Guild Master/Legacy UI/Build Legacy Theme Assets"
    /// </summary>
    public static class LegacyThemeBuilder
    {
        private const string GeneratedArtPath = "Assets/_Game/Art/UI/Generated";
        private const string GeneratedPrefabsPath = "Assets/_Game/Prefabs/UI/Legacy";
        private const string ResourcesFolder = "Assets/Resources";
        private const string ThemeCatalogAssetPath = "Assets/Resources/LegacyThemeSpriteCatalog.asset";

        // Every sprite name GenerateSprite() produces — used to (re)build LegacyThemeSpriteCatalog
        // so runtime code (e.g. DungeonsTabController) can fetch these via LegacyThemeSprites.Get().
        private static readonly string[] GeneratedSpriteNames =
        {
            "dialog_border", "object_border_dim_white", "object_border_brass", "object_border_no_background",
            "object_border_ascended", "object_border_rounded_left_ascended",
            "object_border_dim_white_extra_opaque", "object_border_dim_white_unavailable",
        };

        // Use 128x128 to have plenty of safe area for 30px corners when slicing
        private const int TextureSize = 128;

        [InitializeOnLoadMethod]
        [MenuItem("Tools/Guild Master/Legacy UI/Build Legacy Theme Assets")]
        public static void BuildThemeAssets()
        {
            Debug.Log("Starting Legacy Theme Generation...");
            EnsureDirectories();

            // 1. Generate Sprites
            GenerateSprite("dialog_border", LegacyUITheme.CardviewDarkBackground, LegacyUITheme.Black, LegacyUITheme.Border3DpPX, LegacyUITheme.CornerRadiusPX, true, true, true, true);
            GenerateSprite("object_border_dim_white", LegacyUITheme.StandardBackground, LegacyUITheme.DimWhite, LegacyUITheme.Border1DpPX, LegacyUITheme.CornerRadiusPX, true, true, true, true);
            GenerateSprite("object_border_brass", LegacyUITheme.StandardBackground, LegacyUITheme.BrassBorder, LegacyUITheme.Border1DpPX, LegacyUITheme.CornerRadiusPX, true, true, true, true);
            GenerateSprite("object_border_no_background", LegacyUITheme.Transparent, LegacyUITheme.DimWhite, LegacyUITheme.Border1DpPX, LegacyUITheme.CornerRadiusPX, true, true, true, true);
            GenerateSprite("object_border_ascended", LegacyUITheme.AscendedBackground, LegacyUITheme.AscendedUnit, LegacyUITheme.Border1DpPX, LegacyUITheme.CornerRadiusPX, true, true, true, true);
            
            // Special case: rounded left only
            GenerateSprite("object_border_rounded_left_ascended", LegacyUITheme.AscendedBackground, LegacyUITheme.AscendedUnit, LegacyUITheme.Border1DpPX, LegacyUITheme.CornerRadiusPX, true, false, true, false);

            // Phase 7: dungeon UI needs an extra-opaque variant (corner badges: loot/pet icon backdrops)
            // and an unavailable/locked variant (red-tinted fill, same dim_white stroke as the standard card).
            GenerateSprite("object_border_dim_white_extra_opaque", LegacyUITheme.ExtraOpaqueBackground, LegacyUITheme.DimWhite, LegacyUITheme.Border1DpPX, LegacyUITheme.CornerRadiusPX, true, true, true, true);
            GenerateSprite("object_border_dim_white_unavailable", LegacyUITheme.StandardBackgroundUnavailable, LegacyUITheme.DimWhite, LegacyUITheme.Border1DpPX, LegacyUITheme.CornerRadiusPX, true, true, true, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 2. Generate Prefabs (Simple reusable frames)
            CreatePrefab("LegacyPanel", "dialog_border");
            CreatePrefab("LegacyDialogFrame", "dialog_border"); // Could have distinct layout later
            CreatePrefab("LegacyCardFrame", "object_border_dim_white");
            CreatePrefab("LegacyButtonFrame", "object_border_brass");
            CreatePrefab("LegacyAscendedFrame", "object_border_ascended");
            CreatePrefab("LegacyNoBackgroundFrame", "object_border_no_background");
            CreatePrefab("LegacyRoundedLeftAscendedFrame", "object_border_rounded_left_ascended");
            CreatePrefab("LegacyCardExtraOpaqueFrame", "object_border_dim_white_extra_opaque");
            CreatePrefab("LegacyCardUnavailableFrame", "object_border_dim_white_unavailable");

            // 3. Rebuild the runtime-loadable theme sprite catalog (Resources.Load target)
            BuildThemeSpriteCatalog();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Legacy Theme Generation Complete!");
        }

        private static void BuildThemeSpriteCatalog()
        {
            if (!AssetDatabase.IsValidFolder(ResourcesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            var catalog = AssetDatabase.LoadAssetAtPath<LegacyThemeSpriteCatalog>(ThemeCatalogAssetPath);
            bool isNew = catalog == null;
            if (isNew) catalog = ScriptableObject.CreateInstance<LegacyThemeSpriteCatalog>();

            catalog.entries.Clear();
            foreach (var name in GeneratedSpriteNames)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GeneratedArtPath}/{name}.png");
                if (sprite == null)
                {
                    Debug.LogWarning($"[LegacyThemeBuilder] Could not load generated sprite '{name}' for theme catalog.");
                    continue;
                }
                catalog.entries.Add(new LegacyThemeSpriteCatalog.Entry { key = name, sprite = sprite });
            }

            if (isNew) AssetDatabase.CreateAsset(catalog, ThemeCatalogAssetPath);
            else EditorUtility.SetDirty(catalog);
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(GeneratedArtPath)) Directory.CreateDirectory(GeneratedArtPath);
            if (!Directory.Exists(GeneratedPrefabsPath)) Directory.CreateDirectory(GeneratedPrefabsPath);
        }

        private static void GenerateSprite(string name, Color fill, Color stroke, float strokeWidth, float radius, bool roundTL, bool roundTR, bool roundBL, bool roundBR)
        {
            Texture2D tex = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[TextureSize * TextureSize];
            
            float center = TextureSize / 2f;
            float innerSize = center - radius;

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;

                    // Determine if we are in a rounded corner quadrant
                    bool isCorner = false;
                    if (px < innerSize && py > TextureSize - innerSize && roundTL) isCorner = true;
                    if (px > TextureSize - innerSize && py > TextureSize - innerSize && roundTR) isCorner = true;
                    if (px < innerSize && py < innerSize && roundBL) isCorner = true;
                    if (px > TextureSize - innerSize && py < innerSize && roundBR) isCorner = true;

                    float distFromCenterOfCorner = 0;
                    if (isCorner)
                    {
                        float cx = (px < center) ? innerSize : TextureSize - innerSize;
                        float cy = (py < center) ? innerSize : TextureSize - innerSize;
                        distFromCenterOfCorner = Mathf.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
                    }
                    else
                    {
                        // For flat edges, distance is just distance to the closest border
                        float dx = Mathf.Max(0, Mathf.Abs(px - center) - innerSize);
                        float dy = Mathf.Max(0, Mathf.Abs(py - center) - innerSize);
                        distFromCenterOfCorner = Mathf.Sqrt(dx * dx + dy * dy);
                    }

                    // Anti-aliased alpha calculation
                    float outerAlpha = Mathf.Clamp01(radius - distFromCenterOfCorner + 0.5f);
                    float innerAlpha = Mathf.Clamp01(radius - strokeWidth - distFromCenterOfCorner + 0.5f);

                    // Blend colors
                    Color finalColor = Color.clear;
                    if (outerAlpha > 0)
                    {
                        // We are inside the shape
                        if (innerAlpha < 1f)
                        {
                            // We are in the stroke transition zone
                            // Blend between fill and stroke based on innerAlpha
                            finalColor = Color.Lerp(stroke, fill, innerAlpha);
                            // Apply overall shape alpha for the outer edge
                            finalColor.a *= outerAlpha;
                        }
                        else
                        {
                            // Pure fill
                            finalColor = fill;
                        }
                    }

                    pixels[y * TextureSize + x] = finalColor;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            byte[] pngData = tex.EncodeToPNG();
            string path = $"{GeneratedArtPath}/{name}.png";
            File.WriteAllBytes(path, pngData);
            
            Object.DestroyImmediate(tex);

            // Import settings
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Bilinear; // Smooth scaling
                importer.sRGBTexture = true;
                importer.alphaIsTransparency = true;
                
                TextureImporterSettings texSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(texSettings);
                texSettings.spriteMeshType = SpriteMeshType.FullRect;
                texSettings.spriteGenerateFallbackPhysicsShape = false;
                importer.SetTextureSettings(texSettings);
                
                // Set the 9-slice border
                // We add 2 extra pixels to the slice to ensure we are well into the flat area
                int sliceBorder = Mathf.CeilToInt(radius) + 2; 
                importer.spriteBorder = new Vector4(
                    roundTL || roundBL ? sliceBorder : 0, 
                    roundBL || roundBR ? sliceBorder : 0, 
                    roundTR || roundBR ? sliceBorder : 0, 
                    roundTL || roundTR ? sliceBorder : 0
                );
                
                importer.SaveAndReimport();
            }
        }

        private static void CreatePrefab(string prefabName, string spriteName)
        {
            string spritePath = $"{GeneratedArtPath}/{spriteName}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                Debug.LogError($"Could not find sprite {spriteName} for prefab {prefabName}");
                return;
            }

            string prefabPath = $"{GeneratedPrefabsPath}/{prefabName}.prefab";
            
            // Check if exists to support idempotency (updating existing rather than completely replacing if possible)
            // But since this is a simple frame, we will just construct a new one.
            GameObject go = new GameObject(prefabName);
            RectTransform rt = go.AddComponent<RectTransform>();
            Image img = go.AddComponent<Image>();
            
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white; // The color is baked into the sprite, so we keep Image color white

            // Save
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
        }
    }
}
