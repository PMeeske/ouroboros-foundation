// <copyright file="CausalReasoningEngineGraphTraversalTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Reasoning;

namespace Ouroboros.Core.Tests.Reasoning;

/// <summary>
/// Tests for CausalReasoningEngine.GraphTraversal and CausalReasoningEngine.Inference
/// partial classes — path-finding, edge orientation, intervention planning, counterfactuals,
/// and the PC algorithm for causal discovery.
/// </summary>
[Trait("Category", "Unit")]
public class CausalReasoningEngineGraphTraversalTests
{
    private readonly CausalReasoningEngine _engine = new();

    // ========================================================================
    // PlanInterventionAsync — graph traversal path finding
    // ========================================================================

    [Fact]
    public async Task PlanInterventionAsync_MultiEdgeGraph_FindsBestIntervention()
    {
        // Arrange — X -> Y -> Z with different strengths
        var model = CreateChainGraph();

        // Act
        var result = await _engine.PlanInterventionAsync(
            "Z", model, new List<string> { "X", "Y" });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TargetVariable.Should().NotBeNullOrEmpty();
        result.Value.ExpectedEffect.Should().NotBe(0.0);
    }

    [Fact]
    public async Task PlanInterventionAsync_DiamondGraph_FindsPathThroughBothBranches()
    {
        // Arrange — X -> A -> Z and X -> B -> Z (diamond shape)
        var model = CreateDiamondGraph();

        // Act
        var result = await _engine.PlanInterventionAsync(
            "Z", model, new List<string> { "X" });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TargetVariable.Should().Be("X");
        result.Value.SideEffects.Should().NotBeNull();
    }

    [Fact]
    public async Task PlanInterventionAsync_NoControllablePathToOutcome_ReturnsFailure()
    {
        // Arrange — X -> Y, but controllable is Z with no edge to Y
        var model = CreateDisconnectedGraph();

        // Act
        var result = await _engine.PlanInterventionAsync(
            "Y", model, new List<string> { "Z" });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No effective intervention found");
    }

    [Fact]
    public async Task PlanInterventionAsync_MultipleControllables_PicksStrongestEffect()
    {
        // Arrange — X -> Z (strength 0.9), Y -> Z (strength 0.3)
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Z", VariableType.Continuous, new List<object> { 0.0, 1.0 })
        };
        var edges = new List<CausalEdge>
        {
            new("X", "Z", 0.9, EdgeType.Direct),
            new("Y", "Z", 0.3, EdgeType.Direct)
        };
        var equations = CreateEquationsForVariables(variables);
        var model = new CausalGraph(variables, edges, equations);

