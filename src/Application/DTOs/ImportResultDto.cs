namespace Application.DTOs;

/// <summary>
/// Result summary for CSV import operations.
/// </summary>
public class ImportResultDto
{
    public int ImportedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; } = new();
}
