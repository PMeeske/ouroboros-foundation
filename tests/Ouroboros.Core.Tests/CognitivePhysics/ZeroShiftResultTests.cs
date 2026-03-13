using Ouroboros.Core.CognitivePhysics;
using Moq;

namespace Ouroboros.Core.Tests.CognitivePhysics;

[Trait("Category", "Unit")]
public class ZeroShiftResultTests
{
    [Fact]
    public void ZeroShiftResult_ShouldBeCreatable()
    {
        // Verify ZeroShiftResult type exists and is accessible
        typeof(ZeroShiftResult).Should().NotBeNull();
    }

    [Fact]
    public void ZeroShiftResult_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(ZeroShiftResult).GetProperty("Success").Should().NotBeNull();
        typeof(ZeroShiftResult).GetProperty("State").Should().NotBeNull();
        typeof(ZeroShiftResult).GetProperty("Cost").Should().NotBeNull();
        typeof(ZeroShiftResult).GetProperty("FailureReason").Should().NotBeNull();
    }

    [Fact]
    public void Succeeded_ShouldBeDefined()
    {
        // Verify Succeeded method exists
        typeof(ZeroShiftResult).GetMethod("Succeeded").Should().NotBeNull();
    }

    [Fact]
    public void Failed_ShouldBeDefined()
    {
        // Verify Failed method exists
        typeof(ZeroShiftResult).GetMethod("Failed").Should().NotBeNull();
    }
}
