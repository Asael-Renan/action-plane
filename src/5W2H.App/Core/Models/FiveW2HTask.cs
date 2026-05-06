namespace _5W2H.App.Core.Models;

/// <summary>
/// Represents a 5W2H (What, Why, Where, When, Who, How, How Much) task entity.
/// This is the core domain entity for the application.
/// </summary>
public class FiveW2HTask
{
    /// <summary>Gets or sets the unique identifier for the task.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets WHAT - the task or action to be performed.</summary>
    public string What { get; set; } = string.Empty;

    /// <summary>Gets or sets WHY - the reason or objective for the task.</summary>
    public string Why { get; set; } = string.Empty;

    /// <summary>Gets or sets WHERE - the location or scope where the task occurs.</summary>
    public string Where { get; set; } = string.Empty;

    /// <summary>Gets or sets WHEN - the scheduled date/time for the task.</summary>
    public DateTime When { get; set; }

    /// <summary>Gets or sets WHO - the person responsible for the task.</summary>
    public string Who { get; set; } = string.Empty;

    /// <summary>Gets or sets HOW - the method or process to accomplish the task.</summary>
    public string How { get; set; } = string.Empty;

    /// <summary>Gets or sets HOW MUCH - the estimated cost or budget.</summary>
    public decimal HowMuch { get; set; }

    /// <summary>Gets or sets the current status of the task.</summary>
    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    /// <summary>Gets or sets the priority level of the task.</summary>
    public Priority Priority { get; set; } = Priority.Medium;

    /// <summary>Gets or sets additional notes or remarks.</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>Gets or sets when the task was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets when the task was last updated.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Validates the task entity for required fields.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(What) &&
               !string.IsNullOrWhiteSpace(Why) &&
               !string.IsNullOrWhiteSpace(Who) &&
               !string.IsNullOrWhiteSpace(How) &&
               HowMuch >= 0;
    }
}
