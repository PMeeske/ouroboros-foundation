using Ouroboros.Agent;

namespace Ouroboros.Abstractions.Tests.Agent;

[Trait("Category", "Unit")]
public class RetryConfigTests
{
    [Fact]
    public void Default_ReturnsStandardDefaults()
    {
        // Act
        var config = RetryConfig.Default();

        // Assert
        config.MaxRetries.Should().Be(3);
        config.InitialDelay.Should().Be(TimeSpan.FromMilliseconds(100));
        config.MaxDelay.Should().Be(TimeSpan.FromSeconds(10));
        config.BackoffMultiplier.Should().Be(2.0);
    }

    [Fact]
    public void ParameterlessConstructor_UsesDefaults()
    {
        // Act
        var config = new RetryConfig();

        // Assert
        config.MaxRetries.Should().Be(3);
        config.InitialDelay.Should().Be(TimeSpan.FromMilliseconds(100));
        config.MaxDelay.Should().Be(TimeSpan.FromSeconds(10));
        config.BackoffMultiplier.Should().Be(2.0);
    }

    [Fact]
    public void ParameterizedConstructor_SetsCustomValues()
    {
        // Act
        var config = new RetryConfig(
            MaxRetries: 5,
            InitialDelay: TimeSpan.FromSeconds(1),
            MaxDelay: TimeSpan.FromMinutes(1),
            BackoffMultiplier: 3.0);

        // Assert
        config.MaxRetries.Should().Be(5);
        config.InitialDelay.Should().Be(TimeSpan.FromSeconds(1));
        config.MaxDelay.Should().Be(TimeSpan.FromMinutes(1));
        config.BackoffMultiplier.Should().Be(3.0);
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = RetryConfig.Default();

        // Act
        var modified = original with { MaxRetries = 10 };

        // Assert
        modified.MaxRetries.Should().Be(10);
        modified.BackoffMultiplier.Should().Be(2.0);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = RetryConfig.Default();
        var b = RetryConfig.Default();

        // Assert
        a.Should().Be(b);
    }
}
