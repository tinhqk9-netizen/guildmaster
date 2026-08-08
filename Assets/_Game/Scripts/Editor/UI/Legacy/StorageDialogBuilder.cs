using System.IO;
using GuildMaster.Runtime.UI.Headquarters;
using GuildMaster.Runtime.UI.Legacy;
using GuildMaster.Runtime.UI.Shell;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GuildMaster.Editor.UI.Headquarters
{
    /// <summary>
    /// Builds the Phase 5C Storage dialog (simplified per change request — no filter/sort UI) and
    /// wires its prefab into Main.unity.
    /// </summary>
    public static class StorageDialogBuilder
    {
        private const string OutputPath = "Assets/_Game/Prefabs/UI/Headquarters/StorageDialog.prefab";
        private const string FramePath = "Assets/_Game/Prefabs/UI/Legacy/LegacyDialogFrame.prefab";
        private const string ButtonPath = "Assets/_Game/Prefabs/UI/Legacy/LegacyButtonFrame.prefab";
        private const string SlotBorderSpritePath = "Assets/_Game/Art/UI/Generated/object_border_dim_white.png";

        [MenuItem("Tools/Guild Master/Legacy UI/Build Storage Dialog")]
        public static void BuildStorageDialog()
        {
            var frame = AssetDatabase.LoadAssetAtPath<GameObject>(FramePath);
            if (frame == null)
            {
                Debug.LogError("[StorageDialogBuilder] LegacyDialogFrame.prefab is missing.");
                return;
            }

            GameObject dialog = (GameObject)PrefabUtility.InstantiatePrefab(frame);
            dialog.name = "StorageDialog";
            var root = dialog.GetComponent<RectTransform>();
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = new Vector2(960f, 1620f);

            var rootFitter = dialog.GetComponent<ContentSizeFitter>();
            if (rootFitter != null)
            {
                rootFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                rootFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }

            var rootLayout = dialog.GetComponent<VerticalLayoutGroup>() ?? dialog.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(36, 36, 30, 30);
            rootLayout.spacing = 14f;
            rootLayout.childAlignment = TextAnchor.UpperCenter;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            Text title = AddText(dialog.transform, "Title", "Storage", 42, LegacyUITheme.DimWhite, FontStyle.Bold, TextAnchor.MiddleCenter, 58f);
            Text capacity = AddText(dialog.transform, "CapacityText", "0 / 0", 26, LegacyUITheme.BrassBorder, FontStyle.Bold, TextAnchor.MiddleCenter, 44f);
            BuildUpgradeRow(dialog.transform, "StorageUpgrade", "Capacity upgrade", out var upgradeInfo, out var upgradeButton);

            // ── Grid scroll area (fills the bulk of the dialog — no filter/sort rows above it) ──
            var scroll = new GameObject("ItemScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(LayoutElement));
            scroll.transform.SetParent(dialog.transform, false);
            scroll.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.12f);
            var scrollLayout = scroll.GetComponent<LayoutElement>();
            scrollLayout.preferredHeight = 1300f;
            scrollLayout.minHeight = 900f;
            scrollLayout.flexibleHeight = 1f;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewport.transform.SetParent(scroll.transform, false);
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

            var content = new GameObject("GridContent", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
            var grid = content.GetComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(12, 12, 12, 12);
            grid.spacing = new Vector2(14f, 14f);
            grid.cellSize = new Vector2(156f, 156f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5; // 5-column grid per spec
            grid.childAlignment = TextAnchor.UpperCenter;
            var contentFitter = content.GetComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = scroll.GetComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 36f;

            GameObject close = CreateButton(dialog.transform, "CloseButton", "Close", 280f, 66f);

            // ── Item Detail overlay (real detail — child of dialog, ignores VerticalLayoutGroup) ──
            var overlay = new GameObject("ItemDetailOverlay", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            overlay.transform.SetParent(dialog.transform, false);
            var overlayRect = (RectTransform)overlay.transform;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
            overlay.GetComponent<LayoutElement>().ignoreLayout = true;

            var box = new GameObject("Box", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            box.transform.SetParent(overlay.transform, false);
            var boxRect = (RectTransform)box.transform;
            boxRect.anchorMin = boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.anchoredPosition = Vector2.zero;
            boxRect.sizeDelta = new Vector2(760f, 980f);
            var boxImage = box.GetComponent<Image>();
            boxImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SlotBorderSpritePath);
            boxImage.type = Image.Type.Sliced;
            boxImage.color = LegacyUITheme.CardviewDarkBackground;
            var boxLayout = box.GetComponent<VerticalLayoutGroup>();
            boxLayout.padding = new RectOffset(34, 34, 30, 30);
            boxLayout.spacing = 14f;
            boxLayout.childAlignment = TextAnchor.UpperCenter;
            boxLayout.childControlWidth = true;
            boxLayout.childControlHeight = true;
            boxLayout.childForceExpandWidth = true;
            boxLayout.childForceExpandHeight = false;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconGo.transform.SetParent(box.transform, false);
            var itemIcon = iconGo.GetComponent<Image>();
            itemIcon.preserveAspect = true;
            var iconLayout = iconGo.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = 150f;
            iconLayout.preferredHeight = 150f;

            Text itemName = AddText(box.transform, "ItemName", "Item Name", 34, LegacyUITheme.DimWhite, FontStyle.Bold, TextAnchor.MiddleCenter, 58f);
            Text itemInfo = AddText(box.transform, "ItemInfo", "", 24, LegacyUITheme.DimWhite, FontStyle.Normal, TextAnchor.UpperLeft, 260f);
            Text itemHint = AddText(box.transform, "ItemHint", "", 20, LegacyUITheme.BrassBorder, FontStyle.Italic, TextAnchor.MiddleCenter, 40f);

            // ── Action row (Unequip / Use / Sell — each hidden/disabled per-item by the panel) ──
            var actionRow = new GameObject("ActionRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            actionRow.transform.SetParent(box.transform, false);
            var actionRowLayout = actionRow.GetComponent<LayoutElement>();
            actionRowLayout.preferredHeight = 70f;
            var actionRowGroup = actionRow.GetComponent<HorizontalLayoutGroup>();
            actionRowGroup.spacing = 14f;
            actionRowGroup.childAlignment = TextAnchor.MiddleCenter;
            actionRowGroup.childControlWidth = true;
            actionRowGroup.childControlHeight = true;
            actionRowGroup.childForceExpandWidth = true;
            actionRowGroup.childForceExpandHeight = true;

            GameObject unequipGo = CreateButton(actionRow.transform, "UnequipButton", "Unequip", 200f, 66f);
            GameObject useGo = CreateButton(actionRow.transform, "UseButton", "Use", 200f, 66f);
            GameObject sellGo = CreateButton(actionRow.transform, "SellButton", "Sell", 200f, 66f);

            GameObject itemClose = CreateButton(box.transform, "ItemDetailCloseButton", "Close", 260f, 64f);

            var detailPanel = overlay.AddComponent<StorageItemDetailPanel>();
            var detailSerialized = new SerializedObject(detailPanel);
            detailSerialized.FindProperty("_icon").objectReferenceValue = itemIcon;
            detailSerialized.FindProperty("_nameText").objectReferenceValue = itemName;
            detailSerialized.FindProperty("_infoText").objectReferenceValue = itemInfo;
            detailSerialized.FindProperty("_hintText").objectReferenceValue = itemHint;
            detailSerialized.FindProperty("_closeButton").objectReferenceValue = itemClose.GetComponent<Button>();
            detailSerialized.FindProperty("_unequipButton").objectReferenceValue = unequipGo.GetComponent<Button>();
            detailSerialized.FindProperty("_unequipButtonLabel").objectReferenceValue = unequipGo.GetComponentInChildren<Text>();
            detailSerialized.FindProperty("_useButton").objectReferenceValue = useGo.GetComponent<Button>();
            detailSerialized.FindProperty("_sellButton").objectReferenceValue = sellGo.GetComponent<Button>();
            detailSerialized.ApplyModifiedPropertiesWithoutUndo();

            var component = dialog.AddComponent<StorageDialog>();
            var serialized = new SerializedObject(component);
            serialized.FindProperty("_titleText").objectReferenceValue = title;
            serialized.FindProperty("_capacityText").objectReferenceValue = capacity;
            serialized.FindProperty("_upgradeInfoText").objectReferenceValue = upgradeInfo;
            serialized.FindProperty("_upgradeButton").objectReferenceValue = upgradeButton;
            serialized.FindProperty("_closeButton").objectReferenceValue = close.GetComponent<Button>();

            serialized.FindProperty("_gridContent").objectReferenceValue = contentRect;
            serialized.FindProperty("_itemSlotBorderSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(SlotBorderSpritePath);

            serialized.FindProperty("_itemDetailPanel").objectReferenceValue = detailPanel;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            PrefabUtility.SaveAsPrefabAssetAndConnect(dialog, OutputPath, InteractionMode.AutomatedAction);
            Object.DestroyImmediate(dialog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            WireMainScene();
            Debug.Log("[StorageDialogBuilder] StorageDialog prefab built and Main scene wiring applied.");
        }

        private static void WireMainScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != "Assets/_Game/Scenes/Main.unity")
                scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/Main.unity");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(OutputPath);
            bool changed = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var hub in root.GetComponentsInChildren<HeadquartersHubController>(true))
                {
                    var so = new SerializedObject(hub);
                    var property = so.FindProperty("_storageDialogPrefab");
                    if (property != null && property.objectReferenceValue != prefab)
                    {
                        property.objectReferenceValue = prefab;
                        so.ApplyModifiedPropertiesWithoutUndo();
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        private static Text AddText(Transform parent, string name, string value, int size, Color color, FontStyle style, TextAnchor alignment, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = color;
            text.fontStyle = style;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.text = value;
            go.GetComponent<LayoutElement>().preferredHeight = height;
            return text;
        }

        private static void BuildUpgradeRow(Transform parent, string name, string label, out Text info, out Button button)
        {
            var row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var rowLayout = row.GetComponent<LayoutElement>();
            rowLayout.preferredHeight = 74f;
            var group = row.GetComponent<HorizontalLayoutGroup>();
            group.spacing = 14f;
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = true;

            info = AddText(row.transform, "Info", label, 24, LegacyUITheme.DimWhite, FontStyle.Normal, TextAnchor.MiddleLeft, 74f);
            info.GetComponent<LayoutElement>().flexibleWidth = 1f;
            var buttonGo = CreateButton(row.transform, "UpgradeButton", "Upgrade", 280f, 66f);
            button = buttonGo.GetComponent<Button>();
        }

        private static GameObject CreateButton(Transform parent, string name, string label, float width, float height)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonPath);
            var button = prefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent)
                : new GameObject(name, typeof(RectTransform), typeof(Image));
            button.name = name;
            var image = button.GetComponent<Image>() ?? button.AddComponent<Image>();
            var uiButton = button.GetComponent<Button>() ?? button.AddComponent<Button>();
            uiButton.targetGraphic = image;
            var layout = button.GetComponent<LayoutElement>() ?? button.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(button.transform, false);
            var rect = textGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 30;
            text.fontStyle = FontStyle.Bold;
            text.color = LegacyUITheme.DimWhite;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;
            return button;
        }
    }
}
