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
    /// <summary>Builds the Phase 5D Workshop dialog (queue list + recipe overlay) and wires its
    /// prefab into Main.unity.</summary>
    public static class WorkshopDialogBuilder
    {
        private const string OutputPath = "Assets/_Game/Prefabs/UI/Headquarters/WorkshopDialog.prefab";
        private const string FramePath = "Assets/_Game/Prefabs/UI/Legacy/LegacyDialogFrame.prefab";
        private const string ButtonPath = "Assets/_Game/Prefabs/UI/Legacy/LegacyButtonFrame.prefab";
        private const string RowBorderSpritePath = "Assets/_Game/Art/UI/Generated/object_border_dim_white.png";

        [MenuItem("Tools/Guild Master/Legacy UI/Build Workshop Dialog")]
        public static void BuildWorkshopDialog()
        {
            var frame = AssetDatabase.LoadAssetAtPath<GameObject>(FramePath);
            if (frame == null)
            {
                Debug.LogError("[WorkshopDialogBuilder] LegacyDialogFrame.prefab is missing.");
                return;
            }

            GameObject dialog = (GameObject)PrefabUtility.InstantiatePrefab(frame);
            dialog.name = "WorkshopDialog";
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

            Text title = AddText(dialog.transform, "Title", "Workshop", 42, LegacyUITheme.DimWhite, FontStyle.Bold, TextAnchor.MiddleCenter, 58f);
            Text queueCount = AddText(dialog.transform, "QueueCountText", "0 / 0", 26, LegacyUITheme.BrassBorder, FontStyle.Bold, TextAnchor.MiddleCenter, 44f);

            // Empty state before the scroll (lesson from Phase 5C — keeps the message visible
            // right under the header instead of trailing after a large flexible empty scroll box).
            Text empty = AddText(dialog.transform, "EmptyState", "Workshop queue is empty. Craft something from Recipes.", 26, LegacyUITheme.DimWhite, FontStyle.Normal, TextAnchor.MiddleCenter, 90f);
            empty.gameObject.SetActive(false);

            // ── Queue + completed list (vertical rows, not a grid — matches legacy dialog_workshop) ──
            var scroll = new GameObject("ListScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(LayoutElement));
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
            contentGroup.spacing = 12f;
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

            GameObject recipesBtn = CreateButton(dialog.transform, "RecipesButton", "Recipes", 280f, 66f);
            GameObject close = CreateButton(dialog.transform, "CloseButton", "Close", 280f, 66f);

            // ── Recipe overlay (child of dialog, ignores VerticalLayoutGroup, never a 2nd popup) ──
            var overlay = BuildRecipeOverlay(dialog.transform, out var recipeTitle, out var recipeClose,
                out var recipeContentRect, out var recipeEmpty);

            var component = dialog.AddComponent<WorkshopDialog>();
            var serialized = new SerializedObject(component);
            serialized.FindProperty("_titleText").objectReferenceValue = title;
            serialized.FindProperty("_queueCountText").objectReferenceValue = queueCount;
            serialized.FindProperty("_closeButton").objectReferenceValue = close.GetComponent<Button>();
            serialized.FindProperty("_recipesButton").objectReferenceValue = recipesBtn.GetComponent<Button>();
            serialized.FindProperty("_listContent").objectReferenceValue = contentRect;
            serialized.FindProperty("_emptyState").objectReferenceValue = empty.gameObject;
            serialized.FindProperty("_rowBorderSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(RowBorderSpritePath);

            var recipePanelComponent = overlay.AddComponent<WorkshopRecipePanel>();
            var recipeSerialized = new SerializedObject(recipePanelComponent);
            recipeSerialized.FindProperty("_titleText").objectReferenceValue = recipeTitle;
            recipeSerialized.FindProperty("_closeButton").objectReferenceValue = recipeClose.GetComponent<Button>();
            recipeSerialized.FindProperty("_listContent").objectReferenceValue = recipeContentRect;
            recipeSerialized.FindProperty("_emptyState").objectReferenceValue = recipeEmpty.gameObject;
            recipeSerialized.FindProperty("_rowBorderSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(RowBorderSpritePath);
            recipeSerialized.ApplyModifiedPropertiesWithoutUndo();

            serialized.FindProperty("_recipePanel").objectReferenceValue = recipePanelComponent;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            PrefabUtility.SaveAsPrefabAssetAndConnect(dialog, OutputPath, InteractionMode.AutomatedAction);
            Object.DestroyImmediate(dialog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            WireMainScene();
            Debug.Log("[WorkshopDialogBuilder] WorkshopDialog prefab built and Main scene wiring applied.");
        }

        private static GameObject BuildRecipeOverlay(Transform dialogTransform, out Text recipeTitle, out GameObject recipeClose,
            out RectTransform recipeContentRect, out Text recipeEmpty)
        {
            var overlay = new GameObject("RecipeOverlay", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            overlay.transform.SetParent(dialogTransform, false);
            var overlayRect = (RectTransform)overlay.transform;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            overlay.GetComponent<Image>().color = LegacyUITheme.CardviewDarkBackground;
            overlay.GetComponent<LayoutElement>().ignoreLayout = true;

            var box = new GameObject("Box", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            box.transform.SetParent(overlay.transform, false);
            var boxRect = (RectTransform)box.transform;
            boxRect.anchorMin = Vector2.zero;
            boxRect.anchorMax = Vector2.one;
            boxRect.offsetMin = new Vector2(30f, 26f);
            boxRect.offsetMax = new Vector2(-30f, -26f);
            var boxLayout = box.GetComponent<VerticalLayoutGroup>();
            boxLayout.spacing = 14f;
            boxLayout.childAlignment = TextAnchor.UpperCenter;
            boxLayout.childControlWidth = true;
            boxLayout.childControlHeight = true;
            boxLayout.childForceExpandWidth = true;
            boxLayout.childForceExpandHeight = false;

            recipeTitle = AddText(box.transform, "Title", "Recipes", 38, LegacyUITheme.DimWhite, FontStyle.Bold, TextAnchor.MiddleCenter, 54f);
            recipeEmpty = AddText(box.transform, "EmptyState", "No recipes defined.", 26, LegacyUITheme.DimWhite, FontStyle.Normal, TextAnchor.MiddleCenter, 80f);
            recipeEmpty.gameObject.SetActive(false);

            var scroll = new GameObject("ListScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(LayoutElement));
            scroll.transform.SetParent(box.transform, false);
            scroll.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.12f);
            var scrollLayout = scroll.GetComponent<LayoutElement>();
            scrollLayout.flexibleHeight = 1f;
            scrollLayout.minHeight = 900f;

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
            recipeContentRect = content.GetComponent<RectTransform>();
            recipeContentRect.anchorMin = new Vector2(0f, 1f);
            recipeContentRect.anchorMax = new Vector2(1f, 1f);
            recipeContentRect.pivot = new Vector2(0.5f, 1f);
            recipeContentRect.anchoredPosition = Vector2.zero;
            recipeContentRect.sizeDelta = Vector2.zero;
            var contentGroup = content.GetComponent<VerticalLayoutGroup>();
            contentGroup.padding = new RectOffset(10, 10, 10, 10);
            contentGroup.spacing = 12f;
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
            scrollRect.content = recipeContentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 36f;

            recipeClose = CreateButton(box.transform, "RecipeCloseButton", "Close", 280f, 66f);

            return overlay;
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
                    var property = so.FindProperty("_workshopDialogPrefab");
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
