using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Speech request for the queue.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record SpeechRequest(
    string Text,
    string Persona,
    TaskCompletionSource<bool>? Completion = null);