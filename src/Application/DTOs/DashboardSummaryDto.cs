namespace Application.DTOs;

/// <summary>
/// Dashboard statistics summary.
/// </summary>
public class DashboardSummaryDto
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int PendingTasks { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AverageCost { get; set; }
    public Dictionary<string, int> TasksByResponsible { get; set; } = new();
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
    public Dictionary<string, decimal> CostByPriority { get; set; } = new();
}
