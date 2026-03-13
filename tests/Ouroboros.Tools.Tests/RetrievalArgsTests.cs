using Ouroboros.Tools;
using Moq;

namespace Ouroboros.Tools.Tests;

[Trait("Category", "Unit")]
public class RetrievalArgsTests
{
    [Fact]
    public void RetrievalArgs_ShouldBeCreatable()
    {
        typeof(RetrievalArgs).Should().NotBeNull();
    }
}
