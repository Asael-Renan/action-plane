using _5W2H.App.Core.Services;
using _5W2H.App.UI.Models;
using _5W2H.App.UI.ViewModels;
using _5W2H.App.UI.Views;
using System.Windows;

namespace _5W2H.App.UI.Services;

public class DialogService : IDialogService
{
    private readonly ITaskService _taskService;

    public DialogService(ITaskService taskService)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
    }

    public Task<bool> ShowEditItemDialogAsync(TaskModel task)
    {
        var viewModel = new EditItemViewModel(_taskService, task);
        var window = new EditItemWindow
        {
            DataContext = viewModel,
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
        };

        viewModel.CloseRequested += result =>
        {
            window.DialogResult = result;
            window.Close();
        };

        return Task.FromResult(window.ShowDialog() == true);
    }
}
