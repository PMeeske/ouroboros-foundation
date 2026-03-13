using Ouroboros.Abstractions.Monads;
using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class MaintenanceExecutionTests
{
    private static MaintenanceTask CreateTask() => new()
    {
        Id = Guid.NewGuid(),
        Name = "TestTask",
        Description = "Test",
        TaskType = MaintenanceTaskType.Custom,
        Schedule = TimeSpan.FromHours(1),
        Execute = _ => Task.FromResult(Result<object>.Success("done"))
    };

    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var task = CreateTask();
        var execution = new MaintenanceExecution { Task = task };

        execution.Task.Should().Be(task);
        execution.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        execution.CompletedAt.Should().BeNull();
        execution.Status.Should().Be(default(MaintenanceStatus));
        execution.ResultMessage.Should().BeNull();
        execution.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var task = CreateTask();
        var started = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var completed = new DateTime(2025, 6, 1, 12, 5, 0, DateTimeKind.Utc);

        var execution = new MaintenanceExecution
        {
            Task = task,
            StartedAt = started,
            CompletedAt = completed,
            Status = MaintenanceStatus.Completed,
            ResultMessage = "Success",
            Metadata = new Dictionary<string, object> { ["items"] = 42 }
        };

        execution.StartedAt.Should().Be(started);
        execution.CompletedAt.Should().Be(completed);
        execution.Status.Should().Be(MaintenanceStatus.Completed);
        execution.ResultMessage.Should().Be("Success");
        execution.Metadata.Should().ContainKey("items");
    }

    [Fact]
    public void With_CanUpdateStatus()
    {
        var execution = new MaintenanceExecution
        {
            Task = CreateTask(),
            Status = MaintenanceStatus.Running
        };

        var completed = execution with
        {
            Status = MaintenanceStatus.Completed,
            CompletedAt = DateTime.UtcNow
        };

        completed.Status.Should().Be(MaintenanceStatus.Completed);
        completed.CompletedAt.Should().NotBeNull();
        execution.Status.Should().Be(MaintenanceStatus.Running);
    }
}
