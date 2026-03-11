using Ouroboros.Core.Security.Authentication;
using Ouroboros.Core.Security.Authorization;

namespace Ouroboros.Core.Tests.Security;

/// <summary>
/// Additional tests for AuthenticationResult and AuthenticationPrincipal to fill coverage gaps.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Category", "Security")]
public class AuthenticationResultAdditionalTests
{
    [Fact]
    public void Success_SetsAllProperties()
    {
        var principal = new AuthenticationPrincipal
        {
            Id = "u1",
            Name = "alice",
            Roles = new List<string> { "admin" },
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var result = AuthenticationResult.Success(principal, "jwt-token-123");

        result.IsSuccess.Should().BeTrue();
        result.Principal.Should().BeSameAs(principal);
        result.Token.Should().Be("jwt-token-123");
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Failure_SetsAllProperties()
    {
        var result = AuthenticationResult.Failure("Bad credentials");

        result.IsSuccess.Should().BeFalse();
        result.Principal.Should().BeNull();
        result.Token.Should().BeNull();
        result.ErrorMessage.Should().Be("Bad credentials");
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Security")]
public class AuthenticationPrincipalAdditionalTests
{
    [Fact]
    public void DefaultProperties_HaveExpectedValues()
    {
        var principal = new AuthenticationPrincipal();

        principal.Id.Should().BeEmpty();
        principal.Name.Should().BeEmpty();
        principal.Email.Should().BeNull();
        principal.Roles.Should().BeEmpty();
        principal.Claims.Should().BeEmpty();
    }

    [Fact]
    public void IsExpired_NotExpired_ReturnsFalse()
    {
        var principal = new AuthenticationPrincipal
        {
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        principal.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_AlreadyExpired_ReturnsTrue()
    {
        var principal = new AuthenticationPrincipal
        {
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };

        principal.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void HasRole_ExistingRole_ReturnsTrue()
    {
        var principal = new AuthenticationPrincipal
        {
            Roles = new List<string> { "Admin", "User" }
        };

        principal.HasRole("admin").Should().BeTrue();
        principal.HasRole("ADMIN").Should().BeTrue();
    }

    [Fact]
    public void HasRole_NonExistentRole_ReturnsFalse()
    {
        var principal = new AuthenticationPrincipal
        {
            Roles = new List<string> { "User" }
        };

        principal.HasRole("Admin").Should().BeFalse();
    }

    [Fact]
    public void HasAnyRole_MatchesOne_ReturnsTrue()
    {
        var principal = new AuthenticationPrincipal
        {
            Roles = new List<string> { "user" }
        };

        principal.HasAnyRole("admin", "user", "editor").Should().BeTrue();
    }

    [Fact]
    public void HasAnyRole_MatchesNone_ReturnsFalse()
    {
        var principal = new AuthenticationPrincipal
        {
            Roles = new List<string> { "viewer" }
        };

        principal.HasAnyRole("admin", "editor").Should().BeFalse();
    }

    [Fact]
    public void HasAllRoles_AllPresent_ReturnsTrue()
    {
        var principal = new AuthenticationPrincipal
        {
            Roles = new List<string> { "admin", "editor", "user" }
        };

        principal.HasAllRoles("admin", "editor").Should().BeTrue();
    }

    [Fact]
    public void HasAllRoles_SomeMissing_ReturnsFalse()
    {
        var principal = new AuthenticationPrincipal
        {
            Roles = new List<string> { "admin" }
        };

        principal.HasAllRoles("admin", "editor").Should().BeFalse();
    }

    [Fact]
    public void GetClaim_ExistingClaim_ReturnsValue()
    {
        var principal = new AuthenticationPrincipal
        {
            Claims = new Dictionary<string, string> { ["department"] = "engineering" }
        };

        principal.GetClaim("department").Should().Be("engineering");
    }

    [Fact]
    public void GetClaim_NonExistentClaim_ReturnsNull()
    {
        var principal = new AuthenticationPrincipal();

        principal.GetClaim("missing").Should().BeNull();
    }

    [Fact]
    public void Email_CanBeSet()
    {
        var principal = new AuthenticationPrincipal
        {
            Email = "test@example.com"
        };

        principal.Email.Should().Be("test@example.com");
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Security")]
public class AuthorizationResultAdditionalTests
{
    [Fact]
    public void Allow_CreatesAuthorizedResult()
    {
        var result = AuthorizationResult.Allow();

        result.IsAuthorized.Should().BeTrue();
        result.DenialReason.Should().BeNull();
    }

    [Fact]
    public void Deny_CreatesUnauthorizedResultWithReason()
    {
        var result = AuthorizationResult.Deny("Insufficient permissions");

        result.IsAuthorized.Should().BeFalse();
        result.DenialReason.Should().Be("Insufficient permissions");
    }
}
