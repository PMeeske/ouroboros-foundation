
namespace Ouroboros.Agent.MetaAI.SelfModel;

/// <summary>
/// Interface for temporal self-continuity.
/// Tracks past self, present self, and projected future self as unified entity.
/// </summary>
public interface ITemporalSelfModel
{
    /// <summary>
    /// Captures a snapshot of the current self state.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Snapshot of the current self.</returns>
    Task<SelfSnapshot> CaptureCurrentSelfAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the trajectory of self across time.
    /// </summary>
    /// <param name="maxSnapshots">Maximum number of snapshots to include.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Self trajectory with growth analysis.</returns>
    Task<Result<SelfTrajectory, string>> GetSelfTrajectoryAsync(
        int maxSnapshots, CancellationToken ct = default);

    /// <summary>
    /// Projects the future self based on current trends.
    /// </summary>
    /// <param name="horizon">Time horizon for the projection.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Projected future self state.</returns>
    Task<Result<SelfProjection, string>> ProjectFutureSelfAsync(
        TimeSpan horizon, CancellationToken ct = default);

    /// <summary>
    /// Measures the coherence of self across time (0.0 to 1.0).
    /// </summary>
    /// <returns>Temporal coherence score.</returns>
    double MeasureTemporalCoherence();

    /// <summary>
    /// Gets recent self snapshots.
    /// </summary>
    /// <param name="count">Number of snapshots to retrieve.</param>
    /// <returns>List of recent snapshots.</returns>
    List<SelfSnapshot> GetSnapshots(int count = 10);
}

/// <summary>
/// Snapshot of the self at a point in time.
/// </summary>
/// <param name="Id">Unique snapshot identifier.</param>
/// <param name="Timestamp">When the snapshot was taken.</param>
/// <param name="Capabilities">Capability name to proficiency level mapping.</param>
/// <param name="Beliefs">Belief name to belief content mapping.</param>
/// <param name="PersonalityTraits">Trait name to trait strength mapping.</param>
public sealed record SelfSnapshot(
    string Id, DateTime Timestamp, Dictionary<string, double> Capabilities,
    Dictionary<string, string> Beliefs, Dictionary<string, double> PersonalityTraits);

/// <summary>
/// Trajectory of self change across multiple snapshots.
/// </summary>
/// <param name="Snapshots">Ordered list of self snapshots.</param>
/// <param name="GrowthRates">Capability name to growth rate mapping.</param>
/// <param name="EmergingCapabilities">Capabilities that are newly appearing.</param>
/// <param name="DecliningCapabilities">Capabilities that are declining.</param>
public sealed record SelfTrajectory(
    List<SelfSnapshot> Snapshots, Dictionary<string, double> GrowthRates,
    List<string> EmergingCapabilities, List<string> DecliningCapabilities);

/// <summary>
/// Projection of the future self.
/// </summary>
/// <param name="ProjectedSelf">The projected self snapshot.</param>
/// <param name="Horizon">Time horizon of the projection.</param>
/// <param name="ConfidenceLevel">Confidence in the projection (0.0 to 1.0).</param>
/// <param name="KeyUncertainties">Major sources of uncertainty in the projection.</param>
public sealed record SelfProjection(
    SelfSnapshot ProjectedSelf, TimeSpan Horizon,
    double ConfidenceLevel, List<string> KeyUncertainties);
