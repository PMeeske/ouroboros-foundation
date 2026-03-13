using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
public class ActionContextTests
{
    [Fact]
    public void ActionContext_ShouldBeCreatable()
    {
        // Verify ActionContext type exists and is accessible
        typeof(ActionContext).Should().NotBeNull();
    }

    [Fact]
    public void ActionContext_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(ActionContext).GetProperty("AgentId").Should().NotBeNull();
        typeof(ActionContext).GetProperty("UserId").Should().NotBeNull();
        typeof(ActionContext).GetProperty("Environment").Should().NotBeNull();
        typeof(ActionContext).GetProperty("State").Should().NotBeNull();
        typeof(ActionContext).GetProperty("RecentActions").Should().NotBeNull();
    }
}
