#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Legacy;
using GuildMaster.Runtime.UI.Shell;

namespace GuildMaster.Editor.UI.Legacy
{
    /// <summary>
    /// Phase 3: procedurally builds the App Shell (Top HUD + Currency Bar, 4 main tabs, Navigation
    /// Drawer, PopupRoot) directly into the currently open scene (intended to be Main.unity),
    /// using only Phase 1 (LegacySpriteRegistry) and Phase 2 (LegacyUITheme, generated 9-slice
    /// prefabs) assets. Builds NO new visual style of its own.
    ///
    /// Idempotent: if an "AppShellCanvas" GameObject already exists, it is destroyed and rebuilt
    /// from scratch rather than duplicated, so re-running this tool is always safe.
    /// </summary>
    public static class AppShellBuilder
    {
        private const string CanvasName = "AppShellCanvas";
        private const string PrefabRoot = "Assets/_Game/Prefabs/UI/Legacy";
        private static readonly Vector2 ReferenceResolution = new Vector2(1080, 1920);

        [MenuItem("Tools/Guild Master/Legacy UI/Build App Shell")]
        public static void BuildAppShell()
        {
            // Idempotent: remove any previous run before rebuilding.
            GameObject existing = GameObject.Find(CanvasName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            GameObject canvasGo = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // above the legacy 8-button HUD's canvas

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRt = canvasGo.GetComponent<RectTransform>();

            // ─── Tab content (built first, sits behind HUD/Nav) ─────────────
            GameObject tabRoot = CreateStretch("TabContentRoot", canvasRt);
            SetOffsets((RectTransform)tabRoot.transform, 0, 150, 0, 190); // leave room for BottomNav (bottom) + TopHUD (top)

            string[] tabNames = { "Headquarters", "Adventurers", "Dungeons", "Raids" };
            var tabPanels = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject panel = CreateStretch($"Tab_{tabNames[i]}", (RectTransform)tabRoot.transform);
                var img = panel.AddComponent<Image>();
                img.color = LegacyUITheme.CardviewDarkBackground;

                GameObject labelGo = CreateText(panel.transform, "Label", "", 48, LegacyUITheme.DimWhite, TextAnchor.MiddleCenter);
                SetStretch((RectTransform)labelGo.transform);

                panel.AddComponent<TabPlaceholderView>();
                var so = new SerializedObject(panel.GetComponent<TabPlaceholderView>());
                so.FindProperty("_label").objectReferenceValue = labelGo.GetComponent<Text>();
                so.ApplyModifiedPropertiesWithoutUndo();

                tabPanels[i] = panel;
            }

            // ─── Top HUD ──────────────────────────────────────────────────
            GameObject topHud = CreateUIObject("TopHUD", canvasRt);
            var topRt = (RectTransform)topHud.transform;
            topRt.anchorMin = new Vector2(0, 1);
            topRt.anchorMax = new Vector2(1, 1);
            topRt.pivot = new Vector2(0.5f, 1);
            topRt.anchoredPosition = Vector2.zero;
            topRt.sizeDelta = new Vector2(0, 190);

            var topBg = topHud.AddComponent<Image>();
            topBg.color = LegacyUITheme.CardviewDarkBackground;

            Button menuButton = CreateIconButton(topHud.transform, "MenuButton", null, "☰", new Vector2(70, 70), new Vector2(16, -16), TextAnchor.UpperLeft);

            GameObject titleGo = CreateText(topHud.transform, "ScreenTitle", "Headquarters", 44, LegacyUITheme.DimWhite, TextAnchor.MiddleCenter);
            var titleRt = (RectTransform)titleGo.transform;
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.sizeDelta = new Vector2(-200, 70);
            titleRt.anchoredPosition = new Vector2(0, -16);

            // Gems (top-right)
            GameObject gemsIcon = CreateIcon(topHud.transform, "GemsIcon", "gem", new Vector2(48, 48));
            AnchorTopRight((RectTransform)gemsIcon.transform, new Vector2(-190, -16));
            GameObject gemsText = CreateText(topHud.transform, "GemsText", "0", 32, LegacyUITheme.DimWhite, TextAnchor.MiddleLeft);
            AnchorTopRight((RectTransform)gemsText.transform, new Vector2(-130, -22));
            ((RectTransform)gemsText.transform).sizeDelta = new Vector2(120, 50);

            // Currency bar (row under title)
            string[] coinNames = { "coin_platinum", "coin_gold", "coin_silver", "coin_copper" };
            var currencyTexts = new GameObject[4];
            float coinX = 16;
            for (int i = 0; i < 4; i++)
            {
                GameObject icon = CreateIcon(topHud.transform, $"{coinNames[i]}_Icon", coinNames[i], new Vector2(36, 36));
                var iconRt = (RectTransform)icon.transform;
                iconRt.anchorMin = iconRt.anchorMax = new Vector2(0, 1);
                iconRt.pivot = new Vector2(0, 1);
                iconRt.anchoredPosition = new Vector2(coinX, -100);
                coinX += 42;

                GameObject text = CreateText(topHud.transform, $"{coinNames[i]}_Text", "0", 28, LegacyUITheme.DimWhite, TextAnchor.MiddleLeft);
                var textRt = (RectTransform)text.transform;
                textRt.anchorMin = textRt.anchorMax = new Vector2(0, 1);
                textRt.pivot = new Vector2(0, 1);
                textRt.sizeDelta = new Vector2(90, 40);
                textRt.anchoredPosition = new Vector2(coinX, -104);
                coinX += 96;

                currencyTexts[i] = text;
            }

            // Tooltip icons row (Shop, Messages, Merchant, Quests) — text-only fallback: no
            // dedicated PNG sprite exists for these in the Phase 1 import (they were XML vectors
            // or not part of the decompiled drawable set), see unity_legacy_shape_mapping.md.
            string[] tooltipLabels = { "Shop", "Msg", "Merchant", "Quests" };
            float tipX = -16;
            for (int i = tooltipLabels.Length - 1; i >= 0; i--)
            {
                GameObject tip = CreateText(topHud.transform, $"Tooltip_{tooltipLabels[i]}", tooltipLabels[i], 22, LegacyUITheme.BrassBorder, TextAnchor.MiddleRight);
                var tipRt = (RectTransform)tip.transform;
                tipRt.anchorMin = tipRt.anchorMax = new Vector2(1, 1);
                tipRt.pivot = new Vector2(1, 1);
                tipRt.sizeDelta = new Vector2(130, 36);
                tipRt.anchoredPosition = new Vector2(tipX, -150);
                tipX -= 136;
            }

            // ─── Bottom Nav ───────────────────────────────────────────────
            GameObject bottomNav = CreateUIObject("BottomNav", canvasRt);
            var bottomRt = (RectTransform)bottomNav.transform;
            bottomRt.anchorMin = new Vector2(0, 0);
            bottomRt.anchorMax = new Vector2(1, 0);
            bottomRt.pivot = new Vector2(0.5f, 0);
            bottomRt.anchoredPosition = Vector2.zero;
            bottomRt.sizeDelta = new Vector2(0, 150);

            var bottomBg = bottomNav.AddComponent<Image>();
            bottomBg.color = LegacyUITheme.CardviewDarkBackground;

            string[] navIconNames = { null, "bottom_nav_adventurers", "bottom_nav_dungeons", "bottom_nav_raids" };
            var tabButtons = new Button[4];
            var tabIcons = new Image[4];
            float navCellW = 1080f / 4f;
            for (int i = 0; i < 4; i++)
            {
                GameObject cell = CreateUIObject($"NavCell_{tabNames[i]}", (RectTransform)bottomNav.transform);
                var cellRt = (RectTransform)cell.transform;
                cellRt.anchorMin = new Vector2(0, 0);
                cellRt.anchorMax = new Vector2(0, 1);
                cellRt.pivot = new Vector2(0, 0.5f);
                cellRt.sizeDelta = new Vector2(navCellW, 0);
                cellRt.anchoredPosition = new Vector2(navCellW * i, 0);

                var btn = cell.AddComponent<Button>();
                var btnImg = cell.AddComponent<Image>();
                btnImg.color = new Color(0, 0, 0, 0.001f); // invisible full-cell click target
                btn.targetGraphic = btnImg;

                GameObject iconGo;
                if (navIconNames[i] != null)
                {
                    iconGo = CreateIcon(cell.transform, "Icon", navIconNames[i], new Vector2(56, 56));
                }
                else
                {
                    // Headquarters nav icon is an unconverted XML vector (bottom_nav_castle) —
                    // no bitmap sprite exists yet (see unity_legacy_shape_mapping.md). Text-only
                    // fallback, not invented art.
                    iconGo = CreateUIObject("Icon", cell.transform);
                    ((RectTransform)iconGo.transform).sizeDelta = new Vector2(56, 56);
                    var placeholderImg = iconGo.AddComponent<Image>();
                    placeholderImg.color = LegacyUITheme.DimWhite;
                }
                var iconRt2 = (RectTransform)iconGo.transform;
                iconRt2.anchorMin = iconRt2.anchorMax = new Vector2(0.5f, 0.5f);
                iconRt2.anchoredPosition = new Vector2(0, 20);

                GameObject label = CreateText(cell.transform, "Label", tabNames[i], 22, LegacyUITheme.DimWhite, TextAnchor.MiddleCenter);
                var labelRt = (RectTransform)label.transform;
                labelRt.anchorMin = new Vector2(0, 0);
                labelRt.anchorMax = new Vector2(1, 0);
                labelRt.pivot = new Vector2(0.5f, 0);
                labelRt.sizeDelta = new Vector2(-8, 30);
                labelRt.anchoredPosition = new Vector2(0, 10);

                tabButtons[i] = btn;
                tabIcons[i] = iconGo.GetComponent<Image>();
            }

            // ─── Popup Root (above everything else in this canvas) ─────────
            GameObject popupRoot = CreateStretch("PopupRoot", canvasRt);
            GameObject popupBackdrop = CreateStretch("PopupBackdrop", (RectTransform)popupRoot.transform);
            var backdropImg = popupBackdrop.AddComponent<Image>();
            backdropImg.color = new Color(0, 0, 0, 0.6f);
            popupBackdrop.SetActive(false);

            // ─── Drawer (Left, above tab content + HUD, below PopupRoot) ───
            GameObject drawerRoot = CreateStretch("DrawerRoot", canvasRt);
            drawerRoot.transform.SetSiblingIndex(popupRoot.transform.GetSiblingIndex()); // just under PopupRoot

            GameObject drawerBackdropGo = CreateStretch("DrawerBackdrop", (RectTransform)drawerRoot.transform);
            var drawerBackdropImg = drawerBackdropGo.AddComponent<Image>();
            drawerBackdropImg.color = new Color(0, 0, 0, 0.5f);
            var drawerBackdropBtn = drawerBackdropGo.AddComponent<Button>();
            drawerBackdropBtn.targetGraphic = drawerBackdropImg;

            GameObject drawerPanel = CreateUIObject("DrawerPanel", (RectTransform)drawerRoot.transform);
            var drawerPanelRt = (RectTransform)drawerPanel.transform;
            drawerPanelRt.anchorMin = new Vector2(0, 0);
            drawerPanelRt.anchorMax = new Vector2(0, 1);
            drawerPanelRt.pivot = new Vector2(0, 0.5f);
            drawerPanelRt.anchoredPosition = Vector2.zero;
            drawerPanelRt.sizeDelta = new Vector2(800, 0); // ~74% of 1080

            var drawerPanelImg = drawerPanel.AddComponent<Image>();
            drawerPanelImg.color = LegacyUITheme.CardviewDarkBackground;

            string[] drawerItems =
            {
                "Shop", "Settings", "Recall Adventurers", "Messages", "FAQ",
                "Bestiary", "Achievements", "Cloud Save", "Redeem Code", "Community"
            };
            var drawerButtons = new Button[drawerItems.Length];
            float itemY = -20;
            const float itemH = 96;
            for (int i = 0; i < drawerItems.Length; i++)
            {
                GameObject item = CreateUIObject($"DrawerItem_{drawerItems[i].Replace(" ", "")}", drawerPanelRt);
                var itemRt = (RectTransform)item.transform;
                itemRt.anchorMin = new Vector2(0, 1);
                itemRt.anchorMax = new Vector2(1, 1);
                itemRt.pivot = new Vector2(0.5f, 1);
                itemRt.sizeDelta = new Vector2(0, itemH);
                itemRt.anchoredPosition = new Vector2(0, itemY);

                var itemImg = item.AddComponent<Image>();
                itemImg.color = new Color(1, 1, 1, 0.02f);
                var itemBtn = item.AddComponent<Button>();
                itemBtn.targetGraphic = itemImg;

                GameObject itemLabel = CreateText(item.transform, "Label", drawerItems[i], 34, LegacyUITheme.DimWhite, TextAnchor.MiddleLeft);
                var itemLabelRt = (RectTransform)itemLabel.transform;
                SetStretch(itemLabelRt);
                itemLabelRt.offsetMin = new Vector2(32, 0);

                drawerButtons[i] = itemBtn;
                itemY -= itemH;
            }

            drawerRoot.SetActive(false);

            // ─── Wire AppShellController ────────────────────────────────
            var controllerGo = new GameObject("AppShellController");
            controllerGo.transform.SetParent(canvasRt, false);
            var controller = controllerGo.AddComponent<AppShellController>();

            var controllerSo = new SerializedObject(controller);
            SetObjRef(controllerSo, "_screenTitleText", titleGo.GetComponent<Text>());
            SetObjRef(controllerSo, "_gemsText", gemsText.GetComponent<Text>());
            SetObjRef(controllerSo, "_platinumText", currencyTexts[0].GetComponent<Text>());
            SetObjRef(controllerSo, "_goldText", currencyTexts[1].GetComponent<Text>());
            SetObjRef(controllerSo, "_silverText", currencyTexts[2].GetComponent<Text>());
            SetObjRef(controllerSo, "_copperText", currencyTexts[3].GetComponent<Text>());
            SetObjRef(controllerSo, "_menuButton", menuButton);
            SetObjArray(controllerSo, "_tabButtons", tabButtons);
            SetObjArray(controllerSo, "_tabButtonIcons", tabIcons);
            SetObjArray(controllerSo, "_tabPanels", tabPanels);
            SetObjRef(controllerSo, "_drawerRoot", drawerRoot);
            SetObjRef(controllerSo, "_drawerBackdropButton", drawerBackdropBtn);
            SetObjArray(controllerSo, "_drawerItemButtons", drawerButtons);
            SetObjRef(controllerSo, "_popupRoot", popupRoot.GetComponent<RectTransform>());
            SetObjRef(controllerSo, "_popupBackdrop", popupBackdrop);
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(canvasGo);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("[AppShellBuilder] App Shell built: TopHUD, Currency Bar, 4 tabs, Drawer (10 items), PopupRoot. " +
                      $"Canvas sortingOrder={canvas.sortingOrder} (renders above the legacy HUD canvas).");
        }

