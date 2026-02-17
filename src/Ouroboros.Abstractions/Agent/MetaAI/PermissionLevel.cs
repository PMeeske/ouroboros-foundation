namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Defines permission levels for agent actions.
/// </summary>
public enum PermissionLevel
{
    /// <summary>No permission granted.</summary>
    None = 0,

    /// <summary>Isolated execution with no external access.</summary>
    Isolated = 1,

    /// <summary>Read-only permission.</summary>
    Read = 2,

    /// <summary>Read-only permission (alias for Read).</summary>
    ReadOnly = 2,

    /// <summary>Write permission (includes Read).</summary>
    Write = 3,

    /// <summary>Execute permission.</summary>
    Execute = 4,

    /// <summary>User data access with confirmation required.</summary>
    UserDataWithConfirmation = 5,

    /// <summary>Full administrative access.</summary>
    Admin = 6,
}