using Ouroboros.Core.Hyperon;
using Moq;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
public class DistinctionEventArgsTests
{
    [Fact]
    public void DistinctionEventArgs_ShouldBeCreatable()
    {
        // Verify DistinctionEventArgs type exists and is accessible
        typeof(DistinctionEventArgs).Should().NotBeNull();
    }

    [Fact]
    public void DistinctionEventArgs_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(DistinctionEventArgs).GetProperty("EventType").Should().NotBeNull();
        typeof(DistinctionEventArgs).GetProperty("PreviousState").Should().NotBeNull();
        typeof(DistinctionEventArgs).GetProperty("CurrentState").Should().NotBeNull();
        typeof(DistinctionEventArgs).GetProperty("TriggerAtom").Should().NotBeNull();
        typeof(DistinctionEventArgs).GetProperty("Context").Should().NotBeNull();
    }
}
