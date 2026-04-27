namespace Domain.Enums;

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
