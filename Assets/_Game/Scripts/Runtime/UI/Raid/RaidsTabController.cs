using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Legacy;
using GuildMaster.Runtime.UI.Shell;

namespace GuildMaster.Runtime.UI.Raid
{
    /// <summary>
    /// Phase 8 controller for the Raids tab. Mirrors the structure/conventions of
    /// GuildMaster.Runtime.UI.Dungeon.DungeonsTabController (Phase 7).
    ///
    /// Backend audit (see Docs/Legacy_Audit/phase_8_full_report.md for the full table): the
    /// backend exposes RaidDefinition records (12, from Assets/StreamingAssets/GameData/raids.json)
    /// with only id/className/RequiredClearDungeonId/RequiredClearProgress — no RaidService, no
    /// RaidRuntime, no raid save state, no raid-specific team/attempt/reward system exists anywhere
    /// in Assets/_Game/Scripts/Runtime/Services or Runtime/Save/SaveData.cs. The only real,
    /// backend-truthful raid feature is therefore: list the definitions and compute their
    /// lock/unlock state exactly the way DungeonService.IsDungeonUnlocked does it (same
    /// RequiredClearDungeonId/RequiredClearProgress fields, checked against
    /// SaveData.Dungeons[].MaxProgress). Everything past that — team setup, starting a raid, an
    /// active-raid state, a summary, or rewards — has zero backend support, so those screens are
    /// never built; Detail instead shows one honest, legacy-styled fallback note. No gameplay,
    /// timers, attempts, or rewards are fabricated anywhere in this controller.
    /// </summary>
    public sealed class RaidsTabController : MonoBehaviour
    {
        private enum Screen { Hub, Detail }

        private ServiceContainer _services;
        private GameObject _root;
        private RectTransform _content;
        private Screen _screen;
        private RaidDefinition _selectedRaid;
        private bool _initialized;

        public void Setup(ServiceContainer services)
        {
            _services = services;
            _initialized = services != null && services.Database != null && services.Save != null;
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
                case Screen.Detail: ShowDetail(_selectedRaid); break;
                default: ShowHub(); break;
            }
        }

