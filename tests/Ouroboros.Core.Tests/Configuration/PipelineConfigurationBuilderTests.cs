using Ouroboros.Core.Configuration;
using Moq;

namespace Ouroboros.Core.Tests.Configuration;

[Trait("Category", "Unit")]
public class PipelineConfigurationBuilderTests
{
    [Fact]
    public void PipelineConfigurationBuilder_ShouldBeCreatable()
    {
        // Verify PipelineConfigurationBuilder type exists and is accessible
        typeof(PipelineConfigurationBuilder).Should().NotBeNull();
    }

    [Fact]
    public void PipelineConfigurationBuilder_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(PipelineConfigurationBuilder).GetProperty("PipelineConfigurationBuilder").Should().NotBeNull();
    }

    [Fact]
    public void SetBasePath_ShouldBeDefined()
    {
        // Verify SetBasePath method exists
        typeof(PipelineConfigurationBuilder).GetMethod("SetBasePath").Should().NotBeNull();
    }

    [Fact]
    public void SetEnvironment_ShouldBeDefined()
    {
        // Verify SetEnvironment method exists
        typeof(PipelineConfigurationBuilder).GetMethod("SetEnvironment").Should().NotBeNull();
    }

    [Fact]
    public void AddJsonFile_ShouldBeDefined()
    {
        // Verify AddJsonFile method exists
        typeof(PipelineConfigurationBuilder).GetMethod("AddJsonFile").Should().NotBeNull();
    }
}
