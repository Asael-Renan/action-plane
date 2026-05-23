using System.ComponentModel.DataAnnotations;
using FiveW2H.App.Core.Models;
using FiveW2H.App.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiveW2H.App.UI.Models;

namespace FiveW2H.App.UI.ViewModels;

public enum TaskEditorMode
{
    Create,
    Edit
}

public partial class TaskEditorViewModel : ObservableValidator
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
    private string company = string.Empty;

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

    public TaskEditorMode Mode { get; }
    public IReadOnlyList<FiveW2H.App.Core.Models.TaskStatus> Statuses { get; } = Enum.GetValues<FiveW2H.App.Core.Models.TaskStatus>();
    public IReadOnlyList<Priority> Priorities { get; } = Enum.GetValues<Priority>();
    public string Title => Mode == TaskEditorMode.Create ? "Nova Acao" : "Editar Acao";
    public string Subtitle => Mode == TaskEditorMode.Create ? "Preencha os campos principais do plano 5W2H." : $"Registro #{Id:00000}";
    public string ModeBadgeText => Mode == TaskEditorMode.Create ? "Novo registro pronto para cadastro" : "Edicao pronta para salvar";
    public string PrimaryActionText => Mode == TaskEditorMode.Create ? "Criar Acao" : "Salvar Acao";

    public event Action<bool?>? CloseRequested;

    public TaskEditorViewModel(ITaskService taskService, TaskModel? task = null)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));

        Mode = task is null ? TaskEditorMode.Create : TaskEditorMode.Edit;
        SelectedStatus = FiveW2H.App.Core.Models.TaskStatus.Pending;
        SelectedPriority = Priority.Medium;
        When = DateTime.Today;

        if (task is null)
        {
            return;
        }

        Id = task.Id;
        What = task.What;
        Why = task.Why;
        Where = task.Where;
        Company = task.Company;
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

            if (Mode == TaskEditorMode.Create)
            {
                await _taskService.CreateTaskAsync(new CreateFiveW2HTaskDto
                {
                    What = What.Trim(),
                    Why = Why.Trim(),
                    Where = Where.Trim(),
                    Company = Company.Trim(),
                    When = When.Value,
                    Who = Who.Trim(),
                    How = How.Trim(),
                    HowMuch = HowMuch,
                    Status = SelectedStatus,
                    Priority = SelectedPriority,
                    Notes = Notes.Trim()
                });
            }
            else
            {
                await _taskService.UpdateTaskAsync(new UpdateFiveW2HTaskDto
                {
                    Id = Id,
                    What = What.Trim(),
                    Why = Why.Trim(),
                    Where = Where.Trim(),
                    Company = Company.Trim(),
                    When = When.Value,
                    Who = Who.Trim(),
                    How = How.Trim(),
                    HowMuch = HowMuch,
                    Status = SelectedStatus,
                    Priority = SelectedPriority,
                    Notes = Notes.Trim()
                });
            }

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
