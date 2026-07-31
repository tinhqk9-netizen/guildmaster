#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.UI.HUD;
using GuildMaster.Runtime.UI.Inventory;
using GuildMaster.Runtime.UI.Character;
using GuildMaster.Runtime.UI.Popup;
using GuildMaster.Runtime.UI;
using GuildMaster.Runtime.Assets;
using GuildMaster.Runtime.Boot;

namespace GuildMaster.Editor.UI
{
    public static class UIWiringGenerator
    {
        private const string CatalogPath = "Assets/_Game/Data/AssetCatalog.asset";

        /// <summary>
        /// Step 1 of the unified apply pipeline (see GuildMasterUnifiedApply.Apply). Not a menu item on
        /// its own anymore — running it in isolation would leave the card containers and Settings
        /// confirm/cancel buttons unwired, so the single supported entrypoint is
        /// "GuildMaster/UI/Apply Complete Functional Game UI".
        /// </summary>
        public static void WireScene()
        {
            var canvas = GameObject.Find("UICanvas");
            if (canvas == null)
            {
                Debug.LogError("[UIWiring] UICanvas not found. Please generate foundation first.");
                return;
            }

            // Optional visual catalog — when present, HUD gets real currency icons and the
            // nav buttons get pack icons. When absent, the UI still wires up as plain text/gray.
            var catalog = AssetDatabase.LoadAssetAtPath<AssetCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogWarning("[UIWiring] AssetCatalog not found — wiring UI without sprite icons. "
                                 + "Run 'GuildMaster/Assets/Build Asset Catalog' first for real icons.");
            }

            Transform hudRoot = canvas.transform.Find("SafeArea/HudRoot");
            Transform screenRoot = canvas.transform.Find("SafeArea/ScreenRoot");
            Transform popupRoot = canvas.transform.Find("SafeArea/PopupRoot");

            if (hudRoot == null || screenRoot == null || popupRoot == null)
            {
                Debug.LogError("[UIWiring] Roots not found inside SafeArea.");
                return;
            }

            // 1. UIManager - Removed because UIService is not a MonoBehaviour and cannot be attached.
            // Runtime Bootstrapper will instantiate UIService.

            // 2. HUD
            var hud = CreateOrGetComponent<HUDController>(hudRoot, "HUDVisual");
            hud.ScreenId = UIScreenId.MainHUD;
            var moneyText = CreateText(hud.transform, "MoneyText", new Vector2(-200, 800));
            var gemsText = CreateText(hud.transform, "GemsText", new Vector2(200, 800));

            // Currency icons beside the (real) money/gems values — decorative, no logic change.
            if (catalog != null)
            {
                CreateIcon(hud.transform, "MoneyIcon", catalog.coinIcon, new Vector2(-310, 800), 64f);
                CreateIcon(hud.transform, "GemsIcon", catalog.gemIcon, new Vector2(90, 800), 64f);
            }

            var tavernBtn = CreateButton(hud.transform, "Btn_Tavern", "Tavern", new Vector2(0, 750));
            var invBtn = CreateButton(hud.transform, "Btn_Inventory", "Inventory", new Vector2(0, 600));
            var charBtn = CreateButton(hud.transform, "Btn_Character", "Character", new Vector2(0, 450));
            var popupBtn = CreateButton(hud.transform, "Btn_Dungeon", "Dungeon", new Vector2(0, 300));
            var craftBtn = CreateButton(hud.transform, "Btn_Craft", "Craft", new Vector2(0, 150));
            var merchantBtn = CreateButton(hud.transform, "Btn_Merchant", "Merchant", new Vector2(0, 0));
            var questBtn = CreateButton(hud.transform, "Btn_Quest", "Quest", new Vector2(0, -150));
            var settingsBtn = CreateButton(hud.transform, "Btn_Settings", "Settings", new Vector2(0, -300));

            // Real pack icons on the nav buttons (left-aligned), keeping the runtime text label.
            if (catalog != null)
            {
                AddNavIcon(invBtn, catalog.Nav("Inventory"));
                AddNavIcon(charBtn, catalog.Nav("Character"));
                AddNavIcon(popupBtn, catalog.Nav("Dungeon"));
                AddNavIcon(craftBtn, catalog.Nav("Craft"));
                AddNavIcon(merchantBtn, catalog.Nav("Merchant"));
                AddNavIcon(settingsBtn, catalog.Nav("Settings"));
            }

            var hudSO = new SerializedObject(hud);
            hudSO.FindProperty("_moneyText").objectReferenceValue = moneyText;
            hudSO.FindProperty("_gemsText").objectReferenceValue = gemsText;
            hudSO.FindProperty("_inventoryButton").objectReferenceValue = invBtn;
            hudSO.FindProperty("_characterButton").objectReferenceValue = charBtn;
            hudSO.FindProperty("_dungeonButton").objectReferenceValue = popupBtn;
            hudSO.FindProperty("_craftButton").objectReferenceValue = craftBtn;
            hudSO.FindProperty("_merchantButton").objectReferenceValue = merchantBtn;
            hudSO.FindProperty("_settingsButton").objectReferenceValue = settingsBtn;
            hudSO.FindProperty("_tavernButton").objectReferenceValue = tavernBtn;
            hudSO.FindProperty("_questButton").objectReferenceValue = questBtn;
            hudSO.ApplyModifiedProperties();

