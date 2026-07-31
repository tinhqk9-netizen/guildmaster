#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using GuildMaster.Runtime.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.UI.Character;
using GuildMaster.Runtime.UI.Inventory;
using GuildMaster.Runtime.UI.Craft;
using GuildMaster.Runtime.UI.Merchant;
using GuildMaster.Runtime.UI.Dungeon;
using GuildMaster.Runtime.UI.Quest;
using GuildMaster.Runtime.UI.Settings;
using GuildMaster.Runtime.UI.Tavern;
using GuildMaster.Runtime.UI.HUD;
using GuildMaster.Editor.UI;

namespace GuildMaster.Editor
{
    /// <summary>
    /// Unified tool that rebuilds all UI screens in Main.unity cleanly,
    /// binding all required serialized fields and setting up Edit Mode preview cards.
    /// </summary>
    public static class GuildMasterUnifiedApply
    {
        [MenuItem("GuildMaster/Apply Unified UI")]
        public static void ApplyAllUI()
        {
            var uiRoot = GameObject.Find("UI");
            if (uiRoot == null)
            {
                Debug.LogError("[UnifiedApply] Hierarchy node 'UI' not found!");
                return;
            }

            var safeArea = uiRoot.transform.Find("UICanvas/SafeArea");
            if (safeArea == null)
            {
                Debug.LogError("[UnifiedApply] UICanvas/SafeArea not found!");
                return;
            }

            var screenRoot = safeArea.Find("ScreenRoot");
            if (screenRoot == null)
            {
                var srGo = new GameObject("ScreenRoot", typeof(RectTransform));
                srGo.transform.SetParent(safeArea, false);
                var srRt = (RectTransform)srGo.transform;
                srRt.anchorMin = Vector2.zero;
                srRt.anchorMax = Vector2.one;
                srRt.offsetMin = Vector2.zero;
                srRt.offsetMax = Vector2.zero;
                screenRoot = srGo.transform;
            }

            Undo.RegisterFullObjectHierarchyUndo(screenRoot.gameObject, "Apply Unified UI");

            BuildTavern(screenRoot);
            BuildCharacter(screenRoot);
            BuildInventory(screenRoot);
            BuildCraft(screenRoot);
            BuildMerchant(screenRoot);
            BuildDungeon(screenRoot);
            BuildQuest(screenRoot);
            BuildSettings(screenRoot);

            SetupHUD(uiRoot.transform);

            ShowOnlyScreenInEditMode(screenRoot, "CharacterScreen");

            EditorUtility.SetDirty(screenRoot.gameObject);
            if (screenRoot.gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(screenRoot.gameObject.scene);

            Debug.Log("[UnifiedApply] ✅ Rebuilt and bound all UI screens! Displaying 'CharacterScreen' in Edit Mode.");
        }

        [MenuItem("GuildMaster/Show Screen/Character Screen")]
        public static void ShowCharacterScreenMenu() => ShowScreenByName("CharacterScreen");

        [MenuItem("GuildMaster/Show Screen/Tavern Screen")]
        public static void ShowTavernScreenMenu() => ShowScreenByName("TavernScreen");

        [MenuItem("GuildMaster/Show Screen/Inventory Screen")]
        public static void ShowInventoryScreenMenu() => ShowScreenByName("InventoryScreen");

        [MenuItem("GuildMaster/Show Screen/Craft Screen")]
        public static void ShowCraftScreenMenu() => ShowScreenByName("CraftScreen");

        [MenuItem("GuildMaster/Show Screen/Merchant Screen")]
        public static void ShowMerchantScreenMenu() => ShowScreenByName("MerchantScreen");

        [MenuItem("GuildMaster/Show Screen/Dungeon Screen")]
        public static void ShowDungeonScreenMenu() => ShowScreenByName("DungeonScreen");

        [MenuItem("GuildMaster/Show Screen/Quest Screen")]
        public static void ShowQuestScreenMenu() => ShowScreenByName("QuestScreen");

        [MenuItem("GuildMaster/Show Screen/Settings Screen")]
        public static void ShowSettingsScreenMenu() => ShowScreenByName("SettingsScreen");

        private static void ShowScreenByName(string screenName)
        {
            var uiRoot = GameObject.Find("UI");
            if (uiRoot == null) return;
            var safeArea = uiRoot.transform.Find("UICanvas/SafeArea");
            if (safeArea == null) return;
            var screenRoot = safeArea.Find("ScreenRoot");
            if (screenRoot == null) return;
            ShowOnlyScreenInEditMode(screenRoot, screenName);
            EditorUtility.SetDirty(screenRoot.gameObject);
            if (screenRoot.gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(screenRoot.gameObject.scene);
            Debug.Log($"[UnifiedApply] Showing '{screenName}' in Edit Mode.");
        }

        public static void ShowOnlyScreenInEditMode(Transform screenRoot, string targetScreenName)
        {
            if (screenRoot == null) return;
            for (int i = 0; i < screenRoot.childCount; i++)
            {
                var child = screenRoot.GetChild(i);
                child.gameObject.SetActive(child.name == targetScreenName);
            }
        }

        private static T EnsureCleanScreen<T>(Transform parent, string name, UIScreenId screenId) where T : UIScreen
        {
            var old = parent.Find(name);
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);

            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.10f, 0.12f, 0.16f, 0.98f);

            var screen = go.AddComponent<T>();
            SetScreenId(screen, screenId);

            return screen;
        }

