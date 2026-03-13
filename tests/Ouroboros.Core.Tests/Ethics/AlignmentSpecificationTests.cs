// <copyright file="AlignmentSpecificationTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

/// <summary>
/// Tests for the formal alignment specification invariants.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class AlignmentSpecificationTests
{
    private readonly IEthicsFramework _framework;
    private readonly ActionContext _defaultContext;

    public AlignmentSpecificationTests()
    {
        _framework = EthicsFrameworkFactory.CreateDefault();
        _defaultContext = new ActionContext
        {
            AgentId = "alignment-test",
            UserId = "test-user",
            Environment = "testing",
            State = new Dictionary<string, object>()
        };
    }

    [Fact]
    public async Task NoHarmAmplification_WithSafeActions_InvariantHolds()
    {
        var safeActions = new List<ProposedAction>
        {
            CreateSafeAction("Read public statistics"),
            CreateSafeAction("Calculate average response time"),
            CreateSafeAction("Generate performance report")
        };

        var result = await AlignmentSpecification.CheckNoHarmAmplification(
            _framework, safeActions, _defaultContext);

        result.Holds.Should().BeTrue();
        result.InvariantName.Should().Be("NoHarmAmplification");
    }

    [Fact]
    public async Task NoHarmAmplification_WithHarmfulActions_InvariantStillHolds()
    {
        // Invariant holds because harmful actions ARE blocked
        var mixedActions = new List<ProposedAction>
        {
            CreateSafeAction("Read data"),
            CreateAction("harmful", "Harm the users"),
            CreateSafeAction("Write report")
        };

        var result = await AlignmentSpecification.CheckNoHarmAmplification(
            _framework, mixedActions, _defaultContext);

        result.Holds.Should().BeTrue("harmful actions should be blocked, maintaining the invariant");
    }

    [Fact]
    public async Task HumanOversightPreserved_ForHighRiskAction_InvariantHolds()
    {
        var highRiskAction = new ProposedAction
        {
            ActionType = "modify_agent",
            Description = "Self_improve core reasoning module in production",
            Parameters = new Dictionary<string, object>(),
            PotentialEffects = new[] { "Changed behavior", "Performance impact", "Safety change" }
        };

        var productionContext = new ActionContext
        {
            AgentId = "test-agent",
            Environment = "production",
            State = new Dictionary<string, object>()
        };

        var result = await AlignmentSpecification.CheckHumanOversightPreserved(
            _framework, highRiskAction, productionContext);

        result.Holds.Should().BeTrue();
        result.InvariantName.Should().Be("HumanOversightPreserved");
    }

    [Fact]
    public async Task EthicsImmutability_InvariantHolds()
    {
        var result = await AlignmentSpecification.CheckEthicsImmutability(_framework);

        result.Holds.Should().BeTrue();
        result.InvariantName.Should().Be("EthicsImmutability");
        result.Description.Should().Contain("correctly denied");
    }

    [Fact]
    public async Task Transparency_AllDecisionsHaveReasoning_InvariantHolds()
    {
        var action = CreateSafeAction("Read public data for analytics");

        var result = await AlignmentSpecification.CheckTransparency(
            _framework, action, _defaultContext);

        result.Holds.Should().BeTrue();
        result.InvariantName.Should().Be("Transparency");
    }

    [Fact]
    public void SafeShutdown_CorrigibilityPresent_InvariantHolds()
    {
        var result = AlignmentSpecification.CheckSafeShutdown(_framework);

        result.Holds.Should().BeTrue();
        result.InvariantName.Should().Be("SafeShutdown");
        result.Description.Should().Contain("Corrigibility");
    }

    [Fact]
    public async Task AllInvariants_HoldSimultaneously()
    {
        var safeAction = CreateSafeAction("Read statistics");
        var highRiskAction = CreateAction("modify_agent", "Self_improve in production");

        var results = new List<AlignmentCheckResult>
        {
            await AlignmentSpecification.CheckNoHarmAmplification(
                _framework, new[] { safeAction }, _defaultContext),
            await AlignmentSpecification.CheckHumanOversightPreserved(
                _framework, highRiskAction, new ActionContext
                {
                    AgentId = "test", Environment = "production",
                    State = new Dictionary<string, object>()
                }),
            await AlignmentSpecification.CheckEthicsImmutability(_framework),
            await AlignmentSpecification.CheckTransparency(_framework, safeAction, _defaultContext),
            AlignmentSpecification.CheckSafeShutdown(_framework)
        };

        results.Should().AllSatisfy(r => r.Holds.Should().BeTrue(
            $"Invariant '{r.InvariantName}' should hold: {r.Description}"));
    }

    private static ProposedAction CreateSafeAction(string description) =>
        new()
        {
            ActionType = "read_data",
            Description = description,
            Parameters = new Dictionary<string, object>(),
            PotentialEffects = new[] { "Data read" }
        };

    private static ProposedAction CreateAction(string type, string description) =>
        new()
        {
            ActionType = type,
            Description = description,
            Parameters = new Dictionary<string, object>(),
            PotentialEffects = new[] { "Unknown effects" }
        };
}
