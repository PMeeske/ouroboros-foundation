using Ouroboros.Domain.Vectors;

namespace Ouroboros.Domain.Tests.Vectors;

[Trait("Category", "Unit")]
public class VectorStoreFactoryExtensionsTests
{
    [Fact]
    public void VectorStoreFactoryExtensions_ShouldBeDefined()
    {
        typeof(VectorStoreFactoryExtensions).Should().NotBeNull();
    }
}
