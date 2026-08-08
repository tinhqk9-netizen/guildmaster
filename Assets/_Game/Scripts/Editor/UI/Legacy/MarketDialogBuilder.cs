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
    /// <summary>Builds the Phase 5F Market dialog (Selling/Sold/Buy sections) and wires its prefab
    /// into Main.unity. Idempotent — safe to re-run.</summary>
    public static class MarketDialogBuilder
    {
        private const string OutputPath = "Assets/_Game/Prefabs/UI/Headquarters/MarketDialog.prefab";
        private const string FramePath = "Assets/_Game/Prefabs/UI/Legacy/LegacyDialogFrame.prefab";
        private const string ButtonPath = "Assets/_Game/Prefabs/UI/Legacy/LegacyButtonFrame.prefab";
        private const string RowBorderSpritePath = "Assets/_Game/Art/UI/Generated/object_border_dim_white.png";

        [MenuItem("Tools/Guild Master/Legacy UI/Build Market Dialog")]
        public static void BuildMarketDialog()
        {
            var frame = AssetDatabase.LoadAssetAtPath<GameObject>(FramePath);
            if (frame == null)
            {
                Debug.LogError("[MarketDialogBuilder] LegacyDialogFrame.prefab is missing.");
                return;
            }

            GameObject dialog = (GameObject)PrefabUtility.InstantiatePrefab(frame);
            dialog.name = "MarketDialog";
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

            Text title = AddText(dialog.transform, "Title", "Market", 42, LegacyUITheme.DimWhite, FontStyle.Bold, TextAnchor.MiddleCenter, 58f);
            BuildUpgradeRow(dialog.transform, "ListingUpgrade", "Listing capacity", out var listingUpgradeInfo, out var listingUpgradeButton);
            BuildUpgradeRow(dialog.transform, "SpeedUpgrade", "Market speed", out var speedUpgradeInfo, out var speedUpgradeButton);
            Text empty = AddText(dialog.transform, "EmptyState", "The market is quiet right now.", 26, LegacyUITheme.DimWhite, FontStyle.Normal, TextAnchor.MiddleCenter, 90f);
            empty.gameObject.SetActive(false);

            var scroll = new GameObject("ListScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(LayoutElement));
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

            var content = new GameObject("ListContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
            var contentGroup = content.GetComponent<VerticalLayoutGroup>();
            contentGroup.padding = new RectOffset(10, 10, 10, 10);
            contentGroup.spacing = 10f;
            contentGroup.childAlignment = TextAnchor.UpperCenter;
            contentGroup.childControlWidth = true;
            contentGroup.childControlHeight = true;
            contentGroup.childForceExpandWidth = true;
            contentGroup.childForceExpandHeight = false;
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

            var component = dialog.AddComponent<MarketDialog>();
            var serialized = new SerializedObject(component);
            serialized.FindProperty("_titleText").objectReferenceValue = title;
            serialized.FindProperty("_listingUpgradeInfoText").objectReferenceValue = listingUpgradeInfo;
            serialized.FindProperty("_listingUpgradeButton").objectReferenceValue = listingUpgradeButton;
            serialized.FindProperty("_speedUpgradeInfoText").objectReferenceValue = speedUpgradeInfo;
            serialized.FindProperty("_speedUpgradeButton").objectReferenceValue = speedUpgradeButton;
            serialized.FindProperty("_closeButton").objectReferenceValue = close.GetComponent<Button>();
            serialized.FindProperty("_listContent").objectReferenceValue = contentRect;
            serialized.FindProperty("_emptyState").objectReferenceValue = empty.gameObject;
            serialized.FindProperty("_rowBorderSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(RowBorderSpritePath);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            PrefabUtility.SaveAsPrefabAssetAndConnect(dialog, OutputPath, InteractionMode.AutomatedAction);
            Object.DestroyImmediate(dialog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            WireMainScene();
            Debug.Log("[MarketDialogBuilder] MarketDialog prefab built and Main scene wiring applied.");
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
                    var property = so.FindProperty("_marketDialogPrefab");
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
            row.GetComponent<LayoutElement>().preferredHeight = 74f;
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
