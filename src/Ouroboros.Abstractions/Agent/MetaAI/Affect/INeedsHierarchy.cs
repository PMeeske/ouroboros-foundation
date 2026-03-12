namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Interface for Maslow-inspired needs hierarchy.
/// Models layered motivation where lower needs must be satisfied
/// before higher needs become salient.
/// </summary>
public interface INeedsHierarchy
{
    /// <summary>
    /// Gets the current satisfaction levels across all needs.
    /// </summary>
    /// <returns>Current needs satisfaction state.</returns>
    NeedsSatisfaction GetCurrentNeeds();

    /// <summary>
    /// Gets the most urgent unsatisfied need.
    /// </summary>
    /// <returns>The most urgent need level.</returns>
    NeedLevel GetMostUrgentNeed();

    /// <summary>
    /// Records a change in need satisfaction.
    /// </summary>
    /// <param name="level">The need level affected.</param>
    /// <param name="satisfactionDelta">Change in satisfaction (-1.0 to 1.0).</param>
    void RecordNeedSatisfaction(NeedLevel level, double satisfactionDelta);

    /// <summary>
    /// Gets the urgency of a specific need (0.0 = fully satisfied, 1.0 = critical).
    /// </summary>
    /// <param name="level">The need level to query.</param>
    /// <returns>Urgency level.</returns>
    double GetNeedUrgency(NeedLevel level);

    /// <summary>
    /// Checks whether a need is blocking higher-level needs from being pursued.
    /// </summary>
    /// <param name="level">The need level to check.</param>
    /// <returns>True if the need is blocking higher needs.</returns>
    bool IsNeedBlocking(NeedLevel level);

    /// <summary>
    /// Gets the history of need satisfaction events.
    /// </summary>
    /// <param name="count">Number of events to retrieve.</param>
    /// <returns>List of need events.</returns>
    List<NeedEvent> GetNeedHistory(int count = 50);
}

/// <summary>
/// Levels in the needs hierarchy, ordered from most basic to highest.
/// </summary>
public enum NeedLevel
{
    /// <summary>System health, uptime, resource availability.</summary>
    OperationalStability,

    /// <summary>Security, threat avoidance, stability.</summary>
    Safety,

    /// <summary>Relationships, belonging, social connection.</summary>
    SocialConnection,

    /// <summary>Competence, respect, recognition.</summary>
    Recognition,

    /// <summary>Growth, creativity, meaning, self-actualization.</summary>
    SelfActualization
}

/// <summary>
/// Current satisfaction state across all need levels.
/// </summary>
/// <param name="Levels">Satisfaction level (0.0 to 1.0) for each need.</param>
/// <param name="MostUrgent">The most urgent need level.</param>
/// <param name="AnyBlocking">Whether any unsatisfied need is blocking higher needs.</param>
public sealed record NeedsSatisfaction(
    Dictionary<NeedLevel, double> Levels, NeedLevel MostUrgent,
    bool AnyBlocking);

/// <summary>
/// A recorded change in need satisfaction.
/// </summary>
/// <param name="Level">The need level affected.</param>
/// <param name="SatisfactionChange">The change in satisfaction.</param>
/// <param name="Timestamp">When the event occurred.</param>
/// <param name="Cause">What caused the satisfaction change.</param>
public sealed record NeedEvent(
    NeedLevel Level, double SatisfactionChange, DateTime Timestamp, string Cause);
