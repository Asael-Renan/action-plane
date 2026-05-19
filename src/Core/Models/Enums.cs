namespace FiveW2H.App.Core.Models;

/// <summary>
/// Represents the status of a 5W2H task.
/// </summary>
public enum TaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    OnHold = 3,
    Cancelled = 4
}

/// <summary>
/// Represents the priority level of a task.
/// </summary>
public enum Priority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
