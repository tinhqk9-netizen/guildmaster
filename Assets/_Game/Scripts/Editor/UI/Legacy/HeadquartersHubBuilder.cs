#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Legacy;
using GuildMaster.Runtime.UI.Shell;

namespace GuildMaster.Editor.UI.Legacy
{
    /// <summary>
    /// Phase 4: replaces the Headquarters tab's placeholder label with a ScrollView of 6 building
    /// cards (Quarters/Tavern/Storage/Market/Workshop/Shelter), matching the legacy layout
    /// (P2-01 in claude_ui_reconstruction_handoff.md). Uses only Phase 1 sign_* sprites and
    /// Phase 2 theme colors — no new visual style.
    ///
    /// Idempotent: destroys and rebuilds "HeadquartersHubRoot" (under Tab_Headquarters) and the
    /// "PopupTemplate" object each run.
    /// </summary>
    public static class HeadquartersHubBuilder
    {
        private static readonly string[] BuildingIds =
            { "Quarters", "Tavern", "Storage", "Market", "Workshop", "Shelter" };
        private static readonly string[] SignNames =
            { "sign_quarters", "sign_tavern", "sign_storage", "sign_market", "sign_workshop", "sign_shelter" };

        [MenuItem("Tools/Guild Master/Legacy UI/Build Headquarters Hub")]
        public static void BuildHub()
        {
            GameObject tabHq = GameObject.Find("AppShellCanvas/TabContentRoot/Tab_Headquarters");
            if (tabHq == null)
            {
                Debug.LogError("[HeadquartersHubBuilder] 'Tab_Headquarters' not found — run Phase 3's Build App Shell first.");
                return;
            }

            // Idempotent cleanup
            var existingRoot = tabHq.transform.Find("HeadquartersHubRoot");
            if (existingRoot != null) Object.DestroyImmediate(existingRoot.gameObject);
            var existingTemplate = tabHq.transform.Find("PopupTemplate");
            if (existingTemplate != null) Object.DestroyImmediate(existingTemplate.gameObject);

            // Hide (don't delete) the Phase 3 placeholder label so Phase 3's own screenshots/tests
            // still make sense if re-run, while the hub visually takes over.
            var oldLabel = tabHq.transform.Find("Label");
            if (oldLabel != null) oldLabel.gameObject.SetActive(false);

            // ─── ScrollView ─────────────────────────────────────────────
            GameObject hubRoot = CreateStretch("HeadquartersHubRoot", tabHq.transform);

            GameObject scrollGo = CreateStretch("ScrollView", hubRoot.transform);
            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollGo.AddComponent<RectMask2D>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewport = CreateStretch("Viewport", scrollGo.transform);
            viewport.AddComponent<RectMask2D>();
            scrollRect.viewport = (RectTransform)viewport.transform;

            GameObject content = CreateUIObject("Content", viewport.transform);
            var contentRt = (RectTransform)content.transform;
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0, 0);

            var vlayout = content.AddComponent<VerticalLayoutGroup>();
            vlayout.padding = new RectOffset(24, 24, 24, 24);
            vlayout.spacing = 20;
            vlayout.childForceExpandWidth = true;
            vlayout.childForceExpandHeight = false;
            vlayout.childControlHeight = true;
            vlayout.childControlWidth = true;

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;

            var hub = hubRoot.AddComponent<HeadquartersHubController>();
            var cards = new BuildingCardView[BuildingIds.Length];

            for (int i = 0; i < BuildingIds.Length; i++)
            {
                cards[i] = BuildCard(content.transform, BuildingIds[i], SignNames[i]);
            }

            // ─── Popup template (inactive, cloned per open) ─────────────
            GameObject template = BuildPopupTemplate(tabHq.transform);

