using Application.DTOs;

namespace Application.Interfaces;

/// <summary>
/// Imports and exports 5W2H records using external files.
/// </summary>
public interface IDataImportExportService
{
    Task ExportCsvAsync(string filePath, IEnumerable<FiveW2HTaskDto> tasks);

    Task<ImportResultDto> ImportCsvAsync(string filePath);
}
