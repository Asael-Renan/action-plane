using Domain.Entities;

namespace Domain.Interfaces;

/// <summary>
/// Repository interface for 5W2H task operations.
/// Follows the Repository pattern for data access abstraction.
/// </summary>
public interface IFiveW2HRepository
{
    /// <summary>Gets a task by its identifier.</summary>
    Task<FiveW2HTask?> GetByIdAsync(int id);

    /// <summary>Gets all tasks.</summary>
    Task<IEnumerable<FiveW2HTask>> GetAllAsync();

    /// <summary>Gets all tasks with optional filters.</summary>
    Task<IEnumerable<FiveW2HTask>> GetFilteredAsync(
        string? searchText = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? responsible = null);

    /// <summary>Adds a new task to the repository.</summary>
    Task<int> AddAsync(FiveW2HTask task);

    /// <summary>Updates an existing task.</summary>
    Task<bool> UpdateAsync(FiveW2HTask task);

    /// <summary>Deletes a task by its identifier.</summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>Checks if a task with the given identifier exists.</summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>Gets the total count of tasks.</summary>
    Task<int> GetCountAsync();

    /// <summary>Gets total cost of all tasks.</summary>
    Task<decimal> GetTotalCostAsync();
}
