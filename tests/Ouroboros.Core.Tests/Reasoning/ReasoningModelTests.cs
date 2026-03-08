using Ouroboros.Core.Reasoning;

namespace Ouroboros.Tests.Reasoning;

[Trait("Category", "Unit")]
public sealed class ReasoningModelTests
{
    [Fact]
    public void Variable_Creation_SetsProperties()
    {
        var sut = new Variable("X", VariableType.Continuous, [1.0, 2.0, 3.0]);

        sut.Name.Should().Be("X");
        sut.Type.Should().Be(VariableType.Continuous);
        sut.PossibleValues.Should().HaveCount(3);
    }

    [Fact]
    public void Variable_BinaryType_HasCorrectType()
    {
        var sut = new Variable("flag", VariableType.Binary, [(object)0, (object)1]);

        sut.Type.Should().Be(VariableType.Binary);
    }

    [Fact]
    public void CausalEdge_Creation_SetsProperties()
    {
        var sut = new CausalEdge("Cause", "Effect", 0.8, EdgeType.Direct);

        sut.Cause.Should().Be("Cause");
        sut.Effect.Should().Be("Effect");
        sut.Strength.Should().Be(0.8);
        sut.Type.Should().Be(EdgeType.Direct);
    }

    [Fact]
    public void CausalEdge_ConfoundedType_IsCorrect()
    {
        var sut = new CausalEdge("A", "B", 0.5, EdgeType.Confounded);

        sut.Type.Should().Be(EdgeType.Confounded);
    }

    [Fact]
    public void CausalPath_Creation_SetsProperties()
    {
        var edges = new List<CausalEdge>
        {
            new("A", "B", 0.9, EdgeType.Direct),
            new("B", "C", 0.8, EdgeType.Direct)
        };
        var sut = new CausalPath(["A", "B", "C"], 0.72, false, edges);

        sut.Variables.Should().HaveCount(3);
        sut.TotalEffect.Should().Be(0.72);
        sut.IsDirect.Should().BeFalse();
        sut.Edges.Should().HaveCount(2);
    }

    [Fact]
    public void CausalPath_DirectPath_SetsFlagCorrectly()
    {
        var edges = new List<CausalEdge> { new("A", "B", 0.9, EdgeType.Direct) };
        var sut = new CausalPath(["A", "B"], 0.9, true, edges);

        sut.IsDirect.Should().BeTrue();
    }

    [Fact]
    public void CausalGraph_Creation_SetsAllCollections()
    {
        var variables = new List<Variable>
        {
            new("X", VariableType.Continuous, []),
            new("Y", VariableType.Continuous, [])
        };
        var edges = new List<CausalEdge> { new("X", "Y", 0.7, EdgeType.Direct) };
        var equations = new Dictionary<string, StructuralEquation>();

        var sut = new CausalGraph(variables, edges, equations);

        sut.Variables.Should().HaveCount(2);
        sut.Edges.Should().HaveCount(1);
        sut.Equations.Should().NotBeNull();
    }

    [Fact]
    public void Distribution_Normal_SetsProperties()
    {
        var sut = new Distribution(DistributionType.Normal, 5.0, 1.0, [4.5, 5.0, 5.5], new Dictionary<object, double>());

        sut.Type.Should().Be(DistributionType.Normal);
        sut.Mean.Should().Be(5.0);
        sut.Variance.Should().Be(1.0);
        sut.Samples.Should().HaveCount(3);
    }

    [Fact]
    public void Distribution_Bernoulli_SetsProperties()
    {
        var probs = new Dictionary<object, double> { [0] = 0.3, [1] = 0.7 };
        var sut = new Distribution(DistributionType.Bernoulli, 0.7, 0.21, [], probs);

        sut.Type.Should().Be(DistributionType.Bernoulli);
        sut.Probabilities.Should().HaveCount(2);
    }

    [Fact]
    public void Explanation_Creation_SetsProperties()
    {
        var paths = new List<CausalPath>
        {
            new(["A", "B"], 0.9, true, [new("A", "B", 0.9, EdgeType.Direct)])
        };
        var attributions = new Dictionary<string, double> { ["A"] = 0.9 };
        var sut = new Explanation("B", paths, attributions, "A directly causes B");

        sut.Effect.Should().Be("B");
        sut.CausalPaths.Should().HaveCount(1);
        sut.Attributions["A"].Should().Be(0.9);
        sut.NarrativeExplanation.Should().Be("A directly causes B");
    }

    [Fact]
    public void Intervention_Creation_SetsProperties()
    {
        var sut = new Intervention("X", 42.0, 0.8, 0.95, ["Y", "Z"]);

        sut.TargetVariable.Should().Be("X");
        sut.NewValue.Should().Be(42.0);
        sut.ExpectedEffect.Should().Be(0.8);
        sut.Confidence.Should().Be(0.95);
        sut.SideEffects.Should().HaveCount(2);
    }

    [Fact]
    public void Observation_Creation_SetsProperties()
    {
        var values = new Dictionary<string, object> { ["temp"] = 25.0 };
        var now = DateTime.UtcNow;
        var sut = new Observation(values, now, "test context");

        sut.Values.Should().ContainKey("temp");
        sut.Timestamp.Should().Be(now);
        sut.Context.Should().Be("test context");
    }

    [Fact]
    public void Observation_NullContext_IsAllowed()
    {
        var sut = new Observation(new Dictionary<string, object>(), DateTime.UtcNow, null);

        sut.Context.Should().BeNull();
    }

    [Fact]
    public void StructuralEquation_Creation_SetsProperties()
    {
        Func<Dictionary<string, object>, object> fn = inputs => (double)inputs["X"] * 2;
        var sut = new StructuralEquation("Y", ["X"], fn, 0.1);

        sut.Outcome.Should().Be("Y");
        sut.Parents.Should().ContainSingle().Which.Should().Be("X");
        sut.NoiseVariance.Should().Be(0.1);
        sut.Function(new Dictionary<string, object> { ["X"] = 3.0 }).Should().Be(6.0);
    }

    [Fact]
    public void EdgeType_HasAllExpectedValues()
    {
        Enum.GetValues<EdgeType>().Should().HaveCount(4);
        Enum.IsDefined(EdgeType.Direct).Should().BeTrue();
        Enum.IsDefined(EdgeType.Confounded).Should().BeTrue();
        Enum.IsDefined(EdgeType.Mediated).Should().BeTrue();
        Enum.IsDefined(EdgeType.Collider).Should().BeTrue();
    }

    [Fact]
    public void VariableType_HasAllExpectedValues()
    {
        Enum.GetValues<VariableType>().Should().HaveCount(4);
        Enum.IsDefined(VariableType.Binary).Should().BeTrue();
        Enum.IsDefined(VariableType.Categorical).Should().BeTrue();
        Enum.IsDefined(VariableType.Continuous).Should().BeTrue();
        Enum.IsDefined(VariableType.Ordinal).Should().BeTrue();
    }

    [Fact]
    public void DistributionType_HasAllExpectedValues()
    {
        Enum.GetValues<DistributionType>().Should().HaveCount(4);
    }

    [Fact]
    public void DiscoveryAlgorithm_HasAllExpectedValues()
    {
        Enum.GetValues<DiscoveryAlgorithm>().Should().HaveCount(5);
        Enum.IsDefined(DiscoveryAlgorithm.PC).Should().BeTrue();
        Enum.IsDefined(DiscoveryAlgorithm.NOTEARS).Should().BeTrue();
    }
}
