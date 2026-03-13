using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.Social;

/// <summary>
/// Interface for social cognition including norms, face management,
/// conversational maxims, and register adaptation.
/// Based on Grice's maxims and Brown &amp; Levinson politeness theory.
/// </summary>
public interface ISocialCognition
{
    /// <summary>
    /// Assesses the social context of an utterance within a relationship.
    /// </summary>
    /// <param name="utterance">The utterance to assess.</param>
    /// <param name="relationship">Description of the relationship with the interlocutor.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Social context assessment.</returns>
    Task<Result<SocialAssessment, string>> AssessSocialContextAsync(
        string utterance, string relationship, CancellationToken ct = default);

    /// <summary>
    /// Analyzes a proposed response for potential face threats.
    /// </summary>
    /// <param name="proposedResponse">The response being considered.</param>
    /// <param name="relationship">Description of the relationship.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Face threat analysis with mitigation suggestions.</returns>
    Task<Result<FaceThreatAnalysis, string>> AnalyzeFaceThreatAsync(
        string proposedResponse, string relationship, CancellationToken ct = default);

    /// <summary>
    /// Adapts a message to a target formality level.
    /// </summary>
    /// <param name="message">The message to adapt.</param>
    /// <param name="targetLevel">Target formality level.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The adapted message.</returns>
    Task<Result<string, string>> AdaptRegisterAsync(
        string message, FormalityLevel targetLevel, CancellationToken ct = default);

    /// <summary>
    /// Checks an utterance for violations of Grice's conversational maxims.
    /// </summary>
    /// <param name="utterance">The utterance to check.</param>
    /// <param name="context">Conversational context.</param>
    /// <returns>The violation detected, or null if none.</returns>
    GriceViolation? CheckGriceMaxims(string utterance, string context);

    /// <summary>
    /// Records the outcome of a social interaction for calibration.
    /// </summary>
    /// <param name="interactionId">The interaction identifier.</param>
    /// <param name="wasAppropriate">Whether the social behavior was appropriate.</param>
    void RecordSocialOutcome(string interactionId, bool wasAppropriate);
}

/// <summary>
/// Levels of formality in communication.
/// </summary>
[ExcludeFromCodeCoverage]
public enum FormalityLevel
{
    /// <summary>Used with very close relations.</summary>
    Intimate,

    /// <summary>Used with friends and peers.</summary>
    Casual,

    /// <summary>Used in professional contexts.</summary>
    Consultative,

    /// <summary>Used in official or ceremonial contexts.</summary>
    Formal,

    /// <summary>Used in fixed, ritualized contexts.</summary>
    Frozen
}

/// <summary>
/// Assessment of social context.
/// </summary>
/// <param name="DetectedFormality">The formality level detected in the interaction.</param>
/// <param name="RapportLevel">Level of rapport with the interlocutor (0.0 to 1.0).</param>
/// <param name="ApplicableNorms">Social norms applicable to this context.</param>
/// <param name="SocialScripts">Relevant social scripts for the situation.</param>
public sealed record SocialAssessment(
    FormalityLevel DetectedFormality, double RapportLevel,
    List<string> ApplicableNorms, List<string> SocialScripts);

/// <summary>
/// Analysis of face threats in a proposed response.
/// </summary>
/// <param name="PositiveFaceThreat">Threat to the hearer's positive face (0.0 to 1.0).</param>
/// <param name="NegativeFaceThreat">Threat to the hearer's negative face (0.0 to 1.0).</param>
/// <param name="RecommendedPolitenessStrategy">Recommended politeness strategy.</param>
/// <param name="MitigatedVersion">A face-threat-mitigated version of the response.</param>
public sealed record FaceThreatAnalysis(
    double PositiveFaceThreat, double NegativeFaceThreat,
    string RecommendedPolitenessStrategy, string MitigatedVersion);

/// <summary>
/// A violation of one of Grice's conversational maxims.
/// </summary>
/// <param name="Maxim">The maxim that was violated (Quantity, Quality, Relation, Manner).</param>
/// <param name="Description">Description of the violation.</param>
/// <param name="Severity">Severity of the violation (0.0 to 1.0).</param>
public sealed record GriceViolation(
    string Maxim, string Description, double Severity);
