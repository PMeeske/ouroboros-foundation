using Ouroboros.Core.Configuration;

namespace Ouroboros.Core.Tests.Configuration;

[Trait("Category", "Unit")]
public class PipelineConfigurationBuilderTests
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        var builder = new PipelineConfigurationBuilder();
        builder.Should().NotBeNull();
    }

    [Fact]
    public void SetEnvironment_ReturnsSelf()
    {
        var builder = new PipelineConfigurationBuilder();
        var result = builder.SetEnvironment("Development");
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddEnvironmentVariables_WithPrefix_ReturnsSelf()
    {
        var builder = new PipelineConfigurationBuilder();
        var result = builder.AddEnvironmentVariables("TEST_");
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddEnvironmentVariables_WithoutPrefix_ReturnsSelf()
    {
        var builder = new PipelineConfigurationBuilder();
        var result = builder.AddEnvironmentVariables();
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void Build_ReturnsConfig()
    {
        var builder = new PipelineConfigurationBuilder();
        var config = builder.Build();
        config.Should().NotBeNull();
    }

    [Fact]
    public void BuildConfiguration_ReturnsIConfiguration()
    {
        var builder = new PipelineConfigurationBuilder();
        var config = builder.BuildConfiguration();
        config.Should().NotBeNull();
    }

    [Fact]
    public void CreateDefault_WithNoArgs_ReturnsBuilder()
    {
        var builder = PipelineConfigurationBuilder.CreateDefault();
        builder.Should().NotBeNull();
    }

    [Fact]
    public void CreateDefault_WithEnvironment_ReturnsBuilder()
    {
        var builder = PipelineConfigurationBuilder.CreateDefault(environmentName: "Test");
        builder.Should().NotBeNull();
    }

    [Fact]
    public void FluentChaining_Works()
    {
        var builder = new PipelineConfigurationBuilder()
            .SetEnvironment("Development")
            .AddEnvironmentVariables("APP_");

        builder.Should().NotBeNull();
        var config = builder.Build();
        config.Should().NotBeNull();
    }
}