        private static void SetScreenId(UIScreen screen, UIScreenId screenId)
        {
            if (screen == null) return;
            screen.ScreenId = screenId; // Set public field directly!
            var so = new SerializedObject(screen);
            var prop = so.FindProperty("ScreenId") ?? so.FindProperty("_screenId");
            if (prop != null)
            {
                prop.enumValueIndex = (int)screenId;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void Bind(UnityEngine.Object target, string propName, UnityEngine.Object val)
        {
            if (target == null || val == null) return;
            var so = new SerializedObject(target);
            var prop = so.FindProperty(propName);
            if (prop != null)
            {
                prop.objectReferenceValue = val;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static Text AddTextChild(Transform parent, string name, string defaultText, int fontSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var txt = go.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize;
            txt.color = color;
            txt.text = defaultText;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.raycastTarget = false;

            return txt;
        }

        private static Transform AddTabBar(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = UITemporaryTheme.TabBarHeight;
            le.minHeight       = UITemporaryTheme.TabBarHeight;
            le.flexibleHeight  = 0;

            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            hlg.childForceExpandWidth  = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth      = true;
            hlg.childControlHeight     = true;

            return go.transform;
        }

        private static Button AddTabButton(Transform tabBar, string name, string label, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(tabBar, false);

            var img = go.AddComponent<Image>();
            img.color = UITemporaryTheme.TabInactive;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var txtGo = new GameObject("Text", typeof(RectTransform));
            txtGo.transform.SetParent(go.transform, false);
            var txtRt = (RectTransform)txtGo.transform;
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero; txtRt.offsetMax = Vector2.zero;

            var txt = txtGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = UITemporaryTheme.SmallFontSize;
            txt.color = UITemporaryTheme.TextPrimary;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = label;

            if (onClick != null) btn.onClick.AddListener(onClick);
            return btn;
        }

        // ── Individual Screen Builders ──────────────────────────────────────────

        private static void BuildTavern(Transform screenRoot)
        {
            var tav      = EnsureCleanScreen<TavernScreen>(screenRoot, "TavernScreen", UIScreenId.Tavern);
            var scaffold = UIScreenLayoutBuilder.Build(tav.transform, "Tavern");

            // Tab bar inserted right after Header (Sibling index 1)
            var tabBar = AddTabBar(tav.transform, "TabBar_Tavern");
            tabBar.SetSiblingIndex(1); // Header=0, TabBar=1, SummaryRow=2...

            var tabTavern   = AddTabButton(tabBar, "Tab_Tavern",   "Tavern",   tav.OnClickTabTavern);
            var tabQuarters = AddTabButton(tabBar, "Tab_Quarters", "Quarters", tav.OnClickTabQuarters);

            // Centered 2-line summary inside SummaryRow (Sibling index 2)
            scaffold.SummaryRow.SetSiblingIndex(2);

            var oldSumTxt = scaffold.SummaryRow.Find("SummaryText");
            if (oldSumTxt != null) UnityEngine.Object.DestroyImmediate(oldSumTxt.gameObject);

            var oldVlg = scaffold.SummaryRow.GetComponent<VerticalLayoutGroup>();
            if (oldVlg != null) UnityEngine.Object.DestroyImmediate(oldVlg);

            var oldHlg = scaffold.SummaryRow.GetComponent<HorizontalLayoutGroup>();
            if (oldHlg != null) UnityEngine.Object.DestroyImmediate(oldHlg);

            var sumVlg = scaffold.SummaryRow.gameObject.AddComponent<VerticalLayoutGroup>();
            sumVlg.padding = new RectOffset(8, 8, 4, 4);
            sumVlg.spacing = 2;
            sumVlg.childForceExpandWidth = true;
            sumVlg.childForceExpandHeight = false;
            sumVlg.childControlWidth = true;
            sumVlg.childControlHeight = true;

            var popText = AddTextChild(scaffold.SummaryRow, "PopulationText", "Guests: 1/2  |  Quarters: 2 capacity",
                UITemporaryTheme.SmallFontSize, UITemporaryTheme.TextHighlight);
            popText.alignment = TextAnchor.MiddleCenter;

            var timerText = AddTextChild(scaffold.SummaryRow, "TimerText", "Next visitor in: 3h 33m",
                UITemporaryTheme.SmallFontSize, UITemporaryTheme.TextSecondary);
            timerText.alignment = TextAnchor.MiddleCenter;

            // ContentArea (Sibling index 3)
            scaffold.ContentArea.parent.parent.SetSiblingIndex(3); // ContentScroll
            var contentRt = scaffold.ContentArea;
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot     = new Vector2(0.5f, 1f);
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            var oldGrid = contentRt.GetComponent<GridLayoutGroup>();
            if (oldGrid != null) UnityEngine.Object.DestroyImmediate(oldGrid);

            var vlg = contentRt.gameObject.GetComponent<VerticalLayoutGroup>()
                      ?? contentRt.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.spacing = 8;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var csf = contentRt.gameObject.GetComponent<ContentSizeFitter>()
                      ?? contentRt.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            // Detail Panel (Sibling index 4) with green feedback text container
            scaffold.DetailPanel.SetSiblingIndex(4);
            var feedbackTxt = AddTextChild(scaffold.DetailPanel, "FeedbackText", "",
                UITemporaryTheme.SmallFontSize, UITemporaryTheme.SuccessColor);
            feedbackTxt.alignment = TextAnchor.MiddleRight;
            var fbRt = (RectTransform)feedbackTxt.transform;
            fbRt.anchorMin = new Vector2(0.5f, 0);
            fbRt.anchorMax = new Vector2(1f, 1f);
            fbRt.offsetMin = new Vector2(0, 8);
            fbRt.offsetMax = new Vector2(-16, -8);

            // ActionBar (Sibling index 5)
            scaffold.ActionBar.SetSiblingIndex(5);

            var recruitBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar,
                "Btn_RecruitSelected", "Recruit Selected", true, tav.OnClickRecruitSelected);
            var prevBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Prev", "< Prev", false, tav.OnClickSelectPrevious);
            var nextBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Next", "Next >", false, tav.OnClickSelectNext);

            var upgradeQuartersBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar,
                "Btn_UpgradeQuarters", "Upgrade Quarters", false, tav.OnClickUpgradeQuarters);
            var upgradeCapacityBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar,
                "Btn_UpgradeCapacity", "Upgrade Capacity", false, tav.OnClickUpgradeTavernCapacity);
            var upgradeTimeBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar,
                "Btn_UpgradeTime", "Upgrade Speed", false, tav.OnClickUpgradeTavernTime);

            Bind(tav, "_tabTavernBtn",     tabTavern);
            Bind(tav, "_tabQuartersBtn",   tabQuarters);
            Bind(tav, "_cardContainer",    scaffold.ContentArea);
            Bind(tav, "_timerText",        timerText);
            Bind(tav, "_populationText",   popText);
            Bind(tav, "_detailText",       scaffold.DetailText);
            Bind(tav, "_feedbackText",     feedbackTxt);

            Bind(tav, "_recruitButton", recruitBtn);
            Bind(tav, "_prevButton",    prevBtn);
            Bind(tav, "_nextButton",    nextBtn);

            Bind(tav, "_upgradeQuartersButton", upgradeQuartersBtn);
            Bind(tav, "_upgradeCapacityButton", upgradeCapacityBtn);
            Bind(tav, "_upgradeTimeButton",     upgradeTimeBtn);

            // Add Edit Mode preview cards for Tavern
            if (scaffold.ContentArea != null && scaffold.ContentArea.childCount == 0)
            {
                UICardFactory.CreateDivider(scaffold.ContentArea, "TAVERN VISITORS");
                UICardFactory.CreateCard(scaffold.ContentArea, "⚔️ Footman Warrior (Lv.1)", "Role: Frontline Melee Tank • Recruit Cost: Free", true, true, null, preferredHeight: 70);
                UICardFactory.CreateCard(scaffold.ContentArea, "🔮 Apprentice Mage (Lv.1)", "Role: Elemental Magic Burst Caster • Recruit Cost: Free", false, true, null, preferredHeight: 70);
            }
        }

