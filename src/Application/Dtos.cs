using FiveW2H.App.Core.Models;

namespace FiveW2H.App.Application;

/// <summary>
/// Data Transfer Object for displaying a 5W2H task in the UI.
/// </summary>
public class FiveW2HTaskDto
{
    public int Id { get; set; }
    public string What { get; set; } = string.Empty;
    public string Why { get; set; } = string.Empty;
    public string Where { get; set; } = string.Empty;
    public DateTime When { get; set; }
    public string Who { get; set; } = string.Empty;
    public string How { get; set; } = string.Empty;
    public decimal HowMuch { get; set; }
    public FiveW2H.App.Core.Models.TaskStatus Status { get; set; }
    public Priority Priority { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Data Transfer Object for creating a 5W2H task.
/// </summary>
public class CreateFiveW2HTaskDto
{
    public string What { get; set; } = string.Empty;
    public string Why { get; set; } = string.Empty;
    public string Where { get; set; } = string.Empty;
    public DateTime When { get; set; }
    public string Who { get; set; } = string.Empty;
    public string How { get; set; } = string.Empty;
    public decimal HowMuch { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Data Transfer Object for updating a 5W2H task.
/// </summary>
public class UpdateFiveW2HTaskDto
{
    public int Id { get; set; }
    public string What { get; set; } = string.Empty;
    public string Why { get; set; } = string.Empty;
    public string Where { get; set; } = string.Empty;
    public DateTime When { get; set; }
    public string Who { get; set; } = string.Empty;
    public string How { get; set; } = string.Empty;
    public decimal HowMuch { get; set; }
    public FiveW2H.App.Core.Models.TaskStatus Status { get; set; }
    public Priority Priority { get; set; }
    public string Notes { get; set; } = string.Empty;
}

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

/// <summary>
/// Result summary for CSV import operations.
/// </summary>
public class ImportResultDto
{
    public int ImportedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; } = new();
}
