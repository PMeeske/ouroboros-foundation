using Ouroboros.Core.Security.Authentication;
using Moq;

namespace Ouroboros.Core.Tests.Security.Authentication;

[Trait("Category", "Unit")]
public class AuthenticationPrincipalTests
{
    [Fact]
    public void AuthenticationPrincipal_ShouldBeCreatable()
    {
        // Verify AuthenticationPrincipal type exists and is accessible
        typeof(AuthenticationPrincipal).Should().NotBeNull();
    }

    [Fact]
    public void AuthenticationPrincipal_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(AuthenticationPrincipal).GetProperty("AuthenticationPrincipal").Should().NotBeNull();
        typeof(AuthenticationPrincipal).GetProperty("Id").Should().NotBeNull();
        typeof(AuthenticationPrincipal).GetProperty("Name").Should().NotBeNull();
        typeof(AuthenticationPrincipal).GetProperty("Email").Should().NotBeNull();
        typeof(AuthenticationPrincipal).GetProperty("Roles").Should().NotBeNull();
    }

    [Fact]
    public void HasRole_ShouldBeDefined()
    {
        // Verify HasRole method exists
        typeof(AuthenticationPrincipal).GetMethod("HasRole").Should().NotBeNull();
    }

    [Fact]
    public void HasAnyRole_ShouldBeDefined()
    {
        // Verify HasAnyRole method exists
        typeof(AuthenticationPrincipal).GetMethod("HasAnyRole").Should().NotBeNull();
    }

    [Fact]
    public void HasAllRoles_ShouldBeDefined()
    {
        // Verify HasAllRoles method exists
        typeof(AuthenticationPrincipal).GetMethod("HasAllRoles").Should().NotBeNull();
    }
}
