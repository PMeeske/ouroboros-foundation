using Ouroboros.Core.Reasoning;

namespace Ouroboros.Core.Tests.Reasoning;

[Trait("Category", "Unit")]
public class CausalEdgeTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var edge = new CausalEdge("X", "Y", 0.8, EdgeType.Direct);
        edge.Cause.Should().Be("X");
        edge.Effect.Should().Be("Y");
        edge.Strength.Should().Be(0.8);
        edge.Type.Should().Be(EdgeType.Direct);
    }

    [Fact]
    public void RecordEquality_Works()
    {
        var a = new CausalEdge("X", "Y", 0.8, EdgeType.Direct);
        var b = new CausalEdge("X", "Y", 0.8, EdgeType.Direct);
        a.Should().Be(b);
    }
}

[Trait("Category", "Unit")]
public class CausalGraphTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var variables = new List<Variable>
        {
            new("X", VariableType.Binary, new List<object> { 0, 1 }),
            new("Y", VariableType.Continuous, new List<object>())
        };
        var edges = new List<CausalEdge>
        {
            new("X", "Y", 0.9, EdgeType.Direct)
        };
        var equations = new Dictionary<string, StructuralEquation>();

        var graph = new CausalGraph(variables, edges, equations);
        graph.Variables.Should().HaveCount(2);
        graph.Edges.Should().HaveCount(1);
        graph.Equations.Should().BeEmpty();
    }
}

[Trait("Category", "Unit")]
public class CausalPathTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var path = new CausalPath(
            new List<string> { "X", "Y", "Z" },
            0.72,
            false,
            new List<CausalEdge>
            {
                new("X", "Y", 0.9, EdgeType.Direct),
                new("Y", "Z", 0.8, EdgeType.Mediated)
            });

        path.Variables.Should().HaveCount(3);
        path.TotalEffect.Should().Be(0.72);
        path.IsDirect.Should().BeFalse();
        path.Edges.Should().HaveCount(2);
    }
}

[Trait("Category", "Unit")]
public class DistributionTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var dist = new Distribution(
            DistributionType.Normal,
            0.0,
            1.0,
            new List<double> { -0.5, 0.1, 0.8 },
            new Dictionary<object, double>());

        dist.Type.Should().Be(DistributionType.Normal);
        dist.Mean.Should().Be(0.0);
        dist.Variance.Should().Be(1.0);
        dist.Samples.Should().HaveCount(3);
    }
}

[Trait("Category", "Unit")]
public class ExplanationTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var explanation = new Explanation(
            "Y",
            new List<CausalPath>(),
            new Dictionary<string, double> { { "X", 0.8 } },
            "X causes Y");

        explanation.Effect.Should().Be("Y");
        explanation.CausalPaths.Should().BeEmpty();
        explanation.Attributions.Should().ContainKey("X");
        explanation.NarrativeExplanation.Should().Be("X causes Y");
    }
}

[Trait("Category", "Unit")]
public class StructuralEquationTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        Func<Dictionary<string, object>, object> func = inputs => (double)inputs["X"] * 2.0;
        var eq = new StructuralEquation(
            "Y",
            new List<string> { "X" },
            func,
            0.1);

        eq.Outcome.Should().Be("Y");
        eq.Parents.Should().ContainSingle("X");
        eq.NoiseVariance.Should().Be(0.1);

        var result = eq.Function(new Dictionary<string, object> { { "X", 3.0 } });
        result.Should().Be(6.0);
    }
}

[Trait("Category", "Unit")]
public class VariableTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var variable = new Variable("temp", VariableType.Continuous, new List<object>());
        variable.Name.Should().Be("temp");
        variable.Type.Should().Be(VariableType.Continuous);
        variable.PossibleValues.Should().BeEmpty();
    }
}

[Trait("Category", "Unit")]
public class EdgeTypeEnumTests
{
    [Theory]
    [InlineData(EdgeType.Direct)]
    [InlineData(EdgeType.Confounded)]
    [InlineData(EdgeType.Mediated)]
    [InlineData(EdgeType.Collider)]
    public void AllValues_AreDefined(EdgeType type)
    {
        Enum.IsDefined(typeof(EdgeType), type).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class DistributionTypeEnumTests
{
    [Theory]
    [InlineData(DistributionType.Normal)]
    [InlineData(DistributionType.Bernoulli)]
    [InlineData(DistributionType.Categorical)]
    [InlineData(DistributionType.Empirical)]
    public void AllValues_AreDefined(DistributionType type)
    {
        Enum.IsDefined(typeof(DistributionType), type).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class VariableTypeEnumTests
{
    [Theory]
    [InlineData(VariableType.Binary)]
    [InlineData(VariableType.Categorical)]
    [InlineData(VariableType.Continuous)]
    [InlineData(VariableType.Ordinal)]
    public void AllValues_AreDefined(VariableType type)
    {
        Enum.IsDefined(typeof(VariableType), type).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class DiscoveryAlgorithmEnumTests
{
    [Theory]
    [InlineData(DiscoveryAlgorithm.PC)]
    [InlineData(DiscoveryAlgorithm.FCI)]
    [InlineData(DiscoveryAlgorithm.GES)]
    [InlineData(DiscoveryAlgorithm.NOTEARS)]
    [InlineData(DiscoveryAlgorithm.DAGsNoCurl)]
    public void AllValues_AreDefined(DiscoveryAlgorithm algo)
    {
        Enum.IsDefined(typeof(DiscoveryAlgorithm), algo).Should().BeTrue();
    }
}
