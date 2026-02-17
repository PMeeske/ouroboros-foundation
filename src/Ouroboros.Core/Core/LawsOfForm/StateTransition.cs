namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Represents a state transition in the machine's history.
/// </summary>
/// <typeparam name="TState">The state type.</typeparam>
/// <param name="Timestamp">When the transition occurred.</param>
/// <param name="Form">The form state after transition.</param>
/// <param name="State">The certain state (if applicable).</param>
/// <param name="Reason">The reason for the transition.</param>
public record StateTransition<TState>(
    DateTimeOffset Timestamp,
    Form Form,
    Option<TState> State,
    string Reason)
    where TState : notnull;