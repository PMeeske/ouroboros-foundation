// <copyright file="CausalReasoningEngineTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Reasoning;

namespace Ouroboros.Core.Tests.Reasoning;

/// <summary>
/// Tests for CausalReasoningEngine implementing Pearl's causal inference framework.
/// </summary>
[Trait("Category", "Unit")]
public class CausalReasoningEngineTests
{
    private readonly CausalReasoningEngine _engine = new();

    // --- DiscoverCausalStructureAsync ---

    [Fact]
    public async Task DiscoverCausalStructureAsync_NullData_ReturnsFailure()
    {
        var result = await _engine.DiscoverCausalStructureAsync(
            null!, DiscoveryAlgorithm.PC);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_EmptyData_ReturnsFailure()
    {
        var result = await _engine.DiscoverCausalStructureAsync(
            new List<Observation>(), DiscoveryAlgorithm.PC);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_PC_ReturnsGraph()
    {
        // Arrange: create correlated data
        var data = CreateCorrelatedObservations();

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(
            data, DiscoveryAlgorithm.PC);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Variables.Should().NotBeEmpty();
        result.Value.Equations.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_FCI_ReturnsNotImplemented()
    {
        var data = CreateCorrelatedObservations();
        var result = await _engine.DiscoverCausalStructureAsync(
            data, DiscoveryAlgorithm.FCI);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_Cancellation_Throws()
    {
        var data = CreateCorrelatedObservations();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _engine.DiscoverCausalStructureAsync(
            data, DiscoveryAlgorithm.PC, cts.Token);

        result.IsFailure.Should().BeTrue();
    }

    // --- EstimateInterventionEffectAsync ---

    [Fact]
    public async Task EstimateInterventionEffectAsync_NullIntervention_ReturnsFailure()
    {
        var model = CreateSimpleCausalGraph();
        var result = await _engine.EstimateInterventionEffectAsync(null!, "Y", model);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EstimateInterventionEffectAsync_EmptyOutcome_ReturnsFailure()
    {
        var model = CreateSimpleCausalGraph();
        var result = await _engine.EstimateInterventionEffectAsync("X", "", model);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EstimateInterventionEffectAsync_NullModel_ReturnsFailure()
    {
        var result = await _engine.EstimateInterventionEffectAsync("X", "Y", null!);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EstimateInterventionEffectAsync_DirectEdge_ReturnsNonZero()
    {
        // Arrange: X -> Y with strength 0.8
        var model = CreateSimpleCausalGraph();

        // Act
        var result = await _engine.EstimateInterventionEffectAsync("X", "Y", model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(0.0);
    }

    [Fact]
    public async Task EstimateInterventionEffectAsync_NoPath_ReturnsZero()
    {
        // Arrange: model with X -> Y, but no path from Y to X
        var model = CreateSimpleCausalGraph();

        // Act
        var result = await _engine.EstimateInterventionEffectAsync("Y", "X", model);

        // Assert: no reverse path
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0.0);
    }

    // --- EstimateCounterfactualAsync ---

    [Fact]
    public async Task EstimateCounterfactualAsync_NullIntervention_ReturnsFailure()
    {
        var model = CreateSimpleCausalGraph();
        var factual = new Observation(
            new Dictionary<string, object> { { "X", 1.0 }, { "Y", 2.0 } },
            DateTime.UtcNow, null);

        var result = await _engine.EstimateCounterfactualAsync(null!, "Y", factual, model);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EstimateCounterfactualAsync_NullFactual_ReturnsFailure()
    {
        var model = CreateSimpleCausalGraph();
        var result = await _engine.EstimateCounterfactualAsync("X", "Y", null!, model);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EstimateCounterfactualAsync_ValidInput_ReturnsDistribution()
    {
        // Arrange
        var model = CreateSimpleCausalGraph();
        var factual = new Observation(
            new Dictionary<string, object> { { "X", 1.0 }, { "Y", 2.0 } },
            DateTime.UtcNow, null);

        // Act
        var result = await _engine.EstimateCounterfactualAsync("X", "Y", factual, model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    // --- ExplainCausallyAsync ---

    [Fact]
    public async Task ExplainCausallyAsync_NullEffect_ReturnsFailure()
    {
        var model = CreateSimpleCausalGraph();
        var result = await _engine.ExplainCausallyAsync(
            null!, new List<string> { "X" }, model);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExplainCausallyAsync_EmptyCauses_ReturnsFailure()
    {
        var model = CreateSimpleCausalGraph();
        var result = await _engine.ExplainCausallyAsync(
            "Y", new List<string>(), model);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExplainCausallyAsync_ValidInput_ReturnsExplanation()
    {
        // Arrange
        var model = CreateSimpleCausalGraph();

        // Act
        var result = await _engine.ExplainCausallyAsync(
            "Y", new List<string> { "X" }, model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Effect.Should().Be("Y");
        result.Value.NarrativeExplanation.Should().NotBeNullOrEmpty();
    }

    // --- PlanInterventionAsync ---

    [Fact]
    public async Task PlanInterventionAsync_NullDesiredOutcome_ReturnsFailure()
    {
        var model = CreateSimpleCausalGraph();
        var result = await _engine.PlanInterventionAsync(
            null!, model, new List<string> { "X" });
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task PlanInterventionAsync_EmptyControllables_ReturnsFailure()
    {
        var model = CreateSimpleCausalGraph();
        var result = await _engine.PlanInterventionAsync(
            "Y", model, new List<string>());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task PlanInterventionAsync_ValidInput_ReturnsIntervention()
    {
        // Arrange
        var model = CreateSimpleCausalGraph();

        // Act
        var result = await _engine.PlanInterventionAsync(
            "Y", model, new List<string> { "X" });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TargetVariable.Should().Be("X");
    }

    [Fact]
    public async Task PlanInterventionAsync_NoPathToOutcome_ReturnsFailure()
    {
        // Arrange: model with X -> Y only
        var model = CreateSimpleCausalGraph();

        // Act: try to plan intervention on Y (no edges from Y)
        var result = await _engine.PlanInterventionAsync(
            "X", model, new List<string> { "Y" });

        // Assert: Y has no outgoing edges to X
        result.IsFailure.Should().BeTrue();
    }

    // --- Helper methods ---

    private static List<Observation> CreateCorrelatedObservations()
    {
        var random = new Random(42);
        var data = new List<Observation>();

        for (int i = 0; i < 100; i++)
        {
            double x = random.NextDouble() * 10;
            double y = x * 0.8 + random.NextDouble(); // Y strongly correlated with X
            double z = random.NextDouble() * 10; // Z independent

            data.Add(new Observation(
                new Dictionary<string, object>
                {
                    { "X", x },
                    { "Y", y },
                    { "Z", z }
                },
                DateTime.UtcNow.AddMinutes(-i),
                null));
        }

        return data;
    }

    private static CausalGraph CreateSimpleCausalGraph()
    {
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0, 1.0, 2.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0, 1.0, 2.0 })
        };

        var edges = new List<CausalEdge>
        {
            new("X", "Y", 0.8, EdgeType.Direct)
        };

        var equations = new Dictionary<string, StructuralEquation>
        {
            ["X"] = new("X", new List<string>(),
                values => values.GetValueOrDefault("X", 0.0), 1.0),
            ["Y"] = new("Y", new List<string> { "X" },
                values =>
                {
                    var x = Convert.ToDouble(values.GetValueOrDefault("X", 0.0));
                    return x * 0.8;
                }, 0.5)
        };

        return new CausalGraph(variables, edges, equations);
    }
}
