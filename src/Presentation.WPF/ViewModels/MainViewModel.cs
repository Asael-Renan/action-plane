using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Presentation.WPF.Models;
using System.Collections.ObjectModel;
using DomainTaskStatus = Domain.Enums.TaskStatus;

namespace Presentation.WPF.ViewModels;

/// <summary>
/// ViewModel for the main task management window.
/// Handles displaying, filtering, and managing 5W2H tasks.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IFiveW2HTaskService _taskService;

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
    private DomainTaskStatus? filterStatus;

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
    private PlotModel statusChartModel = new();

    [ObservableProperty]
    private PlotModel priorityChartModel = new();

    [ObservableProperty]
    private PlotModel responsibleChartModel = new();

    [ObservableProperty]
    private PlotModel timelineChartModel = new();

    public IReadOnlyList<Priority> Priorities { get; } = Enum.GetValues<Priority>();

    public IReadOnlyList<DomainTaskStatus> Statuses { get; } = Enum.GetValues<DomainTaskStatus>();

    public MainViewModel(IFiveW2HTaskService taskService)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
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
    public void OpenFilterScreen()
    {
        SelectedTabIndex = 1;
    }

    [RelayCommand]
    public async Task DeleteSelectedTask()
    {
        if (SelectedTask is null)
        {
            StatusMessage = "Please select a task to delete";
            return;
        }

        if (MessageBox.Show(
            $"Are you sure you want to delete '{SelectedTask.What}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo) != MessageBoxResult.Yes)
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

    private void UpdateCharts(IReadOnlyCollection<TaskModel> models)
    {
        StatusChartModel = BuildPieChart(
            string.Empty,
            models.GroupBy(task => task.Status)
                .Select(group => (Label: group.Key, Value: group.Count(), Color: GetStatusColor(group.Key))));

        PriorityChartModel = BuildPieChart(
            string.Empty,
            models.GroupBy(task => task.Priority)
                .Select(group => (Label: group.Key, Value: group.Count(), Color: GetPriorityColor(group.Key))));

        ResponsibleChartModel = BuildResponsibleChart(models);
        TimelineChartModel = BuildTimelineChart(models);
    }

    private static PlotModel BuildPieChart(string title, IEnumerable<(string Label, int Value, OxyColor Color)> slices)
    {
        var model = new PlotModel { Title = title };
        var pieSeries = new PieSeries
        {
            StrokeThickness = 0,
            InsideLabelPosition = 0.72,
            AngleSpan = 360,
            StartAngle = 0
        };

        foreach (var slice in slices.Where(slice => slice.Value > 0))
        {
            pieSeries.Slices.Add(new PieSlice(slice.Label, slice.Value) { Fill = slice.Color });
        }

        if (pieSeries.Slices.Count == 0)
        {
            pieSeries.Slices.Add(new PieSlice("Sem dados", 1) { Fill = OxyColor.FromRgb(189, 189, 189) });
        }

        model.Series.Add(pieSeries);
        return model;
    }

    private static PlotModel BuildResponsibleChart(IReadOnlyCollection<TaskModel> models)
    {
        var model = new PlotModel();
        var topResponsible = models
            .GroupBy(task => string.IsNullOrWhiteSpace(task.Who) ? "Nao informado" : task.Who)
            .OrderByDescending(group => group.Count())
            .Take(5)
            .ToList();

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Left,
            GapWidth = 12,
            IsTickCentered = true
        };

        foreach (var group in topResponsible)
        {
            categoryAxis.Labels.Add(group.Key);
        }

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            MinimumPadding = 0,
            AbsoluteMinimum = 0
        };

        var barSeries = new BarSeries
        {
            FillColor = OxyColor.FromRgb(33, 150, 243),
            StrokeColor = OxyColors.Transparent,
            LabelPlacement = LabelPlacement.Inside,
            LabelFormatString = "{0:0}"
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
        var model = new PlotModel();
        var orderedGroups = models
            .GroupBy(task => new DateTime(task.When.Year, task.When.Month, 1))
            .OrderBy(group => group.Key)
            .ToList();

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            GapWidth = 0.6
        };

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            MinimumPadding = 0,
            AbsoluteMinimum = 0
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
        nameof(DomainTaskStatus.Completed) => OxyColor.FromRgb(76, 175, 80),
        nameof(DomainTaskStatus.InProgress) => OxyColor.FromRgb(255, 193, 7),
        nameof(DomainTaskStatus.OnHold) => OxyColor.FromRgb(41, 182, 246),
        nameof(DomainTaskStatus.Cancelled) => OxyColor.FromRgb(158, 158, 158),
        _ => OxyColor.FromRgb(244, 67, 54)
    };

    private static OxyColor GetPriorityColor(string priority) => priority switch
    {
        nameof(Priority.Critical) => OxyColor.FromRgb(183, 28, 28),
        nameof(Priority.High) => OxyColor.FromRgb(244, 67, 54),
        nameof(Priority.Medium) => OxyColor.FromRgb(255, 193, 7),
        _ => OxyColor.FromRgb(76, 175, 80)
    };
}
