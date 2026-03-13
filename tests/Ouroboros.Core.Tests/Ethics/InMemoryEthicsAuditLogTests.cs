using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class InMemoryEthicsAuditLogTests
{
    private readonly InMemoryEthicsAuditLog _sut = new();

    private static EthicsAuditEntry CreateEntry(
        string agentId = "agent-1",
        string evaluationType = "Action",
        string description = "Test action",
        DateTime? timestamp = null)
    {
        return new EthicsAuditEntry
        {
            Timestamp = timestamp ?? DateTime.UtcNow,
            AgentId = agentId,
            UserId = "user-1",
            EvaluationType = evaluationType,
            Description = description,
            Clearance = EthicalClearance.Permitted("Test clearance")
        };
    }

    private static EthicalViolation CreateViolation(string description = "Test violation")
    {
        return new EthicalViolation
        {
            ViolatedPrinciple = EthicalPrinciple.DoNoHarm,
            Description = description,
            Severity = ViolationSeverity.High,
            Evidence = "Test evidence",
            AffectedParties = new[] { "Users" }
        };
    }

    [Fact]
    public async Task LogEvaluationAsync_ValidEntry_AddsToLog()
    {
        var entry = CreateEntry();

        await _sut.LogEvaluationAsync(entry);

        var entries = _sut.GetAllEntries();
        entries.Should().HaveCount(1);
        entries[0].AgentId.Should().Be("agent-1");
    }

    [Fact]
    public async Task LogEvaluationAsync_NullEntry_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.LogEvaluationAsync(null!));
    }

    [Fact]
    public async Task LogEvaluationAsync_MultipleEntries_AllStored()
    {
        await _sut.LogEvaluationAsync(CreateEntry(agentId: "a1"));
        await _sut.LogEvaluationAsync(CreateEntry(agentId: "a2"));
        await _sut.LogEvaluationAsync(CreateEntry(agentId: "a3"));

        _sut.GetAllEntries().Should().HaveCount(3);
    }

    [Fact]
    public async Task LogViolationAttemptAsync_ValidParams_CreatesEntry()
    {
        var violations = new List<EthicalViolation> { CreateViolation() };

        await _sut.LogViolationAttemptAsync("agent-1", "user-1", "Attempted harm", violations);

        var entries = _sut.GetAllEntries();
        entries.Should().HaveCount(1);
        entries[0].EvaluationType.Should().Be("ViolationAttempt");
        entries[0].AgentId.Should().Be("agent-1");
    }

    [Fact]
    public async Task LogViolationAttemptAsync_NullAgentId_ThrowsArgumentNullException()
    {
        var violations = new List<EthicalViolation> { CreateViolation() };

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.LogViolationAttemptAsync(null!, "user-1", "desc", violations));
    }

    [Fact]
    public async Task LogViolationAttemptAsync_NullDescription_ThrowsArgumentNullException()
    {
        var violations = new List<EthicalViolation> { CreateViolation() };

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.LogViolationAttemptAsync("agent", "user", null!, violations));
    }

    [Fact]
    public async Task LogViolationAttemptAsync_NullViolations_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.LogViolationAttemptAsync("agent", "user", "desc", null!));
    }

    [Fact]
    public async Task LogViolationAttemptAsync_NullUserId_Succeeds()
    {
        var violations = new List<EthicalViolation> { CreateViolation() };

        await _sut.LogViolationAttemptAsync("agent-1", null, "test", violations);

        var entries = _sut.GetAllEntries();
        entries.Should().HaveCount(1);
        entries[0].UserId.Should().BeNull();
    }

    [Fact]
    public async Task GetAuditHistoryAsync_FiltersByAgentId()
    {
        await _sut.LogEvaluationAsync(CreateEntry(agentId: "agent-1"));
        await _sut.LogEvaluationAsync(CreateEntry(agentId: "agent-2"));
        await _sut.LogEvaluationAsync(CreateEntry(agentId: "agent-1"));

        var history = await _sut.GetAuditHistoryAsync("agent-1");

        history.Should().HaveCount(2);
        history.Should().OnlyContain(e => e.AgentId == "agent-1");
    }

    [Fact]
    public async Task GetAuditHistoryAsync_FiltersByStartTime()
    {
        var oldTime = DateTime.UtcNow.AddHours(-2);
        var recentTime = DateTime.UtcNow;

        await _sut.LogEvaluationAsync(CreateEntry(timestamp: oldTime));
        await _sut.LogEvaluationAsync(CreateEntry(timestamp: recentTime));

        var cutoff = DateTime.UtcNow.AddHours(-1);
        var history = await _sut.GetAuditHistoryAsync("agent-1", startTime: cutoff);

        history.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAuditHistoryAsync_FiltersByEndTime()
    {
        var oldTime = DateTime.UtcNow.AddHours(-2);
        var recentTime = DateTime.UtcNow;

        await _sut.LogEvaluationAsync(CreateEntry(timestamp: oldTime));
        await _sut.LogEvaluationAsync(CreateEntry(timestamp: recentTime));

        var cutoff = DateTime.UtcNow.AddHours(-1);
        var history = await _sut.GetAuditHistoryAsync("agent-1", endTime: cutoff);

        history.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAuditHistoryAsync_NullAgentId_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.GetAuditHistoryAsync(null!));
    }

    [Fact]
    public async Task GetAuditHistoryAsync_NoMatchingAgent_ReturnsEmpty()
    {
        await _sut.LogEvaluationAsync(CreateEntry(agentId: "agent-1"));

        var history = await _sut.GetAuditHistoryAsync("nonexistent");

        history.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAuditHistoryAsync_ReturnsOrderedByTimestampDescending()
    {
        var t1 = DateTime.UtcNow.AddMinutes(-3);
        var t2 = DateTime.UtcNow.AddMinutes(-2);
        var t3 = DateTime.UtcNow.AddMinutes(-1);

        await _sut.LogEvaluationAsync(CreateEntry(timestamp: t2));
        await _sut.LogEvaluationAsync(CreateEntry(timestamp: t1));
        await _sut.LogEvaluationAsync(CreateEntry(timestamp: t3));

        var history = await _sut.GetAuditHistoryAsync("agent-1");

        history.Should().HaveCount(3);
        history[0].Timestamp.Should().Be(t3);
        history[1].Timestamp.Should().Be(t2);
        history[2].Timestamp.Should().Be(t1);
    }

    [Fact]
    public void GetAllEntries_EmptyLog_ReturnsEmpty()
    {
        _sut.GetAllEntries().Should().BeEmpty();
    }
}
