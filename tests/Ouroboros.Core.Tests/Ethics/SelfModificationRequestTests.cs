using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
public class SelfModificationRequestTests
{
    [Fact]
    public void SelfModificationRequest_ShouldBeCreatable()
    {
        // Verify SelfModificationRequest type exists and is accessible
        typeof(SelfModificationRequest).Should().NotBeNull();
    }

    [Fact]
    public void SelfModificationRequest_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(SelfModificationRequest).GetProperty("Type").Should().NotBeNull();
        typeof(SelfModificationRequest).GetProperty("Description").Should().NotBeNull();
        typeof(SelfModificationRequest).GetProperty("Justification").Should().NotBeNull();
        typeof(SelfModificationRequest).GetProperty("ActionContext").Should().NotBeNull();
        typeof(SelfModificationRequest).GetProperty("ExpectedImprovements").Should().NotBeNull();
    }
}
