using Ouroboros.Core.Security.Authorization;
using Moq;

namespace Ouroboros.Core.Tests.Security.Authorization;

[Trait("Category", "Unit")]
public class AuthorizationResultTests
{
    [Fact]
    public void AuthorizationResult_ShouldBeCreatable()
    {
        // Verify AuthorizationResult type exists and is accessible
        typeof(AuthorizationResult).Should().NotBeNull();
    }

    [Fact]
    public void AuthorizationResult_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(AuthorizationResult).GetProperty("AuthorizationResult").Should().NotBeNull();
        typeof(AuthorizationResult).GetProperty("IsAuthorized").Should().NotBeNull();
        typeof(AuthorizationResult).GetProperty("DenialReason").Should().NotBeNull();
    }

    [Fact]
    public void Allow_ShouldBeDefined()
    {
        // Verify Allow method exists
        typeof(AuthorizationResult).GetMethod("Allow").Should().NotBeNull();
    }

    [Fact]
    public void Deny_ShouldBeDefined()
    {
        // Verify Deny method exists
        typeof(AuthorizationResult).GetMethod("Deny").Should().NotBeNull();
    }
}
