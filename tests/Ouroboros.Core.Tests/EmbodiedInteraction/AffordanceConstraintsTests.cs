using Ouroboros.Core.EmbodiedInteraction;
using Moq;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class AffordanceConstraintsTests
{
    [Fact]
    public void AffordanceConstraints_ShouldBeCreatable()
    {
        // Verify AffordanceConstraints type exists and is accessible
        typeof(AffordanceConstraints).Should().NotBeNull();
    }
}
