namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Represents user information for authorization.
/// </summary>
public sealed record UserInfo
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public string UserId { get; init; }

    /// <summary>
    /// Gets the user's permissions/roles.
    /// </summary>
    public IReadOnlySet<string> Permissions { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserInfo"/> class.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="permissions">The user's permissions.</param>
    public UserInfo(string userId, IReadOnlySet<string> permissions)
    {
        this.UserId = userId;
        this.Permissions = permissions;
    }

    /// <summary>
    /// Checks if the user has a specific permission.
    /// </summary>
    /// <param name="permission">The permission to check.</param>
    /// <returns>True if the user has the permission.</returns>
    public bool HasPermission(string permission)
    {
        return this.Permissions.Contains(permission);
    }
}