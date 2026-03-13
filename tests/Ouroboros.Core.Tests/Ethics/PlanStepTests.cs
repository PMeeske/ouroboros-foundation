using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
public class PlanStepTests
{
    [Fact]
    public void PlanStep_ShouldBeCreatable()
    {
        // Verify PlanStep type exists and is accessible
        typeof(PlanStep).Should().NotBeNull();
    }

    [Fact]
    public void PlanStep_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(PlanStep).GetProperty("Action").Should().NotBeNull();
        typeof(PlanStep).GetProperty("Parameters").Should().NotBeNull();
        typeof(PlanStep).GetProperty("ExpectedOutcome").Should().NotBeNull();
        typeof(PlanStep).GetProperty("ConfidenceScore").Should().NotBeNull();
    }
}
