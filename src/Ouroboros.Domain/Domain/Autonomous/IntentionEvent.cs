namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Event fired when an intention status changes.
/// </summary>
public sealed record IntentionEvent(
    Intention Intention,
    IntentionStatus OldStatus,
    IntentionStatus NewStatus,
    DateTime Timestamp);