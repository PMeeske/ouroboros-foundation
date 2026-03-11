using Ouroboros.Core.Reasoning;

namespace Ouroboros.Core.Tests.Reasoning;

[Trait("Category", "Unit")]
[Trait("Category", "Reasoning")]
public class CausalMeTTaIntegrationAdditionalTests
{
    // ========================================================================
    // ConvertToMeTTa - detailed output validation
    // ========================================================================

    [Fact]
    public void ConvertToMeTTa_MultipleVariablesAndEdges_IncludesAll()
    {
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, new List<object> { 0.0 }),
            new("Y", VariableType.Categorical, new List<object> { "a", "b" }),
            new("Z", VariableType.Binary, new List<object> { 0, 1 })
        };
        var edges = new List<CausalEdge>
        {
            new("X", "Y", 0.7, EdgeType.Direct),
            new("Y", "Z", 0.5, EdgeType.Mediated)
        };
        var graph = new CausalGraph(variables, edges, new Dictionary<string, StructuralEquation>());

        var result = CausalMeTTaIntegration.ConvertToMeTTa(graph);

        result.IsSuccess.Should().BeTrue();

        // All variables should be present
        result.Value.Should().Contain("(variable X)");
        result.Value.Should().Contain("(variable Y)");
        result.Value.Should().Contain("(variable Z)");

        // Variable types
        result.Value.Should().Contain("continuous-variable");
        result.Value.Should().Contain("categorical");
        result.Value.Should().Contain("binary-variable");

        // All edges
        result.Value.Should().Contain("causes X Y");
        result.Value.Should().Contain("causes Y Z");
    }

    [Fact]
    public void ConvertToMeTTa_EmptyGraph_ReturnsSuccessWithRules()
    {
        var graph = new CausalGraph(
            new List<Variable>(),
            new List<CausalEdge>(),
            new Dictionary<string, StructuralEquation>());

        var result = CausalMeTTaIntegration.ConvertToMeTTa(graph);

        result.IsSuccess.Should().BeTrue();
        // Should still contain the standard rules even with no variables/edges
        result.Value.Should().Contain("d-separated");
        result.Value.Should().Contain("do-intervention");
        result.Value.Should().Contain("counterfactual");
    }

    // ========================================================================
    // EdgeToMeTTaAtom - edge types
    // ========================================================================

    [Theory]
    [InlineData(EdgeType.Direct, "direct")]
    [InlineData(EdgeType.Mediated, "mediated")]
    public void EdgeToMeTTaAtom_DifferentEdgeTypes_IncludesTypeString(EdgeType edgeType, string expectedStr)
    {
        var edge = new CausalEdge("A", "B", 0.5, edgeType);

        var atom = CausalMeTTaIntegration.EdgeToMeTTaAtom(edge);

        atom.Should().Contain(expectedStr);
        atom.Should().Contain("causes A B");
    }

    [Fact]
    public void EdgeToMeTTaAtom_UsesInvariantCultureForStrength()
    {
        var edge = new CausalEdge("X", "Y", 0.123456, EdgeType.Direct);

        var atom = CausalMeTTaIntegration.EdgeToMeTTaAtom(edge);

        // Should use "." as decimal separator regardless of culture
        atom.Should().Contain("0.123456");
    }

    // ========================================================================
    // VariableToMeTTaAtom - variable types
    // ========================================================================

    [Theory]
    [InlineData(VariableType.Continuous, "continuous")]
    [InlineData(VariableType.Categorical, "categorical")]
    [InlineData(VariableType.Binary, "binary")]
    public void VariableToMeTTaAtom_DifferentTypes_IncludesTypeString(VariableType varType, string expectedStr)
    {
        var variable = new Variable("X", varType, new List<object> { 0.0 });

        var atom = CausalMeTTaIntegration.VariableToMeTTaAtom(variable);

        atom.Should().Contain("variable X");
        atom.Should().Contain(expectedStr);
    }

    // ========================================================================
    // GenerateDSeparationQuery - formatting
    // ========================================================================

    [Fact]
    public void GenerateDSeparationQuery_EmptyConditioningSet_HasEmptyParens()
    {
        var query = CausalMeTTaIntegration.GenerateDSeparationQuery("X", "Y", new List<string>());

        query.Should().Contain("d-separated X Y ()");
    }

    [Fact]
    public void GenerateDSeparationQuery_MultipleConditions_JoinsWithSpace()
    {
        var query = CausalMeTTaIntegration.GenerateDSeparationQuery(
            "X", "Y", new List<string> { "A", "B", "C" });

        query.Should().Contain("(A B C)");
    }

    // ========================================================================
    // GenerateInterventionQuery - format
    // ========================================================================

    [Fact]
    public void GenerateInterventionQuery_IncludesQueryPrefix()
    {
        var query = CausalMeTTaIntegration.GenerateInterventionQuery("Treatment", "Outcome");

        query.Should().StartWith("!");
        query.Should().Contain("query");
        query.Should().Contain("&causal-space");
        query.Should().Contain("Treatment");
        query.Should().Contain("Outcome");
    }

    // ========================================================================
    // GenerateCounterfactualQuery - with observations
    // ========================================================================

    [Fact]
    public void GenerateCounterfactualQuery_MultipleObservations_IncludesAll()
    {
        var factual = new Observation(
            new Dictionary<string, object> { ["X"] = 1.0, ["Z"] = 0.5 },
            DateTime.UtcNow, null);

        var query = CausalMeTTaIntegration.GenerateCounterfactualQuery("X", "Y", factual);

        query.Should().Contain("counterfactual Y");
        query.Should().Contain("do X");
        query.Should().Contain("observed");
        query.Should().Contain("(X 1)");
        query.Should().Contain("(Z 0.5)");
    }

    // ========================================================================
    // GeneratePathFindingRules - structure
    // ========================================================================

    [Fact]
    public void GeneratePathFindingRules_ContainsBaseAndRecursiveCases()
    {
        var graph = CreateSimpleGraph();

        var rules = CausalMeTTaIntegration.GeneratePathFindingRules(graph);

        rules.Should().Contain("Direct path");
        rules.Should().Contain("Indirect path");
        rules.Should().Contain("cons");
    }

    // ========================================================================
    // GenerateEffectComputationRules - structure
    // ========================================================================

    [Fact]
    public void GenerateEffectComputationRules_ContainsDirectAndTotalEffect()
    {
        var graph = CreateSimpleGraph();

        var rules = CausalMeTTaIntegration.GenerateEffectComputationRules(graph);

        rules.Should().Contain("Direct causal effect");
        rules.Should().Contain("Total causal effect");
        rules.Should().Contain("Path effect");
        rules.Should().Contain("product");
        rules.Should().Contain("sum");
    }

    // ========================================================================
    // ExplanationToMeTTa - detailed output
    // ========================================================================

    [Fact]
    public void ExplanationToMeTTa_MultipleAttributionsAndPaths_IncludesAll()
    {
        var paths = new List<CausalPath>
        {
            new(new List<string> { "X", "Y" }, 0.8, true,
                new List<CausalEdge> { new("X", "Y", 0.8, EdgeType.Direct) }),
            new(new List<string> { "X", "Z", "Y" }, 0.3, false,
                new List<CausalEdge>
                {
                    new("X", "Z", 0.5, EdgeType.Direct),
                    new("Z", "Y", 0.6, EdgeType.Direct)
                })
        };
        var attributions = new Dictionary<string, double>
        {
            ["X"] = 0.8,
            ["Z"] = 0.3
        };
        var explanation = new Explanation("Y", paths, attributions, "X causes Y directly and indirectly");

        var result = CausalMeTTaIntegration.ExplanationToMeTTa(explanation);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("explanation Y");
        result.Value.Should().Contain("(X 0.800)");
        result.Value.Should().Contain("(Z 0.300)");
        result.Value.Should().Contain("path (X Y)");
        result.Value.Should().Contain("path (X Z Y)");
        result.Value.Should().Contain("True");  // IsDirect
        result.Value.Should().Contain("False"); // Not direct
    }

    [Fact]
    public void ExplanationToMeTTa_EmptyAttributionsAndPaths_ReturnsSuccess()
    {
        var explanation = new Explanation(
            "Y",
            new List<CausalPath>(),
            new Dictionary<string, double>(),
            "No causes found");

        var result = CausalMeTTaIntegration.ExplanationToMeTTa(explanation);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("explanation Y");
        result.Value.Should().Contain("attributions");
        result.Value.Should().Contain("causal-paths");
    }

    // ========================================================================
    // GenerateInterventionPlanningRules - structure
    // ========================================================================

    [Fact]
    public void GenerateInterventionPlanningRules_ContainsAllComponents()
    {
        var rules = CausalMeTTaIntegration.GenerateInterventionPlanningRules();

        rules.Should().Contain("Intervention Planning Rules");
        rules.Should().Contain("max-by effect-size");
        rules.Should().Contain("Evaluate intervention candidate");
        rules.Should().Contain("total-effect");
        rules.Should().Contain("Identify side effects");
        rules.Should().Contain("filter");
        rules.Should().Contain("lambda");
    }

    // ========================================================================
    // Helpers
    // ========================================================================

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
        return new CausalGraph(variables, edges, new Dictionary<string, StructuralEquation>());
    }
}
