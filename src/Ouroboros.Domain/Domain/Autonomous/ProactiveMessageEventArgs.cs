namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Event args for proactive messages from Ouroboros.
/// </summary>
public sealed record ProactiveMessageEventArgs(
    string Message,
    IntentionPriority Priority,
    string Source,
    DateTime Timestamp);