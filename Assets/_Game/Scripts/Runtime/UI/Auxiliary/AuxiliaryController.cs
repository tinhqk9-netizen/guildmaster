using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Legacy;
using GuildMaster.Runtime.UI.Shell;

namespace GuildMaster.Runtime.UI.Auxiliary
{
    /// <summary>
    /// Phase 9 controller for the App Shell drawer's auxiliary screens (Quests, Bestiary,
    /// Settings, About, Recall Adventurers, and honest fallbacks for every drawer entry with no
    /// real backend). Every screen is a runtime-built bordered-card popup opened through
    /// <see cref="AppShellController.OpenPopup"/> / closed through <c>ClosePopup</c> — it never
    /// rebuilds the shell, never touches Phase 1-8 tab controllers, and reads/mutates state only
    /// through the existing service interfaces (IQuestService, ISettingsService, IDungeonService,
    /// GameDatabase). Visual conventions (object_border_* bordered cards, AddAction/AddText
    /// helpers) mirror <see cref="GuildMaster.Runtime.UI.Dungeon.DungeonsTabController"/> and
    /// <see cref="GuildMaster.Runtime.UI.Raid.RaidsTabController"/> exactly, per
    /// Docs/Legacy_Audit/phase_7_full_report.md / phase_8_full_report.md.
    /// </summary>
    public sealed class AuxiliaryController : MonoBehaviour
    {
        private ServiceContainer _services;
        private AppShellController _shell;
        private bool _initialized;

        public void Setup(ServiceContainer services, AppShellController shell, GameObject drawerRoot)
        {
            _services = services;
            _shell = shell;
            _initialized = services != null && shell != null;
            if (!_initialized || drawerRoot == null) return;

            var panel = drawerRoot.transform.Find("DrawerPanel");
            if (panel == null) return;

            Wire(panel, "DrawerItem_Quests", OpenQuests);
            Wire(panel, "DrawerItem_Bestiary", OpenBestiaryHub);
            Wire(panel, "DrawerItem_Settings", OpenSettings);
            Wire(panel, "DrawerItem_RecallAdventurers", OpenRecall);
            Wire(panel, "DrawerItem_Shop", OpenShop);
            Wire(panel, "DrawerItem_Messages", () => OpenFallback("MESSAGES", "drawer_icon_king_message", "No messages are available."));
            Wire(panel, "DrawerItem_FAQ", OpenAbout);
            Wire(panel, "DrawerItem_Achievements", () => OpenFallback("ACHIEVEMENTS", "drawer_icon_achievements", "Achievement records are not available yet."));
            Wire(panel, "DrawerItem_CloudSave", () => OpenFallback("CLOUD SAVE", null, "Cloud save is not available yet. The Cloud Save preference under Settings is stored locally only."));
            Wire(panel, "DrawerItem_RedeemCode", () => OpenFallback("REDEEM CODE", null, "Code redemption is not available yet."));
            Wire(panel, "DrawerItem_Community", () => OpenFallback("COMMUNITY", "drawer_icon_reddit", "Community links are not available yet."));

            NormalizeDrawer(panel);
        }

        // ================================================================
        // Phase 9 UX Hotfix — Drawer normalization (shared helper, no per-item hacks).
        //
        // Root cause of the original visual bugs: the old AddDrawerIcons() sized every icon at
        // LegacyUITheme.DP(56) = 168px inside a 96px-tall row (icons rendered ~1.75x the row
        // height, overlapping the row above/below), only shifted the label's offsetMin for the 7
        // rows that had real legacy icon art (the other 4 rows kept their original offsetMin.x,
        // producing the "Settings label positioned wrong" / inconsistent-alignment bug), and never
        // touched DrawerItem_Quests's RectTransform after it was duplicated from DrawerItem_Shop —
        // that duplicate ended up with sizeDelta.x = -800 on a horizontally stretched anchor (net
        // rendered width = 0) and anchoredPosition.y = -1940 (below the 1920px-tall DrawerPanel),
        // i.e. an invisible, zero-width row parked off the bottom of the panel. There was also no
        // ScrollRect anywhere in the drawer, so nothing could have reached it even if it had been
        // visible.
        //
        // Fix: every row is reparented under a real ScrollRect/Viewport/Content
        // (VerticalLayoutGroup + ContentSizeFitter + RectMask2D, the same pattern already used by
        // BuildPopup()'s scroll content) so all 11 rows are always reachable regardless of screen
        // height, and each row is rebuilt into a canonical
        // DrawerItem_X -> IconSlot -> Icon / Label structure driven entirely by LayoutElement +
        // HorizontalLayoutGroup — no manual per-item anchoredPosition/sizeDelta math survives, so a
        // corrupted transform like the old Quests duplicate cannot happen again.
        // ================================================================
        private static readonly (string ItemName, string IconKey)[] DrawerRowOrder =
        {
            ("DrawerItem_Shop", "shop"),
            ("DrawerItem_Settings", null),
            ("DrawerItem_RecallAdventurers", "drawer_icon_recall_adventurers"),
            ("DrawerItem_Messages", "drawer_icon_king_message"),
            ("DrawerItem_FAQ", "drawer_icon_faq"),
            ("DrawerItem_Bestiary", "drawer_icon_bestiary"),
            ("DrawerItem_Achievements", "drawer_icon_achievements"),
            ("DrawerItem_CloudSave", null),
            ("DrawerItem_RedeemCode", null),
            ("DrawerItem_Community", "drawer_icon_reddit"),
            ("DrawerItem_Quests", null),
        };

