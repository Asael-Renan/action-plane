using FiveW2H.App.Application;
using FiveW2H.App.UI.Models;
using FiveW2H.App.UI.ViewModels;
using FiveW2H.App.UI.Views;
using System.Windows;

namespace FiveW2H.App.UI.Services;

public class DialogService : IDialogService
{
    private readonly ITaskService _taskService;

    public DialogService(ITaskService taskService)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
    }

    public Task<bool> ShowTaskEditorDialogAsync(TaskModel? task = null)
    {
        var viewModel = new TaskEditorViewModel(_taskService, task);
        var window = new EditItemWindow
        {
            DataContext = viewModel,
            Owner = System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
        };

        viewModel.CloseRequested += result =>
        {
            window.DialogResult = result;
            window.Close();
        };

        return Task.FromResult(window.ShowDialog() == true);
    }
}
