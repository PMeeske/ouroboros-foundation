namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Represents an autonomous intention that Ouroboros wants to execute.
/// Intentions require approval before execution in push-based mode.
/// </summary>
public sealed record Intention
{
    /// <summary>Unique identifier for this intention.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Human-readable title describing the intention.</summary>
    public required string Title { get; init; }

    /// <summary>Detailed description of what Ouroboros wants to do.</summary>
    public required string Description { get; init; }

    /// <summary>Why this action would be beneficial.</summary>
    public required string Rationale { get; init; }

    /// <summary>The category of this intention.</summary>
    public required IntentionCategory Category { get; init; }

    /// <summary>Priority level.</summary>
    public IntentionPriority Priority { get; init; } = IntentionPriority.Normal;

    /// <summary>When this intention was created.</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>When this intention expires (null = no expiry).</summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>The source neuron/component that generated this intention.</summary>
    public required string Source { get; init; }

    /// <summary>Target neuron/component (if applicable).</summary>
    public string? Target { get; init; }

    /// <summary>Structured action data (tool name, parameters, etc.).</summary>
    public IntentionAction? Action { get; init; }

    /// <summary>Expected outcomes or results.</summary>
    public List<string> ExpectedOutcomes { get; init; } = [];

    /// <summary>Potential risks or concerns.</summary>
    public List<string> Risks { get; init; } = [];

    /// <summary>Whether this intention requires explicit user approval.</summary>
    public bool RequiresApproval { get; init; } = true;

    /// <summary>Current status of this intention.</summary>
    public IntentionStatus Status { get; init; } = IntentionStatus.Pending;

    /// <summary>Optional user comment from approval/rejection.</summary>
    public string? UserComment { get; init; }

    /// <summary>When this intention was acted upon.</summary>
    public DateTime? ActedAt { get; init; }

    /// <summary>Result of execution (if executed).</summary>
    public string? ExecutionResult { get; init; }

    /// <summary>Vector embedding for semantic search.</summary>
    public float[]? Embedding { get; init; }

    /// <summary>Metadata for extensibility.</summary>
    public Dictionary<string, object> Metadata { get; init; } = [];
}