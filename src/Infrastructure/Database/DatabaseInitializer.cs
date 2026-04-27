using Dapper;
using System.Data.SQLite;

namespace Infrastructure.Database;

/// <summary>
/// SQLite database initializer for creating schema and tables.
/// </summary>
public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>Initializes the database schema.</summary>
    public async Task InitializeAsync()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            const string createTableSql = @"
                CREATE TABLE IF NOT EXISTS FiveW2HTasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    What TEXT NOT NULL,
                    Why TEXT NOT NULL,
                    [Where] TEXT,
                    [When] DATETIME NOT NULL,
                    Who TEXT NOT NULL,
                    How TEXT NOT NULL,
                    HowMuch DECIMAL(10, 2) NOT NULL,
                    Status INTEGER NOT NULL DEFAULT 0,
                    Priority INTEGER NOT NULL DEFAULT 2,
                    Notes TEXT,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                );

                CREATE INDEX IF NOT EXISTS IDX_FiveW2HTasks_Status 
                    ON FiveW2HTasks(Status);
                
                CREATE INDEX IF NOT EXISTS IDX_FiveW2HTasks_Who 
                    ON FiveW2HTasks(Who);
                
                CREATE INDEX IF NOT EXISTS IDX_FiveW2HTasks_When 
                    ON FiveW2HTasks([When]);
                
                CREATE INDEX IF NOT EXISTS IDX_FiveW2HTasks_Priority 
                    ON FiveW2HTasks(Priority);
            ";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = createTableSql;
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    /// <summary>Seeds initial sample data if table is empty.</summary>
    public async Task SeedSampleDataAsync()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            await connection.OpenAsync();

            var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM FiveW2HTasks");
            if (count > 0)
                return;

            const string insertSql = @"
                INSERT INTO FiveW2HTasks (What, Why, [Where], [When], Who, How, HowMuch, Status, Priority, Notes)
                VALUES (@What, @Why, @Where, @When, @Who, @How, @HowMuch, @Status, @Priority, @Notes)
            ";

            var sampleTasks = new[]
            {
                new { What = "Design Database", Why = "Prepare for project", Where = "Office", When = DateTime.UtcNow.AddDays(7), Who = "Alice", How = "Using design tools", HowMuch = 5000m, Status = 1, Priority = 3, Notes = "Start next week" },
                new { What = "Development Setup", Why = "Initialize project", Where = "Development", When = DateTime.UtcNow.AddDays(3), Who = "Bob", How = "Install dependencies", HowMuch = 2000m, Status = 0, Priority = 3, Notes = "Critical path item" },
                new { What = "Testing Plan", Why = "Quality assurance", Where = "QA Lab", When = DateTime.UtcNow.AddDays(14), Who = "Charlie", How = "Create test matrix", HowMuch = 3000m, Status = 0, Priority = 2, Notes = "Due mid-month" }
            };

            foreach (var task in sampleTasks)
            {
                await connection.ExecuteAsync(insertSql, task);
            }
        }
    }
}
