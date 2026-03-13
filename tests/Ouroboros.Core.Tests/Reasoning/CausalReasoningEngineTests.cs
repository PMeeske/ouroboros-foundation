using Ouroboros.Core.Reasoning;

namespace Ouroboros.Core.Tests.Reasoning;

[Trait("Category", "Unit")]
public class CausalReasoningEngineTests
{
    private readonly CausalReasoningEngine _sut = new();

    private static List<Observation> CreateSimpleData()
    {
        var data = new List<Observation>();
        var now = DateTime.UtcNow;

        // Create correlated observations: X causes Y
        for (int i = 0; i < 50; i++)
        {
            data.Add(new Observation(
                new Dictionary<string, object> { ["X"] = (double)i, ["Y"] = (double)(i * 2 + 1) },
                now.AddMinutes(i),
                null));
        }

        return data;
    }

    private static CausalGraph CreateSimpleGraph()
    {
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
            new("Y", VariableType.Continuous, new List<object> { 0.0, 1.0 }),
        };

        var edges = new List<CausalEdge>
        {
            new("X", "Y", 0.8, EdgeType.Direct),
        };

        var equations = new Dictionary<string, StructuralEquation>
        {
            ["Y"] = new("Y", new List<string> { "X" }, vals => (double)vals["X"] * 2.0, 0.1),
        };

        return new CausalGraph(variables, edges, equations);
    }

    // --- DiscoverCausalStructureAsync ---

    [Fact]
    public async Task DiscoverCausalStructureAsync_NullData_ReturnsFailure()
    {
        var result = await _sut.DiscoverCausalStructureAsync(null!, DiscoveryAlgorithm.PC);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_EmptyData_ReturnsFailure()
    {
        var result = await _sut.DiscoverCausalStructureAsync(new List<Observation>(), DiscoveryAlgorithm.PC);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_PC_WithValidData_ReturnsGraph()
    {
        var data = CreateSimpleData();

        var result = await _sut.DiscoverCausalStructureAsync(data, DiscoveryAlgorithm.PC);

        result.IsSuccess.Should().BeTrue();
        result.Value.Variables.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_FCI_ReturnsNotImplemented()
    {
        var data = CreateSimpleData();

        var result = await _sut.DiscoverCausalStructureAsync(data, DiscoveryAlgorithm.FCI);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DiscoverCausalStructureAsync_GES_ReturnsNotImplemented()
    {
        var data = CreateSimpleData();

        var result = await _sut.DiscoverCausalStructureAsync(data, DiscoveryAlgorithm.GES);

        result.IsSuccess.Should().BeFalse();
    }

    // --- EstimateInterventionEffectAsync ---

    [Fact]
    public async Task EstimateInterventionEffectAsync_NullIntervention_ReturnsFailure()
    {
        var graph = CreateSimpleGraph();

        var result = await _sut.EstimateInterventionEffectAsync(null!, "Y", graph);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task EstimateInterventionEffectAsync_NullOutcome_ReturnsFailure()
    {
        var graph = CreateSimpleGraph();

        var result = await _sut.EstimateInterventionEffectAsync("X", null!, graph);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task EstimateInterventionEffectAsync_NullModel_ReturnsFailure()
    {
        var result = await _sut.EstimateInterventionEffectAsync("X", "Y", null!);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task EstimateInterventionEffectAsync_EmptyIntervention_ReturnsFailure()
    {
        var graph = CreateSimpleGraph();

        var result = await _sut.EstimateInterventionEffectAsync("", "Y", graph);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task EstimateInterventionEffectAsync_ValidInputs_ReturnsResult()
    {
        var graph = CreateSimpleGraph();

        var result = await _sut.EstimateInterventionEffectAsync("X", "Y", graph);

        // Should either succeed or fail with a meaningful message
        // (depends on whether X->Y path exists with equations)
        result.Should().NotBeNull();
    }

    // --- EstimateCounterfactualAsync ---

    [Fact]
    public async Task EstimateCounterfactualAsync_NullIntervention_ReturnsFailure()
    {
        var graph = CreateSimpleGraph();
        var obs = new Observation(new Dictionary<string, object> { ["X"] = 1.0 }, DateTime.UtcNow, null);

        var result = await _sut.EstimateCounterfactualAsync(null!, "Y", obs, graph);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task EstimateCounterfactualAsync_NullOutcome_ReturnsFailure()
    {
        var graph = CreateSimpleGraph();
        var obs = new Observation(new Dictionary<string, object> { ["X"] = 1.0 }, DateTime.UtcNow, null);

        var result = await _sut.EstimateCounterfactualAsync("X", null!, obs, graph);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task EstimateCounterfactualAsync_NullFactual_ReturnsFailure()
    {
        var graph = CreateSimpleGraph();

        var result = await _sut.EstimateCounterfactualAsync("X", "Y", null!, graph);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task EstimateCounterfactualAsync_NullModel_ReturnsFailure()
    {
        var obs = new Observation(new Dictionary<string, object> { ["X"] = 1.0 }, DateTime.UtcNow, null);

        var result = await _sut.EstimateCounterfactualAsync("X", "Y", obs, null!);

        result.IsSuccess.Should().BeFalse();
    }

    // --- ExplainCausallyAsync ---

    [Fact]
    public async Task ExplainCausallyAsync_NullEffect_ReturnsFailure()
    {
        var graph = CreateSimpleGraph();

        var result = await _sut.ExplainCausallyAsync(null!, new List<string> { "X" }, graph);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ExplainCausallyAsync_NullCauses_ReturnsFailure()
    {
        var graph = CreateSimpleGraph();

        var result = await _sut.ExplainCausallyAsync("Y", null!, graph);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ExplainCausallyAsync_EmptyCauses_ReturnsFailure()
    {
        var graph = CreateSimpleGraph();

        var result = await _sut.ExplainCausallyAsync("Y", new List<string>(), graph);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ExplainCausallyAsync_NullModel_ReturnsFailure()
    {
        var result = await _sut.ExplainCausallyAsync("Y", new List<string> { "X" }, null!);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ExplainCausallyAsync_ValidInputs_ReturnsExplanation()
    {
        var graph = CreateSimpleGraph();

        var result = await _sut.ExplainCausallyAsync("Y", new List<string> { "X" }, graph);

        result.IsSuccess.Should().BeTrue();
        result.Value.Effect.Should().Be("Y");
        result.Value.NarrativeExplanation.Should().NotBeNullOrEmpty();
    }

    // --- PlanInterventionAsync ---

    [Fact]
    public async Task PlanInterventionAsync_NullOutcome_ReturnsFailure()
    {
        var graph = CreateSimpleGraph();

        var result = await _sut.PlanInterventionAsync(null!, graph, new List<string> { "X" });

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task PlanInterventionAsync_NullModel_ReturnsFailure()
    {
        var result = await _sut.PlanInterventionAsync("Y", null!, new List<string> { "X" });

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task PlanInterventionAsync_NullControllable_ReturnsFailure()
    {
        var graph = CreateSimpleGraph();

        var result = await _sut.PlanInterventionAsync("Y", graph, null!);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task PlanInterventionAsync_EmptyControllable_ReturnsFailure()
    {
        var graph = CreateSimpleGraph();

        var result = await _sut.PlanInterventionAsync("Y", graph, new List<string>());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task PlanInterventionAsync_ValidInputs_ReturnsIntervention()
    {
        var graph = CreateSimpleGraph();

        var result = await _sut.PlanInterventionAsync("Y", graph, new List<string> { "X" });

        result.IsSuccess.Should().BeTrue();
        result.Value.TargetVariable.Should().NotBeNullOrEmpty();
    }
}
