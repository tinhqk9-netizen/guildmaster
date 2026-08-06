using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using GuildMaster.Runtime.Boot;
using GuildMaster.Runtime.UI.Foundation;

namespace GuildMaster.Editor
{
    public static class B3_ErrorPopupSetupTool
    {
        [MenuItem("Tools/B3 Fix/Setup Error Popup")]
        public static void SetupErrorPopup()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "Main")
            {
                if (EditorUtility.DisplayDialog("Wrong Scene", "Please open the Main scene before running this tool.", "Open Main Scene", "Cancel"))
                {
                    EditorSceneManager.OpenScene("Assets/_Game/Scenes/Main.unity");
                }
                else
                {
                    return;
                }
            }

            var bootstrap = Object.FindFirstObjectByType<UIRuntimeBootstrap>(FindObjectsInactive.Include);
            if (bootstrap == null)
            {
                Debug.LogError("Could not find UIRuntimeBootstrap in the scene.");
                return;
            }

            var existingPopup = Object.FindFirstObjectByType<ErrorPopup>(FindObjectsInactive.Include);
            if (existingPopup != null)
            {
                WireBootstrap(bootstrap, existingPopup);
                Debug.Log("ErrorPopup already exists. Wired it to bootstrap.");
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                return;
            }

            // Find a Canvas to put the popup in, preferably a root canvas
            var canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null)
            {
                var goCanvas = new GameObject("Canvas");
                canvas = goCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                goCanvas.AddComponent<CanvasScaler>();
                goCanvas.AddComponent<GraphicRaycaster>();
            }

            // Create Popup
            var popupObj = new GameObject("ErrorPopup");
            popupObj.transform.SetParent(canvas.transform, false);
            var rect = popupObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            
            var image = popupObj.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.9f);

            var errorPopup = popupObj.AddComponent<ErrorPopup>();

            // Title
            var titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(popupObj.transform, false);
            var titleText = titleObj.AddComponent<Text>();
            titleText.text = "CRITICAL ERROR";
            titleText.fontSize = 40;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.red;
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 200);
            titleRect.sizeDelta = new Vector2(600, 100);

            // Message
            var msgObj = new GameObject("MessageText");
            msgObj.transform.SetParent(popupObj.transform, false);
            var msgText = msgObj.AddComponent<Text>();
            msgText.text = "Error message here";
            msgText.fontSize = 24;
            msgText.alignment = TextAnchor.MiddleCenter;
            msgText.color = Color.white;
            var msgRect = msgObj.GetComponent<RectTransform>();
            msgRect.anchoredPosition = new Vector2(0, 50);
            msgRect.sizeDelta = new Vector2(800, 200);

            // Retry Button
            var retryObj = new GameObject("RetryButton");
            retryObj.transform.SetParent(popupObj.transform, false);
            var retryImage = retryObj.AddComponent<Image>();
            retryImage.color = Color.green;
            var retryButton = retryObj.AddComponent<Button>();
            var retryRect = retryObj.GetComponent<RectTransform>();
            retryRect.anchoredPosition = new Vector2(-150, -150);
            retryRect.sizeDelta = new Vector2(200, 60);

            var retryTextObj = new GameObject("Text");
            retryTextObj.transform.SetParent(retryObj.transform, false);
            var retryText = retryTextObj.AddComponent<Text>();
            retryText.text = "Retry";
            retryText.fontSize = 24;
            retryText.alignment = TextAnchor.MiddleCenter;
            retryText.color = Color.black;
            retryTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 60);

            // Reset Data Button
            var resetObj = new GameObject("ResetDataButton");
            resetObj.transform.SetParent(popupObj.transform, false);
            var resetImage = resetObj.AddComponent<Image>();
            resetImage.color = Color.red;
            var resetButton = resetObj.AddComponent<Button>();
            var resetRect = resetObj.GetComponent<RectTransform>();
            resetRect.anchoredPosition = new Vector2(150, -150);
            resetRect.sizeDelta = new Vector2(200, 60);

            var resetTextObj = new GameObject("Text");
            resetTextObj.transform.SetParent(resetObj.transform, false);
            var resetText = resetTextObj.AddComponent<Text>();
            resetText.text = "Reset Data";
            resetText.fontSize = 24;
            resetText.alignment = TextAnchor.MiddleCenter;
            resetText.color = Color.white;
            resetTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 60);

            // Wire up ErrorPopup
            var serializedPopup = new SerializedObject(errorPopup);
            serializedPopup.FindProperty("_errorText").objectReferenceValue = msgText;
            serializedPopup.FindProperty("_retryButton").objectReferenceValue = retryButton;
            serializedPopup.FindProperty("_resetDataButton").objectReferenceValue = resetButton;
            serializedPopup.ApplyModifiedProperties();

            popupObj.SetActive(false);

            WireBootstrap(bootstrap, errorPopup);

            Debug.Log("Successfully generated ErrorPopup and wired it to UIRuntimeBootstrap.");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void WireBootstrap(UIRuntimeBootstrap bootstrap, ErrorPopup popup)
        {
            var serializedBootstrap = new SerializedObject(bootstrap);
            serializedBootstrap.FindProperty("_errorPopup").objectReferenceValue = popup;
            serializedBootstrap.ApplyModifiedProperties();
        }
    }
}
