using Ouroboros.Agent;

namespace Ouroboros.Abstractions.Tests.Agent;

[Trait("Category", "Unit")]
public class OrchestratorConfigTests
{
    [Fact]
    public void Default_ReturnsDefaultConfiguration()
    {
        // Act
        var config = OrchestratorConfig.Default();

        // Assert
        config.EnableTracing.Should().BeTrue();
        config.EnableMetrics.Should().BeTrue();
        config.EnableSafetyChecks.Should().BeTrue();
        config.ExecutionTimeout.Should().BeNull();
        config.RetryConfig.Should().BeNull();
        config.CustomSettings.Should().BeEmpty();
    }

    [Fact]
    public void GetSetting_ExistingKey_ReturnsValue()
    {
        // Arrange
        var config = new OrchestratorConfig
        {
            CustomSettings = new Dictionary<string, object> { ["key1"] = "value1" }
        };

        // Act & Assert
        config.GetSetting<string>("key1").Should().Be("value1");
    }

    [Fact]
    public void GetSetting_NonExistingKey_ReturnsDefault()
    {
        // Arrange
        var config = OrchestratorConfig.Default();

        // Act & Assert
        config.GetSetting("nonexistent", "fallback").Should().Be("fallback");
    }

    [Fact]
    public void GetSetting_WrongType_ReturnsDefault()
    {
        // Arrange
        var config = new OrchestratorConfig
        {
            CustomSettings = new Dictionary<string, object> { ["key"] = 42 }
        };

        // Act & Assert
        config.GetSetting<string>("key", "fallback").Should().Be("fallback");
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = OrchestratorConfig.Default();

        // Act
        var modified = original with { EnableTracing = false };

        // Assert
        modified.EnableTracing.Should().BeFalse();
        modified.EnableMetrics.Should().BeTrue();
    }

    [Fact]
    public void ExecutionTimeout_CanBeSet()
    {
        // Act
        var config = new OrchestratorConfig
        {
            ExecutionTimeout = TimeSpan.FromSeconds(30)
        };

        // Assert
        config.ExecutionTimeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void RetryConfig_CanBeSet()
    {
        // Act
        var config = new OrchestratorConfig
        {
            RetryConfig = RetryConfig.Default()
        };

        // Assert
        config.RetryConfig.Should().NotBeNull();
        config.RetryConfig!.MaxRetries.Should().Be(3);
    }
}