        private void NormalizeDrawer(Transform panel)
        {
            float rowHeight = LegacyUITheme.DP(32f);       // 96px — matches the original legacy row height
            float iconSlotWidth = LegacyUITheme.DP(26f);   // 78px — within the 72-84px target range
            float iconSize = LegacyUITheme.DP(20f);        // 60px — within the 48-64px target range
            float rowSpacing = LegacyUITheme.DP(2f);       // 6px
            float rowPaddingX = LegacyUITheme.DP(6f);      // 18px
            float labelGap = LegacyUITheme.DP(4f);         // 12px between IconSlot and Label

            var scrollTr = panel.Find("DrawerScrollView");
            RectTransform content;
            if (scrollTr == null)
            {
                var scrollGo = new GameObject("DrawerScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
                scrollGo.transform.SetParent(panel, false);
                var scrollRt = (RectTransform)scrollGo.transform;
                scrollRt.anchorMin = Vector2.zero; scrollRt.anchorMax = Vector2.one;
                scrollRt.offsetMin = Vector2.zero; scrollRt.offsetMax = Vector2.zero;

                var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
                contentGo.transform.SetParent(scrollGo.transform, false);
                content = (RectTransform)contentGo.transform;
                content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1);
                content.pivot = new Vector2(0.5f, 1); content.sizeDelta = Vector2.zero;
                var vlayout = contentGo.GetComponent<VerticalLayoutGroup>();
                vlayout.spacing = rowSpacing;
                vlayout.padding = new RectOffset(0, 0, LegacyUITheme.DP(7), LegacyUITheme.DP(16));
                vlayout.childControlWidth = true; vlayout.childControlHeight = true;
                vlayout.childForceExpandWidth = true; vlayout.childForceExpandHeight = false;
                contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var scroll = scrollGo.GetComponent<ScrollRect>();
                scroll.horizontal = false; scroll.vertical = true; scroll.viewport = scrollRt; scroll.content = content;
            }
            else
            {
                content = (RectTransform)scrollTr.Find("Content");
            }

            for (int i = 0; i < DrawerRowOrder.Length; i++)
            {
                var row = DrawerRowOrder[i];
                var item = panel.Find(row.ItemName) ?? content.Find(row.ItemName);
                if (item == null) continue;
                NormalizeDrawerRow(item, row.IconKey, content, i, rowHeight, iconSlotWidth, iconSize, rowPaddingX, labelGap);
            }
        }

        private void NormalizeDrawerRow(Transform item, string iconKey, Transform content, int siblingIndex,
            float rowHeight, float iconSlotWidth, float iconSize, float rowPaddingX, float labelGap)
        {
            item.SetParent(content, false);
            item.SetSiblingIndex(siblingIndex);

            var rowGo = item.gameObject;
            var rowLayoutElement = rowGo.GetComponent<LayoutElement>() ?? rowGo.AddComponent<LayoutElement>();
            rowLayoutElement.preferredHeight = rowHeight; rowLayoutElement.minHeight = rowHeight; rowLayoutElement.flexibleWidth = 1;

            var rowLayout = rowGo.GetComponent<HorizontalLayoutGroup>() ?? rowGo.AddComponent<HorizontalLayoutGroup>();
            rowLayout.padding = new RectOffset((int)rowPaddingX, (int)rowPaddingX, 0, 0);
            rowLayout.spacing = labelGap;
            rowLayout.childControlWidth = true; rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false; rowLayout.childForceExpandHeight = true;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;

            // IconSlot is always present at the same fixed width, whether or not this row has a
            // real legacy icon — this is what keeps every row's Label starting at the same X.
            var iconSlotTr = item.Find("IconSlot");
            GameObject iconSlotGo = iconSlotTr != null ? iconSlotTr.gameObject
                : new GameObject("IconSlot", typeof(RectTransform), typeof(LayoutElement));
            if (iconSlotTr == null) iconSlotGo.transform.SetParent(item, false);
            iconSlotGo.transform.SetSiblingIndex(0);
            var iconSlotLayout = iconSlotGo.GetComponent<LayoutElement>() ?? iconSlotGo.AddComponent<LayoutElement>();
            iconSlotLayout.preferredWidth = iconSlotWidth; iconSlotLayout.minWidth = iconSlotWidth; iconSlotLayout.flexibleWidth = 0;

            // A stray "Icon" directly under the row (from the old AddDrawerIcons implementation)
            // must move under IconSlot, not stay on the row itself.
            var strayIcon = item.Find("Icon");
            if (strayIcon != null) Destroy(strayIcon.gameObject);

            var iconTr = iconSlotGo.transform.Find("Icon");
            var sprite = string.IsNullOrEmpty(iconKey) ? null : LegacySpriteRegistry.GetSprite(iconKey);
            if (sprite != null)
            {
                var iconGo = iconTr != null ? iconTr.gameObject : new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(iconSlotGo.transform, false);
                var iconRt = (RectTransform)iconGo.transform;
                iconRt.anchorMin = new Vector2(0.5f, 0.5f); iconRt.anchorMax = new Vector2(0.5f, 0.5f); iconRt.pivot = new Vector2(0.5f, 0.5f);
                iconRt.sizeDelta = new Vector2(iconSize, iconSize); iconRt.anchoredPosition = Vector2.zero;
                var iconImg = iconGo.GetComponent<Image>() ?? iconGo.AddComponent<Image>();
                iconImg.sprite = sprite; iconImg.preserveAspect = true; iconImg.raycastTarget = false; iconImg.color = Color.white;
            }
            else if (iconTr != null)
            {
                // No real icon for this row — leave the slot empty rather than fabricate one.
                Destroy(iconTr.gameObject);
            }

            var labelTr = item.Find("Label");
            if (labelTr != null)
            {
                labelTr.SetSiblingIndex(1);
                var labelLayout = labelTr.GetComponent<LayoutElement>() ?? labelTr.gameObject.AddComponent<LayoutElement>();
                labelLayout.flexibleWidth = 1; labelLayout.minWidth = 0;
                var labelText = labelTr.GetComponent<Text>();
                if (labelText != null)
                {
                    labelText.alignment = TextAnchor.MiddleLeft;
                    labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    labelText.verticalOverflow = VerticalWrapMode.Truncate;
                }
            }
        }

