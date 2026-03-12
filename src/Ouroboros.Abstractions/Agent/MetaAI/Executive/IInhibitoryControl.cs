namespace Ouroboros.Agent.MetaAI.Executive;

/// <summary>
/// Interface for cognitive inhibitory control.
/// Suppresses impulsive responses, filters irrelevant information,
/// and modulates autonomous action firing.
/// Based on Miyake et al. (2000) Executive Functions model.
/// </summary>
public interface IInhibitoryControl
{
    /// <summary>
    /// Evaluates whether a proposed action should be inhibited given the current context.
    /// </summary>
    /// <param name="proposedAction">The action being considered.</param>
    /// <param name="context">Current situational context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Inhibition evaluation result.</returns>
    Task<Result<InhibitionResult, string>> EvaluateResponseInhibitionAsync(
        string proposedAction, string context, CancellationToken ct = default);

    /// <summary>
    /// Determines whether a given impulse should be suppressed based on urgency.
    /// </summary>
    /// <param name="impulse">The impulse description.</param>
    /// <param name="urgency">Urgency level (0.0 to 1.0).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the impulse should be suppressed.</returns>
    Task<Result<bool, string>> ShouldSuppressAsync(
        string impulse, double urgency, CancellationToken ct = default);

    /// <summary>
    /// Records the outcome of an inhibition decision for calibration.
    /// </summary>
    /// <param name="actionId">The action identifier.</param>
    /// <param name="wasCorrectToInhibit">Whether the inhibition was correct in hindsight.</param>
    void RecordInhibitionOutcome(string actionId, bool wasCorrectToInhibit);

    /// <summary>
    /// Gets the current inhibition strength (0.0 = no inhibition, 1.0 = maximum).
    /// </summary>
    /// <returns>Current inhibition strength.</returns>
    double GetInhibitionStrength();

    /// <summary>
    /// Gets inhibition accuracy statistics.
    /// </summary>
    /// <returns>Inhibition statistics.</returns>
    InhibitionStats GetStats();
}

/// <summary>
/// Result of an inhibition evaluation.
/// </summary>
/// <param name="ShouldInhibit">Whether the action should be inhibited.</param>
/// <param name="Confidence">Confidence in the decision (0.0 to 1.0).</param>
/// <param name="Reason">Human-readable reason for the decision.</param>
/// <param name="SuggestedDelay">Suggested delay before allowing the action.</param>
public sealed record InhibitionResult(
    bool ShouldInhibit, double Confidence, string Reason, TimeSpan SuggestedDelay);

/// <summary>
/// Accuracy statistics for inhibitory control.
/// </summary>
/// <param name="TotalEvaluations">Total number of inhibition evaluations.</param>
/// <param name="CorrectInhibitions">Number of correct inhibitions (true positives).</param>
/// <param name="FalseAlarms">Number of false alarms (inhibited when should not have).</param>
/// <param name="Misses">Number of misses (did not inhibit when should have).</param>
/// <param name="Accuracy">Overall accuracy (0.0 to 1.0).</param>
public sealed record InhibitionStats(
    int TotalEvaluations, int CorrectInhibitions, int FalseAlarms,
    int Misses, double Accuracy);
