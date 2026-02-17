namespace Ouroboros.Domain.Voice;

/// <summary>
/// Agent response token/chunk streamed word-by-word or sentence-by-sentence.
/// </summary>
public sealed record AgentResponseEvent : InteractionEvent
{
    /// <summary>Gets the text chunk.</summary>
    public required string TextChunk { get; init; }

    /// <summary>Gets whether this completes the response.</summary>
    public bool IsComplete { get; init; }

    /// <summary>Gets the type of response content.</summary>
    public ResponseType Type { get; init; } = ResponseType.Direct;

    /// <summary>Gets whether this chunk ends a sentence (natural pause point for TTS).</summary>
    public bool IsSentenceEnd { get; init; }
}