// <copyright file="AffectModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Types of affective signals monitored by homeostasis policies.
/// </summary>
public enum SignalType
{
    /// <summary>Stress indicator from system load or failures.</summary>
    Stress,

    /// <summary>Confidence level in current task.</summary>
    Confidence,

    /// <summary>Curiosity-driven exploration drive.</summary>
    Curiosity,

    /// <summary>General valence (positive/negative affect).</summary>
    Valence,

    /// <summary>Arousal level (energy/activation).</summary>
    Arousal,

    /// <summary>Frustration from repeated failures.</summary>
    Frustration,

    /// <summary>Engagement level with current task.</summary>
    Engagement,

    /// <summary>Fatigue from extended operations.</summary>
    Fatigue,

    /// <summary>Satisfaction from successful outcomes.</summary>
    Satisfaction,

    /// <summary>Anxiety from uncertainty or risk.</summary>
    Anxiety,

    /// <summary>Custom signal type.</summary>
    Custom,
}

/// <summary>
/// Represents the current affective state of an agent.
/// </summary>
public sealed record AffectiveState(
    double Stress,
    double Confidence,
    double Curiosity,
    double Valence,
    double Arousal,
    double Frustration,
    double Engagement,
    double Fatigue,
    double Satisfaction,
    double Anxiety,
    DateTime Timestamp,
    Dictionary<string, double>? CustomSignals = null)
{
    /// <summary>
    /// Gets the value of a specific signal type.
    /// </summary>
    public double GetSignalValue(SignalType signal) => signal switch
    {
        SignalType.Stress => Stress,
        SignalType.Confidence => Confidence,
        SignalType.Curiosity => Curiosity,
        SignalType.Valence => Valence,
        SignalType.Arousal => Arousal,
        SignalType.Frustration => Frustration,
        SignalType.Engagement => Engagement,
        SignalType.Fatigue => Fatigue,
        SignalType.Satisfaction => Satisfaction,
        SignalType.Anxiety => Anxiety,
        SignalType.Custom => CustomSignals?.Values.FirstOrDefault() ?? 0.0,
        _ => 0.0,
    };
}

/// <summary>
/// Represents a valence signal measurement.
/// </summary>
public sealed record ValenceSignal(
    string Source,
    double Value,
    SignalType Type,
    DateTime Timestamp,
    TimeSpan? Duration);

/// <summary>
/// Result of stress detection using signal analysis.
/// </summary>
public sealed record StressDetectionResult(
    double StressLevel,
    double Frequency,
    double Amplitude,
    bool IsAnomalous,
    List<double> SpectralPeaks,
    string Analysis,
    DateTime DetectedAt);

/// <summary>
/// Configuration for affect monitoring.
/// </summary>
public sealed record AffectConfig(
    double StressThreshold = 0.7,
    double ConfidenceDecayRate = 0.01,
    double CuriosityBoostFactor = 0.2,
    int SignalHistorySize = 1000,
    int FourierWindowSize = 64);

/// <summary>
/// Interface for monitoring and computing synthetic affective states.
/// Tracks valence, stress, confidence, curiosity, and arousal.
/// </summary>
public interface IValenceMonitor
{
    /// <summary>
    /// Gets the current affective state snapshot.
    /// </summary>
    AffectiveState GetCurrentState();

    /// <summary>
    /// Records a valence signal from a source.
    /// </summary>
    /// <param name="source">Signal source identifier</param>
    /// <param name="value">Signal value (typically -1.0 to 1.0 for valence, 0.0 to 1.0 for others)</param>
    /// <param name="type">Type of signal</param>
    void RecordSignal(string source, double value, SignalType type);

    /// <summary>
    /// Computes stress level using Fourier-based signal analysis.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Stress detection result with spectral analysis</returns>
    Task<StressDetectionResult> DetectStressAsync(CancellationToken ct = default);

    /// <summary>
    /// Updates confidence based on task outcome.
    /// </summary>
    /// <param name="taskId">Task identifier</param>
    /// <param name="success">Whether the task succeeded</param>
    /// <param name="weight">Weight of this outcome (0.0 to 1.0)</param>
    void UpdateConfidence(string taskId, bool success, double weight = 1.0);

    /// <summary>
    /// Updates curiosity based on novelty detection.
    /// </summary>
    /// <param name="noveltyScore">Novelty score (0.0 to 1.0)</param>
    /// <param name="context">Context description</param>
    void UpdateCuriosity(double noveltyScore, string context);

    /// <summary>
    /// Gets recent signals of a specific type.
    /// </summary>
    /// <param name="type">Signal type</param>
    /// <param name="count">Number of signals to retrieve</param>
    /// <returns>List of recent signals</returns>
    List<ValenceSignal> GetRecentSignals(SignalType type, int count = 100);

    /// <summary>
    /// Gets the signal history for spectral analysis.
    /// </summary>
    /// <param name="type">Signal type</param>
    /// <returns>Array of signal values</returns>
    double[] GetSignalHistory(SignalType type);

    /// <summary>
    /// Gets the running average for a signal type.
    /// </summary>
    /// <param name="type">Signal type</param>
    /// <param name="windowSize">Window size for averaging</param>
    /// <returns>Running average value</returns>
    double GetRunningAverage(SignalType type, int windowSize = 10);

    /// <summary>
    /// Resets all affective states to baseline.
    /// </summary>
    void Reset();

    /// <summary>
    /// Gets affective state history.
    /// </summary>
    /// <param name="count">Number of states to retrieve</param>
    /// <returns>List of recent affective states</returns>
    List<AffectiveState> GetStateHistory(int count = 50);
}
