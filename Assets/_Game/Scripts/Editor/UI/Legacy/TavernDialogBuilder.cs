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
    /// <summary>Builds the Phase 5B Tavern dialog and wires its prefab into Main.unity.</summary>
    public static class TavernDialogBuilder
    {
        private const string OutputPath = "Assets/_Game/Prefabs/UI/Headquarters/TavernDialog.prefab";
        private const string FramePath = "Assets/_Game/Prefabs/UI/Legacy/LegacyDialogFrame.prefab";
        private const string ButtonPath = "Assets/_Game/Prefabs/UI/Legacy/LegacyButtonFrame.prefab";
        private const string CardSpritePath = "Assets/_Game/Art/UI/Generated/object_border_dim_white.png";
        private const string ButtonSpritePath = "Assets/_Game/Art/UI/Generated/object_border_brass.png";

        [MenuItem("Tools/Guild Master/Legacy UI/Build Tavern Dialog")]
        public static void BuildTavernDialog()
        {
            var frame = AssetDatabase.LoadAssetAtPath<GameObject>(FramePath);
            if (frame == null)
            {
                Debug.LogError("[TavernDialogBuilder] LegacyDialogFrame.prefab is missing.");
                return;
            }

            GameObject dialog = (GameObject)PrefabUtility.InstantiatePrefab(frame);
            dialog.name = "TavernDialog";
            var root = dialog.GetComponent<RectTransform>();
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = new Vector2(900f, 1350f);

            var rootFitter = dialog.GetComponent<ContentSizeFitter>();
            if (rootFitter != null)
            {
                rootFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                rootFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }

            var rootLayout = dialog.GetComponent<VerticalLayoutGroup>() ?? dialog.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(42, 42, 34, 34);
            rootLayout.spacing = 18f;
            rootLayout.childAlignment = TextAnchor.UpperCenter;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            Text title = AddText(dialog.transform, "Title", "Tavern", 44, LegacyUITheme.DimWhite, FontStyle.Bold, TextAnchor.MiddleCenter, 62f);

            var summary = new GameObject("Summary", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            summary.transform.SetParent(dialog.transform, false);
            var summaryLayout = summary.GetComponent<LayoutElement>();
            summaryLayout.preferredHeight = 58f;
            var summaryGroup = summary.GetComponent<HorizontalLayoutGroup>();
            summaryGroup.spacing = 22f;
            summaryGroup.childAlignment = TextAnchor.MiddleCenter;
            summaryGroup.childControlWidth = true;
            summaryGroup.childControlHeight = true;
            summaryGroup.childForceExpandWidth = true;
            summaryGroup.childForceExpandHeight = false;
            Text guestCount = AddText(summary.transform, "GuestCount", "Guests 0/0", 26, LegacyUITheme.BrassBorder, FontStyle.Bold, TextAnchor.MiddleCenter, 52f);
            Text quarters = AddText(summary.transform, "Quarters", "Quarters 0/0", 24, LegacyUITheme.DimWhite, FontStyle.Normal, TextAnchor.MiddleCenter, 52f);

            Text timer = AddText(dialog.transform, "Timer", "Visitor arriving soon", 22, LegacyUITheme.DimWhite, FontStyle.Normal, TextAnchor.MiddleCenter, 44f);

            var upgrades = new GameObject("TavernUpgrades", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            upgrades.transform.SetParent(dialog.transform, false);
            var upgradesLayout = upgrades.GetComponent<LayoutElement>();
            upgradesLayout.preferredHeight = 150f;
            upgradesLayout.minHeight = 150f;
            var upgradesGroup = upgrades.GetComponent<VerticalLayoutGroup>();
            upgradesGroup.spacing = 8f;
            upgradesGroup.childAlignment = TextAnchor.MiddleCenter;
            upgradesGroup.childControlWidth = true;
            upgradesGroup.childControlHeight = true;
            upgradesGroup.childForceExpandWidth = true;
            upgradesGroup.childForceExpandHeight = false;

            Text capacityUpgrade = AddUpgradeRow(upgrades.transform, "CapacityUpgrade", "Guest Capacity\nLevel 1  •  Next cost 0 gold", out GameObject capacityButton);
            Text speedUpgrade = AddUpgradeRow(upgrades.transform, "SpeedUpgrade", "Visitor Speed\nLevel 1  •  Next cost 0 gold", out GameObject speedButton);

            var scroll = new GameObject("GuestScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(LayoutElement));
            scroll.transform.SetParent(dialog.transform, false);
            scroll.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.12f);
            var scrollLayout = scroll.GetComponent<LayoutElement>();
            scrollLayout.preferredHeight = 1000f;
            scrollLayout.minHeight = 720f;
            scrollLayout.flexibleHeight = 1f;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewport.transform.SetParent(scroll.transform, false);
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

            var content = new GameObject("GuestContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
            var contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(18, 18, 18, 18);
            contentLayout.spacing = 16f;
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            var contentFitter = content.GetComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Text empty = AddText(content.transform, "EmptyState", "No guests are waiting at the Tavern.", 28, LegacyUITheme.DimWhite, FontStyle.Normal, TextAnchor.MiddleCenter, 100f);
            empty.gameObject.SetActive(false);

            var scrollRect = scroll.GetComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 36f;

            GameObject close = CreateButton(dialog.transform, "CloseButton", "Close", 280f, 68f);

            var component = dialog.AddComponent<TavernDialog>();
            var serialized = new SerializedObject(component);
            serialized.FindProperty("_guestCountText").objectReferenceValue = guestCount;
            serialized.FindProperty("_quartersText").objectReferenceValue = quarters;
            serialized.FindProperty("_timerText").objectReferenceValue = timer;
            serialized.FindProperty("_capacityUpgradeText").objectReferenceValue = capacityUpgrade;
            serialized.FindProperty("_capacityUpgradeButton").objectReferenceValue = capacityButton.GetComponent<Button>();
            serialized.FindProperty("_speedUpgradeText").objectReferenceValue = speedUpgrade;
            serialized.FindProperty("_speedUpgradeButton").objectReferenceValue = speedButton.GetComponent<Button>();
            serialized.FindProperty("_guestContent").objectReferenceValue = contentRect;
            serialized.FindProperty("_emptyState").objectReferenceValue = empty.gameObject;
            serialized.FindProperty("_closeButton").objectReferenceValue = close.GetComponent<Button>();
            serialized.FindProperty("_cardFrameSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(CardSpritePath);
            serialized.FindProperty("_buttonSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            PrefabUtility.SaveAsPrefabAssetAndConnect(dialog, OutputPath, InteractionMode.AutomatedAction);
            Object.DestroyImmediate(dialog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            WireMainScene();
            Debug.Log("[TavernDialogBuilder] TavernDialog prefab built and Main scene wiring applied.");
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
                    var property = so.FindProperty("_tavernDialogPrefab");
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

        private static Text AddUpgradeRow(Transform parent, string name, string label, out GameObject button)
        {
            var row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var rowLayout = row.GetComponent<LayoutElement>();
            rowLayout.preferredHeight = 68f;
            rowLayout.minHeight = 68f;
            var group = row.GetComponent<HorizontalLayoutGroup>();
            group.spacing = 16f;
            group.padding = new RectOffset(12, 12, 4, 4);
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;

            var text = AddText(row.transform, "Label", label, 20, LegacyUITheme.DimWhite, FontStyle.Normal, TextAnchor.MiddleLeft, 60f);
            text.GetComponent<LayoutElement>().flexibleWidth = 1f;
            button = CreateButton(row.transform, "UpgradeButton", "UPGRADE", 190f, 58f);
            return text;
        }
    }
}
