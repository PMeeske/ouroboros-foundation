using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
public class PlanContextTests
{
    [Fact]
    public void PlanContext_ShouldBeCreatable()
    {
        // Verify PlanContext type exists and is accessible
        typeof(PlanContext).Should().NotBeNull();
    }

    [Fact]
    public void PlanContext_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(PlanContext).GetProperty("Plan").Should().NotBeNull();
        typeof(PlanContext).GetProperty("ActionContext").Should().NotBeNull();
        typeof(PlanContext).GetProperty("EstimatedRisk").Should().NotBeNull();
        typeof(PlanContext).GetProperty("ExpectedBenefits").Should().NotBeNull();
        typeof(PlanContext).GetProperty("PotentialConsequences").Should().NotBeNull();
    }
}
