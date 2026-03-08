using Ouroboros.Core.Security.Authentication;
using Ouroboros.Core.Security.Authorization;

namespace Ouroboros.Core.Tests.Security;

[Trait("Category", "Unit")]
[Trait("Category", "Security")]
public class RoleBasedAuthorizationProviderTests
{
    private readonly RoleBasedAuthorizationProvider _sut = new();

    private static AuthenticationPrincipal CreatePrincipal(params string[] roles)
    {
        return new AuthenticationPrincipal
        {
            Id = "user1",
            Name = "testuser",
            Roles = roles.ToList(),
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    // --- AuthorizeToolExecutionAsync ---

    [Fact]
    public async Task AuthorizeToolExecutionAsync_NoRequirements_AllowsAll()
    {
        var principal = CreatePrincipal("user");

        var result = await _sut.AuthorizeToolExecutionAsync(principal, "any-tool");

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeToolExecutionAsync_HasRequiredRole_Allows()
    {
        _sut.RequireRoleForTool("admin-tool", "admin");
        var principal = CreatePrincipal("admin");

        var result = await _sut.AuthorizeToolExecutionAsync(principal, "admin-tool");

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeToolExecutionAsync_MissingRequiredRole_Denies()
    {
        _sut.RequireRoleForTool("admin-tool", "admin");
        var principal = CreatePrincipal("user");

        var result = await _sut.AuthorizeToolExecutionAsync(principal, "admin-tool");

        result.IsAuthorized.Should().BeFalse();
        result.DenialReason.Should().Contain("admin");
    }

    [Fact]
    public async Task AuthorizeToolExecutionAsync_MultipleRequiredRoles_AnyRoleSuffices()
    {
        _sut.RequireRoleForTool("protected-tool", "admin");
        _sut.RequireRoleForTool("protected-tool", "superuser");
        var principal = CreatePrincipal("superuser");

        var result = await _sut.AuthorizeToolExecutionAsync(principal, "protected-tool");

        result.IsAuthorized.Should().BeTrue();
    }

    // --- CheckPermissionAsync ---

    [Fact]
    public async Task CheckPermissionAsync_HasPermission_Allows()
    {
        _sut.AssignPermissionToRole("admin", "document:write");
        var principal = CreatePrincipal("admin");

        var result = await _sut.CheckPermissionAsync(principal, "document:write");

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPermissionAsync_MissingPermission_Denies()
    {
        _sut.AssignPermissionToRole("user", "document:read");
        var principal = CreatePrincipal("user");

        var result = await _sut.CheckPermissionAsync(principal, "document:write");

        result.IsAuthorized.Should().BeFalse();
        result.DenialReason.Should().Contain("document:write");
    }

    [Fact]
    public async Task CheckPermissionAsync_NoRolesAssigned_Denies()
    {
        var principal = CreatePrincipal("user");

        var result = await _sut.CheckPermissionAsync(principal, "any-permission");

        result.IsAuthorized.Should().BeFalse();
    }

    // --- CheckResourceAccessAsync ---

    [Fact]
    public async Task CheckResourceAccessAsync_Authorized_Allows()
    {
        _sut.AssignPermissionToRole("editor", "document:write");
        var principal = CreatePrincipal("editor");

        var result = await _sut.CheckResourceAccessAsync(principal, "document", "doc-123", "write");

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task CheckResourceAccessAsync_Unauthorized_Denies()
    {
        var principal = CreatePrincipal("viewer");

        var result = await _sut.CheckResourceAccessAsync(principal, "document", "doc-123", "delete");

        result.IsAuthorized.Should().BeFalse();
    }

    // --- AssignPermissionToRole ---

    [Fact]
    public async Task AssignPermissionToRole_MultiplePermissions_AllAccessible()
    {
        _sut.AssignPermissionToRole("admin", "user:read");
        _sut.AssignPermissionToRole("admin", "user:write");
        _sut.AssignPermissionToRole("admin", "user:delete");
        var principal = CreatePrincipal("admin");

        var readResult = await _sut.CheckPermissionAsync(principal, "user:read");
        var writeResult = await _sut.CheckPermissionAsync(principal, "user:write");
        var deleteResult = await _sut.CheckPermissionAsync(principal, "user:delete");

        readResult.IsAuthorized.Should().BeTrue();
        writeResult.IsAuthorized.Should().BeTrue();
        deleteResult.IsAuthorized.Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Security")]
public class AuthorizationResultTests
{
    [Fact]
    public void Allow_CreatesAuthorizedResult()
    {
        var result = AuthorizationResult.Allow();

        result.IsAuthorized.Should().BeTrue();
        result.DenialReason.Should().BeNull();
    }

    [Fact]
    public void Deny_CreatesUnauthorizedResult()
    {
        var result = AuthorizationResult.Deny("Insufficient permissions");

        result.IsAuthorized.Should().BeFalse();
        result.DenialReason.Should().Be("Insufficient permissions");
    }
}
