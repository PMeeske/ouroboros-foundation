using Microsoft.Extensions.Configuration;
using Ouroboros.Core.Configuration;
using Serilog;

namespace Ouroboros.Core.Tests.Configuration;

/// <summary>
/// Additional tests for LoggingConfiguration and PipelineConfigurationBuilder
/// to cover remaining uncovered lines.
/// </summary>
[Trait("Category", "Unit")]
public class LoggingConfigurationAdditionalTests
{
    [Fact]
    public void CreateLogger_WithNullPipelineConfig_ReturnsLogger()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        ILogger logger = LoggingConfiguration.CreateLogger(config, null);

        logger.Should().NotBeNull();
    }

    [Fact]
    public void CreateLogger_PipelineConfigWithNullObservability_ReturnsLogger()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var pipelineConfig = new PipelineConfiguration { Observability = null };

        ILogger logger = LoggingConfiguration.CreateLogger(config, pipelineConfig);

        logger.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    [InlineData("Custom")]
    public void CreateDefaultLogger_VariousEnvironments_ReturnsLogger(string env)
    {
        ILogger logger = LoggingConfiguration.CreateDefaultLogger(env);
        logger.Should().NotBeNull();
    }
}

[Trait("Category", "Unit")]
public class PipelineConfigurationBuilderAdditionalTests
{
    [Fact]
    public void SetBasePath_ReturnsSelf()
    {
        var builder = new PipelineConfigurationBuilder();
        var result = builder.SetBasePath(Path.GetTempPath());
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddJsonFile_ReturnsSelf()
    {
        var builder = new PipelineConfigurationBuilder();
        builder.SetBasePath(Path.GetTempPath());
        var result = builder.AddJsonFile("nonexistent.json", optional: true);
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddJsonFile_WithReloadOnChange_ReturnsSelf()
    {
        var builder = new PipelineConfigurationBuilder();
        builder.SetBasePath(Path.GetTempPath());
        var result = builder.AddJsonFile("settings.json", optional: true, reloadOnChange: true);
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddEnvironmentConfiguration_ReturnsSelf()
    {
        var builder = new PipelineConfigurationBuilder();
        builder.SetBasePath(Path.GetTempPath());
        var result = builder.AddEnvironmentConfiguration();
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddEnvironmentConfiguration_WithReloadOnChange_ReturnsSelf()
    {
        var builder = new PipelineConfigurationBuilder();
        builder.SetBasePath(Path.GetTempPath());
        var result = builder.AddEnvironmentConfiguration(optional: true, reloadOnChange: true);
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void CreateDefault_WithBasePath_ReturnsBuilder()
    {
        var builder = PipelineConfigurationBuilder.CreateDefault(basePath: Path.GetTempPath());
        builder.Should().NotBeNull();
    }

    [Fact]
    public void CreateDefault_WithBothArgs_ReturnsBuilder()
    {
        var builder = PipelineConfigurationBuilder.CreateDefault(
            basePath: Path.GetTempPath(),
            environmentName: "Development");
        builder.Should().NotBeNull();
    }

    [Fact]
    public void CreateDefault_NullBasePath_ReturnsBuilder()
    {
        var builder = PipelineConfigurationBuilder.CreateDefault(basePath: null, environmentName: "Test");
        builder.Should().NotBeNull();
    }

    [Fact]
    public void FullChain_BuildReturnsValidConfig()
    {
        var builder = new PipelineConfigurationBuilder()
            .SetBasePath(Path.GetTempPath())
            .SetEnvironment("Development")
            .AddEnvironmentConfiguration()
            .AddEnvironmentVariables("TEST_");

        var config = builder.Build();
        config.Should().NotBeNull();

        var rawConfig = builder.BuildConfiguration();
        rawConfig.Should().NotBeNull();
    }
}
