namespace GuildMaster.Runtime.UI.Core
{
    public interface IUIService
    {
        void RegisterScreen(UIScreen screen);
        void ShowScreen(UIScreenId screenId);
        void HideScreen(UIScreenId screenId);
        void Back();
        void ShowPopup(UIScreenId popupId);
        void ClosePopup();
        
        // S4 Dialog System
        void RegisterDialogScreen(GuildMaster.Runtime.UI.Popup.PopupScreen popupScreen);
        void ShowInfo(string title, string message);
        void ShowError(string title, string message);
        void ShowDeferred();
    }
}
