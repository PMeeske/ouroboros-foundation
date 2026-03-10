namespace Ouroboros.Tests.Interop;

using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Interop;
using Ouroboros.Core.Kleisli;
using Ouroboros.Core.Steps;

[Trait("Category", "Unit")]
public class CompatInteropTests
{
    [Fact]
    public async Task ToCompatNode_FromStep_ExecutesCorrectly()
    {
        Step<string, int> step = s => Task.FromResult(s.Length);

        var node = step.ToCompatNode();
        var result = await (node.Node.InvokeAsync("hello"));

        result.Should().Be(5);
    }

    [Fact]
    public void ToCompatNode_FromStep_DefaultName()
    {
        Step<string, int> step = s => Task.FromResult(s.Length);

        var node = step.ToCompatNode();

        node.Node.Name.Should().Be("Step[String->Int32]");
    }

    [Fact]
    public void ToCompatNode_FromStep_CustomName()
    {
        Step<string, int> step = s => Task.FromResult(s.Length);

        var node = step.ToCompatNode("my-step");

        node.Node.Name.Should().Be("my-step");
    }

    [Fact]
    public async Task ToCompatNode_FromKleisliResult_ExecutesCorrectly()
    {
        KleisliResult<string, int, string> kr = s =>
            Task.FromResult(int.TryParse(s, out int v)
                ? Result<int, string>.Success(v)
                : Result<int, string>.Failure($"bad: {s}"));

        var node = kr.ToCompatNode();
        var result = await node.Node.InvokeAsync("42");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task ToCompatNode_FromKleisliResult_FailureCase()
    {
        KleisliResult<string, int, string> kr = s =>
            Task.FromResult(Result<int, string>.Failure("nope"));

        var node = kr.ToCompatNode();
        var result = await node.Node.InvokeAsync("abc");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("nope");
    }

    [Fact]
    public void ToCompatNode_FromKleisliResult_DefaultName()
    {
        KleisliResult<string, int, string> kr = _ =>
            Task.FromResult(Result<int, string>.Success(0));

        var node = kr.ToCompatNode();

        node.Node.Name.Should().Be("KleisliResult[String->Int32]");
    }

    [Fact]
    public void ToCompatNode_FromKleisliResult_CustomName()
    {
        KleisliResult<string, int, string> kr = _ =>
            Task.FromResult(Result<int, string>.Success(0));

        var node = kr.ToCompatNode("parse");

        node.Node.Name.Should().Be("parse");
    }

    [Fact]
    public async Task ToCompatNode_FromKleisliOption_SomeCase()
    {
        KleisliOption<int, int> ko = n =>
            Task.FromResult(n > 0 ? Option<int>.Some(n) : Option<int>.None());

        var node = ko.ToCompatNode();
        var result = await node.Node.InvokeAsync(5);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task ToCompatNode_FromKleisliOption_NoneCase()
    {
        KleisliOption<int, int> ko = n =>
            Task.FromResult(n > 0 ? Option<int>.Some(n) : Option<int>.None());

        var node = ko.ToCompatNode();
        var result = await node.Node.InvokeAsync(-1);

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToCompatNode_FromKleisliOption_DefaultName()
    {
        KleisliOption<int, string> ko = _ =>
            Task.FromResult(Option<string>.None());

        var node = ko.ToCompatNode();

        node.Node.Name.Should().Be("KleisliOption[Int32->String]");
    }

    [Fact]
    public void ToCompatNode_FromKleisliOption_CustomName()
    {
        KleisliOption<int, string> ko = _ =>
            Task.FromResult(Option<string>.None());

        var node = ko.ToCompatNode("filter");

        node.Node.Name.Should().Be("filter");
    }

    [Fact]
    public async Task ToCompatNode_FromPureFunc_ExecutesCorrectly()
    {
        Func<int, string> f = i => $"val={i}";

        var node = f.ToCompatNode();
        var result = await node.Node.InvokeAsync(7);

        result.Should().Be("val=7");
    }

    [Fact]
    public void ToCompatNode_FromPureFunc_DefaultName()
    {
        Func<int, string> f = i => i.ToString();

        var node = f.ToCompatNode();

        node.Node.Name.Should().Be("Func[Int32->String]");
    }

    [Fact]
    public void ToCompatNode_FromPureFunc_CustomName()
    {
        Func<int, string> f = i => i.ToString();

        var node = f.ToCompatNode("convert");

        node.Node.Name.Should().Be("convert");
    }

    [Fact]
    public async Task ToCompatNode_FromAsyncFunc_ExecutesCorrectly()
    {
        Func<int, Task<int>> f = async i =>
        {
            await Task.Yield();
            return i * 3;
        };

        var node = f.ToCompatNode();
        var result = await node.Node.InvokeAsync(4);

        result.Should().Be(12);
    }

    [Fact]
    public void ToCompatNode_FromAsyncFunc_DefaultName()
    {
        Func<string, Task<int>> f = s => Task.FromResult(s.Length);

        var node = f.ToCompatNode();

        node.Node.Name.Should().Be("Async[String->Int32]");
    }

    [Fact]
    public void ToCompatNode_FromAsyncFunc_CustomName()
    {
        Func<string, Task<int>> f = s => Task.FromResult(s.Length);

        var node = f.ToCompatNode("async-len");

        node.Node.Name.Should().Be("async-len");
    }

    [Fact]
    public void StartPipeline_CreatesPipelineBuilder()
    {
        var builder = CompatInterop.StartPipeline<string>("TestPipeline");

        builder.Should().NotBeNull();
        builder.Should().BeOfType<PipelineBuilder<string>>();
    }

    [Fact]
    public void StartPipeline_DefaultName()
    {
        var builder = CompatInterop.StartPipeline<int>();

        builder.Should().NotBeNull();
    }
}
