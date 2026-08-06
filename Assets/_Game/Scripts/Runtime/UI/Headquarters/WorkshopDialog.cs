using System.Collections.Generic;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Legacy;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Headquarters
{
    /// <summary>
    /// Phase 5D Workshop dialog. Reads ICraftService only; never consumes ingredients, grants
    /// items, or advances craft time itself — every mutation goes through the real service
    /// (TryStartCraft / ClaimCompletedCraft), and this view only re-reads state afterward.
    /// Craft duration is the same hard-coded 10s used by CraftService.ProgressWorkshop and by the
    /// existing CraftScreen (S6) — ICraftService exposes no duration getter, so this is the only
    /// real, already-established value available (see report "Known limitations").
    /// No Cancel action exists: ICraftService has no method to remove a queued item.
    /// </summary>
    public sealed class WorkshopDialog : MonoBehaviour
    {
        public const long CraftDurationSeconds = 10; // Mirrors CraftService.DEFAULT_CRAFT_DURATION_SECONDS (private, not exposed).

        [Header("Header")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _queueCountText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _recipesButton;

        [Header("Queue list")]
        [SerializeField] private RectTransform _listContent;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private Sprite _rowBorderSprite;

        [Header("Recipe panel (child overlay — not an AppShell popup)")]
        [SerializeField] private WorkshopRecipePanel _recipePanel;

        private ServiceContainer _services;
        private System.Action _onClose;
        private System.Action _onStateChanged;

        public void Setup(ServiceContainer services, System.Action onClose, System.Action onStateChanged)
        {
            _services = services;
            _onClose = onClose;
            _onStateChanged = onStateChanged;

            if (_titleText != null) _titleText.text = "Workshop";

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(() => _onClose?.Invoke());
            }

            if (_recipesButton != null)
            {
                _recipesButton.onClick.RemoveAllListeners();
                _recipesButton.onClick.AddListener(OpenRecipes);
            }

            if (_recipePanel != null)
            {
                _recipePanel.Setup(_services, onClose: CloseRecipes, onCraftChanged: OnStateChangedInternal);
                _recipePanel.Hide();
            }

            Refresh();
        }

        public void Refresh()
        {
            if (_services?.Craft == null) return;

            var queue = _services.Craft.GetQueue();
            var completed = _services.Craft.GetCompletedItems();
            int capacity = _services.Craft.GetQueueCapacity();

            if (_queueCountText != null) _queueCountText.text = $"{queue.Count} / {capacity}";

            BuildList(queue, completed);

            bool empty = queue.Count == 0 && completed.Count == 0;
            if (_emptyState != null) _emptyState.SetActive(empty);
        }

        private void OnStateChangedInternal()
        {
            Refresh();
            _onStateChanged?.Invoke();
        }

        // ── Queue + completed list (one unified list, matching legacy dialog_workshop's single list) ──

        private void BuildList(IReadOnlyList<ItemActionSaveData> queue, IReadOnlyList<ItemActionSaveData> completed)
        {
            if (_listContent == null) return;
            for (int i = _listContent.childCount - 1; i >= 0; i--)
                Destroy(_listContent.GetChild(i).gameObject);

            for (int i = 0; i < queue.Count; i++)
            {
                bool isActive = i == 0;
                long remaining = isActive ? System.Math.Max(0, CraftDurationSeconds - queue[i].SecondsPassed) : -1;
                float progress = isActive ? Mathf.Clamp01((float)queue[i].SecondsPassed / CraftDurationSeconds) : 0f;
                string status = isActive ? $"Crafting... {remaining}s" : "Waiting...";
                WorkshopRowBuilder.CreateRow(_listContent, _rowBorderSprite, queue[i].DefinitionId, queue[i].StackCount,
                    status, showProgress: isActive, progress: progress, actionLabel: null, onAction: null);
            }

            foreach (var item in completed)
            {
                string instanceId = item.InstanceId;
                WorkshopRowBuilder.CreateRow(_listContent, _rowBorderSprite, item.DefinitionId, item.StackCount,
                    "Ready!", showProgress: false, progress: 1f, actionLabel: "Collect",
                    onAction: () => OnCollectClicked(instanceId));
            }
        }

        private void OnCollectClicked(string instanceId)
        {
            if (_services?.Craft == null) return;
            bool success = _services.Craft.ClaimCompletedCraft(instanceId);
            if (success) OnStateChangedInternal();
        }

        // ── Recipes (child overlay, no AppShell popup involved) ────────────────────

        private void OpenRecipes()
        {
            _recipePanel?.Show();
        }

        private void CloseRecipes()
        {
            _recipePanel?.Hide();
        }
    }
}
