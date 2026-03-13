using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.Social;

/// <summary>
/// Interface for intersubjectivity and common ground tracking.
/// Maintains shared understanding between Iaret and conversation partners.
/// Based on Clark (1996) Common Ground theory.
/// </summary>
public interface IIntersubjectivity
{
    /// <summary>
    /// Adds a proposition to the common ground with a specific person.
    /// </summary>
    /// <param name="personId">Identifier of the conversation partner.</param>
    /// <param name="proposition">The proposition to add.</param>
    /// <param name="method">How the proposition was grounded.</param>
    void AddToCommonGround(string personId, string proposition, GroundingMethod method);

    /// <summary>
    /// Checks whether a proposition is in the common ground with a specific person.
    /// </summary>
    /// <param name="personId">Identifier of the conversation partner.</param>
    /// <param name="proposition">The proposition to check.</param>
    /// <returns>True if the proposition is in the common ground.</returns>
    bool IsInCommonGround(string personId, string proposition);

    /// <summary>
    /// Gets all propositions in the common ground with a specific person.
    /// </summary>
    /// <param name="personId">Identifier of the conversation partner.</param>
    /// <returns>List of common ground propositions.</returns>
    List<string> GetCommonGround(string personId);

    /// <summary>
    /// Detects potential misunderstandings between utterance and response.
    /// </summary>
    /// <param name="personId">Identifier of the conversation partner.</param>
    /// <param name="utterance">The original utterance.</param>
    /// <param name="response">The response received.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Misunderstanding detection result.</returns>
    Task<Result<MisunderstandingDetection, string>> DetectMisunderstandingAsync(
        string personId, string utterance, string response, CancellationToken ct = default);

    /// <summary>
    /// Records whether a grounding attempt was successful.
    /// </summary>
    /// <param name="personId">Identifier of the conversation partner.</param>
    /// <param name="proposition">The proposition being grounded.</param>
    /// <param name="understood">Whether mutual understanding was achieved.</param>
    void RecordGroundingSuccess(string personId, string proposition, bool understood);
}

/// <summary>
/// Method by which a proposition was grounded in common knowledge.
/// </summary>
[ExcludeFromCodeCoverage]
public enum GroundingMethod
{
    /// <summary>Explicitly stated and acknowledged.</summary>
    Explicit,

    /// <summary>Implicitly conveyed through context.</summary>
    Implicit,

    /// <summary>Presupposed as shared background knowledge.</summary>
    Presupposed,

    /// <summary>Inferred from available evidence.</summary>
    Inferred
}

/// <summary>
/// Result of misunderstanding detection.
/// </summary>
/// <param name="MisunderstandingDetected">Whether a misunderstanding was detected.</param>
/// <param name="MisalignedProposition">The proposition that is misaligned.</param>
/// <param name="SuggestedClarification">Suggested clarification to resolve the misunderstanding.</param>
/// <param name="Confidence">Confidence in the detection (0.0 to 1.0).</param>
public sealed record MisunderstandingDetection(
    bool MisunderstandingDetected, string MisalignedProposition,
    string SuggestedClarification, double Confidence);
