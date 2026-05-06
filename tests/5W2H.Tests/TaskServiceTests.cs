using Xunit;
using Moq;
using _5W2H.App.Core.Models;
using _5W2H.App.Core.Services;
using _5W2H.App.Data;
using TaskStatus = _5W2H.App.Core.Models.TaskStatus;

namespace _5W2H.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _mockRepository;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _mockRepository = new Mock<ITaskRepository>();
        _service = new TaskService(_mockRepository.Object);
    }

    [Fact]
    public async Task CreateTaskAsync_WithValidDto_ReturnsTaskDto()
    {
        // Arrange
        var createDto = new CreateFiveW2HTaskDto
        {
            What = "Test task",
            Why = "Testing",
            Where = "Office",
            When = DateTime.UtcNow.AddDays(1),
            Who = "Test Person",
            How = "By testing",
            HowMuch = 500,
            Notes = "Test notes"
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<FiveW2HTask>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateTaskAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test task", result.What);
        Assert.Equal(TaskStatus.Pending, result.Status);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<FiveW2HTask>()), Times.Once);
    }

    [Fact]
    public async Task CreateTaskAsync_WithMissingWhat_ThrowsException()
    {
        // Arrange
        var createDto = new CreateFiveW2HTaskDto
        {
            What = "",
            Why = "Testing",
            Who = "Test Person",
            How = "By testing",
            HowMuch = 500,
            When = DateTime.UtcNow.AddDays(1)
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateTaskAsync(createDto)
        );
    }

    [Fact]
    public async Task CreateTaskAsync_WithNegativeCost_ThrowsException()
    {
        // Arrange
        var createDto = new CreateFiveW2HTaskDto
        {
            What = "Test task",
            Why = "Testing",
            Who = "Test Person",
            How = "By testing",
            HowMuch = -100,
            When = DateTime.UtcNow.AddDays(1)
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateTaskAsync(createDto)
        );
    }

    [Fact]
    public async Task GetTaskAsync_WithValidId_ReturnsTaskDto()
    {
        // Arrange
        var task = new FiveW2HTask
        {
            Id = 1,
            What = "Test task",
            Why = "Testing",
            Who = "Test Person",
            How = "By testing",
            HowMuch = 500
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(task);

        // Act
        var result = await _service.GetTaskAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test task", result.What);
    }

    [Fact]
    public async Task GetTaskAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((FiveW2HTask?)null);

        // Act
        var result = await _service.GetTaskAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteTaskAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteTaskAsync(1);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ReturnsSummaryDto()
    {
        // Arrange
        var tasks = new[]
        {
            new FiveW2HTask { Id = 1, What = "Task 1", Who = "Alice", Status = TaskStatus.Pending, Priority = Priority.High, HowMuch = 1000 },
            new FiveW2HTask { Id = 2, What = "Task 2", Who = "Bob", Status = TaskStatus.InProgress, Priority = Priority.Medium, HowMuch = 2000 },
            new FiveW2HTask { Id = 3, What = "Task 3", Who = "Alice", Status = TaskStatus.Completed, Priority = Priority.Low, HowMuch = 500 }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(tasks);

        // Act
        var summary = await _service.GetDashboardSummaryAsync();

        // Assert
        Assert.Equal(3, summary.TotalTasks);
        Assert.Equal(1, summary.CompletedTasks);
        Assert.Equal(1, summary.InProgressTasks);
        Assert.Equal(1, summary.PendingTasks);
        Assert.Equal(3500, summary.TotalCost);
    }
}

public class FiveW2HTaskTests
{
    [Fact]
    public void FiveW2HTask_IsValid_WithValidData_ReturnsTrue()
    {
        // Arrange
        var task = new FiveW2HTask
        {
            What = "Complete project",
            Why = "For business",
            Who = "John",
            How = "By coding",
            HowMuch = 1000
        };

        // Act
        var result = task.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FiveW2HTask_IsValid_WithMissingWhat_ReturnsFalse()
    {
        // Arrange
        var task = new FiveW2HTask
        {
            What = "",
            Why = "For business",
            Who = "John",
            How = "By coding",
            HowMuch = 1000
        };

        // Act
        var result = task.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FiveW2HTask_IsValid_WithNegativeHowMuch_ReturnsFalse()
    {
        // Arrange
        var task = new FiveW2HTask
        {
            What = "Complete project",
            Why = "For business",
            Who = "John",
            How = "By coding",
            HowMuch = -100
        };

        // Act
        var result = task.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FiveW2HTask_DefaultStatus_IsPending()
    {
        // Arrange & Act
        var task = new FiveW2HTask();

        // Assert
        Assert.Equal(TaskStatus.Pending, task.Status);
    }

    [Fact]
    public void FiveW2HTask_DefaultPriority_IsMedium()
    {
        // Arrange & Act
        var task = new FiveW2HTask();

        // Assert
        Assert.Equal(Priority.Medium, task.Priority);
    }
}

