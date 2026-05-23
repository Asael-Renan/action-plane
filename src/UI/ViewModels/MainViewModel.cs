using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using FiveW2H.App.Application;
using FiveW2H.App.Core.Models;
using FiveW2H.App.Infrastructure.Settings;
using FiveW2H.App.Infrastructure.Updates;
using FiveW2H.App.UI.Models;
using FiveW2H.App.UI.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using TaskStatus = FiveW2H.App.Core.Models.TaskStatus;

namespace FiveW2H.App.UI.ViewModels;

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
    private readonly IAppUpdateService _appUpdateService;
    private readonly IThemeService _themeService;

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
    private string? filterCompany;

    [ObservableProperty]
    private FiveW2H.App.Core.Models.TaskStatus? filterStatus;

    [ObservableProperty]
    private Priority? filterPriority;

    [ObservableProperty]
    private string newWhat = string.Empty;

    [ObservableProperty]
    private string newWhy = string.Empty;

    [ObservableProperty]
    private string newWhere = string.Empty;

    [ObservableProperty]
    private string newCompany = string.Empty;

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
    private PlotModel companyDistributionChartModel = new();

    [ObservableProperty]
    private PlotModel companyCostChartModel = new();

    [ObservableProperty]
    private PlotModel timelineChartModel = new();

    [ObservableProperty]
    private bool isDarkTheme = true;

    [ObservableProperty]
    private ObservableCollection<string> companyOptions = new();

    [ObservableProperty]
    private ObservableCollection<string> responsibleOptions = new();

    [ObservableProperty]
    private GroupingOption selectedGrouping = new() { Label = "Agrupar por empresa", PropertyName = nameof(TaskModel.CompanyGroupLabel) };

    private ICollectionView _tasksView;

    public string ThemeToggleGlyph => IsDarkTheme ? "\uE706" : "\uE708";
    public ICollectionView TasksView => _tasksView;

    public IReadOnlyList<Priority> Priorities { get; } = Enum.GetValues<Priority>();
    public IReadOnlyList<FiveW2H.App.Core.Models.TaskStatus> Statuses { get; } = Enum.GetValues<FiveW2H.App.Core.Models.TaskStatus>();
    public IReadOnlyList<GroupingOption> GroupingOptions { get; } =
    [
        new() { Label = "Sem agrupamento", PropertyName = string.Empty },
        new() { Label = "Agrupar por empresa", PropertyName = nameof(TaskModel.CompanyGroupLabel) },
        new() { Label = "Agrupar por responsavel", PropertyName = nameof(TaskModel.Who) },
        new() { Label = "Agrupar por status", PropertyName = nameof(TaskModel.Status) }
    ];

    public MainViewModel(
        ITaskService taskService,
        IBackupService backupService,
        IDialogService dialogService,
        IFileDialogService fileDialogService,
        IMessageDialogService messageDialogService,
        IAppUpdateService appUpdateService,
        IThemeService themeService)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
        _messageDialogService = messageDialogService ?? throw new ArgumentNullException(nameof(messageDialogService));
        _appUpdateService = appUpdateService ?? throw new ArgumentNullException(nameof(appUpdateService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _tasksView = CollectionViewSource.GetDefaultView(Tasks);
        IsDarkTheme = _themeService.IsDarkTheme;
        _themeService.ThemeChanged += OnThemeChanged;
        ApplyGrouping();
    }

    [RelayCommand]
    private void ToggleTheme() => _themeService.Toggle();

    partial void OnIsDarkThemeChanged(bool value)
    {
        OnPropertyChanged(nameof(ThemeToggleGlyph));
    }

    partial void OnTasksChanged(ObservableCollection<TaskModel> value)
    {
        _tasksView = CollectionViewSource.GetDefaultView(value);
        OnPropertyChanged(nameof(TasksView));
        ApplyGrouping();
    }

    partial void OnSelectedGroupingChanged(GroupingOption value)
    {
        ApplyGrouping();
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        IsDarkTheme = _themeService.IsDarkTheme;
        if (Tasks.Count > 0)
        {
            UpdateCharts(Tasks.ToList());
        }
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
            UpdateFilterOptions(models);
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
                FilterResponsible,
                FilterCompany
            );

            var models = taskDtos.Select(MapToModel).ToList();
            ApplyClientSideFilters(models);
            Tasks = new ObservableCollection<TaskModel>(models);
            UpdateFilterOptions(models);
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
        FilterCompany = null;
        FilterStatus = null;
        FilterPriority = null;
        await LoadTasks();
    }

    [RelayCommand]
    public async Task CheckForUpdates()
    {
        await CheckForUpdatesCore(showInteractiveMessages: true);
    }

    public async Task CheckForUpdatesOnStartupAsync()
    {
        await CheckForUpdatesCore(showInteractiveMessages: false);
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
                Company = NewCompany.Trim(),
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
        NewCompany = string.Empty;
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
        var filePath = _fileDialogService.ShowSaveExportDialog();
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
            await _backupService.ExportAsync(filePath, taskDtos);

            StatusMessage = $"Exported {taskDtos.Count()} task(s)";
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
        var filePath = _fileDialogService.ShowOpenImportDialog();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            StatusMessage = "Import cancelled";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Importing data...";

            var result = await _backupService.ImportAsync(filePath);
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

    private async Task CheckForUpdatesCore(bool showInteractiveMessages)
    {
        var previousStatus = StatusMessage;

        try
        {
            IsLoading = true;
            StatusMessage = "Verificando atualizacoes...";

            var result = await _appUpdateService.CheckForUpdatesAsync();

            if (!result.IsInstalled)
            {
                StatusMessage = showInteractiveMessages ? result.Message : previousStatus;

                if (showInteractiveMessages)
                {
                    _messageDialogService.ShowWarning(result.Message, "Atualizacoes");
                }

                return;
            }

            if (!result.IsUpdateAvailable)
            {
                StatusMessage = showInteractiveMessages ? result.Message : previousStatus;
                return;
            }

            StatusMessage = result.Message;
            var availableVersion = string.IsNullOrWhiteSpace(result.AvailableVersion)
                ? "nova versao"
                : $"versao {result.AvailableVersion}";

            if (!_messageDialogService.Confirm(
                $"A {availableVersion} esta disponivel. Baixar, aplicar e reiniciar agora?",
                "Atualizacao disponivel"))
            {
                StatusMessage = "Atualizacao adiada";
                return;
            }

            StatusMessage = "Baixando atualizacao...";
            await _appUpdateService.DownloadAndApplyUpdateAsync();
            StatusMessage = "Atualizacao baixada. Reiniciando...";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro de atualizacao: {ex.Message}";

            if (showInteractiveMessages)
            {
                _messageDialogService.ShowWarning(StatusMessage, "Atualizacoes");
            }
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
            Company = dto.Company,
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

    private void UpdateFilterOptions(IEnumerable<TaskModel> models)
    {
        CompanyOptions = new ObservableCollection<string>(models
            .Select(task => task.Company.Trim())
            .Where(company => !string.IsNullOrWhiteSpace(company))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(company => company, StringComparer.CurrentCultureIgnoreCase));

        ResponsibleOptions = new ObservableCollection<string>(models
            .Select(task => task.Who.Trim())
            .Where(responsible => !string.IsNullOrWhiteSpace(responsible))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(responsible => responsible, StringComparer.CurrentCultureIgnoreCase));
    }

    private void ApplyGrouping()
    {
        var view = TasksView;
        if (view is null)
        {
            return;
        }

        using (view.DeferRefresh())
        {
            view.GroupDescriptions.Clear();

            if (!string.IsNullOrWhiteSpace(SelectedGrouping.PropertyName))
            {
                view.GroupDescriptions.Add(new PropertyGroupDescription(SelectedGrouping.PropertyName));
            }
        }
    }

    private void UpdateDashboardMetrics(List<TaskModel> models)
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
        CompanyDistributionChartModel = BuildCompanyDistributionChart(models);
        CompanyCostChartModel = BuildCompanyCostChart(models);
        TimelineChartModel = BuildTimelineChart(models);
    }

    private PlotModel CreatePlotModel()
    {
        var textColor = IsDarkTheme ? OxyColor.FromRgb(248, 250, 252) : OxyColor.FromRgb(15, 23, 42);
        return new PlotModel
        {
            PlotAreaBorderColor = OxyColors.Transparent,
            TextColor = textColor,
            TitleColor = textColor,
            Background = OxyColors.Transparent,
            PlotAreaBackground = OxyColors.Transparent
        };
    }

    private OxyColor ChartAxisTextColor => IsDarkTheme
        ? OxyColor.FromRgb(194, 180, 163)
        : OxyColor.FromRgb(100, 116, 139);

    private PlotModel BuildStatusDonutChart(IReadOnlyCollection<TaskModel> models)
    {
        var model = CreatePlotModel();
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
            pieSeries.Slices.Add(new PieSlice("Sem dados", 1)
            {
                Fill = IsDarkTheme ? OxyColor.FromRgb(63, 69, 81) : OxyColor.FromRgb(203, 213, 225)
            });
        }

        model.Series.Add(pieSeries);
        model.IsLegendVisible = true;
        return model;
    }

    private PlotModel BuildPriorityColumnChart(IReadOnlyCollection<TaskModel> models)
    {
        var model = CreatePlotModel();
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
            TextColor = ChartAxisTextColor,
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
            TextColor = ChartAxisTextColor
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

    private PlotModel BuildCompanyDistributionChart(IReadOnlyCollection<TaskModel> models)
    {
        var model = CreatePlotModel();
        var topCompanies = models
            .GroupBy(task => GetCompanyLabel(task.Company))
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.CurrentCultureIgnoreCase)
            .Take(6)
            .ToList();

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Left,
            GapWidth = 12,
            IsTickCentered = true,
            TextColor = ChartAxisTextColor,
            TicklineColor = OxyColors.Transparent,
            AxislineColor = OxyColors.Transparent
        };

        if (topCompanies.Count == 0)
        {
            categoryAxis.Labels.Add("Sem dados");
        }
        else
        {
            foreach (var group in topCompanies)
            {
                categoryAxis.Labels.Add(group.Key);
            }
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
            TextColor = ChartAxisTextColor
        };

        var barSeries = new BarSeries
        {
            FillColor = OxyColor.FromRgb(88, 101, 242),
            StrokeColor = OxyColors.Transparent,
            BarWidth = 16
        };

        if (topCompanies.Count == 0)
        {
            barSeries.Items.Add(new BarItem(0));
        }
        else
        {
            foreach (var group in topCompanies)
            {
                barSeries.Items.Add(new BarItem(group.Count()));
            }
        }

        model.Axes.Add(categoryAxis);
        model.Axes.Add(valueAxis);
        model.Series.Add(barSeries);
        return model;
    }

    private PlotModel BuildCompanyCostChart(IReadOnlyCollection<TaskModel> models)
    {
        var model = CreatePlotModel();
        var topCompanies = models
            .GroupBy(task => GetCompanyLabel(task.Company))
            .Select(group => new
            {
                Company = group.Key,
                TotalCost = group.Sum(task => task.HowMuch)
            })
            .OrderByDescending(group => group.TotalCost)
            .ThenBy(group => group.Company, StringComparer.CurrentCultureIgnoreCase)
            .Take(6)
            .ToList();

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Left,
            GapWidth = 12,
            IsTickCentered = true,
            TextColor = ChartAxisTextColor,
            TicklineColor = OxyColors.Transparent,
            AxislineColor = OxyColors.Transparent
        };

        if (topCompanies.Count == 0)
        {
            categoryAxis.Labels.Add("Sem dados");
        }
        else
        {
            foreach (var group in topCompanies)
            {
                categoryAxis.Labels.Add(group.Company);
            }
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
            TextColor = ChartAxisTextColor,
            StringFormat = "R$ #,##0"
        };

        var barSeries = new BarSeries
        {
            FillColor = OxyColor.FromRgb(34, 197, 94),
            StrokeColor = OxyColors.Transparent,
            BarWidth = 16
        };

        if (topCompanies.Count == 0)
        {
            barSeries.Items.Add(new BarItem(0));
        }
        else
        {
            foreach (var group in topCompanies)
            {
                barSeries.Items.Add(new BarItem((double)group.TotalCost));
            }
        }

        model.Axes.Add(categoryAxis);
        model.Axes.Add(valueAxis);
        model.Series.Add(barSeries);
        return model;
    }

    private PlotModel BuildTimelineChart(IReadOnlyCollection<TaskModel> models)
    {
        var model = CreatePlotModel();
        var orderedGroups = models
            .GroupBy(task => new DateTime(task.When.Year, task.When.Month, 1))
            .OrderBy(group => group.Key)
            .ToList();

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            GapWidth = 0.6,
            TextColor = ChartAxisTextColor
        };

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            MinimumPadding = 0,
            AbsoluteMinimum = 0,
            TextColor = ChartAxisTextColor
        };

        var lineSeries = new LineSeries
        {
            Color = OxyColor.FromRgb(34, 197, 94),
            StrokeThickness = 3,
            MarkerType = MarkerType.Circle,
            MarkerSize = 4,
            MarkerFill = OxyColor.FromRgb(34, 197, 94)
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
                categoryAxis.Labels.Add(group.Key.ToString("MM/yyyy", CultureInfo.InvariantCulture));
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
        nameof(TaskStatus.Completed) => OxyColor.FromRgb(34, 197, 94),      // Verde vibrante
        nameof(TaskStatus.InProgress) => OxyColor.FromRgb(59, 130, 246),    // Azul
        nameof(TaskStatus.OnHold) => OxyColor.FromRgb(251, 146, 60),        // Laranja
        nameof(TaskStatus.Cancelled) => OxyColor.FromRgb(239, 68, 68),      // Vermelho
        _ => OxyColor.FromRgb(148, 163, 184)                                // Cinza
    };

    private static OxyColor GetPriorityColor(string priority) => priority switch
    {
        nameof(Priority.Critical) => OxyColor.FromRgb(220, 38, 38),     // Vermelho intenso
        nameof(Priority.High) => OxyColor.FromRgb(249, 115, 22),        // Laranja
        nameof(Priority.Medium) => OxyColor.FromRgb(234, 179, 8),       // Amarelo
        _ => OxyColor.FromRgb(34, 197, 94)                              // Verde
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

    private static string GetCompanyLabel(string? company)
    {
        return string.IsNullOrWhiteSpace(company)
            ? "Sem empresa"
            : company.Trim();
    }
}
