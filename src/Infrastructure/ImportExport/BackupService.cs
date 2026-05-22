using FiveW2H.App.Application;
using FiveW2H.App.Core.Models;
using FiveW2H.App.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using TaskStatus = FiveW2H.App.Core.Models.TaskStatus;

namespace FiveW2H.App.Infrastructure.ImportExport;

/// <summary>
/// Service for managing 5W2H task backups and data transfers.
/// Handles CSV/JSON export and import operations.
/// </summary>
public class BackupService : IBackupService
{
    private static readonly string[] ExpectedHeaders =
    [
        "Id",
        "What",
        "Why",
        "Where",
        "Company",
        "When",
        "Who",
        "How",
        "HowMuch",
        "Status",
        "Priority",
        "Notes"
    ];

    private static readonly string[] LegacyHeaders =
    [
        "Id",
        "What",
        "Why",
        "Where",
        "When",
        "Who",
        "How",
        "HowMuch",
        "Status",
        "Priority",
        "Notes"
    ];

    private readonly ITaskRepository _repository;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public BackupService(ITaskRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>Exports tasks to CSV format.</summary>
    public async Task ExportCsvAsync(string filePath, IEnumerable<FiveW2HTaskDto> tasks)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("A valid export file path is required.", nameof(filePath));

        var csv = new StringBuilder();
        csv.AppendLine(string.Join(",", ExpectedHeaders));

        foreach (var task in tasks)
        {
            csv.AppendLine(string.Join(",",
                task.Id.ToString(CultureInfo.InvariantCulture),
                Escape(task.What),
                Escape(task.Why),
                Escape(task.Where),
                Escape(task.Company),
                Escape(task.When.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                Escape(task.Who),
                Escape(task.How),
                task.HowMuch.ToString(CultureInfo.InvariantCulture),
                Escape(task.Status.ToString()),
                Escape(task.Priority.ToString()),
                Escape(task.Notes)));
        }

        await File.WriteAllTextAsync(filePath, csv.ToString(), new UTF8Encoding(false));
    }

    /// <summary>Imports tasks from CSV file.</summary>
    public async Task<ImportResultDto> ImportCsvAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("A valid import file path is required.", nameof(filePath));

        var result = new ImportResultDto();
        var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);

        if (lines.Length == 0)
        {
            result.Errors.Add("The selected file is empty.");
            return result;
        }

        var headers = ParseCsvLine(lines[0]);
        var hasCompanyColumn = HasCompanyHeader(headers);
        if (!HeadersAreValid(headers))
        {
            result.Errors.Add($"Invalid CSV header. Expected: {string.Join(",", ExpectedHeaders)}");
            return result;
        }

        var existingTasks = (await _repository.GetAllAsync()).ToList();

