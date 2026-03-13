using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics.Homeostasis;

[Trait("Category", "Unit")]
public class HomeostasisSnapshotTests
{
    [Fact]
    public void HomeostasisSnapshot_ShouldBeCreatable()
    {
        // Verify HomeostasisSnapshot type exists and is accessible
        typeof(HomeostasisSnapshot).Should().NotBeNull();
    }

    [Fact]
    public void HomeostasisSnapshot_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(HomeostasisSnapshot).GetProperty("OverallBalance").Should().NotBeNull();
        typeof(HomeostasisSnapshot).GetProperty("ActiveTensions").Should().NotBeNull();
        typeof(HomeostasisSnapshot).GetProperty("TraditionWeights").Should().NotBeNull();
        typeof(HomeostasisSnapshot).GetProperty("UnresolvedParadoxCount").Should().NotBeNull();
        typeof(HomeostasisSnapshot).GetProperty("IsStable").Should().NotBeNull();
    }
}
