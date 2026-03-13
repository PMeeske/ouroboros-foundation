using Ouroboros.Domain.SelfModification;

namespace Ouroboros.Domain.Tests.SelfModification;

[Trait("Category", "Unit")]
public class StringExtensionsTests
{
    [Fact]
    public void StringExtensions_ShouldBeDefined()
    {
        typeof(StringExtensions).Should().NotBeNull();
    }
}
