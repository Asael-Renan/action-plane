using Domain.Enums;

namespace Application.DTOs;

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
    public Domain.Enums.TaskStatus Status { get; set; }
    public Priority Priority { get; set; }
    public string Notes { get; set; } = string.Empty;
}
