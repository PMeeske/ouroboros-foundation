// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Reflection;

using Ouroboros.Domain.Reflection;

/// <summary>
/// Tests for <see cref="BenchmarkTask"/>.
/// </summary>
[Trait("Category", "Unit")]
public class BenchmarkTaskTests
{
    // ----------------------------------------------------------------
    // Record properties
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        Func<Task<bool>> execute = () => Task.FromResult(true);
        var timeout = TimeSpan.FromSeconds(5);

        // Act
        var task = new BenchmarkTask("TestTask", CognitiveDimension.Reasoning, execute, timeout);

        // Assert
        task.Name.Should().Be("TestTask");
        task.Dimension.Should().Be(CognitiveDimension.Reasoning);
        task.Execute.Should().BeSameAs(execute);
        task.Timeout.Should().Be(timeout);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        Func<Task<bool>> execute = () => Task.FromResult(true);
        var timeout = TimeSpan.FromSeconds(5);

        // Act
        var task1 = new BenchmarkTask("Task", CognitiveDimension.Planning, execute, timeout);
        var task2 = new BenchmarkTask("Task", CognitiveDimension.Planning, execute, timeout);

        // Assert
        task1.Should().Be(task2);
    }

    // ----------------------------------------------------------------
    // ExecuteWithTimeoutAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task ExecuteWithTimeoutAsync_SuccessfulTask_ReturnsTrue()
    {
        // Arrange
        var task = new BenchmarkTask(
            "Success",
            CognitiveDimension.Reasoning,
            () => Task.FromResult(true),
            TimeSpan.FromSeconds(5));

        // Act
        bool result = await task.ExecuteWithTimeoutAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_FailedTask_ReturnsFalse()
    {
        // Arrange
        var task = new BenchmarkTask(
            "Failure",
            CognitiveDimension.Reasoning,
            () => Task.FromResult(false),
            TimeSpan.FromSeconds(5));

        // Act
        bool result = await task.ExecuteWithTimeoutAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_TimesOut_ReturnsFalse()
    {
        // Arrange
        var task = new BenchmarkTask(
            "Slow",
            CognitiveDimension.Planning,
            async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(30));
                return true;
            },
            TimeSpan.FromMilliseconds(50));

        // Act
        bool result = await task.ExecuteWithTimeoutAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_ThrowsException_ReturnsFalse()
    {
        // Arrange
        var task = new BenchmarkTask(
            "Error",
            CognitiveDimension.Learning,
            () => throw new InvalidOperationException("Test error"),
            TimeSpan.FromSeconds(5));

        // Act
        bool result = await task.ExecuteWithTimeoutAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_CancelledToken_ReturnsFalse()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var task = new BenchmarkTask(
            "Cancelled",
            CognitiveDimension.Reasoning,
            () => Task.FromResult(true),
            TimeSpan.FromSeconds(5));

        // Act
        bool result = await task.ExecuteWithTimeoutAsync(cts.Token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_DifferentDimensions_AllWork()
    {
        // Arrange & Act & Assert
        foreach (CognitiveDimension dimension in Enum.GetValues<CognitiveDimension>())
        {
            var task = new BenchmarkTask(
                $"Test-{dimension}",
                dimension,
                () => Task.FromResult(true),
                TimeSpan.FromSeconds(1));

            bool result = await task.ExecuteWithTimeoutAsync();
            result.Should().BeTrue();
        }
    }
}