            // ─── Wire HeadquartersHubController ─────────────────────────
            var so = new SerializedObject(hub);
            so.FindProperty("_quartersCard").objectReferenceValue = cards[0];
            so.FindProperty("_tavernCard").objectReferenceValue = cards[1];
            so.FindProperty("_storageCard").objectReferenceValue = cards[2];
            so.FindProperty("_marketCard").objectReferenceValue = cards[3];
            so.FindProperty("_workshopCard").objectReferenceValue = cards[4];
            so.FindProperty("_shelterCard").objectReferenceValue = cards[5];
            so.FindProperty("_popupPrefabSource").objectReferenceValue = template;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Wire into AppShellController so Initialize() calls hub.Initialize() automatically.
            GameObject shellGo = GameObject.Find("AppShellCanvas/AppShellController");
            if (shellGo != null)
            {
                var shellSo = new SerializedObject(shellGo.GetComponent<AppShellController>());
                var prop = shellSo.FindProperty("_headquartersHub");
                if (prop != null)
                {
                    prop.objectReferenceValue = hub;
                    shellSo.ApplyModifiedPropertiesWithoutUndo();
                }
                else
                {
                    Debug.LogWarning("[HeadquartersHubBuilder] AppShellController has no '_headquartersHub' field — recompile after adding it, then re-run this tool.");
                }
            }

            EditorUtility.SetDirty(tabHq);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[HeadquartersHubBuilder] Headquarters Hub built: 6 building cards in a ScrollView, popup template wired.");
        }

        private static BuildingCardView BuildCard(Transform parent, string title, string signName)
        {
            GameObject card = CreateUIObject($"Card_{title}", parent);
            var le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 140;
            le.minHeight = 140;

            var img = card.AddComponent<Image>();
            img.color = LegacyUITheme.StandardBackground;

            var border = card.AddComponent<Outline>();
            border.effectColor = LegacyUITheme.DimWhite;
            border.effectDistance = new Vector2(2, -2);

            var btn = card.AddComponent<Button>();
            btn.targetGraphic = img;

            // Sign icon, overlaid from the left (legacy: 42x54dp -> ~126x162px at 3x, scaled down
            // to fit a 140px-tall card comfortably)
            GameObject signGo = CreateUIObject("Sign", card.transform);
            var signImg = signGo.AddComponent<Image>();
            signImg.sprite = LegacySpriteRegistry.GetSprite(signName);
            signImg.preserveAspect = true;
            var signRt = (RectTransform)signGo.transform;
            signRt.anchorMin = signRt.anchorMax = new Vector2(0, 0.5f);
            signRt.pivot = new Vector2(0, 0.5f);
            signRt.anchoredPosition = new Vector2(24, 0);
            signRt.sizeDelta = new Vector2(84, 108);

            GameObject titleGo = CreateText(card.transform, "Title", title, 34, LegacyUITheme.DimWhite, TextAnchor.MiddleCenter);
            var titleRt = (RectTransform)titleGo.transform;
            titleRt.anchorMin = new Vector2(0, 0.5f);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.offsetMin = new Vector2(140, 0);
            titleRt.offsetMax = new Vector2(-24, -10);
            ((Text)titleGo.GetComponent<Text>()).fontStyle = FontStyle.Bold;

            GameObject statusGo = CreateText(card.transform, "Status", "", 26, LegacyUITheme.DimWhite, TextAnchor.MiddleCenter);
            var statusRt = (RectTransform)statusGo.transform;
            statusRt.anchorMin = new Vector2(0, 0);
            statusRt.anchorMax = new Vector2(1, 0.5f);
            statusRt.offsetMin = new Vector2(140, 10);
            statusRt.offsetMax = new Vector2(-24, 0);

            GameObject newGo = CreateText(card.transform, "NewIndicator", "NEW", 22, LegacyUITheme.BrassBorder, TextAnchor.UpperRight);
            var newRt = (RectTransform)newGo.transform;
            newRt.anchorMin = newRt.anchorMax = new Vector2(1, 1);
            newRt.pivot = new Vector2(1, 1);
            newRt.anchoredPosition = new Vector2(-16, -12);
            newRt.sizeDelta = new Vector2(80, 32);
            ((Text)newGo.GetComponent<Text>()).fontStyle = FontStyle.BoldAndItalic;
            newGo.SetActive(false);

            var view = card.AddComponent<BuildingCardView>();
            var so = new SerializedObject(view);
            so.FindProperty("_titleText").objectReferenceValue = titleGo.GetComponent<Text>();
            so.FindProperty("_statusText").objectReferenceValue = statusGo.GetComponent<Text>();
            so.FindProperty("_signIcon").objectReferenceValue = signImg;
            so.FindProperty("_newIndicator").objectReferenceValue = newGo;
            so.FindProperty("_button").objectReferenceValue = btn;
            so.ApplyModifiedPropertiesWithoutUndo();

            return view;
        }

