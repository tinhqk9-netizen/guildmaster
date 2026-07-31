using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.UI.Foundation
{
    public class SaveStatusIndicator : MonoBehaviour
    {
        [SerializeField] private GameObject _savingIcon;
        [SerializeField] private Image _savingIconImage;
        [SerializeField] private Color _colorSaving = Color.white;
        [SerializeField] private Color _colorSuccess = Color.green;
        [SerializeField] private Color _colorError = Color.red;
        
        private ISaveService _saveService;
        private float _hideTimer = 0f;

        public void Initialize(ISaveService saveService)
        {
            _saveService = saveService;
            if (_saveService != null)
            {
                _saveService.OnSaveStarted += HandleSaveStarted;
                _saveService.OnSaveCompleted += HandleSaveCompleted;
            }
            
            if (_savingIcon != null)
                _savingIcon.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_saveService != null)
            {
                _saveService.OnSaveStarted -= HandleSaveStarted;
                _saveService.OnSaveCompleted -= HandleSaveCompleted;
            }
        }

        private void Update()
        {
            if (_hideTimer > 0)
            {
                _hideTimer -= Time.deltaTime;
                if (_hideTimer <= 0)
                {
                    if (_savingIcon != null)
                        _savingIcon.SetActive(false);
                }
            }
        }

        private void HandleSaveStarted()
        {
            if (_savingIcon != null)
            {
                _savingIcon.SetActive(true);
                if (_savingIconImage != null) _savingIconImage.color = _colorSaving;
            }
            _hideTimer = 0f;
        }

        private void HandleSaveCompleted(bool success)
        {
            if (_savingIconImage != null)
            {
                _savingIconImage.color = success ? _colorSuccess : _colorError;
            }
            _hideTimer = 1.0f; // Show result for 1 second before hiding
        }
    }
}
