using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Dungeon;
using GuildMaster.Runtime.UI.Character;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.Boot;
using GuildMaster.Runtime.UI.Tavern;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Database;

namespace GuildMaster.Editor
{
    public static class SetupMultiDungeonUI
    {
        // [MenuItem("GuildMaster/Setup Multi-Dungeon UI (3 Slots)")] — DISABLED: UI is manually edited.
        public static void RunSetup()
        {
            FixDungeonUILayout();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Fix Dungeon UI Layout — fixes all RectTransforms/Layouts & creates Preview Cards
        // ─────────────────────────────────────────────────────────────────────────────

        // [MenuItem("GuildMaster/Fix Dungeon UI Layout")] — DISABLED: UI is manually edited.
        public static void FixDungeonUILayout()
        {
            if (EditorApplication.isPlaying || Application.isPlaying)
            {
                Debug.LogWarning("[FixDungeonUILayout] Cannot run in Play Mode!");
                return;
            }

            DungeonScreen screen = UnityEngine.Object.FindAnyObjectByType<DungeonScreen>(FindObjectsInactive.Include);
            if (screen == null)
            {
                Debug.LogError("[FixDungeonUILayout] DungeonScreen not found!");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(screen.gameObject, "Fix Dungeon UI Layout");

            Transform t = screen.transform;

            // ── 0) Add stylish background to DungeonScreen if missing ───────────────
            EnsureImage(screen.gameObject, new Color(0.10f, 0.12f, 0.16f, 0.98f));

            // ── 1) SlotBarContainer: top bar for Slot 1/2/3 tabs ─────────────────
            Transform slotBar = t.Find("SlotBarContainer");
            if (slotBar == null)
            {
                var slotBarGo = new GameObject("SlotBarContainer", typeof(RectTransform));
                slotBarGo.transform.SetParent(t, false);
                slotBar = slotBarGo.transform;
            }

            // Put Header at top (sibling 0), SlotBar at sibling 1
            Transform header = t.Find("Header");
            if (header != null) header.SetSiblingIndex(0);
            slotBar.SetSiblingIndex(1);

            var slotBarRT = slotBar.GetComponent<RectTransform>();
            slotBarRT.anchorMin = new Vector2(0.02f, 0.865f);
            slotBarRT.anchorMax = new Vector2(0.98f, 0.930f);
            slotBarRT.offsetMin = Vector2.zero;
            slotBarRT.offsetMax = Vector2.zero;

            EnsureHLG(slotBar.gameObject, spacing: 8f);
            EnsureImage(slotBar.gameObject, new Color(0.08f, 0.08f, 0.12f, 0.90f));

            // Populate preview tabs for Edit Mode
            PopulatePreviewTabs(slotBar);

            // Bind _slotBarContainer property
            var so = new SerializedObject(screen);
            var slotBarProp = so.FindProperty("_slotBarContainer");
            if (slotBarProp != null) slotBarProp.objectReferenceValue = slotBarRT;

            // ── 2) ContentScroll: fill middle of screen ───────────────────────────
            Transform contentScroll = t.Find("ContentScroll");
            if (contentScroll != null)
            {
                var rt = contentScroll.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.02f, 0.14f);
                rt.anchorMax = new Vector2(0.98f, 0.855f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                EnsureImage(contentScroll.gameObject, new Color(0.14f, 0.15f, 0.19f, 0.80f));

                // ── Viewport ──────────────────────────────────────────────────────
                Transform viewport = contentScroll.Find("Viewport");
                if (viewport != null)
                {
                    var vrt = viewport.GetComponent<RectTransform>();
                    vrt.anchorMin = Vector2.zero;
                    vrt.anchorMax = Vector2.one;
                    vrt.offsetMin = Vector2.zero;
                    vrt.offsetMax = Vector2.zero;

                    // Ensure Mask / RectMask2D so scrolling clips nicely
                    var mask = viewport.GetComponent<Mask>();
                    if (mask != null) mask.showMaskGraphic = false;

                    // ── Content inside Viewport ────────────────────────────────────
                    Transform content = viewport.Find("Content");
                    if (content != null)
                    {
                        var crt = content.GetComponent<RectTransform>();
                        crt.anchorMin = new Vector2(0f, 1f);
                        crt.anchorMax = new Vector2(1f, 1f);
                        crt.pivot     = new Vector2(0.5f, 1f);
                        crt.offsetMin = Vector2.zero;
                        crt.offsetMax = Vector2.zero;

                        EnsureVLG(content.gameObject, spacing: 0f, controlHeight: true, expandWidth: true);
                        EnsureCSF(content.gameObject);

                        // ── Panel_Select ───────────────────────────────────────────
                        FixPanel(content.Find("Panel_Select"), "Panel_Select");

                        // ── Panel_Active ───────────────────────────────────────────
                        FixPanel(content.Find("Panel_Active"), "Panel_Active");

                        // ── Panel_Loot ─────────────────────────────────────────────
                        FixPanel(content.Find("Panel_Loot"), "Panel_Loot");
                    }
                }
            }

            // ── 3) Fix card containers + create Edit Mode Preview Cards ──────────
            Transform dungeonCardContainer = FindDeep(t, "DungeonCardContainer");
            FixCardContainer(dungeonCardContainer, "DungeonCardContainer");
            PopulatePreviewDungeonCards(dungeonCardContainer);

            Transform combatCardContainer = FindDeep(t, "CombatCardContainer");
            FixCardContainer(combatCardContainer, "CombatCardContainer");

            Transform lootCardContainer = FindDeep(t, "LootCardContainer");
            FixCardContainer(lootCardContainer, "LootCardContainer");

            // ── 4) ActionBar: bottom row of buttons ───────────────────────────────
            Transform actionBar = t.Find("ActionBar");
            if (actionBar != null)
            {
                var rt = actionBar.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.02f, 0.01f);
                rt.anchorMax = new Vector2(0.98f, 0.13f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                EnsureHLG(actionBar.gameObject, spacing: 10f);
                EnsureImage(actionBar.gameObject, new Color(0.12f, 0.13f, 0.17f, 0.90f));

                // Style Start Button
                Transform btnStart = actionBar.Find("Btn_Start");
                if (btnStart != null)
                {
                    var img = btnStart.GetComponent<Image>() ?? btnStart.gameObject.AddComponent<Image>();
                    img.color = new Color(0.20f, 0.65f, 0.35f, 1f); // Vibrant Green
                    var txt = btnStart.GetComponentInChildren<Text>();
                    if (txt != null)
                    {
                        txt.text = "Start Expedition ⚔️";
                        txt.color = Color.white;
                        txt.fontSize = 22;
                        txt.alignment = TextAnchor.MiddleCenter;
                    }
                }

                // Style Collect Loot Button
                Transform btnCollect = actionBar.Find("Btn_Collect");
                if (btnCollect != null)
                {
                    var img = btnCollect.GetComponent<Image>() ?? btnCollect.gameObject.AddComponent<Image>();
                    img.color = new Color(0.85f, 0.60f, 0.15f, 1f); // Vibrant Gold
                    var txt = btnCollect.GetComponentInChildren<Text>();
                    if (txt != null)
                    {
                        txt.text = "Collect Loot 🎒";
                        txt.color = Color.white;
                        txt.fontSize = 22;
                        txt.alignment = TextAnchor.MiddleCenter;
                    }
                }

                HideChildButton(actionBar, "Btn_Continue");
                HideChildButton(actionBar, "Btn_AutoBattle");
                HideChildButton(actionBar, "Btn_PrevDungeon");
                HideChildButton(actionBar, "Btn_NextDungeon");
            }

            // ── 5) Header: top title bar ──────────────────────────────────────────
            if (header != null)
            {
                var rt = header.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0.935f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                EnsureImage(header.gameObject, new Color(0.12f, 0.14f, 0.18f, 1f));
            }

            // ── 6) Style text labels in Panel_Select ──────────────────────────────
            Transform selectedDungeonTxt = FindDeep(t, "SelectedDungeonText");
            if (selectedDungeonTxt != null)
            {
                var txt = selectedDungeonTxt.GetComponent<Text>();
                if (txt != null)
                {
                    txt.fontSize = 22;
                    txt.color = new Color(1.00f, 0.85f, 0.30f, 1f); // Gold highlight
                    txt.text = "🎯 Selected: 🌵 Barren Wastelands";
                }
            }

            Transform partyTxt = FindDeep(t, "PartyText");
            if (partyTxt != null)
            {
                var txt = partyTxt.GetComponent<Text>();
                if (txt != null)
                {
                    txt.fontSize = 20;
                    txt.color = new Color(0.85f, 0.88f, 0.95f, 1f);
                    txt.text = "🛡️ Đội 1 (4/4 người): Hero A, Hero B, Hero C, Hero D";
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(screen.gameObject);
            if (screen.gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(screen.gameObject.scene);

            Debug.Log("[FixDungeonUILayout] ✅ Dungeon UI Layout & Edit-Mode Preview Cards successfully generated!");
        }

        // ── Preview Generation (Edit Mode Placeholders) ───────────────────────────

        private static void PopulatePreviewTabs(Transform container)
        {
            if (container == null) return;
            // Clear old previews
            for (int i = container.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(container.GetChild(i).gameObject);

            string[] tabNames = new string[] { "⚔️ Đội 1 (Rảnh)", "Đội 2 (Rảnh)", "Đội 3 (Rảnh)" };
            for (int i = 0; i < 3; i++)
            {
                var btnGo = new GameObject($"PreviewTab_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                btnGo.transform.SetParent(container, false);
                var img = btnGo.GetComponent<Image>();
                img.color = (i == 0) ? new Color(0.25f, 0.55f, 0.95f, 1f) : new Color(0.22f, 0.23f, 0.28f, 0.8f);

                var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
                txtGo.transform.SetParent(btnGo.transform, false);
                var txtRt = txtGo.GetComponent<RectTransform>();
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.offsetMin = Vector2.zero;
                txtRt.offsetMax = Vector2.zero;

                var txt = txtGo.GetComponent<Text>();
                txt.text = tabNames[i];
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 20;
                txt.color = Color.white;
                txt.alignment = TextAnchor.MiddleCenter;
            }
        }

        private static void PopulatePreviewDungeonCards(Transform container)
        {
            if (container == null) return;
            // Only add previews if container is currently empty
            if (container.childCount > 0) return;

            (string title, string sub, bool selected)[] sampleDungeons = new[]
            {
                ("🌵 Barren Wastelands", "Quái vật: 4 chủng loại • Khuyên dùng Lv.1", true),
                ("⚓ Blackwater Port", "Quái vật: 6 chủng loại • Khuyên dùng Lv.5", false),
                ("🌲 Enchanted Forest", "Quái vật: 5 chủng loại • Khuyên dùng Lv.10", false)
            };

            foreach (var item in sampleDungeons)
            {
                var cardGo = new GameObject("PreviewCard_" + item.title, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                cardGo.transform.SetParent(container, false);

                var le = cardGo.GetComponent<LayoutElement>();
                le.preferredHeight = 70f;
                le.minHeight = 70f;
                le.flexibleWidth = 1f;

                var img = cardGo.GetComponent<Image>();
                img.color = item.selected
                    ? new Color(0.20f, 0.45f, 0.75f, 1f)  // Selected blue
                    : new Color(0.18f, 0.22f, 0.30f, 0.9f); // Normal dark card

                // Layout inside card
                var hlg = cardGo.AddComponent<HorizontalLayoutGroup>();
                hlg.padding = new RectOffset(16, 16, 10, 10);
                hlg.spacing = 12f;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;

                // Text column
                var colGo = new GameObject("TextCol", typeof(RectTransform), typeof(VerticalLayoutGroup));
                colGo.transform.SetParent(cardGo.transform, false);
                var colLe = colGo.AddComponent<LayoutElement>();
                colLe.flexibleWidth = 1f;

                var vlg = colGo.GetComponent<VerticalLayoutGroup>();
                vlg.spacing = 4f;
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;

                // Title
                var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
                titleGo.transform.SetParent(colGo.transform, false);
                var titleTxt = titleGo.GetComponent<Text>();
                titleTxt.text = item.title;
                titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                titleTxt.fontSize = 20;
                titleTxt.color = item.selected ? new Color(1.00f, 0.88f, 0.30f, 1f) : Color.white;

                // Subtitle
                var subGo = new GameObject("Sub", typeof(RectTransform), typeof(Text));
                subGo.transform.SetParent(colGo.transform, false);
                var subTxt = subGo.GetComponent<Text>();
                subTxt.text = item.sub;
                subTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                subTxt.fontSize = 16;
                subTxt.color = new Color(0.75f, 0.78f, 0.85f, 1f);
            }
        }

        // ── Layout fix helpers ────────────────────────────────────────────────────

        private static void FixPanel(Transform panel, string name)
        {
            if (panel == null) { Debug.LogWarning($"[FixDungeonUILayout] {name} not found!"); return; }

            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            EnsureVLG(panel.gameObject, spacing: 10f, controlHeight: true, expandWidth: true);
            EnsureCSF(panel.gameObject);
        }

        private static void FixCardContainer(Transform container, string name)
        {
            if (container == null) { Debug.LogWarning($"[FixDungeonUILayout] {name} not found!"); return; }

            var rt = container.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var grid = container.GetComponent<GridLayoutGroup>();
            if (grid != null) UnityEngine.Object.DestroyImmediate(grid);

            var le = container.GetComponent<LayoutElement>();
            if (le != null) UnityEngine.Object.DestroyImmediate(le);

            EnsureVLG(container.gameObject, spacing: 8f, controlHeight: true, expandWidth: true);
            EnsureCSF(container.gameObject);
        }

        private static void HideChildButton(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var result = FindDeep(root.GetChild(i), name);
                if (result != null) return result;
            }
            return null;
        }

        private static void EnsureVLG(GameObject go, float spacing, bool controlHeight, bool expandWidth)
        {
            var grid = go.GetComponent<GridLayoutGroup>();
            if (grid != null) UnityEngine.Object.DestroyImmediate(grid);
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null) UnityEngine.Object.DestroyImmediate(hlg);

            var vlg = go.GetComponent<VerticalLayoutGroup>() ?? go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing                = spacing;
            vlg.childForceExpandWidth  = expandWidth;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = controlHeight;
            vlg.childAlignment         = TextAnchor.UpperLeft;
        }

        private static void EnsureHLG(GameObject go, float spacing)
        {
            var hlg = go.GetComponent<HorizontalLayoutGroup>() ?? go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing              = spacing;
            hlg.childForceExpandWidth  = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth    = true;
            hlg.childControlHeight   = true;
            hlg.childAlignment       = TextAnchor.MiddleCenter;
        }

        private static void EnsureCSF(GameObject go)
        {
            var csf = go.GetComponent<ContentSizeFitter>() ?? go.AddComponent<ContentSizeFitter>();
            csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        private static void EnsureImage(GameObject go, Color color)
        {
            var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
            img.color = color;
        }

        [MenuItem("GuildMaster/Spawn Hero to Tavern Now")]
        public static void SpawnHeroNow()
        {
            if (Application.isPlaying)
            {
                var bootstrap = UnityEngine.Object.FindAnyObjectByType<UIRuntimeBootstrap>();
                if (bootstrap == null || bootstrap.Services == null || bootstrap.Services.Tavern == null)
                {
                    Debug.LogError("[SpawnHeroToTavernTool] Could not find active UIRuntimeBootstrap or Services in Play Mode!");
                    return;
                }

                var tavern = bootstrap.Services.Tavern;
                var save = bootstrap.Services.Save;

                tavern.GenerateVisitor();
                save.Save(out Exception error);

                var tavernScreen = UnityEngine.Object.FindAnyObjectByType<TavernScreen>(FindObjectsInactive.Include);
                if (tavernScreen != null && tavernScreen.gameObject.activeInHierarchy)
                {
                    tavernScreen.Refresh();
                }

                Debug.Log("[SpawnHeroToTavernTool] ✅ A new Hero has arrived at the Tavern!");
            }
            else
            {
                try
                {
                    var dataProvider = new EditorExternalGameDataProvider();
                    var serializer = new UnityJsonSerializer();
                    var database = new GameDatabase();
                    var builder = new DatabaseBuilder(dataProvider, serializer, database);
                    builder.Build();

                    var container = new ServiceContainer(database);
                    container.Tavern.GenerateVisitor();
                    container.Save.Save(out Exception error);

                    Debug.Log("[SpawnHeroToTavernTool] ✅ A new Hero has been added to Tavern save data!");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SpawnHeroToTavernTool] Failed to spawn hero: {ex.Message}");
                }
            }
        }
    }
}
