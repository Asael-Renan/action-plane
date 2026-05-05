using System.Globalization;
using System.Text;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using DomainTaskStatus = Domain.Enums.TaskStatus;

namespace Application.Services;

/// <summary>
/// UTF-8 CSV import/export service for 5W2H tasks.
/// </summary>
public class CsvTaskDataTransferService : IDataImportExportService
{
    private static readonly string[] ExpectedHeaders =
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

    private readonly IFiveW2HRepository _repository;

    public CsvTaskDataTransferService(IFiveW2HRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

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
                if (values.Count != ExpectedHeaders.Length)
                {
                    result.Errors.Add($"Line {lineNumber}: expected {ExpectedHeaders.Length} columns but found {values.Count}.");
                    continue;
                }

                var task = BuildTask(values, lineNumber);

                if (task.Id > 0 && await _repository.ExistsAsync(task.Id))
                {
                    task.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(task);
                    result.UpdatedCount++;
                    continue;
                }

                if (existingTasks.Any(existing => IsDuplicate(existing, task)))
                {
                    result.SkippedCount++;
                    continue;
                }

                task.Id = 0;
                task.CreatedAt = DateTime.UtcNow;
                task.UpdatedAt = DateTime.UtcNow;
                task.Id = await _repository.AddAsync(task);
                existingTasks.Add(task);
                result.ImportedCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Line {lineNumber}: {ex.Message}");
            }
        }

        return result;
    }

    private static FiveW2HTask BuildTask(IReadOnlyList<string> values, int lineNumber)
    {
        var id = string.IsNullOrWhiteSpace(values[0])
            ? 0
            : ParseInt(values[0], "Id", lineNumber);

        var when = ParseDate(values[4], lineNumber);
        var howMuch = ParseDecimal(values[7], lineNumber);
        var status = ParseEnum<DomainTaskStatus>(values[8], "Status", lineNumber);
        var priority = ParseEnum<Priority>(values[9], "Priority", lineNumber);

        var task = new FiveW2HTask
        {
            Id = id,
            What = values[1].Trim(),
            Why = values[2].Trim(),
            Where = values[3].Trim(),
            When = when,
            Who = values[5].Trim(),
            How = values[6].Trim(),
            HowMuch = howMuch,
            Status = status,
            Priority = priority,
            Notes = values[10].Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!task.IsValid())
            throw new InvalidOperationException("Required fields are missing or HowMuch is negative.");

        return task;
    }

    private static int ParseInt(string value, string columnName, int lineNumber)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;

        throw new InvalidOperationException($"{columnName} is not a valid integer on line {lineNumber}.");
    }

    private static DateTime ParseDate(string value, int lineNumber)
    {
        var formats = new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy", "MM/dd/yyyy" };
        if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return result;

        if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
            return result;

        throw new InvalidOperationException($"When is not a valid date on line {lineNumber}.");
    }

    private static decimal ParseDecimal(string value, int lineNumber)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result) ||
            decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result))
        {
            return result;
        }

        throw new InvalidOperationException($"HowMuch is not a valid number on line {lineNumber}.");
    }

    private static TEnum ParseEnum<TEnum>(string value, string columnName, int lineNumber)
        where TEnum : struct
    {
        if (Enum.TryParse<TEnum>(value, true, out var result))
            return result;

        throw new InvalidOperationException($"{columnName} is not valid on line {lineNumber}.");
    }

    private static bool HeadersAreValid(IReadOnlyList<string> headers)
    {
        return headers.Count == ExpectedHeaders.Length &&
               !headers.Where((header, index) => !string.Equals(header, ExpectedHeaders[index], StringComparison.OrdinalIgnoreCase)).Any();
    }

    private static bool IsDuplicate(FiveW2HTask existing, FiveW2HTask imported)
    {
        return string.Equals(existing.What, imported.What, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(existing.Who, imported.Who, StringComparison.OrdinalIgnoreCase) &&
               existing.When.Date == imported.When.Date;
    }

    private static string Escape(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];

            if (character == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (character == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        values.Add(current.ToString());
        return values;
    }
}