        private static GameObject BuildPopupTemplate(Transform parent)
        {
            GameObject template = CreateUIObject("PopupTemplate", parent);
            var rt = (RectTransform)template.transform;
            rt.sizeDelta = new Vector2(700, 420);
            rt.anchoredPosition = Vector2.zero;

            var img = template.AddComponent<Image>();
            img.color = LegacyUITheme.CardviewDarkBackground;
            var outline = template.AddComponent<Outline>();
            outline.effectColor = LegacyUITheme.BrassBorder;
            outline.effectDistance = new Vector2(3, -3);

            GameObject titleGo = CreateText(template.transform, "Title", "Feature", 44, LegacyUITheme.DimWhite, TextAnchor.MiddleCenter);
            var titleRt = (RectTransform)titleGo.transform;
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.sizeDelta = new Vector2(-40, 100);
            titleRt.anchoredPosition = new Vector2(0, -30);

            GameObject bodyGo = CreateText(template.transform, "Body", "(placeholder — Phase 5+ will build real content here)", 26, LegacyUITheme.GreyBorder, TextAnchor.MiddleCenter);
            var bodyRt = (RectTransform)bodyGo.transform;
            bodyRt.anchorMin = new Vector2(0, 0);
            bodyRt.anchorMax = new Vector2(1, 1);
            bodyRt.offsetMin = new Vector2(40, 100);
            bodyRt.offsetMax = new Vector2(-40, -140);

            GameObject closeGo = CreateUIObject("CloseButton", template.transform);
            var closeRt = (RectTransform)closeGo.transform;
            closeRt.anchorMin = new Vector2(0.5f, 0);
            closeRt.anchorMax = new Vector2(0.5f, 0);
            closeRt.pivot = new Vector2(0.5f, 0);
            closeRt.sizeDelta = new Vector2(240, 80);
            closeRt.anchoredPosition = new Vector2(0, 30);
            var closeImg = closeGo.AddComponent<Image>();
            closeImg.color = LegacyUITheme.StandardBackground;
            var closeOutline = closeGo.AddComponent<Outline>();
            closeOutline.effectColor = LegacyUITheme.BrassBorder;
            var closeBtn = closeGo.AddComponent<Button>();
            closeBtn.targetGraphic = closeImg;
            GameObject closeLabel = CreateText(closeGo.transform, "Label", "Close", 32, LegacyUITheme.DimWhite, TextAnchor.MiddleCenter);
            SetStretch((RectTransform)closeLabel.transform);

            var panel = template.AddComponent<SimplePopupPanel>();
            var panelSo = new SerializedObject(panel);
            panelSo.FindProperty("_titleText").objectReferenceValue = titleGo.GetComponent<Text>();
            panelSo.FindProperty("_closeButton").objectReferenceValue = closeBtn;
            panelSo.ApplyModifiedPropertiesWithoutUndo();

            template.SetActive(false);
            return template;
        }

        // ─── Shared helpers (mirrors AppShellBuilder's) ─────────────────────

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreateStretch(string name, Transform parent)
        {
            GameObject go = CreateUIObject(name, parent);
            SetStretch((RectTransform)go.transform);
            return go;
        }

        private static void SetStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize, Color color, TextAnchor alignment)
        {
            GameObject go = CreateUIObject(name, parent);
            var label = go.AddComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return go;
        }
    }
}
#endif
