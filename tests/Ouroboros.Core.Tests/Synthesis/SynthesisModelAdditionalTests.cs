using Ouroboros.Core.Synthesis;

namespace Ouroboros.Core.Tests.Synthesis;

[Trait("Category", "Unit")]
public class AbstractSyntaxTreeTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var root = new ASTNode("Apply", "add", new List<ASTNode>
        {
            new("Variable", "x", new List<ASTNode>()),
            new("Variable", "y", new List<ASTNode>())
        });
        var ast = new AbstractSyntaxTree(root, 2, 3);

        ast.Root.Should().Be(root);
        ast.Depth.Should().Be(2);
        ast.NodeCount.Should().Be(3);
    }
}

[Trait("Category", "Unit")]
public class CompressionStrategyEnumTests
{
    [Theory]
    [InlineData(CompressionStrategy.AntiUnification)]
    [InlineData(CompressionStrategy.EGraph)]
    [InlineData(CompressionStrategy.FragmentGrammar)]
    public void AllValues_AreDefined(CompressionStrategy strategy)
    {
        Enum.IsDefined(typeof(CompressionStrategy), strategy).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class ExecutionStepTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var step = new ExecutionStep("increment", new List<object> { 5 }, 6);
        step.PrimitiveName.Should().Be("increment");
        step.Inputs.Should().ContainSingle().Which.Should().Be(5);
        step.Output.Should().Be(6);
    }
}

[Trait("Category", "Unit")]
public class ExecutionTraceTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var steps = new List<ExecutionStep>
        {
            new("add", new List<object> { 1, 2 }, 3)
        };
        var trace = new ExecutionTrace(steps, 3, TimeSpan.FromMilliseconds(50));

        trace.Steps.Should().HaveCount(1);
        trace.FinalResult.Should().Be(3);
        trace.Duration.Should().Be(TimeSpan.FromMilliseconds(50));
    }
}

[Trait("Category", "Unit")]
public class PrimitiveTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        Func<object[], object> impl = args => (int)args[0] + 1;
        var primitive = new Primitive("increment", "int -> int", impl, -0.5);

        primitive.Name.Should().Be("increment");
        primitive.Type.Should().Be("int -> int");
        primitive.LogPrior.Should().Be(-0.5);
        primitive.Implementation(new object[] { 5 }).Should().Be(6);
    }
}

[Trait("Category", "Unit")]
public class RewriteRuleTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var pattern = new ASTNode("Apply", "identity", new List<ASTNode>
        {
            new("Variable", "x", new List<ASTNode>())
        });
        var replacement = new ASTNode("Variable", "x", new List<ASTNode>());

        var rule = new RewriteRule("remove-identity", pattern, replacement);
        rule.Name.Should().Be("remove-identity");
        rule.Pattern.Should().Be(pattern);
        rule.Replacement.Should().Be(replacement);
    }
}

[Trait("Category", "Unit")]
public class TypeRuleTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var rule = new TypeRule("add", new List<string> { "int", "int" }, "int");
        rule.Name.Should().Be("add");
        rule.InputTypes.Should().HaveCount(2);
        rule.OutputType.Should().Be("int");
    }
}

[Trait("Category", "Unit")]
public class UsageStatisticsTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var useCounts = new Dictionary<string, int> { { "add", 10 }, { "mul", 5 } };
        var successRates = new Dictionary<string, double> { { "add", 0.9 }, { "mul", 0.7 } };
        var stats = new UsageStatistics(useCounts, successRates, 15);

        stats.PrimitiveUseCounts.Should().HaveCount(2);
        stats.PrimitiveSuccessRates["add"].Should().Be(0.9);
        stats.TotalProgramsSynthesized.Should().Be(15);
    }
}
