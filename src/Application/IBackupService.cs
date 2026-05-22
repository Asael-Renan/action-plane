using FiveW2H.App.Core.Models;

namespace FiveW2H.App.Application;

/// <summary>
/// Service for import/export and backup operations.
/// </summary>
public interface IBackupService
{
    Task ExportCsvAsync(string filePath, IEnumerable<FiveW2HTaskDto> tasks);
    Task<ImportResultDto> ImportCsvAsync(string filePath);
    Task<string> ExportToJsonAsync(IEnumerable<FiveW2HTask> tasks);
    Task ExportToFileAsync(IEnumerable<FiveW2HTask> tasks, string filePath, string format);
}
