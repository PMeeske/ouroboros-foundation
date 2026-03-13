using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
public class EthicsFrameworkFactoryTests
{
    [Fact]
    public void EthicsFrameworkFactory_ShouldBeCreatable()
    {
        // Verify EthicsFrameworkFactory type exists and is accessible
        typeof(EthicsFrameworkFactory).Should().NotBeNull();
    }

    [Fact]
    public void CreateDefault_ShouldBeDefined()
    {
        // Verify CreateDefault method exists
        typeof(EthicsFrameworkFactory).GetMethod("CreateDefault").Should().NotBeNull();
    }

    [Fact]
    public void CreateWithAuditLog_ShouldBeDefined()
    {
        // Verify CreateWithAuditLog method exists
        typeof(EthicsFrameworkFactory).GetMethod("CreateWithAuditLog").Should().NotBeNull();
    }

    [Fact]
    public void CreateCustom_ShouldBeDefined()
    {
        // Verify CreateCustom method exists
        typeof(EthicsFrameworkFactory).GetMethod("CreateCustom").Should().NotBeNull();
    }
}
