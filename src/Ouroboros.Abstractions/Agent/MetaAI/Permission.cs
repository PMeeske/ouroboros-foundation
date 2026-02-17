namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Represents a permission for an agent action.
/// </summary>
/// <param name="Resource">The resource being accessed.</param>
/// <param name="Level">The permission level required.</param>
/// <param name="Reason">Reason for the permission request.</param>
public sealed record Permission(
    string Resource,
    PermissionLevel Level,
    string Reason);