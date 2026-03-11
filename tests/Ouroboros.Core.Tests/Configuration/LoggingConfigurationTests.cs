using Microsoft.Extensions.Configuration;
using Ouroboros.Core.Configuration;
using Serilog;

namespace Ouroboros.Core.Tests.Configuration;

[Trait("Category", "Unit")]
public class LoggingConfigurationTests
{
    [Fact]
    public void CreateDefaultLogger_Production_ReturnsLogger()
    {
        ILogger logger = LoggingConfiguration.CreateDefaultLogger("Production");

        logger.Should().NotBeNull();
    }

    [Fact]
    public void CreateDefaultLogger_Development_ReturnsLogger()
    {
        ILogger logger = LoggingConfiguration.CreateDefaultLogger("Development");

        logger.Should().NotBeNull();
    }

    [Fact]
    public void CreateDefaultLogger_NoArgument_ReturnsLogger()
    {
        ILogger logger = LoggingConfiguration.CreateDefaultLogger();

        logger.Should().NotBeNull();
    }

    [Fact]
    public void CreateLogger_WithConfiguration_ReturnsLogger()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Serilog:MinimumLevel:Default", "Information" }
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        ILogger logger = LoggingConfiguration.CreateLogger(configuration);

        logger.Should().NotBeNull();
    }

    [Fact]
    public void CreateLogger_WithPipelineConfig_ReturnsLogger()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Serilog:MinimumLevel:Default", "Information" }
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var pipelineConfig = new PipelineConfiguration
        {
            Observability = new ObservabilityConfiguration
            {
                MinimumLogLevel = "DEBUG"
            }
        };

        ILogger logger = LoggingConfiguration.CreateLogger(configuration, pipelineConfig);

        logger.Should().NotBeNull();
    }

    [Theory]
    [InlineData("VERBOSE")]
    [InlineData("TRACE")]
    [InlineData("DEBUG")]
    [InlineData("INFORMATION")]
    [InlineData("INFO")]
    [InlineData("WARNING")]
    [InlineData("WARN")]
    [InlineData("ERROR")]
    [InlineData("FATAL")]
    [InlineData("CRITICAL")]
    [InlineData("UNKNOWN")]
    public void CreateLogger_WithVariousLogLevels_ReturnsLogger(string logLevel)
    {
        var inMemorySettings = new Dictionary<string, string?>();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var pipelineConfig = new PipelineConfiguration
        {
            Observability = new ObservabilityConfiguration
            {
                MinimumLogLevel = logLevel
            }
        };

        ILogger logger = LoggingConfiguration.CreateLogger(configuration, pipelineConfig);

        logger.Should().NotBeNull();
    }
}
