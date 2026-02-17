namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Speech request for the queue.
/// </summary>
public sealed record SpeechRequest(
    string Text,
    string Persona,
    TaskCompletionSource<bool>? Completion = null);