            // 3. Inventory — component acquired here so SetActive(false) below works.
            //    All child creation and field wiring are handled by
            //    GuildMasterUnifiedApply.BuildInventory() (called after WireScene).
            var inv = CreateOrGetComponent<InventoryScreen>(screenRoot, "InventoryScreen");
            inv.ScreenId = UIScreenId.Inventory;

            // 4. Character — same: component only, no legacy text/button creation.
            //    All child creation and field wiring are handled by
            //    GuildMasterUnifiedApply.BuildCharacter() (called after WireScene).
            var chr = CreateOrGetComponent<CharacterScreen>(screenRoot, "CharacterScreen");
            chr.ScreenId = UIScreenId.Character;


            // 5. Popup
            var pop = CreateOrGetComponent<PopupScreen>(popupRoot, "PopupScreen");
            pop.ScreenId = UIScreenId.None;
            pop.IsPopup = true;
            var pTitle = CreateText(pop.transform, "Title", new Vector2(0, 200));
            var pMsg = CreateText(pop.transform, "Message", new Vector2(0, 0));
            pMsg.rectTransform.sizeDelta = new Vector2(600, 300);
            var pOkBtn = CreateButton(pop.transform, "Btn_OK", "OK", new Vector2(0, -200));
            var popSO = new SerializedObject(pop);
            popSO.FindProperty("_titleText").objectReferenceValue = pTitle;
            popSO.FindProperty("_messageText").objectReferenceValue = pMsg;
            popSO.FindProperty("_okButton").objectReferenceValue = pOkBtn;
            popSO.ApplyModifiedProperties();

            // 6. Dungeon, Craft, Merchant, Settings (and Tavern, Character, Inventory, Quest) get their
            //    real functional screens + card containers from GuildMaster/UI/Apply Complete Functional
            //    Game UI, which calls this method as step 1. No "not implemented yet" dead panels here.

            // 7. Runtime composition root — wires UIService + screens at play time (S5 minimal).
            var composerGo = GameObject.Find("UIRuntimeBootstrap");
            if (composerGo == null) composerGo = new GameObject("UIRuntimeBootstrap");
            if (composerGo.GetComponent<UIRuntimeBootstrap>() == null) composerGo.AddComponent<UIRuntimeBootstrap>();

            // Hide them by default except HUD
            inv.gameObject.SetActive(false);
            chr.gameObject.SetActive(false);
            pop.gameObject.SetActive(false);

            // Save Scene
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log("[UIWiring] Scene Wired and Saved successfully.");
        }

        private static T CreateOrGetComponent<T>(Transform parent, string name) where T : Component
        {
            Transform t = parent.Find(name);
            GameObject go;
            if (t == null)
            {
                go = new GameObject(name);
                go.transform.SetParent(parent, false);
            }
            else
            {
                go = t.gameObject;
            }

            // Ensure RectTransform
            if (go.GetComponent<RectTransform>() == null)
            {
                go.AddComponent<RectTransform>();
            }

            T comp = go.GetComponent<T>();
            if (comp == null) comp = go.AddComponent<T>();
            return comp;
        }

        /// <summary>A standalone sprite icon (e.g. currency icon on the HUD).</summary>
        private static Image CreateIcon(Transform parent, string name, Sprite sprite, Vector2 pos, float size)
        {
            var img = CreateOrGetComponent<Image>(parent, name);
            img.rectTransform.anchoredPosition = pos;
            img.rectTransform.sizeDelta = new Vector2(size, size);
            img.sprite = sprite;
            img.preserveAspect = true;
            img.enabled = sprite != null; // avoid a white box when the sprite is missing
            return img;
        }

        /// <summary>Adds a left-aligned pack icon inside a nav button, keeping the text label.</summary>
        private static void AddNavIcon(Button button, Sprite sprite)
        {
            if (button == null || sprite == null) return;
            var icon = CreateIcon(button.transform, "Icon", sprite, new Vector2(-150, 0), 72f);
            icon.raycastTarget = false; // let clicks pass through to the button
        }

        private static Text CreateText(Transform parent, string name, Vector2 pos)
        {
            var t = CreateOrGetComponent<Text>(parent, name);
            t.rectTransform.anchoredPosition = pos;
            t.rectTransform.sizeDelta = new Vector2(300, 100);
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 32;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            return t;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 pos)
        {
            var b = CreateOrGetComponent<Button>(parent, name);
            b.GetComponent<RectTransform>().anchoredPosition = pos;
            b.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100); // Mobile sized
            
            var img = b.gameObject.GetComponent<Image>();
            if (img == null) img = b.gameObject.AddComponent<Image>();
            img.color = Color.gray;
            b.targetGraphic = img;

            var txt = CreateText(b.transform, "Text", Vector2.zero);
            txt.text = label;
            txt.color = Color.black;
            txt.rectTransform.sizeDelta = new Vector2(400, 100);

            return b;
        }
    }
}
#endif
