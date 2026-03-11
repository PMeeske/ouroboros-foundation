using Ouroboros.Core.Conversation;
using Ouroboros.Core.Memory;
using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.Conversation;

[Trait("Category", "Unit")]
public sealed class ConversationBuilderTests
{
    private static MemoryContext<string> CreateContext(string data = "hello")
        => new(data, new ConversationMemory());

    [Fact]
    public async Task Build_NoSteps_ReturnsInputUnchanged()
    {
        var builder = new ConversationBuilder<string, object>(new object());
        var pipeline = builder.Build();
        var input = CreateContext();

        var (result, logs) = await pipeline(input);

        result.Data.Should().Be("hello");
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task AddStep_AppliesContextualStep()
    {
        var builder = new ConversationBuilder<string, object>(new object());

        ContextualStep<MemoryContext<string>, MemoryContext<string>, object> step =
            (input, _) => Task.FromResult((input with { Data = input.Data.ToUpper() }, new List<string> { "uppercased" }));

        builder.AddStep(step);
        var pipeline = builder.Build();
        var input = CreateContext();

        var (result, logs) = await pipeline(input);

        result.Data.Should().Be("HELLO");
        logs.Should().ContainSingle("uppercased");
    }

    [Fact]
    public void AddStep_ReturnsSameBuilder_ForChaining()
    {
        var builder = new ConversationBuilder<string, object>(new object());

        ContextualStep<MemoryContext<string>, MemoryContext<string>, object> step =
            (input, _) => Task.FromResult((input, new List<string>()));

        var returned = builder.AddStep(step);
        returned.Should().BeSameAs(builder);
    }

    [Fact]
    public async Task AddProcessor_AppliesProcessorFunction()
    {
        var builder = new ConversationBuilder<string, string>("context-value");

        builder.AddProcessor(
            (input, ctx) => Task.FromResult(input with { Data = $"{input.Data}-{ctx}" }),
            "processed");

        var pipeline = builder.Build();
        var input = CreateContext("data");

        var (result, logs) = await pipeline(input);

        result.Data.Should().Be("data-context-value");
        logs.Should().ContainSingle("processed");
    }

    [Fact]
    public async Task AddProcessor_NoLogMessage_EmptyLogs()
    {
        var builder = new ConversationBuilder<string, object>(new object());

        builder.AddProcessor(
            (input, _) => Task.FromResult(input with { Data = input.Data + "!" }));

        var pipeline = builder.Build();
        var input = CreateContext("hi");

        var (result, logs) = await pipeline(input);

        result.Data.Should().Be("hi!");
        logs.Should().BeEmpty();
    }

    [Fact]
    public void AddProcessor_ReturnsSameBuilder_ForChaining()
    {
        var builder = new ConversationBuilder<string, object>(new object());

        var returned = builder.AddProcessor(
            (input, _) => Task.FromResult(input));

        returned.Should().BeSameAs(builder);
    }

    [Fact]
    public async Task AddTransformation_AppliesSyncTransformation()
    {
        var builder = new ConversationBuilder<string, object>(new object());

        builder.AddTransformation(
            input => input with { Data = input.Data.ToUpper() },
            "transformed");

        var pipeline = builder.Build();
        var input = CreateContext("test");

        var (result, logs) = await pipeline(input);

        result.Data.Should().Be("TEST");
        logs.Should().ContainSingle("transformed");
    }

    [Fact]
    public void AddTransformation_ReturnsSameBuilder_ForChaining()
    {
        var builder = new ConversationBuilder<string, object>(new object());

        var returned = builder.AddTransformation(input => input);
        returned.Should().BeSameAs(builder);
    }

    [Fact]
    public async Task MultipleSteps_AreAppliedInOrder()
    {
        var builder = new ConversationBuilder<string, object>(new object());

        builder.AddTransformation(input => input with { Data = input.Data + "1" }, "step1");
        builder.AddTransformation(input => input with { Data = input.Data + "2" }, "step2");
        builder.AddTransformation(input => input with { Data = input.Data + "3" }, "step3");

        var pipeline = builder.Build();
        var input = CreateContext("x");

        var (result, logs) = await pipeline(input);

        result.Data.Should().Be("x123");
        logs.Should().HaveCount(3);
        logs.Should().ContainInOrder("step1", "step2", "step3");
    }

    [Fact]
    public async Task BuildAndRun_ReturnsOnlyResult()
    {
        var builder = new ConversationBuilder<string, object>(new object());
        builder.AddTransformation(input => input with { Data = input.Data.ToUpper() }, "log");

        var pipeline = builder.BuildAndRun();
        var input = CreateContext("test");

        var result = await pipeline(input);
        result.Data.Should().Be("TEST");
    }

    [Fact]
    public async Task RunAsync_ExecutesPipeline()
    {
        var builder = new ConversationBuilder<string, object>(new object());
        builder.AddTransformation(input => input with { Data = input.Data + "!" });

        var input = CreateContext("hello");
        var result = await builder.RunAsync(input);

        result.Data.Should().Be("hello!");
    }

    [Fact]
    public async Task Build_AccumulatesLogsFromAllSteps()
    {
        var builder = new ConversationBuilder<string, object>(new object());

        ContextualStep<MemoryContext<string>, MemoryContext<string>, object> step =
            (input, _) => Task.FromResult((input, new List<string> { "log-a", "log-b" }));

        builder.AddStep(step);
        builder.AddTransformation(input => input, "log-c");

        var pipeline = builder.Build();
        var input = CreateContext();

        var (_, logs) = await pipeline(input);
        logs.Should().HaveCount(3);
        logs.Should().ContainInOrder("log-a", "log-b", "log-c");
    }
}
