using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
public class SkillUsageContextTests
{
    [Fact]
    public void SkillUsageContext_ShouldBeCreatable()
    {
        // Verify SkillUsageContext type exists and is accessible
        typeof(SkillUsageContext).Should().NotBeNull();
    }

    [Fact]
    public void SkillUsageContext_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(SkillUsageContext).GetProperty("Skill").Should().NotBeNull();
        typeof(SkillUsageContext).GetProperty("ActionContext").Should().NotBeNull();
        typeof(SkillUsageContext).GetProperty("Goal").Should().NotBeNull();
        typeof(SkillUsageContext).GetProperty("InputParameters").Should().NotBeNull();
        typeof(SkillUsageContext).GetProperty("HistoricalSuccessRate").Should().NotBeNull();
    }
}
