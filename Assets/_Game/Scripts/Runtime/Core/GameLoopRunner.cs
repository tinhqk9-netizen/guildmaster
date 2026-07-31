using UnityEngine;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Core
{
    public class GameLoopRunner : MonoBehaviour
    {
        private IGameLoopService _gameLoopService;
        private ISaveService _saveService;
        private float _accumulator;
        private float _saveTimer;
        private bool _isFirstSave;

        public void Initialize(IGameLoopService gameLoopService, ISaveService saveService)
        {
            _gameLoopService = gameLoopService;
            _saveService = saveService;
            _isFirstSave = true;
            _saveTimer = 0f;
            _gameLoopService.Initialize();
        }

        private void Update()
        {
            if (_gameLoopService == null) return;

            // Using unscaledDeltaTime makes the clock pause-safe 
            // and independent of Time.timeScale modifications.
            float dt = Time.unscaledDeltaTime;
            _accumulator += dt;
            _saveTimer += dt;

            while (_accumulator >= 1.0f)
            {
                _accumulator -= 1.0f;
                _gameLoopService.TickRuntime();
            }

            float targetSaveTime = _isFirstSave ? 8.0f : 3.0f;
            if (_saveTimer >= targetSaveTime)
            {
                _saveTimer -= targetSaveTime;
                _isFirstSave = false;
                _saveService?.Save(out _);
            }
        }
        
        private void OnApplicationPause(bool isPaused)
        {
            if (!isPaused && _gameLoopService != null)
            {
                // When returning from background, we process catchup for any missed time.
                _gameLoopService.ProcessOfflineCatchup();
            }
        }
    }
}
