using Ouroboros.Core.Security.Authentication;

namespace Ouroboros.Core.Security.Authorization;

/// <summary>
/// Interface for authorization providers.
/// </summary>
public interface IAuthorizationProvider
{
    /// <summary>
    /// Checks if a principal is authorized to execute a tool.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<AuthorizationResult> AuthorizeToolExecutionAsync(
        AuthenticationPrincipal principal,
        string toolName,
        string? input = null,
        CancellationToken ct = default);

    /// <summary>
    /// Checks if a principal has a specific permission.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<AuthorizationResult> CheckPermissionAsync(
        AuthenticationPrincipal principal,
        string permission,
        CancellationToken ct = default);

    /// <summary>
    /// Checks if a principal can access a resource.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<AuthorizationResult> CheckResourceAccessAsync(
        AuthenticationPrincipal principal,
        string resourceType,
        string resourceId,
        string action,
        CancellationToken ct = default);
}