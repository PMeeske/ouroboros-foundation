namespace Ouroboros.Domain.Voice;

/// <summary>
/// Agent internal thinking/reasoning displayed in real-time.
/// Shown dimmed to distinguish from actual response.
/// </summary>
public sealed record AgentThinkingEvent : InteractionEvent
{
    /// <summary>Gets the thought chunk text.</summary>
    public required string ThoughtChunk { get; init; }

    /// <summary>Gets the current phase of thinking.</summary>
    public ThinkingPhase Phase { get; init; } = ThinkingPhase.Reasoning;

    /// <summary>Gets whether this completes the thinking phase.</summary>
    public bool IsComplete { get; init; }
}