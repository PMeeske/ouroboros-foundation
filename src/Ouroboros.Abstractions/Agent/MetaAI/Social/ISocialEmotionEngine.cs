namespace Ouroboros.Agent.MetaAI.Social;

/// <summary>
/// Interface for social emotion processing.
/// Models guilt, shame, pride, gratitude, empathy, and jealousy
/// with trigger conditions and regulation strategies.
/// Based on Gross's Process Model of emotion regulation.
/// </summary>
public interface ISocialEmotionEngine
{
    /// <summary>
    /// Evaluates a social situation and identifies the resulting social emotion.
    /// </summary>
    /// <param name="situation">Description of the social situation.</param>
    /// <param name="otherAgentState">State or behavior of the other agent.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The social emotion arising from the situation.</returns>
    Task<Result<SocialEmotion, string>> EvaluateSocialSituationAsync(
        string situation, string otherAgentState, CancellationToken ct = default);

    /// <summary>
    /// Applies a regulation strategy to modulate a social emotion.
    /// </summary>
    /// <param name="emotion">The emotion to regulate.</param>
    /// <param name="strategy">The regulation strategy to apply.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result of the regulation attempt.</returns>
    Task<Result<EmotionRegulationResult, string>> RegulateEmotionAsync(
        SocialEmotion emotion, RegulationStrategy strategy, CancellationToken ct = default);

    /// <summary>
    /// Generates an empathic response to another agent's emotional state.
    /// </summary>
    /// <param name="otherAgentEmotion">The other agent's perceived emotion.</param>
    /// <param name="context">Context of the interaction.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An empathy response with suggested action.</returns>
    Task<Result<EmpathyResponse, string>> GenerateEmpathyAsync(
        string otherAgentEmotion, string context, CancellationToken ct = default);

    /// <summary>
    /// Gets all currently active social emotions.
    /// </summary>
    /// <returns>List of active social emotions.</returns>
    List<SocialEmotion> GetActiveEmotions();

    /// <summary>
    /// Records whether an emotional response was contextually appropriate.
    /// </summary>
    /// <param name="emotionId">The emotion identifier.</param>
    /// <param name="wasAppropriate">Whether the emotion was appropriate.</param>
    void RecordEmotionOutcome(string emotionId, bool wasAppropriate);
}

/// <summary>
/// Types of social emotions.
/// </summary>
public enum SocialEmotionType
{
    /// <summary>Feeling of responsibility for a wrong.</summary>
    Guilt,

    /// <summary>Feeling of inadequacy or unworthiness.</summary>
    Shame,

    /// <summary>Feeling of satisfaction in one's achievements.</summary>
    Pride,

    /// <summary>Feeling of thankfulness for received benefits.</summary>
    Gratitude,

    /// <summary>Understanding and sharing another's feelings.</summary>
    Empathy,

    /// <summary>Resentment toward another's advantages.</summary>
    Jealousy,

    /// <summary>Concern for the suffering of others.</summary>
    Compassion
}

/// <summary>
/// Strategies for emotion regulation based on Gross's Process Model.
/// </summary>
public enum RegulationStrategy
{
    /// <summary>Reinterpreting the situation to change its emotional impact.</summary>
    Reappraisal,

    /// <summary>Inhibiting the expression of the emotion.</summary>
    Suppression,

    /// <summary>Redirecting attention away from the emotional trigger.</summary>
    Distraction,

    /// <summary>Modifying the situation to reduce emotional impact.</summary>
    SituationModification,

    /// <summary>Selectively attending to specific aspects of the situation.</summary>
    AttentionDeployment
}

/// <summary>
/// Represents a social emotion instance.
/// </summary>
/// <param name="Id">Unique emotion identifier.</param>
/// <param name="Type">The type of social emotion.</param>
/// <param name="Intensity">Intensity level (0.0 to 1.0).</param>
/// <param name="Trigger">What triggered the emotion.</param>
/// <param name="Context">Contextual information.</param>
/// <param name="Timestamp">When the emotion was recorded.</param>
public sealed record SocialEmotion(
    string Id, SocialEmotionType Type, double Intensity, string Trigger,
    string Context, DateTime Timestamp);

/// <summary>
/// Result of an emotion regulation attempt.
/// </summary>
/// <param name="OriginalEmotion">The type of emotion that was regulated.</param>
/// <param name="OriginalIntensity">Intensity before regulation.</param>
/// <param name="RegulatedIntensity">Intensity after regulation.</param>
/// <param name="UsedStrategy">The strategy that was applied.</param>
/// <param name="Success">Whether the regulation was effective.</param>
public sealed record EmotionRegulationResult(
    SocialEmotionType OriginalEmotion, double OriginalIntensity,
    double RegulatedIntensity, RegulationStrategy UsedStrategy, bool Success);

/// <summary>
/// Empathic response to another agent's emotion.
/// </summary>
/// <param name="TargetAgent">The agent being empathized with.</param>
/// <param name="PerceivedEmotion">The perceived emotional state.</param>
/// <param name="ResonanceStrength">How strongly the emotion resonates (0.0 to 1.0).</param>
/// <param name="SuggestedResponse">Suggested empathic response.</param>
public sealed record EmpathyResponse(
    string TargetAgent, string PerceivedEmotion, double ResonanceStrength,
    string SuggestedResponse);
