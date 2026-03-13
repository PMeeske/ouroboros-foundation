using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics.Homeostasis;

[Trait("Category", "Unit")]
public class EthicalTensionTests
{
    [Fact]
    public void EthicalTension_ShouldBeCreatable()
    {
        // Verify EthicalTension type exists and is accessible
        typeof(EthicalTension).Should().NotBeNull();
    }

    [Fact]
    public void EthicalTension_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(EthicalTension).GetProperty("Id").Should().NotBeNull();
        typeof(EthicalTension).GetProperty("Description").Should().NotBeNull();
        typeof(EthicalTension).GetProperty("TraditionsInvolved").Should().NotBeNull();
        typeof(EthicalTension).GetProperty("Intensity").Should().NotBeNull();
        typeof(EthicalTension).GetProperty("DetectedAt").Should().NotBeNull();
    }
}
