using Ouroboros.Core.Security.Authentication;

namespace Ouroboros.Core.Tests.Security;

[Trait("Category", "Unit")]
[Trait("Category", "Security")]
public class InMemoryAuthenticationProviderComprehensiveTests
{
    private readonly InMemoryAuthenticationProvider _sut = new();

    private static AuthenticationPrincipal CreatePrincipal(
        string id = "user-1",
        string name = "testuser",
        params string[] roles)
    {
        return new AuthenticationPrincipal
        {
            Id = id,
            Name = name,
            Roles = roles.ToList(),
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    // --- RegisterUser ---

    [Fact]
    public async Task RegisterUser_ThenAuthenticate_Succeeds()
    {
        var principal = CreatePrincipal();
        _sut.RegisterUser("alice", "secret", principal);

        var result = await _sut.AuthenticateAsync("alice", "secret");

        result.IsSuccess.Should().BeTrue();
        result.Principal.Should().NotBeNull();
        result.Principal!.Id.Should().Be("user-1");
    }

    [Fact]
    public async Task RegisterUser_OverwritesExistingUser()
    {
        var principal1 = CreatePrincipal(id: "old-id", name: "old");
        var principal2 = CreatePrincipal(id: "new-id", name: "new");

        _sut.RegisterUser("alice", "pass1", principal1);
        _sut.RegisterUser("alice", "pass2", principal2);

        var result1 = await _sut.AuthenticateAsync("alice", "pass1");
        result1.IsSuccess.Should().BeFalse();

        var result2 = await _sut.AuthenticateAsync("alice", "pass2");
        result2.IsSuccess.Should().BeTrue();
        result2.Principal!.Id.Should().Be("new-id");
    }

    // --- AuthenticateAsync ---

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsSuccessWithToken()
    {
        var principal = CreatePrincipal();
        _sut.RegisterUser("bob", "password123", principal);

        var result = await _sut.AuthenticateAsync("bob", "password123");

        result.IsSuccess.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsPrincipal()
    {
        var principal = CreatePrincipal(id: "p1", name: "bob", roles: "admin");
        _sut.RegisterUser("bob", "pass", principal);

        var result = await _sut.AuthenticateAsync("bob", "pass");

        result.Principal.Should().NotBeNull();
        result.Principal!.Name.Should().Be("bob");
        result.Principal.Roles.Should().Contain("admin");
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidUsername_ReturnsFailure()
    {
        var result = await _sut.AuthenticateAsync("nonexistent", "password");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.Principal.Should().BeNull();
        result.Token.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_WrongPassword_ReturnsFailure()
    {
        _sut.RegisterUser("alice", "correct", CreatePrincipal());

        var result = await _sut.AuthenticateAsync("alice", "wrong");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid");
    }

    [Fact]
    public async Task AuthenticateAsync_EmptyUsername_ReturnsFailure()
    {
        var result = await _sut.AuthenticateAsync("", "password");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateAsync_EmptyPassword_ReturnsFailure()
    {
        _sut.RegisterUser("alice", "secret", CreatePrincipal());

        var result = await _sut.AuthenticateAsync("alice", "");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateAsync_GeneratesUniqueTokensPerAuthentication()
    {
        _sut.RegisterUser("alice", "pass", CreatePrincipal());

        var result1 = await _sut.AuthenticateAsync("alice", "pass");
        var result2 = await _sut.AuthenticateAsync("alice", "pass");

        result1.Token.Should().NotBe(result2.Token);
    }

    [Fact]
    public async Task AuthenticateAsync_CaseSensitiveUsername()
    {
        _sut.RegisterUser("Alice", "pass", CreatePrincipal());

        var result = await _sut.AuthenticateAsync("alice", "pass");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateAsync_CaseSensitivePassword()
    {
        _sut.RegisterUser("alice", "Password", CreatePrincipal());

        var result = await _sut.AuthenticateAsync("alice", "password");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateAsync_ErrorMessageDoesNotRevealWhichFieldIsWrong()
    {
        _sut.RegisterUser("alice", "secret", CreatePrincipal());

        var badUser = await _sut.AuthenticateAsync("wrong", "secret");
        var badPass = await _sut.AuthenticateAsync("alice", "wrong");

        badUser.ErrorMessage.Should().Be(badPass.ErrorMessage,
            because: "error messages should not distinguish between wrong username and wrong password");
    }

    // --- ValidateTokenAsync ---

    [Fact]
    public async Task ValidateTokenAsync_RevokedToken_ReturnsFailure()
    {
        await _sut.RevokeTokenAsync("some-token");

        var result = await _sut.ValidateTokenAsync("some-token");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("revoked");
    }

    [Fact]
    public async Task ValidateTokenAsync_UnknownToken_ReturnsFailure()
    {
        var result = await _sut.ValidateTokenAsync("unknown-token");

        result.IsSuccess.Should().BeFalse();
    }

    // --- RefreshTokenAsync ---

    [Fact]
    public async Task RefreshTokenAsync_AlwaysReturnsFailure()
    {
        var result = await _sut.RefreshTokenAsync("any-token");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not implemented");
    }

    // --- RevokeTokenAsync ---

    [Fact]
    public async Task RevokeTokenAsync_ReturnsTrue()
    {
        var result = await _sut.RevokeTokenAsync("token-to-revoke");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeTokenAsync_RevokedTokenFailsValidation()
    {
        _sut.RegisterUser("alice", "pass", CreatePrincipal());
        var authResult = await _sut.AuthenticateAsync("alice", "pass");
        var token = authResult.Token!;

        await _sut.RevokeTokenAsync(token);
        var validateResult = await _sut.ValidateTokenAsync(token);

        validateResult.IsSuccess.Should().BeFalse();
        validateResult.ErrorMessage.Should().Contain("revoked");
    }

    [Fact]
    public async Task RevokeTokenAsync_RevokingSameTokenTwice_ReturnsTrueBothTimes()
    {
        var result1 = await _sut.RevokeTokenAsync("duplicate-token");
        var result2 = await _sut.RevokeTokenAsync("duplicate-token");

        result1.Should().BeTrue();
        result2.Should().BeTrue();
    }

    // --- CancellationToken support ---

    [Fact]
    public async Task AuthenticateAsync_WithCancellationToken_Works()
    {
        _sut.RegisterUser("alice", "pass", CreatePrincipal());
        using var cts = new CancellationTokenSource();

        var result = await _sut.AuthenticateAsync("alice", "pass", cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithCancellationToken_Works()
    {
        using var cts = new CancellationTokenSource();

        var result = await _sut.ValidateTokenAsync("token", cts.Token);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeTokenAsync_WithCancellationToken_Works()
    {
        using var cts = new CancellationTokenSource();

        var result = await _sut.RevokeTokenAsync("token", cts.Token);

        result.Should().BeTrue();
    }

    // --- Interface compliance ---

    [Fact]
    public void ImplementsIAuthenticationProvider()
    {
        _sut.Should().BeAssignableTo<IAuthenticationProvider>();
    }

    // --- Multiple users ---

    [Fact]
    public async Task MultipleUsers_EachAuthenticatesIndependently()
    {
        _sut.RegisterUser("alice", "alicePass", CreatePrincipal(id: "1", name: "alice"));
        _sut.RegisterUser("bob", "bobPass", CreatePrincipal(id: "2", name: "bob"));

        var aliceResult = await _sut.AuthenticateAsync("alice", "alicePass");
        var bobResult = await _sut.AuthenticateAsync("bob", "bobPass");

        aliceResult.IsSuccess.Should().BeTrue();
        aliceResult.Principal!.Name.Should().Be("alice");

        bobResult.IsSuccess.Should().BeTrue();
        bobResult.Principal!.Name.Should().Be("bob");
    }

    [Fact]
    public async Task MultipleUsers_CannotCrossAuthenticate()
    {
        _sut.RegisterUser("alice", "alicePass", CreatePrincipal(name: "alice"));
        _sut.RegisterUser("bob", "bobPass", CreatePrincipal(name: "bob"));

        var result = await _sut.AuthenticateAsync("alice", "bobPass");

        result.IsSuccess.Should().BeFalse();
    }
}
