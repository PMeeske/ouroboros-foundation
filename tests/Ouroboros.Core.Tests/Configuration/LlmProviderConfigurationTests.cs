using FluentAssertions;
using Ouroboros.Core.Configuration;
using Xunit;

namespace Ouroboros.Tests.Configuration;

[Trait("Category", "Unit")]
public class LlmProviderConfigurationTests
{
    [Fact]
    public void Default_DefaultProvider_ShouldBeOllama()
    {
        var config = new LlmProviderConfiguration();
        config.DefaultProvider.Should().Be("Ollama");
    }

    [Fact]
    public void Default_OllamaEndpoint_ShouldBeLocalhost()
    {
        var config = new LlmProviderConfiguration();
        config.OllamaEndpoint.Should().Be("http://localhost:11434");
    }

    [Fact]
    public void Default_DefaultEmbeddingModel_ShouldBeNomicEmbedText()
    {
        var config = new LlmProviderConfiguration();
        config.DefaultEmbeddingModel.Should().Be("nomic-embed-text");
    }

    [Fact]
    public void Default_OpenAiApiKey_ShouldBeNull()
    {
        var config = new LlmProviderConfiguration();
        config.OpenAiApiKey.Should().BeNull();
    }

    [Fact]
    public void Default_RequestTimeoutSeconds_ShouldBe120()
    {
        var config = new LlmProviderConfiguration();
        config.RequestTimeoutSeconds.Should().Be(120);
    }

    [Fact]
    public void SetDefaultProvider_ShouldPersist()
    {
        var config = new LlmProviderConfiguration { DefaultProvider = "OpenAI" };
        config.DefaultProvider.Should().Be("OpenAI");
    }

    [Fact]
    public void SetOpenAiApiKey_ShouldPersist()
    {
        var config = new LlmProviderConfiguration { OpenAiApiKey = "sk-test" };
        config.OpenAiApiKey.Should().Be("sk-test");
    }
}
