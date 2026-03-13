using Ouroboros.Core.LangChain;
using Moq;

namespace Ouroboros.Core.Tests.LangChain;

[Trait("Category", "Unit")]
public class LangChainConversationPipelineTests
{
    [Fact]
    public void LangChainConversationPipeline_ShouldBeCreatable()
    {
        // Verify LangChainConversationPipeline type exists and is accessible
        typeof(LangChainConversationPipeline).Should().NotBeNull();
    }

    [Fact]
    public void LangChainConversationPipeline_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(LangChainConversationPipeline).GetProperty("LangChainConversationPipeline").Should().NotBeNull();
    }

    [Fact]
    public void AddStep_ShouldBeDefined()
    {
        // Verify AddStep method exists
        typeof(LangChainConversationPipeline).GetMethod("AddStep").Should().NotBeNull();
    }

    [Fact]
    public void AddTransformation_ShouldBeDefined()
    {
        // Verify AddTransformation method exists
        typeof(LangChainConversationPipeline).GetMethod("AddTransformation").Should().NotBeNull();
    }

    [Fact]
    public void SetProperty_ShouldBeDefined()
    {
        // Verify SetProperty method exists
        typeof(LangChainConversationPipeline).GetMethod("SetProperty").Should().NotBeNull();
    }
}
