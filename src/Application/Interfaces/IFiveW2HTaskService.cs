using Application.DTOs;

namespace Application.Interfaces;

/// <summary>
/// Application service for managing 5W2H tasks.
/// Contains business logic and use cases.
/// </summary>
public interface IFiveW2HTaskService
{
    /// <summary>Gets a task by ID.</summary>
    Task<FiveW2HTaskDto?> GetTaskAsync(int id);

    /// <summary>Gets all tasks.</summary>
    Task<IEnumerable<FiveW2HTaskDto>> GetAllTasksAsync();

    /// <summary>Searches and filters tasks.</summary>
    Task<IEnumerable<FiveW2HTaskDto>> SearchTasksAsync(
        string? searchText = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? responsible = null);

    /// <summary>Creates a new task.</summary>
    Task<FiveW2HTaskDto> CreateTaskAsync(CreateFiveW2HTaskDto dto);

    /// <summary>Updates an existing task.</summary>
    Task<FiveW2HTaskDto> UpdateTaskAsync(UpdateFiveW2HTaskDto dto);

    /// <summary>Deletes a task.</summary>
    Task<bool> DeleteTaskAsync(int id);

    /// <summary>Gets dashboard summary statistics.</summary>
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
}
