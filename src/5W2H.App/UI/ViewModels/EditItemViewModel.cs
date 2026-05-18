using System.ComponentModel.DataAnnotations;
using FiveW2H.App.Core.Models;
using FiveW2H.App.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiveW2H.App.UI.Models;

namespace FiveW2H.App.UI.ViewModels;

public partial class EditItemViewModel : ObservableValidator
{
    private readonly ITaskService _taskService;

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    [Required(ErrorMessage = "What is required.")]
    private string what = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Why is required.")]
    private string why = string.Empty;

    [ObservableProperty]
    private string where = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "When is required.")]
    private DateTime? when;

    [ObservableProperty]
    [Required(ErrorMessage = "Who is required.")]
    private string who = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "How is required.")]
    private string how = string.Empty;

    [ObservableProperty]
    [Range(0, double.MaxValue, ErrorMessage = "How Much cannot be negative.")]
    private decimal howMuch;

    [ObservableProperty]
    private FiveW2H.App.Core.Models.TaskStatus selectedStatus;

    [ObservableProperty]
    private Priority selectedPriority;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    private string validationSummary = string.Empty;

    [ObservableProperty]
    private bool isSaving;

    public IReadOnlyList<FiveW2H.App.Core.Models.TaskStatus> Statuses { get; } = Enum.GetValues<FiveW2H.App.Core.Models.TaskStatus>();
    public IReadOnlyList<Priority> Priorities { get; } = Enum.GetValues<Priority>();

    public event Action<bool?>? CloseRequested;

    public EditItemViewModel(ITaskService taskService, TaskModel task)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        ArgumentNullException.ThrowIfNull(task);

        Id = task.Id;
        What = task.What;
        Why = task.Why;
        Where = task.Where;
        When = task.When;
        Who = task.Who;
        How = task.How;
        HowMuch = task.HowMuch;
        SelectedStatus = Enum.TryParse<FiveW2H.App.Core.Models.TaskStatus>(task.Status, out var status) ? status : FiveW2H.App.Core.Models.TaskStatus.Pending;
        SelectedPriority = Enum.TryParse<Priority>(task.Priority, out var priority) ? priority : Priority.Medium;
        Notes = task.Notes;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        ValidateAllProperties();
        if (HasErrors || When is null)
        {
            ValidationSummary = string.Join(Environment.NewLine, GetErrors().Select(error => error.ErrorMessage));
            SaveCommand.NotifyCanExecuteChanged();
            return;
        }

        try
        {
            IsSaving = true;
            ValidationSummary = string.Empty;

            await _taskService.UpdateTaskAsync(new UpdateFiveW2HTaskDto
            {
                Id = Id,
                What = What.Trim(),
                Why = Why.Trim(),
                Where = Where.Trim(),
                When = When.Value,
                Who = Who.Trim(),
                How = How.Trim(),
                HowMuch = HowMuch,
                Status = SelectedStatus,
                Priority = SelectedPriority,
                Notes = Notes.Trim()
            });

            CloseRequested?.Invoke(true);
        }
        catch (Exception ex)
        {
            ValidationSummary = ex.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(false);
    }

    private bool CanSave()
    {
        return !IsSaving && !HasErrors;
    }

    partial void OnWhatChanged(string value) => ValidateAndRefresh(value, nameof(What));
    partial void OnWhyChanged(string value) => ValidateAndRefresh(value, nameof(Why));
    partial void OnWhenChanged(DateTime? value) => ValidateAndRefresh(value, nameof(When));
    partial void OnWhoChanged(string value) => ValidateAndRefresh(value, nameof(Who));
    partial void OnHowChanged(string value) => ValidateAndRefresh(value, nameof(How));
    partial void OnHowMuchChanged(decimal value) => ValidateAndRefresh(value, nameof(HowMuch));
    partial void OnIsSavingChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();

    private void ValidateAndRefresh<T>(T value, string propertyName)
    {
        ValidateProperty(value, propertyName);
        SaveCommand.NotifyCanExecuteChanged();
    }
}
