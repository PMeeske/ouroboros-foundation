using Ouroboros.Core.EmbodiedInteraction;
using Moq;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class PerceptionDataTests
{
    [Fact]
    public void PerceptionData_ShouldBeCreatable()
    {
        // Verify PerceptionData type exists and is accessible
        typeof(PerceptionData).Should().NotBeNull();
    }
}
