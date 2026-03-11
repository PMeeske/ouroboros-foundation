using Ouroboros.Core.Security.Authentication;
using Ouroboros.Core.Security.Authorization;

namespace Ouroboros.Core.Tests.Security;

[Trait("Category", "Unit")]
[Trait("Category", "Security")]
public class RoleBasedAuthorizationProviderComprehensiveTests
{
    private readonly RoleBasedAuthorizationProvider _sut = new();

    private static AuthenticationPrincipal CreatePrincipal(params string[] roles)
    {
        return new AuthenticationPrincipal
        {
            Id = "user-1",
            Name = "testuser",
            Roles = roles.ToList(),
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    // --- AuthorizeToolExecutionAsync ---

    [Fact]
    public async Task AuthorizeToolExecutionAsync_NoRoleRequirements_AllowsAnyone()
    {
        var principal = CreatePrincipal("user");

        var result = await _sut.AuthorizeToolExecutionAsync(principal, "unrestricted-tool");

        result.IsAuthorized.Should().BeTrue();
        result.DenialReason.Should().BeNull();
    }

    [Fact]
    public async Task AuthorizeToolExecutionAsync_PrincipalHasRequiredRole_Allows()
    {
        _sut.RequireRoleForTool("admin-tool", "admin");
        var principal = CreatePrincipal("admin");

        var result = await _sut.AuthorizeToolExecutionAsync(principal, "admin-tool");

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeToolExecutionAsync_PrincipalMissingRequiredRole_Denies()
    {
        _sut.RequireRoleForTool("admin-tool", "admin");
        var principal = CreatePrincipal("user");

        var result = await _sut.AuthorizeToolExecutionAsync(principal, "admin-tool");

        result.IsAuthorized.Should().BeFalse();
        result.DenialReason.Should().Contain("admin-tool");
        result.DenialReason.Should().Contain("admin");
    }

    [Fact]
    public async Task AuthorizeToolExecutionAsync_MultipleRequiredRoles_AnyOneSuffices()
    {
        _sut.RequireRoleForTool("tool", "admin");
        _sut.RequireRoleForTool("tool", "superuser");
        var principal = CreatePrincipal("superuser");

        var result = await _sut.AuthorizeToolExecutionAsync(principal, "tool");

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeToolExecutionAsync_PrincipalWithNoRoles_DeniedFromRestrictedTool()
    {
        _sut.RequireRoleForTool("tool", "admin");
        var principal = CreatePrincipal();

        var result = await _sut.AuthorizeToolExecutionAsync(principal, "tool");

        result.IsAuthorized.Should().BeFalse();
    }

    [Fact]
    public async Task AuthorizeToolExecutionAsync_RoleComparisonIsCaseInsensitive()
    {
        _sut.RequireRoleForTool("tool", "Admin");
        var principal = CreatePrincipal("admin");

        var result = await _sut.AuthorizeToolExecutionAsync(principal, "tool");

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeToolExecutionAsync_WithInput_DoesNotAffectAuthorization()
    {
        _sut.RequireRoleForTool("tool", "admin");
        var principal = CreatePrincipal("admin");

        var result = await _sut.AuthorizeToolExecutionAsync(principal, "tool", "some input");

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeToolExecutionAsync_WithNullInput_Works()
    {
        _sut.RequireRoleForTool("tool", "admin");
        var principal = CreatePrincipal("admin");

        var result = await _sut.AuthorizeToolExecutionAsync(principal, "tool", null);

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeToolExecutionAsync_DifferentToolsSameRole()
    {
        _sut.RequireRoleForTool("tool-a", "admin");
        _sut.RequireRoleForTool("tool-b", "admin");
        var principal = CreatePrincipal("admin");

        var resultA = await _sut.AuthorizeToolExecutionAsync(principal, "tool-a");
        var resultB = await _sut.AuthorizeToolExecutionAsync(principal, "tool-b");

        resultA.IsAuthorized.Should().BeTrue();
        resultB.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeToolExecutionAsync_DifferentToolsDifferentRoles()
    {
        _sut.RequireRoleForTool("admin-tool", "admin");
        _sut.RequireRoleForTool("editor-tool", "editor");
        var principal = CreatePrincipal("admin");

        var adminResult = await _sut.AuthorizeToolExecutionAsync(principal, "admin-tool");
        var editorResult = await _sut.AuthorizeToolExecutionAsync(principal, "editor-tool");

        adminResult.IsAuthorized.Should().BeTrue();
        editorResult.IsAuthorized.Should().BeFalse();
    }

    // --- CheckPermissionAsync ---

    [Fact]
    public async Task CheckPermissionAsync_RoleHasPermission_Allows()
    {
        _sut.AssignPermissionToRole("admin", "users:write");
        var principal = CreatePrincipal("admin");

        var result = await _sut.CheckPermissionAsync(principal, "users:write");

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPermissionAsync_RoleLacksPermission_Denies()
    {
        _sut.AssignPermissionToRole("user", "users:read");
        var principal = CreatePrincipal("user");

        var result = await _sut.CheckPermissionAsync(principal, "users:write");

        result.IsAuthorized.Should().BeFalse();
        result.DenialReason.Should().Contain("users:write");
    }

    [Fact]
    public async Task CheckPermissionAsync_NoRolesConfigured_Denies()
    {
        var principal = CreatePrincipal("user");

        var result = await _sut.CheckPermissionAsync(principal, "anything");

        result.IsAuthorized.Should().BeFalse();
    }

    [Fact]
    public async Task CheckPermissionAsync_PermissionComparisonIsCaseInsensitive()
    {
        _sut.AssignPermissionToRole("admin", "Users:Write");
        var principal = CreatePrincipal("admin");

        var result = await _sut.CheckPermissionAsync(principal, "users:write");

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPermissionAsync_MultipleRoles_PermissionFromAnyRoleSuffices()
    {
        _sut.AssignPermissionToRole("editor", "docs:write");
        var principal = CreatePrincipal("user", "editor");

        var result = await _sut.CheckPermissionAsync(principal, "docs:write");

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPermissionAsync_MultiplePermissionsOnRole_EachAccessible()
    {
        _sut.AssignPermissionToRole("admin", "read");
        _sut.AssignPermissionToRole("admin", "write");
        _sut.AssignPermissionToRole("admin", "delete");
        var principal = CreatePrincipal("admin");

        var readResult = await _sut.CheckPermissionAsync(principal, "read");
        var writeResult = await _sut.CheckPermissionAsync(principal, "write");
        var deleteResult = await _sut.CheckPermissionAsync(principal, "delete");

        readResult.IsAuthorized.Should().BeTrue();
        writeResult.IsAuthorized.Should().BeTrue();
        deleteResult.IsAuthorized.Should().BeTrue();
    }

    // --- CheckResourceAccessAsync ---

    [Fact]
    public async Task CheckResourceAccessAsync_BuildsPermissionFromResourceTypeAndAction()
    {
        _sut.AssignPermissionToRole("editor", "document:write");
        var principal = CreatePrincipal("editor");

        var result = await _sut.CheckResourceAccessAsync(principal, "document", "doc-123", "write");

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task CheckResourceAccessAsync_UnauthorizedAction_Denies()
    {
        _sut.AssignPermissionToRole("viewer", "document:read");
        var principal = CreatePrincipal("viewer");

        var result = await _sut.CheckResourceAccessAsync(principal, "document", "doc-123", "delete");

        result.IsAuthorized.Should().BeFalse();
    }

    [Fact]
    public async Task CheckResourceAccessAsync_ResourceIdDoesNotAffectAuthorization()
    {
        _sut.AssignPermissionToRole("admin", "pipeline:execute");
        var principal = CreatePrincipal("admin");

        var result1 = await _sut.CheckResourceAccessAsync(principal, "pipeline", "pipe-1", "execute");
        var result2 = await _sut.CheckResourceAccessAsync(principal, "pipeline", "pipe-999", "execute");

        result1.IsAuthorized.Should().BeTrue();
        result2.IsAuthorized.Should().BeTrue();
    }

    // --- AssignPermissionToRole ---

    [Fact]
    public async Task AssignPermissionToRole_DuplicatePermission_DoesNotCauseDuplicateChecks()
    {
        _sut.AssignPermissionToRole("admin", "read");
        _sut.AssignPermissionToRole("admin", "read");
        var principal = CreatePrincipal("admin");

        var result = await _sut.CheckPermissionAsync(principal, "read");

        result.IsAuthorized.Should().BeTrue();
    }

    // --- RequireRoleForTool ---

    [Fact]
    public async Task RequireRoleForTool_DuplicateRole_DoesNotCauseIssues()
    {
        _sut.RequireRoleForTool("tool", "admin");
        _sut.RequireRoleForTool("tool", "admin");
        var principal = CreatePrincipal("admin");

        var result = await _sut.AuthorizeToolExecutionAsync(principal, "tool");

        result.IsAuthorized.Should().BeTrue();
    }

    // --- CancellationToken support ---

    [Fact]
    public async Task AuthorizeToolExecutionAsync_WithCancellationToken_Works()
    {
        using var cts = new CancellationTokenSource();

        var result = await _sut.AuthorizeToolExecutionAsync(CreatePrincipal("user"), "tool", null, cts.Token);

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPermissionAsync_WithCancellationToken_Works()
    {
        using var cts = new CancellationTokenSource();

        var result = await _sut.CheckPermissionAsync(CreatePrincipal("user"), "perm", cts.Token);

        result.IsAuthorized.Should().BeFalse();
    }

    [Fact]
    public async Task CheckResourceAccessAsync_WithCancellationToken_Works()
    {
        using var cts = new CancellationTokenSource();

        var result = await _sut.CheckResourceAccessAsync(
            CreatePrincipal("user"), "type", "id", "action", cts.Token);

        result.IsAuthorized.Should().BeFalse();
    }

    // --- Interface compliance ---

    [Fact]
    public void ImplementsIAuthorizationProvider()
    {
        _sut.Should().BeAssignableTo<IAuthorizationProvider>();
    }
}
