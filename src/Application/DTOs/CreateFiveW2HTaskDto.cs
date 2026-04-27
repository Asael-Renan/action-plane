using Domain.Enums;

namespace Application.DTOs;

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
