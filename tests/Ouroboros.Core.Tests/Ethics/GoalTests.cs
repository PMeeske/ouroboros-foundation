using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
public class GoalTests
{
    [Fact]
    public void Goal_ShouldBeCreatable()
    {
        // Verify Goal type exists and is accessible
        typeof(Goal).Should().NotBeNull();
    }

    [Fact]
    public void Goal_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(Goal).GetProperty("Id").Should().NotBeNull();
        typeof(Goal).GetProperty("Description").Should().NotBeNull();
        typeof(Goal).GetProperty("Type").Should().NotBeNull();
        typeof(Goal).GetProperty("Priority").Should().NotBeNull();
    }
}
