using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Character;
using GuildMaster.Runtime.UI.Tavern;
using GuildMaster.Runtime.UI.Core;

public class SetupBatch2UI : EditorWindow
{
    // [MenuItem("GuildMaster/Setup Batch 2 UI")] — DISABLED: UI is manually edited, do not re-apply.
    public static void RunSetup()
    {
        if (EditorApplication.isPlaying || Application.isPlaying) return;

        Debug.Log("Starting Batch 2 UI High Precision Redesign Setup...");

        // Ensure Main scene is open
        string mainScenePath = "Assets/_Game/Scenes/Main.unity";
        var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (activeScene.path != mainScenePath)
        {
            Debug.Log($"Opening Main scene at: {mainScenePath}");
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(mainScenePath);
        }

        Canvas mainCanvas = Object.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
        if (mainCanvas == null)
        {
            Debug.LogError("Could not find Canvas in Main scene!");
            return;
        }

        // -------------------------------------------------------------
        // 1. SETUP TAVERN SCREEN TABS
        // -------------------------------------------------------------
        TavernScreen tavernScreen = Object.FindAnyObjectByType<TavernScreen>(FindObjectsInactive.Include);
        if (tavernScreen != null)
        {
            Undo.RecordObject(tavernScreen, "Setup TavernScreen Tabs");
            var so = new SerializedObject(tavernScreen);

            // Find or create TabBar container right after Header
            Transform tabBarTransform = tavernScreen.transform.Find("TabBar");
            if (tabBarTransform == null)
            {
                GameObject tabBarGo = new GameObject("TabBar", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                tabBarGo.transform.SetParent(tavernScreen.transform, false);
                tabBarTransform = tabBarGo.transform;
                tabBarTransform.SetSiblingIndex(1); // Place right after Header

                var hlg = tabBarGo.GetComponent<HorizontalLayoutGroup>();
                hlg.spacing = 10;
                hlg.padding = new RectOffset(10, 10, 5, 5);
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = true;
                hlg.childForceExpandHeight = true;

                var le = tabBarGo.GetComponent<LayoutElement>();
                le.minHeight = 44;
                le.preferredHeight = 44;
                le.flexibleWidth = 1;
            }

            // Tavern & Quarters Tab Buttons
            Button tavernTab = so.FindProperty("_tavernTabButton").objectReferenceValue as Button;
            if (tavernTab == null || tavernTab.transform.parent != tabBarTransform)
            {
                if (tavernTab != null) DestroyImmediate(tavernTab.gameObject);
                tavernTab = CreateStyledButton(tabBarTransform, "TavernTabBtn", "Tavern", Vector2.zero, new Vector2(0, 0), new Color(0.2f, 0.45f, 0.75f));
                so.FindProperty("_tavernTabButton").objectReferenceValue = tavernTab;
            }

            Button quartersTab = so.FindProperty("_quartersTabButton").objectReferenceValue as Button;
            if (quartersTab == null || quartersTab.transform.parent != tabBarTransform)
            {
                if (quartersTab != null) DestroyImmediate(quartersTab.gameObject);
                quartersTab = CreateStyledButton(tabBarTransform, "QuartersTabBtn", "Quarters", Vector2.zero, new Vector2(0, 0), new Color(0.25f, 0.3f, 0.4f));
                so.FindProperty("_quartersTabButton").objectReferenceValue = quartersTab;
            }

            // Quarters Content Panel
            RectTransform quartersContent = so.FindProperty("_quartersContent").objectReferenceValue as RectTransform;
            if (quartersContent == null)
            {
                GameObject go = new GameObject("QuartersContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
                go.transform.SetParent(tavernScreen.transform, false);
                quartersContent = go.GetComponent<RectTransform>();

                var vlg = go.GetComponent<VerticalLayoutGroup>();
                vlg.spacing = 12;
                vlg.padding = new RectOffset(20, 20, 20, 20);
                vlg.childControlWidth = true;
                vlg.childControlHeight = false;

                var le = go.GetComponent<LayoutElement>();
                le.flexibleHeight = 1;
                le.flexibleWidth = 1;

                quartersContent.gameObject.SetActive(false);
                so.FindProperty("_quartersContent").objectReferenceValue = quartersContent;
            }

            // Tavern Content Panel
            RectTransform tavernContent = so.FindProperty("_tavernContent").objectReferenceValue as RectTransform;
            if (tavernContent == null)
            {
                GameObject go = new GameObject("TavernContent", typeof(RectTransform), typeof(LayoutElement));
                go.transform.SetParent(tavernScreen.transform, false);
                go.transform.SetSiblingIndex(2);
                tavernContent = go.GetComponent<RectTransform>();

                var le = go.GetComponent<LayoutElement>();
                le.flexibleHeight = 1;
                le.flexibleWidth = 1;

                so.FindProperty("_tavernContent").objectReferenceValue = tavernContent;
            }

            // Move Upgrade buttons into QuartersContent
            Button upgQuarters = so.FindProperty("_upgradeQuartersButton").objectReferenceValue as Button;
            Button upgCap = so.FindProperty("_upgradeCapacityButton").objectReferenceValue as Button;
            Button upgTime = so.FindProperty("_upgradeTimeButton").objectReferenceValue as Button;

            if (upgQuarters != null) upgQuarters.transform.SetParent(quartersContent, false);
            if (upgCap != null) upgCap.transform.SetParent(quartersContent, false);
            if (upgTime != null) upgTime.transform.SetParent(quartersContent, false);

            so.ApplyModifiedProperties();
            Debug.Log("[SetupBatch2UI] TavernScreen Tabs & Panels configured.");
        }

        // -------------------------------------------------------------
        // 2. SETUP CHARACTER SCREEN & EQUIPMENT POPUP
        // -------------------------------------------------------------
        CharacterScreen charScreen = Object.FindAnyObjectByType<CharacterScreen>(FindObjectsInactive.Include);
        if (charScreen != null)
        {
            Undo.RecordObject(charScreen, "Setup CharacterScreen UI");
            var so = new SerializedObject(charScreen);

            // Unlock CharacterScreen children for free manual editing in Unity Scene View
            var charVlg = charScreen.GetComponent<VerticalLayoutGroup>();
            if (charVlg != null) DestroyImmediate(charVlg);

            // Enable Rich Text on _detailText
            Text detailTxt = so.FindProperty("_detailText").objectReferenceValue as Text;
            if (detailTxt != null)
            {
                detailTxt.supportRichText = true;
            }

            // Disable Raycast Target on decorative backgrounds to prevent blocking clicks
            Image charScreenImg = charScreen.GetComponent<Image>();
            if (charScreenImg != null) charScreenImg.raycastTarget = false;

            Transform detailPanelTr = charScreen.transform.Find("DetailPanel");
            if (detailPanelTr != null)
            {
                Image detailImg = detailPanelTr.GetComponent<Image>();
                if (detailImg != null) detailImg.raycastTarget = false;

                RectTransform detailRt = detailPanelTr.GetComponent<RectTransform>();
                if (detailRt != null)
                {
                    detailRt.anchorMin = new Vector2(0f, 0.10f);
                    detailRt.anchorMax = new Vector2(1f, 0.42f);
                    detailRt.offsetMin = new Vector2(12, 4);
                    detailRt.offsetMax = new Vector2(-12, -4);
                }
            }

            // Find Button Bar
            Button addBtn = so.FindProperty("_addToPartyButton").objectReferenceValue as Button;
            Transform btnContainer = (addBtn != null) ? addBtn.transform.parent : charScreen.transform;

            // Fix Buttons Bar Anchors (Bottom 10%)
            if (btnContainer != null)
            {
                RectTransform btnRt = btnContainer.GetComponent<RectTransform>();
                if (btnRt != null)
                {
                    btnRt.anchorMin = new Vector2(0f, 0.00f);
                    btnRt.anchorMax = new Vector2(1f, 0.10f);
                    btnRt.offsetMin = new Vector2(12, 4);
                    btnRt.offsetMax = new Vector2(-12, -4);
                }

                // Ensure Button Container has a HorizontalLayoutGroup
                if (btnContainer.GetComponent<HorizontalLayoutGroup>() == null)
                {
                    var hlg = btnContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
                    hlg.spacing = 8;
                    hlg.padding = new RectOffset(8, 8, 4, 4);
                    hlg.childControlWidth = true;
                    hlg.childControlHeight = true;
                    hlg.childForceExpandWidth = true;
                    hlg.childForceExpandHeight = true;
                }
            }

            // Fix _cardContainer Layout (ScrollRect Content & Viewport) (Middle 50%)
            RectTransform cardContainerRt = so.FindProperty("_cardContainer").objectReferenceValue as RectTransform;
            if (cardContainerRt != null)
            {
                Transform viewport = cardContainerRt.parent;
                if (viewport != null)
                {
                    // Ensure Viewport Image has Alpha = 1.0 so Mask stencil does NOT render child cards transparent!
                    Image viewportImg = viewport.GetComponent<Image>();
                    if (viewportImg != null)
                    {
                        viewportImg.color = new Color(1f, 1f, 1f, 1f);
                        viewportImg.raycastTarget = true;
                    }

                    Mask viewportMask = viewport.GetComponent<Mask>();
                    if (viewportMask != null)
                    {
                        viewportMask.showMaskGraphic = false;
                    }

                    RectTransform viewportRt = viewport.GetComponent<RectTransform>();
                    if (viewportRt != null)
                    {
                        viewportRt.anchorMin = Vector2.zero;
                        viewportRt.anchorMax = Vector2.one;
                        viewportRt.offsetMin = Vector2.zero;
                        viewportRt.offsetMax = Vector2.zero;
                    }

                    Transform scrollView = viewport.parent;
                    if (scrollView != null)
                    {
                        RectTransform scrollRt = scrollView.GetComponent<RectTransform>();
                        if (scrollRt != null)
                        {
                            scrollRt.anchorMin = new Vector2(0f, 0.42f);
                            scrollRt.anchorMax = new Vector2(1f, 0.92f);
                            scrollRt.offsetMin = new Vector2(12, 4);
                            scrollRt.offsetMax = new Vector2(-12, -4);
                        }
                    }
                }

                cardContainerRt.anchorMin = new Vector2(0f, 1f);
                cardContainerRt.anchorMax = new Vector2(1f, 1f);
                cardContainerRt.pivot = new Vector2(0.5f, 1f);
                cardContainerRt.anchoredPosition = Vector2.zero;

                // Remove conflicting GridLayoutGroup if present
                var glg = cardContainerRt.GetComponent<GridLayoutGroup>();
                if (glg != null) DestroyImmediate(glg);

                var vlg = cardContainerRt.GetComponent<VerticalLayoutGroup>();
                if (vlg == null) vlg = Undo.AddComponent<VerticalLayoutGroup>(cardContainerRt.gameObject);
                if (vlg != null)
                {
                    vlg.spacing = 10;
                    vlg.padding = new RectOffset(10, 10, 10, 10);
                    vlg.childAlignment = TextAnchor.UpperCenter;
                    vlg.childControlWidth = true;
                    vlg.childControlHeight = false;
                    vlg.childForceExpandWidth = true;
                    vlg.childForceExpandHeight = false;
                }

                var csf = cardContainerRt.GetComponent<ContentSizeFitter>();
                if (csf == null) csf = Undo.AddComponent<ContentSizeFitter>(cardContainerRt.gameObject);
                if (csf != null)
                {
                    csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }
            }

            // Dismiss Button
            Button dismissBtn = so.FindProperty("_dismissButton").objectReferenceValue as Button;
            if (dismissBtn == null || dismissBtn.transform.parent != btnContainer)
            {
                if (dismissBtn != null) DestroyImmediate(dismissBtn.gameObject);
                dismissBtn = CreateStyledButton(btnContainer, "DismissButton", "Dismiss", Vector2.zero, new Vector2(90, 36), new Color(0.75f, 0.2f, 0.2f));
                so.FindProperty("_dismissButton").objectReferenceValue = dismissBtn;
            }

            // Equipment Popup Setup
            EquipmentPopup popup = so.FindProperty("_equipmentPopup").objectReferenceValue as EquipmentPopup;
            if (popup == null || popup.transform.parent != mainCanvas.transform)
            {
                if (popup != null && popup.transform.parent != mainCanvas.transform)
                {
                    DestroyImmediate(popup.gameObject);
                    popup = null;
                }

                if (popup == null)
                {
                    // Create Popup Root (Full Screen Modal Overlay as last child of Canvas)
                    GameObject popupRoot = new GameObject("EquipmentPopup", typeof(RectTransform), typeof(CanvasGroup), typeof(EquipmentPopup));
                    popupRoot.transform.SetParent(mainCanvas.transform, false);
                    popupRoot.transform.SetAsLastSibling(); // Ensure it renders on top of ALL screens

                    RectTransform rootRt = popupRoot.GetComponent<RectTransform>();
                    SetupFullStretch(rootRt);

                    // Dim Overlay Background
                    GameObject overlay = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
                    overlay.transform.SetParent(popupRoot.transform, false);
                    SetupFullStretch(overlay.GetComponent<RectTransform>());
                    overlay.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

                    // Dialog Card Box (Center Window)
                    GameObject card = new GameObject("DialogCard", typeof(RectTransform), typeof(Image));
                    card.transform.SetParent(popupRoot.transform, false);
                    RectTransform cardRt = card.GetComponent<RectTransform>();
                    cardRt.anchorMin = new Vector2(0.5f, 0.5f);
                    cardRt.anchorMax = new Vector2(0.5f, 0.5f);
                    cardRt.sizeDelta = new Vector2(440, 540);
                    cardRt.anchoredPosition = Vector2.zero;
                    card.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 1f);

                    // Title
                    Text titleTxt = CreateStyledText(card.transform, "Title", "Select Equipment", new Vector2(0, 230), new Vector2(400, 40), 22, TextAnchor.MiddleCenter, Color.yellow);

                    // Item Container
                    GameObject itemContainerGo = new GameObject("ItemContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
                    itemContainerGo.transform.SetParent(card.transform, false);
                    RectTransform itemContainerRt = itemContainerGo.GetComponent<RectTransform>();
                    itemContainerRt.anchorMin = new Vector2(0.05f, 0.18f);
                    itemContainerRt.anchorMax = new Vector2(0.95f, 0.85f);
                    itemContainerRt.offsetMin = Vector2.zero;
                    itemContainerRt.offsetMax = Vector2.zero;

                    VerticalLayoutGroup vlg = itemContainerGo.GetComponent<VerticalLayoutGroup>();
                    vlg.spacing = 8;
                    vlg.childControlWidth = true;
                    vlg.childControlHeight = false;

                    // Buttons
                    Button equipBtn = CreateStyledButton(card.transform, "EquipButton", "Equip Selected", new Vector2(-95, -220), new Vector2(170, 44), new Color(0.2f, 0.65f, 0.35f));
                    Button closeBtn = CreateStyledButton(card.transform, "CloseButton", "Close", new Vector2(95, -220), new Vector2(140, 44), new Color(0.65f, 0.2f, 0.2f));
                    Text feedbackTxt = CreateStyledText(card.transform, "FeedbackText", "", new Vector2(0, -170), new Vector2(400, 30), 14, TextAnchor.MiddleCenter, Color.white);

                    // Bind references into EquipmentPopup script
                    popup = popupRoot.GetComponent<EquipmentPopup>();
                    popup.IsPopup = true; // Mark as Popup Dialog for UIService

                    var popSo = new SerializedObject(popup);
                    popSo.FindProperty("IsPopup").boolValue = true;
                    popSo.FindProperty("_titleText").objectReferenceValue = titleTxt;
                    popSo.FindProperty("_equipButton").objectReferenceValue = equipBtn;
                    popSo.FindProperty("_closeButton").objectReferenceValue = closeBtn;
                    popSo.FindProperty("_feedbackText").objectReferenceValue = feedbackTxt;
                    popSo.FindProperty("_itemContainer").objectReferenceValue = itemContainerRt;
                    popSo.ApplyModifiedProperties();

                    popupRoot.SetActive(false); // Hide initially
                }
                so.FindProperty("_equipmentPopup").objectReferenceValue = popup;
            }

            so.ApplyModifiedProperties();
            Debug.Log("[SetupBatch2UI] CharacterScreen & EquipmentPopup configured.");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        AssetDatabase.SaveAssets();

        CheckMissingScripts();
        Debug.Log(">>> Batch 2 UI High Precision Setup Completed! <<<");
    }

    private static void SetupFullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Button CreateStyledButton(Transform parent, string name, string textContent, Vector2 pos, Vector2 size, Color bgColor)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.GetComponent<Image>();
        img.color = bgColor;

        Text txt = CreateStyledText(go.transform, "Text", textContent, Vector2.zero, size, 16, TextAnchor.MiddleCenter, Color.white);

        return go.GetComponent<Button>();
    }

    private static Text CreateStyledText(Transform parent, string name, string content, Vector2 pos, Vector2 size, int fontSize, TextAnchor alignment, Color textColor)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Text txt = go.GetComponent<Text>();
        txt.text = content;
        txt.fontSize = fontSize;
        txt.alignment = alignment;
        txt.color = textColor;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.supportRichText = true;

        return txt;
    }

    private static void CheckMissingScripts()
    {
        Debug.Log("Checking for Missing Scripts or References...");
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject go in rootObjects)
        {
            Component[] components = go.GetComponentsInChildren<Component>(true);
            foreach (Component c in components)
            {
                if (c == null)
                {
                    Debug.LogError($"Missing Script found on GameObject: {go.name}", go);
                }
            }
        }
        Debug.Log("Check Complete.");
    }
}