        private void BuildRoot()
        {
            if (_root != null) Destroy(_root);
            _root = new GameObject("Phase8RaidsContent", typeof(RectTransform), typeof(Image));
            _root.transform.SetParent(transform, false);
            var rootRt = _root.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero; rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero; rootRt.offsetMax = Vector2.zero;
            _root.GetComponent<Image>().color = LegacyUITheme.CardviewDarkBackground;

            var scrollGo = new GameObject("RaidScroll", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
            scrollGo.transform.SetParent(_root.transform, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero; scrollRt.anchorMax = Vector2.one;
            int margin = LegacyUITheme.DP(12);
            scrollRt.offsetMin = new Vector2(margin, LegacyUITheme.DP(16));
            scrollRt.offsetMax = new Vector2(-margin, -LegacyUITheme.DP(48));

            var bodyGo = new GameObject("RaidContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
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

        private List<RaidDefinition> GetAllRaids()
        {
            var all = _services.Database.GetAll<RaidDefinition>()?.ToList() ?? new List<RaidDefinition>();
            // No legacy ordering data exists for raids (unlike DungeonOrder, which reflects a known
            // navigation sequence) — sort alphabetically by display name for a stable, honest order.
            return all.OrderBy(r => Format(r.id), StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// Same unlock rule as DungeonService.IsDungeonUnlocked (RequiredClearDungeonId /
        /// RequiredClearProgress checked against SaveData.Dungeons[].MaxProgress) — there is no
        /// RaidService to call, but the RaidDefinition fields are identical in shape and the
        /// underlying save data (cleared dungeon progress) is real, so this reuses the exact same
        /// logic rather than inventing a different one.
        /// </summary>
        private bool IsRaidUnlocked(RaidDefinition raid)
        {
            if (raid == null) return false;
            if (string.IsNullOrEmpty(raid.RequiredClearDungeonId)) return true;
            var clearedDungeons = _services.Save.CurrentData?.Dungeons;
            return clearedDungeons != null && clearedDungeons.Any(d =>
                string.Equals(d.DefinitionId, raid.RequiredClearDungeonId, StringComparison.OrdinalIgnoreCase) &&
                d.MaxProgress >= raid.RequiredClearProgress);
        }

        // ================================================================
        // 8A — Raids Hub
        // ================================================================
        private void ShowHub()
        {
            _screen = Screen.Hub; ClearContent();
            AddText(_content, "Header", "RAIDS", 26, LegacyUITheme.BrassBorder, 52, TextAnchor.MiddleLeft, true);
            AddText(_content, "Subheader", "Select a raid to inspect its room sequence and start a run.", 13, LegacyUITheme.DimWhite, 40, TextAnchor.MiddleLeft, true);

            var raids = GetAllRaids();
            if (raids.Count == 0)
            {
                var emptyFrame = new GameObject("EmptyFrame", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                emptyFrame.transform.SetParent(_content, false);
                emptyFrame.GetComponent<LayoutElement>().preferredHeight = LegacyUITheme.DP(70);
                var ei = emptyFrame.GetComponent<Image>();
                ei.sprite = BorderSprite("object_border_no_background");
                ei.type = ei.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
                ei.color = ei.sprite != null ? Color.white : Color.clear;
                AddText(emptyFrame.transform, "EmptyText", "No raids are currently available.", 16, LegacyUITheme.DimWhite, LegacyUITheme.DP(70), TextAnchor.MiddleCenter);
                return;
            }

            foreach (var raid in raids)
            {
                bool unlocked = IsRaidUnlocked(raid);
                BuildHubCard(raid, unlocked);
            }
        }

        private void BuildHubCard(RaidDefinition raid, bool unlocked)
        {
            var card = new GameObject("Raid_" + raid.id, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            card.transform.SetParent(_content, false);
            var cardLayout = card.GetComponent<LayoutElement>();
            cardLayout.preferredHeight = LegacyUITheme.DP(84); cardLayout.flexibleWidth = 1;
            var cardImage = card.GetComponent<Image>();
            cardImage.sprite = BorderSprite(unlocked ? "object_border_dim_white" : "object_border_dim_white_unavailable");
            cardImage.type = cardImage.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            cardImage.color = cardImage.sprite != null ? Color.white : (unlocked ? LegacyUITheme.StandardBackground : LegacyUITheme.StandardBackgroundUnavailable);
            var button = card.GetComponent<Button>();
            button.targetGraphic = cardImage;
            button.interactable = true; // both states route to Detail — locked shows the requirement, unlocked shows the honest "not available yet" note
            button.onClick.AddListener(() => ShowDetail(raid));

            // Banner: only generic raid art exists (no per-raid banner asset), reused honestly for
            // every card rather than fabricating unique art.
            var banner = new GameObject("Banner", typeof(RectTransform), typeof(Image));
            banner.transform.SetParent(card.transform, false);
            var bannerRt = banner.GetComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(1, 0); bannerRt.anchorMax = new Vector2(1, 1);
            bannerRt.pivot = new Vector2(1, 0.5f);
            float bannerWidth = LegacyUITheme.DP(84) * 1.8f;
            bannerRt.sizeDelta = new Vector2(bannerWidth, 0);
            bannerRt.anchoredPosition = new Vector2(-LegacyUITheme.DP(1), 0);
            var bannerImage = banner.GetComponent<Image>();
            bannerImage.sprite = LegacySpriteRegistry.GetSprite("epic_raid");
            bannerImage.preserveAspect = false;
            bannerImage.raycastTarget = false;
            var bannerColor = Color.white; bannerColor.a = unlocked ? 0.7f : 0.35f;
            bannerImage.color = bannerColor;

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
            titleText.text = Format(raid.id);
            titleText.alignment = TextAnchor.UpperLeft; titleText.raycastTarget = false;
            titleText.horizontalOverflow = HorizontalWrapMode.Wrap;

            string statusLabel = unlocked ? "UNLOCKED" : "LOCKED";
            var status = new GameObject("Status", typeof(RectTransform), typeof(Text));
            status.transform.SetParent(card.transform, false);
            var statusRt = status.GetComponent<RectTransform>();
            statusRt.anchorMin = new Vector2(0, 1); statusRt.anchorMax = new Vector2(0.62f, 1); statusRt.pivot = new Vector2(0, 1);
            statusRt.anchoredPosition = new Vector2(LegacyUITheme.DP(12), -LegacyUITheme.DP(30));
            statusRt.sizeDelta = new Vector2(0, LegacyUITheme.DP(18));
            var statusText = status.GetComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 12; statusText.fontStyle = FontStyle.Bold;
            statusText.color = unlocked ? LegacyUITheme.BrassBorder : LegacyUITheme.Failure;
            statusText.text = statusLabel;
            statusText.alignment = TextAnchor.UpperLeft; statusText.raycastTarget = false;

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
            summaryText.text = unlocked ? "Tap for details" : GetUnlockText(raid);
            summaryText.alignment = TextAnchor.UpperLeft; summaryText.raycastTarget = false;
            summaryText.horizontalOverflow = HorizontalWrapMode.Wrap; summaryText.verticalOverflow = VerticalWrapMode.Truncate;

            if (!unlocked)
            {
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
        // 8B — Raid Detail. Real unlock data + an honest fallback frame (8G policy) for
        // everything the backend does not support (team setup, starting, active state,
        // summary, rewards — none of which exist for raids anywhere in the backend).
        // ================================================================
        private void ShowDetail(RaidDefinition raid)
        {
            if (raid == null) { ShowHub(); return; }
            _selectedRaid = raid; _screen = Screen.Detail; ClearContent();
            AddText(_content, "Header", Format(raid.id), 24, LegacyUITheme.BrassBorder, 48, TextAnchor.MiddleLeft, true);

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
            bannerImage.sprite = LegacySpriteRegistry.GetSprite("epic_raid");
            bannerImage.preserveAspect = false; bannerImage.raycastTarget = false;
            var bc = Color.white; bc.a = 0.6f; bannerImage.color = bc;

            bool unlocked = IsRaidUnlocked(raid);
            AddText(_content, "Status", unlocked ? "UNLOCKED" : "LOCKED  •  " + GetUnlockText(raid), 15, LegacyUITheme.DimWhite, 36, TextAnchor.MiddleLeft, true);

            // 8G fallback: no combat, team, active-run, summary, or reward system exists for raids
            // in the backend — this is stated plainly instead of showing a fake action.
            var fallbackFrame = new GameObject("FallbackFrame", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            fallbackFrame.transform.SetParent(_content, false);
            fallbackFrame.GetComponent<LayoutElement>().preferredHeight = LegacyUITheme.DP(64);
            var ffImage = fallbackFrame.GetComponent<Image>();
            ffImage.sprite = BorderSprite("object_border_no_background");
            ffImage.type = ffImage.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            ffImage.color = ffImage.sprite != null ? Color.white : Color.clear;
            var note = AddText(fallbackFrame.transform, "Note",
                unlocked
                    ? "Start the restored Legacy room sequence with the current party."
                    : "Raid is locked until the requirement is met.",
                14, LegacyUITheme.DimWhite, LegacyUITheme.DP(64), TextAnchor.MiddleCenter, true);
            note.fontStyle = FontStyle.Italic;
            note.horizontalOverflow = HorizontalWrapMode.Wrap;

            var active = _services.Raid?.ActiveRaid;
            bool activeForThisRaid = active != null &&
                string.Equals(active.Definition.id, raid.id, StringComparison.OrdinalIgnoreCase);
            if (activeForThisRaid)
            {
                bool boss = active.Definition.LegacyEncounters?.Any(e =>
                    e.LegacyProgress == active.LegacyProgress && e.IsBossRoom) == true;
                string runText = active.IsComplete
                    ? "RAID COMPLETE"
                    : active.IsFailed ? "RAID FAILED" : "ROOM " + active.LegacyProgress + "/" + active.Definition.LegacyMaxProgress;
                string eventText = active.HasActiveEvent
                    ? "EVENT " + Format(active.EventKey) + " / " + active.EventProgress
                    : "EVENT NONE";
                AddText(_content, "RunState", runText + "  •  " + (boss ? "BOSS ROOM" : "ENCOUNTER ROOM"),
                    14, LegacyUITheme.BrassBorder, 30, TextAnchor.MiddleLeft, true);
                AddText(_content, "EventState", eventText + "  •  DARKNESS " + _services.Raid.CurrentDarkness,
                    12, LegacyUITheme.DimWhite, 24, TextAnchor.MiddleLeft, true);
                AddText(_content, "Rewards", "PENDING REWARDS " + active.PendingRewards.Count +
                    (string.IsNullOrEmpty(active.EventOutcome) ? string.Empty : "  •  " + active.EventOutcome),
                    11, LegacyUITheme.DimWhite, 42, TextAnchor.MiddleLeft, true);
            }

            if (unlocked && _services.Raid != null)
            {
                if (_services.Raid.ActiveRaid == null)
                {
                    AddAction(_content, "StartRaid", "START RAID", "Use current party", "object_border_brass",
                        () => { if (_services.Raid.StartRaid(raid.id, null, out _)) ShowDetail(raid); });
                }
                else if (string.Equals(_services.Raid.ActiveRaid.Definition.id, raid.id, StringComparison.OrdinalIgnoreCase))
                {
                    if (_services.Raid.ActiveRaid.IsComplete)
                    {
                        AddAction(_content, "CollectRaid", "COLLECT REWARDS", "Move raid drops into Storage", "object_border_brass",
                            () => { _services.Raid.CollectRewards(out _); ShowDetail(raid); });
                    }
                    else
                    {
                        AddAction(_content, "FightRaid", "FIGHT CURRENT ROOM", "Resolve the current encounter", "object_border_brass",
                            () => { _services.Raid.FightCurrentRoom(out _); ShowDetail(raid); });
                        AddAction(_content, "AbandonRaid", "ABANDON RAID", "End this in-session run", "object_border_dim_white",
                            () => { _services.Raid.AbandonRaid(); ShowDetail(raid); });
                    }
                }
            }

            AddAction(_content, "Back", "BACK TO RAIDS", "", "object_border_dim_white", ShowHub);
        }

        private string GetUnlockText(RaidDefinition raid)
        {
            if (string.IsNullOrEmpty(raid.RequiredClearDungeonId)) return "Locked by backend state.";
            return "Requires clear: " + Format(raid.RequiredClearDungeonId) + " (progress " + raid.RequiredClearProgress + ")";
        }

        private static string Format(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "Unknown";
            return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(id.Replace("_", " "));
        }

        private static Sprite BorderSprite(string key) => LegacyThemeSprites.Get(key);

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
    }
}
