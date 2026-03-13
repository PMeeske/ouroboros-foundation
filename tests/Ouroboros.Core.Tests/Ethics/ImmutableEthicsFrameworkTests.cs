// <copyright file="ImmutableEthicsFrameworkTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

/// <summary>
/// Tests for ImmutableEthicsFramework covering plan, goal, skill, and research evaluation.
/// Complements AlignmentSpecificationTests and EthicsFrameworkAdversarialTests.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class ImmutableEthicsFrameworkTests
{
    private readonly IEthicsFramework _framework;
    private readonly InMemoryEthicsAuditLog _auditLog;
    private readonly ActionContext _defaultContext;

    public ImmutableEthicsFrameworkTests()
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

    // --- EvaluatePlanAsync ---

    [Fact]
    public async Task EvaluatePlanAsync_SafePlan_IsPermitted()
    {
        // Arrange
        var plan = new Plan
        {
            Goal = "Generate performance report",
            Steps = new List<PlanStep>
            {
                new PlanStep
                {
                    Action = "read_data",
                    ExpectedOutcome = "Read public statistics",
                    Parameters = new Dictionary<string, object>()
                },
                new PlanStep
                {
                    Action = "compute",
                    ExpectedOutcome = "Calculate averages",
                    Parameters = new Dictionary<string, object>()
                }
            }
        };
        var planContext = new PlanContext
        {
            Plan = plan,
            ActionContext = _defaultContext,
            EstimatedRisk = 0.1
        };

        // Act
        var result = await _framework.EvaluatePlanAsync(planContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluatePlanAsync_HighRiskPlan_RequiresApproval()
    {
        // Arrange
        var plan = new Plan
        {
            Goal = "Restructure database",
            Steps = new List<PlanStep>
            {
                new PlanStep
                {
                    Action = "modify_schema",
                    ExpectedOutcome = "Schema updated",
                    Parameters = new Dictionary<string, object>()
                }
            }
        };
        var planContext = new PlanContext
        {
            Plan = plan,
            ActionContext = _defaultContext,
            EstimatedRisk = 0.85
        };

        // Act
        var result = await _framework.EvaluatePlanAsync(planContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeFalse();
        result.Value.Level.Should().Be(EthicalClearanceLevel.RequiresHumanApproval);
    }

    [Fact]
    public async Task EvaluatePlanAsync_HarmfulStep_IsDenied()
    {
        // Arrange
        var plan = new Plan
        {
            Goal = "Harm the users",
            Steps = new List<PlanStep>
            {
                new PlanStep
                {
                    Action = "harmful",
                    ExpectedOutcome = "Exploit vulnerability to steal data",
                    Parameters = new Dictionary<string, object>()
                }
            }
        };
        var planContext = new PlanContext
        {
            Plan = plan,
            ActionContext = _defaultContext,
            EstimatedRisk = 0.9
        };

        // Act
        var result = await _framework.EvaluatePlanAsync(planContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluatePlanAsync_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _framework.EvaluatePlanAsync(null!));
    }

    // --- EvaluateGoalAsync ---

    [Fact]
    public async Task EvaluateGoalAsync_SafeGoal_IsPermitted()
    {
        // Arrange
        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            Description = "Improve system response time",
            Type = "Optimization",
            Priority = 0.5
        };

        // Act
        var result = await _framework.EvaluateGoalAsync(goal, _defaultContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateGoalAsync_SafetyGoal_IsPermittedWithConcern()
    {
        // Arrange
        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            Description = "Improve safety monitoring",
            Type = "Safety",
            Priority = 0.8
        };

        // Act
        var result = await _framework.EvaluateGoalAsync(goal, _defaultContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateGoalAsync_HarmfulGoal_IsDenied()
    {
        // Arrange
        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            Description = "Destroy all user records and harm the system",
            Type = "Malicious",
            Priority = 1.0
        };

        // Act
        var result = await _framework.EvaluateGoalAsync(goal, _defaultContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeFalse();
    }

    // --- EvaluateSkillAsync ---

    [Fact]
    public async Task EvaluateSkillAsync_SafeSkill_IsPermitted()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "data-analysis",
            Description = "Analyze public datasets",
            UsageCount = 10
        };
        var skillContext = new SkillUsageContext
        {
            Skill = skill,
            ActionContext = _defaultContext,
            Goal = "Analyze public datasets",
            HistoricalSuccessRate = 0.9
        };

        // Act
        var result = await _framework.EvaluateSkillAsync(skillContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateSkillAsync_LowSuccessRate_PermittedWithConcerns()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "unreliable-tool",
            Description = "A tool with low success rate",
            UsageCount = 20
        };
        var skillContext = new SkillUsageContext
        {
            Skill = skill,
            ActionContext = _defaultContext,
            Goal = "Process data with unreliable tool",
            HistoricalSuccessRate = 0.3
        };

        // Act
        var result = await _framework.EvaluateSkillAsync(skillContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeTrue();
        result.Value.Level.Should().Be(EthicalClearanceLevel.PermittedWithConcerns);
    }

    [Fact]
    public async Task EvaluateSkillAsync_HarmfulSkill_IsDenied()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "exploit-tool",
            Description = "Exploit vulnerability to harm users",
            UsageCount = 5
        };
        var skillContext = new SkillUsageContext
        {
            Skill = skill,
            ActionContext = _defaultContext,
            Goal = "Exploit vulnerability to harm users",
            HistoricalSuccessRate = 0.9
        };

        // Act
        var result = await _framework.EvaluateSkillAsync(skillContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeFalse();
    }

    // --- EvaluateResearchAsync ---

    [Fact]
    public async Task EvaluateResearchAsync_SafeResearch_IsPermitted()
    {
        // Act
        var result = await _framework.EvaluateResearchAsync(
            "Analysis of publicly available performance metrics",
            _defaultContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateResearchAsync_SensitiveResearch_RequiresApproval()
    {
        // Act
        var result = await _framework.EvaluateResearchAsync(
            "Analysis of user data and personal information",
            _defaultContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsPermitted.Should().BeFalse();
        result.Value.Level.Should().Be(EthicalClearanceLevel.RequiresHumanApproval);
    }

    // --- ReportEthicalConcernAsync ---

    [Fact]
    public async Task ReportEthicalConcernAsync_LogsConcern()
    {
        // Arrange
        var concern = new EthicalConcern
        {
            RelatedPrinciple = EthicalPrinciple.Transparency,
            Description = "Model output lacked explanation",
            Level = ConcernLevel.Medium,
            RecommendedAction = "Add reasoning chain"
        };

        // Act
        await _framework.ReportEthicalConcernAsync(concern, _defaultContext);

        // Assert
        var entries = _auditLog.GetAllEntries();
        entries.Should().HaveCount(1);
        entries[0].EvaluationType.Should().Be("ConcernReport");
    }

    // --- AuditLog integration ---

    [Fact]
    public async Task AllEvaluations_AreRecordedInAuditLog()
    {
        // Arrange
        var safeAction = new ProposedAction
        {
            ActionType = "read_data",
            Description = "Read public stats",
            Parameters = new Dictionary<string, object>(),
            PotentialEffects = new[] { "Data read" }
        };
        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            Description = "Optimize performance",
            Type = "Optimization",
            Priority = 0.5
        };

        // Act
        await _framework.EvaluateActionAsync(safeAction, _defaultContext);
        await _framework.EvaluateGoalAsync(goal, _defaultContext);
        await _framework.EvaluateResearchAsync("Public metric analysis", _defaultContext);

        // Assert
        var entries = _auditLog.GetAllEntries();
        entries.Should().HaveCount(3);
        entries.Select(e => e.EvaluationType).Should().Contain("Action");
        entries.Select(e => e.EvaluationType).Should().Contain("Goal");
        entries.Select(e => e.EvaluationType).Should().Contain("Research");
    }
}
