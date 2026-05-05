namespace Presentation.WPF.Services;

public interface IMessageDialogService
{
    bool Confirm(string message, string title);

    void ShowWarning(string message, string title);
}
