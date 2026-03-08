using Ouroboros.Core.Security.Authentication;

namespace Ouroboros.Core.Tests.Security;

[Trait("Category", "Unit")]
[Trait("Category", "Security")]
public class InMemoryAuthenticationProviderTests
{
    private readonly InMemoryAuthenticationProvider _sut = new();

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsSuccess()
    {
        var principal = new AuthenticationPrincipal
        {
            Id = "user1",
            Name = "alice",
            Roles = new List<string> { "admin" },
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        _sut.RegisterUser("alice", "password123", principal);

        var result = await _sut.AuthenticateAsync("alice", "password123");

        result.IsSuccess.Should().BeTrue();
        result.Principal.Should().NotBeNull();
        result.Principal!.Name.Should().Be("alice");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidUsername_ReturnsFailure()
    {
        var result = await _sut.AuthenticateAsync("nonexistent", "password");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid");
    }

    [Fact]
    public async Task AuthenticateAsync_WrongPassword_ReturnsFailure()
    {
        var principal = new AuthenticationPrincipal { Id = "u1", Name = "bob" };
        _sut.RegisterUser("bob", "correct", principal);

        var result = await _sut.AuthenticateAsync("bob", "wrong");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid");
    }

    [Fact]
    public async Task ValidateTokenAsync_RevokedToken_ReturnsFailure()
    {
        await _sut.RevokeTokenAsync("token123");

        var result = await _sut.ValidateTokenAsync("token123");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("revoked");
    }

    [Fact]
    public async Task ValidateTokenAsync_UnknownToken_ReturnsFailure()
    {
        var result = await _sut.ValidateTokenAsync("random-token");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshTokenAsync_ReturnsFailure()
    {
        var result = await _sut.RefreshTokenAsync("any-token");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeTokenAsync_ReturnsTrue()
    {
        var result = await _sut.RevokeTokenAsync("token-to-revoke");

        result.Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Security")]
public class AuthenticationPrincipalTests
{
    [Fact]
    public void HasRole_ExistingRole_ReturnsTrue()
    {
        var principal = new AuthenticationPrincipal
        {
            Id = "1", Name = "test",
            Roles = new List<string> { "admin", "user" }
        };

        principal.HasRole("admin").Should().BeTrue();
    }

    [Fact]
    public void HasRole_CaseInsensitive_ReturnsTrue()
    {
        var principal = new AuthenticationPrincipal
        {
            Id = "1", Name = "test",
            Roles = new List<string> { "Admin" }
        };

        principal.HasRole("admin").Should().BeTrue();
    }

    [Fact]
    public void HasRole_NonExistingRole_ReturnsFalse()
    {
        var principal = new AuthenticationPrincipal
        {
            Id = "1", Name = "test",
            Roles = new List<string> { "user" }
        };

        principal.HasRole("admin").Should().BeFalse();
    }

    [Fact]
    public void HasAnyRole_WithMatchingRole_ReturnsTrue()
    {
        var principal = new AuthenticationPrincipal
        {
            Id = "1", Name = "test",
            Roles = new List<string> { "user" }
        };

        principal.HasAnyRole("admin", "user").Should().BeTrue();
    }

    [Fact]
    public void HasAnyRole_WithNoMatchingRole_ReturnsFalse()
    {
        var principal = new AuthenticationPrincipal
        {
            Id = "1", Name = "test",
            Roles = new List<string> { "user" }
        };

        principal.HasAnyRole("admin", "superadmin").Should().BeFalse();
    }

    [Fact]
    public void HasAllRoles_WithAllRoles_ReturnsTrue()
    {
        var principal = new AuthenticationPrincipal
        {
            Id = "1", Name = "test",
            Roles = new List<string> { "admin", "user" }
        };

        principal.HasAllRoles("admin", "user").Should().BeTrue();
    }

    [Fact]
    public void HasAllRoles_MissingSome_ReturnsFalse()
    {
        var principal = new AuthenticationPrincipal
        {
            Id = "1", Name = "test",
            Roles = new List<string> { "user" }
        };

        principal.HasAllRoles("admin", "user").Should().BeFalse();
    }

    [Fact]
    public void IsExpired_FutureExpiry_ReturnsFalse()
    {
        var principal = new AuthenticationPrincipal
        {
            Id = "1", Name = "test",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        principal.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_PastExpiry_ReturnsTrue()
    {
        var principal = new AuthenticationPrincipal
        {
            Id = "1", Name = "test",
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };

        principal.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void GetClaim_ExistingClaim_ReturnsValue()
    {
        var principal = new AuthenticationPrincipal
        {
            Id = "1", Name = "test",
            Claims = new Dictionary<string, string> { ["department"] = "engineering" }
        };

        principal.GetClaim("department").Should().Be("engineering");
    }

    [Fact]
    public void GetClaim_MissingClaim_ReturnsNull()
    {
        var principal = new AuthenticationPrincipal { Id = "1", Name = "test" };

        principal.GetClaim("nonexistent").Should().BeNull();
    }
}
