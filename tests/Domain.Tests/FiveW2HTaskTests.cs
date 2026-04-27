using Xunit;
using Domain.Entities;
using Domain.Enums;

namespace Domain.Tests;

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
