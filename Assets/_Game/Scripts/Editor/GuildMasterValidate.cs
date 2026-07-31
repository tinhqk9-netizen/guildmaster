#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.UI.HUD;
using GuildMaster.Runtime.UI.Tavern;
using GuildMaster.Runtime.UI.Character;
using GuildMaster.Runtime.UI.Inventory;
using GuildMaster.Runtime.UI.Craft;
using GuildMaster.Runtime.UI.Merchant;
using GuildMaster.Runtime.UI.Dungeon;
using GuildMaster.Runtime.UI.Quest;
using GuildMaster.Runtime.UI.Settings;
using GuildMaster.Runtime.UI.Popup;

namespace GuildMaster.Editor.UI
{
    /// <summary>Read-only sanity check for the scene produced by GuildMasterUnifiedApply. Never mutates the scene.</summary>
    public static class GuildMasterValidate
    {
        private static readonly string[] LegacyPhrases =
        {
            "is not implemented yet",
            "Dungeon (Deferred)",
            "Advance Time",
            "Tick 1",
            "Tick 10",
            "Fast Forward x10",
            "Toggle Party Membership",
            "Add/Remove Party",
            "First Item",
            "Recruit [0]",
        };

        [MenuItem("GuildMaster/Verification/Validate Applied Game UI")]
        public static void Validate()
        {
            var issues = new List<string>();

            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            if (canvases.Length != 1) issues.Add($"Expected exactly 1 Canvas, found {canvases.Length}.");

            var eventSystems = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
            if (eventSystems.Length != 1) issues.Add($"Expected exactly 1 EventSystem, found {eventSystems.Length}.");

            var hud = Object.FindFirstObjectByType<HUDController>(FindObjectsInactive.Include);
            if (hud == null)
            {
                issues.Add("HUDController not found.");
            }
            else
            {
                var so = new SerializedObject(hud);
                foreach (var f in new[] { "_tavernButton", "_characterButton", "_inventoryButton", "_dungeonButton", "_craftButton", "_merchantButton", "_questButton", "_settingsButton" })
                    CheckButtonRef(so, f, issues);
            }

            CheckScreen<TavernScreen>("TavernScreen", issues, requireCardList: true);
            CheckScreen<CharacterScreen>("CharacterScreen", issues, requireCardList: true);
            CheckScreen<InventoryScreen>("InventoryScreen", issues, requireCardList: true);
            CheckScreen<DungeonScreen>("DungeonScreen", issues, requireCardList: true);
            CheckScreen<CraftScreen>("CraftScreen", issues, requireCardList: true, requireTabBar: true);
            CheckScreen<MerchantScreen>("MerchantScreen", issues, requireCardList: true, requireTabBar: true);
            CheckScreen<QuestScreen>("QuestScreen", issues, requireCardList: true);
            CheckScreen<SettingsScreen>("SettingsScreen", issues, requireCardList: false);

            var popup = Object.FindFirstObjectByType<PopupScreen>(FindObjectsInactive.Include);
            if (popup == null)
            {
                issues.Add("PopupScreen not found.");
            }
            else if (popup.transform.Find("Btn_OK") == null)
            {
                issues.Add("PopupScreen.Btn_OK not found.");
            }

            // Scan every Text component in the whole scene (active or not) for legacy leftovers —
            // this is the check that actually catches orphaned GameObjects the Apply tool forgot to purge.
            foreach (var text in Resources.FindObjectsOfTypeAll<Text>())
            {
                if (text == null || text.gameObject.scene.name == null) continue; // skip prefab assets, keep scene objects only
                if (string.IsNullOrEmpty(text.text)) continue;
                foreach (var phrase in LegacyPhrases)
                {
                    if (text.text.Contains(phrase))
                        issues.Add($"Legacy text \"{phrase}\" still present on {GetPath(text.transform)}.");
                }
            }

            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                foreach (var c in go.GetComponents<Component>())
                {
                    if (c == null) issues.Add($"Missing script on GameObject {GetPath(go.transform)}.");
                }
            }

            if (issues.Count == 0)
            {
                Debug.Log("[GuildMasterValidate] PASS");
            }
            else
            {
                Debug.LogWarning($"[GuildMasterValidate] FAIL\n- {string.Join("\n- ", issues)}");
            }
        }

        private static void CheckButtonRef(SerializedObject so, string field, List<string> issues)
        {
            var prop = so.FindProperty(field);
            if (prop == null) { issues.Add($"HUDController field {field} not found on component."); return; }
            if (prop.objectReferenceValue == null) issues.Add($"HUDController.{field} is null (button not wired).");
        }

        private static void CheckScreen<T>(string name, List<string> issues, bool requireCardList, bool requireTabBar = false)
            where T : UIScreen
        {
            var all = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (all.Length == 0) { issues.Add($"{name}: no component found."); return; }
            if (all.Length > 1) { issues.Add($"{name}: duplicate component found ({all.Length} instances)."); return; }

            Transform root = all[0].transform;
            RequireChild(root, "Header", name, issues);
            RequireChild(root, "DetailPanel", name, issues);
            RequireChild(root, "ActionBar", name, issues);

            if (requireCardList)
            {
                Transform scroll = root.Find("ContentScroll");
                if (scroll == null || scroll.GetComponent<ScrollRect>() == null)
                    issues.Add($"{name}: ContentScroll (with ScrollRect) missing — card list will not render.");
                else if (scroll.Find("Viewport/Content") == null)
                    issues.Add($"{name}: ContentScroll/Viewport/Content missing.");
            }

            if (requireTabBar)
            {
                bool foundTabBar = false;
                for (int i = 0; i < root.childCount; i++)
                {
                    if (root.GetChild(i).name.StartsWith("TabBar_")) { foundTabBar = true; break; }
                }
                if (!foundTabBar) issues.Add($"{name}: TabBar_ child not found — tabs will not render.");
            }

            var backT = root.Find("Header/Btn_Back");
            if (backT == null || backT.GetComponent<Button>() == null)
                issues.Add($"{name}: Header/Btn_Back missing or not a Button.");
        }

        private static void RequireChild(Transform root, string name, string screenName, List<string> issues)
        {
            if (root.Find(name) == null) issues.Add($"{screenName}: missing {name}.");
        }

        private static string GetPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
            return path;
        }
    }
}
#endif