        private static void BuildCharacter(Transform screenRoot)
        {
            var chr      = EnsureCleanScreen<CharacterScreen>(screenRoot, "CharacterScreen", UIScreenId.Character);
            var scaffold = UIScreenLayoutBuilder.Build(chr.transform, "Characters");

            // Sibling 0: Header
            // Sibling 1: SummaryRow
            var summaryTxt = AddTextChild(scaffold.SummaryRow, "SummaryText", "",
                UITemporaryTheme.SmallFontSize, UITemporaryTheme.TextSecondary);

            // Sibling 2: Party Tab Bar (3 Party Tabs: Party 1, Party 2, Party 3)
            var partyTabBar = AddTabBar(chr.transform, "PartyTabBar");
            partyTabBar.SetSiblingIndex(2);

            // Sibling 3: Party Slot Container (4 Slot cards)
            var partySlotGo = new GameObject("PartySlotContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            partySlotGo.transform.SetParent(chr.transform, false);
            partySlotGo.transform.SetSiblingIndex(3);

            var slotLe = partySlotGo.GetComponent<LayoutElement>();
            slotLe.preferredHeight = 54f;
            slotLe.minHeight       = 54f;
            slotLe.flexibleHeight  = 0f;
            slotLe.flexibleWidth   = 1f;

            var slotHlg = partySlotGo.GetComponent<HorizontalLayoutGroup>();
            slotHlg.padding = new RectOffset(8, 8, 4, 4);
            slotHlg.spacing = 8f;
            slotHlg.childForceExpandWidth  = true;
            slotHlg.childForceExpandHeight = true;
            slotHlg.childControlWidth      = true;
            slotHlg.childControlHeight     = true;

            // Sibling 4: ContentScroll (Hero roster scroll)
            var scrollGo = scaffold.ContentArea.parent.parent.gameObject;
            scrollGo.transform.SetSiblingIndex(4);

            var scrollLe = scrollGo.GetComponent<LayoutElement>() ?? scrollGo.AddComponent<LayoutElement>();
            scrollLe.flexibleHeight = 1f;
            scrollLe.minHeight       = 150f;
            scrollLe.flexibleWidth   = 1f;

            var viewportRt = (RectTransform)scaffold.ContentArea.parent;
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;
            viewportRt.anchoredPosition = Vector2.zero;

            var contentRt = scaffold.ContentArea;
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot     = new Vector2(0.5f, 1f);
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;
            contentRt.anchoredPosition = Vector2.zero;

            var oldGrid = contentRt.GetComponent<GridLayoutGroup>();
            if (oldGrid != null) UnityEngine.Object.DestroyImmediate(oldGrid);

            var vlg = contentRt.gameObject.GetComponent<VerticalLayoutGroup>()
                      ?? contentRt.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 8, 8);
            vlg.spacing = 6;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = true;

            var csf = contentRt.gameObject.GetComponent<ContentSizeFitter>()
                      ?? contentRt.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            // Sibling 5: DetailPanel
            scaffold.DetailPanel.SetSiblingIndex(5);
            var feedbackTxt = AddTextChild(scaffold.DetailPanel, "FeedbackText", "",
                UITemporaryTheme.SmallFontSize, UITemporaryTheme.SuccessColor);
            feedbackTxt.alignment = TextAnchor.MiddleRight;
            var fbRt = (RectTransform)feedbackTxt.transform;
            fbRt.anchorMin = new Vector2(0.5f, 0);
            fbRt.anchorMax = new Vector2(1f, 1f);
            fbRt.offsetMin = new Vector2(0, 8);
            fbRt.offsetMax = new Vector2(-16, -8);

            // Sibling 6: ActionBar
            scaffold.ActionBar.SetSiblingIndex(6);

            Bind(chr, "_cardContainer",        scaffold.ContentArea);
            Bind(chr, "_summaryText",          summaryTxt);
            Bind(chr, "_detailText",           scaffold.DetailText);
            Bind(chr, "_partyTabBar",          partyTabBar);
            Bind(chr, "_partySlotContainer",   (RectTransform)partySlotGo.transform);

            var addPartyBtn    = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_AddParty",    "Add to Party 1",    true,  chr.OnClickAddToParty);
            var removePartyBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_RemoveParty", "Remove from Party", false, chr.OnClickRemoveFromParty);
            var equipBtn       = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Equip",       "Equip Item",   true, chr.OnClickEquipWeapon);
            var unequipWpnBtn  = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_UnWpn",       "Unwield",      false, chr.OnClickUnequipWeapon);
            var unequipArmBtn  = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_UnArm",       "Remove Armor", false, chr.OnClickUnequipArmor);
            var unequipAccBtn  = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_UnAcc",       "Remove Acc.",  false, chr.OnClickUnequipAccessory);

            var dismissBtnGo = new GameObject("Btn_Dismiss", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            dismissBtnGo.transform.SetParent(scaffold.ActionBar, false);
            var disLe = dismissBtnGo.GetComponent<LayoutElement>();
            disLe.preferredHeight = 48f;
            disLe.flexibleWidth = 1f;

            var disImg = dismissBtnGo.GetComponent<Image>();
            disImg.color = UITemporaryTheme.ButtonDanger;

            var disBtn = dismissBtnGo.GetComponent<Button>();
            disBtn.targetGraphic = disImg;

            var disTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            disTxtGo.transform.SetParent(dismissBtnGo.transform, false);
            var disTxtRt = (RectTransform)disTxtGo.transform;
            disTxtRt.anchorMin = Vector2.zero; disTxtRt.anchorMax = Vector2.one; disTxtRt.offsetMin = Vector2.zero; disTxtRt.offsetMax = Vector2.zero;

            var disTxt = disTxtGo.GetComponent<Text>();
            disTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            disTxt.fontSize = UITemporaryTheme.SmallFontSize;
            disTxt.color = Color.white;
            disTxt.alignment = TextAnchor.MiddleCenter;
            disTxt.text = "Dismiss";

            // Add Edit Mode preview cards for CharacterScreen
            if (partyTabBar != null && partyTabBar.childCount == 0)
            {
                UICardFactory.CreateCard((RectTransform)partyTabBar, "Party 1 (3/4)", "", true, true, null, 42);
                UICardFactory.CreateCard((RectTransform)partyTabBar, "Party 2 (0/4)", "", false, true, null, 42);
                UICardFactory.CreateCard((RectTransform)partyTabBar, "Party 3 (0/4)", "", false, true, null, 42);
            }

            if (partySlotGo.transform.childCount == 0)
            {
                UICardFactory.CreateCard((RectTransform)partySlotGo.transform, "★ Footman", "Slot 1", true, false, null, 68);
                UICardFactory.CreateCard((RectTransform)partySlotGo.transform, "★ Archer", "Slot 2", true, false, null, 68);
                UICardFactory.CreateCard((RectTransform)partySlotGo.transform, "★ Knight", "Slot 3", true, false, null, 68);
                UICardFactory.CreateCard((RectTransform)partySlotGo.transform, "[Empty]", "Slot 4", false, false, null, 68);
            }

            if (scaffold.ContentArea != null && scaffold.ContentArea.childCount == 0)
            {
                UICardFactory.CreateCard(scaffold.ContentArea, "★ Footman", "Lv.1  HP:100 | XP:0 <color=#4CAF50>★ Party 1</color>", true, true, null, 58);
                UICardFactory.CreateCard(scaffold.ContentArea, "★ Archer", "Lv.1  HP:80 | XP:0 <color=#4CAF50>★ Party 1</color>", false, true, null, 58);
                UICardFactory.CreateCard(scaffold.ContentArea, "Apprentice Mage", "Lv.1  HP:60 | XP:0", false, true, null, 58);
            }

            disBtn.onClick.AddListener(chr.OnClickDismissCharacter);

            Bind(chr, "_addToPartyButton",        addPartyBtn);
            Bind(chr, "_removeFromPartyButton",   removePartyBtn);
            Bind(chr, "_equipButton",             equipBtn);
            Bind(chr, "_unequipWeaponButton",     unequipWpnBtn);
            Bind(chr, "_unequipArmorButton",      unequipArmBtn);
            Bind(chr, "_unequipAccessoryButton",  unequipAccBtn);
            Bind(chr, "_dismissButton",           disBtn);
            Bind(chr, "_feedbackText",            feedbackTxt);

            // Edit Mode persistent Hierarchy preview cards for CharacterScreen
            if (partyTabBar != null && partyTabBar.childCount == 0)
            {
                var tabRt = (RectTransform)partyTabBar;
                UICardFactory.CreateCard(tabRt, "Party 1 (3/4)", "", true, true, null, 36);
                UICardFactory.CreateCard(tabRt, "Party 2 (0/4)", "", false, true, null, 36);
                UICardFactory.CreateCard(tabRt, "Party 3 (0/4)", "", false, true, null, 36);
            }

            var slotRt = (RectTransform)partySlotGo.transform;
            if (slotRt != null && slotRt.childCount == 0)
            {
                UICardFactory.CreateCard(slotRt, "★ Footman", "Slot 1", true, false, null, 44);
                UICardFactory.CreateCard(slotRt, "★ Archer", "Slot 2", true, false, null, 44);
                UICardFactory.CreateCard(slotRt, "★ Knight", "Slot 3", true, false, null, 44);
                UICardFactory.CreateCard(slotRt, "[Empty]", "Slot 4", false, false, null, 44);
            }

            if (scaffold.ContentArea != null && scaffold.ContentArea.childCount == 0)
            {
                UICardFactory.CreateCard(scaffold.ContentArea, "★ Footman (Lv.1)", "HP: 40 | XP: 0 ★ IN PARTY 1", true, true, null, 58);
                UICardFactory.CreateCard(scaffold.ContentArea, "Archer (Lv.1)", "HP: 30 | XP: 0 ★ IN PARTY 1", false, true, null, 58);
                UICardFactory.CreateCard(scaffold.ContentArea, "Knight (Lv.1)", "HP: 55 | XP: 0 ★ IN PARTY 1", false, true, null, 58);
                UICardFactory.CreateCard(scaffold.ContentArea, "Alchemist (Lv.1)", "HP: 35 | XP: 0", false, true, null, 58);
                UICardFactory.CreateCard(scaffold.ContentArea, "Rogue (Lv.1)", "HP: 28 | XP: 0", false, true, null, 58);
                UICardFactory.CreateCard(scaffold.ContentArea, "Apprentice (Lv.1)", "HP: 22 | XP: 0", false, true, null, 58);
            }

            if (summaryTxt != null && string.IsNullOrEmpty(summaryTxt.text))
            {
                summaryTxt.text = "Adventurers: 6  |  Party 1: 3/4";
            }

            if (scaffold.DetailText != null && string.IsNullOrEmpty(scaffold.DetailText.text))
            {
                scaffold.DetailText.text = "<b><color=#FFD700>★ Footman</color></b>  <color=#AAAAAA>Lv.1</color>  <color=#4CAF50><b>[Party 1]</b></color>\nHP: 40 | XP: 0\n\n<b><color=#FFC107>── STATS ──</color></b>\n  CON: 8    DEX: 4    DEF: 20\n  INT: 4    MGD: 20   IMM: 0\n\n<b><color=#FFC107>── EQUIPMENT ──</color></b>\n  🗡️ <b>Weapon:</b>    (empty)\n  🛡️ <b>Armor:</b>     (empty)\n  💍 <b>Accessory:</b> (empty)";
            }
        }

        private static void BuildInventory(Transform screenRoot)
        {
            var inv      = EnsureCleanScreen<InventoryScreen>(screenRoot, "InventoryScreen", UIScreenId.Inventory);
            var scaffold = UIScreenLayoutBuilder.Build(inv.transform, "Inventory");

            // ── Summary row ──────────────────────────────────────────────────────
            var summaryTxt = AddTextChild(scaffold.SummaryRow, "SummaryText", "Items: 0 / 20",
                UITemporaryTheme.SmallFontSize, UITemporaryTheme.TextSecondary);

            // ── Tab bar: All | Weapons | Armor | Consumables | Materials ─────────
            var tabBar = AddTabBar(inv.transform, "TabBar_Inventory");
            tabBar.SetSiblingIndex(2); // Header=0, SummaryRow=1, TabBar=2

            var tabAll         = AddTabButton(tabBar, "Tab_All",         "All",         inv.OnClickTabAll);
            var tabWeapons     = AddTabButton(tabBar, "Tab_Weapons",     "Weapons",     inv.OnClickTabWeapons);
            var tabArmor       = AddTabButton(tabBar, "Tab_Armor",       "Armor",       inv.OnClickTabArmor);
            var tabConsumables = AddTabButton(tabBar, "Tab_Consumables", "Consumables", inv.OnClickTabConsumables);
            var tabMaterials   = AddTabButton(tabBar, "Tab_Materials",   "Materials",   inv.OnClickTabMaterials);

            // ── Bindings ─────────────────────────────────────────────────────────
            Bind(inv, "_cardContainer",      scaffold.ContentArea);
            Bind(inv, "_summaryText",        summaryTxt);
            Bind(inv, "_detailText",         scaffold.DetailText);
            Bind(inv, "_tabAllBtn",          tabAll);
            Bind(inv, "_tabWeaponsBtn",      tabWeapons);
            Bind(inv, "_tabArmorBtn",        tabArmor);
            Bind(inv, "_tabConsumablesBtn",  tabConsumables);
            Bind(inv, "_tabMaterialsBtn",    tabMaterials);

            // ── Action buttons ───────────────────────────────────────────────────
            var lockBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Lock", "Lock",   false, inv.OnClickToggleLockSelected);
            var useBtn  = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Use",  "Use",    true,  inv.OnClickUseSelectedConsumable);
            var sellBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Sell", "Sell",   false, inv.OnClickSellSelected);
            var prevBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Prev", "< Prev", false, inv.OnClickSelectPrevious);
            var nextBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Next", "Next >", false, inv.OnClickSelectNext);

            Bind(inv, "_lockButton",  lockBtn);
            Bind(inv, "_useButton",   useBtn);
            Bind(inv, "_sellButton",  sellBtn);
            Bind(inv, "_prevButton",  prevBtn);
            Bind(inv, "_nextButton",  nextBtn);

            var lockTxt = lockBtn.GetComponentInChildren<Text>();
            if (lockTxt != null) Bind(inv, "_lockButtonLabel", lockTxt);

            // ── Edit Mode preview cards ───────────────────────────────────────────
            if (scaffold.ContentArea != null && scaffold.ContentArea.childCount == 0)
            {
                UICardFactory.CreateDivider(scaffold.ContentArea, "ALL ITEMS  (4)");
                UICardFactory.CreateCard(scaffold.ContentArea, "Health Potion", "Consumable  x5", true,  true, null, preferredHeight: 65);
                UICardFactory.CreateCard(scaffold.ContentArea, "Iron Sword",    "Weapon",         false, true, null, preferredHeight: 65);
                UICardFactory.CreateCard(scaffold.ContentArea, "Leather Armor", "Armor",          false, true, null, preferredHeight: 65);
                UICardFactory.CreateCard(scaffold.ContentArea, "Iron Ore",      "Material  x12",  false, true, null, preferredHeight: 65);
            }

            var feedbackTxt = UICardFactory.CreateFeedbackLabel(scaffold.DetailPanel, "FeedbackText");
            Bind(inv, "_feedbackText", feedbackTxt);
        }

        private static void BuildCraft(Transform screenRoot)
        {
            var crf      = EnsureCleanScreen<CraftScreen>(screenRoot, "CraftScreen", UIScreenId.Craft);
            var scaffold = UIScreenLayoutBuilder.Build(crf.transform, "Workshop");

            var summaryTxt = AddTextChild(scaffold.SummaryRow, "SummaryText", "",
                UITemporaryTheme.SmallFontSize, UITemporaryTheme.TextSecondary);

            // Tab bar inserted after SummaryRow
            var tabBar = AddTabBar(crf.transform, "TabBar_Craft");
            tabBar.SetSiblingIndex(2); // Header=0, SummaryRow=1, TabBar=2

            // Search Bar inserted after TabBar and before ContentScroll
            var searchGo = new GameObject("SearchBarContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(Image), typeof(LayoutElement));
            searchGo.transform.SetParent(crf.transform, false);
            searchGo.transform.SetSiblingIndex(3); // Header=0, SummaryRow=1, TabBar=2, SearchBar=3, ContentScroll=4...

            var searchLe = searchGo.GetComponent<LayoutElement>();
            searchLe.preferredHeight = 42f;
            searchLe.minHeight       = 42f;
            searchLe.flexibleHeight  = 0f;
            searchLe.flexibleWidth   = 1f;

            var searchRt = searchGo.GetComponent<RectTransform>();
            searchRt.anchorMin = new Vector2(0.02f, 0.85f);
            searchRt.anchorMax = new Vector2(0.98f, 0.90f);
            searchRt.offsetMin = Vector2.zero;
            searchRt.offsetMax = Vector2.zero;

            var searchImg = searchGo.GetComponent<Image>();
            searchImg.color = new Color(0.12f, 0.13f, 0.17f, 0.90f);

            var searchHlg = searchGo.GetComponent<HorizontalLayoutGroup>();
            searchHlg.padding = new RectOffset(10, 10, 6, 6);
            searchHlg.spacing = 8f;
            searchHlg.childControlWidth = true;
            searchHlg.childControlHeight = true;
            searchHlg.childForceExpandWidth = true;

            var inputGo = new GameObject("InputField_Search", typeof(RectTransform), typeof(Image), typeof(InputField));
            inputGo.transform.SetParent(searchGo.transform, false);

            var inputImg = inputGo.GetComponent<Image>();
            inputImg.color = new Color(0.18f, 0.20f, 0.26f, 1f);

            var inputField = inputGo.GetComponent<InputField>();

            var placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
            placeholderGo.transform.SetParent(inputGo.transform, false);
            var phRt = placeholderGo.GetComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero; phRt.anchorMax = Vector2.one; phRt.offsetMin = new Vector2(10, 0); phRt.offsetMax = new Vector2(-10, 0);

            var phTxt = placeholderGo.GetComponent<Text>();
            phTxt.text = "🔍  Search recipes (Weapons, Armor, Potions)...";
            phTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            phTxt.fontSize = 18;
            phTxt.color = new Color(0.60f, 0.63f, 0.70f, 0.8f);
            phTxt.alignment = TextAnchor.MiddleLeft;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(inputGo.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one; textRt.offsetMin = new Vector2(10, 0); textRt.offsetMax = new Vector2(-10, 0);

            var txt = textGo.GetComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 18;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleLeft;

            inputField.textComponent = txt;
            inputField.placeholder   = phTxt;

            var tabRecipes   = AddTabButton(tabBar, "Tab_Recipes",   "Recipes",   crf.OnClickTabRecipes);
            var tabQueue     = AddTabButton(tabBar, "Tab_Queue",     "Queue",     crf.OnClickTabQueue);
            var tabCompleted = AddTabButton(tabBar, "Tab_Completed", "Completed", crf.OnClickTabCompleted);

            Bind(crf, "_cardContainer",       scaffold.ContentArea);
            Bind(crf, "_summaryText",         summaryTxt);
            Bind(crf, "_detailText",          scaffold.DetailText);
            Bind(crf, "_tabRecipesBtn",       tabRecipes);
            Bind(crf, "_tabQueueBtn",         tabQueue);
            Bind(crf, "_tabCompletedBtn",     tabCompleted);
            Bind(crf, "_searchInputField",    inputField);
            Bind(crf, "_searchBarContainer",  searchGo);

            var craftBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Craft", "Craft",      true,  crf.OnClickCraftSelected);
            var claimBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Claim", "Claim Item", true,  crf.OnClickClaimSelected);
            var upgradeQueueBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_UpgradeQueue", "Upgrade Queue", false, crf.OnClickUpgradeQueue);

            Bind(crf, "_craftButton", craftBtn);
            Bind(crf, "_claimButton", claimBtn);
            Bind(crf, "_upgradeQueueButton", upgradeQueueBtn);

            if (scaffold.ContentArea != null && scaffold.ContentArea.childCount == 0)
            {
                UICardFactory.CreateDivider(scaffold.ContentArea, "CRAFTING RECIPES");
                UICardFactory.CreateCard(scaffold.ContentArea, "⚔️ Abyssal Cutlass", "Outputs: abyssalcutlass x1", true, true, null, preferredHeight: 70);
                UICardFactory.CreateCard(scaffold.ContentArea, "🛡️ Ancient Armor", "Outputs: ancientarmor x1", false, true, null, preferredHeight: 70);
                UICardFactory.CreateCard(scaffold.ContentArea, "🧪 Health Potion", "Outputs: potion_health x1", false, true, null, preferredHeight: 70);
            }

            var feedbackTxt = UICardFactory.CreateFeedbackLabel(scaffold.DetailPanel, "FeedbackText");
            Bind(crf, "_feedbackText", feedbackTxt);
        }

        private static void BuildMerchant(Transform screenRoot)
        {
            var mer      = EnsureCleanScreen<MerchantScreen>(screenRoot, "MerchantScreen", UIScreenId.Merchant);
            var scaffold = UIScreenLayoutBuilder.Build(mer.transform, "Merchant");

            var summaryTxt = AddTextChild(scaffold.SummaryRow, "SummaryText", "",
                UITemporaryTheme.SmallFontSize, UITemporaryTheme.TextSecondary);

            var tabBar = AddTabBar(mer.transform, "TabBar_Merchant");
            tabBar.SetSiblingIndex(2);

            var tabBuy      = AddTabButton(tabBar, "Tab_Buy",      "Buy",      mer.OnClickTabBuy);
            var tabSell     = AddTabButton(tabBar, "Tab_Sell",     "Sell",     mer.OnClickTabSell);
            var tabListings = AddTabButton(tabBar, "Tab_Listings", "Listings", mer.OnClickTabListings);

            Bind(mer, "_cardContainer",   scaffold.ContentArea);
            Bind(mer, "_summaryText",     summaryTxt);
            Bind(mer, "_detailText",      scaffold.DetailText);
            Bind(mer, "_tabBuyBtn",       tabBuy);
            Bind(mer, "_tabSellBtn",      tabSell);
            Bind(mer, "_tabListingsBtn",  tabListings);

            var buyRegBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_BuyRegular", "Buy Regular", true,  mer.OnClickBuySelectedRegular);
            var buySpBtn  = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_BuySpecial", "Buy Special", true,  mer.OnClickBuySelectedSpecial);
            var sellBtn   = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Sell",       "Sell Item",   false, mer.OnClickSellSelected);
            var claimSoldBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_ClaimSold", "Claim Gold",  true,  mer.OnClickClaimSold);

            Bind(mer, "_buyRegularButton", buyRegBtn);
            Bind(mer, "_buySpecialButton", buySpBtn);
            Bind(mer, "_sellButton",       sellBtn);
            Bind(mer, "_claimSoldButton",  claimSoldBtn);

            if (scaffold.ContentArea != null && scaffold.ContentArea.childCount == 0)
            {
                UICardFactory.CreateDivider(scaffold.ContentArea, "MERCHANT LISTINGS");
                UICardFactory.CreateCard(scaffold.ContentArea, "🧪 Health Potion", "Price: 50g • In Stock", true, true, null, preferredHeight: 65);
                UICardFactory.CreateCard(scaffold.ContentArea, "⚔️ Iron Sword", "Price: 200g • In Stock", false, true, null, preferredHeight: 65);
                UICardFactory.CreateCard(scaffold.ContentArea, "🛡️ Leather Armor", "Price: 180g • In Stock", false, true, null, preferredHeight: 65);
            }

            var feedbackTxt = UICardFactory.CreateFeedbackLabel(scaffold.DetailPanel, "FeedbackText");
            Bind(mer, "_feedbackText", feedbackTxt);
        }

        private static void BuildDungeon(Transform screenRoot)
        {
            var dun      = EnsureCleanScreen<DungeonScreen>(screenRoot, "DungeonScreen", UIScreenId.Dungeon);
            var scaffold = UIScreenLayoutBuilder.Build(dun.transform, "Dungeons");

            var summaryTxt = AddTextChild(scaffold.SummaryRow, "SummaryText", "",
                UITemporaryTheme.SmallFontSize, UITemporaryTheme.TextSecondary);

            var panelSelect = BuildDungeonPanel_Select(dun, scaffold, summaryTxt);
            var panelActive = BuildDungeonPanel_Active(dun, scaffold);
            var panelLoot   = BuildDungeonPanel_Loot(dun, scaffold);

            // Action bar shared buttons (Only 2 buttons: Start & Collect)
            var startBtn   = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Start",   "Start Expedition ⚔️", true,  dun.OnClickStartSelected);
            var collectBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Collect", "Collect Loot 🎒",    true,  dun.OnClickCollectLoot);

            var contBtn    = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Continue","Continue",      false, dun.OnClickContinue);
            var autoBtn    = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_AutoBattle", "Auto: OFF",  false, dun.OnClickToggleAutoBattle);
            var prevBtn    = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_PrevDungeon", "< Dungeon", false, dun.OnClickSelectPreviousDungeon);
            var nextBtn    = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_NextDungeon", "Dungeon >", false, dun.OnClickSelectNextDungeon);

            contBtn.gameObject.SetActive(false);
            autoBtn.gameObject.SetActive(false);
            prevBtn.gameObject.SetActive(false);
            nextBtn.gameObject.SetActive(false);

            Bind(dun, "_summaryText",        summaryTxt);
            Bind(dun, "_startButton",        startBtn);
            Bind(dun, "_continueButton",     contBtn);
            Bind(dun, "_autoBattleButton",   autoBtn);
            Bind(dun, "_collectLootButton",  collectBtn);
            Bind(dun, "_panelSelect",        panelSelect.gameObject);
            Bind(dun, "_panelActive",        panelActive.gameObject);
            Bind(dun, "_panelLoot",          panelLoot.gameObject);

            var feedbackTxt = UICardFactory.CreateFeedbackLabel(scaffold.DetailPanel, "FeedbackText");
            Bind(dun, "_feedbackText", feedbackTxt);

            // Apply Multi-Expedition Layout Fix & Preview Cards for DungeonScreen
            SetupMultiDungeonUI.FixDungeonUILayout();
        }

        private static RectTransform BuildDungeonPanel_Select(DungeonScreen dun, ScreenScaffold scaffold, Text summaryTxt)
        {
            var go  = new GameObject("Panel_Select", typeof(RectTransform));
            go.transform.SetParent(scaffold.ContentArea, false);
            var rt  = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var vg  = go.AddComponent<VerticalLayoutGroup>();
            vg.childForceExpandWidth = true; vg.childForceExpandHeight = false;
            vg.childControlWidth = true; vg.childControlHeight = true;
            vg.spacing = 6;
            vg.padding = new RectOffset(8, 8, 8, 8);

            var selTxtGo = new GameObject("SelectedDungeonText", typeof(RectTransform));
            selTxtGo.transform.SetParent(go.transform, false);
            selTxtGo.AddComponent<LayoutElement>().preferredHeight = 32;
            var selTxt = selTxtGo.AddComponent<Text>();
            selTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            selTxt.fontSize = UITemporaryTheme.TitleFontSize;
            selTxt.color = UITemporaryTheme.TextHighlight;
            selTxt.alignment = TextAnchor.MiddleLeft;
            selTxt.raycastTarget = false;

            var cardContGo = new GameObject("DungeonCardContainer", typeof(RectTransform));
            cardContGo.transform.SetParent(go.transform, false);
            var cardContRt = (RectTransform)cardContGo.transform;
            cardContGo.AddComponent<LayoutElement>().flexibleHeight = 1;
            var cardContVG = cardContGo.AddComponent<VerticalLayoutGroup>();
            cardContVG.childForceExpandWidth = true;
            cardContVG.childForceExpandHeight = false;
            cardContVG.childControlWidth = true;
            cardContVG.childControlHeight = true;
            cardContVG.spacing = 4;

            var partyTextGo = new GameObject("PartyText", typeof(RectTransform));
            partyTextGo.transform.SetParent(go.transform, false);
            partyTextGo.AddComponent<LayoutElement>().preferredHeight = 32;
            var partyTxt = partyTextGo.AddComponent<Text>();
            partyTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            partyTxt.fontSize = UITemporaryTheme.SmallFontSize;
            partyTxt.color = UITemporaryTheme.TextSecondary;
            partyTxt.alignment = TextAnchor.MiddleLeft;
            partyTxt.raycastTarget = false;

            Bind(dun, "_dungeonCardContainer", cardContRt);
            Bind(dun, "_selectedDungeonText",  selTxt);
            Bind(dun, "_partyText",            partyTxt);

            return rt;
        }

        private static RectTransform BuildDungeonPanel_Active(DungeonScreen dun, ScreenScaffold scaffold)
        {
            var go = new GameObject("Panel_Active", typeof(RectTransform));
            go.transform.SetParent(scaffold.ContentArea, false);
            go.SetActive(false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var vg = go.AddComponent<VerticalLayoutGroup>();
            vg.childForceExpandWidth = true; vg.childForceExpandHeight = false;
            vg.childControlWidth = true; vg.childControlHeight = true;
            vg.spacing = 6;
            vg.padding = new RectOffset(8, 8, 8, 8);

            var titleGo = new GameObject("ActiveDungeonTitle", typeof(RectTransform));
            titleGo.transform.SetParent(go.transform, false);
            titleGo.AddComponent<LayoutElement>().preferredHeight = 36;
            var titleTxt = titleGo.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.fontSize = UITemporaryTheme.TitleFontSize;
            titleTxt.color = UITemporaryTheme.TextPrimary;
            titleTxt.alignment = TextAnchor.MiddleLeft;
            titleTxt.raycastTarget = false;

            var turnGo = new GameObject("TurnText", typeof(RectTransform));
            turnGo.transform.SetParent(go.transform, false);
            turnGo.AddComponent<LayoutElement>().preferredHeight = 26;
            var turnTxt = turnGo.AddComponent<Text>();
            turnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            turnTxt.fontSize = UITemporaryTheme.SmallFontSize;
            turnTxt.color = UITemporaryTheme.TextSecondary;
            turnTxt.alignment = TextAnchor.MiddleLeft;
            turnTxt.raycastTarget = false;

            var actionGo = new GameObject("ActionText", typeof(RectTransform));
            actionGo.transform.SetParent(go.transform, false);
            actionGo.AddComponent<LayoutElement>().preferredHeight = 26;
            var actionTxt = actionGo.AddComponent<Text>();
            actionTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            actionTxt.fontSize = UITemporaryTheme.SmallFontSize;
            actionTxt.color = UITemporaryTheme.TextHighlight;
            actionTxt.alignment = TextAnchor.MiddleLeft;
            actionTxt.raycastTarget = false;

            var combatContGo = new GameObject("CombatCardContainer", typeof(RectTransform));
            combatContGo.transform.SetParent(go.transform, false);
            combatContGo.AddComponent<LayoutElement>().flexibleHeight = 1;
            var combatContVG = combatContGo.AddComponent<VerticalLayoutGroup>();
            combatContVG.childForceExpandWidth = true;
            combatContVG.childForceExpandHeight = false;
            combatContVG.childControlWidth = true;
            combatContVG.childControlHeight = true;
            combatContVG.spacing = 4;

            Bind(dun, "_activeDungeonTitle",   titleTxt);
            Bind(dun, "_activeTurnText",        turnTxt);
            Bind(dun, "_activeActionText",      actionTxt);
            Bind(dun, "_combatCardContainer",   (RectTransform)combatContGo.transform);

            return rt;
        }

        private static RectTransform BuildDungeonPanel_Loot(DungeonScreen dun, ScreenScaffold scaffold)
        {
            var go = new GameObject("Panel_Loot", typeof(RectTransform));
            go.transform.SetParent(scaffold.ContentArea, false);
            go.SetActive(false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var vg = go.AddComponent<VerticalLayoutGroup>();
            vg.childForceExpandWidth = true; vg.childForceExpandHeight = false;
            vg.childControlWidth = true; vg.childControlHeight = true;
            vg.spacing = 6;
            vg.padding = new RectOffset(8, 8, 8, 8);

            var lootContGo = new GameObject("LootCardContainer", typeof(RectTransform));
            lootContGo.transform.SetParent(go.transform, false);
            lootContGo.AddComponent<LayoutElement>().flexibleHeight = 1;
            var lootContVG = lootContGo.AddComponent<VerticalLayoutGroup>();
            lootContVG.childForceExpandWidth = true;
            lootContVG.childForceExpandHeight = false;
            lootContVG.childControlWidth = true;
            lootContVG.childControlHeight = true;
            lootContVG.spacing = 4;

            Bind(dun, "_lootCardContainer", (RectTransform)lootContGo.transform);
            return rt;
        }

        private static void BuildQuest(Transform screenRoot)
        {
            var que      = EnsureCleanScreen<QuestScreen>(screenRoot, "QuestScreen", UIScreenId.Quest);
            var scaffold = UIScreenLayoutBuilder.Build(que.transform, "Quests");

            var summaryTxt = AddTextChild(scaffold.SummaryRow, "SummaryText", "",
                UITemporaryTheme.SmallFontSize, UITemporaryTheme.TextSecondary);

            Bind(que, "_cardContainer", scaffold.ContentArea);
            Bind(que, "_summaryText",   summaryTxt);
            Bind(que, "_detailText",    scaffold.DetailText);

            var claimBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Claim", "Claim Reward", true, que.OnClickClaimSelected);
            var cycleBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_CycleDoctrine", "Doctrine: WAR", false, que.OnClickCycleDoctrine);
            UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Prev", "< Prev", false, que.OnClickSelectPrevious);
            UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Next", "Next >", false, que.OnClickSelectNext);

            Bind(que, "_claimButton",       claimBtn);
            Bind(que, "_cycleDoctrineButton", cycleBtn);

            var feedbackTxt = UICardFactory.CreateFeedbackLabel(scaffold.DetailPanel, "FeedbackText");
            Bind(que, "_feedbackText", feedbackTxt);
        }

        private static void BuildSettings(Transform screenRoot)
        {
            var set      = EnsureCleanScreen<SettingsScreen>(screenRoot, "SettingsScreen", UIScreenId.Settings);
            var scaffold = UIScreenLayoutBuilder.Build(set.transform, "Settings");

            var summaryTxt = AddTextChild(scaffold.SummaryRow, "SummaryText", "",
                UITemporaryTheme.SmallFontSize, UITemporaryTheme.TextSecondary);

            scaffold.ContentArea.parent.gameObject.SetActive(false);

            Bind(set, "_summaryText", summaryTxt);
            Bind(set, "_detailText",  scaffold.DetailText);

            var sndBtn  = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Sound",        "Toggle Sound",        false, set.OnClickToggleSound);
            var musBtn  = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Music",        "Toggle Music",        false, set.OnClickToggleMusic);
            var vibBtn  = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Vibration",    "Toggle Vibration",    false, set.OnClickToggleVibration);
            var notBtn  = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Notifications","Toggle Notifications",false, set.OnClickToggleNotifications);
            var saveBtn = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Save",         "Save Settings",       true,  set.OnClickSave);
            var rstBtn  = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_Reset",        "Reset Save Data",     false, set.OnClickReset);
            var conBtn  = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_ConfirmReset", "Yes, Reset",          false, set.OnClickConfirmReset);
            var canBtn  = UIScreenLayoutBuilder.AddActionButton(scaffold.ActionBar, "Btn_CancelReset",  "No, Cancel",          false, set.OnClickCancelReset);

            conBtn.gameObject.SetActive(false);
            canBtn.gameObject.SetActive(false);

            Bind(set, "_toggleSoundButton",         sndBtn);
            Bind(set, "_toggleMusicButton",         musBtn);
            Bind(set, "_toggleVibrationButton",     vibBtn);
            Bind(set, "_toggleNotificationsButton", notBtn);
            Bind(set, "_saveButton",                saveBtn);
            Bind(set, "_resetButton",               rstBtn);
            Bind(set, "_confirmResetButton",        conBtn);
            Bind(set, "_cancelResetButton",         canBtn);

            var feedbackTxt = UICardFactory.CreateFeedbackLabel(scaffold.DetailPanel, "FeedbackText");
            Bind(set, "_feedbackText", feedbackTxt);
        }

        private static void SetupHUD(Transform uiRoot)
        {
            var safeArea = uiRoot.Find("UICanvas/SafeArea");
            if (safeArea == null) return;

            var hudGo = safeArea.Find("HUDVisual");
            if (hudGo == null) return;

            var controller = safeArea.GetComponent<HUDController>() ?? safeArea.gameObject.AddComponent<HUDController>();

            Bind(controller, "_moneyText",  FindTextDeep(hudGo, "MoneyText"));
            Bind(controller, "_guildText",  FindTextDeep(hudGo, "GuildLevelText"));

            Bind(controller, "_tavernButton",    FindButtonDeep(hudGo, "Btn_Tavern"));
            Bind(controller, "_characterButton", FindButtonDeep(hudGo, "Btn_Character"));
            Bind(controller, "_inventoryButton", FindButtonDeep(hudGo, "Btn_Inventory"));
            Bind(controller, "_craftButton",     FindButtonDeep(hudGo, "Btn_Craft"));
            Bind(controller, "_merchantButton",  FindButtonDeep(hudGo, "Btn_Merchant"));
            Bind(controller, "_dungeonButton",   FindButtonDeep(hudGo, "Btn_Dungeon"));
            Bind(controller, "_questButton",     FindButtonDeep(hudGo, "Btn_Quest"));
            Bind(controller, "_settingsButton",  FindButtonDeep(hudGo, "Btn_Settings"));
        }



        private static Text FindTextDeep(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t != null) return t.GetComponent<Text>();
            for (int i = 0; i < parent.childCount; i++)
            {
                var res = FindTextDeep(parent.GetChild(i), name);
                if (res != null) return res;
            }
            return null;
        }

        private static Button FindButtonDeep(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t != null) return t.GetComponent<Button>();
            for (int i = 0; i < parent.childCount; i++)
            {
                var res = FindButtonDeep(parent.GetChild(i), name);
                if (res != null) return res;
            }
            return null;
        }
    }
}
#endif
