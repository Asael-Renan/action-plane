using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using _5W2H.App.Core.Models;
using _5W2H.App.Core.Services;
using _5W2H.App.UI.Models;
using System.Collections.ObjectModel;
using OxyPlot;
using OxyPlot.Series;

namespace _5W2H.App.UI.ViewModels;

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
        var pieSeries = new PieSeries();
        
        if (summary.CompletedTasks > 0)
            pieSeries.Slices.Add(new PieSlice("Completas", summary.CompletedTasks) { Fill = OxyColor.FromRgb(76, 175, 80) });
        
        if (summary.InProgressTasks > 0)
            pieSeries.Slices.Add(new PieSlice("Em Progresso", summary.InProgressTasks) { Fill = OxyColor.FromRgb(255, 193, 7) });
        
        if (summary.PendingTasks > 0)
            pieSeries.Slices.Add(new PieSlice("Pendentes", summary.PendingTasks) { Fill = OxyColor.FromRgb(244, 67, 54) });

        model.Series.Add(pieSeries);
        StatusChartModel = model;
    }

    private void LoadPriorityChart()
    {
        var model = new PlotModel { Title = "" };
        var pieSeries = new PieSeries();
        
        pieSeries.Slices.Add(new PieSlice("Alta", TotalTasks > 0 ? TotalTasks / 3 : 0) { Fill = OxyColor.FromRgb(244, 67, 54) });
        pieSeries.Slices.Add(new PieSlice("Média", TotalTasks > 0 ? TotalTasks / 3 : 0) { Fill = OxyColor.FromRgb(255, 193, 7) });
        pieSeries.Slices.Add(new PieSlice("Baixa", TotalTasks > 0 ? TotalTasks / 3 : 0) { Fill = OxyColor.FromRgb(76, 175, 80) });

        model.Series.Add(pieSeries);
        PriorityChartModel = model;
    }

    private void LoadCostChart(DashboardSummaryDto summary)
    {
        var model = new PlotModel { Title = "" };
        
        var barSeries = new BarSeries();
        barSeries.Items.Add(new BarItem { Value = (double)summary.TotalCost });

        model.Series.Add(barSeries);
        CostChartModel = model;
    }

    private void LoadTrendChart(DashboardSummaryDto summary)
    {
        var model = new PlotModel { Title = "" };
        
        var lineSeries = new LineSeries
        {
            Color = OxyColor.FromRgb(33, 150, 243),
            StrokeThickness = 2,
            MarkerType = MarkerType.Circle,
            MarkerSize = 5,
            MarkerFill = OxyColor.FromRgb(33, 150, 243)
        };

        lineSeries.Points.Add(new DataPoint(1, summary.TotalTasks));
        lineSeries.Points.Add(new DataPoint(2, summary.CompletedTasks));
        lineSeries.Points.Add(new DataPoint(3, summary.InProgressTasks));
        lineSeries.Points.Add(new DataPoint(4, summary.PendingTasks));

        model.Series.Add(lineSeries);
        TrendChartModel = model;
    }
}
