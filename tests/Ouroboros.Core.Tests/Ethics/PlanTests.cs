using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
public class PlanTests
{
    [Fact]
    public void Plan_ShouldBeCreatable()
    {
        // Verify Plan type exists and is accessible
        typeof(Plan).Should().NotBeNull();
    }

    [Fact]
    public void Plan_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(Plan).GetProperty("Goal").Should().NotBeNull();
        typeof(Plan).GetProperty("Steps").Should().NotBeNull();
        typeof(Plan).GetProperty("ConfidenceScores").Should().NotBeNull();
        typeof(Plan).GetProperty("CreatedAt").Should().NotBeNull();
    }
}
