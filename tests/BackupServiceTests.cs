using Xunit;
using Moq;
using FiveW2H.App.Application;
using FiveW2H.App.Core.Models;
using FiveW2H.App.Data;
using FiveW2H.App.Infrastructure.ImportExport;
using TaskStatus = FiveW2H.App.Core.Models.TaskStatus;

namespace FiveW2H.Tests;

public class BackupServiceTests
{
    private readonly BackupService _service;
    private readonly Mock<ITaskRepository> _mockRepository;

    public BackupServiceTests()
    {
        _mockRepository = new Mock<ITaskRepository>();
        _service = new BackupService(_mockRepository.Object);
    }

    [Fact]
    public async Task ExportToCsvAsync_WithTasks_WritesCsvContent()
    {
        var tasks = new[]
        {
            new FiveW2HTaskDto
            {
                Id = 1,
                What = "Task 1",
                Why = "Reason 1",
                Where = "Location 1",
                When = new DateTime(2024, 1, 1),
                Who = "Person 1",
                How = "Method 1",
                HowMuch = 1000,
                Status = TaskStatus.Pending,
                Priority = Priority.High,
                Notes = "Notes 1"
            }
        };

        var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.csv");

        try
        {
            await _service.ExportCsvAsync(filePath, tasks);

            Assert.True(File.Exists(filePath));
            var content = await File.ReadAllTextAsync(filePath);
            Assert.Contains("Task 1", content);
            Assert.Contains("1000", content);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_WithTasks_ReturnsJsonString()
    {
        var tasks = new[]
        {
            new FiveW2HTask
            {
                Id = 1,
                What = "Task 1",
                Why = "Reason 1",
                When = new DateTime(2024, 1, 1),
                Who = "Person 1",
                How = "Method 1",
                HowMuch = 1000
            }
        };

        var json = await _service.ExportToJsonAsync(tasks);

        Assert.NotEmpty(json);
        Assert.Contains("Task 1", json);
        Assert.Contains("1000", json);
    }
}
