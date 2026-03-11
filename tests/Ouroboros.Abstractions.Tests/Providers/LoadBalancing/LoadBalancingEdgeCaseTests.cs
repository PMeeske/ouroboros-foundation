using Ouroboros.Providers.LoadBalancing;

namespace Ouroboros.Abstractions.Tests.Providers.LoadBalancing;

/// <summary>
/// Additional tests for LoadBalancing types covering record equality,
/// with-expressions, and edge cases.
/// </summary>
[Trait("Category", "Unit")]
public class LoadBalancingEdgeCaseTests
{
    [Fact]
    public void ProviderHealthStatus_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var ts = DateTime.UtcNow;
        var a = new ProviderHealthStatus("p1", true, 0.95, 100.0, 0, null, null, 1000, 950, ts);
        var b = new ProviderHealthStatus("p1", true, 0.95, 100.0, 0, null, null, 1000, 950, ts);

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ProviderHealthStatus_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new ProviderHealthStatus(
            "p1", true, 0.95, 100.0, 0, null, null, 1000, 950, DateTime.UtcNow);

        // Act
        var modified = original with { IsHealthy = false, ConsecutiveFailures = 3 };

        // Assert
        modified.IsHealthy.Should().BeFalse();
        modified.ConsecutiveFailures.Should().Be(3);
        modified.ProviderId.Should().Be("p1");
        modified.SuccessRate.Should().Be(0.95);
    }

    [Fact]
    public void ProviderHealthStatus_UnhealthyProvider_Properties()
    {
        // Arrange
        var lastError = DateTime.UtcNow;
        var lastSuccess = DateTime.UtcNow.AddMinutes(-30);

        // Act
        var status = new ProviderHealthStatus(
            "failing-provider", false, 0.2, 5000.0, 10,
            lastError, lastSuccess, 100, 20, DateTime.UtcNow);

        // Assert
        status.IsHealthy.Should().BeFalse();
        status.SuccessRate.Should().Be(0.2);
        status.ConsecutiveFailures.Should().Be(10);
        status.LastErrorAt.Should().Be(lastError);
        status.LastSuccessAt.Should().Be(lastSuccess);
        status.TotalRequests.Should().Be(100);
        status.SuccessfulRequests.Should().Be(20);
    }

    [Fact]
    public void ProviderSelectionResult_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var health = new ProviderHealthStatus(
            "p1", true, 0.95, 100.0, 0, null, null, 1000, 950, DateTime.UtcNow);

        var original = new ProviderSelectionResult<string>(
            "provider-a", "p1", "round-robin", "Selected", health);

        // Act
        var modified = original with { Strategy = "weighted" };

        // Assert
        modified.Strategy.Should().Be("weighted");
        modified.Provider.Should().Be("provider-a");
    }
}
