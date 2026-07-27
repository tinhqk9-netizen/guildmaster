using System.Collections.Generic;
using UnityEngine;

namespace GuildMaster.Runtime.UI.Core
{
    public class UIService : IUIService
    {
        private readonly Dictionary<UIScreenId, UIScreen> _screens = new Dictionary<UIScreenId, UIScreen>();
        private readonly Stack<UIScreenId> _screenStack = new Stack<UIScreenId>();
        private UIScreenId _currentPopup = UIScreenId.None;

        public void RegisterScreen(UIScreen screen)
        {
            if (screen == null) return;
            _screens[screen.ScreenId] = screen;
            screen.Hide(); // default state
        }

        public void ShowScreen(UIScreenId screenId)
        {
            if (!_screens.ContainsKey(screenId))
            {
                Debug.LogWarning($"[UIService] Screen {screenId} not registered. (Deferred or Placeholder)");
                return;
            }

            // Hide current screen if any
            if (_screenStack.Count > 0)
            {
                var current = _screenStack.Peek();
                if (_screens.TryGetValue(current, out var currentScreen))
                {
                    currentScreen.Hide();
                }
            }

            _screenStack.Push(screenId);
            _screens[screenId].Show();
        }

        public void HideScreen(UIScreenId screenId)
        {
            if (_screens.TryGetValue(screenId, out var screen))
            {
                screen.Hide();
            }
        }

        public void Back()
        {
            if (_screenStack.Count <= 1) return; // Don't pop the root screen

            var current = _screenStack.Pop();
            HideScreen(current);

            var previous = _screenStack.Peek();
            if (_screens.TryGetValue(previous, out var prevScreen))
            {
                prevScreen.Show();
            }
        }

        public void ShowPopup(UIScreenId popupId)
        {
            if (!_screens.ContainsKey(popupId))
            {
                Debug.LogWarning($"[UIService] Popup {popupId} not registered.");
                return;
            }

            if (_currentPopup != UIScreenId.None)
            {
                ClosePopup();
            }

            _currentPopup = popupId;
            _screens[popupId].Show();
        }

        public void ClosePopup()
        {
            if (_currentPopup != UIScreenId.None && _screens.TryGetValue(_currentPopup, out var popup))
            {
                popup.Hide();
                _currentPopup = UIScreenId.None;
            }
        }

        // S4 Dialog System
        private GuildMaster.Runtime.UI.Popup.PopupScreen _dialogScreen;

        public void RegisterDialogScreen(GuildMaster.Runtime.UI.Popup.PopupScreen popupScreen)
        {
            _dialogScreen = popupScreen;
            if (_dialogScreen != null) _dialogScreen.Hide();
        }

        public void ShowInfo(string title, string message)
        {
            if (_dialogScreen != null)
            {
                _dialogScreen.ShowMessage(title, message);
            }
            else
            {
                Debug.LogWarning($"[UIService] ShowInfo: {title} - {message} (No Dialog Registered)");
            }
        }

        public void ShowError(string title, string message)
        {
            ShowInfo($"[Error] {title}", message);
        }

        public void ShowDeferred()
        {
            if (_dialogScreen != null)
            {
                _dialogScreen.ShowDeferred();
            }
            else
            {
                Debug.LogWarning("[UIService] Feature Deferred (No Dialog Registered)");
            }
        }
    }
}
