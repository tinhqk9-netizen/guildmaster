using UnityEngine;

namespace GuildMaster.Runtime.UI.Core
{
    /// <summary>
    /// Single source of truth for the temporary playtest UI's colors/sizes. No art assets — flat
    /// colors and Unity built-in UI primitives only, so every screen looks consistent without any
    /// per-screen guesswork.
    /// </summary>
    public static class UITemporaryTheme
    {
        // ── Backgrounds ─────────────────────────────────────────────────────────
        public static readonly Color BackgroundColor   = new Color(0.10f, 0.12f, 0.16f, 1f);
        public static readonly Color PanelPrimary      = new Color(0.16f, 0.17f, 0.20f, 1f);
        public static readonly Color PanelSecondary    = new Color(0.22f, 0.23f, 0.27f, 1f);
        public static readonly Color PanelTertiary     = new Color(0.19f, 0.20f, 0.24f, 1f);

        // ── Cards ────────────────────────────────────────────────────────────────
        public static readonly Color CardNormal        = new Color(0.18f, 0.26f, 0.38f, 1f);
        public static readonly Color CardSelected      = new Color(0.20f, 0.60f, 1.00f, 1f);
        public static readonly Color CardDisabled      = new Color(0.18f, 0.18f, 0.20f, 0.6f);
        public static readonly Color CardHover         = new Color(0.25f, 0.40f, 0.60f, 1f);

        // ── Buttons ──────────────────────────────────────────────────────────────
        public static readonly Color ButtonPrimary     = new Color(0.30f, 0.70f, 0.40f, 1f);
        public static readonly Color ButtonSecondary   = new Color(0.45f, 0.47f, 0.52f, 1f);
        public static readonly Color ButtonDanger      = new Color(0.80f, 0.30f, 0.25f, 1f);
        public static readonly Color ButtonDisabled    = new Color(0.22f, 0.22f, 0.24f, 0.7f);

        // ── Tabs ─────────────────────────────────────────────────────────────────
        public static readonly Color TabActive         = new Color(0.25f, 0.55f, 0.95f, 1f);
        public static readonly Color TabInactive       = new Color(0.25f, 0.26f, 0.30f, 1f);

        // ── Text ─────────────────────────────────────────────────────────────────
        public static readonly Color TextPrimary       = Color.white;
        public static readonly Color TextSecondary     = new Color(0.80f, 0.82f, 0.86f, 1f);
        public static readonly Color TextMuted         = new Color(0.55f, 0.57f, 0.62f, 1f);
        public static readonly Color TextHighlight     = new Color(1.00f, 0.85f, 0.30f, 1f);

        // ── Feedback ─────────────────────────────────────────────────────────────
        public static readonly Color SuccessColor      = new Color(0.35f, 0.80f, 0.40f, 1f);
        public static readonly Color FailureColor      = new Color(0.90f, 0.35f, 0.30f, 1f);
        public static readonly Color WarningColor      = new Color(0.95f, 0.65f, 0.10f, 1f);
        public static readonly Color PlaceholderIcon   = new Color(0.60f, 0.62f, 0.68f, 1f);

        // ── Progress bars ────────────────────────────────────────────────────────
        public static readonly Color BarBackground     = new Color(0.15f, 0.16f, 0.19f, 1f);
        public static readonly Color BarHp             = new Color(0.20f, 0.75f, 0.30f, 1f);
        public static readonly Color BarHpLow          = new Color(0.85f, 0.30f, 0.20f, 1f);
        public static readonly Color BarXp             = new Color(0.40f, 0.60f, 0.90f, 1f);
        public static readonly Color BarCraft          = new Color(0.80f, 0.60f, 0.10f, 1f);
        public static readonly Color BarQuest          = new Color(0.50f, 0.35f, 0.80f, 1f);
        public static readonly Color BarDungeon        = new Color(0.25f, 0.55f, 0.95f, 1f);

        // ── Badge ────────────────────────────────────────────────────────────────
        public static readonly Color BadgeColor        = new Color(0.90f, 0.30f, 0.20f, 1f);

        // ── Sizes ────────────────────────────────────────────────────────────────
        public const float HeaderHeight     = 130f;
        public const float SummaryHeight    =  64f;
        public const float TabBarHeight     =  80f;
        public const float DetailHeight     = 280f;
        public const float ActionBarHeight  = 130f;
        public const float ProgressBarHeight =  20f;

        public const float CardWidth        = 310f;
        public const float CardHeight       = 140f;
        public const float GuestCardHeight  = 160f;  // Tavern cards need more room for stats
        public const float HudButtonWidth   = 340f;
        public const float HudButtonHeight  = 160f;

        // ── Font sizes ───────────────────────────────────────────────────────────
        public const int TitleFontSize         = 34;
        public const int SectionFontSize       = 24;
        public const int BodyFontSize          = 22;
        public const int SmallFontSize         = 18;
        public const int CardTitleFontSize     = 20;
        public const int CardSubtitleFontSize  = 16;
        public const int BadgeFontSize         = 14;

        public const float Padding = 16f;
    }
}
