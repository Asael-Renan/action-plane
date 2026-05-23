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
    private const string SpreadsheetDefaultHow = "Importado da planilha";
    private const string SpreadsheetDefaultWho = "Nao informado";

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

    public async Task ExportAsync(string filePath, IEnumerable<FiveW2HTaskDto> tasks)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("A valid export file path is required.", nameof(filePath));

        var extension = Path.GetExtension(filePath);
        if (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            await ActionPlanSpreadsheetSerializer.ExportAsync(filePath, tasks);
            return;
        }

        if (!extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Unsupported export format. Use .csv or .xlsx.");

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

    public async Task<ImportResultDto> ImportAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("A valid import file path is required.", nameof(filePath));

        var extension = Path.GetExtension(filePath);
        return extension.ToLowerInvariant() switch
        {
            ".xlsx" => await ImportSpreadsheetAsync(filePath),
            ".csv" => await ImportCsvCoreAsync(filePath),
            _ => throw new InvalidOperationException("Unsupported import format. Use .csv or .xlsx.")
        };
    }

    private async Task<ImportResultDto> ImportCsvCoreAsync(string filePath)
    {
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

    private async Task<ImportResultDto> ImportSpreadsheetAsync(string filePath)
    {
        var result = new ImportResultDto();
        var rows = await ActionPlanSpreadsheetSerializer.ReadRowsAsync(filePath);
        if (rows.Count == 0)
        {
            result.Errors.Add("The selected spreadsheet does not contain any importable rows.");
            return result;
        }

        var existingTasks = (await _repository.GetAllAsync()).ToList();
        var tasksById = existingTasks.ToDictionary(task => task.Id);
        var tasksByNaturalKey = existingTasks
            .GroupBy(BuildNaturalKey)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        foreach (var row in rows)
        {
            try
            {
                var importedTask = BuildTaskFromSpreadsheetRow(row);
                if (importedTask is null)
                {
                    result.SkippedCount++;
                    continue;
                }

                var existingTask = FindExistingTask(row, importedTask, tasksById, tasksByNaturalKey);
                if (existingTask is null)
                {
                    importedTask.Id = await _repository.AddAsync(importedTask);
                    result.ImportedCount++;
                    tasksById[importedTask.Id] = importedTask;
                    tasksByNaturalKey[BuildNaturalKey(importedTask)] = importedTask;
                    continue;
                }

                var mergedTask = MergeSpreadsheetTask(existingTask, importedTask, row.Values);
                if (TasksAreEquivalent(existingTask, mergedTask))
                {
                    result.SkippedCount++;
                    continue;
                }

                await _repository.UpdateAsync(mergedTask);
                result.UpdatedCount++;
                tasksById[mergedTask.Id] = mergedTask;
                tasksByNaturalKey[BuildNaturalKey(mergedTask)] = mergedTask;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{row.SheetName} linha {row.RowNumber}: {ex.Message}");
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

    private static FiveW2HTask? BuildTaskFromSpreadsheetRow(SpreadsheetImportRow row)
    {
        if (!TryGetRequiredValue(row.Values, "what", out var what) ||
            !TryGetRequiredValue(row.Values, "why", out var why) ||
            !TryGetRequiredValue(row.Values, "when", out var whenRaw))
        {
            return null;
        }

        var when = ParseSpreadsheetDate(whenRaw);
        var createdAt = TryGetValue(row.Values, "createdAt", out var createdAtRaw)
            ? ParseSpreadsheetDate(createdAtRaw)
            : DateTime.UtcNow;
        var updatedAt = TryGetValue(row.Values, "updatedAt", out var updatedAtRaw)
            ? ParseSpreadsheetDate(updatedAtRaw)
            : createdAt;

        return new FiveW2HTask
        {
            Id = TryGetValue(row.Values, "id", out var idRaw) && int.TryParse(idRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
                ? id
                : 0,
            What = what,
            Why = why,
            Where = GetOptionalValue(row.Values, "where"),
            Company = GetOptionalValue(row.Values, "company"),
            When = when,
            Who = GetOptionalValue(row.Values, "who", SpreadsheetDefaultWho),
            How = GetOptionalValue(row.Values, "how", SpreadsheetDefaultHow),
            HowMuch = TryGetValue(row.Values, "howMuch", out var howMuchRaw)
                ? ParseDecimal(howMuchRaw)
                : 0,
            Status = TryGetValue(row.Values, "status", out var statusRaw)
                ? ParseStatus(statusRaw)
                : TaskStatus.Pending,
            Priority = TryGetValue(row.Values, "priority", out var priorityRaw)
                ? ParsePriority(priorityRaw)
                : Priority.Medium,
            Notes = GetOptionalValue(row.Values, "notes"),
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    private static FiveW2HTask? FindExistingTask(
        SpreadsheetImportRow row,
        FiveW2HTask importedTask,
        Dictionary<int, FiveW2HTask> tasksById,
        Dictionary<string, FiveW2HTask> tasksByNaturalKey)
    {
        if (TryGetValue(row.Values, "id", out var idRaw) &&
            int.TryParse(idRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) &&
            id > 0 &&
            tasksById.TryGetValue(id, out var taskById))
        {
            return taskById;
        }

        tasksByNaturalKey.TryGetValue(BuildNaturalKey(importedTask), out var taskByNaturalKey);
        return taskByNaturalKey;
    }

    private static FiveW2HTask MergeSpreadsheetTask(FiveW2HTask existingTask, FiveW2HTask importedTask, IReadOnlyDictionary<string, string> rawValues)
    {
        return new FiveW2HTask
        {
            Id = existingTask.Id,
            What = importedTask.What,
            Why = importedTask.Why,
            Where = TryGetValue(rawValues, "where", out _) ? importedTask.Where : existingTask.Where,
            Company = TryGetValue(rawValues, "company", out _) ? importedTask.Company : existingTask.Company,
            When = importedTask.When,
            Who = TryGetValue(rawValues, "who", out _) ? importedTask.Who : existingTask.Who,
            How = TryGetValue(rawValues, "how", out _) ? importedTask.How : existingTask.How,
            HowMuch = TryGetValue(rawValues, "howMuch", out _) ? importedTask.HowMuch : existingTask.HowMuch,
            Status = importedTask.Status,
            Priority = TryGetValue(rawValues, "priority", out _) ? importedTask.Priority : existingTask.Priority,
            Notes = TryGetValue(rawValues, "notes", out _) ? importedTask.Notes : existingTask.Notes,
            CreatedAt = TryGetValue(rawValues, "createdAt", out _) ? importedTask.CreatedAt : existingTask.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static bool TasksAreEquivalent(FiveW2HTask left, FiveW2HTask right)
    {
        return left.What == right.What &&
               left.Why == right.Why &&
               left.Where == right.Where &&
               left.Company == right.Company &&
               left.When == right.When &&
               left.Who == right.Who &&
               left.How == right.How &&
               left.HowMuch == right.HowMuch &&
               left.Status == right.Status &&
               left.Priority == right.Priority &&
               left.Notes == right.Notes &&
               left.CreatedAt == right.CreatedAt;
    }

    private static string BuildNaturalKey(FiveW2HTask task)
    {
        return string.Join("|",
            NormalizeForKey(task.What),
            NormalizeForKey(task.Why),
            NormalizeForKey(task.Company),
            task.When.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
    }

    private static string NormalizeForKey(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static bool TryGetRequiredValue(IReadOnlyDictionary<string, string> values, string key, out string value)
    {
        var found = TryGetValue(values, key, out value);
        value = value.Trim();
        return found && !string.IsNullOrWhiteSpace(value);
    }

    private static bool TryGetValue(IReadOnlyDictionary<string, string> values, string key, out string value)
    {
        if (values.TryGetValue(key, out value!))
        {
            value = value.Trim();
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static string GetOptionalValue(IReadOnlyDictionary<string, string> values, string key, string fallback = "")
    {
        return TryGetValue(values, key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }

    private static DateTime ParseSpreadsheetDate(string rawValue)
    {
        if (double.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var oaDate))
        {
            return DateTime.FromOADate(oaDate);
        }

        if (DateTime.TryParse(rawValue, CultureInfo.GetCultureInfo("pt-BR"), DateTimeStyles.AssumeLocal, out var ptBrDate))
        {
            return ptBrDate;
        }

        if (DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var invariantDate))
        {
            return invariantDate;
        }

        throw new FormatException($"Invalid date value '{rawValue}'.");
    }

    private static decimal ParseDecimal(string rawValue)
    {
        if (decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out var ptBrValue))
        {
            return ptBrValue;
        }

        if (decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantValue))
        {
            return invariantValue;
        }

        throw new FormatException($"Invalid decimal value '{rawValue}'.");
    }

    private static TaskStatus ParseStatus(string rawValue)
    {
        var normalized = rawValue.Trim().ToUpperInvariant();
        return normalized switch
        {
            "CONCLUIDO" or "CONCLUÍDO" or "COMPLETED" => TaskStatus.Completed,
            "EM ANDAMENTO" or "IN PROGRESS" or "INPROGRESS" => TaskStatus.InProgress,
            "EM ESPERA" or "ON HOLD" or "ONHOLD" => TaskStatus.OnHold,
            "CANCELADO" or "CANCELLED" => TaskStatus.Cancelled,
            "PENDENTE" or "PENDING" => TaskStatus.Pending,
            _ when Enum.TryParse<TaskStatus>(rawValue, true, out var parsedStatus) => parsedStatus,
            _ => TaskStatus.Pending
        };
    }

    private static Priority ParsePriority(string rawValue)
    {
        var normalized = rawValue.Trim().ToUpperInvariant();
        return normalized switch
        {
            "CRITICA" or "CRÍTICA" or "CRITICAL" => Priority.Critical,
            "ALTA" or "HIGH" => Priority.High,
            "BAIXA" or "LOW" => Priority.Low,
            "MEDIA" or "MÉDIA" or "MEDIUM" => Priority.Medium,
            _ when Enum.TryParse<Priority>(rawValue, true, out var parsedPriority) => parsedPriority,
            _ => Priority.Medium
        };
    }
}
