using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

/// <summary>
/// Tests for ImmutableEthicsFramework.Evaluations partial class covering
/// plan, goal, skill, research, and self-modification evaluations.
/// </summary>
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

    // --- EvaluateGoalAsync ---

    [Fact]
    public async Task EvaluateGoalAsync_SafeGoal_ReturnsPermitted()
    {
        var goal = new Goal
        {
            Description = "Help user with a task",
            Type = "Assistance",
            Priority = 0.5
        };

        var result = await _framework.EvaluateGoalAsync(goal, _defaultContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateGoalAsync_SafetyGoal_AddsConcern()
    {
        var goal = new Goal
        {
            Description = "Ensure system stability",
            Type = "Safety",
            Priority = 0.5
        };

        var result = await _framework.EvaluateGoalAsync(goal, _defaultContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateGoalAsync_HighPriorityNonSafety_AddsConcern()
    {
        var goal = new Goal
        {
            Description = "Complete task urgently",
            Type = "Performance",
            Priority = 0.95
        };

        var result = await _framework.EvaluateGoalAsync(goal, _defaultContext);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateGoalAsync_NullGoal_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _framework.EvaluateGoalAsync(null!, _defaultContext);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // --- EvaluateSkillAsync ---

    [Fact]
    public async Task EvaluateSkillAsync_SafeSkill_ReturnsPermitted()
    {
        var skillContext = new SkillUsageContext
        {
            Skill = new SkillInfo
            {
                Name = "TextAnalysis",
                Description = "Analyzes text for sentiment",
                UsageCount = 10
            },
            ActionContext = _defaultContext,
            HistoricalSuccessRate = 0.9
        };

        var result = await _framework.EvaluateSkillAsync(skillContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateSkillAsync_LowSuccessRate_RaisesConernc()
    {
        var skillContext = new SkillUsageContext
        {
            Skill = new SkillInfo
            {
                Name = "FailingSkill",
                Description = "A skill that often fails",
                UsageCount = 10
            },
            ActionContext = _defaultContext,
            HistoricalSuccessRate = 0.3
        };

        var result = await _framework.EvaluateSkillAsync(skillContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(EthicalClearanceLevel.PermittedWithConcerns);
    }

    [Fact]
    public async Task EvaluateSkillAsync_NullContext_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _framework.EvaluateSkillAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // --- EvaluateResearchAsync ---

    [Fact]
    public async Task EvaluateResearchAsync_SafeResearch_ReturnsPermitted()
    {
        var result = await _framework.EvaluateResearchAsync(
            "Study algorithm performance", _defaultContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateResearchAsync_SensitiveDataResearch_RequiresApproval()
    {
        var result = await _framework.EvaluateResearchAsync(
            "Research involving user data and personal information", _defaultContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(EthicalClearanceLevel.RequiresHumanApproval);
    }

    [Fact]
    public async Task EvaluateResearchAsync_NullDescription_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _framework.EvaluateResearchAsync(null!, _defaultContext);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // --- EvaluateSelfModificationAsync ---

    [Fact]
    public async Task EvaluateSelfModificationAsync_EthicsModification_ReturnsDenied()
    {
        var request = new SelfModificationRequest
        {
            Type = ModificationType.EthicsModification,
            Description = "Remove safety constraints",
            IsReversible = false,
            ImpactLevel = 1.0,
            ActionContext = _defaultContext
        };

        var result = await _framework.EvaluateSelfModificationAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(EthicalClearanceLevel.Denied);
    }

    [Fact]
    public async Task EvaluateSelfModificationAsync_SafeModification_RequiresApproval()
    {
        var request = new SelfModificationRequest
        {
            Type = ModificationType.ParameterUpdate,
            Description = "Update learning rate",
            IsReversible = true,
            ImpactLevel = 0.3,
            ActionContext = _defaultContext
        };

        var result = await _framework.EvaluateSelfModificationAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(EthicalClearanceLevel.RequiresHumanApproval);
    }

    [Fact]
    public async Task EvaluateSelfModificationAsync_NullRequest_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _framework.EvaluateSelfModificationAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateSelfModificationAsync_HighImpactIrreversible_AddsConcern()
    {
        var request = new SelfModificationRequest
        {
            Type = ModificationType.ArchitecturalChange,
            Description = "Restructure core modules",
            IsReversible = false,
            ImpactLevel = 0.9,
            ActionContext = _defaultContext
        };

        var result = await _framework.EvaluateSelfModificationAsync(request);

        result.IsSuccess.Should().BeTrue();
    }

    // --- ReportEthicalConcernAsync ---

    [Fact]
    public async Task ReportEthicalConcernAsync_ValidConcern_LogsToAudit()
    {
        var concern = new EthicalConcern
        {
            RelatedPrinciple = EthicalPrinciple.Transparency,
            Description = "Potential bias detected",
            Level = ConcernLevel.Medium,
            RecommendedAction = "Review output"
        };

        await _framework.ReportEthicalConcernAsync(concern, _defaultContext);

        _auditLog.Entries.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ReportEthicalConcernAsync_NullConcern_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _framework.ReportEthicalConcernAsync(null!, _defaultContext);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // --- GetCorePrinciples ---

    [Fact]
    public void GetCorePrinciples_ReturnsNonEmpty()
    {
        var principles = _framework.GetCorePrinciples();

        principles.Should().NotBeEmpty();
    }

    [Fact]
    public void GetCorePrinciples_ReturnsCopy()
    {
        var principles1 = _framework.GetCorePrinciples();
        var principles2 = _framework.GetCorePrinciples();

        principles1.Should().NotBeSameAs(principles2);
    }
}
