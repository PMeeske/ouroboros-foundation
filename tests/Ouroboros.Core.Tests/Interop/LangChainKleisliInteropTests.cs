namespace Ouroboros.Tests.Interop;

using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Interop;
using Ouroboros.Core.Kleisli;
using Ouroboros.Core.Steps;

/// <summary>
/// Tests for the PipelineBuilder{TIn, TCurrent} class defined in LangChainKleisliInterop.cs.
/// The file contains the typed pipeline builder with Then/Build/ExecuteAsync methods.
/// </summary>
[Trait("Category", "Unit")]
public class LangChainKleisliInteropTests
{
    [Fact]
    public async Task TypedBuilder_Then_WithStep_ChainsExecution()
    {
        Step<string, string> upper = s => Task.FromResult(s.ToUpperInvariant());
        Step<string, int> length = s => Task.FromResult(s.Length);

        var pipeNode = upper.ToCompatNode("upper");
        var builder = new PipelineBuilder<string, string>("test", pipeNode);

        var result = await builder.Then(length).ExecuteAsync("hello").ConfigureAwait(false);

        result.Should().Be(5);
    }

    [Fact]
    public async Task TypedBuilder_Then_WithFunc_ChainsExecution()
    {
        Step<int, int> doubleStep = i => Task.FromResult(i * 2);

        var pipeNode = doubleStep.ToCompatNode("double");
        var builder = new PipelineBuilder<int, int>("test", pipeNode);

        var result = await builder.Then((int i) => $"result={i}").ExecuteAsync(5).ConfigureAwait(false);

        result.Should().Be("result=10");
    }

    [Fact]
    public async Task TypedBuilder_Build_ReturnsPipeNode()
    {
        Step<int, string> toString = i => Task.FromResult(i.ToString());

        var pipeNode = toString.ToCompatNode("toString");
        var builder = new PipelineBuilder<int, string>("test", pipeNode);

        var built = builder.Build();

        var result = await (42 | built).ConfigureAwait(false);
        result.Should().Be("42");
    }

    [Fact]
    public async Task TypedBuilder_ExecuteAsync_RunsPipeline()
    {
        Step<string, int> length = s => Task.FromResult(s.Length);

        var pipeNode = length.ToCompatNode("length");
        var builder = new PipelineBuilder<string, int>("test", pipeNode);

        var result = await builder.ExecuteAsync("hello world").ConfigureAwait(false);

        result.Should().Be(11);
    }

    [Fact]
    public async Task TypedBuilder_MultipleThen_ChainsCorrectly()
    {
        Step<string, string> upper = s => Task.FromResult(s.ToUpperInvariant());

        var pipeNode = upper.ToCompatNode("upper");
        var builder = new PipelineBuilder<string, string>("multi", pipeNode);

        var result = await builder
            .Then((string s) => s.Length)
            .Then((int n) => n > 3)
            .ExecuteAsync("hello");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task TypedBuilder_Then_WithStepCustomName()
    {
        Step<int, int> increment = i => Task.FromResult(i + 1);
        Step<int, string> show = i => Task.FromResult(i.ToString());

        var pipeNode = increment.ToCompatNode("inc");
        var builder = new PipelineBuilder<int, int>("test", pipeNode);

        var pipeline = builder.Then(show, "show").Build();

        pipeline.Node.Name.Should().Contain("show");
    }

    [Fact]
    public async Task TypedBuilder_Then_WithFuncCustomName()
    {
        Step<int, int> increment = i => Task.FromResult(i + 1);

        var pipeNode = increment.ToCompatNode("inc");
        var builder = new PipelineBuilder<int, int>("test", pipeNode);

        var pipeline = builder.Then((int i) => i.ToString(), "stringify").Build();

        pipeline.Node.Name.Should().Contain("stringify");
    }

    [Fact]
    public async Task TypedBuilder_EndToEnd_WithEnhancedSteps()
    {
        var pipeNode = EnhancedSteps.Upper.ToCompatNode("upper");
        var builder = new PipelineBuilder<string, string>("e2e", pipeNode);

        var result = await builder
            .Then(EnhancedSteps.Length)
            .Then(EnhancedSteps.Show)
            .ExecuteAsync("test");

        result.Should().Be("length=4");
    }

    [Fact]
    public async Task TypedBuilder_BuildThenExecuteViaOperator()
    {
        Step<int, int> triple = i => Task.FromResult(i * 3);

        var pipeNode = triple.ToCompatNode("triple");
        var pipeline = new PipelineBuilder<int, int>("test", pipeNode).Build();

        var result = await (7 | pipeline).ConfigureAwait(false);

        result.Should().Be(21);
    }

    [Fact]
    public async Task TypedBuilder_SingleStep_ExecuteAsync()
    {
        Step<string, string> identity = s => Task.FromResult(s);

        var pipeNode = identity.ToCompatNode("id");
        var builder = new PipelineBuilder<string, string>("test", pipeNode);

        var result = await builder.ExecuteAsync("unchanged").ConfigureAwait(false);

        result.Should().Be("unchanged");
    }
}
