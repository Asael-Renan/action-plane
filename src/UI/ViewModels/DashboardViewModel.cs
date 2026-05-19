using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiveW2H.App.Core.Models;
using FiveW2H.App.Core.Services;
using FiveW2H.App.UI.Models;
using System.Collections.ObjectModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using TaskStatus = FiveW2H.App.Core.Models.TaskStatus;

namespace FiveW2H.App.UI.ViewModels;

/// <summary>
/// ViewModel for the dashboard view showing statistics and charts.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly ITaskService _taskService;

    [ObservableProperty]
    private int totalTasks = 0;

    [ObservableProperty]
    private int completedTasks = 0;

    [ObservableProperty]
    private int inProgressTasks = 0;

    [ObservableProperty]
    private int pendingTasks = 0;

    [ObservableProperty]
    private decimal totalCost = 0;

    [ObservableProperty]
    private decimal averageCost = 0;

    [ObservableProperty]
    private ObservableCollection<string> statusLabels = new();

    [ObservableProperty]
    private ObservableCollection<string> costLabels = new();

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private PlotModel statusChartModel = new();

    [ObservableProperty]
    private PlotModel priorityChartModel = new();

    [ObservableProperty]
    private PlotModel costChartModel = new();

    [ObservableProperty]
    private PlotModel trendChartModel = new();

    public DashboardViewModel(ITaskService taskService)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
    }

    [RelayCommand]
    public async Task LoadDashboard()
    {
        try
        {
            IsLoading = true;

            var summary = await _taskService.GetDashboardSummaryAsync();

            TotalTasks = summary.TotalTasks;
            CompletedTasks = summary.CompletedTasks;
            InProgressTasks = summary.InProgressTasks;
            PendingTasks = summary.PendingTasks;
            TotalCost = summary.TotalCost;
            AverageCost = summary.AverageCost;

            LoadStatusLabels(summary.TasksByStatus);
            LoadCostLabels(summary.CostByPriority);
            LoadChartData(summary);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Erro ao carregar dashboard: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadStatusLabels(Dictionary<string, int> statusData)
    {
        StatusLabels = new ObservableCollection<string>(statusData.Keys);
    }

    private void LoadCostLabels(Dictionary<string, decimal> costData)
    {
        CostLabels = new ObservableCollection<string>(costData.Keys);
    }

    private void LoadChartData(DashboardSummaryDto summary)
    {
        LoadStatusChart(summary);
        LoadPriorityChart();
        LoadCostChart(summary);
        LoadTrendChart(summary);
    }

    private void LoadStatusChart(DashboardSummaryDto summary)
    {
        var model = new PlotModel { Title = "" };
        var pieSeries = new PieSeries
        {
            StrokeThickness = 0,
            InsideLabelFormat = string.Empty,
            OutsideLabelFormat = string.Empty,
            InnerDiameter = 0.58,
            AngleSpan = 360,
            StartAngle = 135
        };
        
        if (summary.CompletedTasks > 0)
            pieSeries.Slices.Add(new PieSlice("Completas", summary.CompletedTasks) { Fill = OxyColor.FromRgb(34, 197, 94) });
        
        if (summary.InProgressTasks > 0)
            pieSeries.Slices.Add(new PieSlice("Em Progresso", summary.InProgressTasks) { Fill = OxyColor.FromRgb(59, 130, 246) });
        
        if (summary.PendingTasks > 0)
            pieSeries.Slices.Add(new PieSlice("Pendentes", summary.PendingTasks) { Fill = OxyColor.FromRgb(251, 146, 60) });

        if (pieSeries.Slices.Count == 0)
            pieSeries.Slices.Add(new PieSlice("Sem dados", 1) { Fill = OxyColor.FromRgb(203, 213, 225) });

        model.Series.Add(pieSeries);
        model.IsLegendVisible = true;
        StatusChartModel = model;
    }

    private void LoadPriorityChart()
    {
        var model = new PlotModel { Title = "" };
        var pieSeries = new PieSeries
        {
            StrokeThickness = 0,
            InsideLabelFormat = string.Empty,
            OutsideLabelFormat = string.Empty,
            InnerDiameter = 0.58,
            AngleSpan = 360,
            StartAngle = 135
        };
        
        if (TotalTasks > 0)
        {
            pieSeries.Slices.Add(new PieSlice("Alta", TotalTasks / 3) { Fill = OxyColor.FromRgb(249, 115, 22) });
            pieSeries.Slices.Add(new PieSlice("Média", TotalTasks / 3) { Fill = OxyColor.FromRgb(234, 179, 8) });
            pieSeries.Slices.Add(new PieSlice("Baixa", TotalTasks / 3) { Fill = OxyColor.FromRgb(34, 197, 94) });
        }
        else
        {
            pieSeries.Slices.Add(new PieSlice("Sem dados", 1) { Fill = OxyColor.FromRgb(203, 213, 225) });
        }

        model.Series.Add(pieSeries);
        model.IsLegendVisible = true;
        PriorityChartModel = model;
    }

    private void LoadCostChart(DashboardSummaryDto summary)
    {
        var model = new PlotModel { Title = "" };
        
        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Left,
            IsTickCentered = true,
            TicklineColor = OxyColors.Transparent,
            AxislineColor = OxyColors.Transparent
        };
        categoryAxis.Labels.Add("Custo Total");

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            MinimumPadding = 0,
            AbsoluteMinimum = 0,
            MajorGridlineColor = OxyColors.Transparent,
            TicklineColor = OxyColors.Transparent,
            AxislineColor = OxyColors.Transparent
        };
        
        var barSeries = new BarSeries
        {
            FillColor = OxyColor.FromRgb(109, 40, 217),
            StrokeThickness = 0,
            BarWidth = 16
        };
        barSeries.Items.Add(new BarItem { Value = (double)summary.TotalCost });

        model.Axes.Add(categoryAxis);
        model.Axes.Add(valueAxis);
        model.Series.Add(barSeries);
        CostChartModel = model;
    }

    private void LoadTrendChart(DashboardSummaryDto summary)
    {
        var model = new PlotModel { Title = "" };
        
        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            TicklineColor = OxyColors.Transparent,
            AxislineColor = OxyColors.Transparent
        };
        categoryAxis.Labels.Add("Total");
        categoryAxis.Labels.Add("Completas");
        categoryAxis.Labels.Add("Em Progresso");
        categoryAxis.Labels.Add("Pendentes");

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            MinimumPadding = 0,
            AbsoluteMinimum = 0,
            MajorGridlineColor = OxyColors.Transparent,
            TicklineColor = OxyColors.Transparent,
            AxislineColor = OxyColors.Transparent
        };
        
        var lineSeries = new LineSeries
        {
            Color = OxyColor.FromRgb(34, 197, 94),
            StrokeThickness = 3,
            MarkerType = MarkerType.Circle,
            MarkerSize = 6,
            MarkerFill = OxyColor.FromRgb(34, 197, 94),
            MarkerStroke = OxyColors.White,
            MarkerStrokeThickness = 2
        };

        lineSeries.Points.Add(new DataPoint(0, summary.TotalTasks));
        lineSeries.Points.Add(new DataPoint(1, summary.CompletedTasks));
        lineSeries.Points.Add(new DataPoint(2, summary.InProgressTasks));
        lineSeries.Points.Add(new DataPoint(3, summary.PendingTasks));

        model.Axes.Add(categoryAxis);
        model.Axes.Add(valueAxis);
        model.Series.Add(lineSeries);
        TrendChartModel = model;
    }

    private static OxyColor GetStatusColor(string status) => status switch
    {
        nameof(TaskStatus.Completed) => OxyColor.FromRgb(34, 197, 94),
        nameof(TaskStatus.InProgress) => OxyColor.FromRgb(59, 130, 246),
        nameof(TaskStatus.OnHold) => OxyColor.FromRgb(251, 146, 60),
        nameof(TaskStatus.Cancelled) => OxyColor.FromRgb(239, 68, 68),
        _ => OxyColor.FromRgb(148, 163, 184)
    };

    private static OxyColor GetPriorityColor(string priority) => priority switch
    {
        nameof(Priority.Critical) => OxyColor.FromRgb(220, 38, 38),
        nameof(Priority.High) => OxyColor.FromRgb(249, 115, 22),
        nameof(Priority.Medium) => OxyColor.FromRgb(234, 179, 8),
        _ => OxyColor.FromRgb(34, 197, 94)
    };
}