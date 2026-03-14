using Ouroboros.Domain.Reflection;

namespace Ouroboros.Tests.Reflection;

[Trait("Category", "Unit")]
public class BenchmarkTaskTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        Func<Task<bool>> execute = () => Task.FromResult(true);
        var timeout = TimeSpan.FromSeconds(10);

        var task = new BenchmarkTask("Test Task", CognitiveDimension.Reasoning, execute, timeout);

        task.Name.Should().Be("Test Task");
        task.Dimension.Should().Be(CognitiveDimension.Reasoning);
        task.Execute.Should().BeSameAs(execute);
        task.Timeout.Should().Be(timeout);
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_Success_ShouldReturnTrue()
    {
        var task = new BenchmarkTask("Quick", CognitiveDimension.Learning,
            () => Task.FromResult(true), TimeSpan.FromSeconds(5));

        var result = await task.ExecuteWithTimeoutAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_Failure_ShouldReturnFalse()
    {
        var task = new BenchmarkTask("Failing", CognitiveDimension.Planning,
            () => Task.FromResult(false), TimeSpan.FromSeconds(5));

        var result = await task.ExecuteWithTimeoutAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_Timeout_ShouldReturnFalse()
    {
        var task = new BenchmarkTask("Slow", CognitiveDimension.Reasoning,
            async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                return true;
            },
            TimeSpan.FromMilliseconds(50));

        var result = await task.ExecuteWithTimeoutAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_Exception_ShouldReturnFalse()
    {
        var task = new BenchmarkTask("Throwing", CognitiveDimension.Learning,
            () => throw new InvalidOperationException("boom"),
            TimeSpan.FromSeconds(5));

        var result = await task.ExecuteWithTimeoutAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_Cancellation_ShouldReturnFalse()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var task = new BenchmarkTask("Cancelled", CognitiveDimension.Reasoning,
            () => Task.FromResult(true), TimeSpan.FromSeconds(5));

        var result = await task.ExecuteWithTimeoutAsync(cts.Token);

        result.Should().BeFalse();
    }
}
