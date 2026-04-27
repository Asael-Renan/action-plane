using Xunit;
using Application.Services;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Tests;

public class DataExportServiceTests
{
    private readonly DataExportService _service;

    public DataExportServiceTests()
    {
        _service = new DataExportService();
    }

    [Fact]
    public async Task ExportToCsvAsync_WithTasks_ReturnsCsvString()
    {
        // Arrange
        var tasks = new[]
        {
            new FiveW2HTask
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

        // Act
        var csv = await _service.ExportToCsvAsync(tasks);

        // Assert
        Assert.NotEmpty(csv);
        Assert.Contains("Id,What,Why,Where,When,Who,How,HowMuch,Status,Priority,Notes,CreatedAt,UpdatedAt", csv);
        Assert.Contains("Task 1", csv);
        Assert.Contains("1000", csv);
    }

    [Fact]
    public async Task ExportToJsonAsync_WithTasks_ReturnsJsonString()
    {
        // Arrange
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

        // Act
        var json = await _service.ExportToJsonAsync(tasks);

        // Assert
        Assert.NotEmpty(json);
        Assert.Contains("Task 1", json);
        Assert.Contains("1000", json);
    }

    [Fact]
    public async Task ExportToCsvAsync_WithEmptyList_ReturnsHeaderOnly()
    {
        // Arrange
        var tasks = Array.Empty<FiveW2HTask>();

        // Act
        var csv = await _service.ExportToCsvAsync(tasks);

        // Assert
        Assert.NotEmpty(csv);
        Assert.Contains("Id,What,Why,Where,When,Who,How,HowMuch,Status,Priority,Notes,CreatedAt,UpdatedAt", csv);
    }
}
