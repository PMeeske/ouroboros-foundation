using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicsFrameworkFactoryTests
{
    [Fact]
    public void CreateDefault_ReturnsNonNullFramework()
    {
        var framework = EthicsFrameworkFactory.CreateDefault();

        framework.Should().NotBeNull();
        framework.Should().BeAssignableTo<IEthicsFramework>();
    }

    [Fact]
    public void CreateDefault_ReturnedFrameworkHasCorePrinciples()
    {
        var framework = EthicsFrameworkFactory.CreateDefault();

        framework.GetCorePrinciples().Should().NotBeEmpty();
    }

    [Fact]
    public void CreateWithAuditLog_NullAuditLog_ThrowsArgumentNullException()
    {
        var act = () => EthicsFrameworkFactory.CreateWithAuditLog(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateWithAuditLog_ValidLog_ReturnsFramework()
    {
        var auditLog = new InMemoryEthicsAuditLog();

        var framework = EthicsFrameworkFactory.CreateWithAuditLog(auditLog);

        framework.Should().NotBeNull();
    }

    [Fact]
    public void CreateCustom_NullAuditLog_ThrowsArgumentNullException()
    {
        var act = () => EthicsFrameworkFactory.CreateCustom(null!, new Mock<IEthicalReasoner>().Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateCustom_NullReasoner_ThrowsArgumentNullException()
    {
        var act = () => EthicsFrameworkFactory.CreateCustom(new InMemoryEthicsAuditLog(), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateCustom_ValidParams_ReturnsFramework()
    {
        var framework = EthicsFrameworkFactory.CreateCustom(
            new InMemoryEthicsAuditLog(), new Mock<IEthicalReasoner>().Object);

        framework.Should().NotBeNull();
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class InMemoryEthicsAuditLogTests
{
    private readonly InMemoryEthicsAuditLog _sut = new();

    [Fact]
    public async Task LogEvaluationAsync_NullEntry_ThrowsArgumentNullException()
    {
        var act = () => _sut.LogEvaluationAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task LogEvaluationAsync_ValidEntry_StoresEntry()
    {
        var entry = new EthicsAuditEntry
        {
            AgentId = "agent-1",
            Timestamp = DateTime.UtcNow,
            EvaluationType = "Action",
            Description = "Test action",
            Clearance = EthicalClearance.Permitted("OK")
        };

        await _sut.LogEvaluationAsync(entry);

        var entries = _sut.GetAllEntries();
        entries.Should().HaveCount(1);
        entries[0].AgentId.Should().Be("agent-1");
    }

    [Fact]
    public async Task LogViolationAttemptAsync_NullAgentId_ThrowsArgumentNullException()
    {
        var act = () => _sut.LogViolationAttemptAsync(
            null!, null, "desc", new List<EthicalViolation>());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task LogViolationAttemptAsync_NullDescription_ThrowsArgumentNullException()
    {
        var act = () => _sut.LogViolationAttemptAsync(
            "agent", null, null!, new List<EthicalViolation>());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task LogViolationAttemptAsync_NullViolations_ThrowsArgumentNullException()
    {
        var act = () => _sut.LogViolationAttemptAsync(
            "agent", null, "desc", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task LogViolationAttemptAsync_ValidInput_StoresViolationEntry()
    {
        var violations = new List<EthicalViolation>
        {
            new()
            {
                ViolatedPrinciple = EthicalPrinciple.DoNoHarm,
                Description = "Harmful action",
                Severity = ViolationSeverity.High,
                Evidence = "test",
                AffectedParties = new List<string> { "Users" }
            }
        };

        await _sut.LogViolationAttemptAsync("agent-1", "user-1", "Attempted harmful action", violations);

        var entries = _sut.GetAllEntries();
        entries.Should().HaveCount(1);
        entries[0].EvaluationType.Should().Be("ViolationAttempt");
    }

    [Fact]
    public async Task GetAuditHistoryAsync_NullAgentId_ThrowsArgumentNullException()
    {
        var act = () => _sut.GetAuditHistoryAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetAuditHistoryAsync_FiltersById()
    {
        var entry1 = new EthicsAuditEntry { AgentId = "agent-1", Timestamp = DateTime.UtcNow, EvaluationType = "Action", Description = "test1", Clearance = EthicalClearance.Permitted("OK") };
        var entry2 = new EthicsAuditEntry { AgentId = "agent-2", Timestamp = DateTime.UtcNow, EvaluationType = "Action", Description = "test2", Clearance = EthicalClearance.Permitted("OK") };

        await _sut.LogEvaluationAsync(entry1);
        await _sut.LogEvaluationAsync(entry2);

        var history = await _sut.GetAuditHistoryAsync("agent-1");
        history.Should().HaveCount(1);
        history[0].AgentId.Should().Be("agent-1");
    }

    [Fact]
    public async Task GetAuditHistoryAsync_FiltersByTimeRange()
    {
        var earlyEntry = new EthicsAuditEntry
        {
            AgentId = "agent-1",
            Timestamp = DateTime.UtcNow.AddHours(-2),
            EvaluationType = "Action",
            Description = "early",
            Clearance = EthicalClearance.Permitted("OK")
        };
        var lateEntry = new EthicsAuditEntry
        {
            AgentId = "agent-1",
            Timestamp = DateTime.UtcNow,
            EvaluationType = "Action",
            Description = "late",
            Clearance = EthicalClearance.Permitted("OK")
        };

        await _sut.LogEvaluationAsync(earlyEntry);
        await _sut.LogEvaluationAsync(lateEntry);

        var history = await _sut.GetAuditHistoryAsync("agent-1",
            startTime: DateTime.UtcNow.AddMinutes(-5));
        history.Should().HaveCount(1);
        history[0].Description.Should().Be("late");
    }

    [Fact]
    public async Task GetAllEntries_OrderedByTimestampDescending()
    {
        var entry1 = new EthicsAuditEntry
        {
            AgentId = "a", Timestamp = DateTime.UtcNow.AddHours(-2),
            EvaluationType = "A", Description = "first", Clearance = EthicalClearance.Permitted("OK")
        };
        var entry2 = new EthicsAuditEntry
        {
            AgentId = "a", Timestamp = DateTime.UtcNow,
            EvaluationType = "A", Description = "second", Clearance = EthicalClearance.Permitted("OK")
        };

        await _sut.LogEvaluationAsync(entry1);
        await _sut.LogEvaluationAsync(entry2);

        var entries = _sut.GetAllEntries();
        entries[0].Timestamp.Should().BeOnOrAfter(entries[1].Timestamp);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class AutoDenyApprovalProviderTests
{
    [Fact]
    public async Task RequestApprovalAsync_AlwaysRejectsRequest()
    {
        var sut = new AutoDenyApprovalProvider();
        var request = new HumanApprovalRequest
        {
            Category = "action",
            Description = "Test action",
            Clearance = EthicalClearance.RequiresApproval("Needs review")
        };

        var response = await sut.RequestApprovalAsync(request);

        response.Decision.Should().Be(HumanApprovalDecision.Rejected);
        response.RequestId.Should().Be(request.Id);
    }
}
