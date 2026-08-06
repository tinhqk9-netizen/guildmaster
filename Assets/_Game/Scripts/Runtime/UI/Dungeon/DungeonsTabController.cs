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

namespace GuildMaster.Runtime.UI.Dungeon
{
    /// <summary>
    /// Phase 7 controller for the Dungeons tab. It keeps all internal dungeon screens under
    /// Tab_Dungeons and reads/mutates state only through the existing service interfaces.
    ///
    /// Phase 7 Visual Reconstruction pass: cards are rendered with the generated legacy bordered
    /// sprites (object_border_dim_white / _brass / _unavailable / _extra_opaque, see
    /// LegacyThemeSprites) instead of flat Image+Outline blocks, matching fragment_dungeons.xml /
    /// layout_dungeon.xml / dialog_dungeon_detail.xml / dialog_send_team.xml / dialog_report.xml /
    /// dialog_idle_progress.xml. See Docs/Legacy_Audit/phase_7_full_report.md for the full gap
    /// audit and per-screen mapping.
    /// </summary>
    public sealed class DungeonsTabController : MonoBehaviour
    {
        private enum Screen { Hub, Detail, Team, Active, Report, Loot }

        private static readonly string[] DungeonOrder =
        {
            "enchanted_forest", "the_desert", "eternal_battlefield", "the_golden_city",
            "blackwater_port", "frostbite_peaks", "obsidian_mines", "the_southern_grove",
            "barren_wastelands", "hidden_city_of_larox", "lost_lands"
        };

        private ServiceContainer _services;
        private GameObject _root;
        private RectTransform _content;
        private Screen _screen;
        private DungeonDefinition _selectedDungeon;
        private int _partyIndex;
        private bool _initialized;

        public void Setup(ServiceContainer services)
        {
            _services = services;
            _initialized = services != null && services.Dungeon != null && services.Database != null;
            if (!_initialized) return;
            var placeholder = GetComponent<TabPlaceholderView>();
            if (placeholder != null) placeholder.enabled = false;
            var legacyLabel = transform.Find("Label");
            if (legacyLabel != null) Destroy(legacyLabel.gameObject);
            BuildRoot();
            ShowHub();
        }

        public void Refresh()
        {
            if (!_initialized) return;
            switch (_screen)
            {
                case Screen.Detail: ShowDetail(_selectedDungeon); break;
                case Screen.Team: ShowTeam(_selectedDungeon); break;
                case Screen.Active: ShowActive(); break;
                case Screen.Report: ShowReport(); break;
                case Screen.Loot: ShowLoot(); break;
                default: ShowHub(); break;
            }
        }

