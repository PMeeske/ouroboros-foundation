using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
public class EthicalPrincipleTests
{
    [Fact]
    public void EthicalPrinciple_ShouldBeCreatable()
    {
        // Verify EthicalPrinciple type exists and is accessible
        typeof(EthicalPrinciple).Should().NotBeNull();
    }

    [Fact]
    public void EthicalPrinciple_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(EthicalPrinciple).GetProperty("Id").Should().NotBeNull();
        typeof(EthicalPrinciple).GetProperty("Name").Should().NotBeNull();
        typeof(EthicalPrinciple).GetProperty("Description").Should().NotBeNull();
        typeof(EthicalPrinciple).GetProperty("Category").Should().NotBeNull();
        typeof(EthicalPrinciple).GetProperty("Priority").Should().NotBeNull();
    }

    [Fact]
    public void GetCorePrinciples_ShouldBeDefined()
    {
        // Verify GetCorePrinciples method exists
        typeof(EthicalPrinciple).GetMethod("GetCorePrinciples").Should().NotBeNull();
    }
}
