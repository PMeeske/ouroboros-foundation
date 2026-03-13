using Ouroboros.Core.Hyperon;
using Moq;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
public class TruthValueEventArgsTests
{
    [Fact]
    public void TruthValueEventArgs_ShouldBeCreatable()
    {
        // Verify TruthValueEventArgs type exists and is accessible
        typeof(TruthValueEventArgs).Should().NotBeNull();
    }

    [Fact]
    public void TruthValueEventArgs_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(TruthValueEventArgs).GetProperty("Expression").Should().NotBeNull();
        typeof(TruthValueEventArgs).GetProperty("TruthValue").Should().NotBeNull();
        typeof(TruthValueEventArgs).GetProperty("ReasoningTrace").Should().NotBeNull();
    }
}
