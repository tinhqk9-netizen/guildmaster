#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Events;
using GuildMaster.Runtime.UI.Core;

namespace GuildMaster.Editor.UI
{
    /// <summary>References into one screen's scaffold, handed back to the caller so it can wire the
    /// specific serialized fields (card containers, detail text, action buttons) for that screen.</summary>
    public class ScreenScaffold
    {
        public RectTransform HeaderRow;
        public Text HeaderTitle;
        public Button BackButton;
        public RectTransform SummaryRow;
        public RectTransform ContentArea;   // the ScrollRect's Content — put card containers as children of this
        public RectTransform DetailPanel;
        public Text DetailText;
        public RectTransform ActionBar;
    }

    /// <summary>
    /// Builds the shared Header / Summary / ScrollRect-Content / Detail / ActionBar scaffold used by
    /// every gameplay screen, entirely with LayoutGroup/ContentSizeFitter/ScrollRect — no manual
    /// anchoredPosition — so nothing overlaps regardless of list length or text length. Idempotent:
    /// re-running finds existing named children instead of duplicating them.
    /// </summary>
    public static class UIScreenLayoutBuilder
    {
        public static ScreenScaffold Build(Transform screenRoot, string title)
        {
            var scaffold = new ScreenScaffold();

            var rootRt = (RectTransform)screenRoot;
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            var rootPanel = EnsureComponent<Image>(screenRoot.gameObject);
            rootPanel.color = UITemporaryTheme.BackgroundColor;

            var rootGroup = EnsureComponent<VerticalLayoutGroup>(screenRoot.gameObject);
            rootGroup.padding = new RectOffset(0, 0, 0, 0);
            rootGroup.spacing = 6;
            rootGroup.childForceExpandWidth = true;
            rootGroup.childForceExpandHeight = false;
            rootGroup.childControlWidth = true;
            rootGroup.childControlHeight = true;

            // --- Header ---
            var header = FindOrCreateChild(screenRoot, "Header");
            scaffold.HeaderRow = header;
            SetLayoutElement(header, preferredHeight: UITemporaryTheme.HeaderHeight, flexibleHeight: 0);
            var headerBg = EnsureComponent<Image>(header.gameObject);
            headerBg.color = UITemporaryTheme.PanelSecondary;
            var headerLayout = EnsureComponent<HorizontalLayoutGroup>(header.gameObject);
            headerLayout.padding = new RectOffset(16, 16, 10, 10);
            headerLayout.spacing = 16;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = true;
            headerLayout.childAlignment = TextAnchor.MiddleLeft;

            var backGo = FindOrCreateChild(header, "Btn_Back");
            SetLayoutElement(backGo, preferredWidth: 140, preferredHeight: 44);
            var backImg = EnsureComponent<Image>(backGo.gameObject);
            backImg.color = UITemporaryTheme.ButtonSecondary;
            var backBtn = EnsureComponent<Button>(backGo.gameObject);
            backBtn.targetGraphic = backImg;
            scaffold.BackButton = backBtn;
            EnsureTextChild(backGo, "Text", "Back", UITemporaryTheme.BodyFontSize, UITemporaryTheme.TextPrimary, TextAnchor.MiddleCenter);

            var titleGo = FindOrCreateChild(header, "Title");
            SetLayoutElement(titleGo, flexibleWidth: 1, preferredHeight: 44);
            var titleTxt = EnsureComponent<Text>(titleGo.gameObject);
            titleTxt.text = title;
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.fontSize = UITemporaryTheme.TitleFontSize;
            titleTxt.color = UITemporaryTheme.TextPrimary;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.horizontalOverflow = HorizontalWrapMode.Overflow; // Use Overflow to fix ugly text wrapping like "Works hop"
            scaffold.HeaderTitle = titleTxt;

            // --- Summary row (small status line under header; screens fill its text themselves) ---
            var summary = FindOrCreateChild(screenRoot, "SummaryRow");
            scaffold.SummaryRow = summary;
            SetLayoutElement(summary, preferredHeight: 32, flexibleHeight: 0);
            var summaryBg = EnsureComponent<Image>(summary.gameObject);
            summaryBg.color = UITemporaryTheme.PanelPrimary;
            var summaryTextGo = FindOrCreateChild(summary, "SummaryText");
            var summaryRt = (RectTransform)summaryTextGo;
            summaryRt.anchorMin = Vector2.zero;
            summaryRt.anchorMax = Vector2.one;
            summaryRt.offsetMin = new Vector2(16, 4);
            summaryRt.offsetMax = new Vector2(-16, -4);
            var summaryTxt = EnsureComponent<Text>(summaryTextGo.gameObject);
            summaryTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            summaryTxt.fontSize = UITemporaryTheme.BodyFontSize;
            summaryTxt.color = UITemporaryTheme.TextSecondary;
            summaryTxt.alignment = TextAnchor.MiddleLeft;

            // --- Content: ScrollRect with Viewport(mask) + Content(GridLayoutGroup) ---
            var scrollGo = FindOrCreateChild(screenRoot, "ContentScroll");
            SetLayoutElement(scrollGo, flexibleHeight: 1);
            var scrollBg = EnsureComponent<Image>(scrollGo.gameObject);
            scrollBg.color = UITemporaryTheme.PanelPrimary;
            var scrollRect = EnsureComponent<ScrollRect>(scrollGo.gameObject);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewportGo = FindOrCreateChild(scrollGo, "Viewport");
            var viewportRt = (RectTransform)viewportGo;
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;
            EnsureComponent<RectMask2D>(viewportGo.gameObject);
            scrollRect.viewport = viewportRt;

            var contentGo = FindOrCreateChild(viewportGo, "Content");
            var contentRt = (RectTransform)contentGo;
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1f);
            var grid = EnsureComponent<GridLayoutGroup>(contentGo.gameObject);
            grid.padding = new RectOffset(12, 12, 12, 12);
            grid.spacing = new Vector2(10, 10);
            grid.cellSize = new Vector2(UITemporaryTheme.CardWidth, UITemporaryTheme.CardHeight);
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            var fitter = EnsureComponent<ContentSizeFitter>(contentGo.gameObject);
            fitter.enabled = true;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            scrollRect.content = contentRt;
            scaffold.ContentArea = contentRt;