        private void BuildRoot()
        {
            if (_root != null) Destroy(_root);
            _root = new GameObject("Phase7DungeonsContent", typeof(RectTransform), typeof(Image));
            _root.transform.SetParent(transform, false);
            var rootRt = _root.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero; rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero; rootRt.offsetMax = Vector2.zero;
            _root.GetComponent<Image>().color = LegacyUITheme.CardviewDarkBackground;

            var scrollGo = new GameObject("DungeonScroll", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
            scrollGo.transform.SetParent(_root.transform, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero; scrollRt.anchorMax = Vector2.one;
            int margin = LegacyUITheme.DP(12);
            scrollRt.offsetMin = new Vector2(margin, LegacyUITheme.DP(16));
            scrollRt.offsetMax = new Vector2(-margin, -LegacyUITheme.DP(48));

            var bodyGo = new GameObject("DungeonContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            bodyGo.transform.SetParent(scrollGo.transform, false);
            _content = bodyGo.GetComponent<RectTransform>();
            _content.anchorMin = new Vector2(0, 1); _content.anchorMax = new Vector2(1, 1);
            _content.pivot = new Vector2(.5f, 1); _content.sizeDelta = Vector2.zero;
            var layout = bodyGo.GetComponent<VerticalLayoutGroup>();
            layout.spacing = LegacyUITheme.DP(8);
            int pad = LegacyUITheme.DP(4);
            layout.padding = new RectOffset(pad, pad, pad, pad);
            layout.childControlWidth = true; layout.childControlHeight = true;
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            bodyGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.viewport = scrollRt; scroll.content = _content;
        }

        private void ClearContent()
        {
            if (_content == null) return;
            for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);
        }

        private List<DungeonDefinition> GetAllDungeons()
        {
            var all = _services.Database.GetAll<DungeonDefinition>()?.ToList() ?? new List<DungeonDefinition>();
            var ordered = new List<DungeonDefinition>();
            foreach (var id in DungeonOrder)
            {
                var match = all.FirstOrDefault(x => string.Equals(x.id, id, StringComparison.OrdinalIgnoreCase));
                if (match != null) ordered.Add(match);
            }
            ordered.AddRange(all.Where(x => !ordered.Contains(x)));
            return ordered;
        }

        private ExpeditionRuntime GetActiveExpedition()
        {
            return _services.Dungeon.GetExpedition(_partyIndex);
        }

        private bool IsActive(DungeonDefinition dungeon)
        {
            var active = GetActiveExpedition()?.Dungeon;
            return active?.Definition != null && string.Equals(active.Definition.id, dungeon?.id, StringComparison.OrdinalIgnoreCase);
        }

        // ================================================================
        // 7A — Dungeons Hub (fragment_dungeons.xml / layout_dungeon.xml)
        // ================================================================
        private void ShowHub()
        {
            _screen = Screen.Hub; ClearContent();
            AddText(_content, "Header", "DUNGEONS", 26, LegacyUITheme.BrassBorder, 52, TextAnchor.MiddleLeft, true);
            AddText(_content, "Subheader", "Select an area to view its status and send a party.", 14, LegacyUITheme.DimWhite, 32, TextAnchor.MiddleLeft, true);

            var active = GetActiveExpedition();
            if (active?.Dungeon?.PendingDrops?.Count > 0)
                AddAction(_content, "Loot", "LOOT AVAILABLE  •  " + Format(active.Dungeon.Definition?.id),
                    "Collect pending chest items", "object_border_brass", () => { _selectedDungeon = active.Dungeon.Definition; ShowLoot(); });
            else if (active?.Dungeon != null)
                AddAction(_content, "ActiveRun", "ACTIVE RUN  •  " + Format(active.Dungeon.Definition?.id),
                    "View current expedition progress", "object_border_brass", () => { _selectedDungeon = active.Dungeon.Definition; ShowActive(); });

            foreach (var dungeon in GetAllDungeons())
            {
                bool unlocked = _services.Dungeon.IsDungeonUnlocked(dungeon.id);
                bool activeDungeon = IsActive(dungeon);
                BuildHubCard(dungeon, unlocked, activeDungeon);
            }
            if (GetAllDungeons().Count == 0) AddText(_content, "Empty", "No dungeon definitions are available.", 16, LegacyUITheme.DimWhite, 54, TextAnchor.MiddleCenter);
        }

        private void BuildHubCard(DungeonDefinition dungeon, bool unlocked, bool activeDungeon)
        {
            // container_dungeon: fixed-height bordered card, banner right-docked, title top-left.
            var card = new GameObject("Dungeon_" + dungeon.id, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            card.transform.SetParent(_content, false);
            var cardLayout = card.GetComponent<LayoutElement>();
            cardLayout.preferredHeight = LegacyUITheme.DP(84); cardLayout.flexibleWidth = 1;
            var cardImage = card.GetComponent<Image>();
            cardImage.sprite = BorderSprite(unlocked ? "object_border_dim_white" : "object_border_dim_white_unavailable");
            cardImage.type = cardImage.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            cardImage.color = cardImage.sprite != null ? Color.white : (unlocked ? LegacyUITheme.StandardBackground : LegacyUITheme.StandardBackgroundUnavailable);
            var button = card.GetComponent<Button>();
            button.targetGraphic = cardImage;
            button.interactable = true; // whole card is a hit target — locked cards still open Detail to show the unlock requirement
            button.onClick.AddListener(() =>
            {
                // Backend-aligned routing: never leave the player guessing which state a
                // dungeon is in — jump straight to the screen that state implies.
                if (!unlocked) { ShowDetail(dungeon); return; }
                var expedition = GetActiveExpedition();
                bool isThisDungeonActive = expedition?.Dungeon?.Definition != null
                    && string.Equals(expedition.Dungeon.Definition.id, dungeon.id, StringComparison.OrdinalIgnoreCase);
                if (isThisDungeonActive && expedition.Dungeon.PendingDrops?.Count > 0) { _selectedDungeon = dungeon; ShowLoot(); }
                else if (isThisDungeonActive) { _selectedDungeon = dungeon; ShowActive(); }
                else { ShowDetail(dungeon); }
            });

            // Banner: right-docked, alpha-treated when locked.
            var banner = new GameObject("Banner", typeof(RectTransform), typeof(Image));
            banner.transform.SetParent(card.transform, false);
            var bannerRt = banner.GetComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(1, 0); bannerRt.anchorMax = new Vector2(1, 1);
            bannerRt.pivot = new Vector2(1, 0.5f);
            float bannerWidth = LegacyUITheme.DP(84) * 1.8f;
            bannerRt.sizeDelta = new Vector2(bannerWidth, 0);
            bannerRt.anchoredPosition = new Vector2(-LegacyUITheme.DP(1), 0);
            var bannerImage = banner.GetComponent<Image>();
            bannerImage.sprite = LegacySpriteRegistry.GetSprite("area_" + dungeon.id) ?? LegacySpriteRegistry.GetSprite("summary_" + dungeon.id);
            bannerImage.preserveAspect = false;
            bannerImage.raycastTarget = false;
            var bannerColor = Color.white; bannerColor.a = unlocked ? 0.7f : 0.35f;
            bannerImage.color = bannerColor;

            // Title, top-left.
            var title = new GameObject("Title", typeof(RectTransform), typeof(Text));
            title.transform.SetParent(card.transform, false);
            var titleRt = title.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1); titleRt.anchorMax = new Vector2(0.62f, 1); titleRt.pivot = new Vector2(0, 1);
            titleRt.anchoredPosition = new Vector2(LegacyUITheme.DP(12), -LegacyUITheme.DP(8));
            titleRt.sizeDelta = new Vector2(0, LegacyUITheme.DP(28));
            var titleText = title.GetComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 20; titleText.fontStyle = FontStyle.Bold;
            titleText.color = unlocked ? LegacyUITheme.DimWhite : LegacyUITheme.GreyBorder;
            titleText.text = Format(dungeon.id);
            titleText.alignment = TextAnchor.UpperLeft; titleText.raycastTarget = false;
            titleText.horizontalOverflow = HorizontalWrapMode.Wrap;

            // Status/progress line under the title.
            string statusLabel = !unlocked ? "LOCKED" : activeDungeon ? "ACTIVE" : GetClearText(dungeon.id);
            var status = new GameObject("Status", typeof(RectTransform), typeof(Text));
            status.transform.SetParent(card.transform, false);
            var statusRt = status.GetComponent<RectTransform>();
            statusRt.anchorMin = new Vector2(0, 1); statusRt.anchorMax = new Vector2(0.62f, 1); statusRt.pivot = new Vector2(0, 1);
            statusRt.anchoredPosition = new Vector2(LegacyUITheme.DP(12), -LegacyUITheme.DP(30));
            statusRt.sizeDelta = new Vector2(0, LegacyUITheme.DP(18));
            var statusText = status.GetComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 12; statusText.fontStyle = FontStyle.Bold;
            statusText.color = !unlocked ? LegacyUITheme.Failure : activeDungeon ? LegacyUITheme.BrassBorder : LegacyUITheme.DimWhite;
            statusText.text = statusLabel;
            statusText.alignment = TextAnchor.UpperLeft; statusText.raycastTarget = false;

            // Encounter/clear summary line.
            var summary = new GameObject("Summary", typeof(RectTransform), typeof(Text));
            summary.transform.SetParent(card.transform, false);
            var summaryRt = summary.GetComponent<RectTransform>();
            summaryRt.anchorMin = new Vector2(0, 1); summaryRt.anchorMax = new Vector2(0.62f, 1); summaryRt.pivot = new Vector2(0, 1);
            summaryRt.anchoredPosition = new Vector2(LegacyUITheme.DP(12), -LegacyUITheme.DP(48));
            summaryRt.sizeDelta = new Vector2(0, LegacyUITheme.DP(30));
            var summaryText = summary.GetComponent<Text>();
            summaryText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            summaryText.fontSize = 11;
            summaryText.color = LegacyUITheme.DimWhite;
            summaryText.text = !unlocked ? GetUnlockText(dungeon)
                : (dungeon.EnemyIds?.Count ?? 0) > 0 ? dungeon.EnemyIds.Count + " encounter types" : "Encounter data unavailable";
            summaryText.alignment = TextAnchor.UpperLeft; summaryText.raycastTarget = false;
            summaryText.horizontalOverflow = HorizontalWrapMode.Wrap; summaryText.verticalOverflow = VerticalWrapMode.Truncate;

            // Active-run progress bar across the bottom of the left text column.
            if (activeDungeon)
            {
                var active = GetActiveExpedition()?.Dungeon;
                if (active != null)
                {
                    var barGo = new GameObject("ActionProgress", typeof(RectTransform), typeof(Image));
                    barGo.transform.SetParent(card.transform, false);
                    var barRt = barGo.GetComponent<RectTransform>();
                    barRt.anchorMin = new Vector2(0, 0); barRt.anchorMax = new Vector2(0.62f, 0); barRt.pivot = new Vector2(0, 0);
                    barRt.anchoredPosition = new Vector2(LegacyUITheme.DP(12), LegacyUITheme.DP(8));
                    barRt.sizeDelta = new Vector2(-LegacyUITheme.DP(12), LegacyUITheme.DP(6));
                    barGo.GetComponent<Image>().color = LegacyUITheme.GreyBorder;
                    var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                    fill.transform.SetParent(barGo.transform, false);
                    var fillRt = fill.GetComponent<RectTransform>();
                    float pct = Mathf.Clamp01((float)active.Progress / Math.Max(1, active.MaxProgress));
                    fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(pct, 1); fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
                    fill.GetComponent<Image>().color = LegacyUITheme.BrassBorder;
                }
            }
            else if (!unlocked)
            {
                // Lock overlay badge — real icon/text treatment, not just a dimmed banner.
                var lockBadge = new GameObject("LockBadge", typeof(RectTransform), typeof(Image));
                lockBadge.transform.SetParent(card.transform, false);
                var lockRt = lockBadge.GetComponent<RectTransform>();
                lockRt.anchorMin = new Vector2(1, 0.5f); lockRt.anchorMax = new Vector2(1, 0.5f); lockRt.pivot = new Vector2(1, 0.5f);
                lockRt.sizeDelta = new Vector2(LegacyUITheme.DP(44), LegacyUITheme.DP(20));
                lockRt.anchoredPosition = new Vector2(-LegacyUITheme.DP(6), 0);
                var lockImg = lockBadge.GetComponent<Image>();
                lockImg.sprite = BorderSprite("object_border_dim_white_extra_opaque");
                lockImg.type = lockImg.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
                lockImg.color = lockImg.sprite != null ? Color.white : new Color(0, 0, 0, 0.6f);
                var lockText = new GameObject("Text", typeof(RectTransform), typeof(Text));
                lockText.transform.SetParent(lockBadge.transform, false);
                var ltRt = lockText.GetComponent<RectTransform>();
                ltRt.anchorMin = Vector2.zero; ltRt.anchorMax = Vector2.one; ltRt.offsetMin = Vector2.zero; ltRt.offsetMax = Vector2.zero;
                var lt = lockText.GetComponent<Text>();
                lt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                lt.fontSize = 10; lt.fontStyle = FontStyle.Bold; lt.color = LegacyUITheme.DimWhite;
                lt.text = "LOCKED"; lt.alignment = TextAnchor.MiddleCenter; lt.raycastTarget = false;
            }
        }

        // ================================================================
        // 7B — Dungeon Detail (dialog_dungeon_detail.xml)
        // ================================================================
        private void ShowDetail(DungeonDefinition dungeon)
        {
            if (dungeon == null) { ShowHub(); return; }
            _selectedDungeon = dungeon; _screen = Screen.Detail; ClearContent();
            AddText(_content, "Header", Format(dungeon.id), 24, LegacyUITheme.BrassBorder, 48, TextAnchor.MiddleLeft, true);

            // container_fight banner frame (object_border_no_background — border only, no fill).
            var bannerFrame = new GameObject("BannerFrame", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            bannerFrame.transform.SetParent(_content, false);
            bannerFrame.GetComponent<LayoutElement>().preferredHeight = LegacyUITheme.DP(120);
            var frameImage = bannerFrame.GetComponent<Image>();
            frameImage.sprite = BorderSprite("object_border_no_background");
            frameImage.type = frameImage.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            frameImage.color = frameImage.sprite != null ? Color.white : Color.clear;
            var banner = new GameObject("Banner", typeof(RectTransform), typeof(Image));
            banner.transform.SetParent(bannerFrame.transform, false);
            var bannerRt = banner.GetComponent<RectTransform>();
            int inset = LegacyUITheme.DP(3);
            bannerRt.anchorMin = Vector2.zero; bannerRt.anchorMax = Vector2.one;
            bannerRt.offsetMin = new Vector2(inset, inset); bannerRt.offsetMax = new Vector2(-inset, -inset);
            var bannerImage = banner.GetComponent<Image>();
            bannerImage.sprite = LegacySpriteRegistry.GetSprite("area_" + dungeon.id) ?? LegacySpriteRegistry.GetSprite("summary_" + dungeon.id);
            bannerImage.preserveAspect = false; bannerImage.raycastTarget = false;
            var bc = Color.white; bc.a = 0.6f; bannerImage.color = bc;

            AddText(_content, "Status", GetDetailStatus(dungeon), 15, LegacyUITheme.DimWhite, 36, TextAnchor.MiddleLeft, true);
            AddText(_content, "Encounters", "Encounters: " + ((dungeon.EnemyIds?.Count ?? 0) > 0 ? string.Join(", ", dungeon.EnemyIds.Take(5).Select(Format)) : "Not exposed by backend"), 14, LegacyUITheme.DimWhite, 44, TextAnchor.MiddleLeft, true);
            AddText(_content, "Progress", GetClearText(dungeon.id), 14, LegacyUITheme.DimWhite, 32, TextAnchor.MiddleLeft, true);

            // Exactly one primary (brass) action per state — the obvious next step, never a
            // pile of equally weighted buttons.
            var active = GetActiveExpedition();
            bool locked = !_services.Dungeon.IsDungeonUnlocked(dungeon.id);
            bool hasActivity = GetClearCount(dungeon.id) > 0 || IsActive(dungeon);

            if (locked)
            {
                AddText(_content, "Locked", GetUnlockText(dungeon), 14, LegacyUITheme.DimWhite, 42, TextAnchor.MiddleLeft, true);
            }
            else if (IsActive(dungeon))
            {
                if (active?.Dungeon?.PendingDrops?.Count > 0)
                    AddAction(_content, "ViewLoot", "VIEW LOOT", "Collect pending drops", "object_border_brass", () => ShowLoot());
                else
                    AddAction(_content, "Continue", "CONTINUE EXPEDITION", "Open the live expedition state", "object_border_brass", () => ShowActive());
            }
            else
            {
                var partyMembers = _services.Party.GetPartyMembers(_partyIndex);
                bool canStartNow = partyMembers.Count > 0 && _services.Dungeon.GetExpedition(_partyIndex) == null;
                if (canStartNow)
                {
                    AddAction(_content, "Start", "START EXPEDITION", "Party " + partyMembers.Count + "/" + _services.Party.MaxPartySize + " ready", "object_border_brass", () => StartDungeon());
                    AddAction(_content, "Team", "TEAM SETUP", "Change the assigned party", "object_border_dim_white", () => ShowTeam(dungeon));
                }
                else
                {
                    AddAction(_content, "Team", "SET UP TEAM", "Assign adventurers before starting", "object_border_brass", () => ShowTeam(dungeon));
                }
            }

            // Report is only offered when there is something to summarize — no dead-end
            // limitation screen reachable from a dungeon that was never attempted.
            if (hasActivity)
                AddAction(_content, "Report", "EXPEDITION SUMMARY", "Progress and party for this expedition", "object_border_dim_white", () => ShowReport());
            AddAction(_content, "Back", "BACK TO DUNGEONS", "", "object_border_dim_white", ShowHub);
        }

        // ================================================================
        // 7C — Team Setup (dialog_send_team.xml)
        // ================================================================
        private void ShowTeam(DungeonDefinition dungeon)
        {
            if (dungeon == null) { ShowHub(); return; }
            _selectedDungeon = dungeon; _screen = Screen.Team; ClearContent();
            AddText(_content, "Header", "TEAM SETUP", 24, LegacyUITheme.BrassBorder, 48, TextAnchor.MiddleLeft, true);
            AddText(_content, "Dungeon", "Dungeon: " + Format(dungeon.id), 15, LegacyUITheme.DimWhite, 34, TextAnchor.MiddleLeft, true);
            AddText(_content, "Party", "Party " + (_partyIndex + 1) + "  •  " + _services.Party.GetPartyMembers(_partyIndex).Count + "/" + _services.Party.MaxPartySize, 15, LegacyUITheme.DimWhite, 34, TextAnchor.MiddleLeft, true);
            var members = _services.Party.GetPartyMembers(_partyIndex).ToList();
            for (int i = 0; i < _services.Party.MaxPartySize; i++)
            {
                string id = i < members.Count ? members[i] : null;
                if (id == null)
                {
                    // Real empty-slot visual: dashed/no-background bordered row, not a bare grey block.
                    AddAction(_content, "Slot_" + i, "EMPTY SLOT", "Add an adventurer from the list below", "object_border_no_background", null, false);
                }
                else
                {
                    var character = _services.Character.GetAllCharacters().FirstOrDefault(x => x.InstanceId == id);
                    var row = AddAction(_content, "Member_" + i, FormatCharacter(character), "Tap to remove from party", "object_border_brass", () => RemoveMember(id));
                    AddPortrait(row.transform, character);
                }
            }
            AddText(_content, "Available", "AVAILABLE ADVENTURERS", 16, LegacyUITheme.BrassBorder, 36, TextAnchor.MiddleLeft, true);
            foreach (var character in _services.Character.GetAllCharacters())
            {
                bool selected = members.Contains(character.InstanceId);
                bool unavailable = _services.Party.IsInAnyParty(character.InstanceId) && !selected;
                string sub = selected ? "Already assigned" : unavailable ? "Assigned to another party" : "Add to party";
                var row = AddAction(_content, "Available_" + character.InstanceId, FormatCharacter(character), sub,
                    selected || unavailable ? "object_border_dim_white_unavailable" : "object_border_dim_white",
                    () => AddMember(character.InstanceId));
                if (selected || unavailable) row.GetComponent<Button>().interactable = false;
                AddPortrait(row.transform, character);
            }
            bool alreadyDispatched = _services.Dungeon.GetExpedition(_partyIndex) != null;
            bool canStart = members.Count > 0 && !alreadyDispatched && _services.Dungeon.IsDungeonUnlocked(dungeon.id);
            string startReason = canStart ? "Dispatch this party"
                : alreadyDispatched ? "This party is already on an expedition."
                : members.Count == 0 ? "Assign at least one adventurer."
                : "This dungeon is locked.";
            AddAction(_content, "Start", "START EXPEDITION", startReason, canStart ? "object_border_brass" : "object_border_dim_white_unavailable", () => StartDungeon(), canStart);
            AddAction(_content, "Back", "BACK TO DETAIL", "", "object_border_dim_white", () => ShowDetail(_selectedDungeon));
        }

        // ================================================================
        // 7D — Active Run
        // ================================================================
        private void ShowActive()
        {
            _screen = Screen.Active; ClearContent();
            var exp = GetActiveExpedition();
            if (exp?.Dungeon == null) { ShowHub(); return; }
            var dungeon = exp.Dungeon;
            AddText(_content, "Header", "ACTIVE DUNGEON", 24, LegacyUITheme.BrassBorder, 48, TextAnchor.MiddleLeft, true);
            AddText(_content, "Dungeon", Format(dungeon.Definition?.id), 18, LegacyUITheme.DimWhite, 34, TextAnchor.MiddleLeft, true);
            AddText(_content, "Progress", "Progress: " + dungeon.Progress + " / " + dungeon.MaxProgress, 16, LegacyUITheme.DimWhite, 34, TextAnchor.MiddleLeft, true);
            AddProgressBar(_content, dungeon.Progress, Math.Max(1, dungeon.MaxProgress));
            AddText(_content, "Action", "Current action: " + FormatAction(dungeon.ActionType), 15, LegacyUITheme.DimWhite, 34, TextAnchor.MiddleLeft, true);

            // Party strip: portrait avatars instead of a plain comma-joined name list.
            var strip = new GameObject("PartyStrip", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            strip.transform.SetParent(_content, false);
            strip.GetComponent<LayoutElement>().preferredHeight = LegacyUITheme.DP(48);
            var stripLayout = strip.GetComponent<HorizontalLayoutGroup>();
            stripLayout.spacing = LegacyUITheme.DP(6); stripLayout.childControlWidth = false; stripLayout.childControlHeight = true;
            stripLayout.childForceExpandWidth = false; stripLayout.childForceExpandHeight = true;
            foreach (var member in exp.Party) AddPortrait(strip.transform, member);

            AddText(_content, "Encounter", "Enemies in encounter: " + (dungeon.Enemies?.Count ?? 0), 14, LegacyUITheme.DimWhite, 32, TextAnchor.MiddleLeft, true);

            // One clear primary action: loot takes priority over recall when drops are waiting.
            if (dungeon.PendingDrops?.Count > 0)
                AddAction(_content, "Loot", "VIEW LOOT", "Collect " + dungeon.PendingDrops.Count + " pending item(s)", "object_border_brass", ShowLoot);
            else
                // Recall is an enabled, real action — it must not use the "unavailable" red-tint semantics.
                AddAction(_content, "Recall", "RECALL EXPEDITION", "Stop through DungeonService", "object_border_dim_white", RecallDungeon);
            AddAction(_content, "Report", "EXPEDITION SUMMARY", "Progress and party for this expedition", "object_border_dim_white", ShowReport);
            AddAction(_content, "Back", "BACK TO DUNGEONS", "", "object_border_dim_white", ShowHub);
        }

        // ================================================================
        // 7E — Expedition Summary (secondary, informational only — dialog_report.xml scaffold).
        // Only reachable when the dungeon has real activity to summarize (see hasActivity in
        // ShowDetail / always-available from Active). Shows only data the backend actually has:
        // dungeon, progress, and party. No combat statistics are fabricated.
        // ================================================================
        private void ShowReport()
        {
            _screen = Screen.Report; ClearContent();
            var dungeon = _selectedDungeon ?? GetActiveExpedition()?.Dungeon?.Definition;
            AddText(_content, "Header", "EXPEDITION SUMMARY", 24, LegacyUITheme.BrassBorder, 48, TextAnchor.MiddleLeft, true);
            AddText(_content, "Dungeon", dungeon != null ? Format(dungeon.id) : "Unknown dungeon", 18, LegacyUITheme.DimWhite, 36, TextAnchor.MiddleLeft, true);

            var active = GetActiveExpedition();
            bool isThisActive = dungeon != null && IsActive(dungeon);
            if (isThisActive && active?.Dungeon != null)
            {
                AddText(_content, "Progress", "Progress: " + active.Dungeon.Progress + " / " + active.Dungeon.MaxProgress, 15, LegacyUITheme.BrassBorder, 32, TextAnchor.MiddleLeft, true).fontStyle = FontStyle.Bold;
                var strip = new GameObject("PartyStrip", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                strip.transform.SetParent(_content, false);
                strip.GetComponent<LayoutElement>().preferredHeight = LegacyUITheme.DP(48);
                var stripLayout = strip.GetComponent<HorizontalLayoutGroup>();
                stripLayout.spacing = LegacyUITheme.DP(6); stripLayout.childControlWidth = false; stripLayout.childControlHeight = true;
                stripLayout.childForceExpandWidth = false; stripLayout.childForceExpandHeight = true;
                foreach (var member in active.Party) AddPortrait(strip.transform, member);
            }
            else if (dungeon != null)
            {
                AddText(_content, "Progress", GetClearText(dungeon.id), 15, LegacyUITheme.BrassBorder, 32, TextAnchor.MiddleLeft, true).fontStyle = FontStyle.Bold;
            }

            var note = AddText(_content, "Note", "Detailed battle records are not available for this expedition.", 13, LegacyUITheme.GreyBorder, 40, TextAnchor.MiddleLeft, true);
            note.fontStyle = FontStyle.Italic;
            AddAction(_content, "Back", "BACK", "", "object_border_dim_white", BackFromSubscreen);
        }

        // ================================================================
        // 7F — Loot (bordered-cell grid, matches layout_item pattern)
        // ================================================================
        private void ShowLoot()
        {
            _screen = Screen.Loot; ClearContent();
            var dungeon = GetActiveExpedition()?.Dungeon;
            AddText(_content, "Header", "LOOT", 24, LegacyUITheme.BrassBorder, 48, TextAnchor.MiddleLeft, true);
            if (dungeon?.PendingDrops == null || dungeon.PendingDrops.Count == 0)
            {
                var emptyFrame = new GameObject("EmptyFrame", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                emptyFrame.transform.SetParent(_content, false);
                emptyFrame.GetComponent<LayoutElement>().preferredHeight = LegacyUITheme.DP(70);
                var ei = emptyFrame.GetComponent<Image>();
                ei.sprite = BorderSprite("object_border_no_background");
                ei.type = ei.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
                ei.color = ei.sprite != null ? Color.white : Color.clear;
                AddText(emptyFrame.transform, "EmptyText", "No pending loot is available.", 16, LegacyUITheme.DimWhite, LegacyUITheme.DP(70), TextAnchor.MiddleCenter);
            }
            else
            {
                var grid = new GameObject("LootGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
                grid.transform.SetParent(_content, false);
                int rows = Mathf.CeilToInt(dungeon.PendingDrops.Count / 5f);
                int cellSize = LegacyUITheme.DP(66);
                grid.GetComponent<LayoutElement>().preferredHeight = rows * (cellSize + LegacyUITheme.DP(4));
                var gl = grid.GetComponent<GridLayoutGroup>();
                gl.cellSize = new Vector2(cellSize, cellSize);
                gl.spacing = new Vector2(LegacyUITheme.DP(4), LegacyUITheme.DP(4));
                gl.childAlignment = TextAnchor.UpperLeft;
                foreach (var item in dungeon.PendingDrops)
                {
                    var cell = new GameObject("Loot_" + item.InstanceId, typeof(RectTransform), typeof(Image));
                    cell.transform.SetParent(grid.transform, false);
                    var cellImage = cell.GetComponent<Image>();
                    cellImage.sprite = BorderSprite("object_border_dim_white");
                    cellImage.type = cellImage.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
                    cellImage.color = cellImage.sprite != null ? Color.white : LegacyUITheme.StandardBackground;

                    var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                    icon.transform.SetParent(cell.transform, false);
                    var iconRt = icon.GetComponent<RectTransform>();
                    int iconInset = LegacyUITheme.DP(10);
                    iconRt.anchorMin = Vector2.zero; iconRt.anchorMax = new Vector2(1, 1);
                    iconRt.offsetMin = new Vector2(iconInset, iconInset * 1.6f); iconRt.offsetMax = new Vector2(-iconInset, -iconInset);
                    var iconImg = icon.GetComponent<Image>();
                    iconImg.sprite = LegacySpriteRegistry.GetItemSprite(item.Definition?.id);
                    iconImg.preserveAspect = true; iconImg.raycastTarget = false;
                    iconImg.color = iconImg.sprite != null ? Color.white : new Color(1f, 1f, 1f, 0.25f);

                    var qty = new GameObject("Qty", typeof(RectTransform), typeof(Text));
                    qty.transform.SetParent(cell.transform, false);
                    var qtyRt = qty.GetComponent<RectTransform>();
                    qtyRt.anchorMin = new Vector2(0, 0); qtyRt.anchorMax = new Vector2(1, 0); qtyRt.pivot = new Vector2(0.5f, 0);
                    qtyRt.sizeDelta = new Vector2(0, LegacyUITheme.DP(16));
                    qtyRt.anchoredPosition = new Vector2(0, LegacyUITheme.DP(3));
                    var qtyText = qty.GetComponent<Text>();
                    qtyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    qtyText.fontSize = 11; qtyText.fontStyle = FontStyle.Bold; qtyText.color = LegacyUITheme.BrassBorder;
                    qtyText.text = "x" + item.StackCount; qtyText.alignment = TextAnchor.MiddleCenter; qtyText.raycastTarget = false;
                }
                AddAction(_content, "Collect", "COLLECT LOOT", "Use DungeonService.CollectDrops", "object_border_brass", CollectLoot);
            }
            AddAction(_content, "Back", "BACK", "", "object_border_dim_white", BackFromSubscreen);
        }

        // 7G — Idle Progress: retired as a primary/secondary route. No dungeon-specific idle
        // summary or offline-reward model exists in DungeonRuntime/ExpeditionRuntime, so a
        // dedicated screen would only ever show a backend-limitation message with zero gameplay
        // value. Generic offline catch-up (GameLoop/DungeonService) still runs and is reflected
        // directly in Progress/MaxProgress on Active Expedition — no separate UI is needed for it.

        private void StartDungeon()
        {
            var ids = _services.Party.GetPartyMembers(_partyIndex).ToList();
            bool ok = _services.Dungeon.StartExpedition(_partyIndex, _selectedDungeon.id, ids, out string error);
            if (!ok) Debug.LogWarning("[Phase7] Start dungeon rejected: " + error);
            ShowActive();
        }

        private void RecallDungeon() { _services.Dungeon.StopExpedition(_partyIndex); ShowHub(); }
        private void CollectLoot() { _services.Dungeon.CollectDrops(_partyIndex); ShowHub(); }
        private void BackFromSubscreen() { if (_selectedDungeon != null) ShowDetail(_selectedDungeon); else ShowHub(); }
        private void AddMember(string id) { _services.Party.AddToParty(id, _partyIndex); ShowTeam(_selectedDungeon); }
        private void RemoveMember(string id) { _services.Party.RemoveFromParty(id, _partyIndex); ShowTeam(_selectedDungeon); }

        private string GetDetailStatus(DungeonDefinition dungeon)
        {
            if (!_services.Dungeon.IsDungeonUnlocked(dungeon.id)) return "LOCKED  •  " + GetUnlockText(dungeon);
            if (IsActive(dungeon)) return "ACTIVE  •  Expedition in progress";
            return "UNLOCKED  •  Ready for team setup";
        }

        private string GetUnlockText(DungeonDefinition dungeon)
        {
            if (string.IsNullOrEmpty(dungeon.RequiredClearDungeonId)) return "Dungeon is locked by backend state.";
            return "Requires clear: " + Format(dungeon.RequiredClearDungeonId) + " (progress " + dungeon.RequiredClearProgress + ")";
        }

        private string GetClearText(string id)
        {
            var save = _services.Save.CurrentData?.Dungeons?.FirstOrDefault(x => string.Equals(x.DefinitionId, id, StringComparison.OrdinalIgnoreCase));
            return save == null ? "NOT CLEARED" : "CLEARED " + save.ClearCount + " TIME(S)";
        }

        private int GetClearCount(string id)
        {
            var save = _services.Save.CurrentData?.Dungeons?.FirstOrDefault(x => string.Equals(x.DefinitionId, id, StringComparison.OrdinalIgnoreCase));
            return save?.ClearCount ?? 0;
        }

        private static string FormatAction(int action)
        {
            switch (action)
            {
                case 0: return "Entering dungeon";
                case 1: return "Entering new room";
                case 2: return "Fighting enemies";
                case 3: return "Collecting loot";
                case 4: return "Searching next room";
                case 5: return "Party recovering";
                case 6: return "Retreating";
                default: return "Exploring";
            }
        }

        private static string Format(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "Unknown";
            return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(id.Replace("_", " "));
        }

        private static string FormatCharacter(CharacterRuntime c)
        {
            return c == null ? "Unknown adventurer" : Format(c.Definition?.id ?? c.DefinitionId) + "  Lv." + c.Level;
        }

        private static Sprite BorderSprite(string key) => LegacyThemeSprites.Get(key);

        /// <summary>
        /// Bordered action row using the generated legacy sprites (object_border_*) instead of a
        /// flat Image+Outline block. borderKey selects the variant: object_border_dim_white
        /// (default), object_border_brass (prominent/primary), object_border_dim_white_unavailable
        /// (locked/blocked), object_border_no_background (border-only, e.g. empty slots).
        /// </summary>
        private static GameObject AddAction(Transform parent, string name, string title, string subtitle, string borderKey, Action click, bool interactable = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(parent, false);
            var layout = go.GetComponent<LayoutElement>(); layout.preferredHeight = LegacyUITheme.DP(58); layout.flexibleWidth = 1;
            var image = go.GetComponent<Image>();
            image.sprite = BorderSprite(borderKey);
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

        private static Text AddText(Transform parent, string name, string value, int size, Color color, float height, TextAnchor alignment, bool flexible = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>(); le.preferredHeight = height; le.flexibleWidth = flexible ? 1 : 0;
            var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.color = color; text.text = value; text.alignment = alignment; text.raycastTarget = false; text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Image CreateImage(Transform parent, string name, int size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>(); le.preferredWidth = size; le.preferredHeight = size; le.flexibleWidth = 0;
            var image = go.GetComponent<Image>(); image.color = LegacyUITheme.DimWhite; image.raycastTarget = false; return image;
        }

        private static void AddPortrait(Transform parent, CharacterRuntime character)
        {
            var image = CreateImage(parent, "Portrait", LegacyUITheme.DP(44));
            image.sprite = LegacySpriteRegistry.GetUnitSprite(character?.Definition?.id); image.preserveAspect = true;
        }

        private static void AddProgressBar(Transform parent, int value, int max)
        {
            var bg = new GameObject("ProgressBar", typeof(RectTransform), typeof(Image), typeof(LayoutElement)); bg.transform.SetParent(parent, false);
            var le = bg.GetComponent<LayoutElement>(); le.preferredHeight = LegacyUITheme.DP(10); le.flexibleWidth = 1;
            bg.GetComponent<Image>().color = LegacyUITheme.GreyBorder;
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)); fill.transform.SetParent(bg.transform, false);
            var rt = fill.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = new Vector2(Mathf.Clamp01((float)value / max), 1); rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().color = LegacyUITheme.BrassBorder;
        }
    }
}
