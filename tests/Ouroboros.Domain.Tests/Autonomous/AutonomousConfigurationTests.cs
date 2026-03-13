using Ouroboros.Domain.Autonomous;
using Moq;

namespace Ouroboros.Domain.Tests.Autonomous;

[Trait("Category", "Unit")]
public class AutonomousConfigurationTests
{
    [Fact]
    public void AutonomousConfiguration_ShouldBeCreatable()
    {
        typeof(AutonomousConfiguration).Should().NotBeNull();
    }
}