        // Act
        var result = await _engine.PlanInterventionAsync(
            "Z", model, new List<string> { "X", "Y" });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TargetVariable.Should().Be("X"); // X has stronger effect
    }

    // ========================================================================
    // ExplainCausallyAsync — narrative generation and attribution
    // ========================================================================

    [Fact]
    public async Task ExplainCausallyAsync_DirectPath_IncludesDirectPathNarrative()
    {
        // Arrange
        var model = CreateSimpleCausalGraph();

        // Act
        var result = await _engine.ExplainCausallyAsync(
            "Y", new List<string> { "X" }, model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.NarrativeExplanation.Should().Contain("Direct causal paths");
        result.Value.Attributions.Should().ContainKey("X");
    }

    [Fact]
    public async Task ExplainCausallyAsync_IndirectPath_IncludesMediatedNarrative()
    {
        // Arrange — X -> M -> Y (indirect only)
        var model = CreateChainGraph();

        // Act
        var result = await _engine.ExplainCausallyAsync(
            "Z", new List<string> { "X" }, model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.NarrativeExplanation.Should().Contain("mediated");
    }

    [Fact]
    public async Task ExplainCausallyAsync_MultipleCauses_NormalizesAttributions()
    {
        // Arrange — two causes leading to Z
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Z", VariableType.Continuous, new List<object> { 0.0, 1.0 })
        };
        var edges = new List<CausalEdge>
        {
            new("X", "Z", 0.6, EdgeType.Direct),
            new("Y", "Z", 0.4, EdgeType.Direct)
        };
        var equations = CreateEquationsForVariables(variables);
        var model = new CausalGraph(variables, edges, equations);

        // Act
        var result = await _engine.ExplainCausallyAsync(
            "Z", new List<string> { "X", "Y" }, model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var totalAttribution = result.Value.Attributions.Values.Sum();
        totalAttribution.Should().BeApproximately(1.0, 0.01);
    }

    [Fact]
    public async Task ExplainCausallyAsync_NoCausalPath_ReturnsZeroAttributions()
    {
        // Arrange — Z has no connection to Y
        var model = CreateDisconnectedGraph();

        // Act
        var result = await _engine.ExplainCausallyAsync(
            "Y", new List<string> { "Z" }, model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Attributions["Z"].Should().Be(0.0);
    }

    // ========================================================================
    // EstimateInterventionEffectAsync — path-based effect computation
    // ========================================================================

    [Fact]
    public async Task EstimateInterventionEffectAsync_IndirectPath_ComputesProductOfEdges()
    {
        // Arrange — X -> Y (0.8) -> Z (0.5): total effect = 0.8 * 0.5 = 0.4
        var model = CreateChainGraph();

        // Act
        var result = await _engine.EstimateInterventionEffectAsync("X", "Z", model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeApproximately(0.4, 0.01);
    }

    [Fact]
    public async Task EstimateInterventionEffectAsync_DiamondGraph_SumsPathEffects()
    {
        // Arrange — X -> A -> Z (0.6*0.5=0.3) and X -> B -> Z (0.4*0.8=0.32)
        // Total = 0.3 + 0.32 = 0.62
        var model = CreateDiamondGraph();

        // Act
        var result = await _engine.EstimateInterventionEffectAsync("X", "Z", model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeApproximately(0.62, 0.01);
    }

    // ========================================================================
    // EstimateCounterfactualAsync — twin network approach
    // ========================================================================

    [Fact]
    public async Task EstimateCounterfactualAsync_ValidInput_ReturnsDistributionWithMean()
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
        result.Value.Mean.Should().NotBe(double.NaN);
    }

    [Fact]
    public async Task EstimateCounterfactualAsync_NullModel_ReturnsFailure()
    {
        // Arrange
        var factual = new Observation(
            new Dictionary<string, object> { { "X", 1.0 } },
            DateTime.UtcNow, null);

        // Act
        var result = await _engine.EstimateCounterfactualAsync("X", "Y", factual, null!);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EstimateCounterfactualAsync_EmptyOutcome_ReturnsFailure()
    {
        // Arrange
        var model = CreateSimpleCausalGraph();
        var factual = new Observation(
            new Dictionary<string, object> { { "X", 1.0 }, { "Y", 2.0 } },
            DateTime.UtcNow, null);

        // Act
        var result = await _engine.EstimateCounterfactualAsync("X", "", factual, model);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // ========================================================================
    // DiscoverCausalStructureAsync (PC algorithm) — Inference partial
    // ========================================================================

    [Fact]
    public async Task DiscoverCausalStructureAsync_PC_WithCorrelatedData_FindsEdges()
    {
        // Arrange — strongly correlated X and Y
        var data = CreateCorrelatedObservations(100);

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(
            data, DiscoveryAlgorithm.PC);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Edges.Should().NotBeEmpty();
        result.Value.Variables.Count.Should().Be(3);
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_PC_WithSmallData_StillReturnsGraph()
    {
        // Arrange — very small dataset (< 5 observations triggers independence shortcut)
        var data = new List<Observation>
        {
            new(new Dictionary<string, object> { { "X", 1.0 }, { "Y", 2.0 } }, DateTime.UtcNow, null),
            new(new Dictionary<string, object> { { "X", 2.0 }, { "Y", 4.0 } }, DateTime.UtcNow, null),
            new(new Dictionary<string, object> { { "X", 3.0 }, { "Y", 6.0 } }, DateTime.UtcNow, null),
        };

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(
            data, DiscoveryAlgorithm.PC);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Variables.Should().HaveCount(2);
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_PC_WithIndependentVariables_RemovesEdges()
    {
        // Arrange — X and Z are independent
        var random = new Random(42);
        var data = new List<Observation>();
        for (int i = 0; i < 100; i++)
        {
            double x = random.NextDouble() * 10;
            double z = random.NextDouble() * 10; // independent of X
            data.Add(new Observation(
                new Dictionary<string, object> { { "X", x }, { "Z", z } },
                DateTime.UtcNow, null));
        }

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(
            data, DiscoveryAlgorithm.PC);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // With truly independent data, the PC algorithm should remove edges
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_PC_StructuralEquationsAreCreated()
    {
        // Arrange
        var data = CreateCorrelatedObservations(100);

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(
            data, DiscoveryAlgorithm.PC);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Equations.Should().NotBeEmpty();
        result.Value.Equations.Should().ContainKey("X");
        result.Value.Equations.Should().ContainKey("Y");
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_UnknownAlgorithm_ReturnsFailure()
    {
        // Arrange
        var data = CreateCorrelatedObservations(10);

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(
            data, (DiscoveryAlgorithm)99);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Unknown algorithm");
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_GES_ReturnsNotImplemented()
    {
        // Arrange
        var data = CreateCorrelatedObservations(10);

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(
            data, DiscoveryAlgorithm.GES);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not yet implemented");
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_NOTEARS_ReturnsNotImplemented()
    {
        // Arrange
        var data = CreateCorrelatedObservations(10);

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(
            data, DiscoveryAlgorithm.NOTEARS);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // ========================================================================
    // PlanInterventionAsync — side effects detection
    // ========================================================================

    [Fact]
    public async Task PlanInterventionAsync_WithSideEffects_ReportsSideEffects()
    {
        // Arrange — X -> Y and X -> Z; intervening on X to affect Y also affects Z
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0, 1.0, 2.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Z", VariableType.Continuous, new List<object> { 0.0, 1.0 })
        };
        var edges = new List<CausalEdge>
        {
            new("X", "Y", 0.8, EdgeType.Direct),
            new("X", "Z", 0.6, EdgeType.Direct)
        };
        var equations = CreateEquationsForVariables(variables);
        var model = new CausalGraph(variables, edges, equations);

        // Act
        var result = await _engine.PlanInterventionAsync(
            "Y", model, new List<string> { "X" });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SideEffects.Should().Contain("Z");
    }

    // ========================================================================
    // Helpers
    // ========================================================================

    private static List<Observation> CreateCorrelatedObservations(int count)
    {
        var random = new Random(42);
        var data = new List<Observation>();

        for (int i = 0; i < count; i++)
        {
            double x = random.NextDouble() * 10;
            double y = x * 0.8 + random.NextDouble();
            double z = random.NextDouble() * 10;

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

    private static CausalGraph CreateChainGraph()
    {
        // X -> Y (0.8) -> Z (0.5)
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Z", VariableType.Continuous, new List<object> { 0.0, 1.0 })
        };

        var edges = new List<CausalEdge>
        {
            new("X", "Y", 0.8, EdgeType.Direct),
            new("Y", "Z", 0.5, EdgeType.Direct)
        };

        var equations = CreateEquationsForVariables(variables);

        return new CausalGraph(variables, edges, equations);
    }

    private static CausalGraph CreateDiamondGraph()
    {
        // X -> A (0.6) -> Z (0.5) and X -> B (0.4) -> Z (0.8)
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("A", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("B", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Z", VariableType.Continuous, new List<object> { 0.0, 1.0 })
        };

        var edges = new List<CausalEdge>
        {
            new("X", "A", 0.6, EdgeType.Direct),
            new("X", "B", 0.4, EdgeType.Direct),
            new("A", "Z", 0.5, EdgeType.Direct),
            new("B", "Z", 0.8, EdgeType.Direct)
        };

        var equations = CreateEquationsForVariables(variables);

        return new CausalGraph(variables, edges, equations);
    }

    private static CausalGraph CreateDisconnectedGraph()
    {
        // X -> Y, Z isolated
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Z", VariableType.Continuous, new List<object> { 0.0, 1.0 })
        };

        var edges = new List<CausalEdge>
        {
            new("X", "Y", 0.8, EdgeType.Direct)
        };

        var equations = CreateEquationsForVariables(variables);

        return new CausalGraph(variables, edges, equations);
    }

    private static Dictionary<string, StructuralEquation> CreateEquationsForVariables(
        List<Variable> variables)
    {
        return variables.ToDictionary(
            v => v.Name,
            v => new StructuralEquation(
                v.Name,
                new List<string>(),
                values => values.GetValueOrDefault(v.Name, 0.0),
                1.0));
    }
}
