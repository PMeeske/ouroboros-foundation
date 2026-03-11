using Ouroboros.Core.Resilience;

namespace Ouroboros.Core.Tests.Resilience;

[Trait("Category", "Unit")]
public class CircuitBreakerConfigTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var config = new CircuitBreakerConfig();
        config.FailureThreshold.Should().Be(3);
        config.OpenDuration.Should().Be(TimeSpan.FromMinutes(2));
        config.HalfOpenTimeout.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void CustomValues_AreRetained()
    {
        var config = new CircuitBreakerConfig
        {
            FailureThreshold = 5,
            OpenDuration = TimeSpan.FromMinutes(5),
            HalfOpenTimeout = TimeSpan.FromSeconds(30)
        };

        config.FailureThreshold.Should().Be(5);
        config.OpenDuration.Should().Be(TimeSpan.FromMinutes(5));
        config.HalfOpenTimeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var original = new CircuitBreakerConfig();
        var modified = original with { FailureThreshold = 10 };
        modified.FailureThreshold.Should().Be(10);
        modified.OpenDuration.Should().Be(original.OpenDuration);
    }
}

[Trait("Category", "Unit")]
public class ReasonerHealthTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var now = DateTimeOffset.UtcNow;
        var health = new ReasonerHealth("Closed", true, 0, now);

        health.CircuitState.Should().Be("Closed");
        health.SymbolicAvailable.Should().BeTrue();
        health.ConsecutiveLlmFailures.Should().Be(0);
        health.LastLlmSuccess.Should().Be(now);
    }

    [Fact]
    public void Construction_WithNullLastSuccess()
    {
        var health = new ReasonerHealth("Open", false, 5, null);

        health.CircuitState.Should().Be("Open");
        health.SymbolicAvailable.Should().BeFalse();
        health.ConsecutiveLlmFailures.Should().Be(5);
        health.LastLlmSuccess.Should().BeNull();
    }

    [Fact]
    public void RecordEquality_Works()
    {
        var time = DateTimeOffset.UtcNow;
        var a = new ReasonerHealth("Closed", true, 0, time);
        var b = new ReasonerHealth("Closed", true, 0, time);
        a.Should().Be(b);
    }
}
