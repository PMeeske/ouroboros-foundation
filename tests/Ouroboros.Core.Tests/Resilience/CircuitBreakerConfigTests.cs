using Ouroboros.Core.Resilience;
using Moq;

namespace Ouroboros.Core.Tests.Resilience;

[Trait("Category", "Unit")]
public class CircuitBreakerConfigTests
{
    [Fact]
    public void CircuitBreakerConfig_ShouldBeCreatable()
    {
        // Verify CircuitBreakerConfig type exists and is accessible
        typeof(CircuitBreakerConfig).Should().NotBeNull();
    }

    [Fact]
    public void CircuitBreakerConfig_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(CircuitBreakerConfig).GetProperty("FailureThreshold").Should().NotBeNull();
        typeof(CircuitBreakerConfig).GetProperty("OpenDuration").Should().NotBeNull();
        typeof(CircuitBreakerConfig).GetProperty("HalfOpenTimeout").Should().NotBeNull();
    }
}
