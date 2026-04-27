using Domain.Entities;

namespace Domain.Interfaces;

/// <summary>
/// Interface for exporting task data to various formats.
/// </summary>
public interface IDataExportService
{
    /// <summary>Exports tasks to CSV format.</summary>
    Task<string> ExportToCsvAsync(IEnumerable<FiveW2HTask> tasks);

    /// <summary>Exports tasks to JSON format.</summary>
    Task<string> ExportToJsonAsync(IEnumerable<FiveW2HTask> tasks);

    /// <summary>Exports tasks to a file.</summary>
    Task ExportToFileAsync(IEnumerable<FiveW2HTask> tasks, string filePath, string format);
}
