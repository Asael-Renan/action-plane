using Presentation.WPF.Models;

namespace Presentation.WPF.Services;

public interface IDialogService
{
    Task<bool> ShowEditItemDialogAsync(TaskModel task);
}
