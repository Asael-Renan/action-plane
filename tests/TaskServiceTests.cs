using Xunit;
using Moq;
using FiveW2H.App.Application;
using FiveW2H.App.Core.Models;
using FiveW2H.App.Data;
using TaskStatus = FiveW2H.App.Core.Models.TaskStatus;

namespace FiveW2H.Tests;

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
    public async Task CreateTaskAsyncWithValidDtoReturnsTaskDto()
    {
        // Arrange
        var createDto = new CreateFiveW2HTaskDto
        {
            What = "Test task",
            Why = "Testing",
            Where = "Office",
            Company = "Acme",
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
        Assert.Equal("Acme", result.Company);
        Assert.Equal(TaskStatus.Pending, result.Status);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<FiveW2HTask>()), Times.Once);
    }

    [Fact]
    public async Task CreateTaskAsyncWithMissingWhatThrowsException()
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
    public async Task CreateTaskAsyncWithNegativeCostThrowsException()
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
    public async Task GetTaskAsyncWithValidIdReturnsTaskDto()
    {
        // Arrange
        var task = new FiveW2HTask
        {
            Id = 1,
            What = "Test task",
            Why = "Testing",
            Company = "Globex",
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
        Assert.Equal("Globex", result.Company);
    }

    [Fact]
    public async Task SearchTasksAsyncWithCompanyFilterPassesCompanyToRepository()
    {
        var tasks = new[]
        {
            new FiveW2HTask
            {
                Id = 7,
                What = "Review contract",
                Why = "Commercial follow-up",
                Company = "Acme",
                Who = "Maria",
                How = "Meeting",
                HowMuch = 0,
                When = DateTime.UtcNow.AddDays(2)
            }
        };

        _mockRepository
            .Setup(r => r.GetFilteredAsync("review", null, null, "Maria", "Acme"))
            .ReturnsAsync(tasks);

        var result = await _service.SearchTasksAsync("review", null, null, "Maria", "Acme");

        var task = Assert.Single(result);
        Assert.Equal("Acme", task.Company);
        _mockRepository.Verify(r => r.GetFilteredAsync("review", null, null, "Maria", "Acme"), Times.Once);
    }

    [Fact]
    public async Task GetTaskAsyncWithInvalidIdReturnsNull()
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
    public async Task DeleteTaskAsyncWithValidIdReturnsTrue()
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
    public async Task GetDashboardSummaryAsyncReturnsSummaryDto()
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
    public void FiveW2HTaskIsValidWithValidDataReturnsTrue()
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
    public void FiveW2HTaskIsValidWithMissingWhatReturnsFalse()
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
    public void FiveW2HTaskIsValidWithNegativeHowMuchReturnsFalse()
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
    public void FiveW2HTaskDefaultStatusIsPending()
    {
        // Arrange & Act
        var task = new FiveW2HTask();

        // Assert
        Assert.Equal(TaskStatus.Pending, task.Status);
    }

    [Fact]
    public void FiveW2HTaskDefaultPriorityIsMedium()
    {
        // Arrange & Act
        var task = new FiveW2HTask();

        // Assert
        Assert.Equal(Priority.Medium, task.Priority);
    }
}