            // --- Detail panel ---
            var detail = FindOrCreateChild(screenRoot, "DetailPanel");
            scaffold.DetailPanel = detail;
            SetLayoutElement(detail, preferredHeight: 160, flexibleHeight: 0);
            var detailBg = EnsureComponent<Image>(detail.gameObject);
            detailBg.color = UITemporaryTheme.PanelSecondary;
            var detailTextGo = FindOrCreateChild(detail, "DetailText");
            var detailRt = (RectTransform)detailTextGo;
            detailRt.anchorMin = Vector2.zero;
            detailRt.anchorMax = Vector2.one;
            detailRt.offsetMin = new Vector2(16, 16);
            detailRt.offsetMax = new Vector2(-16, -16);
            var detailTxt = EnsureComponent<Text>(detailTextGo.gameObject);
            detailTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            detailTxt.fontSize = UITemporaryTheme.BodyFontSize;
            detailTxt.color = UITemporaryTheme.TextPrimary;
            detailTxt.alignment = TextAnchor.UpperLeft;
            detailTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            detailTxt.verticalOverflow = VerticalWrapMode.Overflow;
            scaffold.DetailText = detailTxt;

            // --- Action bar ---
            var actionBar = FindOrCreateChild(screenRoot, "ActionBar");
            scaffold.ActionBar = actionBar;
            SetLayoutElement(actionBar, preferredHeight: 48, flexibleHeight: 0);
            var actionBg = EnsureComponent<Image>(actionBar.gameObject);
            actionBg.color = UITemporaryTheme.PanelPrimary;
            var actionLayout = EnsureComponent<HorizontalLayoutGroup>(actionBar.gameObject);
            actionLayout.padding = new RectOffset(16, 16, 4, 4);
            actionLayout.spacing = 12;
            actionLayout.childForceExpandWidth = true;
            actionLayout.childForceExpandHeight = true;
            actionLayout.childAlignment = TextAnchor.MiddleCenter;

            // Wire Back -> UIService.Back() the same way every other Back button in the project does
            // (handled by UIRuntimeBootstrap.WireBackButton at runtime via the "Btn_Back" name) — no
            // persistent listener needed here.

            return scaffold;
        }

        /// <summary>Adds one action button into a screen's ActionBar, replacing any previous binding by name.</summary>
        public static Button AddActionButton(RectTransform actionBar, string name, string label, bool primary, UnityEngine.Events.UnityAction action)
        {
            var go = FindOrCreateChild(actionBar, name);
            var img = EnsureComponent<Image>(go.gameObject);
            img.color = primary ? UITemporaryTheme.ButtonPrimary : UITemporaryTheme.ButtonSecondary;
            var btn = EnsureComponent<Button>(go.gameObject);
            btn.targetGraphic = img;
            EnsureTextChild(go, "Text", label, UITemporaryTheme.BodyFontSize, Color.black, TextAnchor.MiddleCenter);

            while (btn.onClick.GetPersistentEventCount() > 0)
                UnityEventTools.RemovePersistentListener(btn.onClick, 0);
            if (action != null) UnityEventTools.AddPersistentListener(btn.onClick, action);
            return btn;
        }

        private static RectTransform FindOrCreateChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return (RectTransform)existing;
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        private static void SetLayoutElement(RectTransform rt, float preferredWidth = -1, float preferredHeight = -1, float flexibleWidth = -1, float flexibleHeight = -1)
        {
            var le = EnsureComponent<LayoutElement>(rt.gameObject);
            if (preferredWidth >= 0) le.preferredWidth = preferredWidth;
            if (preferredHeight >= 0) le.preferredHeight = preferredHeight;
            if (flexibleWidth >= 0) le.flexibleWidth = flexibleWidth;
            if (flexibleHeight >= 0) le.flexibleHeight = flexibleHeight;
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null) c = go.AddComponent<T>();
            return c;
        }

        private static Text EnsureTextChild(RectTransform parent, string name, string content, int fontSize, Color color, TextAnchor alignment)
        {
            var go = FindOrCreateChild(parent, name);
            var rt = (RectTransform)go;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var txt = EnsureComponent<Text>(go.gameObject);
            txt.text = content;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = alignment;
            txt.raycastTarget = false;
            return txt;
        }
    }
}
#endif