        // ─── Small helpers ──────────────────────────────────────────────────

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

        private static void SetOffsets(RectTransform rt, float left, float bottom, float right, float top)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
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
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return go;
        }

        private static GameObject CreateIcon(Transform parent, string name, string legacySpriteName, Vector2 size)
        {
            GameObject go = CreateUIObject(name, parent);
            var img = go.AddComponent<Image>();
            Sprite sprite = LegacySpriteRegistry.GetSprite(legacySpriteName);
            img.sprite = sprite;
            img.preserveAspect = true;
            if (sprite == null)
            {
                img.color = LegacyUITheme.GreyBorder; // visible gap marker if the asset is missing
            }
            ((RectTransform)go.transform).sizeDelta = size;
            return go;
        }

        private static Button CreateIconButton(Transform parent, string name, string legacySpriteName, string textFallback, Vector2 size, Vector2 topLeftOffset, TextAnchor anchor)
        {
            GameObject go = CreateUIObject(name, parent);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = size;
            rt.anchoredPosition = topLeftOffset;

            var img = go.AddComponent<Image>();
            Sprite sprite = legacySpriteName != null ? LegacySpriteRegistry.GetSprite(legacySpriteName) : null;
            if (sprite != null)
            {
                img.sprite = sprite;
                img.preserveAspect = true;
            }
            else
            {
                img.color = new Color(0, 0, 0, 0.001f);
                GameObject label = CreateText(go.transform, "Label", textFallback, 48, LegacyUITheme.DimWhite, TextAnchor.MiddleCenter);
                SetStretch((RectTransform)label.transform);
            }

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            return btn;
        }

        private static void AnchorTopRight(RectTransform rt, Vector2 offsetFromTopRight)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = offsetFromTopRight;
        }

        private static void SetObjRef(SerializedObject so, string fieldName, Object value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[AppShellBuilder] Field '{fieldName}' not found on target.");
                return;
            }
            prop.objectReferenceValue = value;
        }

        private static void SetObjArray(SerializedObject so, string fieldName, Object[] values)
        {
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[AppShellBuilder] Array field '{fieldName}' not found on target.");
                return;
            }
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }
    }
}
#endif
