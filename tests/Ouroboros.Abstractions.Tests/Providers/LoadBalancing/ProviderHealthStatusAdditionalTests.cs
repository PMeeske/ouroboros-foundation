using Ouroboros.Providers.LoadBalancing;

namespace Ouroboros.Abstractions.Tests.Providers.LoadBalancing;

/// <summary>
/// Additional tests for ProviderHealthStatus covering HealthScore edge cases:
/// latency > 10000 (negative latencyScore clamped via Math.Max), exact boundary
/// conditions, and computed property combinations.
/// </summary>
[Trait("Category", "Unit")]
public class ProviderHealthStatusAdditionalTests
{
    [Fact]
    public void HealthScore_VeryHighLatency_ClampsLatencyScoreToZero()
    {
        // Arrange - latency > 10000 produces negative raw latencyScore,
        // but Math.Max(0, ...) clamps it to 0
        var status = new ProviderHealthStatus(
            "p1", true, 0.9, 15000.0, 0, null, null, 100, 90, DateTime.UtcNow);

        // Act
        var score = status.HealthScore;

        // Assert - with latencyScore=0, score = SuccessRate * 0.7 + 0 * 0.3 = 0.63
        score.Should().BeApproximately(0.63, 0.01);
    }

    [Fact]
    public void HealthScore_ExactlyTenThousandLatency_LatencyScoreIsZero()
    {
        // Arrange - latency = 10000 => latencyScore = Max(0, 1.0 - 1.0) = 0
        var status = new ProviderHealthStatus(
            "p1", true, 1.0, 10000.0, 0, null, null, 100, 100, DateTime.UtcNow);

        // Act
        var score = status.HealthScore;

        // Assert - score = 1.0 * 0.7 + 0 * 0.3 = 0.7
        score.Should().BeApproximately(0.7, 0.001);
    }

    [Fact]
    public void HealthScore_ZeroLatency_MaxLatencyScore()
    {
        // Arrange - latency = 0 => latencyScore = Max(0, 1.0 - 0) = 1.0
        var status = new ProviderHealthStatus(
            "p1", true, 1.0, 0.0, 0, null, null, 100, 100, DateTime.UtcNow);

        // Act
        var score = status.HealthScore;

        // Assert - score = 1.0 * 0.7 + 1.0 * 0.3 = 1.0
        score.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public void HealthScore_ZeroSuccessRate_OnlyLatencyContributes()
    {
        // Arrange
        var status = new ProviderHealthStatus(
            "p1", true, 0.0, 0.0, 0, null, null, 100, 0, DateTime.UtcNow);

        // Act
        var score = status.HealthScore;

        // Assert - score = 0.0 * 0.7 + 1.0 * 0.3 = 0.3
        score.Should().BeApproximately(0.3, 0.001);
    }

    [Fact]
    public void HealthScore_UnhealthyAndInCooldown_ReturnsZero()
    {
        // Arrange - both unhealthy and in cooldown
        var status = new ProviderHealthStatus(
            "p1", false, 0.9, 100.0, 5,
            DateTime.UtcNow, DateTime.UtcNow.AddMinutes(5),
            100, 90, DateTime.UtcNow);

        // Act
        var score = status.HealthScore;

        // Assert
        score.Should().Be(0.0);
    }

    [Fact]
    public void IsInCooldown_NullCooldownUntil_ReturnsFalse()
    {
        // Arrange
        var status = new ProviderHealthStatus(
            "p1", true, 0.9, 100.0, 0, null, null, 100, 90, DateTime.UtcNow);

        // Assert
        status.IsInCooldown.Should().BeFalse();
    }

    [Fact]
    public void ProviderHealthStatus_AllNullableFieldsNull()
    {
        // Arrange
        var status = new ProviderHealthStatus(
            "p1", true, 1.0, 50.0, 0, null, null, 500, 500, DateTime.UtcNow);

        // Assert
        status.LastFailureTime.Should().BeNull();
        status.CooldownUntil.Should().BeNull();
        status.IsInCooldown.Should().BeFalse();
    }

    [Fact]
    public void ProviderHealthStatus_LastFailureTimeSet()
    {
        // Arrange
        var failureTime = DateTime.UtcNow.AddMinutes(-10);

        // Act
        var status = new ProviderHealthStatus(
            "p1", true, 0.8, 200.0, 2,
            failureTime, null, 100, 80, DateTime.UtcNow);

        // Assert
        status.LastFailureTime.Should().Be(failureTime);
        status.ConsecutiveFailures.Should().Be(2);
    }

    [Fact]
    public void HealthScore_FiveThousandLatency_MidpointLatencyScore()
    {
        // Arrange - latency = 5000 => latencyScore = Max(0, 1.0 - 0.5) = 0.5
        var status = new ProviderHealthStatus(
            "p1", true, 1.0, 5000.0, 0, null, null, 100, 100, DateTime.UtcNow);

        // Act
        var score = status.HealthScore;

        // Assert - score = 1.0 * 0.7 + 0.5 * 0.3 = 0.85
        score.Should().BeApproximately(0.85, 0.001);
    }
}
