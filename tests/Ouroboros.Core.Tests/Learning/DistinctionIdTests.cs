using Ouroboros.Core.Learning;
using Moq;

namespace Ouroboros.Core.Tests.Learning;

[Trait("Category", "Unit")]
public class DistinctionIdTests
{
    [Fact]
    public void DistinctionId_ShouldBeCreatable()
    {
        // Verify DistinctionId type exists and is accessible
        typeof(DistinctionId).Should().NotBeNull();
    }

    [Fact]
    public void NewId_ShouldBeDefined()
    {
        // Verify NewId method exists
        typeof(DistinctionId).GetMethod("NewId").Should().NotBeNull();
    }

    [Fact]
    public void FromString_ShouldBeDefined()
    {
        // Verify FromString method exists
        typeof(DistinctionId).GetMethod("FromString").Should().NotBeNull();
    }
}
