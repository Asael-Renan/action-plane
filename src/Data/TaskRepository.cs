using FiveW2H.App.Core.Models;
using Dapper;
using System.Data.SQLite;

namespace FiveW2H.App.Data;

/// <summary>
/// Repository interface for 5W2H task operations.
/// Follows the Repository pattern for data access abstraction.
/// </summary>
public interface ITaskRepository
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
        string? responsible = null,
        string? company = null);

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

/// <summary>
/// Repository implementation for 5W2H tasks using Dapper and SQLite.
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly string _connectionString;

    public TaskRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<FiveW2HTask?> GetByIdAsync(int id)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var task = await connection.QueryFirstOrDefaultAsync<FiveW2HTask>(
                "SELECT * FROM FiveW2HTasks WHERE Id = @Id",
                new { Id = id }
            );
            return task;
        }
    }

    public async Task<IEnumerable<FiveW2HTask>> GetAllAsync()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var tasks = await connection.QueryAsync<FiveW2HTask>(
                "SELECT * FROM FiveW2HTasks ORDER BY [When] DESC"
            );
            return tasks;
        }
    }

    public async Task<IEnumerable<FiveW2HTask>> GetFilteredAsync(
        string? searchText = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? responsible = null,
        string? company = null)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = "SELECT * FROM FiveW2HTasks WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                sql += " AND (What LIKE @SearchText OR Why LIKE @SearchText OR Company LIKE @SearchText OR [Where] LIKE @SearchText OR Who LIKE @SearchText OR How LIKE @SearchText OR Notes LIKE @SearchText)";
                parameters.Add("@SearchText", $"%{searchText}%");
            }

            if (startDate.HasValue)
            {
                sql += " AND [When] >= @StartDate";
                parameters.Add("@StartDate", startDate.Value);
            }

            if (endDate.HasValue)
            {
                sql += " AND [When] <= @EndDate";
                parameters.Add("@EndDate", endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(responsible))
            {
                sql += " AND Who LIKE @Who";
                parameters.Add("@Who", $"%{responsible}%");
            }

            if (!string.IsNullOrWhiteSpace(company))
            {
                sql += " AND Company LIKE @Company";
                parameters.Add("@Company", $"%{company}%");
            }

            sql += " ORDER BY [When] DESC";

            var tasks = await connection.QueryAsync<FiveW2HTask>(sql, parameters);
            return tasks;
        }
    }

    public async Task<int> AddAsync(FiveW2HTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            const string sql = @"
                INSERT INTO FiveW2HTasks (What, Why, [Where], Company, [When], Who, How, HowMuch, Status, Priority, Notes, CreatedAt, UpdatedAt)
                VALUES (@What, @Why, @Where, @Company, @When, @Who, @How, @HowMuch, @Status, @Priority, @Notes, @CreatedAt, @UpdatedAt);
                SELECT last_insert_rowid();
            ";

            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                task.What,
                task.Why,
                task.Where,
                task.Company,
                task.When,
                task.Who,
                task.How,
                task.HowMuch,
                Status = (int)task.Status,
                Priority = (int)task.Priority,
                task.Notes,
                task.CreatedAt,
                task.UpdatedAt
            });

            return id;
        }
    }

    public async Task<bool> UpdateAsync(FiveW2HTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            const string sql = @"
                UPDATE FiveW2HTasks
                SET What = @What, Why = @Why, [Where] = @Where, Company = @Company, [When] = @When, Who = @Who,
                    How = @How, HowMuch = @HowMuch, Status = @Status, Priority = @Priority,
                    Notes = @Notes, UpdatedAt = @UpdatedAt
                WHERE Id = @Id
            ";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                task.Id,
                task.What,
                task.Why,
                task.Where,
                task.Company,
                task.When,
                task.Who,
                task.How,
                task.HowMuch,
                Status = (int)task.Status,
                Priority = (int)task.Priority,
                task.Notes,
                task.UpdatedAt
            });

            return rowsAffected > 0;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM FiveW2HTasks WHERE Id = @Id",
                new { Id = id }
            );

            return rowsAffected > 0;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM FiveW2HTasks WHERE Id = @Id",
                new { Id = id }
            );
            return count > 0;
        }
    }

    public async Task<int> GetCountAsync()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM FiveW2HTasks"
            );
            return count;
        }
    }

    public async Task<decimal> GetTotalCostAsync()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var total = await connection.ExecuteScalarAsync<decimal>(
                "SELECT COALESCE(SUM(HowMuch), 0) FROM FiveW2HTasks"
            );
            return total;
        }
    }
}