        private void Wire(Transform panel, string itemName, Action onOpen)
        {
            var item = panel.Find(itemName);
            var button = item != null ? item.GetComponent<Button>() : null;
            if (button == null) return;
            // Additive: AppShellController.Initialize already wired every drawer item to
            // CloseDrawer(); this appends the real destination without touching that wiring.
            button.onClick.AddListener(() =>
            {
                _shell.CloseDrawer();
                onOpen();
            });
        }

        // ================================================================
        // Popup shell (shared by every auxiliary screen)
        // ================================================================

        // ================================================================
        // Phase 9 UX Hotfix — Popup frame (shared helper, no per-screen duplication).
        //
        // Original defects: the panel background used "object_border_dim_white_extra_opaque"
        // (LegacyUITheme.ExtraOpaqueBackground = #40ffffff, ~25% alpha) so the Headquarters shell
        // behind it stayed clearly visible — it read as a translucent debug overlay, not a
        // production legacy screen. The header text scrolled away with the body, so the only close
        // action for a long list (Quests/Bestiary) was a "CLOSE" row at the very bottom — users had
        // to scroll all the way down just to exit. There was also no persistent close control
        // independent of scroll position.
        //
        // Fix: Panel background switched to "dialog_border" (LegacyUITheme.CardviewDarkBackground
        // = #1e1e1e, fully opaque, the same sprite Phase 7/8 dungeon/raid dialogs use). A
        // non-scrolling Header (Title + fixed top-right CloseButton_X, optional top-left
        // BackButton for nested screens) sits above a Divider; only ScrollContent below it scrolls.
        // The CloseButton_X is wired to ClosePopup() directly, independent of any bottom-row CLOSE
        // action, and independent of scroll position — it is always reachable per the mandatory
        // "persistent close control" rule.
        // ================================================================
        private RectTransform BuildPopup(string headerTitle, Action onBack = null)
        {
            var popupGo = new GameObject("AuxiliaryPopup", typeof(RectTransform), typeof(Image));
            var rt = popupGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.06f, 0.08f); rt.anchorMax = new Vector2(0.94f, 0.92f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var bg = popupGo.GetComponent<Image>();
            bg.sprite = LegacyThemeSprites.Get("dialog_border");
            bg.type = bg.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            bg.color = bg.sprite != null ? Color.white : LegacyUITheme.CardviewDarkBackground;

            float headerHeight = LegacyUITheme.DP(56f);   // 168px — room for title + 54px close target
            float closeSize = LegacyUITheme.DP(18f);       // 54px — within the 44-56px touch-target range
            float sideMargin = LegacyUITheme.DP(14f);
            float dividerHeight = LegacyUITheme.DP(1f);

            // --- Header (never scrolls) ---
            var headerGo = new GameObject("Header", typeof(RectTransform));
            headerGo.transform.SetParent(popupGo.transform, false);
            var headerRt = (RectTransform)headerGo.transform;
            headerRt.anchorMin = new Vector2(0, 1); headerRt.anchorMax = new Vector2(1, 1); headerRt.pivot = new Vector2(0.5f, 1);
            headerRt.sizeDelta = new Vector2(0, headerHeight);
            headerRt.anchoredPosition = Vector2.zero;

            var closeGo = new GameObject("CloseButton_X", typeof(RectTransform), typeof(Image), typeof(Button));
            closeGo.transform.SetParent(headerGo.transform, false);
            var closeRt = (RectTransform)closeGo.transform;
            closeRt.anchorMin = new Vector2(1, 0.5f); closeRt.anchorMax = new Vector2(1, 0.5f); closeRt.pivot = new Vector2(1, 0.5f);
            closeRt.sizeDelta = new Vector2(closeSize, closeSize);
            closeRt.anchoredPosition = new Vector2(-sideMargin, 0);
            var closeImg = closeGo.GetComponent<Image>();
            closeImg.sprite = LegacyThemeSprites.Get("object_border_dim_white");
            closeImg.type = closeImg.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            closeImg.color = closeImg.sprite != null ? Color.white : LegacyUITheme.StandardBackground;
            var closeButton = closeGo.GetComponent<Button>();
            closeButton.targetGraphic = closeImg;
            closeButton.onClick.AddListener(ClosePopup);
            StretchGlyph(closeGo.transform, "×", 26).fontStyle = FontStyle.Bold;

            float titleLeft = sideMargin;
            if (onBack != null)
            {
                var backGo = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
                backGo.transform.SetParent(headerGo.transform, false);
                var backRt = (RectTransform)backGo.transform;
                backRt.anchorMin = new Vector2(0, 0.5f); backRt.anchorMax = new Vector2(0, 0.5f); backRt.pivot = new Vector2(0, 0.5f);
                backRt.sizeDelta = new Vector2(closeSize, closeSize);
                backRt.anchoredPosition = new Vector2(sideMargin, 0);
                var backImg = backGo.GetComponent<Image>();
                backImg.sprite = LegacyThemeSprites.Get("object_border_dim_white");
                backImg.type = backImg.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
                backImg.color = backImg.sprite != null ? Color.white : LegacyUITheme.StandardBackground;
                var backButton = backGo.GetComponent<Button>();
                backButton.targetGraphic = backImg;
                backButton.onClick.AddListener(() => onBack());
                StretchGlyph(backGo.transform, "‹", 26).fontStyle = FontStyle.Bold;
                titleLeft = sideMargin * 2 + closeSize;
            }

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleGo.transform.SetParent(headerGo.transform, false);
            var titleRt = (RectTransform)titleGo.transform;
            titleRt.anchorMin = new Vector2(0, 0); titleRt.anchorMax = new Vector2(1, 1);
            titleRt.offsetMin = new Vector2(titleLeft, 0);
            titleRt.offsetMax = new Vector2(-(sideMargin * 2 + closeSize), 0);
            var titleText = titleGo.GetComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 24; titleText.color = LegacyUITheme.BrassBorder; titleText.text = headerTitle;
            titleText.fontStyle = FontStyle.Bold; titleText.alignment = TextAnchor.MiddleLeft; titleText.raycastTarget = false;
            titleText.horizontalOverflow = HorizontalWrapMode.Overflow; titleText.verticalOverflow = VerticalWrapMode.Truncate;

            // --- Divider (visually separates the fixed header from the scrollable body) ---
            var dividerGo = new GameObject("Divider", typeof(RectTransform), typeof(Image));
            dividerGo.transform.SetParent(popupGo.transform, false);
            var dividerRt = (RectTransform)dividerGo.transform;
            dividerRt.anchorMin = new Vector2(0, 1); dividerRt.anchorMax = new Vector2(1, 1); dividerRt.pivot = new Vector2(0.5f, 1);
            dividerRt.sizeDelta = new Vector2(-(sideMargin * 2), dividerHeight);
            dividerRt.anchoredPosition = new Vector2(0, -headerHeight);
            dividerGo.GetComponent<Image>().color = LegacyUITheme.GreyBorder;

            // --- ScrollViewport / ScrollContent (only this part scrolls) ---
            var scrollGo = new GameObject("ScrollViewport", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
            scrollGo.transform.SetParent(popupGo.transform, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero; scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(sideMargin, sideMargin);
            scrollRt.offsetMax = new Vector2(-sideMargin, -(headerHeight + dividerHeight + LegacyUITheme.DP(6f)));

            var bodyGo = new GameObject("ScrollContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            bodyGo.transform.SetParent(scrollGo.transform, false);
            var content = bodyGo.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1); content.sizeDelta = Vector2.zero;
            var layout = bodyGo.GetComponent<VerticalLayoutGroup>();
            layout.spacing = LegacyUITheme.DP(8);
            layout.padding = new RectOffset(LegacyUITheme.DP(4), LegacyUITheme.DP(4), LegacyUITheme.DP(4), LegacyUITheme.DP(24));
            layout.childControlWidth = true; layout.childControlHeight = true;
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            bodyGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.viewport = scrollRt; scroll.content = content;

            _shell.OpenPopup(popupGo);
            return content;
        }

        private void ClosePopup() => _shell.ClosePopup();

        // ================================================================
        // 9B — Quests (real: IQuestService)
        // ================================================================
        private void OpenQuests()
        {
            if (_shell.IsPopupOpen) return;
            var content = BuildPopup("QUESTS");
            var quests = _services.Quest?.GetActiveQuests() ?? new List<QuestRuntime>().AsReadOnly();

            if (quests.Count == 0)
            {
                AddText(content, "Empty", "No quests are currently available.", 16, LegacyUITheme.DimWhite, 60, TextAnchor.MiddleCenter);
            }
            else
            {
                foreach (var q in quests)
                {
                    BuildQuestCard(content, q);
                }
            }
            AddAction(content, "Close", "CLOSE", "", "object_border_dim_white", ClosePopup);
        }

        private void BuildQuestCard(Transform parent, QuestRuntime quest)
        {
            bool claimable = quest.State == QuestState.Completed;
            bool isGems = quest.Rarity >= 4;
            int reward = _services.Quest.GetRewardAmount(quest.Rarity, isGems);

            // Density fix: the original 108dp card (plus a dead-weight zero-height "ClaimRow"
            // spacer object) wasted vertical space and showed fewer quests per viewport. Rows are
            // now sized to their actual content (compact non-claimable cards are shorter than
            // claimable ones, which need room for the CLAIM REWARD row) instead of one fixed tall
            // height for every card.
            float cardHeight = LegacyUITheme.DP(claimable ? 96 : 72);
            var card = new GameObject("Quest_" + quest.InstanceId, typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(VerticalLayoutGroup));
            card.transform.SetParent(parent, false);
            card.GetComponent<LayoutElement>().preferredHeight = cardHeight;
            var cardImg = card.GetComponent<Image>();
            cardImg.sprite = LegacyThemeSprites.Get(claimable ? "object_border_brass" : "object_border_dim_white");
            cardImg.type = cardImg.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            cardImg.color = cardImg.sprite != null ? Color.white : LegacyUITheme.StandardBackground;
            var vlayout = card.GetComponent<VerticalLayoutGroup>();
            vlayout.padding = new RectOffset(LegacyUITheme.DP(10), LegacyUITheme.DP(10), LegacyUITheme.DP(6), LegacyUITheme.DP(6));
            vlayout.spacing = LegacyUITheme.DP(2);
            vlayout.childControlWidth = true; vlayout.childControlHeight = true;
            vlayout.childForceExpandWidth = true; vlayout.childForceExpandHeight = false;

            AddText(card.transform, "Title", Format(quest.Definition?.id) + "  (Rarity " + quest.Rarity + ")", 15, LegacyUITheme.DimWhite, 24, TextAnchor.MiddleLeft, true).fontStyle = FontStyle.Bold;
            AddText(card.transform, "Progress", "Progress: " + quest.Progress + " / " + quest.TargetProgress, 12, LegacyUITheme.DimWhite, 18, TextAnchor.MiddleLeft, true);
            AddProgressBar(card.transform, (float)quest.Progress, Math.Max(1, quest.TargetProgress));

            string rewardText = "Reward: " + (isGems ? reward + " Gems" : reward + " War Doctrine Progress");
            AddText(card.transform, "Reward", rewardText, 11, LegacyUITheme.BrassBorder, 18, TextAnchor.MiddleLeft, true);

            // A CLAIM button only appears when the quest is actually claimable — non-claimable
            // cards stay compact instead of reserving dead space for a button that can't be tapped.
            if (claimable)
            {
                AddAction(card.transform, "Claim", "CLAIM REWARD", "Tap to claim", "object_border_brass",
                    () => { _services.Quest.ClaimReward(quest.InstanceId); RebuildQuests(); });
            }
        }

        private void RebuildQuests()
        {
            // Idempotent rebuild-in-place: close the old popup and reopen fresh content so the
            // claimed quest disappears immediately (matches Dungeons/Raids ClearContent+rebuild
            // convention) and the HUD reflects the new gem/doctrine total right away.
            _shell.ClosePopup();
            OpenQuests();
            _shell.RefreshHud();
        }

        // ================================================================
        // 9C — Bestiary (real: EnemyDefinition catalog; no "discovered" concept in the backend,
        // so the full catalog is shown — a deliberate, documented deviation from legacy
        // fog-of-war, see phase_9_full_report.md Section 8)
        // ================================================================
        private void OpenBestiaryHub()
        {
            if (_shell.IsPopupOpen) _shell.ClosePopup();
            var content = BuildPopup("BESTIARY");
            var enemies = _services.Database?.GetAll<EnemyDefinition>()?.OrderBy(e => e.id, StringComparer.OrdinalIgnoreCase).ToList()
                          ?? new List<EnemyDefinition>();

            if (enemies.Count == 0)
            {
                AddText(content, "Empty", "Bestiary records are not available yet.", 16, LegacyUITheme.DimWhite, 60, TextAnchor.MiddleCenter);
            }
            else
            {
                AddText(content, "Sub", enemies.Count + " known enemies", 13, LegacyUITheme.DimWhite, 26, TextAnchor.MiddleLeft, true);
                foreach (var enemy in enemies)
                {
                    var e = enemy;
                    AddAction(content, "Enemy_" + e.id, Format(e.id), e.Rarity > 0 ? "Rarity " + e.Rarity : "Standard enemy",
                        "object_border_dim_white", () => OpenBestiaryDetail(e));
                }
            }
            AddAction(content, "Close", "CLOSE", "", "object_border_dim_white", ClosePopup);
        }

        private void OpenBestiaryDetail(EnemyDefinition enemy)
        {
            if (_shell.IsPopupOpen) _shell.ClosePopup();
            var content = BuildPopup(Format(enemy.id), OpenBestiaryHub);

            var portraitFrame = new GameObject("PortraitFrame", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            portraitFrame.transform.SetParent(content, false);
            portraitFrame.GetComponent<LayoutElement>().preferredHeight = LegacyUITheme.DP(140);
            var pf = portraitFrame.GetComponent<Image>();
            pf.sprite = LegacyThemeSprites.Get("object_border_no_background");
            pf.type = pf.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            pf.color = pf.sprite != null ? Color.white : Color.clear;
            var portrait = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portrait.transform.SetParent(portraitFrame.transform, false);
            var pRt = portrait.GetComponent<RectTransform>();
            pRt.anchorMin = new Vector2(0.5f, 0.5f); pRt.anchorMax = new Vector2(0.5f, 0.5f);
            pRt.sizeDelta = new Vector2(LegacyUITheme.DP(110), LegacyUITheme.DP(110));
            var pImg = portrait.GetComponent<Image>();
            pImg.sprite = LegacySpriteRegistry.GetUnitSprite(enemy.id);
            pImg.preserveAspect = true; pImg.raycastTarget = false;
            if (pImg.sprite == null) pImg.color = new Color(1f, 1f, 1f, 0.15f);

            AddText(content, "Rarity", "Rarity: " + (enemy.Rarity > 0 ? enemy.Rarity.ToString() : "Not exposed by backend"), 14, LegacyUITheme.DimWhite, 30, TextAnchor.MiddleLeft, true);
            AddText(content, "Hp", "Max HP: " + enemy.BaseMaxHp, 14, LegacyUITheme.DimWhite, 26, TextAnchor.MiddleLeft, true);
            AddText(content, "Def", "Defense: " + enemy.BaseDefense + "   Magic Defense: " + enemy.BaseMagicDefense, 14, LegacyUITheme.DimWhite, 26, TextAnchor.MiddleLeft, true);
            AddText(content, "Dmg", "Damage: " + enemy.MinDamage + " - " + enemy.MaxDamage + (enemy.IsMagic ? "  (Magic)" : "") + (enemy.IsRanged ? "  (Ranged)" : ""), 14, LegacyUITheme.DimWhite, 26, TextAnchor.MiddleLeft, true);
            AddText(content, "Exp", "Experience given: " + enemy.ExpGiven, 14, LegacyUITheme.DimWhite, 26, TextAnchor.MiddleLeft, true);

            AddAction(content, "Back", "BACK TO BESTIARY", "", "object_border_dim_white", OpenBestiaryHub);
        }

        // ================================================================
        // 9D — Settings (real: ISettingsService / SaveData.Settings*)
        // ================================================================
        private void OpenSettings()
        {
            if (_shell.IsPopupOpen) _shell.ClosePopup();
            var content = BuildPopup("SETTINGS");
            var settings = _services.Settings;

            AddText(content, "GameVersion", "Version " + (settings?.GetGameVersion() ?? Application.version), 13, LegacyUITheme.DimWhite, 26, TextAnchor.MiddleLeft, true);

            AddToggle(content, "Sound", "Sound Effects", settings, "sound");
            AddToggle(content, "Music", "Music", settings, "music");
            AddToggle(content, "Vibration", "Vibration", settings, "vibration");
            AddToggle(content, "Notifications", "Notifications", settings, "notifications");
            AddToggle(content, "Cloud", "Cloud Save (local flag only)", settings, "cloud");
            AddToggle(content, "Colorblind", "Colorblind Mode", settings, "colorblind");
            AddToggle(content, "AutoOpenDetail", "Auto-Open Dungeon Detail", settings, "autoopendetail");
            AddToggle(content, "ConfirmRetreat", "Confirm Before Retreat", settings, "confirmretreat");
            AddToggle(content, "ConfirmSwap", "Confirm Before Swap", settings, "confirmswap");
            AddToggle(content, "ConfirmUpgrade", "Confirm Before Upgrade", settings, "confirmupgrade");
            AddToggle(content, "CraftMax", "Craft Max Amount by Default", settings, "craftmax");
            AddToggle(content, "SellMax", "Sell Max Amount by Default", settings, "sellmax");
            AddToggle(content, "VerboseLogs", "Verbose Logs", settings, "verboselogs");

            AddAction(content, "ResetPrompt", "RESET SETTINGS TO DEFAULT", "Restores every toggle above to its default value", "object_border_dim_white_unavailable", OpenResetConfirm);
            AddAction(content, "Close", "CLOSE", "", "object_border_dim_white", ClosePopup);
        }

        private void AddToggle(Transform parent, string name, string label, ISettingsService settings, string key)
        {
            bool value = settings != null && settings.GetToggle(key);
            var row = new GameObject("Toggle_" + name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            row.GetComponent<LayoutElement>().preferredHeight = LegacyUITheme.DP(48);
            var img = row.GetComponent<Image>();
            img.sprite = LegacyThemeSprites.Get("object_border_dim_white");
            img.type = img.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = img.sprite != null ? Color.white : LegacyUITheme.StandardBackground;
            var hl = row.GetComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(LegacyUITheme.DP(10), LegacyUITheme.DP(10), LegacyUITheme.DP(6), LegacyUITheme.DP(6));
            hl.childControlWidth = true; hl.childControlHeight = true; hl.childForceExpandWidth = false; hl.childForceExpandHeight = true;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            labelGo.transform.SetParent(row.transform, false);
            labelGo.GetComponent<LayoutElement>().flexibleWidth = 1;
            var labelText = labelGo.GetComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 15; labelText.color = LegacyUITheme.DimWhite; labelText.text = label;
            labelText.alignment = TextAnchor.MiddleLeft; labelText.raycastTarget = false;

            var stateGo = new GameObject("State", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            stateGo.transform.SetParent(row.transform, false);
            stateGo.GetComponent<LayoutElement>().preferredWidth = LegacyUITheme.DP(70);
            var stateText = stateGo.GetComponent<Text>();
            stateText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            stateText.fontSize = 15; stateText.fontStyle = FontStyle.Bold;
            stateText.color = value ? LegacyUITheme.BrassBorder : LegacyUITheme.GreyBorder;
            stateText.text = value ? "ON" : "OFF";
            stateText.alignment = TextAnchor.MiddleRight; stateText.raycastTarget = false;

            var button = row.GetComponent<Button>();
            button.targetGraphic = img;
            button.onClick.AddListener(() =>
            {
                bool newValue = !settings.GetToggle(key);
                settings.SetToggle(key, newValue);
                settings.SaveCurrentState();
                stateText.text = newValue ? "ON" : "OFF";
                stateText.color = newValue ? LegacyUITheme.BrassBorder : LegacyUITheme.GreyBorder;
            });
        }

        private void OpenResetConfirm()
        {
            if (_shell.IsPopupOpen) _shell.ClosePopup();
            var content = BuildPopup("RESET SETTINGS?", OpenSettings);
            AddText(content, "Warning", "This resets every toggle and language preference above to its default value. This does not affect your save progress, adventurers, or items.", 14, LegacyUITheme.DimWhite, 70, TextAnchor.MiddleLeft, true);
            AddAction(content, "Confirm", "CONFIRM RESET", "This cannot be undone", "object_border_brass", () =>
            {
                _services.Settings.ResetToDefault();
                _services.Settings.SaveCurrentState();
                OpenSettings();
            });
            AddAction(content, "Cancel", "CANCEL", "", "object_border_dim_white", OpenSettings);
        }

        // ================================================================
        // 9E/9G — About / Help / Guide (real Application.version + real game version; no
        // fabricated credits, no fake stateful tutorial — the legacy FAQ dialog's content was not
        // recovered from the decompiled resources, so this shows only real, honest data)
        // ================================================================
        private void OpenAbout()
        {
            if (_shell.IsPopupOpen) return;
            var content = BuildPopup("ABOUT / HELP");
            AddText(content, "Title", "Guild Master - Idle Dungeons", 18, LegacyUITheme.BrassBorder, 32, TextAnchor.MiddleLeft, true).fontStyle = FontStyle.Bold;
            AddText(content, "Version", "Game version: " + (_services.Settings?.GetGameVersion() ?? "unknown"), 14, LegacyUITheme.DimWhite, 26, TextAnchor.MiddleLeft, true);
            AddText(content, "AppVersion", "Application version: " + Application.version, 14, LegacyUITheme.DimWhite, 26, TextAnchor.MiddleLeft, true);
            var frame = new GameObject("FallbackFrame", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            frame.transform.SetParent(content, false);
            frame.GetComponent<LayoutElement>().preferredHeight = LegacyUITheme.DP(60);
            var fi = frame.GetComponent<Image>();
            fi.sprite = LegacyThemeSprites.Get("object_border_no_background");
            fi.type = fi.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            fi.color = fi.sprite != null ? Color.white : Color.clear;
            AddText(frame.transform, "Note", "Guide content is not available yet.", 14, LegacyUITheme.DimWhite, LegacyUITheme.DP(60), TextAnchor.MiddleCenter);
            AddAction(content, "Close", "CLOSE", "", "object_border_dim_white", ClosePopup);
        }

        // ================================================================
        // 9H — Recall Adventurers (real: IDungeonService multi-expedition slots + StopExpedition;
        // no bulk "recall everyone" service exists, so this reads/stops only real expedition state)
        // ================================================================
        private void OpenRecall()
        {
            RebuildRecall();
        }

        private void RebuildRecall()
        {
            if (_shell.IsPopupOpen) _shell.ClosePopup();
            var content = BuildPopup("RECALL ADVENTURERS");
            var dungeon = _services.Dungeon;
            int maxSlots = dungeon?.MaxExpeditions ?? 0;
            int activeCount = 0;

            if (dungeon == null || maxSlots == 0)
            {
                AddText(content, "Empty", "No expedition state is available.", 16, LegacyUITheme.DimWhite, 60, TextAnchor.MiddleCenter);
            }
            else
            {
                for (int i = 0; i < maxSlots; i++)
                {
                    int slot = i;
                    var exp = dungeon.GetExpedition(slot);
                    bool active = exp?.Dungeon != null;
                    if (active) activeCount++;
                    string title = "Party " + (slot + 1) + (active ? "  •  " + Format(exp.Dungeon.Definition?.id) : "  •  Idle");
                    string sub = active ? "Progress: " + exp.Dungeon.Progress + "/" + exp.Dungeon.MaxProgress + "  •  Tap to recall" : "Not on an expedition";
                    AddAction(content, "Slot_" + slot, title, sub, active ? "object_border_brass" : "object_border_dim_white_unavailable",
                        active ? (Action)(() => { dungeon.StopExpedition(slot); RebuildRecall(); }) : null, active);
                }
            }

            if (activeCount > 0)
            {
                AddAction(content, "RecallAll", "RECALL ALL", "Stop every active expedition and bring all parties home", "object_border_brass", () =>
                {
                    for (int i = 0; i < maxSlots; i++) dungeon.StopExpedition(i);
                    RebuildRecall();
                });
            }
            AddAction(content, "Close", "CLOSE", "", "object_border_dim_white", ClosePopup);
        }

        // ================================================================
        // Shop → real Market flow already exists on the Headquarters tab; the drawer entry
        // routes there honestly instead of duplicating the Market dialog.
        // ================================================================
        private void OpenShop()
        {
            _shell.SwitchTab(0);
        }

        // ================================================================
        // Fallback policy (shared): bordered legacy card, real icon if available, short honest
        // English message, no fake buttons, no fake progress, no technical wording.
        // ================================================================
        private void OpenFallback(string title, string spriteKey, string message)
        {
            if (_shell.IsPopupOpen) return;
            var content = BuildPopup(title);
            if (!string.IsNullOrEmpty(spriteKey))
            {
                var sprite = LegacySpriteRegistry.GetSprite(spriteKey);
                if (sprite != null)
                {
                    var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                    iconGo.transform.SetParent(content, false);
                    iconGo.GetComponent<LayoutElement>().preferredHeight = LegacyUITheme.DP(80);
                    var img = iconGo.GetComponent<Image>();
                    img.sprite = sprite; img.preserveAspect = true; img.raycastTarget = false;
                }
            }
            var frame = new GameObject("FallbackFrame", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            frame.transform.SetParent(content, false);
            frame.GetComponent<LayoutElement>().preferredHeight = LegacyUITheme.DP(70);
            var fi = frame.GetComponent<Image>();
            fi.sprite = LegacyThemeSprites.Get("object_border_no_background");
            fi.type = fi.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            fi.color = fi.sprite != null ? Color.white : Color.clear;
            AddText(frame.transform, "Note", message, 15, LegacyUITheme.DimWhite, LegacyUITheme.DP(70), TextAnchor.MiddleCenter);
            AddAction(content, "Close", "CLOSE", "", "object_border_dim_white", ClosePopup);
        }

        // ================================================================
        // Shared helpers (mirrors DungeonsTabController / RaidsTabController conventions)
        // ================================================================
        private static string Format(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "Unknown";
            return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(id.Replace("_", " "));
        }

        /// <summary>Text glyph that fills its parent exactly (CloseButton_X / BackButton are not
        /// inside a LayoutGroup, so AddText's LayoutElement-driven sizing would leave them at the
        /// default zero-size RectTransform instead of centered in the button).</summary>
        private static Text StretchGlyph(Transform parent, string value, int size)
        {
            var go = new GameObject("Glyph", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size; text.color = LegacyUITheme.BrassBorder; text.text = value;
            text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
            return text;
        }

        private static Text AddText(Transform parent, string name, string value, int size, Color color, float height, TextAnchor alignment, bool flexible = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>(); le.preferredHeight = height; le.flexibleWidth = flexible ? 1 : 0;
            var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.color = color; text.text = value; text.alignment = alignment; text.raycastTarget = false; text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static GameObject AddAction(Transform parent, string name, string title, string subtitle, string borderKey, Action click, bool interactable = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(parent, false);
            var layout = go.GetComponent<LayoutElement>(); layout.preferredHeight = LegacyUITheme.DP(58); layout.flexibleWidth = 1;
            var image = go.GetComponent<Image>();
            image.sprite = LegacyThemeSprites.Get(borderKey);
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = image.sprite != null ? Color.white : LegacyUITheme.StandardBackground;
            image.raycastTarget = true;
            var row = go.GetComponent<HorizontalLayoutGroup>();
            row.spacing = LegacyUITheme.DP(8);
            int padding = LegacyUITheme.DP(8);
            row.padding = new RectOffset(padding, padding, padding, padding);
            row.childControlWidth = true; row.childControlHeight = true;
            row.childForceExpandWidth = false; row.childForceExpandHeight = true;
            var button = go.GetComponent<Button>(); button.interactable = interactable; button.targetGraphic = image;
            if (click != null && interactable) button.onClick.AddListener(() => click());
            var body = new GameObject("Text", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            body.transform.SetParent(go.transform, false);
            var bodyLayout = body.GetComponent<LayoutElement>(); bodyLayout.flexibleWidth = 1; bodyLayout.flexibleHeight = 1;
            var textLayout = body.GetComponent<VerticalLayoutGroup>(); textLayout.childControlWidth = true; textLayout.childControlHeight = true; textLayout.childForceExpandWidth = true; textLayout.childForceExpandHeight = false;
            AddText(body.transform, "Title", title, 16, interactable ? LegacyUITheme.DimWhite : LegacyUITheme.GreyBorder, 30, TextAnchor.MiddleLeft, true);
            AddText(body.transform, "Subtitle", subtitle ?? string.Empty, 11, LegacyUITheme.DimWhite, 22, TextAnchor.MiddleLeft, true);
            return go;
        }

        private static void AddProgressBar(Transform parent, float value, float max)
        {
            var bg = new GameObject("ProgressBar", typeof(RectTransform), typeof(Image), typeof(LayoutElement)); bg.transform.SetParent(parent, false);
            var le = bg.GetComponent<LayoutElement>(); le.preferredHeight = LegacyUITheme.DP(10); le.flexibleWidth = 1;
            bg.GetComponent<Image>().color = LegacyUITheme.GreyBorder;
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)); fill.transform.SetParent(bg.transform, false);
            var rt = fill.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = new Vector2(Mathf.Clamp01(value / max), 1); rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().color = LegacyUITheme.BrassBorder;
        }
    }
}
