using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
public class EthicalViolationTests
{
    [Fact]
    public void EthicalViolation_ShouldBeCreatable()
    {
        // Verify EthicalViolation type exists and is accessible
        typeof(EthicalViolation).Should().NotBeNull();
    }

    [Fact]
    public void EthicalViolation_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(EthicalViolation).GetProperty("ViolatedPrinciple").Should().NotBeNull();
        typeof(EthicalViolation).GetProperty("Description").Should().NotBeNull();
        typeof(EthicalViolation).GetProperty("Severity").Should().NotBeNull();
        typeof(EthicalViolation).GetProperty("Evidence").Should().NotBeNull();
        typeof(EthicalViolation).GetProperty("AffectedParties").Should().NotBeNull();
    }
}
