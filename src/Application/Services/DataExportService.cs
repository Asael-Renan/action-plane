using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System.Text;
using System.Text.Json;

namespace Application.Services;

/// <summary>
/// Service for exporting task data to various formats.
/// </summary>
public class DataExportService : IDataExportService
{
    public async Task<string> ExportToCsvAsync(IEnumerable<FiveW2HTask> tasks)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Id,What,Why,Where,When,Who,How,HowMuch,Status,Priority,Notes,CreatedAt,UpdatedAt");

        foreach (var task in tasks)
        {
            var escapedWhat = EscapeCsvField(task.What);
            var escapedWhy = EscapeCsvField(task.Why);
            var escapedWhere = EscapeCsvField(task.Where);
            var escapedWho = EscapeCsvField(task.Who);
            var escapedHow = EscapeCsvField(task.How);
            var escapedNotes = EscapeCsvField(task.Notes);

            csv.AppendLine(
                $"{task.Id},\"{escapedWhat}\",\"{escapedWhy}\",\"{escapedWhere}\"," +
                $"{task.When:yyyy-MM-dd HH:mm:ss},\"{escapedWho}\",\"{escapedHow}\"," +
                $"{task.HowMuch},{task.Status},{task.Priority},\"{escapedNotes}\"," +
                $"{task.CreatedAt:yyyy-MM-dd HH:mm:ss},{task.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        return await Task.FromResult(csv.ToString());
    }

    public async Task<string> ExportToJsonAsync(IEnumerable<FiveW2HTask> tasks)
    {
        var dtos = tasks.Select(t => new
        {
            t.Id,
            t.What,
            t.Why,
            t.Where,
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

        var json = JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = true });
        return await Task.FromResult(json);
    }

    public async Task ExportToFileAsync(IEnumerable<FiveW2HTask> tasks, string filePath, string format)
    {
        string content = format.ToLower() switch
        {
            "csv" => await ExportToCsvAsync(tasks),
            "json" => await ExportToJsonAsync(tasks),
            _ => throw new InvalidOperationException($"Unsupported format: {format}")
        };

        await File.WriteAllTextAsync(filePath, content);
    }

    private static string EscapeCsvField(string field)
    {
        return field.Replace("\"", "\"\"");
    }
}
