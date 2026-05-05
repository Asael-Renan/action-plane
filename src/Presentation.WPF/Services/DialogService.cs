using Application.Interfaces;
using Presentation.WPF.Models;
using Presentation.WPF.ViewModels;
using Presentation.WPF.Views;
using System.Windows;

namespace Presentation.WPF.Services;

public class DialogService : IDialogService
{
    private readonly IFiveW2HTaskService _taskService;

    public DialogService(IFiveW2HTaskService taskService)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
    }

    public Task<bool> ShowEditItemDialogAsync(TaskModel task)
    {
        var viewModel = new EditItemViewModel(_taskService, task);
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

        var dialogResult = window.ShowDialog() == true;
        return Task.FromResult(dialogResult);
    }
}
