#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Events;
using UnityEngine.Events;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.UI.Tavern;
using GuildMaster.Runtime.UI.Character;
using GuildMaster.Runtime.UI.Inventory;
using GuildMaster.Runtime.UI.Craft;
using GuildMaster.Runtime.UI.Merchant;
using GuildMaster.Runtime.UI.Dungeon;
using GuildMaster.Runtime.UI.Quest;
using GuildMaster.Runtime.UI.Settings;
using GuildMaster.Runtime.UI;

namespace GuildMaster.Editor.UI
{
    public static class UIPlaceholderGenerator
    {
        [InitializeOnLoadMethod]
        private static void RunOnceOnCompile()
        {
            if (EditorPrefs.GetBool("S6_5A_Generated", false) == false)
            {
                EditorPrefs.SetBool("S6_5A_Generated", true);
                EditorApplication.delayCall += GeneratePlaceholders;
            }
        }

        [MenuItem("GuildMaster/Wire UI Functional Placeholders (S6.5A)")]
        public static void GeneratePlaceholders()
        {
            var canvas = GameObject.Find("UICanvas");
            if (canvas == null)
            {
                Debug.LogError("[UIPlaceholder] UICanvas not found.");
                return;
            }

            Transform screenRoot = canvas.transform.Find("SafeArea/ScreenRoot");
            if (screenRoot == null) return;

            // 1. Tavern
            var tav = CreateScreen<TavernScreen>(screenRoot, "TavernScreen", UIScreenId.Tavern);
            WireButton(tav.transform, "Btn_SpawnGuest", "Spawn Guest (+1h)", new Vector2(0, 300), tav.OnClickSpawnGuest);
            WireButton(tav.transform, "Btn_Recruit0", "Recruit [0]", new Vector2(0, 150), tav.OnClickRecruitFirst);

            // 2. Character
            var chr = CreateScreen<CharacterScreen>(screenRoot, "CharacterScreen", UIScreenId.Character);
            WireButton(chr.transform, "Btn_Equip", "Equip First Item", new Vector2(-200, 300), chr.OnClickEquipFirstItemToFirstCharacter);
            WireButton(chr.transform, "Btn_Unequip", "Unequip All", new Vector2(200, 300), chr.OnClickUnequipFirstCharacter);

            // 3. Inventory
            var inv = CreateScreen<InventoryScreen>(screenRoot, "InventoryScreen", UIScreenId.Inventory);
            WireButton(inv.transform, "Btn_Lock", "Toggle Lock [0]", new Vector2(-200, 300), inv.OnClickToggleLockFirst);
            WireButton(inv.transform, "Btn_Use", "Use Consumable [0]", new Vector2(200, 300), inv.OnClickUseFirstConsumable);

            // 4. Craft
            var crf = CreateScreen<CraftScreen>(screenRoot, "CraftScreen", UIScreenId.Craft);
            WireButton(crf.transform, "Btn_Craft1", "Craft First Recipe", new Vector2(0, 450), crf.OnClickCraftFirstAvailable);
            WireButton(crf.transform, "Btn_Progress", "Progress Time (+1h)", new Vector2(0, 300), crf.OnClickProgressTime);
            WireButton(crf.transform, "Btn_Claim", "Claim Completed [0]", new Vector2(0, 150), crf.OnClickClaimFirst);

            // 5. Merchant
            var mer = CreateScreen<MerchantScreen>(screenRoot, "MerchantScreen", UIScreenId.Merchant);
            WireButton(mer.transform, "Btn_Buy", "Buy Regular [0]", new Vector2(0, 450), mer.OnClickBuyFirstRegular);
            WireButton(mer.transform, "Btn_Sell", "Sell Item [0]", new Vector2(0, 300), mer.OnClickSellFirstItem);
            WireButton(mer.transform, "Btn_Progress", "Progress Time (+1h)", new Vector2(0, 150), mer.OnClickProgressTime);

            // 6. Dungeon
            var dun = CreateScreen<DungeonScreen>(screenRoot, "DungeonScreen", UIScreenId.Dungeon);
            WireButton(dun.transform, "Btn_Start", "Start First Dungeon", new Vector2(0, 450), dun.OnClickStartFirst);
            WireButton(dun.transform, "Btn_Tick1", "Tick 1", new Vector2(-200, 300), dun.OnClickTick1);
            WireButton(dun.transform, "Btn_Tick10", "Tick 10", new Vector2(200, 300), dun.OnClickTick10);
            WireButton(dun.transform, "Btn_Collect", "Collect Loot", new Vector2(0, 150), dun.OnClickCollectLoot);

            // 7. Quest
            var que = CreateScreen<QuestScreen>(screenRoot, "QuestScreen", UIScreenId.Quest);
            WireButton(que.transform, "Btn_Claim", "Claim First Quest", new Vector2(0, 300), que.OnClickClaimFirst);
            WireButton(que.transform, "Btn_TestInc", "Test Increment (+1)", new Vector2(0, 150), que.OnClickTestIncrement);

            // 8. Settings
            var set = CreateScreen<SettingsScreen>(screenRoot, "SettingsScreen", UIScreenId.Settings);
            WireButton(set.transform, "Btn_Snd", "Toggle Sound", new Vector2(0, 450), set.OnClickToggleSound);
            WireButton(set.transform, "Btn_Save", "Save State", new Vector2(0, 300), set.OnClickSave);
            WireButton(set.transform, "Btn_Reset", "Reset Default", new Vector2(0, 150), set.OnClickReset);

            // Save Scene
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log("[UIPlaceholderGenerator] Functional Placeholders Wired Successfully!");
        }

