using Ouroboros.Core.Reasoning;

namespace Ouroboros.Core.Tests.Reasoning;

[Trait("Category", "Unit")]
[Trait("Category", "Reasoning")]
public class CausalMeTTaIntegrationTests
{
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
        var equations = new Dictionary<string, StructuralEquation>();
        return new CausalGraph(variables, edges, equations);
    }

    [Fact]
    public void ConvertToMeTTa_NullGraph_ReturnsFailure()
    {
        var result = CausalMeTTaIntegration.ConvertToMeTTa(null!);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ConvertToMeTTa_ValidGraph_ReturnsSuccess()
    {
        var graph = CreateSimpleGraph();

        var result = CausalMeTTaIntegration.ConvertToMeTTa(graph);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("causal-space");
        result.Value.Should().Contain("(variable X)");
        result.Value.Should().Contain("(variable Y)");
        result.Value.Should().Contain("causes");
    }

    [Fact]
    public void ConvertToMeTTa_ContainsDSeparationRules()
    {
        var graph = CreateSimpleGraph();

        var result = CausalMeTTaIntegration.ConvertToMeTTa(graph);

        result.Value.Should().Contain("d-separated");
    }

    [Fact]
    public void ConvertToMeTTa_ContainsInterventionRules()
    {
        var graph = CreateSimpleGraph();

        var result = CausalMeTTaIntegration.ConvertToMeTTa(graph);

        result.Value.Should().Contain("do-intervention");
    }

    [Fact]
    public void ConvertToMeTTa_ContainsCounterfactualRules()
    {
        var graph = CreateSimpleGraph();

        var result = CausalMeTTaIntegration.ConvertToMeTTa(graph);

        result.Value.Should().Contain("counterfactual");
    }

    [Fact]
    public void EdgeToMeTTaAtom_ReturnsCorrectFormat()
    {
        var edge = new CausalEdge("X", "Y", 0.8, EdgeType.Direct);

        var atom = CausalMeTTaIntegration.EdgeToMeTTaAtom(edge);

        atom.Should().Contain("causes");
        atom.Should().Contain("X");
        atom.Should().Contain("Y");
        atom.Should().Contain("0.8");
    }

    [Fact]
    public void VariableToMeTTaAtom_ReturnsCorrectFormat()
    {
        var variable = new Variable("X", VariableType.Continuous, new List<object> { 0.0 });

        var atom = CausalMeTTaIntegration.VariableToMeTTaAtom(variable);

        atom.Should().Contain("variable");
        atom.Should().Contain("X");
        atom.Should().Contain("continuous");
    }

    [Fact]
    public void GenerateDSeparationQuery_ReturnsQueryString()
    {
        var query = CausalMeTTaIntegration.GenerateDSeparationQuery("X", "Y", new List<string> { "Z" });

        query.Should().Contain("d-separated");
        query.Should().Contain("X");
        query.Should().Contain("Y");
        query.Should().Contain("Z");
    }

    [Fact]
    public void GenerateInterventionQuery_ReturnsQueryString()
    {
        var query = CausalMeTTaIntegration.GenerateInterventionQuery("X", "Y");

        query.Should().Contain("intervention-effect");
        query.Should().Contain("X");
        query.Should().Contain("Y");
    }

    [Fact]
    public void GenerateCounterfactualQuery_ReturnsQueryString()
    {
        var factual = new Observation(new Dictionary<string, object> { ["X"] = 1.0 }, DateTime.UtcNow, null);

        var query = CausalMeTTaIntegration.GenerateCounterfactualQuery("X", "Y", factual);

        query.Should().Contain("counterfactual");
        query.Should().Contain("Y");
        query.Should().Contain("do");
    }

    [Fact]
    public void GeneratePathFindingRules_ReturnsPathRules()
    {
        var graph = CreateSimpleGraph();

        var rules = CausalMeTTaIntegration.GeneratePathFindingRules(graph);

        rules.Should().Contain("path");
        rules.Should().Contain("causes");
    }

    [Fact]
    public void GenerateEffectComputationRules_ReturnsEffectRules()
    {
        var graph = CreateSimpleGraph();

        var rules = CausalMeTTaIntegration.GenerateEffectComputationRules(graph);

        rules.Should().Contain("total-effect");
        rules.Should().Contain("direct-effect");
        rules.Should().Contain("path-effect");
    }

    [Fact]
    public void ExplanationToMeTTa_NullExplanation_ReturnsFailure()
    {
        var result = CausalMeTTaIntegration.ExplanationToMeTTa(null!);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ExplanationToMeTTa_ValidExplanation_ReturnsSuccess()
    {
        var paths = new List<CausalPath>
        {
            new(new List<string> { "X", "Y" }, 0.8, true, new List<CausalEdge>
            {
                new("X", "Y", 0.8, EdgeType.Direct)
            })
        };
        var attributions = new Dictionary<string, double> { ["X"] = 1.0 };
        var explanation = new Explanation("Y", paths, attributions, "X causes Y");

        var result = CausalMeTTaIntegration.ExplanationToMeTTa(explanation);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("explanation");
        result.Value.Should().Contain("attributions");
        result.Value.Should().Contain("causal-paths");
    }

    [Fact]
    public void GenerateInterventionPlanningRules_ReturnsRules()
    {
        var rules = CausalMeTTaIntegration.GenerateInterventionPlanningRules();

        rules.Should().Contain("best-intervention");
        rules.Should().Contain("intervention-candidate");
        rules.Should().Contain("side-effects");
    }
}
