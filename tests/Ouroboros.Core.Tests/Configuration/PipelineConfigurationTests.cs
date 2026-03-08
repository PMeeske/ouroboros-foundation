using FluentAssertions;
using Ouroboros.Core.Configuration;
using Xunit;

namespace Ouroboros.Tests.Configuration;

[Trait("Category", "Unit")]
public class PipelineConfigurationTests
{
    [Fact]
    public void SectionName_ShouldBePipeline()
    {
        PipelineConfiguration.SectionName.Should().Be("Pipeline");
    }

    [Fact]
    public void Default_LlmProvider_ShouldNotBeNull()
    {
        var config = new PipelineConfiguration();
        config.LlmProvider.Should().NotBeNull();
    }

    [Fact]
    public void Default_VectorStore_ShouldNotBeNull()
    {
        var config = new PipelineConfiguration();
        config.VectorStore.Should().NotBeNull();
    }

    [Fact]
    public void Default_Execution_ShouldNotBeNull()
    {
        var config = new PipelineConfiguration();
        config.Execution.Should().NotBeNull();
    }

    [Fact]
    public void Default_Observability_ShouldNotBeNull()
    {
        var config = new PipelineConfiguration();
        config.Observability.Should().NotBeNull();
    }

    [Fact]
    public void Default_Features_ShouldNotBeNull()
    {
        var config = new PipelineConfiguration();
        config.Features.Should().NotBeNull();
    }

    [Fact]
    public void Default_Features_ShouldBeAllOff()
    {
        var config = new PipelineConfiguration();
        config.Features.AnyEnabled().Should().BeFalse();
    }

    [Fact]
    public void SetLlmProvider_ShouldPersist()
    {
        var provider = new LlmProviderConfiguration { DefaultProvider = "OpenAI" };
        var config = new PipelineConfiguration { LlmProvider = provider };
        config.LlmProvider.DefaultProvider.Should().Be("OpenAI");
    }

    [Fact]
    public void SetExecution_ShouldPersist()
    {
        var exec = new ExecutionConfiguration { MaxTurns = 20 };
        var config = new PipelineConfiguration { Execution = exec };
        config.Execution.MaxTurns.Should().Be(20);
    }
}
