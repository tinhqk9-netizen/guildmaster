using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

namespace GuildMaster.Editor.UI.Legacy
{
    public static class TestSceneBuilder
    {
        [InitializeOnLoadMethod]
        [MenuItem("Tools/Guild Master/Legacy UI/Build Test Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // 6. Setup Camera so Game View has no "No cameras rendering"
            GameObject camGo = new GameObject("Main Camera");
            Camera cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.05f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.orthographic = true;
            cam.orthographicSize = 960; // Half of 1920

            // Setup Canvas
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // 3. Setup alternating background (stripes) to see transparent/strokes clearly
            GameObject bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            
            for (int i = 0; i < 20; i++)
            {
                GameObject stripe = new GameObject($"Stripe_{i}");
                stripe.transform.SetParent(bgGo.transform, false);
                var sRt = stripe.AddComponent<RectTransform>();
                sRt.anchorMin = new Vector2(0, i * 0.05f);
                sRt.anchorMax = new Vector2(1, (i + 1) * 0.05f);
                sRt.offsetMin = Vector2.zero;
                sRt.offsetMax = Vector2.zero;
                var sImg = stripe.AddComponent<Image>();
                // Light and dark grey to reveal pure white, brass, and black
                sImg.color = (i % 2 == 0) ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.4f, 0.4f, 0.4f);
            }

            // 1. All 6 shapes
            string[] prefabs = {
                "LegacyPanel", // dialog_border
                "LegacyCardFrame", // object_border_dim_white
                "LegacyButtonFrame", // object_border_brass
                "LegacyNoBackgroundFrame", // object_border_no_background
                "LegacyAscendedFrame" // object_border_ascended
                // LegacyRoundedLeftAscendedFrame handled specially
            };

            string[] labels = {
                "dialog_border",
                "object_border_dim_white",
                "object_border_brass",
                "object_border_no_background",
                "object_border_ascended"
            };

            // Font for labels
            Font arial = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (arial == null) arial = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 4. For each shape, 3 sizes: Small Square, Horizontal Card, Large Dialog
            for(int i = 0; i < prefabs.Length; i++)
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Game/Prefabs/UI/Legacy/{prefabs[i]}.prefab");
                if (p != null)
                {
                    float yPos = 800 - i * 360;

                    // Label
                    GameObject labelGo = new GameObject($"Label_{prefabs[i]}");
                    labelGo.transform.SetParent(canvasGo.transform, false);
                    var lRt = labelGo.AddComponent<RectTransform>();
                    lRt.anchoredPosition = new Vector2(0, yPos + 120);
                    lRt.sizeDelta = new Vector2(800, 50);
                    var txt = labelGo.AddComponent<Text>();
                    txt.text = labels[i];
                    txt.font = arial;
                    txt.fontSize = 36;
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.color = Color.white;
                    var outline = labelGo.AddComponent<Outline>();
                    outline.effectColor = Color.black;

                    // Small Square (150x150)
                    var instSquare = (GameObject)PrefabUtility.InstantiatePrefab(p, canvasGo.transform);
                    var rtSquare = instSquare.GetComponent<RectTransform>();
                    rtSquare.anchoredPosition = new Vector2(-350, yPos - 50);
                    rtSquare.sizeDelta = new Vector2(150, 150);

                    // Horizontal Card (400x150)
                    var instCard = (GameObject)PrefabUtility.InstantiatePrefab(p, canvasGo.transform);
                    var rtCard = instCard.GetComponent<RectTransform>();
                    rtCard.anchoredPosition = new Vector2(0, yPos - 50);
                    rtCard.sizeDelta = new Vector2(400, 150);

                    // Large Dialog (250x300) (using remaining space on the right)
                    var instDialog = (GameObject)PrefabUtility.InstantiatePrefab(p, canvasGo.transform);
                    var rtDialog = instDialog.GetComponent<RectTransform>();
                    rtDialog.anchoredPosition = new Vector2(350, yPos - 50);
                    rtDialog.sizeDelta = new Vector2(250, 300);
                }
            }

            // 5. Special handling for rounded-left ascended placed next to normal rectangle
            float lastYPos = 800 - 5 * 360;
            
            // Label
            GameObject labelLGo = new GameObject($"Label_Rounded_Left");
            labelLGo.transform.SetParent(canvasGo.transform, false);
            var lLRt = labelLGo.AddComponent<RectTransform>();
            lLRt.anchoredPosition = new Vector2(0, lastYPos + 120);
            lLRt.sizeDelta = new Vector2(800, 50);
            var txtL = labelLGo.AddComponent<Text>();
            txtL.text = "rounded-left vs normal rectangle";
            txtL.font = arial;
            txtL.fontSize = 36;
            txtL.alignment = TextAnchor.MiddleCenter;
            txtL.color = Color.white;
            var outlineL = labelLGo.AddComponent<Outline>();
            outlineL.effectColor = Color.black;

            var pLeft = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Game/Prefabs/UI/Legacy/LegacyRoundedLeftAscendedFrame.prefab");
            var pNorm = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Game/Prefabs/UI/Legacy/LegacyAscendedFrame.prefab");

            if (pLeft != null && pNorm != null)
            {
                // Left Rounded
                var instLeft = (GameObject)PrefabUtility.InstantiatePrefab(pLeft, canvasGo.transform);
                var rtLeft = instLeft.GetComponent<RectTransform>();
                rtLeft.anchoredPosition = new Vector2(-220, lastYPos - 50);
                rtLeft.sizeDelta = new Vector2(400, 200);

                // Normal Rectangle next to it
                var instNorm = (GameObject)PrefabUtility.InstantiatePrefab(pNorm, canvasGo.transform);
                var rtNorm = instNorm.GetComponent<RectTransform>();
                rtNorm.anchoredPosition = new Vector2(220, lastYPos - 50);
                rtNorm.sizeDelta = new Vector2(400, 200);
            }
            
            // Ensure folder exists
            if (!System.IO.Directory.Exists("Assets/_Game/Scenes/Tests"))
            {
                System.IO.Directory.CreateDirectory("Assets/_Game/Scenes/Tests");
            }

            EditorSceneManager.SaveScene(scene, "Assets/_Game/Scenes/Tests/LegacyShapeTest.unity");
            Debug.Log("Test scene built at Assets/_Game/Scenes/Tests/LegacyShapeTest.unity");
        }
    }
}
