using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.UI.Character;
using GuildMaster.Runtime.Services;
using GuildMaster.Definitions;
using GuildMaster.Database;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.UI.Dungeon
{
    /// <summary>
    /// Dungeon screen — Multi-Expedition Slot view (Slot 0, 1, 2).
    /// Idle / auto-battle: GameLoopService ticks all expeditions every second.
    /// This screen auto-refreshes every 0.5s while visible so combat progress
    /// updates in real-time without any manual Continue button.
    /// </summary>
    public class DungeonScreen : UIScreen
    {
        private IDungeonService   _dungeonService;
        private IPartyService     _partyService;
        private ICharacterService _characterService;
        private CharacterScreen   _characterScreen;
        private GameDatabase      _database;

        // ── Slot selection references ────────────────────────────────────────────
        [SerializeField] private RectTransform _slotBarContainer;
        [SerializeField] private Button[]      _slotButtons;
        private int _viewedSlotIndex = 0;

        // ── Panel references ─────────────────────────────────────────────────────
        [SerializeField] private GameObject _panelSelect;
        [SerializeField] private GameObject _panelActive;
        [SerializeField] private GameObject _panelLoot;

        // Select panel
        [SerializeField] private RectTransform _dungeonCardContainer;
        [SerializeField] private Text          _selectedDungeonText;
        [SerializeField] private Text          _partyText;
        [SerializeField] private Button        _startButton;

        // Active panel
        [SerializeField] private Text          _activeDungeonTitle;
        [SerializeField] private Text          _activeTurnText;
        [SerializeField] private Text          _activeActionText;
        [SerializeField] private RectTransform _combatCardContainer;
        [SerializeField] private Button        _continueButton;   // kept for scene compat, hidden at runtime
        [SerializeField] private Button        _autoBattleButton; // hidden — game is always auto

        // Loot panel
        [SerializeField] private RectTransform _lootCardContainer;
        [SerializeField] private Button        _collectLootButton;

        // Shared
        [SerializeField] private Text _summaryText;
        [SerializeField] private Text _feedbackText;

        private int       _selectedDungeonIndex;
        private Coroutine _autoRefreshCoroutine;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        public void Initialize(ServiceContainer services, CharacterScreen characterScreen = null)
        {
            _dungeonService   = services.Dungeon;
            _partyService     = services.Party;
            _characterService = services.Character;
            _characterScreen  = characterScreen;
            _database         = services.Database;
        }

        public override void Show()
        {
            base.Show();
            if (_continueButton   != null) _continueButton.gameObject.SetActive(false);
            if (_autoBattleButton != null) _autoBattleButton.gameObject.SetActive(false);

            Refresh();
            StartAutoRefresh();
        }

        public override void Hide()
        {
            base.Hide();
            StopAutoRefresh();
        }

        // ── Auto-refresh (real-time idle combat display) ──────────────────────────

        private void StartAutoRefresh()
        {
            StopAutoRefresh();
            _autoRefreshCoroutine = StartCoroutine(AutoRefreshLoop());
        }

        private void StopAutoRefresh()
        {
            if (_autoRefreshCoroutine != null)
            {
                StopCoroutine(_autoRefreshCoroutine);
                _autoRefreshCoroutine = null;
            }
        }

        private IEnumerator AutoRefreshLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                if (_dungeonService == null) yield break;
                Refresh();
            }
        }

        // ── Core refresh ──────────────────────────────────────────────────────────

        private List<DungeonDefinition> GetDungeons() =>
            _database != null
                ? new List<DungeonDefinition>(_database.GetAll<DungeonDefinition>())
                : new List<DungeonDefinition>();

        public void SelectSlotIndex(int slotIndex)
        {
            _viewedSlotIndex = Mathf.Clamp(
                slotIndex,
                0,
                _dungeonService != null ? _dungeonService.MaxExpeditions - 1 : 2);
            Refresh();
        }

        private string GetActionName(int actionType)
        {
            switch (actionType)
            {
                case 0: return "🚪 Entering dungeon...";
                case 1: return "🚪 Entering new room...";
                case 2: return "⚔️ Fighting enemies...";
                case 3: return "🎁 Collecting loot...";
                case 4: return "🔍 Searching next room...";
                case 5: return "💀 Party defeated, recovering...";
                case 6: return "🏃‍♂️ Retreating...";
                default: return "⚔️ Exploring...";
            }
        }

        public void Refresh()
        {
            if (_dungeonService == null) return;

            RefreshSlotBar();

            var exp           = _dungeonService.GetExpedition(_viewedSlotIndex);
            bool isActive     = exp != null && exp.Dungeon != null;
            var activeDungeon = exp?.Dungeon;
            int dropCount     = activeDungeon?.PendingDrops?.Count ?? 0;
            bool hasLoot      = isActive && activeDungeon != null
                                && dropCount > 0
                                && activeDungeon.Enemies.Count == 0;

            // Always enable Collect Loot button if there's loot in the chest!
            if (_collectLootButton != null)
            {
                _collectLootButton.interactable = dropCount > 0;
                var txt = _collectLootButton.GetComponentInChildren<Text>();
                if (txt != null)
                    txt.text = dropCount > 0 ? $"Collect ({dropCount}) 🎒" : "Collect Loot 🎒";
            }

            if (_summaryText != null)
                _summaryText.text = $"Slot {_viewedSlotIndex + 1}: "
                    + (hasLoot ? $"⚔️ Room Cleared! ({dropCount} Chests)" : isActive ? $"⚔️ Exploring (Chest: {dropCount})" : "🟢 IDLE");

            if (hasLoot)
                ShowLootPanel(activeDungeon);
            else if (isActive && activeDungeon != null)
                ShowActivePanel(activeDungeon);
            else
                ShowSelectPanel();
        }

        // ── Slot bar ─────────────────────────────────────────────────────────────

        private void RefreshSlotBar()
        {
            if (_slotBarContainer == null) return;

            UICardFactory.ClearContainer(_slotBarContainer);
            int maxSlots = _dungeonService?.MaxExpeditions ?? 3;
            for (int i = 0; i < maxSlots; i++)
            {
                int slotIdx = i;
                var exp     = _dungeonService?.GetExpedition(slotIdx);
                int drops   = exp?.Dungeon?.PendingDrops?.Count ?? 0;
                string label = exp != null 
                    ? (drops > 0 ? $"⚔️ Party {slotIdx + 1} (🎒{drops})" : $"⚔️ Party {slotIdx + 1}")
                    : $"Party {slotIdx + 1} (Idle)";

                UICardFactory.CreateTabButton(
                    _slotBarContainer,
                    $"SlotBtn_{slotIdx}",
                    label,
                    slotIdx == _viewedSlotIndex,
                    () => SelectSlotIndex(slotIdx));
            }
        }

        // ── Helper to format names with icons ────────────────────────────────────

        private string FormatDungeonName(string id)
        {
            if (string.IsNullOrEmpty(id)) return "Dungeon";
            string formatted = id.Replace("_", " ");
            formatted = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formatted);

            if (id.Contains("forest"))     return $"🌲 {formatted}";
            if (id.Contains("port"))       return $"⚓ {formatted}";
            if (id.Contains("peak") || id.Contains("frost")) return $"❄️ {formatted}";
            if (id.Contains("battlefield")) return $"⚔️ {formatted}";
            if (id.Contains("waste"))      return $"🌵 {formatted}";
            return $"🏰 {formatted}";
        }

        private string FormatCharacterName(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return "Hero";
            if (_characterService != null)
            {
                var match = _characterService.GetAllCharacters().FirstOrDefault(c => c.InstanceId == instanceId);
                if (match != null)
                {
                    string name = match.Definition?.id ?? match.DefinitionId ?? "Hero";
                    name = name.Replace("_", " ");
                    name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
                    return $"{name} (Lv.{match.Level})";
                }
            }
            return instanceId;
        }

        // ── A) Select panel ───────────────────────────────────────────────────────

        private void ShowSelectPanel()
        {
            SetPanelActive(_panelSelect, _panelActive, _panelLoot);

            var dungeons = GetDungeons();
            if (_dungeonCardContainer != null)
            {
                UICardFactory.ClearContainer(_dungeonCardContainer);
                if (dungeons.Count == 0)
                {
                    UICardFactory.CreateDivider(_dungeonCardContainer, "No dungeons available");
                }
                else
                {
                    UICardFactory.CreateDivider(_dungeonCardContainer, "DUNGEON LIST");
                    for (int i = 0; i < dungeons.Count; i++)
                    {
                        int captured = i;
                        string dungeonId = dungeons[i].id;
                        bool isUnlocked = _dungeonService == null || _dungeonService.IsDungeonUnlocked(dungeonId);

                        string displayName = isUnlocked 
                            ? FormatDungeonName(dungeonId) 
                            : $"🔒 {FormatDungeonName(dungeonId)} (Locked)";

                        int enemyCount = dungeons[i].EnemyIds != null ? dungeons[i].EnemyIds.Count : 0;
                        string sub = isUnlocked
                            ? (enemyCount > 0 ? $"Enemies: {enemyCount} types" : "Mysterious Area")
                            : (!string.IsNullOrEmpty(dungeons[i].RequiredClearDungeonId)
                                ? $"Requires: {FormatDungeonName(dungeons[i].RequiredClearDungeonId)}"
                                : "Locked");

                        UICardFactory.CreateCard(
                            _dungeonCardContainer,
                            displayName, sub,
                            i == _selectedDungeonIndex,
                            isUnlocked,
                            isUnlocked ? (UnityEngine.Events.UnityAction)(() => SelectDungeonIndex(captured)) : null,
                            preferredHeight: 70);
                    }
                }
            }

            if (_selectedDungeonText != null)
            {
                if (dungeons.Count > 0)
                {
                    _selectedDungeonIndex = Mathf.Clamp(_selectedDungeonIndex, 0, dungeons.Count - 1);
                    _selectedDungeonText.text = $"🎯 Selected: {FormatDungeonName(dungeons[_selectedDungeonIndex].id)}";
                }
                else
                {
                    _selectedDungeonText.text = "No dungeons available.";
                }
            }

            var rawParty = _partyService != null
                ? new List<string>(_partyService.GetPartyMembers(_viewedSlotIndex))
                : (_characterScreen?.GetPartyMemberIds() ?? new List<string>());

            var formattedParty = rawParty.ConvertAll(FormatCharacterName);

            if (_partyText != null)
                _partyText.text = rawParty.Count > 0
                    ? $"🛡️ Party {_viewedSlotIndex + 1} ({rawParty.Count}/4 members): {string.Join(", ", formattedParty)}"
                    : $"⚠️ Party {_viewedSlotIndex + 1} is empty! Add characters in Characters screen.";

            if (_startButton != null)
            {
                _startButton.interactable = dungeons.Count > 0 && rawParty.Count > 0;
                var img = _startButton.GetComponent<Image>();
                if (img != null) img.color = new Color(0.20f, 0.65f, 0.35f, 1f); // Green

                var txt = _startButton.GetComponentInChildren<Text>();
                if (txt != null) txt.text = "Start Expedition ⚔️";

                _startButton.onClick.RemoveAllListeners();
                _startButton.onClick.AddListener(OnClickStartSelected);
            }
        }

        // ── B) Active panel ───────────────────────────────────────────────────────

        private void ShowActivePanel(DungeonRuntime run)
        {
            SetPanelActive(_panelActive, _panelSelect, _panelLoot);

            if (_activeDungeonTitle != null)
                _activeDungeonTitle.text = $"⚔️ [Party {_viewedSlotIndex + 1}] {FormatDungeonName(run.Definition?.id)}";

            if (_activeTurnText != null)
                _activeTurnText.text = $"Progress: {run.Progress} rooms  |  Turns: {run.ActionTurnsPassed}";

            if (_activeActionText != null)
                _activeActionText.text = GetActionName(run.ActionType);

            // Configure Start button as Recall 🏃‍♂️
            if (_startButton != null)
            {
                _startButton.interactable = true;
                var img = _startButton.GetComponent<Image>();
                if (img != null) img.color = new Color(0.85f, 0.30f, 0.25f, 1f); // Red / Orange

                var txt = _startButton.GetComponentInChildren<Text>();
                if (txt != null) txt.text = "Recall 🏃‍♂️";

                _startButton.onClick.RemoveAllListeners();
                _startButton.onClick.AddListener(OnClickStopSelected);
            }

            if (_combatCardContainer != null)
            {
                UICardFactory.ClearContainer(_combatCardContainer);

                int dropCount = run.PendingDrops?.Count ?? 0;
                if (dropCount > 0)
                {
                    UICardFactory.CreateCard(_combatCardContainer,
                        $"🎒 PENDING CHEST: {dropCount} ITEMS",
                        "Click 'Collect Loot' below anytime to claim items!",
                        true, true, () => OnClickCollectLoot(), preferredHeight: 65);
                }

                UICardFactory.CreateDivider(_combatCardContainer, "ENEMIES IN ROOM");
                if (run.Enemies.Count == 0)
                {
                    UICardFactory.CreateDivider(_combatCardContainer, "Room cleared! Searching next room...");
                }
                else
                {
                    foreach (var e in run.Enemies)
                        UICardFactory.CreateCard(_combatCardContainer,
                            $"👾 {e.Definition?.id ?? "Enemy"}",
                            $"HP: {e.CurrentHp}",
                            false, false, null, preferredHeight: 60);
                }

                UICardFactory.CreateDivider(_combatCardContainer, "EXPEDITION PARTY");
                var partyIds = run.AdventurerInstanceIds ?? new List<string>();
                foreach (var id in partyIds)
                    UICardFactory.CreateCard(_combatCardContainer, $"🛡️ {FormatCharacterName(id)}", "Exploring...", false, false, null, preferredHeight: 60);
            }
        }

        // ── C) Loot panel ─────────────────────────────────────────────────────────

        private void ShowLootPanel(DungeonRuntime run)
        {
            SetPanelActive(_panelLoot, _panelSelect, _panelActive);

            if (_lootCardContainer != null)
            {
                UICardFactory.ClearContainer(_lootCardContainer);
                if (run.PendingDrops.Count == 0)
                {
                    UICardFactory.CreateDivider(_lootCardContainer, "No loot");
                }
                else
                {
                    UICardFactory.CreateDivider(_lootCardContainer, $"🏆 REWARD CHEST — PARTY {_viewedSlotIndex + 1}");
                    foreach (var drop in run.PendingDrops)
                        UICardFactory.CreateCard(_lootCardContainer,
                            $"🎁 {drop.Definition?.id ?? drop.InstanceId}",
                            $"Quantity: x{drop.StackCount}",
                            false, false, null, preferredHeight: 60);
                }
            }

            if (_collectLootButton != null)
            {
                _collectLootButton.interactable = run.PendingDrops.Count > 0;
                var txt = _collectLootButton.GetComponentInChildren<Text>();
                if (txt != null) txt.text = "Collect Loot 🎒";
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static void SetPanelActive(GameObject show, GameObject hide1, GameObject hide2)
        {
            if (show  != null) show.SetActive(true);
            if (hide1 != null) hide1.SetActive(false);
            if (hide2 != null) hide2.SetActive(false);
        }

        // ── Selection ─────────────────────────────────────────────────────────────

        public void SelectDungeonIndex(int index)
        {
            var dungeons = GetDungeons();
            if (dungeons.Count == 0) return;
            _selectedDungeonIndex = Mathf.Clamp(index, 0, dungeons.Count - 1);
            Refresh();
        }

        public void OnClickSelectNextDungeon()     => CycleDungeon(1);
        public void OnClickSelectPreviousDungeon() => CycleDungeon(-1);

        private void CycleDungeon(int dir)
        {
            var dungeons = GetDungeons();
            if (dungeons.Count == 0) return;
            _selectedDungeonIndex = ((_selectedDungeonIndex + dir) % dungeons.Count + dungeons.Count) % dungeons.Count;
            Refresh();
        }

        // ── Actions ───────────────────────────────────────────────────────────────

        public void OnClickStartSelected()
        {
            var dungeons = GetDungeons();
            var party = _partyService != null
                ? new List<string>(_partyService.GetPartyMembers(_viewedSlotIndex))
                : (_characterScreen?.GetPartyMemberIds() ?? new List<string>());

            if (_dungeonService != null && dungeons.Count > 0 && party.Count > 0)
            {
                _selectedDungeonIndex = Mathf.Clamp(_selectedDungeonIndex, 0, dungeons.Count - 1);
                bool ok = _dungeonService.StartExpedition(
                    _viewedSlotIndex, dungeons[_selectedDungeonIndex].id, party, out string error);
                ShowFeedback(ok
                    ? $"⚔️ Party {_viewedSlotIndex + 1} dispatched!"
                    : $"Failed: {error}", ok);
            }
            else
            {
                ShowFeedback("Select a dungeon and party first.", false);
            }
            Refresh();
        }

        public void OnClickStopSelected()
        {
            if (_dungeonService != null)
            {
                _dungeonService.StopExpedition(_viewedSlotIndex);
                ShowFeedback($"🏃‍♂️ Party {_viewedSlotIndex + 1} recalled safely!", true);
            }
            Refresh();
        }

        public void OnClickContinue() { }
        public void OnClickToggleAutoBattle() { }

        public void OnClickCollectLoot()
        {
            int collected = _dungeonService?.CollectDrops(_viewedSlotIndex) ?? 0;
            ShowFeedback(
                collected > 0 ? $"🎒 Collected {collected} item stack(s) into inventory!" : "Nothing to collect.",
                collected > 0);
            Refresh();
        }

        private void ShowFeedback(string msg, bool success)
        {
            if (_feedbackText == null) return;
            _feedbackText.text  = msg;
            _feedbackText.color = success ? UITemporaryTheme.SuccessColor : UITemporaryTheme.FailureColor;
        }
    }
}
