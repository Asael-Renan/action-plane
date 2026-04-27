using Domain.Entities;

namespace Domain.Interfaces;

/// <summary>
/// Interface for importing task data from various formats.
/// </summary>
public interface IDataImportService
{
    /// <summary>Imports tasks from CSV content.</summary>
    Task<IEnumerable<FiveW2HTask>> ImportFromCsvAsync(string csvContent);

    /// <summary>Imports tasks from JSON content.</summary>
    Task<IEnumerable<FiveW2HTask>> ImportFromJsonAsync(string jsonContent);

    /// <summary>Imports tasks from a file.</summary>
    Task<IEnumerable<FiveW2HTask>> ImportFromFileAsync(string filePath);
}
