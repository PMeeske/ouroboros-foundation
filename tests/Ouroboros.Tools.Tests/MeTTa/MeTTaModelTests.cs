using Ouroboros.Tools.MeTTa;

namespace Ouroboros.Tools.Tests.MeTTa;

[Trait("Category", "Unit")]
public class HypothesisTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var evidence = new List<Fact>
        {
            new("(color sky blue)", 0.9)
        };
        var hypothesis = new Hypothesis("The sky is blue", 0.85, evidence);

        hypothesis.Statement.Should().Be("The sky is blue");
        hypothesis.Plausibility.Should().Be(0.85);
        hypothesis.SupportingEvidence.Should().HaveCount(1);
    }

    [Fact]
    public void RecordEquality_Works()
    {
        var evidence = new List<Fact>();
        var a = new Hypothesis("H1", 0.5, evidence);
        var b = new Hypothesis("H1", 0.5, evidence);
        a.Should().Be(b);
    }
}

[Trait("Category", "Unit")]
public class ProofStepTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var rule = new Rule("(= (implies $a $b) (or (not $a) $b))", "implication-elimination");
        var facts = new List<Fact> { new("(implies rain wet)", 0.9) };
        var step = new ProofStep("Applying modus ponens", rule, facts);

        step.Inference.Should().Be("Applying modus ponens");
        step.RuleApplied.Should().Be(rule);
        step.UsedFacts.Should().HaveCount(1);
    }
}

[Trait("Category", "Unit")]
public class ProofStrategyEnumTests
{
    [Theory]
    [InlineData(ProofStrategy.Resolution)]
    [InlineData(ProofStrategy.Tableaux)]
    [InlineData(ProofStrategy.NaturalDeduction)]
    public void AllValues_AreDefined(ProofStrategy strategy)
    {
        Enum.IsDefined(typeof(ProofStrategy), strategy).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class ProofTraceTests
{
    [Fact]
    public void ProvedTrace_HasCorrectProperties()
    {
        var steps = new List<ProofStep>();
        var trace = new ProofTrace(steps, true);

        trace.Steps.Should().BeEmpty();
        trace.Proved.Should().BeTrue();
        trace.CounterExample.Should().BeNull();
    }

    [Fact]
    public void UnprovedTrace_HasCounterExample()
    {
        var trace = new ProofTrace(new List<ProofStep>(), false, "x=0 is a counter-example");

        trace.Proved.Should().BeFalse();
        trace.CounterExample.Should().Be("x=0 is a counter-example");
    }
}

[Trait("Category", "Unit")]
public class TypeContextTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var bindings = new Dictionary<string, string> { { "x", "Int" }, { "y", "String" } };
        var constraints = new List<string> { "x > 0", "y.Length > 0" };
        var context = new TypeContext(bindings, constraints);

        context.Bindings.Should().HaveCount(2);
        context.Bindings["x"].Should().Be("Int");
        context.Constraints.Should().HaveCount(2);
    }
}

[Trait("Category", "Unit")]
public class TypedAtomTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var typeParams = new Dictionary<string, string> { { "T", "Int" } };
        var atom = new TypedAtom("(+ 1 2)", "Int", typeParams);

        atom.Atom.Should().Be("(+ 1 2)");
        atom.Type.Should().Be("Int");
        atom.TypeParameters.Should().ContainKey("T");
    }
}

[Trait("Category", "Unit")]
public class HyperonFlowTests
{
    [Fact]
    public void Name_ReturnsFlowName()
    {
        using var engine = new HyperonMeTTaEngine();
        var flow = engine.CreateFlow("test-flow", "A test flow");

        flow.Name.Should().Be("test-flow");
        flow.Description.Should().Be("A test flow");
    }

    [Fact]
    public async Task ExecuteAsync_EmptyFlow_CompletesSuccessfully()
    {
        using var engine = new HyperonMeTTaEngine();
        var flow = engine.CreateFlow("empty", "Empty flow");

        var act = () => flow.ExecuteAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task LoadFacts_AddsFactsToSpace()
    {
        using var engine = new HyperonMeTTaEngine();
        var flow = engine.CreateFlow("facts-flow", "Load facts")
            .LoadFacts("(color red)", "(color blue)");

        await flow.ExecuteAsync();

        var atoms = engine.AtomSpace.All().ToList();
        atoms.Should().Contain(a => a.ToSExpr() == "(color red)");
        atoms.Should().Contain(a => a.ToSExpr() == "(color blue)");
    }

    [Fact]
    public async Task ApplyRule_AddsRuleToSpace()
    {
        using var engine = new HyperonMeTTaEngine();
        var flow = engine.CreateFlow("rule-flow", "Apply rules")
            .ApplyRule("(= (double $x) (* $x 2))");

        await flow.ExecuteAsync();

        var atoms = engine.AtomSpace.All().ToList();
        atoms.Should().Contain(a => a.ToSExpr().Contains("double"));
    }

    [Fact]
    public async Task Query_InvokesResultHandler()
    {
        using var engine = new HyperonMeTTaEngine();
        string? capturedResult = null;

        var flow = engine.CreateFlow("query-flow", "Query")
            .LoadFacts("(parent alice bob)")
            .Query("(parent alice bob)", result => capturedResult = result);

        await flow.ExecuteAsync();

        capturedResult.Should().NotBeNull();
    }

    [Fact]
    public async Task SideEffect_ExecutesSideEffect()
    {
        using var engine = new HyperonMeTTaEngine();
        var executed = false;

        var flow = engine.CreateFlow("side-effect-flow", "Side effect")
            .SideEffect(_ => executed = true);

        await flow.ExecuteAsync();

        executed.Should().BeTrue();
    }

    [Fact]
    public async Task Transform_ExecutesTransformation()
    {
        using var engine = new HyperonMeTTaEngine();
        var transformed = false;

        var flow = engine.CreateFlow("transform-flow", "Transform")
            .Transform(async (eng, ct) =>
            {
                await eng.AddFactAsync("(transformed yes)", ct);
                transformed = true;
            });

        await flow.ExecuteAsync();

        transformed.Should().BeTrue();
    }

    [Fact]
    public async Task Chaining_ExecutesStepsInOrder()
    {
        using var engine = new HyperonMeTTaEngine();
        var order = new List<int>();

        var flow = engine.CreateFlow("chain-flow", "Chaining test")
            .SideEffect(_ => order.Add(1))
            .SideEffect(_ => order.Add(2))
            .SideEffect(_ => order.Add(3));

        await flow.ExecuteAsync();

        order.Should().BeEquivalentTo(new[] { 1, 2, 3 }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellation_ThrowsOperationCanceled()
    {
        using var engine = new HyperonMeTTaEngine();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var flow = engine.CreateFlow("cancel-flow", "Cancelable")
            .SideEffect(_ => { });

        await Assert.ThrowsAsync<OperationCanceledException>(() => flow.ExecuteAsync(cts.Token));
    }
}
