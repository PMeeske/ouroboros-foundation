using FluentAssertions;
using Ouroboros.Domain.Governance;
using Xunit;

namespace Ouroboros.Tests.Domain.Governance;

[Trait("Category", "Unit")]
public class MaintenanceSchedulerTests
{
    private readonly MaintenanceScheduler _sut = new();

    private static MaintenanceTask CreateTask(
        string name = "TestTask",
        bool enabled = true,
        Func<CancellationToken, Task<Result<object>>>? execute = null)
    {
        return new MaintenanceTask
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Description for {name}",
            TaskType = MaintenanceTaskType.Compaction,
            Schedule = TimeSpan.FromHours(1),
            IsEnabled = enabled,
            Execute = execute ?? (_ => Task.FromResult(Result<object>.Success("ok")))
        };
    }

    // ===== ScheduleTask =====

    [Fact]
    public void ScheduleTask_WithValidTask_ShouldSucceed()
    {
        var task = CreateTask();

        var result = _sut.ScheduleTask(task);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(task);
    }

    [Fact]
    public void ScheduleTask_WithNull_ShouldThrow()
    {
        var act = () => _sut.ScheduleTask(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ===== ExecuteTaskAsync =====

    [Fact]
    public async Task ExecuteTaskAsync_SuccessfulTask_ShouldReturnCompleted()
    {
        var task = CreateTask(execute: _ => Task.FromResult(Result<object>.Success("done")));

        var result = await _sut.ExecuteTaskAsync(task);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(MaintenanceStatus.Completed);
        result.Value.ResultMessage.Should().Be("Success");
        result.Value.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteTaskAsync_FailingTask_ShouldReturnFailed()
    {
        var task = CreateTask(execute: _ => Task.FromResult(Result<object>.Failure("error occurred")));

        var result = await _sut.ExecuteTaskAsync(task);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(MaintenanceStatus.Failed);
        result.Value.ResultMessage.Should().Be("error occurred");
    }

    [Fact]
    public async Task ExecuteTaskAsync_ExceptionThrowingTask_ShouldReturnFailed()
    {
        var task = CreateTask(execute: _ => throw new InvalidOperationException("boom"));

        var result = await _sut.ExecuteTaskAsync(task);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(MaintenanceStatus.Failed);
        result.Value.ResultMessage.Should().Be("boom");
    }

    [Fact]
    public async Task ExecuteTaskAsync_WithNull_ShouldThrow()
    {
        var act = async () => await _sut.ExecuteTaskAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ===== GetHistory =====

    [Fact]
    public async Task GetHistory_AfterExecution_ShouldContainEntry()
    {
        var task = CreateTask();
        await _sut.ExecuteTaskAsync(task);

        var history = _sut.GetHistory();

        history.Should().HaveCount(1);
        history[0].Task.Id.Should().Be(task.Id);
    }

    [Fact]
    public void GetHistory_WhenEmpty_ShouldReturnEmptyList()
    {
        var history = _sut.GetHistory();

        history.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistory_WithLimit_ShouldRespectLimit()
    {
        await _sut.ExecuteTaskAsync(CreateTask("T1"));
        await _sut.ExecuteTaskAsync(CreateTask("T2"));
        await _sut.ExecuteTaskAsync(CreateTask("T3"));

        var history = _sut.GetHistory(limit: 2);

        history.Should().HaveCount(2);
    }

    // ===== Alerts =====

    [Fact]
    public void CreateAlert_ShouldAddAlert()
    {
        var alert = new AnomalyAlert
        {
            MetricName = "cpu",
            Description = "High CPU"
        };

        var result = _sut.CreateAlert(alert);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CreateAlert_WithNull_ShouldThrow()
    {
        var act = () => _sut.CreateAlert(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetAlerts_UnresolvedOnly_ShouldFilterResolved()
    {
        _sut.CreateAlert(new AnomalyAlert { MetricName = "cpu", Description = "High CPU" });
        _sut.CreateAlert(new AnomalyAlert { MetricName = "memory", Description = "High Memory", IsResolved = true });

        var unresolved = _sut.GetAlerts(unresolvedOnly: true);

        unresolved.Should().HaveCount(1);
        unresolved[0].MetricName.Should().Be("cpu");
    }

    [Fact]
    public void GetAlerts_AllAlerts_ShouldReturnAll()
    {
        _sut.CreateAlert(new AnomalyAlert { MetricName = "cpu", Description = "A" });
        _sut.CreateAlert(new AnomalyAlert { MetricName = "mem", Description = "B", IsResolved = true });

        var all = _sut.GetAlerts(unresolvedOnly: false);

        all.Should().HaveCount(2);
    }

    [Fact]
    public void ResolveAlert_ExistingAlert_ShouldSucceed()
    {
        var alert = new AnomalyAlert { MetricName = "cpu", Description = "High CPU" };
        _sut.CreateAlert(alert);

        var result = _sut.ResolveAlert(alert.Id, "Fixed by scaling");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsResolved.Should().BeTrue();
        result.Value.Resolution.Should().Be("Fixed by scaling");
        result.Value.ResolvedAt.Should().NotBeNull();
    }

    [Fact]
    public void ResolveAlert_NonExistentId_ShouldFail()
    {
        var result = _sut.ResolveAlert(Guid.NewGuid(), "resolution");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    // ===== Start / Stop =====

    [Fact]
    public void Start_WhenNotRunning_ShouldSucceed()
    {
        var result = _sut.Start();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Start_WhenAlreadyRunning_ShouldFail()
    {
        _sut.Start();

        var result = _sut.Start();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already running");
    }

    [Fact]
    public async Task StopAsync_WhenRunning_ShouldSucceed()
    {
        _sut.Start();

        var result = await _sut.StopAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_WhenNotRunning_ShouldFail()
    {
        var result = await _sut.StopAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not running");
    }

    // ===== Static Factory Methods =====

    [Fact]
    public void CreateCompactionTask_ShouldReturnValidTask()
    {
        var task = MaintenanceScheduler.CreateCompactionTask(
            "Compact",
            TimeSpan.FromHours(6),
            _ => Task.FromResult(Result<CompactionResult>.Success(new CompactionResult())));

        task.Name.Should().Be("Compact");
        task.TaskType.Should().Be(MaintenanceTaskType.Compaction);
        task.Schedule.Should().Be(TimeSpan.FromHours(6));
        task.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void CreateArchivingTask_ShouldReturnValidTask()
    {
        var task = MaintenanceScheduler.CreateArchivingTask(
            "Archive",
            TimeSpan.FromDays(1),
            TimeSpan.FromDays(30),
            (_, _) => Task.FromResult(Result<ArchiveResult>.Success(new ArchiveResult { ArchiveLocation = "/archive" })));

        task.Name.Should().Be("Archive");
        task.TaskType.Should().Be(MaintenanceTaskType.Archiving);
    }

    [Fact]
    public void CreateAnomalyDetectionTask_ShouldReturnValidTask()
    {
        var task = MaintenanceScheduler.CreateAnomalyDetectionTask(
            "Detect",
            TimeSpan.FromMinutes(30),
            _ => Task.FromResult(Result<AnomalyDetectionResult>.Success(new AnomalyDetectionResult())));

        task.Name.Should().Be("Detect");
        task.TaskType.Should().Be(MaintenanceTaskType.AnomalyDetection);
    }

    [Fact]
    public async Task CreateCompactionTask_Execute_ShouldCallCompactor()
    {
        bool called = false;
        var task = MaintenanceScheduler.CreateCompactionTask(
            "Compact",
            TimeSpan.FromHours(1),
            _ => { called = true; return Task.FromResult(Result<CompactionResult>.Success(new CompactionResult())); });

        var result = await task.Execute(CancellationToken.None);

        called.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }
}