        for (var index = 1; index < lines.Length; index++)
        {
            var lineNumber = index + 1;
            if (string.IsNullOrWhiteSpace(lines[index]))
                continue;

            try
            {
                var values = ParseCsvLine(lines[index]);
                var expectedColumnCount = hasCompanyColumn ? ExpectedHeaders.Length : LegacyHeaders.Length;
                if (values.Count != expectedColumnCount)
                {
                    result.Errors.Add($"Line {lineNumber}: expected {expectedColumnCount} columns but found {values.Count}.");
                    continue;
                }

                var id = int.Parse(values[0], CultureInfo.InvariantCulture);
                var existingTask = existingTasks.FirstOrDefault(t => t.Id == id);
                var company = hasCompanyColumn ? values[4] : string.Empty;
                var whenIndex = hasCompanyColumn ? 5 : 4;
                var whoIndex = hasCompanyColumn ? 6 : 5;
                var howIndex = hasCompanyColumn ? 7 : 6;
                var howMuchIndex = hasCompanyColumn ? 8 : 7;
                var statusIndex = hasCompanyColumn ? 9 : 8;
                var priorityIndex = hasCompanyColumn ? 10 : 9;
                var notesIndex = hasCompanyColumn ? 11 : 10;

                var task = new FiveW2HTask
                {
                    Id = id,
                    What = values[1],
                    Why = values[2],
                    Where = values[3],
                    Company = company,
                    When = DateTime.Parse(values[whenIndex], CultureInfo.InvariantCulture),
                    Who = values[whoIndex],
                    How = values[howIndex],
                    HowMuch = decimal.Parse(values[howMuchIndex], CultureInfo.InvariantCulture),
                    Status = Enum.Parse<TaskStatus>(values[statusIndex]),
                    Priority = Enum.Parse<Priority>(values[priorityIndex]),
                    Notes = values[notesIndex],
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (!task.IsValid())
                {
                    result.Errors.Add($"Line {lineNumber}: task validation failed.");
                    result.SkippedCount++;
                    continue;
                }

                if (existingTask != null)
                {
                    task.CreatedAt = existingTask.CreatedAt;
                    await _repository.UpdateAsync(task);
                    result.UpdatedCount++;
                }
                else
                {
                    await _repository.AddAsync(task);
                    result.ImportedCount++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Line {lineNumber}: {ex.Message}");
                result.SkippedCount++;
            }
        }

        return result;
    }

    /// <summary>Exports tasks to JSON format.</summary>
    public async Task<string> ExportToJsonAsync(IEnumerable<FiveW2HTask> tasks)
    {
        var dtos = tasks.Select(t => new
        {
            t.Id,
            t.What,
            t.Why,
            t.Where,
            t.Company,
            t.When,
            t.Who,
            t.How,
            t.HowMuch,
            t.Status,
            t.Priority,
            t.Notes,
            t.CreatedAt,
            t.UpdatedAt
        }).ToList();

        var json = JsonSerializer.Serialize(dtos, JsonOptions);
        return await Task.FromResult(json);
    }

    /// <summary>Exports tasks to file in specified format (csv or json).</summary>
    public async Task ExportToFileAsync(IEnumerable<FiveW2HTask> tasks, string filePath, string format)
    {
        string content = format.ToLowerInvariant() switch
        {
            "csv" => await ExportToCsvAsync(tasks),
            "json" => await ExportToJsonAsync(tasks),
            _ => throw new InvalidOperationException($"Unsupported format: {format}")
        };

        await File.WriteAllTextAsync(filePath, content);
    }

    private static async Task<string> ExportToCsvAsync(IEnumerable<FiveW2HTask> tasks)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Id,What,Why,Where,Company,When,Who,How,HowMuch,Status,Priority,Notes,CreatedAt,UpdatedAt");

        foreach (var task in tasks)
        {
            var escapedWhat = EscapeCsvField(task.What);
            var escapedWhy = EscapeCsvField(task.Why);
            var escapedWhere = EscapeCsvField(task.Where);
            var escapedCompany = EscapeCsvField(task.Company);
            var escapedWho = EscapeCsvField(task.Who);
            var escapedHow = EscapeCsvField(task.How);
            var escapedNotes = EscapeCsvField(task.Notes);

            csv.AppendLine(CultureInfo.InvariantCulture,
                $"{task.Id},\"{escapedWhat}\",\"{escapedWhy}\",\"{escapedWhere}\",\"{escapedCompany}\"," +
                $"{task.When:yyyy-MM-dd HH:mm:ss},\"{escapedWho}\",\"{escapedHow}\"," +
                $"{task.HowMuch},{task.Status},{task.Priority},\"{escapedNotes}\"," +
                $"{task.CreatedAt:yyyy-MM-dd HH:mm:ss},{task.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        return await Task.FromResult(csv.ToString());
    }

    private static string Escape(string field)
    {
        return field.Contains(',') ? $"\"{field}\"" : field;
    }

    private static string EscapeCsvField(string field)
    {
        return field.Replace("\"", "\"\"");
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentValue.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.ToString().Trim());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(c);
            }
        }

        values.Add(currentValue.ToString().Trim());
        return values;
    }

    private static bool HeadersAreValid(List<string> headers)
    {
        return headers.SequenceEqual(ExpectedHeaders) || headers.SequenceEqual(LegacyHeaders);
    }

    private static bool HasCompanyHeader(List<string> headers)
    {
        return headers.SequenceEqual(ExpectedHeaders);
    }
}
