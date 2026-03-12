namespace Ouroboros.Agent.MetaAI.SelfImprovement;

/// <summary>
/// Interface for habit formation and procedural memory.
/// Tracks cue-routine-reward loops, automaticity, and routine building.
/// </summary>
public interface IHabitFormationEngine
{
    /// <summary>
    /// Records an action pattern as a cue-routine-reward loop.
    /// </summary>
    /// <param name="cue">The trigger cue.</param>
    /// <param name="routine">The routine performed.</param>
    /// <param name="reward">The reward received.</param>
    /// <param name="quality">Quality of the routine execution (0.0 to 1.0).</param>
    void RecordActionPattern(string cue, string routine, string reward, double quality);

    /// <summary>
    /// Suggests a habit to form based on observed patterns.
    /// </summary>
    /// <param name="context">Current context for habit suggestion.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Suggested habit.</returns>
    Task<Result<HabitSuggestion, string>> SuggestHabitAsync(
        string context, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a routine pattern has become automatic.
    /// </summary>
    /// <param name="routinePattern">The routine pattern to check.</param>
    /// <returns>True if the routine is automatic.</returns>
    bool IsAutomatic(string routinePattern);

    /// <summary>
    /// Gets all formed habits.
    /// </summary>
    /// <returns>List of formed habits.</returns>
    List<Habit> GetFormedHabits();

    /// <summary>
    /// Gets the automaticity score for a routine pattern (0.0 to 1.0).
    /// </summary>
    /// <param name="routinePattern">The routine pattern to query.</param>
    /// <returns>Automaticity score.</returns>
    double GetAutomaticityScore(string routinePattern);

    /// <summary>
    /// Gets habit formation statistics.
    /// </summary>
    /// <returns>Habit statistics.</returns>
    HabitStats GetStats();
}

/// <summary>
/// Represents a formed habit with its cue-routine-reward loop.
/// </summary>
/// <param name="Id">Unique habit identifier.</param>
/// <param name="Cue">The trigger cue.</param>
/// <param name="Routine">The routine performed.</param>
/// <param name="Reward">The reward received.</param>
/// <param name="RepetitionCount">Number of times the routine has been performed.</param>
/// <param name="AutomaticityScore">Degree of automaticity (0.0 to 1.0).</param>
/// <param name="AverageQuality">Average quality of routine execution (0.0 to 1.0).</param>
/// <param name="FirstPerformed">When the routine was first performed.</param>
/// <param name="LastPerformed">When the routine was most recently performed.</param>
public sealed record Habit(
    string Id, string Cue, string Routine, string Reward,
    int RepetitionCount, double AutomaticityScore, double AverageQuality,
    DateTime FirstPerformed, DateTime LastPerformed);

/// <summary>
/// Suggestion for a new habit to form.
/// </summary>
/// <param name="SuggestedRoutine">The routine to establish.</param>
/// <param name="TriggerCue">The cue that should trigger the routine.</param>
/// <param name="ExpectedReward">The expected reward.</param>
/// <param name="ConfidenceLevel">Confidence in the suggestion (0.0 to 1.0).</param>
/// <param name="Reasoning">Reasoning behind the suggestion.</param>
public sealed record HabitSuggestion(
    string SuggestedRoutine, string TriggerCue, string ExpectedReward,
    double ConfidenceLevel, string Reasoning);

/// <summary>
/// Statistics for habit formation.
/// </summary>
/// <param name="TotalHabits">Total number of tracked habits.</param>
/// <param name="AutomaticHabits">Number of habits that have become automatic.</param>
/// <param name="AverageRepetitionsToAutomatic">Average repetitions required to reach automaticity.</param>
/// <param name="MostUsedHabits">The most frequently used habits.</param>
public sealed record HabitStats(
    int TotalHabits, int AutomaticHabits, double AverageRepetitionsToAutomatic,
    List<Habit> MostUsedHabits);
