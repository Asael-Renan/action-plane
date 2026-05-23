using FiveW2H.App.Core.Models;

namespace FiveW2H.App.Application;

/// <summary>
/// Service for import/export and backup operations.
/// </summary>
public interface IBackupService
{
    Task ExportAsync(string filePath, IEnumerable<FiveW2HTaskDto> tasks);
    Task<ImportResultDto> ImportAsync(string filePath);
    Task<string> ExportToJsonAsync(IEnumerable<FiveW2HTask> tasks);
    Task ExportToFileAsync(IEnumerable<FiveW2HTask> tasks, string filePath, string format);
}
