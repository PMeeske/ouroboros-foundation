using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics.Homeostasis;

[Trait("Category", "Unit")]
public class HomeostasisEventTests
{
    [Fact]
    public void HomeostasisEvent_ShouldBeCreatable()
    {
        // Verify HomeostasisEvent type exists and is accessible
        typeof(HomeostasisEvent).Should().NotBeNull();
    }

    [Fact]
    public void HomeostasisEvent_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(HomeostasisEvent).GetProperty("EventType").Should().NotBeNull();
        typeof(HomeostasisEvent).GetProperty("Description").Should().NotBeNull();
        typeof(HomeostasisEvent).GetProperty("Before").Should().NotBeNull();
        typeof(HomeostasisEvent).GetProperty("After").Should().NotBeNull();
        typeof(HomeostasisEvent).GetProperty("OccurredAt").Should().NotBeNull();
    }
}
