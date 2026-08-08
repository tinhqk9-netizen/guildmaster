using System.Collections;
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
    /// Craft duration and cancellation are delegated to ICraftService so this view never owns
    /// economy timing or refund rules.
    /// </summary>
    public sealed class WorkshopDialog : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _queueCountText;
        [SerializeField] private Text _queueUpgradeInfoText;
        [SerializeField] private Button _queueUpgradeButton;
        [SerializeField] private Text _speedUpgradeInfoText;
        [SerializeField] private Button _speedUpgradeButton;
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
        private Coroutine _refreshCoroutine;

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

            if (_queueUpgradeButton != null)
            {
                _queueUpgradeButton.onClick.RemoveAllListeners();
                _queueUpgradeButton.onClick.AddListener(OnQueueUpgradeClicked);
            }

            if (_speedUpgradeButton != null)
            {
                _speedUpgradeButton.onClick.RemoveAllListeners();
                _speedUpgradeButton.onClick.AddListener(OnSpeedUpgradeClicked);
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
            StartRealtimeRefresh();
        }

        private void OnEnable()
        {
            StartRealtimeRefresh();
        }

        private void OnDisable()
        {
            if (_refreshCoroutine != null)
            {
                StopCoroutine(_refreshCoroutine);
                _refreshCoroutine = null;
            }
        }

        private void StartRealtimeRefresh()
        {
            if (_refreshCoroutine == null && _services?.Craft != null)
                _refreshCoroutine = StartCoroutine(RealtimeRefreshLoop());
        }

        private IEnumerator RealtimeRefreshLoop()
        {
            var wait = new WaitForSecondsRealtime(0.5f);
            while (isActiveAndEnabled)
            {
                yield return wait;
                if (_services?.Craft != null) Refresh();
            }
        }

        public void Refresh()
        {
            if (_services?.Craft == null) return;

            var queue = _services.Craft.GetQueue();
            var completed = _services.Craft.GetCompletedItems();
            int capacity = _services.Craft.GetQueueCapacity();

            if (_queueCountText != null) _queueCountText.text = $"{queue.Count} / {capacity}";

            RefreshUpgradeControls();

            BuildList(queue, completed);

            bool empty = queue.Count == 0 && completed.Count == 0;
            if (_emptyState != null) _emptyState.SetActive(empty);
        }

        private void RefreshUpgradeControls()
        {
            long money = _services.Save.CurrentData.Money;
            int queueLevel = _services.Craft.GetQueueCapacityLevel();
            long queuePrice = _services.Craft.GetUpgradeQueueCapacityPrice();
            bool queueMax = queueLevel >= 10;
            if (_queueUpgradeInfoText != null)
                _queueUpgradeInfoText.text = queueMax
                    ? $"Queue capacity • Level {queueLevel} • MAX"
                    : $"Queue capacity • Level {queueLevel} • Next: {queuePrice}g";
            if (_queueUpgradeButton != null)
            {
                _queueUpgradeButton.interactable = !queueMax && money >= queuePrice;
                var label = _queueUpgradeButton.GetComponentInChildren<Text>();
                if (label != null) label.text = queueMax ? "Max Queue" : "Upgrade Queue";
            }

            int speedLevel = _services.Craft.GetCraftSpeedLevel();
            long speedPrice = _services.Craft.GetUpgradeCraftSpeedPrice();
            bool speedMax = speedLevel >= 25;
            if (_speedUpgradeInfoText != null)
                _speedUpgradeInfoText.text = speedMax
                    ? $"Craft speed • Level {speedLevel} • MAX"
                    : $"Craft speed • Level {speedLevel} • Next: {speedPrice}g";
            if (_speedUpgradeButton != null)
            {
                _speedUpgradeButton.interactable = !speedMax && money >= speedPrice;
                var label = _speedUpgradeButton.GetComponentInChildren<Text>();
                if (label != null) label.text = speedMax ? "Max Speed" : "Upgrade Speed";
            }
        }

        private void OnQueueUpgradeClicked()
        {
            if (_services?.Craft == null) return;
            if (_services.Craft.UpgradeQueueCapacity()) OnStateChangedInternal();
            else RefreshUpgradeControls();
        }

        private void OnSpeedUpgradeClicked()
        {
            if (_services?.Craft == null) return;
            if (_services.Craft.UpgradeCraftSpeed()) OnStateChangedInternal();
            else RefreshUpgradeControls();
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
                long duration = _services.Craft.GetCraftDurationSeconds(queue[i]);
                long remaining = isActive ? System.Math.Max(0, duration - queue[i].SecondsPassed) : -1;
                float progress = isActive && duration > 0 ? Mathf.Clamp01((float)queue[i].SecondsPassed / duration) : 0f;
                string status = isActive ? $"Crafting... {remaining}s" : "Waiting...";
                string instanceId = queue[i].InstanceId;
                WorkshopRowBuilder.CreateRow(_listContent, _rowBorderSprite, queue[i].DefinitionId, queue[i].StackCount,
                    status, showProgress: isActive, progress: progress, actionLabel: "Cancel",
                    onAction: () => OnCancelClicked(instanceId));
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

        private void OnCancelClicked(string instanceId)
        {
            if (_services?.Craft == null) return;
            if (!_services.Craft.CancelCraft(instanceId)) return;

            // CraftService owns the refund/removal semantics. Its cancel API intentionally
            // mutates the current save in memory, so persist the UI action here without touching
            // the service or changing the craft rules.
            if (_services.Save != null) _services.Save.Save(out _);
            OnStateChangedInternal();
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
