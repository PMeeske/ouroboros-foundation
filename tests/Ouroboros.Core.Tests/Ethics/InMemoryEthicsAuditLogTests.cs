using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class InMemoryEthicsAuditLogTests
{
    private readonly InMemoryEthicsAuditLog _sut = new();

    private static EthicsAuditEntry CreateEntry(
        string agentId = "agent-1",
        string? userId = "user-1",
        DateTime? timestamp = null)
    {
        return new EthicsAuditEntry
        {
            Timestamp = timestamp ?? DateTime.UtcNow,
            AgentId = agentId,
            UserId = userId,
            EvaluationType = "Action",
            Description = "Test evaluation",
            Clearance = EthicalClearance.Permitted("Safe action")
        };
    }

    private static EthicalViolation CreateViolation(
        ViolationSeverity severity = ViolationSeverity.High)
    {
        return new EthicalViolation
        {
            ViolatedPrinciple = EthicalPrinciple.DoNoHarm,
            Description = "Test violation",
            Severity = severity,
            Evidence = "Test evidence",
            AffectedParties = new List<string> { "user" }
        };
    }

    // --- LogEvaluationAsync ---

    [Fact]
    public async Task LogEvaluationAsync_AddsEntry()
    {
        var entry = CreateEntry();

        await _sut.LogEvaluationAsync(entry);

        var entries = _sut.GetAllEntries();
        entries.Should().HaveCount(1);
        entries[0].Should().Be(entry);
    }

    [Fact]
    public async Task LogEvaluationAsync_NullEntry_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _sut.LogEvaluationAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task LogEvaluationAsync_MultipleEntries_AllStored()
    {
        var entry1 = CreateEntry(agentId: "agent-1");
        var entry2 = CreateEntry(agentId: "agent-2");
        var entry3 = CreateEntry(agentId: "agent-3");

        await _sut.LogEvaluationAsync(entry1);
        await _sut.LogEvaluationAsync(entry2);
        await _sut.LogEvaluationAsync(entry3);

        _sut.GetAllEntries().Should().HaveCount(3);
    }

    // --- LogViolationAttemptAsync ---

    [Fact]
    public async Task LogViolationAttemptAsync_CreatesEntryWithViolationAttemptType()
    {
        var violations = new List<EthicalViolation> { CreateViolation() };

        await _sut.LogViolationAttemptAsync("agent-1", "user-1", "Harmful action", violations);

        var entries = _sut.GetAllEntries();
        entries.Should().HaveCount(1);
        entries[0].EvaluationType.Should().Be("ViolationAttempt");
    }

    [Fact]
    public async Task LogViolationAttemptAsync_SetsAgentId()
    {
        var violations = new List<EthicalViolation> { CreateViolation() };

        await _sut.LogViolationAttemptAsync("test-agent", null, "Test", violations);

        var entries = _sut.GetAllEntries();
        entries[0].AgentId.Should().Be("test-agent");
    }

    [Fact]
    public async Task LogViolationAttemptAsync_SetsDescription()
    {
        var violations = new List<EthicalViolation> { CreateViolation() };

        await _sut.LogViolationAttemptAsync("agent-1", null, "Attempted harm", violations);

        var entries = _sut.GetAllEntries();
        entries[0].Description.Should().Contain("Attempted harm");
    }

    [Fact]
    public async Task LogViolationAttemptAsync_SetsDeniedClearance()
    {
        var violations = new List<EthicalViolation> { CreateViolation() };

        await _sut.LogViolationAttemptAsync("agent-1", null, "Bad action", violations);

        var entries = _sut.GetAllEntries();
        entries[0].Clearance.IsPermitted.Should().BeFalse();
        entries[0].Clearance.Level.Should().Be(EthicalClearanceLevel.Denied);
    }

    [Fact]
    public async Task LogViolationAttemptAsync_IncludesViolationCountInContext()
    {
        var violations = new List<EthicalViolation>
        {
            CreateViolation(),
            CreateViolation(ViolationSeverity.Critical)
        };

        await _sut.LogViolationAttemptAsync("agent-1", null, "Test", violations);

        var entries = _sut.GetAllEntries();
        entries[0].Context.Should().ContainKey("ViolationCount");
        entries[0].Context["ViolationCount"].Should().Be(2);
    }

    [Fact]
    public async Task LogViolationAttemptAsync_IncludesSeverityInContext()
    {
        var violations = new List<EthicalViolation>
        {
            CreateViolation(ViolationSeverity.Low),
            CreateViolation(ViolationSeverity.Critical)
        };

        await _sut.LogViolationAttemptAsync("agent-1", null, "Test", violations);

        var entries = _sut.GetAllEntries();
        entries[0].Context.Should().ContainKey("Severity");
        entries[0].Context["Severity"].Should().Be(ViolationSeverity.Critical.ToString());
    }

    [Fact]
    public async Task LogViolationAttemptAsync_NullAgentId_ThrowsArgumentNullException()
    {
        var violations = new List<EthicalViolation> { CreateViolation() };

        Func<Task> act = () => _sut.LogViolationAttemptAsync(null!, "user", "desc", violations);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task LogViolationAttemptAsync_NullDescription_ThrowsArgumentNullException()
    {
        var violations = new List<EthicalViolation> { CreateViolation() };

        Func<Task> act = () => _sut.LogViolationAttemptAsync("agent", "user", null!, violations);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task LogViolationAttemptAsync_NullViolations_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _sut.LogViolationAttemptAsync("agent", "user", "desc", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task LogViolationAttemptAsync_NullUserId_DoesNotThrow()
    {
        var violations = new List<EthicalViolation> { CreateViolation() };

        Func<Task> act = () => _sut.LogViolationAttemptAsync("agent", null, "desc", violations);

        await act.Should().NotThrowAsync();
    }

    // --- GetAuditHistoryAsync ---

    [Fact]
    public async Task GetAuditHistoryAsync_FiltersByAgentId()
    {
        await _sut.LogEvaluationAsync(CreateEntry(agentId: "agent-1"));
        await _sut.LogEvaluationAsync(CreateEntry(agentId: "agent-2"));
        await _sut.LogEvaluationAsync(CreateEntry(agentId: "agent-1"));

        var history = await _sut.GetAuditHistoryAsync("agent-1");

        history.Should().HaveCount(2);
        history.Should().AllSatisfy(e => e.AgentId.Should().Be("agent-1"));
    }

    [Fact]
    public async Task GetAuditHistoryAsync_NoMatchingEntries_ReturnsEmpty()
    {
        await _sut.LogEvaluationAsync(CreateEntry(agentId: "agent-1"));

        var history = await _sut.GetAuditHistoryAsync("nonexistent-agent");

        history.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAuditHistoryAsync_FiltersByStartTime()
    {
        var oldTime = DateTime.UtcNow.AddHours(-2);
        var recentTime = DateTime.UtcNow;

        await _sut.LogEvaluationAsync(CreateEntry(timestamp: oldTime));
        await _sut.LogEvaluationAsync(CreateEntry(timestamp: recentTime));

        var history = await _sut.GetAuditHistoryAsync(
            "agent-1",
            startTime: DateTime.UtcNow.AddHours(-1));

        history.Should().HaveCount(1);
        history[0].Timestamp.Should().Be(recentTime);
    }

    [Fact]
    public async Task GetAuditHistoryAsync_FiltersByEndTime()
    {
        var oldTime = DateTime.UtcNow.AddHours(-2);
        var recentTime = DateTime.UtcNow;

        await _sut.LogEvaluationAsync(CreateEntry(timestamp: oldTime));
        await _sut.LogEvaluationAsync(CreateEntry(timestamp: recentTime));

        var history = await _sut.GetAuditHistoryAsync(
            "agent-1",
            endTime: DateTime.UtcNow.AddHours(-1));

        history.Should().HaveCount(1);
        history[0].Timestamp.Should().Be(oldTime);
    }

    [Fact]
    public async Task GetAuditHistoryAsync_FiltersByTimeRange()
    {
        var earlyTime = DateTime.UtcNow.AddHours(-3);
        var middleTime = DateTime.UtcNow.AddHours(-1);
        var lateTime = DateTime.UtcNow;

        await _sut.LogEvaluationAsync(CreateEntry(timestamp: earlyTime));
        await _sut.LogEvaluationAsync(CreateEntry(timestamp: middleTime));
        await _sut.LogEvaluationAsync(CreateEntry(timestamp: lateTime));

        var history = await _sut.GetAuditHistoryAsync(
            "agent-1",
            startTime: DateTime.UtcNow.AddHours(-2),
            endTime: DateTime.UtcNow.AddMinutes(-1));

        history.Should().HaveCount(1);
        history[0].Timestamp.Should().Be(middleTime);
    }

    [Fact]
    public async Task GetAuditHistoryAsync_OrdersDescendingByTimestamp()
    {
        var time1 = DateTime.UtcNow.AddHours(-3);
        var time2 = DateTime.UtcNow.AddHours(-2);
        var time3 = DateTime.UtcNow.AddHours(-1);

        await _sut.LogEvaluationAsync(CreateEntry(timestamp: time1));
        await _sut.LogEvaluationAsync(CreateEntry(timestamp: time3));
        await _sut.LogEvaluationAsync(CreateEntry(timestamp: time2));

        var history = await _sut.GetAuditHistoryAsync("agent-1");

        history.Should().HaveCount(3);
        history[0].Timestamp.Should().Be(time3);
        history[1].Timestamp.Should().Be(time2);
        history[2].Timestamp.Should().Be(time1);
    }

    [Fact]
    public async Task GetAuditHistoryAsync_NullAgentId_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _sut.GetAuditHistoryAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // --- GetAllEntries ---

    [Fact]
    public void GetAllEntries_EmptyLog_ReturnsEmptyList()
    {
        var entries = _sut.GetAllEntries();

        entries.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllEntries_ReturnsAllEntriesRegardlessOfAgentId()
    {
        await _sut.LogEvaluationAsync(CreateEntry(agentId: "agent-1"));
        await _sut.LogEvaluationAsync(CreateEntry(agentId: "agent-2"));

        var entries = _sut.GetAllEntries();

        entries.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllEntries_OrdersDescendingByTimestamp()
    {
        var time1 = DateTime.UtcNow.AddHours(-2);
        var time2 = DateTime.UtcNow;

        await _sut.LogEvaluationAsync(CreateEntry(timestamp: time1));
        await _sut.LogEvaluationAsync(CreateEntry(timestamp: time2));

        var entries = _sut.GetAllEntries();

        entries[0].Timestamp.Should().Be(time2);
        entries[1].Timestamp.Should().Be(time1);
    }

    // --- Interface compliance ---

    [Fact]
    public void ImplementsIEthicsAuditLog()
    {
        _sut.Should().BeAssignableTo<IEthicsAuditLog>();
    }

    // --- Mixed operations ---

    [Fact]
    public async Task MixedOperations_LogEvaluationAndViolation_BothRetrievable()
    {
        var entry = CreateEntry(agentId: "agent-1");
        var violations = new List<EthicalViolation> { CreateViolation() };

        await _sut.LogEvaluationAsync(entry);
        await _sut.LogViolationAttemptAsync("agent-1", "user-1", "Harmful action", violations);

        var history = await _sut.GetAuditHistoryAsync("agent-1");

        history.Should().HaveCount(2);
        history.Should().Contain(e => e.EvaluationType == "Action");
        history.Should().Contain(e => e.EvaluationType == "ViolationAttempt");
    }
}
