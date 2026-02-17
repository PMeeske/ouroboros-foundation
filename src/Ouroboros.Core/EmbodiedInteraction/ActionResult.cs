namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Result of an action.
/// </summary>
/// <param name="Request">The original request.</param>
/// <param name="Success">Whether action succeeded.</param>
/// <param name="Error">Error message if failed.</param>
/// <param name="Duration">How long the action took.</param>
public sealed record ActionResult(
    ActionRequest Request,
    bool Success,
    string? Error = null,
    TimeSpan? Duration = null);