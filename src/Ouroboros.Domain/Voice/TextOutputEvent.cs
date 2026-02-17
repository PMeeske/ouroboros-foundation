namespace Ouroboros.Domain.Voice;

/// <summary>
/// Text displayed to console/UI.
/// </summary>
public sealed record TextOutputEvent : InteractionEvent
{
    /// <summary>Gets the text to display.</summary>
    public required string Text { get; init; }

    /// <summary>Gets the display style.</summary>
    public OutputStyle Style { get; init; } = OutputStyle.Normal;

    /// <summary>Gets whether to append (no newline) or write line.</summary>
    public bool Append { get; init; } = true;
}