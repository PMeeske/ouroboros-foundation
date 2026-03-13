using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.SelfModel;

/// <summary>
/// Interface for cognitive bias detection and debiasing.
/// Monitors reasoning for common biases and applies correction strategies.
/// Based on Kahneman (2011) Dual Process Theory.
/// </summary>
public interface ICognitiveBiasMonitor
{
    /// <summary>
    /// Scans reasoning text for cognitive biases.
    /// </summary>
    /// <param name="reasoning">The reasoning text to scan.</param>
    /// <param name="context">Context in which the reasoning occurred.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of detected biases.</returns>
    Task<Result<List<BiasDetection>, string>> ScanForBiasesAsync(
        string reasoning, string context, CancellationToken ct = default);

    /// <summary>
    /// Applies a debiasing correction to reasoning affected by a detected bias.
    /// </summary>
    /// <param name="reasoning">The biased reasoning text.</param>
    /// <param name="detectedBias">The detected bias to correct for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Debiased reasoning text.</returns>
    Task<Result<string, string>> DebiasAsync(
        string reasoning, BiasDetection detectedBias, CancellationToken ct = default);

    /// <summary>
    /// Records whether a bias detection was accurate for calibration.
    /// </summary>
    /// <param name="biasId">The bias detection identifier.</param>
    /// <param name="wasActuallyBiased">Whether the reasoning was actually biased.</param>
    void RecordBiasOutcome(string biasId, bool wasActuallyBiased);

    /// <summary>
    /// Gets bias detection statistics.
    /// </summary>
    /// <returns>Bias detection statistics.</returns>
    BiasStats GetStats();
}

/// <summary>
/// Types of cognitive biases.
/// </summary>
[ExcludeFromCodeCoverage]
public enum BiasType
{
    /// <summary>Seeking information that confirms existing beliefs.</summary>
    ConfirmationBias,

    /// <summary>Over-relying on the first piece of information encountered.</summary>
    AnchoringBias,

    /// <summary>Overweighting easily recalled examples.</summary>
    AvailabilityHeuristic,

    /// <summary>Overestimating one's competence in areas of low ability.</summary>
    DunningKruger,

    /// <summary>Continuing investment due to past costs rather than future value.</summary>
    SunkCostFallacy,

    /// <summary>Letting one positive trait influence overall judgment.</summary>
    HaloEffect,

    /// <summary>Overweighting recent events in judgment.</summary>
    RecencyBias,

    /// <summary>Drawing different conclusions from the same data based on presentation.</summary>
    FramingEffect,

    /// <summary>Adopting beliefs because many others hold them.</summary>
    BandwagonEffect
}

/// <summary>
/// A detected cognitive bias in reasoning.
/// </summary>
/// <param name="Id">Unique detection identifier.</param>
/// <param name="Type">The type of bias detected.</param>
/// <param name="Confidence">Confidence in the detection (0.0 to 1.0).</param>
/// <param name="Evidence">Evidence supporting the detection.</param>
/// <param name="SuggestedCorrection">Suggested correction strategy.</param>
public sealed record BiasDetection(
    string Id, BiasType Type, double Confidence, string Evidence,
    string SuggestedCorrection);

/// <summary>
/// Statistics for cognitive bias detection.
/// </summary>
/// <param name="TotalScans">Total number of reasoning scans performed.</param>
/// <param name="DetectionsByType">Count of detections by bias type.</param>
/// <param name="FalsePositiveRate">Rate of false positive detections (0.0 to 1.0).</param>
/// <param name="TruePositiveRate">Rate of true positive detections (0.0 to 1.0).</param>
public sealed record BiasStats(
    int TotalScans, Dictionary<BiasType, int> DetectionsByType,
    double FalsePositiveRate, double TruePositiveRate);
