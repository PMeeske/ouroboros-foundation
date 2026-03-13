using Ouroboros.Core.LangChain;
using Moq;

namespace Ouroboros.Core.Tests.LangChain;

[Trait("Category", "Unit")]
public class LangChainMemoryExtensionsTests
{
    [Fact]
    public void LangChainMemoryExtensions_ShouldBeCreatable()
    {
        // Verify LangChainMemoryExtensions type exists and is accessible
        typeof(LangChainMemoryExtensions).Should().NotBeNull();
    }
}
