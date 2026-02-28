// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

using FluentAssertions;
using Ouroboros.Core.Reasoning;
using Xunit;

namespace Ouroboros.Tests.Reasoning;

/// <summary>
/// Complex logic tests for CausalReasoningEngine covering causal graph construction,
/// intervention analysis, counterfactual reasoning, path finding, correlation,
/// and do-calculus operations.
/// </summary>
[Trait("Category", "Unit")]
public class CausalReasoningEngineComplexTests
{
    private readonly CausalReasoningEngine _engine = new();

    // ========================================================================
    // PC Algorithm - Causal Discovery
    // ========================================================================

    [Fact]
    public async Task DiscoverPC_StronglyCorrelated_RetainsEdge()
    {
        // Arrange: X strongly correlates with Y
        var data = GenerateCorrelatedData("X", "Y", correlation: 0.9, sampleCount: 100);

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(data, DiscoveryAlgorithm.PC);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Edges.Should().Contain(e =>
            (e.Cause == "X" && e.Effect == "Y") ||
            (e.Cause == "Y" && e.Effect == "X"));
    }

    [Fact]
    public async Task DiscoverPC_IndependentVariables_RemovesEdge()
    {
        // Arrange: X and Y are independent (correlation near 0)
        var random = new Random(42);
        var data = new List<Observation>();
        for (int i = 0; i < 200; i++)
        {
            data.Add(new Observation(
                new Dictionary<string, object>
                {
                    { "X", random.NextDouble() * 10 },
                    { "Y", random.NextDouble() * 10 } // independent
                },
                DateTime.UtcNow.AddMinutes(-i), null));
        }

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(data, DiscoveryAlgorithm.PC);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Independent variables should have no edge
        result.Value.Edges.Should().NotContain(e =>
            e.Cause == "X" && e.Effect == "Y" && e.Strength > 0.05);
    }

    [Fact]
    public async Task DiscoverPC_ThreeVariables_IdentifiesStructure()
    {
        // Arrange: X -> Y -> Z chain (X causes Y, Y causes Z)
        var random = new Random(42);
        var data = new List<Observation>();
        for (int i = 0; i < 150; i++)
        {
            double x = random.NextDouble() * 10;
            double y = x * 0.8 + random.NextDouble() * 0.5;
            double z = y * 0.6 + random.NextDouble() * 0.5;

            data.Add(new Observation(
                new Dictionary<string, object>
                {
                    { "X", x },
                    { "Y", y },
                    { "Z", z }
                },
                DateTime.UtcNow.AddMinutes(-i), null));
        }

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(data, DiscoveryAlgorithm.PC);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Variables.Should().HaveCount(3);
        result.Value.Equations.Should().HaveCount(3);
    }

    [Fact]
    public async Task DiscoverPC_CreatesVariablesForAllDataColumns()
    {
        // Arrange
        var data = new List<Observation>
        {
            new(new Dictionary<string, object> { { "A", 1.0 }, { "B", 2.0 }, { "C", 3.0 } },
                DateTime.UtcNow, null),
            new(new Dictionary<string, object> { { "A", 4.0 }, { "B", 5.0 }, { "C", 6.0 } },
                DateTime.UtcNow, null)
        };

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(data, DiscoveryAlgorithm.PC);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Variables.Select(v => v.Name).Should().Contain("A");
        result.Value.Variables.Select(v => v.Name).Should().Contain("B");
        result.Value.Variables.Select(v => v.Name).Should().Contain("C");
    }

    [Theory]
    [InlineData(DiscoveryAlgorithm.FCI)]
    [InlineData(DiscoveryAlgorithm.GES)]
    [InlineData(DiscoveryAlgorithm.NOTEARS)]
    [InlineData(DiscoveryAlgorithm.DAGsNoCurl)]
    public async Task DiscoverCausalStructure_UnimplementedAlgorithms_ReturnFailure(
        DiscoveryAlgorithm algorithm)
    {
        // Arrange
        var data = new List<Observation>
        {
            new(new Dictionary<string, object> { { "X", 1.0 } }, DateTime.UtcNow, null)
        };

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(data, algorithm);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not yet implemented");
    }

    // ========================================================================
    // Intervention Effect - Path analysis
    // ========================================================================

