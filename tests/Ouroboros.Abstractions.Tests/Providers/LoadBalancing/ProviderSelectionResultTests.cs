using Ouroboros.Providers.LoadBalancing;

namespace Ouroboros.Abstractions.Tests.Providers.LoadBalancing;

[Trait("Category", "Unit")]
public class ProviderSelectionResultTests
{
    [Fact]
    public void ProviderSelectionResult_AllPropertiesSet()
    {
        // Arrange
        var health = new ProviderHealthStatus(
            "p1", true, 0.95, 100.0, 0, null, null, 1000, 950, DateTime.UtcNow);

        // Act
        var result = new ProviderSelectionResult<string>(
            Provider: "my-provider",
            ProviderId: "p1",
            Strategy: "round-robin",
            Reason: "Best available",
            Health: health);

        // Assert
        result.Provider.Should().Be("my-provider");
        result.ProviderId.Should().Be("p1");
        result.Strategy.Should().Be("round-robin");
        result.Reason.Should().Be("Best available");
        result.Health.Should().Be(health);
    }

    [Fact]
    public void ProviderSelectionResult_WithIntProvider_WorksWithGenericType()
    {
        // Arrange
        var health = new ProviderHealthStatus(
            "p2", true, 1.0, 50.0, 0, null, null, 100, 100, DateTime.UtcNow);

        // Act
        var result = new ProviderSelectionResult<int>(
            42, "p2", "weighted", "Highest score", health);

        // Assert
        result.Provider.Should().Be(42);
    }

    [Fact]
    public void ProviderSelectionResult_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var health = new ProviderHealthStatus(
            "p1", true, 0.9, 100.0, 0, null, null, 100, 90, DateTime.UtcNow);

        var a = new ProviderSelectionResult<string>("prov", "p1", "rr", "reason", health);
        var b = new ProviderSelectionResult<string>("prov", "p1", "rr", "reason", health);

        // Assert
        a.Should().Be(b);
    }
}
