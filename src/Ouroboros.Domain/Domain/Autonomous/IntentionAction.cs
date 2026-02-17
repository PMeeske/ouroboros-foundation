namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// The action details for an intention.
/// </summary>
public sealed record IntentionAction
{
    /// <summary>Type of action (tool, code_change, message, etc.).</summary>
    public required string ActionType { get; init; }

    /// <summary>Tool name if this is a tool invocation.</summary>
    public string? ToolName { get; init; }

    /// <summary>Tool input/arguments.</summary>
    public string? ToolInput { get; init; }

    /// <summary>File path if this is a code modification.</summary>
    public string? FilePath { get; init; }

    /// <summary>Code to replace (for modifications).</summary>
    public string? OldCode { get; init; }

    /// <summary>New code (for modifications).</summary>
    public string? NewCode { get; init; }

    /// <summary>Message content (for communications).</summary>
    public string? Message { get; init; }

    /// <summary>Additional parameters.</summary>
    public Dictionary<string, object> Parameters { get; init; } = [];
}