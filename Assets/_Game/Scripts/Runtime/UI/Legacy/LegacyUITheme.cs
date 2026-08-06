using UnityEngine;

namespace GuildMaster.Runtime.UI.Legacy
{
    /// <summary>
    /// Static theme foundation for the Legacy UI Reconstruction.
    /// Uses fixed readonly values mapping directly from the deep audit.
    /// DP to Pixel conversion factor: 3 (Based on 1080x1920 targeting roughly xxhdpi Android screens).
    /// </summary>
    public static class LegacyUITheme
    {
        // --- Scale Configuration ---
        public const int DpToPxMultiplier = 3;

        public static int DP(int dpValue) => dpValue * DpToPxMultiplier;
        public static float DP(float dpValue) => dpValue * DpToPxMultiplier;

        // --- Core Colors ---
        // Parsed directly from deep_game_colors.json and unity_legacy_shape_mapping.md
        
        public static readonly Color CardviewDarkBackground = HexToColor("#1e1e1e"); // Used for dialog_border
        public static readonly Color StandardBackground = HexToColor("#1fffffff"); // Used for object_border_*
        public static readonly Color StandardBackgroundUnavailable = HexToColor("#1fff0000"); // Red tint for unavailable items
        public static readonly Color AscendedBackground = HexToColor("#1fffdb7f"); // Used for object_border_ascended
        public static readonly Color ExtraOpaqueBackground = HexToColor("#40ffffff");
        
        public static readonly Color DimWhite = HexToColor("#c8c8c8");
        public static readonly Color BrassBorder = HexToColor("#befaa03e");
        public static readonly Color AscendedUnit = HexToColor("#ffdb7f");
        public static readonly Color GreyBorder = HexToColor("#505050");
        public static readonly Color Failure = HexToColor("#c83232");
        public static readonly Color Black = HexToColor("#000000");
        public static readonly Color Transparent = new Color(0, 0, 0, 0);

        // --- Core Shapes/Dimensions ---
        
        // 10dp is the universal corner radius in the game
        public static readonly int CornerRadiusDP = 10;
        public static readonly int CornerRadiusPX = DP(CornerRadiusDP); // 30px

        // Border widths vary between 1dp, 2dp, and 3dp
        public static readonly int Border1DpPX = DP(1); // 3px (standard item border)
        public static readonly int Border2DpPX = DP(2); // 6px (buy button, rarity borders)
        public static readonly int Border3DpPX = DP(3); // 9px (dialog border)

        // Standard dialogs are ~90% of screen width in many cases, but for base margins:
        public static readonly int StandardMarginDP = 16;
        public static readonly int StandardMarginPX = DP(StandardMarginDP);

        // --- Helper ---
        private static Color HexToColor(string hex)
        {
            // Handle ARGB vs RGB hex strings from Android format (#AARRGGBB vs #RRGGBB)
            if (hex.Length == 9) // #AARRGGBB
            {
                if (ColorUtility.TryParseHtmlString("#" + hex.Substring(3, 6) + hex.Substring(1, 2), out Color c))
                {
                    return c;
                }
            }
            else if (ColorUtility.TryParseHtmlString(hex, out Color c))
            {
                return c;
            }
            
            Debug.LogError($"Failed to parse color hex: {hex}");
            return Color.magenta; // Error color
        }
    }
}
