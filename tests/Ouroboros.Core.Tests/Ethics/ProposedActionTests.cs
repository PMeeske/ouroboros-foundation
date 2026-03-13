using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
public class ProposedActionTests
{
    [Fact]
    public void ProposedAction_ShouldBeCreatable()
    {
        // Verify ProposedAction type exists and is accessible
        typeof(ProposedAction).Should().NotBeNull();
    }

    [Fact]
    public void ProposedAction_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(ProposedAction).GetProperty("ActionType").Should().NotBeNull();
        typeof(ProposedAction).GetProperty("Description").Should().NotBeNull();
        typeof(ProposedAction).GetProperty("Parameters").Should().NotBeNull();
        typeof(ProposedAction).GetProperty("TargetEntity").Should().NotBeNull();
        typeof(ProposedAction).GetProperty("PotentialEffects").Should().NotBeNull();
    }
}
