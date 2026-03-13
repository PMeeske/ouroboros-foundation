using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.Executive;

/// <summary>
/// Interface for attention control with endogenous/exogenous distinction.
/// Manages sustained attention, divided attention, and attentional fatigue.
/// </summary>
public interface IAttentionController
{
    /// <summary>
    /// Allocates attention across multiple targets according to the specified mode.
    /// </summary>
    /// <param name="targets">Candidate targets for attention.</param>
    /// <param name="mode">Attention allocation mode.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Attention allocation across targets.</returns>
    Task<AttentionAllocation> AllocateAttentionAsync(
        List<AttentionTarget> targets, AttentionMode mode, CancellationToken ct = default);

    /// <summary>
    /// Records an attention capture event from a stimulus.
    /// </summary>
    /// <param name="stimulusId">Identifier of the stimulus.</param>
    /// <param name="salience">Salience of the stimulus (0.0 to 1.0).</param>
    /// <param name="source">Whether the capture was endogenous or exogenous.</param>
    void RecordAttentionCapture(string stimulusId, double salience, AttentionSource source);

    /// <summary>
    /// Gets the current quality of sustained attention (0.0 to 1.0).
    /// </summary>
    /// <returns>Sustained attention quality.</returns>
    double GetSustainedAttentionQuality();

    /// <summary>
    /// Gets the current attention fatigue level (0.0 = fresh, 1.0 = exhausted).
    /// </summary>
    /// <returns>Fatigue level.</returns>
    double GetAttentionFatigue();

    /// <summary>
    /// Resets attention fatigue to baseline.
    /// </summary>
    void ResetAttentionFatigue();

    /// <summary>
    /// Gets the current attention state snapshot.
    /// </summary>
    /// <returns>Current attention state.</returns>
    AttentionState GetCurrentState();
}

/// <summary>
/// Mode of attention allocation.
/// </summary>
[ExcludeFromCodeCoverage]
public enum AttentionMode
{
    /// <summary>Single-target focused attention.</summary>
    Focused,

    /// <summary>Multi-target divided attention.</summary>
    Divided,

    /// <summary>Broad environmental scanning.</summary>
    Scanning
}

/// <summary>
/// Source of attention direction.
/// </summary>
public enum AttentionSource
{
    /// <summary>Internally driven (goal-directed).</summary>
    Endogenous,

    /// <summary>Externally driven (stimulus-captured).</summary>
    Exogenous
}

/// <summary>
/// A candidate target for attention allocation.
/// </summary>
/// <param name="Id">Unique target identifier.</param>
/// <param name="Description">Human-readable target description.</param>
/// <param name="Priority">Priority level (higher = more important).</param>
/// <param name="Source">Whether attention is internally or externally driven.</param>
public sealed record AttentionTarget(
    string Id, string Description, double Priority, AttentionSource Source);

/// <summary>
/// Result of attention allocation across targets.
/// </summary>
/// <param name="Allocations">Mapping of target ID to allocated attention proportion.</param>
/// <param name="TotalCapacity">Total available attention capacity.</param>
/// <param name="UsedCapacity">Amount of capacity currently in use.</param>
public sealed record AttentionAllocation(
    Dictionary<string, double> Allocations, double TotalCapacity, double UsedCapacity);

/// <summary>
/// Snapshot of the current attention system state.
/// </summary>
/// <param name="FatigueLevel">Current fatigue level (0.0 to 1.0).</param>
/// <param name="SustainedQuality">Quality of sustained attention (0.0 to 1.0).</param>
/// <param name="ActiveTargets">Number of currently active attention targets.</param>
/// <param name="TimeSinceLastReset">Time since last fatigue reset.</param>
/// <param name="CurrentMode">Current attention mode.</param>
public sealed record AttentionState(
    double FatigueLevel, double SustainedQuality, int ActiveTargets,
    TimeSpan TimeSinceLastReset, AttentionMode CurrentMode);