    [Fact]
    public async Task EstimateInterventionEffect_DirectPath_EqualsEdgeStrength()
    {
        // Arrange: X -> Y with strength 0.8
        var model = CreateSimpleChain("X", "Y", 0.8);

        // Act
        var result = await _engine.EstimateInterventionEffectAsync("X", "Y", model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeApproximately(0.8, 0.001);
    }

    [Fact]
    public async Task EstimateInterventionEffect_TwoHopPath_MultipliesStrengths()
    {
        // Arrange: X -> M -> Y with strengths 0.8 and 0.5
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0 }),
            new("M", VariableType.Continuous, new List<object> { 0.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0 })
        };
        var edges = new List<CausalEdge>
        {
            new("X", "M", 0.8, EdgeType.Direct),
            new("M", "Y", 0.5, EdgeType.Direct)
        };
        var equations = variables.ToDictionary(
            v => v.Name,
            v => new StructuralEquation(v.Name, new List<string>(),
                values => values.GetValueOrDefault(v.Name, 0.0), 1.0));
        var model = new CausalGraph(variables, edges, equations);

        // Act
        var result = await _engine.EstimateInterventionEffectAsync("X", "Y", model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeApproximately(0.4, 0.001); // 0.8 * 0.5
    }

    [Fact]
    public async Task EstimateInterventionEffect_MultipleParallelPaths_SumsEffects()
    {
        // Arrange: X -> Y (0.3) and X -> M -> Y (0.4 * 0.5 = 0.2)
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0 }),
            new("M", VariableType.Continuous, new List<object> { 0.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0 })
        };
        var edges = new List<CausalEdge>
        {
            new("X", "Y", 0.3, EdgeType.Direct),
            new("X", "M", 0.4, EdgeType.Direct),
            new("M", "Y", 0.5, EdgeType.Direct)
        };
        var equations = variables.ToDictionary(
            v => v.Name,
            v => new StructuralEquation(v.Name, new List<string>(),
                values => values.GetValueOrDefault(v.Name, 0.0), 1.0));
        var model = new CausalGraph(variables, edges, equations);

        // Act
        var result = await _engine.EstimateInterventionEffectAsync("X", "Y", model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Total = direct (0.3) + indirect (0.4 * 0.5 = 0.2) = 0.5
        result.Value.Should().BeApproximately(0.5, 0.001);
    }

    [Fact]
    public async Task EstimateInterventionEffect_NoPath_ReturnsZero()
    {
        // Arrange: X and Y are disconnected
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0 })
        };
        var model = new CausalGraph(
            variables,
            new List<CausalEdge>(), // no edges
            variables.ToDictionary(
                v => v.Name,
                v => new StructuralEquation(v.Name, new List<string>(),
                    values => values.GetValueOrDefault(v.Name, 0.0), 1.0)));

        // Act
        var result = await _engine.EstimateInterventionEffectAsync("X", "Y", model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0.0);
    }

    // ========================================================================
    // Counterfactual reasoning
    // ========================================================================

    [Fact]
    public async Task EstimateCounterfactual_WithStructuralEquation_PropagatesEffects()
    {
        // Arrange
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0, 1.0 })
        };
        var edges = new List<CausalEdge> { new("X", "Y", 0.8, EdgeType.Direct) };
        var equations = new Dictionary<string, StructuralEquation>
        {
            ["X"] = new("X", new List<string>(),
                values => values.GetValueOrDefault("X", 0.0), 1.0),
            ["Y"] = new("Y", new List<string> { "X" },
                values => Convert.ToDouble(values.GetValueOrDefault("X", 0.0)) * 2.0, 0.5)
        };
        var model = new CausalGraph(variables, edges, equations);

        var factual = new Observation(
            new Dictionary<string, object> { { "X", 1.0 }, { "Y", 2.0 } },
            DateTime.UtcNow, null);

        // Act
        var result = await _engine.EstimateCounterfactualAsync("X", "Y", factual, model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(DistributionType.Empirical);
    }

    [Fact]
    public async Task EstimateCounterfactual_ResultDistribution_HasValidProperties()
    {
        // Arrange
        var model = CreateSimpleChain("X", "Y", 0.8);
        var factual = new Observation(
            new Dictionary<string, object> { { "X", 1.0 }, { "Y", 0.8 } },
            DateTime.UtcNow, null);

        // Act
        var result = await _engine.EstimateCounterfactualAsync("X", "Y", factual, model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Samples.Should().NotBeEmpty();
        result.Value.Probabilities.Should().NotBeEmpty();
    }

    // ========================================================================
    // Causal explanation
    // ========================================================================

    [Fact]
    public async Task ExplainCausally_DirectCause_HighAttribution()
    {
        // Arrange
        var model = CreateSimpleChain("X", "Y", 0.9);

        // Act
        var result = await _engine.ExplainCausallyAsync(
            "Y", new List<string> { "X" }, model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Attributions.Should().ContainKey("X");
        result.Value.Attributions["X"].Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExplainCausally_MultipleCauses_NormalizesAttributions()
    {
        // Arrange - X and Z both cause Y
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0 }),
            new("Z", VariableType.Continuous, new List<object> { 0.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0 })
        };
        var edges = new List<CausalEdge>
        {
            new("X", "Y", 0.6, EdgeType.Direct),
            new("Z", "Y", 0.4, EdgeType.Direct)
        };
        var equations = variables.ToDictionary(
            v => v.Name,
            v => new StructuralEquation(v.Name, new List<string>(),
                values => values.GetValueOrDefault(v.Name, 0.0), 1.0));
        var model = new CausalGraph(variables, edges, equations);

        // Act
        var result = await _engine.ExplainCausallyAsync(
            "Y", new List<string> { "X", "Z" }, model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        double totalAttribution = result.Value.Attributions.Values.Sum();
        totalAttribution.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public async Task ExplainCausally_NarrativeExplanation_ContainsCauseNames()
    {
        // Arrange
        var model = CreateSimpleChain("Smoking", "Cancer", 0.7);

        // Act
        var result = await _engine.ExplainCausallyAsync(
            "Cancer", new List<string> { "Smoking" }, model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.NarrativeExplanation.Should().Contain("Cancer");
        result.Value.NarrativeExplanation.Should().Contain("Smoking");
    }

    [Fact]
    public async Task ExplainCausally_DirectPath_NarrativeMentionsDirectPaths()
    {
        // Arrange
        var model = CreateSimpleChain("X", "Y", 0.8);

        // Act
        var result = await _engine.ExplainCausallyAsync(
            "Y", new List<string> { "X" }, model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.NarrativeExplanation.Should().Contain("Direct causal paths");
    }

    [Fact]
    public async Task ExplainCausally_IndirectPath_NarrativeMentionsMediated()
    {
        // Arrange: X -> M -> Y (indirect)
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0 }),
            new("M", VariableType.Continuous, new List<object> { 0.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0 })
        };
        var edges = new List<CausalEdge>
        {
            new("X", "M", 0.8, EdgeType.Direct),
            new("M", "Y", 0.5, EdgeType.Direct)
        };
        var equations = variables.ToDictionary(
            v => v.Name,
            v => new StructuralEquation(v.Name, new List<string>(),
                values => values.GetValueOrDefault(v.Name, 0.0), 1.0));
        var model = new CausalGraph(variables, edges, equations);

        // Act
        var result = await _engine.ExplainCausallyAsync(
            "Y", new List<string> { "X" }, model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.NarrativeExplanation.Should().Contain("mediated");
    }

    // ========================================================================
    // Intervention planning
    // ========================================================================

    [Fact]
    public async Task PlanIntervention_PicksStrongestControllable()
    {
        // Arrange
        var variables = new List<Variable>
        {
            new("A", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("B", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0 })
        };
        var edges = new List<CausalEdge>
        {
            new("A", "Y", 0.3, EdgeType.Direct),
            new("B", "Y", 0.9, EdgeType.Direct)
        };
        var equations = variables.ToDictionary(
            v => v.Name,
            v => new StructuralEquation(v.Name, new List<string>(),
                values => values.GetValueOrDefault(v.Name, 0.0), 1.0));
        var model = new CausalGraph(variables, edges, equations);

        // Act
        var result = await _engine.PlanInterventionAsync(
            "Y", model, new List<string> { "A", "B" });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TargetVariable.Should().Be("B"); // B has stronger effect
    }

    [Fact]
    public async Task PlanIntervention_NoControllablePathToOutcome_ReturnsFailure()
    {
        // Arrange
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0 })
        };
        var model = new CausalGraph(
            variables,
            new List<CausalEdge>(), // no edges
            variables.ToDictionary(
                v => v.Name,
                v => new StructuralEquation(v.Name, new List<string>(),
                    values => values.GetValueOrDefault(v.Name, 0.0), 1.0)));

        // Act
        var result = await _engine.PlanInterventionAsync(
            "Y", model, new List<string> { "X" });

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task PlanIntervention_FindsSideEffects()
    {
        // Arrange: X -> Y and X -> Z (Z is a side effect)
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0 }),
            new("Z", VariableType.Continuous, new List<object> { 0.0 })
        };
        var edges = new List<CausalEdge>
        {
            new("X", "Y", 0.8, EdgeType.Direct),
            new("X", "Z", 0.5, EdgeType.Direct)
        };
        var equations = variables.ToDictionary(
            v => v.Name,
            v => new StructuralEquation(v.Name, new List<string>(),
                values => values.GetValueOrDefault(v.Name, 0.0), 1.0));
        var model = new CausalGraph(variables, edges, equations);

        // Act
        var result = await _engine.PlanInterventionAsync(
            "Y", model, new List<string> { "X" });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SideEffects.Should().Contain("Z");
    }

    // ========================================================================
    // Variable type inference
    // ========================================================================

    [Fact]
    public async Task DiscoverPC_BinaryData_InfersBinaryType()
    {
        // Arrange
        var data = new List<Observation>();
        for (int i = 0; i < 50; i++)
        {
            data.Add(new Observation(
                new Dictionary<string, object>
                {
                    { "Treatment", i % 2 },
                    { "Outcome", i % 3 == 0 ? 1 : 0 }
                },
                DateTime.UtcNow.AddMinutes(-i), null));
        }

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(data, DiscoveryAlgorithm.PC);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Variables.Should().Contain(v => v.Type == VariableType.Binary);
    }

    [Fact]
    public async Task DiscoverPC_ContinuousData_InfersContinuousType()
    {
        // Arrange
        var random = new Random(42);
        var data = new List<Observation>();
        for (int i = 0; i < 50; i++)
        {
            data.Add(new Observation(
                new Dictionary<string, object>
                {
                    { "Temperature", random.NextDouble() * 100 },
                    { "Pressure", random.NextDouble() * 50 }
                },
                DateTime.UtcNow.AddMinutes(-i), null));
        }

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(data, DiscoveryAlgorithm.PC);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Variables.Should().OnlyContain(v => v.Type == VariableType.Continuous);
    }

    // ========================================================================
    // Edge cases and cancellation
    // ========================================================================

    [Fact]
    public async Task DiscoverPC_SingleObservation_StillWorks()
    {
        // Arrange - minimum data
        var data = new List<Observation>
        {
            new(new Dictionary<string, object> { { "X", 1.0 }, { "Y", 2.0 } },
                DateTime.UtcNow, null)
        };

        // Act
        var result = await _engine.DiscoverCausalStructureAsync(data, DiscoveryAlgorithm.PC);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EstimateInterventionEffect_CyclePrevention_DoesNotInfiniteLoop()
    {
        // Arrange - model with no actual cycles (DAG), but test path finding terminates
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0 }),
            new("Z", VariableType.Continuous, new List<object> { 0.0 })
        };
        var edges = new List<CausalEdge>
        {
            new("X", "Y", 0.5, EdgeType.Direct),
            new("Y", "Z", 0.5, EdgeType.Direct),
            new("X", "Z", 0.3, EdgeType.Direct)
        };
        var equations = variables.ToDictionary(
            v => v.Name,
            v => new StructuralEquation(v.Name, new List<string>(),
                values => values.GetValueOrDefault(v.Name, 0.0), 1.0));
        var model = new CausalGraph(variables, edges, equations);

        // Act - should find all paths without infinite loop
        var result = await _engine.EstimateInterventionEffectAsync("X", "Z", model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Direct (0.3) + Indirect via Y (0.5 * 0.5 = 0.25) = 0.55
        result.Value.Should().BeApproximately(0.55, 0.001);
    }

    // ========================================================================
    // Helpers
    // ========================================================================

    private static CausalGraph CreateSimpleChain(string cause, string effect, double strength)
    {
        var variables = new List<Variable>
        {
            new(cause, VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new(effect, VariableType.Continuous, new List<object> { 0.0, 1.0 })
        };
        var edges = new List<CausalEdge>
        {
            new(cause, effect, strength, EdgeType.Direct)
        };
        var equations = new Dictionary<string, StructuralEquation>
        {
            [cause] = new(cause, new List<string>(),
                values => values.GetValueOrDefault(cause, 0.0), 1.0),
            [effect] = new(effect, new List<string> { cause },
                values =>
                {
                    var x = Convert.ToDouble(values.GetValueOrDefault(cause, 0.0));
                    return x * strength;
                }, 0.5)
        };
        return new CausalGraph(variables, edges, equations);
    }

    private static List<Observation> GenerateCorrelatedData(
        string var1, string var2, double correlation, int sampleCount)
    {
        var random = new Random(42);
        var data = new List<Observation>();

        for (int i = 0; i < sampleCount; i++)
        {
            double x = random.NextDouble() * 10;
            double y = x * correlation + random.NextDouble() * (1 - Math.Abs(correlation));

            data.Add(new Observation(
                new Dictionary<string, object> { { var1, x }, { var2, y } },
                DateTime.UtcNow.AddMinutes(-i), null));
        }

        return data;
    }
}
