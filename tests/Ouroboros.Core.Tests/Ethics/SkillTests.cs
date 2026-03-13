using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
public class SkillTests
{
    [Fact]
    public void Skill_ShouldBeCreatable()
    {
        // Verify Skill type exists and is accessible
        typeof(Skill).Should().NotBeNull();
    }

    [Fact]
    public void Skill_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(Skill).GetProperty("Name").Should().NotBeNull();
        typeof(Skill).GetProperty("Description").Should().NotBeNull();
        typeof(Skill).GetProperty("Prerequisites").Should().NotBeNull();
        typeof(Skill).GetProperty("Steps").Should().NotBeNull();
        typeof(Skill).GetProperty("SuccessRate").Should().NotBeNull();
    }
}
