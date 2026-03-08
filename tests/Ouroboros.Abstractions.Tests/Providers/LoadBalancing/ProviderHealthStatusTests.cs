using Ouroboros.Providers.LoadBalancing;

namespace Ouroboros.Abstractions.Tests.Providers.LoadBalancing;

[Trait("Category", "Unit")]
public class ProviderHealthStatusTests
{
    private static ProviderHealthStatus CreateHealthy(
        double successRate = 0.95,
        double latencyMs = 100.0) =>
        new ProviderHealthStatus(
            ProviderId: "provider-1",
            IsHealthy: true,
            SuccessRate: successRate,
            AverageLatencyMs: latencyMs,
            ConsecutiveFailures: 0,
            LastFailureTime: null,
            CooldownUntil: null,
            TotalRequests: 1000,
            SuccessfulRequests: (int)(1000 * successRate),
            LastChecked: DateTime.UtcNow);

    [Fact]
    public void IsInCooldown_NoCooldown_ReturnsFalse()
    {
        // Arrange
        var status = CreateHealthy();

        // Assert
        status.IsInCooldown.Should().BeFalse();
    }

    [Fact]
    public void IsInCooldown_FutureCooldown_ReturnsTrue()
    {
        // Arrange
        var status = CreateHealthy() with
        {
            CooldownUntil = DateTime.UtcNow.AddMinutes(5)
        };

        // Assert
        status.IsInCooldown.Should().BeTrue();
    }

    [Fact]
    public void IsInCooldown_PastCooldown_ReturnsFalse()
    {
        // Arrange
        var status = CreateHealthy() with
        {
            CooldownUntil = DateTime.UtcNow.AddMinutes(-5)
        };

        // Assert
        status.IsInCooldown.Should().BeFalse();
    }

    [Fact]
    public void HealthScore_HealthyProvider_ReturnsPositiveScore()
    {
        // Arrange
        var status = CreateHealthy(successRate: 0.95, latencyMs: 100.0);

        // Act
        var score = status.HealthScore;

        // Assert
        score.Should().BeGreaterThan(0.0);
        score.Should().BeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public void HealthScore_UnhealthyProvider_ReturnsZero()
    {
        // Arrange
        var status = CreateHealthy() with { IsHealthy = false };

        // Assert
        status.HealthScore.Should().Be(0.0);
    }

    [Fact]
    public void HealthScore_InCooldown_ReturnsZero()
    {
        // Arrange
        var status = CreateHealthy() with
        {
            CooldownUntil = DateTime.UtcNow.AddMinutes(5)
        };

        // Assert
        status.HealthScore.Should().Be(0.0);
    }

    [Fact]
    public void HealthScore_HighSuccessLowLatency_NearOne()
    {
        // Arrange
        var status = CreateHealthy(successRate: 1.0, latencyMs: 0.0);

        // Assert
        status.HealthScore.Should().BeGreaterThanOrEqualTo(0.9);
    }

    [Fact]
    public void HealthScore_HighLatency_ReducesScore()
    {
        // Arrange
        var lowLatency = CreateHealthy(successRate: 0.9, latencyMs: 100.0);
        var highLatency = CreateHealthy(successRate: 0.9, latencyMs: 9000.0);

        // Assert
        lowLatency.HealthScore.Should().BeGreaterThan(highLatency.HealthScore);
    }

    [Fact]
    public void AllPropertiesSet()
    {
        // Act
        var status = new ProviderHealthStatus(
            "p1", true, 0.95, 200.0, 0, null, null, 500, 475, DateTime.UtcNow);

        // Assert
        status.ProviderId.Should().Be("p1");
        status.IsHealthy.Should().BeTrue();
        status.SuccessRate.Should().Be(0.95);
        status.AverageLatencyMs.Should().Be(200.0);
        status.ConsecutiveFailures.Should().Be(0);
        status.TotalRequests.Should().Be(500);
        status.SuccessfulRequests.Should().Be(475);
    }
}