        private static T CreateScreen<T>(Transform parent, string name, UIScreenId id) where T : UIScreen
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

            if (go.GetComponent<RectTransform>() == null) go.AddComponent<RectTransform>();
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(900, 1400);

            var panel = go.GetComponent<Image>();
            if (panel == null) panel = go.AddComponent<Image>();
            panel.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark background

            T comp = go.GetComponent<T>();
            if (comp == null) comp = go.AddComponent<T>();
            comp.ScreenId = id;

            // Ensure Back button exists
            var backBtn = CreateButton(go.transform, "Btn_Back", "Back", new Vector2(0, -600));

            // Ensure Status text exists
            Transform txt = go.transform.Find("ListContent");
            if (txt == null) txt = go.transform.Find("StatusText");
            if (txt == null)
            {
                var textGo = new GameObject("ListContent");
                textGo.transform.SetParent(go.transform, false);
                var textComp = textGo.AddComponent<Text>();
                textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                textComp.fontSize = 24;
                textComp.color = Color.white;
                textComp.alignment = TextAnchor.UpperLeft;
                var trt = textGo.GetComponent<RectTransform>();
                trt.anchoredPosition = new Vector2(0, -100);
                trt.sizeDelta = new Vector2(800, 600);
                
                // Bind to serialized field based on type
                var so = new SerializedObject(comp);
                var prop = so.FindProperty("_statusText");
                if (prop == null) prop = so.FindProperty("_inventoryText");
                if (prop == null) prop = so.FindProperty("_characterText");
                
                if (prop != null)
                {
                    prop.objectReferenceValue = textComp;
                    so.ApplyModifiedProperties();
                }
            }
            else
            {
                // Unhide text if it was hidden
                txt.gameObject.SetActive(true);
            }

            return comp;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 pos)
        {
            Transform t = parent.Find(name);
            GameObject go;
            if (t == null)
            {
                go = new GameObject(name);
                go.transform.SetParent(parent, false);
                var rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(350, 80);
                
                var img = go.AddComponent<Image>();
                img.color = Color.gray;
                
                var b = go.AddComponent<Button>();
                b.targetGraphic = img;

                var textGo = new GameObject("Text");
                textGo.transform.SetParent(go.transform, false);
                var txt = textGo.AddComponent<Text>();
                txt.text = label;
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 28;
                txt.color = Color.black;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 80);
            }
            
            var btn = go.GetComponent<Button>();
            btn.GetComponent<RectTransform>().anchoredPosition = pos;
            return btn;
        }

        private static void WireButton(Transform parent, string name, string label, Vector2 pos, UnityAction action)
        {
            var btn = CreateButton(parent, name, label, pos);
            // Clear existing persistent events to avoid duplicates
            while (btn.onClick.GetPersistentEventCount() > 0)
            {
                UnityEventTools.RemovePersistentListener(btn.onClick, 0);
            }
            UnityEventTools.AddPersistentListener(btn.onClick, action);
        }
    }
}
#endif
