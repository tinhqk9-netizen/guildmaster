using System;
using System.Linq;
using GuildMaster.Runtime.UI.Legacy;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Headquarters
{
    /// <summary>
    /// Runtime row builder shared by <see cref="WorkshopDialog"/>'s queue/completed list and
    /// <see cref="WorkshopRecipePanel"/>'s recipe list. Pure view helper — never touches services.
    /// </summary>
    internal static class WorkshopRowBuilder
    {
        public static GameObject CreateRow(Transform parent, Sprite borderSprite, string definitionId, int stackCount,
            string statusText, bool showProgress, float progress, string actionLabel, Action onAction)
        {
            var row = new GameObject($"Row_{definitionId ?? "unknown"}", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);

            var border = row.GetComponent<Image>();
            border.sprite = borderSprite;
            border.type = borderSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            border.color = borderSprite != null ? Color.white : new Color(1f, 1f, 1f, 0.08f);

            var rowLayout = row.GetComponent<LayoutElement>();
            rowLayout.preferredHeight = 128f;
            rowLayout.minHeight = 128f;

            var rowGroup = row.GetComponent<HorizontalLayoutGroup>();
            rowGroup.padding = new RectOffset(16, 16, 12, 12);
            rowGroup.spacing = 16f;
            rowGroup.childAlignment = TextAnchor.MiddleLeft;
            rowGroup.childControlWidth = true;
            rowGroup.childControlHeight = true;
            rowGroup.childForceExpandWidth = false;
            rowGroup.childForceExpandHeight = false;

            // ── Icon ─────────────────────────────────────────────────────────────
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconGo.transform.SetParent(row.transform, false);
            var icon = iconGo.GetComponent<Image>();
            icon.preserveAspect = true;
            Sprite iconSprite = !string.IsNullOrEmpty(definitionId) ? LegacySpriteRegistry.GetItemSprite(definitionId) : null;
            icon.sprite = iconSprite;
            icon.color = iconSprite != null ? Color.white : new Color(1f, 1f, 1f, 0.25f);
            var iconLayout = iconGo.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = 96f;
            iconLayout.preferredHeight = 96f;

            // ── Info column (name, status, optional progress bar) ──────────────────
            var infoGo = new GameObject("Info", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            infoGo.transform.SetParent(row.transform, false);
            var infoLayout = infoGo.GetComponent<LayoutElement>();
            infoLayout.flexibleWidth = 1f;
            infoLayout.preferredHeight = 104f;
            var infoGroup = infoGo.GetComponent<VerticalLayoutGroup>();
            infoGroup.spacing = 4f;
            infoGroup.childAlignment = TextAnchor.MiddleLeft;
            infoGroup.childControlWidth = true;
            infoGroup.childControlHeight = true;
            infoGroup.childForceExpandHeight = false;

            string name = FormatId(definitionId);
            string title = stackCount > 1 ? $"{name} x{stackCount}" : name;
            AddText(infoGo.transform, "Name", title, 24, LegacyUITheme.DimWhite, FontStyle.Bold, 34f);
            AddText(infoGo.transform, "Status", statusText, 20, LegacyUITheme.BrassBorder, FontStyle.Normal, 28f);

            if (showProgress)
            {
                var barBg = new GameObject("ProgressBg", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                barBg.transform.SetParent(infoGo.transform, false);
                barBg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
                var barBgLayout = barBg.GetComponent<LayoutElement>();
                barBgLayout.preferredHeight = 14f;
                barBgLayout.flexibleWidth = 1f;

                var barFillGo = new GameObject("ProgressFill", typeof(RectTransform), typeof(Image));
                barFillGo.transform.SetParent(barBg.transform, false);
                var fillRect = (RectTransform)barFillGo.transform;
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
                barFillGo.GetComponent<Image>().color = LegacyUITheme.BrassBorder;
            }

            // ── Action button (Collect — only when provided; no Cancel exists) ─────
            if (!string.IsNullOrEmpty(actionLabel) && onAction != null)
            {
                var actionGo = CreateActionButton(row.transform, actionLabel, onAction);
                var actionLayout = actionGo.GetComponent<LayoutElement>() ?? actionGo.AddComponent<LayoutElement>();
                actionLayout.preferredWidth = 160f;
                actionLayout.preferredHeight = 76f;
            }

            return row;
        }

        private static GameObject CreateActionButton(Transform parent, string label, Action onClick)
        {
            var go = new GameObject("ActionButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = LegacyUITheme.BrassBorder;
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => onClick());

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var rect = textGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 22;
            text.fontStyle = FontStyle.Bold;
            text.color = LegacyUITheme.CardviewDarkBackground;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;
            return go;
        }

        private static Text AddText(Transform parent, string name, string value, int size, Color color, FontStyle style, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = color;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.text = value;
            go.GetComponent<LayoutElement>().preferredHeight = height;
            return text;
        }

        public static string FormatId(string id)
        {
            if (string.IsNullOrEmpty(id)) return "Item";
            string normalized = id.Replace('_', ' ').Replace('-', ' ').Trim();
            return string.Join(" ", normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Length == 1 ? part.ToUpperInvariant() : char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant()));
        }
    }
}
