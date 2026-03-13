using Ouroboros.Tools;
using Moq;

namespace Ouroboros.Tools.Tests;

[Trait("Category", "Unit")]
public class DslSuggestionTests
{
    [Fact]
    public void DslSuggestion_ShouldBeCreatable()
    {
        typeof(DslSuggestion).Should().NotBeNull();
    }
}
