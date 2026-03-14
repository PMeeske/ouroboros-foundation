using Ouroboros.Core.Interop;
using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.Interop;

/// <summary>
/// Additional tests for Interop classes covering remaining uncovered lines.
/// </summary>
[Trait("Category", "Unit")]
public class CompatInteropAdditionalTests
{
    [Fact]
    public void ToCompatNode_Step_DefaultName_ContainsTypeNames()
    {
        Step<string, int> step = async s => { await Task.Yield(); return s.Length; };
        var node = step.ToCompatNode();

        node.Node.Name.Should().Contain("String");
        node.Node.Name.Should().Contain("Int32");
    }

    [Fact]
    public void ToCompatNode_Step_CustomName_UsesProvidedName()
    {
        Step<string, int> step = async s => { await Task.Yield(); return s.Length; };
        var node = step.ToCompatNode("MyStep");

        node.Node.Name.Should().Be("MyStep");
    }

    [Fact]
    public async Task ToCompatNode_KleisliResult_ExecutesCorrectly()
    {
        KleisliResult<string, int, string> kleisli = async s =>
        {
            await Task.Yield();
            if (int.TryParse(s, out int v))
                return Result<int, string>.Success(v);
            return Result<int, string>.Failure("parse error");
        };

        var node = kleisli.ToCompatNode("Parse");
        var result = await ("42" | node);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ToCompatNode_KleisliResult_DefaultName_ContainsTypeNames()
    {
        KleisliResult<string, int, string> kleisli = s =>
            Task.FromResult(Result<int, string>.Success(0));

        var node = kleisli.ToCompatNode();
        node.Node.Name.Should().Contain("KleisliResult");
    }

    [Fact]
    public async Task ToCompatNode_KleisliOption_ExecutesCorrectly()
    {
        KleisliOption<int, int> kleisliOption = async n =>
        {
            await Task.Yield();
            return n > 0 ? Option<int>.Some(n) : Option<int>.None();
        };

        var node = kleisliOption.ToCompatNode("OnlyPositive");
        var result = await (5 | node);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public void ToCompatNode_KleisliOption_DefaultName_ContainsTypeNames()
    {
        KleisliOption<int, int> kleisliOption = n =>
            Task.FromResult(Option<int>.Some(n));

        var node = kleisliOption.ToCompatNode();
        node.Node.Name.Should().Contain("KleisliOption");
    }

    [Fact]
    public async Task ToCompatNode_PureFunc_ExecutesCorrectly()
    {
        Func<int, string> f = x => $"val={x}";
        var node = f.ToCompatNode("Format");

        var result = await (42 | node);
        result.Should().Be("val=42");
    }

    [Fact]
    public void ToCompatNode_PureFunc_DefaultName_ContainsTypeNames()
    {
        Func<int, string> f = x => x.ToString();
        var node = f.ToCompatNode();

        node.Node.Name.Should().Contain("Func");
    }

    [Fact]
    public async Task ToCompatNode_AsyncFunc_ExecutesCorrectly()
    {
        Func<int, Task<string>> f = async x =>
        {
            await Task.Yield();
            return $"async={x}";
        };

        var node = f.ToCompatNode("AsyncFormat");
        var result = await (10 | node);

        result.Should().Be("async=10");
    }

    [Fact]
    public void ToCompatNode_AsyncFunc_DefaultName_ContainsTypeNames()
    {
        Func<int, Task<string>> f = x => Task.FromResult(x.ToString());
        var node = f.ToCompatNode();

        node.Node.Name.Should().Contain("Async");
    }

    [Fact]
    public void StartPipeline_ReturnsBuilder()
    {
        var builder = CompatInterop.StartPipeline<string>("TestPipeline");
        builder.Should().NotBeNull();
    }

    [Fact]
    public void StartPipeline_DefaultName_ReturnsBuilder()
    {
        var builder = CompatInterop.StartPipeline<int>();
        builder.Should().NotBeNull();
    }
}

[Trait("Category", "Unit")]
public class PipelineBuilderAdditionalTests
{
    [Fact]
    public async Task AddStep_CreatesTypedBuilder()
    {
        Step<string, int> step = async s => { await Task.Yield(); return s.Length; };

        var builder = new PipelineBuilder<string>("Test")
            .AddStep(step, "Length");

        var result = await builder.ExecuteAsync("hello");
        result.Should().Be(5);
    }

    [Fact]
    public async Task AddResultStep_CreatesTypedBuilder()
    {
        KleisliResult<string, int, string> kleisli = async s =>
        {
            await Task.Yield();
            return int.TryParse(s, out int v)
                ? Result<int, string>.Success(v)
                : Result<int, string>.Failure("parse error");
        };

        var builder = new PipelineBuilder<string>("Test")
            .AddResultStep(kleisli, "Parse");

        var result = await builder.ExecuteAsync("42");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task AddFunc_CreatesTypedBuilder()
    {
        Func<string, int> f = s => s.Length;

        var builder = new PipelineBuilder<string>("Test")
            .AddFunc(f, "Length");

        var result = await builder.ExecuteAsync("hello");
        result.Should().Be(5);
    }
}

[Trait("Category", "Unit")]
public class TypedPipelineBuilderAdditionalTests
{
    [Fact]
    public async Task Then_WithStep_ComposesCorrectly()
    {
        Step<string, int> step1 = async s => { await Task.Yield(); return s.Length; };
        Step<int, string> step2 = async n => { await Task.Yield(); return $"len={n}"; };

        var builder = new PipelineBuilder<string>("Test")
            .AddStep(step1, "Length")
            .Then(step2, "Format");

        var result = await builder.ExecuteAsync("hello");
        result.Should().Be("len=5");
    }

    [Fact]
    public async Task Then_WithFunc_ComposesCorrectly()
    {
        Step<string, int> step = async s => { await Task.Yield(); return s.Length; };
        Func<int, string> f = n => $"len={n}";

        var builder = new PipelineBuilder<string>("Test")
            .AddStep(step, "Length")
            .Then(f, "Format");

        var result = await builder.ExecuteAsync("hello");
        result.Should().Be("len=5");
    }

    [Fact]
    public void Build_ReturnsPipeNode()
    {
        Step<string, int> step = s => Task.FromResult(s.Length);

        var node = new PipelineBuilder<string>("Test")
            .AddStep(step, "Length")
            .Build();

        node.Node.Should().NotBeNull();
    }
}

[Trait("Category", "Unit")]
public class LambdaNodeAdditionalTests
{
    [Fact]
    public void ToString_ReturnsName()
    {
        var node = new LambdaNode<int, string>("MyNode", (i, _) => Task.FromResult(i.ToString()));
        node.ToString().Should().Be("MyNode");
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationToken_PassesToken()
    {
        CancellationToken capturedToken = default;
        var node = new LambdaNode<int, int>("test", (i, ct) =>
        {
            capturedToken = ct;
            return Task.FromResult(i);
        });

        using var cts = new CancellationTokenSource();
        await node.InvokeAsync(42, cts.Token);

        capturedToken.Should().Be(cts.Token);
    }
}

[Trait("Category", "Unit")]
public class EnhancedStepsAdditionalTests
{
    [Fact]
    public async Task Upper_ConvertsToUpperCase()
    {
        var result = await EnhancedSteps.Upper("hello world");
        result.Should().Be("HELLO WORLD");
    }

    [Fact]
    public async Task Length_ReturnsStringLength()
    {
        var result = await EnhancedSteps.Length("hello");
        result.Should().Be(5);
    }

    [Fact]
    public async Task Show_FormatsNumber()
    {
        var result = await EnhancedSteps.Show(42);
        result.Should().Be("length=42");
    }

    [Fact]
    public async Task SafeParse_ValidNumber_ReturnsSuccess()
    {
        var result = await EnhancedSteps.SafeParse("42");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task SafeParse_InvalidNumber_ReturnsFailure()
    {
        var result = await EnhancedSteps.SafeParse("abc");
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cannot parse");
    }

    [Fact]
    public async Task OnlyPositive_PositiveNumber_ReturnsSome()
    {
        var result = await EnhancedSteps.OnlyPositive(5);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task OnlyPositive_Zero_ReturnsNone()
    {
        var result = await EnhancedSteps.OnlyPositive(0);
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task OnlyPositive_Negative_ReturnsNone()
    {
        var result = await EnhancedSteps.OnlyPositive(-3);
        result.HasValue.Should().BeFalse();
    }
}
