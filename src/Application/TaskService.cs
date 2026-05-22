using FiveW2H.App.Core.Models;
using FiveW2H.App.Data;
using TaskStatus = FiveW2H.App.Core.Models.TaskStatus;

namespace FiveW2H.App.Application;

/// <summary>
/// Application service for managing 5W2H tasks.
/// Implements business logic and coordinates between Domain and Data layers.
/// </summary>
public interface ITaskService
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
        string? responsible = null,
        string? company = null);

    /// <summary>Creates a new task.</summary>
    Task<FiveW2HTaskDto> CreateTaskAsync(CreateFiveW2HTaskDto dto);

    /// <summary>Updates an existing task.</summary>
    Task<FiveW2HTaskDto> UpdateTaskAsync(UpdateFiveW2HTaskDto dto);

    /// <summary>Deletes a task.</summary>
    Task<bool> DeleteTaskAsync(int id);

    /// <summary>Gets dashboard summary statistics.</summary>
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
}

/// <summary>
/// Application service for managing 5W2H tasks.
/// Implements business logic and coordinates between Domain and Data layers.
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;

    public TaskService(ITaskRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<FiveW2HTaskDto?> GetTaskAsync(int id)
    {
        var task = await _repository.GetByIdAsync(id);
        return task is not null ? MapToDto(task) : null;
    }

    public async Task<IEnumerable<FiveW2HTaskDto>> GetAllTasksAsync()
    {
        var tasks = await _repository.GetAllAsync();
        return tasks.Select(MapToDto).ToList();
    }

    public async Task<IEnumerable<FiveW2HTaskDto>> SearchTasksAsync(
        string? searchText = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? responsible = null,
        string? company = null)
    {
        var tasks = await _repository.GetFilteredAsync(searchText, startDate, endDate, responsible, company);
        return tasks.Select(MapToDto).ToList();
    }

    public async Task<FiveW2HTaskDto> CreateTaskAsync(CreateFiveW2HTaskDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        ValidateTaskDto(dto);

        var task = new FiveW2HTask
        {
            What = dto.What,
            Why = dto.Why,
            Where = dto.Where,
            Company = dto.Company,
            When = dto.When,
            Who = dto.Who,
            How = dto.How,
            HowMuch = dto.HowMuch,
            Priority = dto.Priority,
            Notes = dto.Notes,
            Status = TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!task.IsValid())
            throw new InvalidOperationException("Task is not valid");

        var id = await _repository.AddAsync(task);
        task.Id = id;

        return MapToDto(task);
    }

    public async Task<FiveW2HTaskDto> UpdateTaskAsync(UpdateFiveW2HTaskDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var exists = await _repository.ExistsAsync(dto.Id);
        if (!exists)
            throw new InvalidOperationException($"Task with ID {dto.Id} not found");

        ValidateUpdateDto(dto);

        var task = new FiveW2HTask
        {
            Id = dto.Id,
            What = dto.What,
            Why = dto.Why,
            Where = dto.Where,
            Company = dto.Company,
            When = dto.When,
            Who = dto.Who,
            How = dto.How,
            HowMuch = dto.HowMuch,
            Status = dto.Status,
            Priority = dto.Priority,
            Notes = dto.Notes,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _repository.UpdateAsync(task);
        if (!result)
            throw new InvalidOperationException("Failed to update task");

        return MapToDto(task);
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var tasks = (await _repository.GetAllAsync()).ToList();

        var summary = new DashboardSummaryDto
        {
            TotalTasks = tasks.Count,
            CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Completed),
            InProgressTasks = tasks.Count(t => t.Status == TaskStatus.InProgress),
            PendingTasks = tasks.Count(t => t.Status == TaskStatus.Pending),
            TotalCost = tasks.Sum(t => t.HowMuch),
            AverageCost = tasks.Count > 0 ? tasks.Average(t => t.HowMuch) : 0,
            TasksByResponsible = tasks
                .GroupBy(t => t.Who)
                .ToDictionary(g => g.Key, g => g.Count()),
            TasksByStatus = new Dictionary<string, int>
            {
                { "Pending", tasks.Count(t => t.Status == TaskStatus.Pending) },
                { "In Progress", tasks.Count(t => t.Status == TaskStatus.InProgress) },
                { "Completed", tasks.Count(t => t.Status == TaskStatus.Completed) },
                { "On Hold", tasks.Count(t => t.Status == TaskStatus.OnHold) },
                { "Cancelled", tasks.Count(t => t.Status == TaskStatus.Cancelled) }
            },
            CostByPriority = tasks
                .GroupBy(t => t.Priority.ToString())
                .ToDictionary(g => g.Key, g => g.Sum(t => t.HowMuch))
        };

        return summary;
    }

    private static void ValidateTaskDto(CreateFiveW2HTaskDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.What))
            throw new InvalidOperationException("What field is required");
        if (string.IsNullOrWhiteSpace(dto.Why))
            throw new InvalidOperationException("Why field is required");
        if (string.IsNullOrWhiteSpace(dto.Who))
            throw new InvalidOperationException("Who field is required");
        if (string.IsNullOrWhiteSpace(dto.How))
            throw new InvalidOperationException("How field is required");
        if (dto.HowMuch < 0)
            throw new InvalidOperationException("HowMuch cannot be negative");
        if (dto.When == default)
            throw new InvalidOperationException("When field is required");
    }

    private static void ValidateUpdateDto(UpdateFiveW2HTaskDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.What))
            throw new InvalidOperationException("What field is required");
        if (string.IsNullOrWhiteSpace(dto.Why))
            throw new InvalidOperationException("Why field is required");
        if (string.IsNullOrWhiteSpace(dto.Who))
            throw new InvalidOperationException("Who field is required");
        if (string.IsNullOrWhiteSpace(dto.How))
            throw new InvalidOperationException("How field is required");
        if (dto.HowMuch < 0)
            throw new InvalidOperationException("HowMuch cannot be negative");
        if (dto.When == default)
            throw new InvalidOperationException("When field is required");
    }

    private static FiveW2HTaskDto MapToDto(FiveW2HTask task)
    {
        return new FiveW2HTaskDto
        {
            Id = task.Id,
            What = task.What,
            Why = task.Why,
            Where = task.Where,
            Company = task.Company,
            When = task.When,
            Who = task.Who,
            How = task.How,
            HowMuch = task.HowMuch,
            Status = task.Status,
            Priority = task.Priority,
            Notes = task.Notes,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}
