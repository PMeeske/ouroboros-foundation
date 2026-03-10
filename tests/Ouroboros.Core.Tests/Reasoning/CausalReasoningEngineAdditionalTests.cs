using Ouroboros.Core.Reasoning;

namespace Ouroboros.Core.Tests.Reasoning;

/// <summary>
/// Additional tests for CausalReasoningEngine covering graph traversal,
/// inference, counterfactual estimation, and intervention planning.
/// </summary>
[Trait("Category", "Unit")]
public class CausalReasoningEngineAdditionalTests
{
    private readonly CausalReasoningEngine _engine = new();

    // --- EstimateInterventionEffectAsync ---

    [Fact]
    public async Task EstimateInterventionEffectAsync_NullIntervention_ReturnsFailure()
    {
        var model = CreateSimpleGraph();
        var result = await _engine.EstimateInterventionEffectAsync(null!, "Y", model);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EstimateInterventionEffectAsync_EmptyIntervention_ReturnsFailure()
    {
        var model = CreateSimpleGraph();
        var result = await _engine.EstimateInterventionEffectAsync("", "Y", model);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EstimateInterventionEffectAsync_NullOutcome_ReturnsFailure()
    {
        var model = CreateSimpleGraph();
        var result = await _engine.EstimateInterventionEffectAsync("X", null!, model);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EstimateInterventionEffectAsync_NullModel_ReturnsFailure()
    {
        var result = await _engine.EstimateInterventionEffectAsync("X", "Y", null!);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EstimateInterventionEffectAsync_NoPath_ReturnsZero()
    {
        var model = CreateDisconnectedGraph();
        var result = await _engine.EstimateInterventionEffectAsync("X", "Z", model);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0.0);
    }

    [Fact]
    public async Task EstimateInterventionEffectAsync_WithPath_ReturnsEffect()
    {
        var model = CreateSimpleGraph();
        var result = await _engine.EstimateInterventionEffectAsync("X", "Y", model);

        result.IsSuccess.Should().BeTrue();
    }

    // --- EstimateCounterfactualAsync ---

    [Fact]
    public async Task EstimateCounterfactualAsync_NullIntervention_ReturnsFailure()
    {
        var model = CreateSimpleGraph();
        var factual = new Observation(
            new Dictionary<string, object> { ["X"] = 1.0, ["Y"] = 2.0 },
            DateTime.UtcNow, null);

        var result = await _engine.EstimateCounterfactualAsync(null!, "Y", factual, model);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EstimateCounterfactualAsync_NullFactual_ReturnsFailure()
    {
        var model = CreateSimpleGraph();
        var result = await _engine.EstimateCounterfactualAsync("X", "Y", null!, model);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EstimateCounterfactualAsync_NullModel_ReturnsFailure()
    {
        var factual = new Observation(
            new Dictionary<string, object> { ["X"] = 1.0 },
            DateTime.UtcNow, null);

        var result = await _engine.EstimateCounterfactualAsync("X", "Y", factual, null!);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EstimateCounterfactualAsync_ValidInput_ReturnsDistribution()
    {
        var model = CreateSimpleGraph();
        var factual = new Observation(
            new Dictionary<string, object> { ["X"] = 1.0, ["Y"] = 2.0 },
            DateTime.UtcNow, null);

        var result = await _engine.EstimateCounterfactualAsync("X", "Y", factual, model);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    // --- ExplainCausallyAsync ---

    [Fact]
    public async Task ExplainCausallyAsync_NullEffect_ReturnsFailure()
    {
        var model = CreateSimpleGraph();
        var result = await _engine.ExplainCausallyAsync(
            null!, new List<string> { "X" }, model);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExplainCausallyAsync_NullCauses_ReturnsFailure()
    {
        var model = CreateSimpleGraph();
        var result = await _engine.ExplainCausallyAsync("Y", null!, model);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExplainCausallyAsync_NullModel_ReturnsFailure()
    {
        var result = await _engine.ExplainCausallyAsync(
            "Y", new List<string> { "X" }, null!);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExplainCausallyAsync_ValidInput_ReturnsExplanation()
    {
        var model = CreateSimpleGraph();
        var result = await _engine.ExplainCausallyAsync(
            "Y", new List<string> { "X" }, model);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Effect.Should().Be("Y");
    }

    // --- PlanInterventionAsync ---

    [Fact]
    public async Task PlanInterventionAsync_NullOutcome_ReturnsFailure()
    {
        var model = CreateSimpleGraph();
        var result = await _engine.PlanInterventionAsync(
            null!, model, new List<string> { "X" });

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task PlanInterventionAsync_NullModel_ReturnsFailure()
    {
        var result = await _engine.PlanInterventionAsync(
            "Y", null!, new List<string> { "X" });

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task PlanInterventionAsync_NullVariables_ReturnsFailure()
    {
        var model = CreateSimpleGraph();
        var result = await _engine.PlanInterventionAsync("Y", model, null!);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task PlanInterventionAsync_NoEffectivePath_ReturnsFailure()
    {
        var model = CreateDisconnectedGraph();
        var result = await _engine.PlanInterventionAsync(
            "Z", model, new List<string> { "X" });

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task PlanInterventionAsync_ValidInput_ReturnsIntervention()
    {
        var model = CreateSimpleGraph();
        var result = await _engine.PlanInterventionAsync(
            "Y", model, new List<string> { "X" });

        result.IsSuccess.Should().BeTrue();
        result.Value.TargetVariable.Should().NotBeNullOrEmpty();
    }

    // --- DiscoverCausalStructureAsync with various algorithms ---

    [Fact]
    public async Task DiscoverCausalStructureAsync_FCI_ReturnsNotImplemented()
    {
        var data = CreateSampleData();
        var result = await _engine.DiscoverCausalStructureAsync(data, DiscoveryAlgorithm.FCI);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not yet implemented");
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_GES_ReturnsNotImplemented()
    {
        var data = CreateSampleData();
        var result = await _engine.DiscoverCausalStructureAsync(data, DiscoveryAlgorithm.GES);

        result.IsFailure.Should().BeTrue();
    }

    private static CausalGraph CreateSimpleGraph()
    {
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0, 1.0 })
        };

        var edges = new List<CausalEdge>
        {
            new("X", "Y", 0.8, EdgeType.Direct)
        };

        var equations = new Dictionary<string, StructuralEquation>
        {
            ["X"] = new StructuralEquation("X", new List<string>(), _ => 0.0, 1.0),
            ["Y"] = new StructuralEquation("Y", new List<string> { "X" },
                vals => vals.TryGetValue("X", out var v) ? Convert.ToDouble(v) * 0.8 : 0.0, 1.0)
        };

        return new CausalGraph(variables, edges, equations);
    }

    private static CausalGraph CreateDisconnectedGraph()
    {
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Z", VariableType.Continuous, new List<object> { 0.0, 1.0 })
        };

        return new CausalGraph(
            variables,
            new List<CausalEdge>(),
            new Dictionary<string, StructuralEquation>
            {
                ["X"] = new StructuralEquation("X", new List<string>(), _ => 0.0, 1.0),
                ["Z"] = new StructuralEquation("Z", new List<string>(), _ => 0.0, 1.0)
            });
    }

    private static List<Observation> CreateSampleData()
    {
        var data = new List<Observation>();
        var rng = new Random(42);
        for (int i = 0; i < 50; i++)
        {
            double x = rng.NextDouble();
            double y = x * 0.8 + rng.NextDouble() * 0.2;
            data.Add(new Observation(
                new Dictionary<string, object> { ["X"] = x, ["Y"] = y },
                DateTime.UtcNow, null));
        }
        return data;
    }
}
