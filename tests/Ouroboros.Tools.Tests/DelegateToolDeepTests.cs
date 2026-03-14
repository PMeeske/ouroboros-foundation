namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Deep tests for DelegateTool covering edge cases in all constructors,
/// FromJson patterns, and cancellation behavior.
/// </summary>
[Trait("Category", "Unit")]
public class DelegateToolDeepTests
{
    #region Sync Constructor - Edge Cases

    [Fact]
    public async Task SyncConstructor_ReturnsEmptyString_SuccessWithEmpty()
    {
        var tool = new DelegateTool("t", "T", (string s) => string.Empty);

        var result = await tool.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task SyncConstructor_ReturnsNull_SuccessWithNull()
    {
        var tool = new DelegateTool("t", "T", (string s) => (string)null!);

        var result = await tool.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SyncConstructor_EmptyInput_PassesEmptyString()
    {
        string receivedInput = null!;
        var tool = new DelegateTool("t", "T", (string s) =>
        {
            receivedInput = s;
            return s;
        });

        await tool.InvokeAsync("");

        receivedInput.Should().BeEmpty();
    }

    [Fact]
    public async Task SyncConstructor_LargeInput_HandlesCorrectly()
    {
        var largeInput = new string('x', 10_000);
        var tool = new DelegateTool("t", "T", (string s) => s);

        var result = await tool.InvokeAsync(largeInput);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveLength(10_000);
    }

    #endregion

    #region Async Constructor - Edge Cases

    [Fact]
    public async Task AsyncConstructor_DelayedResult_ReturnsCorrectly()
    {
        var tool = new DelegateTool("t", "T", async (string s) =>
        {
            await Task.Delay(1);
            return $"delayed:{s}";
        });

        var result = await tool.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("delayed:input");
    }

    [Fact]
    public async Task AsyncConstructor_TaskCanceled_ReturnsFailure()
    {
        var tool = new DelegateTool("t", "T",
            (string s) => Task.FromCanceled<string>(new CancellationToken(true)));

        // OperationCanceledException is rethrown, not caught
        Func<Task> act = () => tool.InvokeAsync("input");

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task AsyncConstructor_AggregateException_CapturedAsFailure()
    {
        var tool = new DelegateTool("t", "T",
            (string s) => Task.FromException<string>(
                new InvalidOperationException("aggregate inner")));

        var result = await tool.InvokeAsync("input");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("aggregate inner");
    }

    #endregion

    #region Full Constructor - CancellationToken

    [Fact]
    public async Task FullConstructor_CancelledToken_ThrowsOperationCanceledException()
    {
        var tool = new DelegateTool("t", "T", async (string s, CancellationToken ct) =>
        {
            ct.ThrowIfCancellationRequested();
            return Result<string, string>.Success(s);
        });

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Func<Task> act = () => tool.InvokeAsync("input", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task FullConstructor_SuccessResult_ReturnsSuccess()
    {
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Success($"ok:{s}")));

        var result = await tool.InvokeAsync("data");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok:data");
    }

    [Fact]
    public async Task FullConstructor_FailureResult_ReturnsFailure()
    {
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("custom error")));

        var result = await tool.InvokeAsync("data");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("custom error");
    }

    #endregion

    #region FromJson - Edge Cases

    [Fact]
    public async Task FromJson_NestedJson_Parses()
    {
        var tool = DelegateTool.FromJson<NestedInput>("t", "T",
            async args => $"name={args.Name},nested={args.Inner?.Key}");

        var result = await tool.InvokeAsync("""{"Name":"A","Inner":{"Key":"B"}}""");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("name=A");
        result.Value.Should().Contain("nested=B");
    }

    [Fact]
    public async Task FromJson_MissingOptionalFields_DefaultsApplied()
    {
        var tool = DelegateTool.FromJson<OptionalInput>("t", "T",
            async args => $"value={args.Value},count={args.Count}");

        var result = await tool.InvokeAsync("""{}""");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("count=0");
    }

    [Fact]
    public async Task FromJson_EmptyJsonObject_ParsesWithDefaults()
    {
        var tool = DelegateTool.FromJson<OptionalInput>("t", "T",
            async args => $"v={args.Value}");

        var result = await tool.InvokeAsync("{}");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task FromJson_FunctionThrows_ReturnsFailure()
    {
        var tool = DelegateTool.FromJson<OptionalInput>("t", "T",
            args => throw new InvalidOperationException("fn error"));

        var result = await tool.InvokeAsync("{}");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void FromJson_GeneratesSchema()
    {
        var tool = DelegateTool.FromJson<OptionalInput>("t", "T",
            async args => "ok");

        tool.JsonSchema.Should().NotBeNullOrEmpty();
        tool.JsonSchema.Should().Contain("Value");
    }

    #endregion

    #region Schema Property

    [Fact]
    public void DelegateTool_WithExplicitSchema_ReturnsSchema()
    {
        var schema = """{"type":"object","properties":{"x":{"type":"string"}}}""";
        var tool = new DelegateTool("t", "T",
            (s, ct) => Task.FromResult(Result<string, string>.Success(s)), schema);

        tool.JsonSchema.Should().Be(schema);
    }

    [Fact]
    public void DelegateTool_WithoutSchema_ReturnsNull()
    {
        var tool = new DelegateTool("t", "T", (string s) => s);

        tool.JsonSchema.Should().BeNull();
    }

    #endregion

    #region Helper Types

    private record NestedInput
    {
        public string Name { get; init; } = "";
        public InnerInput? Inner { get; init; }
    }

    private record InnerInput
    {
        public string Key { get; init; } = "";
    }

    private record OptionalInput
    {
        public string Value { get; init; } = "";
        public int Count { get; init; }
    }

    #endregion
}
