
namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Interface for flow state detection and maintenance.
/// Models optimal experience through challenge-skill balance.
/// Based on Csikszentmihalyi (1990) Flow Theory.
/// </summary>
public interface IFlowStateEngine
{
    /// <summary>
    /// Assesses the current flow state based on skill, challenge, and absorption levels.
    /// </summary>
    /// <param name="skillLevel">Current skill level (0.0 to 1.0).</param>
    /// <param name="challengeLevel">Current challenge level (0.0 to 1.0).</param>
    /// <param name="absorption">Current absorption level (0.0 to 1.0).</param>
    /// <returns>Flow state assessment.</returns>
    FlowAssessment AssessFlowState(double skillLevel, double challengeLevel, double absorption);

    /// <summary>
    /// Records engagement with a task for flow tracking.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="challengeLevel">Challenge level of the task (0.0 to 1.0).</param>
    /// <param name="performanceQuality">Quality of performance (0.0 to 1.0).</param>
    void RecordTaskEngagement(string taskId, double challengeLevel, double performanceQuality);

    /// <summary>
    /// Optimizes task parameters to increase the probability of entering flow.
    /// </summary>
    /// <param name="taskDescription">Description of the task.</param>
    /// <param name="currentSkillLevel">Current skill level for the task (0.0 to 1.0).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Flow optimization recommendations.</returns>
    Task<Result<FlowOptimization, string>> OptimizeForFlowAsync(
        string taskDescription, double currentSkillLevel, CancellationToken ct = default);

    /// <summary>
    /// Gets the current flow state.
    /// </summary>
    /// <returns>Current flow state.</returns>
    FlowState GetCurrentState();

    /// <summary>
    /// Gets flow state statistics.
    /// </summary>
    /// <returns>Flow statistics.</returns>
    FlowStats GetStats();
}

/// <summary>
/// Flow states based on the challenge-skill model.
/// </summary>
public enum FlowState
{
    /// <summary>Low challenge, high skill.</summary>
    Boredom,

    /// <summary>Low challenge, low skill.</summary>
    Apathy,

    /// <summary>Moderate challenge, low skill.</summary>
    Worry,

    /// <summary>High challenge, low skill.</summary>
    Anxiety,

    /// <summary>High challenge, moderate skill.</summary>
    Arousal,

    /// <summary>High challenge matched by high skill — optimal experience.</summary>
    Flow,

    /// <summary>Moderate challenge, high skill.</summary>
    Control,

    /// <summary>Low challenge, moderate skill.</summary>
    Relaxation
}

/// <summary>
/// Assessment of the current flow state.
/// </summary>
/// <param name="State">The identified flow state.</param>
/// <param name="SkillLevel">Current skill level (0.0 to 1.0).</param>
/// <param name="ChallengeLevel">Current challenge level (0.0 to 1.0).</param>
/// <param name="Absorption">Level of absorption in the task (0.0 to 1.0).</param>
/// <param name="TimeDistortion">Degree of time distortion experienced (0.0 to 1.0).</param>
/// <param name="IntrinsicReward">Level of intrinsic reward (0.0 to 1.0).</param>
public sealed record FlowAssessment(
    FlowState State, double SkillLevel, double ChallengeLevel,
    double Absorption, double TimeDistortion, double IntrinsicReward);

/// <summary>
/// Recommendations for optimizing conditions for flow.
/// </summary>
/// <param name="RecommendedChallenge">Recommended challenge level (0.0 to 1.0).</param>
/// <param name="SuggestedAdjustment">Description of the suggested adjustment.</param>
/// <param name="PredictedFlowProbability">Predicted probability of entering flow (0.0 to 1.0).</param>
public sealed record FlowOptimization(
    double RecommendedChallenge, string SuggestedAdjustment, double PredictedFlowProbability);

/// <summary>
/// Statistics for flow state tracking.
/// </summary>
/// <param name="TotalFlowEpisodes">Total number of flow episodes recorded.</param>
/// <param name="AverageFlowDuration">Average duration of flow episodes.</param>
/// <param name="AverageFlowSkillLevel">Average skill level during flow episodes.</param>
/// <param name="FlowEntryRate">Rate of successful flow entries (0.0 to 1.0).</param>
public sealed record FlowStats(
    int TotalFlowEpisodes, TimeSpan AverageFlowDuration,
    double AverageFlowSkillLevel, double FlowEntryRate);
