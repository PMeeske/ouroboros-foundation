using Ouroboros.Abstractions.Monads;
using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class MaintenanceTaskTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var id = Guid.NewGuid();
        Func<CancellationToken, Task<Result<object>>> execute =
            _ => Task.FromResult(Result<object>.Success("done"));

        var task = new MaintenanceTask
        {
            Id = id,
            Name = "Compaction",
            Description = "Compact DAG",
            TaskType = MaintenanceTaskType.Compaction,
            Schedule = TimeSpan.FromHours(6),
            Execute = execute
        };

        task.Id.Should().Be(id);
        task.Name.Should().Be("Compaction");
        task.Description.Should().Be("Compact DAG");
        task.TaskType.Should().Be(MaintenanceTaskType.Compaction);
        task.Schedule.Should().Be(TimeSpan.FromHours(6));
        task.IsEnabled.Should().BeTrue();
        task.Execute.Should().NotBeNull();
    }

    [Fact]
    public void IsEnabled_DefaultsToTrue()
    {
        var task = new MaintenanceTask
        {
            Id = Guid.NewGuid(),
            Name = "T",
            Description = "D",
            TaskType = MaintenanceTaskType.Custom,
            Schedule = TimeSpan.FromHours(1),
            Execute = _ => Task.FromResult(Result<object>.Success("ok"))
        };

        task.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void With_CanDisableTask()
    {
        var task = new MaintenanceTask
        {
            Id = Guid.NewGuid(),
            Name = "T",
            Description = "D",
            TaskType = MaintenanceTaskType.Custom,
            Schedule = TimeSpan.FromHours(1),
            Execute = _ => Task.FromResult(Result<object>.Success("ok"))
        };

        var disabled = task with { IsEnabled = false };

        disabled.IsEnabled.Should().BeFalse();
        task.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_ReturnsExpectedResult()
    {
        var task = new MaintenanceTask
        {
            Id = Guid.NewGuid(),
            Name = "T",
            Description = "D",
            TaskType = MaintenanceTaskType.Custom,
            Schedule = TimeSpan.FromHours(1),
            Execute = _ => Task.FromResult(Result<object>.Success("completed"))
        };

        var result = await task.Execute(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("completed");
    }
}
