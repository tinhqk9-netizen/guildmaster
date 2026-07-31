using System;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Foundation
{
    public class ErrorPopup : MonoBehaviour
    {
        [SerializeField] private Text _errorText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _resetDataButton;

        private Action _onRetry;
        private Action _onResetData;

        public void Initialize(Action onRetry, Action onResetData)
        {
            _onRetry = onRetry;
            _onResetData = onResetData;

            _retryButton.onClick.RemoveAllListeners();
            _retryButton.onClick.AddListener(() => _onRetry?.Invoke());

            _resetDataButton.onClick.RemoveAllListeners();
            _resetDataButton.onClick.AddListener(OnResetDataClicked);
        }

        public void ShowError(string message)
        {
            if (_errorText != null)
                _errorText.text = message;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnResetDataClicked()
        {
            // Simple double-confirm logic for safety could be implemented here
            _onResetData?.Invoke();
        }
    }
}
