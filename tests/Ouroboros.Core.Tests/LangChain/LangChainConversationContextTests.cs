using Ouroboros.Core.LangChain;

namespace Ouroboros.Core.Tests.LangChain;

[Trait("Category", "Unit")]
public class LangChainConversationContextTests
{
    [Fact]
    public void Constructor_Default_CreatesContext()
    {
        var context = new LangChainConversationContext();

        context.Should().NotBeNull();
        context.GetConversationHistory().Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithMaxTurns_CreatesContext()
    {
        var context = new LangChainConversationContext(maxTurns: 5);

        context.Should().NotBeNull();
    }

    [Fact]
    public void SetProperty_ReturnsSameInstance()
    {
        var context = new LangChainConversationContext();

        var result = context.SetProperty("key1", "value1");

        result.Should().BeSameAs(context);
    }

    [Fact]
    public void GetProperty_ExistingKey_ReturnsValue()
    {
        var context = new LangChainConversationContext();
        context.SetProperty("name", "test-value");

        string? value = context.GetProperty<string>("name");

        value.Should().Be("test-value");
    }

    [Fact]
    public void GetProperty_NonExistentKey_ReturnsDefault()
    {
        var context = new LangChainConversationContext();

        string? value = context.GetProperty<string>("missing");

        value.Should().BeNull();
    }

    [Fact]
    public void GetProperty_WrongType_ReturnsDefault()
    {
        var context = new LangChainConversationContext();
        context.SetProperty("key", "string-value");

        int value = context.GetProperty<int>("key");

        value.Should().Be(0);
    }

    [Fact]
    public void AddTurn_AddsToHistory()
    {
        var context = new LangChainConversationContext();

        context.AddTurn("Hello", "Hi there");

        string history = context.GetConversationHistory();
        history.Should().Contain("Hello");
        history.Should().Contain("Hi there");
    }

    [Fact]
    public void GetProperties_ReturnsCopy()
    {
        var context = new LangChainConversationContext();
        context.SetProperty("a", 1);
        context.SetProperty("b", 2);

        var props = context.GetProperties();

        props.Should().HaveCount(2);
        props["a"].Should().Be(1);
        props["b"].Should().Be(2);
    }

    [Fact]
    public void GetProperties_ReturnsNewDictionary()
    {
        var context = new LangChainConversationContext();
        context.SetProperty("key", "value");

        var props1 = context.GetProperties();
        var props2 = context.GetProperties();

        props1.Should().NotBeSameAs(props2);
    }

    [Fact]
    public void SetProperty_Overwrites_ExistingKey()
    {
        var context = new LangChainConversationContext();
        context.SetProperty("key", "original");
        context.SetProperty("key", "updated");

        context.GetProperty<string>("key").Should().Be("updated");
    }
}

[Trait("Category", "Unit")]
public class LangChainConversationPipelineTests
{
    [Fact]
    public void Create_ReturnsNewPipeline()
    {
        var pipeline = LangChainConversationPipeline.Create();

        pipeline.Should().NotBeNull();
    }

    [Fact]
    public async Task RunAsync_EmptyPipeline_ReturnsSameContext()
    {
        var pipeline = LangChainConversationPipeline.Create();
        var context = new LangChainConversationContext();
        context.SetProperty("input", "test");

        var result = await pipeline.RunAsync(context);

        result.GetProperty<string>("input").Should().Be("test");
    }

    [Fact]
    public async Task AddStep_ExecutesStep()
    {
        var pipeline = LangChainConversationPipeline.Create()
            .AddStep(ctx =>
            {
                ctx.SetProperty("processed", true);
                return Task.FromResult(ctx);
            });

        var context = new LangChainConversationContext();
        var result = await pipeline.RunAsync(context);

        result.GetProperty<bool>("processed").Should().BeTrue();
    }

    [Fact]
    public async Task AddTransformation_ExecutesSynchronousTransform()
    {
        var pipeline = LangChainConversationPipeline.Create()
            .AddTransformation(ctx => ctx.SetProperty("transformed", "yes"));

        var context = new LangChainConversationContext();
        var result = await pipeline.RunAsync(context);

        result.GetProperty<string>("transformed").Should().Be("yes");
    }

    [Fact]
    public async Task SetProperty_SetsPropertyInPipeline()
    {
        var pipeline = LangChainConversationPipeline.Create()
            .SetProperty("greeting", "hello");

        var context = new LangChainConversationContext();
        var result = await pipeline.RunAsync(context);

        result.GetProperty<string>("greeting").Should().Be("hello");
    }

    [Fact]
    public async Task WithConversationHistory_AddsHistoryToContext()
    {
        var context = new LangChainConversationContext();
        context.AddTurn("User says hi", "Bot responds");

        var pipeline = LangChainConversationPipeline.Create()
            .WithConversationHistory();

        var result = await pipeline.RunAsync(context);

        result.GetProperty<string>("conversation_history").Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task WithConversationHistory_NoHistory_DoesNotSetProperty()
    {
        var context = new LangChainConversationContext();

        var pipeline = LangChainConversationPipeline.Create()
            .WithConversationHistory();

        var result = await pipeline.RunAsync(context);

        result.GetProperty<string>("conversation_history").Should().BeNull();
    }

    [Fact]
    public async Task AddStep_MultiplePipelineSteps_ExecuteInOrder()
    {
        var order = new List<int>();

        var pipeline = LangChainConversationPipeline.Create()
            .AddStep(ctx => { order.Add(1); return Task.FromResult(ctx); })
            .AddStep(ctx => { order.Add(2); return Task.FromResult(ctx); })
            .AddStep(ctx => { order.Add(3); return Task.FromResult(ctx); });

        await pipeline.RunAsync(new LangChainConversationContext());

        order.Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public void CreateConversationPipeline_ReturnsNewPipeline()
    {
        var pipeline = LangChainConversationBuilder.CreateConversationPipeline();

        pipeline.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAiResponseGeneration_WithFunc_ExecutesGenerator()
    {
        var pipeline = LangChainConversationPipeline.Create()
            .SetProperty("input", "hello")
            .AddAiResponseGeneration(input => Task.FromResult($"Response to: {input}"));

        var context = new LangChainConversationContext();
        var result = await pipeline.RunAsync(context);

        result.GetProperty<string>("text").Should().Contain("Response to:");
    }
}

[Trait("Category", "Unit")]
public class LangChainMemoryExtensionsTests
{
    [Fact]
    public void WithLangChainMemory_String_CreatesContextWithInput()
    {
        var context = "hello".WithLangChainMemory();

        context.Should().NotBeNull();
        context.GetProperty<string>("input").Should().Be("hello");
    }

    [Fact]
    public void WithLangChainMemory_Int_CreatesContextWithInput()
    {
        var context = 42.WithLangChainMemory();

        context.Should().NotBeNull();
        context.GetProperty<int>("input").Should().Be(42);
    }

    [Fact]
    public void WithLangChainMemory_WithMaxTurns_CreatesContext()
    {
        var context = "test".WithLangChainMemory(maxTurns: 10);

        context.Should().NotBeNull();
    }

    [Fact]
    public void WithLangChainMemory_NullInput_DoesNotSetProperty()
    {
        string? nullInput = null;
        var context = nullInput.WithLangChainMemory();

        context.GetProperty<string>("input").Should().BeNull();
    }
}
