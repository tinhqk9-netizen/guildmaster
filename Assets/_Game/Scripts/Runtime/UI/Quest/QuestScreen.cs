using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.UI.Quest
{
    /// <summary>
    /// Quest screen — shows all active quests with title, progress bar, reward, and completion state.
    /// Reward destination is determined by the Legacy quest pool; the player does not choose it.
    /// </summary>
    public class QuestScreen : UIScreen
    {
        private IQuestService _questService;

        // ── Serialized references (wired by Apply tool) ──────────────────────────
        [SerializeField] private RectTransform _cardContainer;
        [SerializeField] private Text          _summaryText;
        [SerializeField] private Text          _detailText;
        [SerializeField] private Button        _claimButton;
        [SerializeField] private Button        _cycleDoctrineButton;
        [SerializeField] private Text          _feedbackText;

        private int _selectedIndex;

        public void Initialize(ServiceContainer services)
        {
            _questService = services.Quest;
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Refresh()
        {
            if (_questService == null) return;

            var quests = _questService.GetActiveQuests();

            if (_summaryText != null)
                _summaryText.text = $"Active quests: {quests?.Count ?? 0}";

            if (_cycleDoctrineButton != null) _cycleDoctrineButton.gameObject.SetActive(false);

            if (quests == null || quests.Count == 0)
            {
                UICardFactory.ClearContainer(_cardContainer);
                UICardFactory.CreateDivider(_cardContainer, "No active quests");
                if (_detailText  != null) _detailText.text  = "No active quests at this time.";
                if (_claimButton != null) _claimButton.interactable = false;
                return;
            }

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, quests.Count - 1);
            BuildQuestCards(quests);
            RefreshDetail(quests[_selectedIndex]);
            if (_claimButton != null)
                _claimButton.interactable = quests[_selectedIndex].State == QuestState.Completed;
        }

        public void OnClickCycleDoctrine() { }

        private void BuildQuestCards(IReadOnlyList<QuestRuntime> quests)
        {
            UICardFactory.ClearContainer(_cardContainer);

            for (int i = 0; i < quests.Count; i++)
            {
                int captured = i;
                var q        = quests[i];
                string defId = q.Definition?.id ?? "Quest";
                long target   = q.TargetProgress > 0 ? q.TargetProgress : (q.Definition?.TargetProgress ?? 1L);
                long progress = q.Progress;
                string sub   = $"{q.Progress}/{target}  [{q.State}]";
                if (q.State == QuestState.Completed) sub += " ✓";

                UICardFactory.CreateCard(_cardContainer, defId, sub,
                    i == _selectedIndex, true, () => SelectIndex(captured));
            }
        }

        private void RefreshDetail(QuestRuntime q)
        {
            if (_detailText == null || q == null) return;
            long target   = q.TargetProgress > 0 ? q.TargetProgress : (q.Definition?.TargetProgress ?? 1L);
            long progress = q.Progress;
            float pct    = target > 0 ? (float)progress / target : 0f;

            string text =
                $"{q.Definition?.id ?? "Quest"}\n" +
                $"Progress: {progress}/{target}  ({pct * 100f:F0}%)\n" +
                $"State: {q.State}\n" +
                $"Instance: {q.InstanceId}\n" +
                $"\nReward pool: {(string.Equals(q.RewardPoolType, "Kings", System.StringComparison.OrdinalIgnoreCase) ? "Kings / Gems" : q.RewardDoctrineId ?? q.Definition?.DoctrineId ?? "Legacy pool")}";

            if (q.State == QuestState.Completed)
                text += "\n✓ Quest complete! Press 'Claim Reward'.";

            _detailText.text = text;
        }

        /// <summary>Click-to-select — also callable from tests.</summary>
        public void SelectIndex(int index)
        {
            var quests = _questService?.GetActiveQuests();
            if (quests == null || quests.Count == 0) return;
            _selectedIndex = Mathf.Clamp(index, 0, quests.Count - 1);
            Refresh();
        }

        public void OnClickSelectNext()     => Cycle(1);
        public void OnClickSelectPrevious() => Cycle(-1);

        private void Cycle(int dir)
        {
            var quests = _questService?.GetActiveQuests();
            if (quests == null || quests.Count == 0) return;
            _selectedIndex = ((_selectedIndex + dir) % quests.Count + quests.Count) % quests.Count;
            Refresh();
        }

        public void OnClickClaimSelected()
        {
            var quests = _questService?.GetActiveQuests();
            if (quests == null || quests.Count == 0) return;
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, quests.Count - 1);
            var q = quests[_selectedIndex];
            if (q.State == QuestState.Completed)
            {
                _questService.ClaimReward(q.InstanceId);
                ShowFeedback($"Reward claimed for {q.Definition?.id ?? q.InstanceId}!", true);
            }
            else
            {
                ShowFeedback("Quest is not complete yet.", false);
            }
            Refresh();
        }

        private void ShowFeedback(string msg, bool success)
        {
            if (_feedbackText == null) return;
            _feedbackText.text  = msg;
            _feedbackText.color = success ? UITemporaryTheme.SuccessColor : UITemporaryTheme.WarningColor;
        }
    }
}
