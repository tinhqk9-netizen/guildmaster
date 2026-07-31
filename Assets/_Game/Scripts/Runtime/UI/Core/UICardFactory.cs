using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace GuildMaster.Runtime.UI.Core
{
    /// <summary>
    /// Builds real, clickable entity cards at runtime (Image + Title + Subtitle + Button) sized to sit
    /// inside a GridLayoutGroup/VerticalLayoutGroup container — no manual anchoredPosition, so cards
    /// never overlap each other regardless of list length. Used by every gameplay screen so the player
    /// selects an entity by clicking its card instead of relying only on index-0 or Next/Prev.
    ///
    /// Also provides helpers for progress bars, stat rows, tab buttons, and feedback labels.
    /// </summary>
    public static class UICardFactory
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Container management
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Clears every previously-built card from a container without leaking listeners.</summary>
        public static void ClearContainer(RectTransform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
                Object.Destroy(container.GetChild(i).gameObject);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Divider (section label)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Non-interactive section label card, used to separate two lists sharing one grid.</summary>
        public static void CreateDivider(RectTransform container, string label)
        {
            var go = new GameObject("Divider_" + label, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(container, false);
            var layout = go.AddComponent<LayoutElement>();
            layout.preferredWidth  = UITemporaryTheme.CardWidth;
            layout.preferredHeight = 44;
            layout.minHeight       = 44;
            layout.flexibleWidth   = 1;
            var bg = go.AddComponent<Image>();
            bg.color = UITemporaryTheme.PanelTertiary;
            var textGo = new GameObject("Text", typeof(RectTransform));
            var textRt = (RectTransform)textGo.transform;
            textRt.SetParent(rt, false);
            textRt.anchorMin  = Vector2.zero;
            textRt.anchorMax  = Vector2.one;
            textRt.offsetMin  = new Vector2(12, 0);
            textRt.offsetMax  = new Vector2(-12, 0);
            var txt = textGo.AddComponent<Text>();
            txt.text           = $"— {label} —";
            txt.font           = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize       = UITemporaryTheme.SectionFontSize;
            txt.color          = UITemporaryTheme.TextHighlight;
            txt.alignment      = TextAnchor.MiddleCenter;
            txt.raycastTarget  = false;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Main entity card
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates one clickable card. onClick is the real selection handler.
        /// Set preferredHeight override to 0 to use the theme default (CardHeight).
        /// </summary>
        public static Button CreateCard(RectTransform container, string title, string subtitle,
            bool isSelected, bool isInteractable, UnityAction onClick, float preferredHeight = 0)
        {
            float cardH = preferredHeight > 0 ? preferredHeight : UITemporaryTheme.CardHeight;

            var go  = new GameObject("Card_" + title, typeof(RectTransform));
            var rt  = (RectTransform)go.transform;
            rt.SetParent(container, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = cardH;
            layout.minHeight       = cardH;
            layout.flexibleWidth   = 1;

            var bg = go.AddComponent<Image>();
            bg.color = isSelected ? UITemporaryTheme.CardSelected
                     : isInteractable ? UITemporaryTheme.CardNormal
                     : UITemporaryTheme.CardDisabled;

            var hLayout = go.AddComponent<HorizontalLayoutGroup>();
            hLayout.padding              = new RectOffset(10, 10, 10, 10);
            hLayout.spacing              = 10;
            hLayout.childForceExpandWidth  = false;
            hLayout.childForceExpandHeight = true;
            hLayout.childAlignment       = TextAnchor.MiddleLeft;
            hLayout.childControlWidth    = true;
            hLayout.childControlHeight   = true;

            // Icon placeholder (left)
            var iconGo     = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(rt, false);
            var iconLayout = iconGo.AddComponent<LayoutElement>();
            iconLayout.preferredWidth  = 54;
            iconLayout.preferredHeight = 54;
            iconLayout.flexibleHeight  = 0;
            iconLayout.flexibleWidth   = 0;
            var iconImg  = iconGo.AddComponent<Image>();
            iconImg.color        = UITemporaryTheme.PlaceholderIcon;
            iconImg.raycastTarget = false;

            // Text column (right)
            var colGo     = new GameObject("TextColumn", typeof(RectTransform));
            colGo.transform.SetParent(rt, false);
            var colLayout = colGo.AddComponent<LayoutElement>();
            colLayout.flexibleWidth = 1;
            var colGroup  = colGo.AddComponent<VerticalLayoutGroup>();
            colGroup.childForceExpandWidth  = true;
            colGroup.childForceExpandHeight = false;
            colGroup.spacing        = 2;
            colGroup.childAlignment = TextAnchor.UpperLeft;
            colGroup.childControlWidth  = true;
            colGroup.childControlHeight = true;

            AddText(colGo.transform, "Title", title,
                UITemporaryTheme.CardTitleFontSize, UITemporaryTheme.TextPrimary,
                TextAnchor.UpperLeft, preferredHeight: 28);

            AddText(colGo.transform, "Subtitle", subtitle,
                UITemporaryTheme.CardSubtitleFontSize, UITemporaryTheme.TextSecondary,
                TextAnchor.UpperLeft, flexible: true);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.interactable  = isInteractable;
            if (isInteractable && onClick != null) btn.onClick.AddListener(onClick);

            return btn;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Progress bar
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a labelled horizontal progress bar.
        /// Returns the fill Image so the caller can update it at refresh time.
        /// </summary>
        public static Image CreateProgressBar(Transform parent, string name, string label,
            float value01, Color fillColor, float preferredHeight = 0)
        {
            float h = preferredHeight > 0 ? preferredHeight : UITemporaryTheme.ProgressBarHeight;

            var go  = new GameObject(name, typeof(RectTransform));
            var rt  = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            var le  = go.AddComponent<LayoutElement>();
            le.preferredHeight = h + 20;
            le.flexibleWidth   = 1;

            var vg  = go.AddComponent<VerticalLayoutGroup>();
            vg.spacing = 2;
            vg.childForceExpandWidth = true;
            vg.childForceExpandHeight = false;
            vg.childControlWidth = true;
            vg.childControlHeight = true;

            // Label
            if (!string.IsNullOrEmpty(label))
                AddText(go.transform, "Label", label,
                    UITemporaryTheme.SmallFontSize, UITemporaryTheme.TextSecondary,
                    TextAnchor.MiddleLeft, preferredHeight: 18);

            // Bar track
            var trackGo  = new GameObject("Track", typeof(RectTransform));
            trackGo.transform.SetParent(go.transform, false);
            var trackLe  = trackGo.AddComponent<LayoutElement>();
            trackLe.preferredHeight = h;
            trackLe.flexibleWidth   = 1;
            var trackImg = trackGo.AddComponent<Image>();
            trackImg.color = UITemporaryTheme.BarBackground;

            // Fill
            var fillGo  = new GameObject("Fill", typeof(RectTransform));
            var fillRt  = (RectTransform)fillGo.transform;
            fillRt.SetParent(trackGo.transform, false);
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(Mathf.Clamp01(value01), 1f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            var fillImg  = fillGo.AddComponent<Image>();
            fillImg.color = fillColor;

            return fillImg;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Stat row (label: value)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Creates a single key:value row inside a detail panel.</summary>
        public static void CreateStatRow(Transform parent, string label, string value)
        {
            var go  = new GameObject("StatRow_" + label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le  = go.AddComponent<LayoutElement>();
            le.preferredHeight = 26;
            le.flexibleWidth   = 1;
            var hg  = go.AddComponent<HorizontalLayoutGroup>();
            hg.childForceExpandWidth  = false;
            hg.childForceExpandHeight = true;
            hg.childControlWidth  = true;
            hg.childControlHeight = true;
            hg.spacing = 8;

            var lblGo  = new GameObject("Label", typeof(RectTransform));
            lblGo.transform.SetParent(go.transform, false);
            var lblLe  = lblGo.AddComponent<LayoutElement>();
            lblLe.preferredWidth = 140;
            lblLe.flexibleWidth  = 0;
            var lblTxt = lblGo.AddComponent<Text>();
            lblTxt.text      = label;
            lblTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            lblTxt.fontSize  = UITemporaryTheme.SmallFontSize;
            lblTxt.color     = UITemporaryTheme.TextSecondary;
            lblTxt.alignment = TextAnchor.MiddleLeft;
            lblTxt.raycastTarget = false;

            var valGo  = new GameObject("Value", typeof(RectTransform));
            valGo.transform.SetParent(go.transform, false);
            var valLe  = valGo.AddComponent<LayoutElement>();
            valLe.flexibleWidth = 1;
            var valTxt = valGo.AddComponent<Text>();
            valTxt.text      = value;
            valTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            valTxt.fontSize  = UITemporaryTheme.SmallFontSize;
            valTxt.color     = UITemporaryTheme.TextPrimary;
            valTxt.alignment = TextAnchor.MiddleLeft;
            valTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            valTxt.raycastTarget = false;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Tab button
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Creates a tab button in a tab bar. Returns the Button so the caller can add onClick.</summary>
        public static Button CreateTabButton(Transform tabBar, string name, string label,
            bool isActive, UnityAction onClick)
        {
            var go  = new GameObject(name, typeof(RectTransform));
            var rt  = (RectTransform)go.transform;
            rt.SetParent(tabBar, false);
            var le  = go.AddComponent<LayoutElement>();
            le.flexibleWidth   = 1;
            le.preferredHeight = UITemporaryTheme.TabBarHeight;
            var img = go.AddComponent<Image>();
            img.color = isActive ? UITemporaryTheme.TabActive : UITemporaryTheme.TabInactive;

            AddText(go.transform, "Text", label,
                UITemporaryTheme.BodyFontSize,
                isActive ? UITemporaryTheme.TextPrimary : UITemporaryTheme.TextSecondary,
                TextAnchor.MiddleCenter);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(onClick);
            return btn;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Feedback label
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Creates a feedback / status label inside a panel.</summary>
        public static Text CreateFeedbackLabel(Transform parent, string name)
        {
            var go  = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le  = go.AddComponent<LayoutElement>();
            le.preferredHeight = 36;
            le.flexibleWidth   = 1;
            var txt = go.AddComponent<Text>();
            txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize  = UITemporaryTheme.SmallFontSize;
            txt.color     = UITemporaryTheme.SuccessColor;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.text      = "";
            return txt;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Private helpers
        // ─────────────────────────────────────────────────────────────────────────

        private static Text AddText(Transform parent, string name, string content,
            int fontSize, Color color, TextAnchor alignment,
            float preferredHeight = -1, bool flexible = false)
        {
            var go  = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le  = go.AddComponent<LayoutElement>();
            if (preferredHeight >= 0) le.preferredHeight = preferredHeight;
            if (flexible) le.flexibleHeight = 1;
            le.flexibleWidth = 1;
            var txt = go.AddComponent<Text>();
            txt.text      = content;
            txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize  = fontSize;
            txt.color     = color;
            txt.alignment = alignment;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow   = VerticalWrapMode.Truncate;
            txt.raycastTarget      = false;
            return txt;
        }
    }
}
