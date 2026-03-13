namespace Ouroboros.Tests.Interop;

using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Interop;
using Ouroboros.Core.Kleisli;
using Ouroboros.Core.Steps;

[Trait("Category", "Unit")]
public class PipelineBuilderTests
{
    // Tests for PipelineBuilder<TIn> (untyped initial builder)

    [Fact]
    public async Task AddStep_CreatesTypedBuilder()
    {
        Step<string, int> lengthStep = s => Task.FromResult(s.Length);

        var builder = new PipelineBuilder<string>("test");
        var typedBuilder = builder.AddStep(lengthStep);

        typedBuilder.Should().NotBeNull();
        typedBuilder.Should().BeOfType<PipelineBuilder<string, int>>();
    }

    [Fact]
    public async Task AddStep_ExecutesPipeline()
    {
        Step<string, int> lengthStep = s => Task.FromResult(s.Length);

        var result = await new PipelineBuilder<string>("test")
            .AddStep(lengthStep)
            .ExecuteAsync("hello");

        result.Should().Be(5);
    }

    [Fact]
    public async Task AddResultStep_CreatesTypedBuilder()
    {
        KleisliResult<string, int, string> parseStep = s =>
            Task.FromResult(int.TryParse(s, out int v)
                ? Result<int, string>.Success(v)
                : Result<int, string>.Failure("bad"));

        var builder = new PipelineBuilder<string>("test")
            .AddResultStep(parseStep);

        var result = await builder.ExecuteAsync("42").ConfigureAwait(false);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task AddFunc_CreatesTypedBuilder()
    {
        var result = await new PipelineBuilder<int>("test")
            .AddFunc((int i) => i * 2)
            .ExecuteAsync(5);

        result.Should().Be(10);
    }

    [Fact]
    public async Task AddFunc_WithCustomName()
    {
        var pipeline = new PipelineBuilder<int>("test")
            .AddFunc((int i) => i.ToString(), "to-string")
            .Build();

        pipeline.Node.Name.Should().Contain("to-string");
    }

    // Tests for PipelineBuilder<TIn, TCurrent> (typed builder)

    [Fact]
    public async Task Then_WithStep_ChainsPipeline()
    {
        Step<string, int> lengthStep = s => Task.FromResult(s.Length);
        Step<int, string> showStep = n => Task.FromResult($"len={n}");

        var result = await new PipelineBuilder<string>("test")
            .AddStep(lengthStep)
            .Then(showStep)
            .ExecuteAsync("hello");

        result.Should().Be("len=5");
    }

    [Fact]
    public async Task Then_WithFunc_ChainsPipeline()
    {
        Step<string, string> upperStep = s => Task.FromResult(s.ToUpperInvariant());

        var result = await new PipelineBuilder<string>("test")
            .AddStep(upperStep)
            .Then((string s) => s.Length)
            .ExecuteAsync("hello");

        result.Should().Be(5);
    }

    [Fact]
    public async Task Then_MultipleThenCalls_ChainsCorrectly()
    {
        Step<string, string> upperStep = s => Task.FromResult(s.ToUpperInvariant());
        Step<string, int> lengthStep = s => Task.FromResult(s.Length);
        Step<int, string> showStep = n => Task.FromResult($"result={n}");

        var result = await new PipelineBuilder<string>("multi")
            .AddStep(upperStep)
            .Then(lengthStep)
            .Then(showStep)
            .ExecuteAsync("test");

        result.Should().Be("result=4");
    }

    [Fact]
    public async Task Build_ReturnsPipeNode()
    {
        Step<int, int> doubleStep = i => Task.FromResult(i * 2);

        var pipeline = new PipelineBuilder<int>("test")
            .AddStep(doubleStep)
            .Build();

        var result = await (3 | pipeline).ConfigureAwait(false);

        result.Should().Be(6);
    }

    [Fact]
    public async Task ExecuteAsync_RunsPipeline()
    {
        Step<int, string> toStringStep = i => Task.FromResult(i.ToString());

        var result = await new PipelineBuilder<int>("test")
            .AddStep(toStringStep)
            .ExecuteAsync(42);

        result.Should().Be("42");
    }

    [Fact]
    public async Task Then_WithStepCustomName()
    {
        Step<int, int> step = i => Task.FromResult(i + 1);
        Step<int, int> step2 = i => Task.FromResult(i * 2);

        var pipeline = new PipelineBuilder<int>("test")
            .AddStep(step, "increment")
            .Then(step2, "double")
            .Build();

        pipeline.Node.Name.Should().Contain("increment");
        pipeline.Node.Name.Should().Contain("double");
    }

    [Fact]
    public async Task Then_WithFuncCustomName()
    {
        Step<int, int> step = i => Task.FromResult(i + 1);

        var pipeline = new PipelineBuilder<int>("test")
            .AddStep(step, "increment")
            .Then((int i) => i.ToString(), "stringify")
            .Build();

        pipeline.Node.Name.Should().Contain("stringify");
    }

    [Fact]
    public async Task FullPipeline_WithEnhancedSteps()
    {
        var result = await new PipelineBuilder<string>("enhanced")
            .AddStep(EnhancedSteps.Upper)
            .Then(EnhancedSteps.Length)
            .Then(EnhancedSteps.Show)
            .ExecuteAsync("hello");

        result.Should().Be("length=5");
    }
}
