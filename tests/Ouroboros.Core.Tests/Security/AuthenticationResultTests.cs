using Ouroboros.Core.Security.Authentication;

namespace Ouroboros.Core.Tests.Security;

[Trait("Category", "Unit")]
public class AuthenticationResultTests
{
    [Fact]
    public void Success_SetsProperties()
    {
        var principal = new AuthenticationPrincipal
        {
            Id = "user-1",
            Name = "alice",
            Roles = new List<string> { "admin" }
        };

        var result = AuthenticationResult.Success(principal, "jwt-token-123");

        result.IsSuccess.Should().BeTrue();
        result.Principal.Should().BeSameAs(principal);
        result.Token.Should().Be("jwt-token-123");
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Failure_SetsProperties()
    {
        var result = AuthenticationResult.Failure("Invalid credentials");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid credentials");
        result.Principal.Should().BeNull();
        result.Token.Should().BeNull();
    }

    [Fact]
    public void DefaultConstruction_AllPropertiesDefault()
    {
        var result = new AuthenticationResult();

        result.IsSuccess.Should().BeFalse();
        result.Principal.Should().BeNull();
        result.Token.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void InitSyntax_SetsProperties()
    {
        var result = new AuthenticationResult
        {
            IsSuccess = true,
            Token = "token",
            ErrorMessage = null
        };

        result.IsSuccess.Should().BeTrue();
        result.Token.Should().Be("token");
    }
}
