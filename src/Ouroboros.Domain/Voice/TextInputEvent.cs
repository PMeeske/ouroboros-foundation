namespace Ouroboros.Domain.Voice;

/// <summary>
/// Text typed by user via keyboard.
/// Can be partial (character-by-character streaming) or complete.
/// </summary>
public sealed record TextInputEvent : InteractionEvent
{
    /// <summary>Gets the text content.</summary>
    public required string Text { get; init; }

    /// <summary>Gets whether this is a partial input (streaming) or complete.</summary>
    public bool IsPartial { get; init; }
}