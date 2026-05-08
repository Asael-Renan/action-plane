using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using _5W2H.App.Core.Models;
using _5W2H.App.Core.Services;
using _5W2H.App.UI.Models;
using _5W2H.App.UI.Services;
using System.Collections.ObjectModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using TaskStatus = _5W2H.App.Core.Models.TaskStatus;

namespace _5W2H.App.UI.ViewModels;

/// <summary>
/// ViewModel for the main task management window.
/// Handles displaying, filtering, and managing 5W2H tasks.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ITaskService _taskService;
    private readonly IBackupService _backupService;
    private readonly IDialogService _dialogService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IMessageDialogService _messageDialogService;

    [ObservableProperty]
    private ObservableCollection<TaskModel> tasks = new();

    [ObservableProperty]
    private TaskModel? selectedTask;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private DateTime? filterStartDate;

    [ObservableProperty]
    private DateTime? filterEndDate;

    [ObservableProperty]
    private string? filterResponsible;

    [ObservableProperty]
    private _5W2H.App.Core.Models.TaskStatus? filterStatus;

    [ObservableProperty]
    private Priority? filterPriority;

    [ObservableProperty]
    private string newWhat = string.Empty;

    [ObservableProperty]
    private string newWhy = string.Empty;

    [ObservableProperty]
    private string newWhere = string.Empty;

    [ObservableProperty]
    private DateTime? newWhen = DateTime.Today;

    [ObservableProperty]
    private string newWho = string.Empty;

    [ObservableProperty]
    private string newHow = string.Empty;

    [ObservableProperty]
    private decimal newHowMuch;

    [ObservableProperty]
    private Priority selectedPriority = Priority.Medium;

    [ObservableProperty]
    private string newNotes = string.Empty;

    [ObservableProperty]
    private int selectedTabIndex;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private int totalActions;

    [ObservableProperty]
    private int pendingActions;

    [ObservableProperty]
    private int completedActions;

    [ObservableProperty]
    private decimal totalCost;

    [ObservableProperty]
    private string pendingPercentText = "0% do total";

    [ObservableProperty]
    private string completedPercentText = "0% do total";

    [ObservableProperty]
    private int criticalPendingActions;

    [ObservableProperty]
    private bool hasCriticalPendingActions;

    [ObservableProperty]
    private PlotModel statusChartModel = new();

    [ObservableProperty]
    private PlotModel priorityChartModel = new();

    [ObservableProperty]
    private PlotModel responsibleChartModel = new();

    [ObservableProperty]
    private PlotModel timelineChartModel = new();

    public IReadOnlyList<Priority> Priorities { get; } = Enum.GetValues<Priority>();
    public IReadOnlyList<_5W2H.App.Core.Models.TaskStatus> Statuses { get; } = Enum.GetValues<_5W2H.App.Core.Models.TaskStatus>();

    public MainViewModel(
        ITaskService taskService,
        IBackupService backupService,
        IDialogService dialogService,
        IFileDialogService fileDialogService,
        IMessageDialogService messageDialogService)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
        _messageDialogService = messageDialogService ?? throw new ArgumentNullException(nameof(messageDialogService));
    }

    [RelayCommand]
    public async Task LoadTasks()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading tasks...";

            var taskDtos = await _taskService.GetAllTasksAsync();
            var models = taskDtos.Select(MapToModel).ToList();

            ApplyClientSideFilters(models);
            Tasks = new ObservableCollection<TaskModel>(models);
            UpdateDashboardMetrics(models);
            UpdateCharts(models);
            StatusMessage = $"Loaded {models.Count} tasks";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SearchTasks()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Searching...";

            var taskDtos = await _taskService.SearchTasksAsync(
                SearchText,
                FilterStartDate,
                FilterEndDate,
                FilterResponsible
            );

            var models = taskDtos.Select(MapToModel).ToList();
            ApplyClientSideFilters(models);
            Tasks = new ObservableCollection<TaskModel>(models);
            UpdateDashboardMetrics(models);
            UpdateCharts(models);
            StatusMessage = $"Found {models.Count} tasks";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ResetFilters()
    {
        SearchText = string.Empty;
        FilterStartDate = null;
        FilterEndDate = null;
        FilterResponsible = null;
        FilterStatus = null;
        FilterPriority = null;
        await LoadTasks();
    }

    [RelayCommand]
    public async Task CreateTask()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Creating task...";

            if (string.IsNullOrWhiteSpace(NewWhat) ||
                string.IsNullOrWhiteSpace(NewWhy) ||
                string.IsNullOrWhiteSpace(NewWho) ||
                string.IsNullOrWhiteSpace(NewHow) ||
                NewWhen is null)
            {
                StatusMessage = "Fill in the required fields to create the task";
                return;
            }

            var dto = new CreateFiveW2HTaskDto
            {
                What = NewWhat.Trim(),
                Why = NewWhy.Trim(),
                Where = NewWhere.Trim(),
                When = NewWhen.Value,
                Who = NewWho.Trim(),
                How = NewHow.Trim(),
                HowMuch = NewHowMuch,
                Priority = SelectedPriority,
                Notes = NewNotes.Trim()
            };

            var createdTask = await _taskService.CreateTaskAsync(dto);

            ClearCreateForm();
            SelectedTabIndex = 1;
            StatusMessage = $"Task #{createdTask.Id} created successfully";
            await SearchTasks();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void ClearCreateForm()
    {
        NewWhat = string.Empty;
        NewWhy = string.Empty;
        NewWhere = string.Empty;
        NewWhen = DateTime.Today;
        NewWho = string.Empty;
        NewHow = string.Empty;
        NewHowMuch = 0;
        SelectedPriority = Priority.Medium;
        NewNotes = string.Empty;
    }

    [RelayCommand]
    public void OpenDashboard()
    {
        SelectedTabIndex = 0;
    }

    [RelayCommand]
    public void OpenFilterScreen()
    {
        SelectedTabIndex = 1;
    }

    [RelayCommand]
    public void OpenCreateScreen()
    {
        SelectedTabIndex = 2;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedTask))]
    public async Task EditSelectedTask()
    {
        if (SelectedTask is null)
        {
            StatusMessage = "Please select a task to edit";
            return;
        }

        var edited = await _dialogService.ShowEditItemDialogAsync(SelectedTask);
        if (!edited)
        {
            StatusMessage = "Edit cancelled";
            return;
        }

        StatusMessage = "Task updated successfully";
        await SearchTasks();
    }

    [RelayCommand]
    public async Task EditTask(TaskModel? task)
    {
        if (task is null)
        {
            StatusMessage = "Please select a task to edit";
            return;
        }

        SelectedTask = task;
        await EditSelectedTask();
    }

    [RelayCommand]
    public async Task ExportData()
    {
        var filePath = _fileDialogService.ShowSaveCsvDialog();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            StatusMessage = "Export cancelled";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Exporting data...";

            var taskDtos = await _taskService.GetAllTasksAsync();
            await _backupService.ExportCsvAsync(filePath, taskDtos);

            StatusMessage = $"Exported {taskDtos.Count()} task(s) to CSV";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ImportData()
    {
        var filePath = _fileDialogService.ShowOpenCsvDialog();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            StatusMessage = "Import cancelled";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Importing data...";

            var result = await _backupService.ImportCsvAsync(filePath);
            await SearchTasks();

            StatusMessage = result.Errors.Count == 0
                ? $"Imported {result.ImportedCount}, updated {result.UpdatedCount}, skipped {result.SkippedCount}"
                : $"Imported {result.ImportedCount}, updated {result.UpdatedCount}, skipped {result.SkippedCount}, errors {result.Errors.Count}";

            if (result.Errors.Count > 0)
            {
                _messageDialogService.ShowWarning(
                    string.Join(Environment.NewLine, result.Errors.Take(8)),
                    "Import validation");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Import error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelectedTask))]
    public async Task DeleteSelectedTask()
    {
        if (SelectedTask is null)
        {
            StatusMessage = "Please select a task to delete";
            return;
        }

        if (!_messageDialogService.Confirm(
            $"Are you sure you want to delete '{SelectedTask.What}'?",
            "Confirm Delete"))
        {
            return;
        }

        try
        {
            IsLoading = true;
            var result = await _taskService.DeleteTaskAsync(SelectedTask.Id);

            if (result)
            {
                StatusMessage = "Task deleted successfully";
                await LoadTasks();
            }
            else
            {
                StatusMessage = "Failed to delete task";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeleteTask(TaskModel? task)
    {
        if (task is null)
        {
            StatusMessage = "Please select a task to delete";
            return;
        }

        SelectedTask = task;
        await DeleteSelectedTask();
    }

    partial void OnSelectedTaskChanged(TaskModel? value)
    {
        EditSelectedTaskCommand.NotifyCanExecuteChanged();
        DeleteSelectedTaskCommand.NotifyCanExecuteChanged();
    }

    private bool HasSelectedTask()
    {
        return SelectedTask is not null;
    }

    private static TaskModel MapToModel(FiveW2HTaskDto dto)
    {
        return new TaskModel
        {
            Id = dto.Id,
            What = dto.What,
            Why = dto.Why,
            Where = dto.Where,
            When = dto.When,
            Who = dto.Who,
            How = dto.How,
            HowMuch = dto.HowMuch,
            Status = dto.Status.ToString(),
            Priority = dto.Priority.ToString(),
            Notes = dto.Notes,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    private void ApplyClientSideFilters(List<TaskModel> models)
    {
        if (FilterStatus is not null)
        {
            models.RemoveAll(task => !string.Equals(task.Status, FilterStatus.Value.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        if (FilterPriority is not null)
        {
            models.RemoveAll(task => !string.Equals(task.Priority, FilterPriority.Value.ToString(), StringComparison.OrdinalIgnoreCase));
        }
    }

    private void UpdateDashboardMetrics(IReadOnlyCollection<TaskModel> models)
    {
        TotalActions = models.Count;
        PendingActions = models.Count(task => string.Equals(task.Status, nameof(TaskStatus.Pending), StringComparison.OrdinalIgnoreCase));
        CompletedActions = models.Count(task => string.Equals(task.Status, nameof(TaskStatus.Completed), StringComparison.OrdinalIgnoreCase));
        TotalCost = models.Sum(task => task.HowMuch);
        CriticalPendingActions = models.Count(task =>
            string.Equals(task.Priority, nameof(Priority.Critical), StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(task.Status, nameof(TaskStatus.Completed), StringComparison.OrdinalIgnoreCase));
        HasCriticalPendingActions = CriticalPendingActions > 0;

        PendingPercentText = TotalActions == 0 ? "0% do total" : $"{PendingActions * 100 / TotalActions}% do total";
        CompletedPercentText = TotalActions == 0 ? "0% do total" : $"{CompletedActions * 100 / TotalActions}% do total";
    }

    private void UpdateCharts(IReadOnlyCollection<TaskModel> models)
    {
        StatusChartModel = BuildStatusDonutChart(models);
        PriorityChartModel = BuildPriorityColumnChart(models);

        ResponsibleChartModel = BuildResponsibleChart(models);
        TimelineChartModel = BuildTimelineChart(models);
    }

    private static PlotModel CreateDarkPlotModel()
    {
        return new PlotModel
        {
            PlotAreaBorderColor = OxyColors.Transparent,
            TextColor = OxyColor.FromRgb(248, 250, 252),
            TitleColor = OxyColor.FromRgb(248, 250, 252),
            Background = OxyColors.Transparent,
            PlotAreaBackground = OxyColors.Transparent
        };
    }

    private static PlotModel BuildStatusDonutChart(IReadOnlyCollection<TaskModel> models)
    {
        var model = CreateDarkPlotModel();
        var pieSeries = new PieSeries
        {
            StrokeThickness = 0,
            InsideLabelFormat = string.Empty,
            OutsideLabelFormat = string.Empty,
            InnerDiameter = 0.58,
            AngleSpan = 360,
            StartAngle = 135,
            Diameter = 0.74
        };

        var statusOrder = new[]
        {
            nameof(TaskStatus.Pending),
            nameof(TaskStatus.InProgress),
            nameof(TaskStatus.Completed)
        };

        foreach (var status in statusOrder)
        {
            var count = models.Count(task => string.Equals(task.Status, status, StringComparison.OrdinalIgnoreCase));
            if (count > 0)
            {
                pieSeries.Slices.Add(new PieSlice(ToStatusLabel(status), count) { Fill = GetStatusColor(status) });
            }
        }

        if (pieSeries.Slices.Count == 0)
        {
            pieSeries.Slices.Add(new PieSlice("Sem dados", 1) { Fill = OxyColor.FromRgb(63, 69, 81) });
        }

        model.Series.Add(pieSeries);
        model.IsLegendVisible = true;
        return model;
    }

    private static PlotModel BuildPriorityColumnChart(IReadOnlyCollection<TaskModel> models)
    {
        var model = CreateDarkPlotModel();
        var categories = new[]
        {
            nameof(Priority.Low),
            nameof(Priority.Medium),
            nameof(Priority.High),
            nameof(Priority.Critical)
        };

        var categoryAxis = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Minimum = -0.5,
            Maximum = 3.5,
            MajorStep = 1,
            MinorStep = 1,
            TextColor = OxyColor.FromRgb(194, 180, 163),
            TicklineColor = OxyColors.Transparent,
            AxislineColor = OxyColors.Transparent,
            LabelFormatter = value =>
            {
                var index = (int)Math.Round(value);
                return index >= 0 && index < categories.Length ? ToPriorityLabel(categories[index]) : string.Empty;
            }
        };

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Minimum = 0,
            AbsoluteMinimum = 0,
            MajorGridlineColor = OxyColors.Transparent,
            MinorGridlineColor = OxyColors.Transparent,
            TicklineColor = OxyColors.Transparent,
            AxislineColor = OxyColors.Transparent,
            TextColor = OxyColor.FromRgb(194, 180, 163)
        };

        var barSeries = new RectangleBarSeries
        {
            StrokeThickness = 0
        };

        for (var index = 0; index < categories.Length; index++)
        {
            var category = categories[index];
            var count = models.Count(task => string.Equals(task.Priority, category, StringComparison.OrdinalIgnoreCase));
            barSeries.Items.Add(new RectangleBarItem(index - 0.17, 0, index + 0.17, count)
            {
                Color = GetPriorityColor(category)
            });
        }

        model.Axes.Add(categoryAxis);
        model.Axes.Add(valueAxis);
        model.Series.Add(barSeries);
        return model;
    }

    private static PlotModel BuildResponsibleChart(IReadOnlyCollection<TaskModel> models)
    {
        var model = CreateDarkPlotModel();
        var topResponsible = models
            .GroupBy(task => string.IsNullOrWhiteSpace(task.Who) ? "Nao informado" : task.Who)
            .OrderByDescending(group => group.Count())
            .Take(5)
            .ToList();

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Left,
            GapWidth = 12,
            IsTickCentered = true,
            TextColor = OxyColor.FromRgb(194, 180, 163),
            TicklineColor = OxyColors.Transparent,
            AxislineColor = OxyColors.Transparent
        };

        foreach (var group in topResponsible)
        {
            categoryAxis.Labels.Add(group.Key);
        }

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            MinimumPadding = 0,
            AbsoluteMinimum = 0,
            MajorGridlineColor = OxyColors.Transparent,
            MinorGridlineColor = OxyColors.Transparent,
            TicklineColor = OxyColors.Transparent,
            AxislineColor = OxyColors.Transparent,
            TextColor = OxyColor.FromRgb(194, 180, 163)
        };

        var barSeries = new BarSeries
        {
            FillColor = OxyColor.FromRgb(66, 165, 245),
            StrokeColor = OxyColors.Transparent,
            BarWidth = 16
        };

        foreach (var group in topResponsible)
        {
            barSeries.Items.Add(new BarItem(group.Count()));
        }

        model.Axes.Add(categoryAxis);
        model.Axes.Add(valueAxis);
        model.Series.Add(barSeries);
        return model;
    }

    private static PlotModel BuildTimelineChart(IReadOnlyCollection<TaskModel> models)
    {
        var model = CreateDarkPlotModel();
        var orderedGroups = models
            .GroupBy(task => new DateTime(task.When.Year, task.When.Month, 1))
            .OrderBy(group => group.Key)
            .ToList();

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            GapWidth = 0.6,
            TextColor = OxyColor.FromRgb(194, 180, 163)
        };

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            MinimumPadding = 0,
            AbsoluteMinimum = 0,
            TextColor = OxyColor.FromRgb(194, 180, 163)
        };

        var lineSeries = new LineSeries
        {
            Color = OxyColor.FromRgb(76, 175, 80),
            StrokeThickness = 3,
            MarkerType = MarkerType.Circle,
            MarkerSize = 4,
            MarkerFill = OxyColor.FromRgb(76, 175, 80)
        };

        if (orderedGroups.Count == 0)
        {
            categoryAxis.Labels.Add("Sem dados");
            lineSeries.Points.Add(new DataPoint(0, 0));
        }
        else
        {
            for (var i = 0; i < orderedGroups.Count; i++)
            {
                var group = orderedGroups[i];
                categoryAxis.Labels.Add(group.Key.ToString("MM/yyyy"));
                lineSeries.Points.Add(new DataPoint(i, group.Count()));
            }
        }

        model.Axes.Add(categoryAxis);
        model.Axes.Add(valueAxis);
        model.Series.Add(lineSeries);
        return model;
    }

    private static OxyColor GetStatusColor(string status) => status switch
    {
        nameof(TaskStatus.Completed) => OxyColor.FromRgb(32, 169, 91),
        nameof(TaskStatus.InProgress) => OxyColor.FromRgb(28, 82, 184),
        nameof(TaskStatus.OnHold) => OxyColor.FromRgb(41, 182, 246),
        nameof(TaskStatus.Cancelled) => OxyColor.FromRgb(158, 158, 158),
        _ => OxyColor.FromRgb(216, 146, 10)
    };

    private static OxyColor GetPriorityColor(string priority) => priority switch
    {
        nameof(Priority.Critical) => OxyColor.FromRgb(176, 18, 21),
        nameof(Priority.High) => OxyColor.FromRgb(216, 93, 5),
        nameof(Priority.Medium) => OxyColor.FromRgb(202, 138, 4),
        _ => OxyColor.FromRgb(32, 169, 91)
    };

    private static string ToStatusLabel(string status) => status switch
    {
        nameof(TaskStatus.Completed) => "Concluido",
        nameof(TaskStatus.InProgress) => "Em Andamento",
        nameof(TaskStatus.Pending) => "Pendente",
        nameof(TaskStatus.OnHold) => "Em Espera",
        nameof(TaskStatus.Cancelled) => "Cancelado",
        _ => status
    };

    private static string ToPriorityLabel(string priority) => priority switch
    {
        nameof(Priority.Critical) => "Critica",
        nameof(Priority.High) => "Alta",
        nameof(Priority.Medium) => "Media",
        nameof(Priority.Low) => "Baixa",
        _ => priority
    };
}
