namespace Presentation.WPF.Models;

/// <summary>
/// Represents a 5W2H task in the UI model.
/// </summary>
public class TaskModel
{
    public int Id { get; set; }
    public string What { get; set; } = string.Empty;
    public string Why { get; set; } = string.Empty;
    public string Where { get; set; } = string.Empty;
    public DateTime When { get; set; }
    public string Who { get; set; } = string.Empty;
    public string How { get; set; } = string.Empty;
    public decimal HowMuch { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
