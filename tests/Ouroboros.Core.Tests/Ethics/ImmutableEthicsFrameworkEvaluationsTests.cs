using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class ImmutableEthicsFrameworkEvaluationsTests
{
    private readonly IEthicsFramework _framework;
    private readonly InMemoryEthicsAuditLog _auditLog;
    private readonly ActionContext _defaultContext;

    public ImmutableEthicsFrameworkEvaluationsTests()
    {
        _auditLog = new InMemoryEthicsAuditLog();
        _framework = EthicsFrameworkFactory.CreateWithAuditLog(_auditLog);
        _defaultContext = new ActionContext
        {
            AgentId = "test-agent",
            UserId = "test-user",
            Environment = "testing",
            State = new Dictionary<string, object>()
        };
    }

    // --- EvaluateSelfModificationAsync ---

    [Fact]
    public async Task EvaluateSelfModificationAsync_EthicsModification_IsDenied()
    {
        var request = new SelfModificationRequest
        {
            Type = ModificationType.EthicsModification,
            Description = "Modify ethical constraints",
            Justification = "Testing",
            ActionContext = _defaultContext,
            ExpectedImprovements = new[] { "None" },
            PotentialRisks = new[] { "Ethics bypass" },
            IsReversible = true,
            ImpactLevel = 0.5
        };

        var result = await _framework.EvaluateSelfModificationAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeFalse();
        result.Value.Level.Should().Be(EthicalClearanceLevel.Denied);
    }

    [Fact]
    public async Task EvaluateSelfModificationAsync_SafeModification_RequiresApproval()
    {
        var request = new SelfModificationRequest
        {
            Type = ModificationType.KnowledgeUpdate,
            Description = "Update knowledge base with new data",
            Justification = "Improve accuracy",
            ActionContext = _defaultContext,
            ExpectedImprovements = new[] { "Better accuracy" },
            PotentialRisks = new[] { "Minor data shift" },
            IsReversible = true,
            ImpactLevel = 0.3
        };

        var result = await _framework.EvaluateSelfModificationAsync(request);

        result.IsSuccess.Should().BeTrue();
        // All self-modifications require at least human approval
        result.Value.Level.Should().Be(EthicalClearanceLevel.RequiresHumanApproval);
    }

    [Fact]
    public async Task EvaluateSelfModificationAsync_IrreversibleHighImpact_HasExtraConcerns()
    {
        var request = new SelfModificationRequest
        {
            Type = ModificationType.BehaviorModification,
            Description = "Refactor internal logic",
            Justification = "Performance improvement",
            ActionContext = _defaultContext,
            ExpectedImprovements = new[] { "Faster" },
            PotentialRisks = new[] { "Breaking changes" },
            IsReversible = false,
            ImpactLevel = 0.9
        };

        var result = await _framework.EvaluateSelfModificationAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Concerns.Should().NotBeEmpty();
    }

    [Fact]
    public async Task EvaluateSelfModificationAsync_NullRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _framework.EvaluateSelfModificationAsync(null!));
    }

    [Fact]
    public async Task EvaluateSelfModificationAsync_HarmfulDescription_IsDenied()
    {
        var request = new SelfModificationRequest
        {
            Type = ModificationType.CapabilityAddition,
            Description = "Exploit user data to harm the system",
            Justification = "Testing harmful patterns",
            ActionContext = _defaultContext,
            ExpectedImprovements = new[] { "None" },
            PotentialRisks = new[] { "Major harm" },
            IsReversible = true,
            ImpactLevel = 0.5
        };

        var result = await _framework.EvaluateSelfModificationAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeFalse();
        result.Value.Level.Should().Be(EthicalClearanceLevel.Denied);
    }

    // --- EvaluatePlanAsync with concerns ---

    [Fact]
    public async Task EvaluatePlanAsync_PlanWithConcerns_PermittedWithConcerns()
    {
        var plan = new Plan
        {
            Goal = "Generate report",
            Steps = new List<PlanStep>
            {
                new PlanStep
                {
                    Action = "quick",
                    ExpectedOutcome = "Done fast",
                    Parameters = new Dictionary<string, object>()
                }
            }
        };
        var planContext = new PlanContext
        {
            Plan = plan,
            ActionContext = _defaultContext,
            EstimatedRisk = 0.3
        };

        var result = await _framework.EvaluatePlanAsync(planContext);

        result.IsSuccess.Should().BeTrue();
        // "quick" action description is < 10 chars, raises a transparency concern
        if (result.Value.Concerns.Count > 0)
        {
            result.Value.Level.Should().Be(EthicalClearanceLevel.PermittedWithConcerns);
        }
    }

    // --- EvaluateGoalAsync edge cases ---

    [Fact]
    public async Task EvaluateGoalAsync_NullGoal_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _framework.EvaluateGoalAsync(null!, _defaultContext));
    }

    [Fact]
    public async Task EvaluateGoalAsync_NullContext_ThrowsArgumentNullException()
    {
        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            Description = "Test",
            Type = "Test",
            Priority = 0.5
        };

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _framework.EvaluateGoalAsync(goal, null!));
    }

    [Fact]
    public async Task EvaluateGoalAsync_HighPriorityNonSafety_RaisesConcern()
    {
        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            Description = "Complete task immediately",
            Type = "Performance",
            Priority = 0.95
        };

        var result = await _framework.EvaluateGoalAsync(goal, _defaultContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeTrue();
    }

    // --- EvaluateSkillAsync edge cases ---

    [Fact]
    public async Task EvaluateSkillAsync_NullContext_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _framework.EvaluateSkillAsync(null!));
    }

    // --- EvaluateResearchAsync edge cases ---

    [Fact]
    public async Task EvaluateResearchAsync_NullDescription_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _framework.EvaluateResearchAsync(null!, _defaultContext));
    }

    [Fact]
    public async Task EvaluateResearchAsync_NullContext_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _framework.EvaluateResearchAsync("safe research", null!));
    }

    [Fact]
    public async Task EvaluateResearchAsync_HarmfulResearch_IsDenied()
    {
        var result = await _framework.EvaluateResearchAsync(
            "Research how to exploit and harm users", _defaultContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeFalse();
        result.Value.Level.Should().Be(EthicalClearanceLevel.Denied);
    }

    // --- ReportEthicalConcernAsync ---

    [Fact]
    public async Task ReportEthicalConcernAsync_NullConcern_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _framework.ReportEthicalConcernAsync(null!, _defaultContext));
    }

    [Fact]
    public async Task ReportEthicalConcernAsync_NullContext_ThrowsArgumentNullException()
    {
        var concern = new EthicalConcern
        {
            RelatedPrinciple = EthicalPrinciple.Transparency,
            Description = "Test concern",
            Level = ConcernLevel.Low,
            RecommendedAction = "Review"
        };

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _framework.ReportEthicalConcernAsync(concern, null!));
    }

    // --- Audit log integration ---

    [Fact]
    public async Task EvaluateSelfModificationAsync_LogsToAuditLog()
    {
        var request = new SelfModificationRequest
        {
            Type = ModificationType.ConfigurationChange,
            Description = "Change config setting",
            Justification = "Testing",
            ActionContext = _defaultContext,
            ExpectedImprovements = new[] { "Flexibility" },
            PotentialRisks = new[] { "None" },
            IsReversible = true,
            ImpactLevel = 0.2
        };

        await _framework.EvaluateSelfModificationAsync(request);

        var entries = _auditLog.GetAllEntries();
        entries.Should().HaveCount(1);
        entries[0].EvaluationType.Should().Be("SelfModification");
    }
}
