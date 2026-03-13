using Ouroboros.Abstractions.Monads;
using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class MaintenanceSchedulerTests
{
    private readonly MaintenanceScheduler _scheduler = new();

    private static MaintenanceTask CreateTestTask(
        string name = "TestTask",
        MaintenanceTaskType type = MaintenanceTaskType.Custom,
        bool isEnabled = true,
        TimeSpan? schedule = null,
        Func<CancellationToken, Task<Result<object>>>? execute = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Description = $"Test task: {name}",
        TaskType = type,
        Schedule = schedule ?? TimeSpan.FromHours(1),
        IsEnabled = isEnabled,
        Execute = execute ?? (_ => Task.FromResult(Result<object>.Success("done")))
    };

    #region ScheduleTask

    [Fact]
    public void ScheduleTask_ValidTask_ReturnsSuccess()
    {
        var task = CreateTestTask();

        var result = _scheduler.ScheduleTask(task);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(task);
    }

    [Fact]
    public void ScheduleTask_NullTask_ThrowsArgumentNullException()
    {
        var act = () => _scheduler.ScheduleTask(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ExecuteTaskAsync

    [Fact]
    public async Task ExecuteTaskAsync_SuccessfulTask_ReturnsCompletedExecution()
    {
        var task = CreateTestTask(execute: _ => Task.FromResult(Result<object>.Success("ok")));

        var result = await _scheduler.ExecuteTaskAsync(task);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(MaintenanceStatus.Completed);
        result.Value.ResultMessage.Should().Be("Success");
        result.Value.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteTaskAsync_FailedTask_ReturnsFailedExecution()
    {
        var task = CreateTestTask(execute: _ => Task.FromResult(Result<object>.Failure("something went wrong")));

        var result = await _scheduler.ExecuteTaskAsync(task);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(MaintenanceStatus.Failed);
        result.Value.ResultMessage.Should().Be("something went wrong");
    }

    [Fact]
    public async Task ExecuteTaskAsync_ThrowingTask_ReturnsFailedExecution()
    {
        var task = CreateTestTask(execute: _ => throw new InvalidOperationException("boom"));

        var result = await _scheduler.ExecuteTaskAsync(task);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(MaintenanceStatus.Failed);
        result.Value.ResultMessage.Should().Be("boom");
    }

    [Fact]
    public async Task ExecuteTaskAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var task = CreateTestTask(execute: ct =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(Result<object>.Success("ok"));
        });

        var act = () => _scheduler.ExecuteTaskAsync(task, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ExecuteTaskAsync_NullTask_ThrowsArgumentNullException()
    {
        var act = () => _scheduler.ExecuteTaskAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteTaskAsync_RecordsInHistory()
    {
        var task = CreateTestTask();
        await _scheduler.ExecuteTaskAsync(task);

        var history = _scheduler.GetHistory();

        history.Should().ContainSingle();
        history[0].Task.Id.Should().Be(task.Id);
    }

    #endregion

    #region GetHistory

    [Fact]
    public void GetHistory_NoExecutions_ReturnsEmpty()
    {
        var history = _scheduler.GetHistory();

        history.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistory_RespectsLimit()
    {
        for (int i = 0; i < 5; i++)
        {
            await _scheduler.ExecuteTaskAsync(CreateTestTask($"Task{i}"));
        }

        var history = _scheduler.GetHistory(limit: 3);

        history.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetHistory_OrdersByStartedAtDescending()
    {
        var task1 = CreateTestTask("First");
        var task2 = CreateTestTask("Second");
        await _scheduler.ExecuteTaskAsync(task1);
        await _scheduler.ExecuteTaskAsync(task2);

        var history = _scheduler.GetHistory();

        history[0].Task.Name.Should().Be("Second");
    }

    #endregion

    #region Start and Stop

    [Fact]
    public void Start_WhenNotRunning_ReturnsSuccess()
    {
        var result = _scheduler.Start();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Start_WhenAlreadyRunning_ReturnsFailure()
    {
        _scheduler.Start();

        var result = _scheduler.Start();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already running");
    }

    [Fact]
    public async Task StopAsync_WhenRunning_ReturnsSuccess()
    {
        _scheduler.Start();

        var result = await _scheduler.StopAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_WhenNotRunning_ReturnsFailure()
    {
        var result = await _scheduler.StopAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not running");
    }

    [Fact]
    public async Task StartStop_CanRestartAfterStop()
    {
        _scheduler.Start();
        await _scheduler.StopAsync();

        var result = _scheduler.Start();

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Alerts

    [Fact]
    public void CreateAlert_ValidAlert_ReturnsSuccess()
    {
        var alert = new AnomalyAlert
        {
            MetricName = "cpu",
            Description = "CPU spike"
        };

        var result = _scheduler.CreateAlert(alert);

        result.IsSuccess.Should().BeTrue();
        result.Value.MetricName.Should().Be("cpu");
    }

    [Fact]
    public void CreateAlert_NullAlert_ThrowsArgumentNullException()
    {
        var act = () => _scheduler.CreateAlert(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetAlerts_UnresolvedOnly_FiltersResolved()
    {
        var unresolved = new AnomalyAlert { MetricName = "cpu", Description = "spike", IsResolved = false };
        var resolved = new AnomalyAlert { MetricName = "mem", Description = "leak", IsResolved = true };
        _scheduler.CreateAlert(unresolved);
        _scheduler.CreateAlert(resolved);

        var alerts = _scheduler.GetAlerts(unresolvedOnly: true);

        alerts.Should().ContainSingle(a => a.MetricName == "cpu");
    }

    [Fact]
    public void GetAlerts_AllAlerts_IncludesResolved()
    {
        var unresolved = new AnomalyAlert { MetricName = "cpu", Description = "spike", IsResolved = false };
        var resolved = new AnomalyAlert { MetricName = "mem", Description = "leak", IsResolved = true };
        _scheduler.CreateAlert(unresolved);
        _scheduler.CreateAlert(resolved);

        var alerts = _scheduler.GetAlerts(unresolvedOnly: false);

        alerts.Should().HaveCount(2);
    }

    [Fact]
    public void ResolveAlert_ExistingAlert_ReturnsSuccess()
    {
        var alert = new AnomalyAlert { MetricName = "cpu", Description = "spike" };
        _scheduler.CreateAlert(alert);

        var result = _scheduler.ResolveAlert(alert.Id, "Fixed by scaling");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsResolved.Should().BeTrue();
        result.Value.Resolution.Should().Be("Fixed by scaling");
        result.Value.ResolvedAt.Should().NotBeNull();
    }

    [Fact]
    public void ResolveAlert_NonExistentAlert_ReturnsFailure()
    {
        var result = _scheduler.ResolveAlert(Guid.NewGuid(), "Fix");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    #endregion

    #region Static Factory Methods

    [Fact]
    public void CreateCompactionTask_ReturnsCorrectType()
    {
        var task = MaintenanceScheduler.CreateCompactionTask(
            "Compact",
            TimeSpan.FromHours(6),
            _ => Task.FromResult(Result<CompactionResult>.Success(new CompactionResult
            {
                SnapshotsCompacted = 10,
                BytesSaved = 1024
            })));

        task.Name.Should().Be("Compact");
        task.TaskType.Should().Be(MaintenanceTaskType.Compaction);
        task.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCompactionTask_ExecuteCallsCompactor()
    {
        bool called = false;
        var task = MaintenanceScheduler.CreateCompactionTask(
            "Compact",
            TimeSpan.FromHours(6),
            _ =>
            {
                called = true;
                return Task.FromResult(Result<CompactionResult>.Success(new CompactionResult()));
            });

        var result = await task.Execute(CancellationToken.None);

        called.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CreateArchivingTask_ReturnsCorrectType()
    {
        var task = MaintenanceScheduler.CreateArchivingTask(
            "Archive",
            TimeSpan.FromDays(1),
            TimeSpan.FromDays(30),
            (_, _) => Task.FromResult(Result<ArchiveResult>.Success(new ArchiveResult
            {
                ArchiveLocation = "/archive"
            })));

        task.Name.Should().Be("Archive");
        task.TaskType.Should().Be(MaintenanceTaskType.Archiving);
        task.Description.Should().Contain("30");
    }

    [Fact]
    public void CreateAnomalyDetectionTask_ReturnsCorrectType()
    {
        var task = MaintenanceScheduler.CreateAnomalyDetectionTask(
            "Detect",
            TimeSpan.FromMinutes(5),
            _ => Task.FromResult(Result<AnomalyDetectionResult>.Success(new AnomalyDetectionResult())));

        task.Name.Should().Be("Detect");
        task.TaskType.Should().Be(MaintenanceTaskType.AnomalyDetection);
    }

    #endregion
}
