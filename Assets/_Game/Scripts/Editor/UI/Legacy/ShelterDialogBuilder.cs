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
    /// <summary>Builds the Phase 5E Shelter dialog (pet grid + pet detail overlay) and wires its
    /// prefab into Main.unity. Idempotent — safe to re-run.</summary>
    public static class ShelterDialogBuilder
    {
        private const string OutputPath = "Assets/_Game/Prefabs/UI/Headquarters/ShelterDialog.prefab";
        private const string FramePath = "Assets/_Game/Prefabs/UI/Legacy/LegacyDialogFrame.prefab";
        private const string ButtonPath = "Assets/_Game/Prefabs/UI/Legacy/LegacyButtonFrame.prefab";
        private const string BorderSpritePath = "Assets/_Game/Art/UI/Generated/object_border_dim_white.png";

        [MenuItem("Tools/Guild Master/Legacy UI/Build Shelter Dialog")]
        public static void BuildShelterDialog()
        {
            var frame = AssetDatabase.LoadAssetAtPath<GameObject>(FramePath);
            if (frame == null)
            {
                Debug.LogError("[ShelterDialogBuilder] LegacyDialogFrame.prefab is missing.");
                return;
            }

            GameObject dialog = (GameObject)PrefabUtility.InstantiatePrefab(frame);
            dialog.name = "ShelterDialog";
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

            Text title = AddText(dialog.transform, "Title", "Shelter", 42, LegacyUITheme.DimWhite, FontStyle.Bold, TextAnchor.MiddleCenter, 58f);
            Text count = AddText(dialog.transform, "CountText", "0 pet(s)", 26, LegacyUITheme.BrassBorder, FontStyle.Bold, TextAnchor.MiddleCenter, 44f);
            Text empty = AddText(dialog.transform, "EmptyState", "No pets in the Shelter yet.", 26, LegacyUITheme.DimWhite, FontStyle.Normal, TextAnchor.MiddleCenter, 90f);
            empty.gameObject.SetActive(false);

            var scroll = new GameObject("GridScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(LayoutElement));
            scroll.transform.SetParent(dialog.transform, false);
            scroll.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.12f);
            var scrollLayout = scroll.GetComponent<LayoutElement>();
            scrollLayout.preferredHeight = 1200f;
            scrollLayout.minHeight = 800f;
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
            grid.cellSize = new Vector2(156f, 176f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
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

            // ── Pet Detail overlay (child of dialog, ignores VerticalLayoutGroup, never a 2nd popup) ──
            var overlay = new GameObject("PetDetailOverlay", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
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
            boxRect.sizeDelta = new Vector2(760f, 760f);
            var boxImage = box.GetComponent<Image>();
            boxImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BorderSpritePath);
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
            var petIcon = iconGo.GetComponent<Image>();
            petIcon.preserveAspect = true;
            var iconLayout = iconGo.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = 160f;
            iconLayout.preferredHeight = 160f;

            Text petName = AddText(box.transform, "PetName", "Pet Name", 34, LegacyUITheme.DimWhite, FontStyle.Bold, TextAnchor.MiddleCenter, 54f);
            Text petInfo = AddText(box.transform, "PetInfo", "", 24, LegacyUITheme.DimWhite, FontStyle.Normal, TextAnchor.UpperLeft, 300f);
            GameObject petClose = CreateButton(box.transform, "PetDetailCloseButton", "Close", 260f, 64f);

            var detailPanel = overlay.AddComponent<PetDetailPanel>();
            var detailSerialized = new SerializedObject(detailPanel);
            detailSerialized.FindProperty("_icon").objectReferenceValue = petIcon;
            detailSerialized.FindProperty("_nameText").objectReferenceValue = petName;
            detailSerialized.FindProperty("_infoText").objectReferenceValue = petInfo;
            detailSerialized.FindProperty("_closeButton").objectReferenceValue = petClose.GetComponent<Button>();
            detailSerialized.ApplyModifiedPropertiesWithoutUndo();

            var component = dialog.AddComponent<ShelterDialog>();
            var serialized = new SerializedObject(component);
            serialized.FindProperty("_titleText").objectReferenceValue = title;
            serialized.FindProperty("_countText").objectReferenceValue = count;
            serialized.FindProperty("_closeButton").objectReferenceValue = close.GetComponent<Button>();
            serialized.FindProperty("_gridContent").objectReferenceValue = contentRect;
            serialized.FindProperty("_emptyState").objectReferenceValue = empty.gameObject;
            serialized.FindProperty("_slotBorderSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(BorderSpritePath);
            serialized.FindProperty("_detailPanel").objectReferenceValue = detailPanel;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            PrefabUtility.SaveAsPrefabAssetAndConnect(dialog, OutputPath, InteractionMode.AutomatedAction);
            Object.DestroyImmediate(dialog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            WireMainScene();
            Debug.Log("[ShelterDialogBuilder] ShelterDialog prefab built and Main scene wiring applied.");
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
                    var property = so.FindProperty("_shelterDialogPrefab");
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
    }
}
