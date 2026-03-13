
namespace Ouroboros.Agent.MetaAI.SelfModel;

/// <summary>
/// Interface for sense of agency modeling.
/// Tracks voluntary vs involuntary actions, prediction-outcome matching,
/// and authorship attribution.
/// Based on Wegner (2002) Apparent Mental Causation.
/// </summary>
public interface IAgencyModel
{
    /// <summary>
    /// Records a voluntary action along with its predicted outcome.
    /// </summary>
    /// <param name="actionId">Unique action identifier.</param>
    /// <param name="predictedOutcome">The predicted outcome of the action.</param>
    void RecordVoluntaryAction(string actionId, string predictedOutcome);

    /// <summary>
    /// Records the actual outcome of a previously registered action.
    /// </summary>
    /// <param name="actionId">The action identifier.</param>
    /// <param name="actualOutcome">The actual observed outcome.</param>
    void RecordActionOutcome(string actionId, string actualOutcome);

    /// <summary>
    /// Gets the agency score for a specific action (0.0 to 1.0).
    /// </summary>
    /// <param name="actionId">The action identifier.</param>
    /// <returns>Agency score based on prediction-outcome match.</returns>
    double GetAgencyScore(string actionId);

    /// <summary>
    /// Gets the overall sense of agency across all actions (0.0 to 1.0).
    /// </summary>
    /// <returns>Overall agency score.</returns>
    double GetOverallAgencyScore();

    /// <summary>
    /// Attributes the type and degree of agency for an action.
    /// </summary>
    /// <param name="action">Description of the action.</param>
    /// <param name="context">Context in which the action occurred.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Agency attribution result.</returns>
    Task<Result<AgencyAttribution, string>> AttributeAgencyAsync(
        string action, string context, CancellationToken ct = default);

    /// <summary>
    /// Gets agency statistics.
    /// </summary>
    /// <returns>Agency statistics.</returns>
    AgencyStats GetStats();
}

/// <summary>
/// Types of agency based on the voluntariness of the action.
/// </summary>
public enum AgencyType
{
    /// <summary>Deliberately chosen action.</summary>
    Voluntary,

    /// <summary>Action in response to an external stimulus.</summary>
    Reactive,

    /// <summary>Action triggered by a condition or rule.</summary>
    Triggered,

    /// <summary>Automatic action without deliberation.</summary>
    Reflexive
}

/// <summary>
/// Attribution of agency for a specific action.
/// </summary>
/// <param name="ActionId">The action identifier.</param>
/// <param name="Type">The type of agency attributed.</param>
/// <param name="AgencyScore">Degree of agency (0.0 to 1.0).</param>
/// <param name="PredictionAccuracy">How well the outcome matched the prediction (0.0 to 1.0).</param>
/// <param name="Narrative">Narrative explanation of the agency attribution.</param>
public sealed record AgencyAttribution(
    string ActionId, AgencyType Type, double AgencyScore,
    double PredictionAccuracy, string Narrative);

/// <summary>
/// Aggregate statistics for sense of agency.
/// </summary>
/// <param name="TotalActions">Total number of tracked actions.</param>
/// <param name="AverageAgencyScore">Average agency score across all actions.</param>
/// <param name="ActionsByType">Count of actions by agency type.</param>
/// <param name="PredictionAccuracy">Overall prediction accuracy (0.0 to 1.0).</param>
public sealed record AgencyStats(
    int TotalActions, double AverageAgencyScore,
    Dictionary<AgencyType, int> ActionsByType, double PredictionAccuracy);
