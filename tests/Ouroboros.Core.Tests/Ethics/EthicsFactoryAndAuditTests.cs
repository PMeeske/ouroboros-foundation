using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicsFrameworkFactoryTests
{
    [Fact]
    public void CreateDefault_ReturnsNonNullFramework()
    {
        var framework = EthicsFrameworkFactory.CreateDefault();

        framework.Should().NotBeNull();
        framework.Should().BeAssignableTo<IEthicsFramework>();
    }

    [Fact]
    public void CreateDefault_ReturnedFrameworkHasCorePrinciples()
    {
        var framework = EthicsFrameworkFactory.CreateDefault();

        framework.GetCorePrinciples().Should().NotBeEmpty();
    }

    [Fact]
    public void CreateWithAuditLog_NullAuditLog_ThrowsArgumentNullException()
    {
        var act = () => EthicsFrameworkFactory.CreateWithAuditLog(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateWithAuditLog_ValidLog_ReturnsFramework()
    {
        var auditLog = new InMemoryEthicsAuditLog();

        var framework = EthicsFrameworkFactory.CreateWithAuditLog(auditLog);

        framework.Should().NotBeNull();
    }

    [Fact]
    public void CreateCustom_NullAuditLog_ThrowsArgumentNullException()
    {
        var act = () => EthicsFrameworkFactory.CreateCustom(null!, new Mock<IEthicalReasoner>().Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateCustom_NullReasoner_ThrowsArgumentNullException()
    {
        var act = () => EthicsFrameworkFactory.CreateCustom(new InMemoryEthicsAuditLog(), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateCustom_ValidParams_ReturnsFramework()
    {
        var framework = EthicsFrameworkFactory.CreateCustom(
            new InMemoryEthicsAuditLog(), new Mock<IEthicalReasoner>().Object);

        framework.Should().NotBeNull();
    }
}
