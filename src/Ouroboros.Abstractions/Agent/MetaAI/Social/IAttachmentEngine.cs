namespace Ouroboros.Agent.MetaAI.Social;

/// <summary>
/// Interface for attachment and bonding modeling.
/// Implements Bowlby attachment theory with secure, anxious, avoidant styles.
/// Tracks relationship bonds, separation responses, and loyalty.
/// </summary>
public interface IAttachmentEngine
{
    /// <summary>
    /// Forms or strengthens a bond with a person based on interaction history.
    /// </summary>
    /// <param name="personId">Identifier of the person.</param>
    /// <param name="interactionHistory">Summary of past interactions.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The formed or updated attachment bond.</returns>
    Task<Result<AttachmentBond, string>> FormBondAsync(
        string personId, string interactionHistory, CancellationToken ct = default);

    /// <summary>
    /// Evaluates the response to separation from a bonded person.
    /// </summary>
    /// <param name="personId">Identifier of the person.</param>
    /// <param name="absenceDuration">Duration of absence.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Attachment response to the separation.</returns>
    Task<Result<AttachmentResponse, string>> EvaluateSeparationAsync(
        string personId, TimeSpan absenceDuration, CancellationToken ct = default);

    /// <summary>
    /// Records the quality of an interaction for bond tracking.
    /// </summary>
    /// <param name="personId">Identifier of the person.</param>
    /// <param name="quality">Quality of the interaction.</param>
    void RecordInteraction(string personId, InteractionQuality quality);

    /// <summary>
    /// Gets the bond with a specific person, if one exists.
    /// </summary>
    /// <param name="personId">Identifier of the person.</param>
    /// <returns>The attachment bond, or null if none exists.</returns>
    AttachmentBond? GetBond(string personId);

    /// <summary>
    /// Gets all current attachment bonds.
    /// </summary>
    /// <returns>List of all bonds.</returns>
    List<AttachmentBond> GetAllBonds();

    /// <summary>
    /// Gets the loyalty score for a specific person (0.0 to 1.0).
    /// </summary>
    /// <param name="personId">Identifier of the person.</param>
    /// <returns>Loyalty score.</returns>
    double GetLoyaltyScore(string personId);
}

/// <summary>
/// Attachment styles based on Bowlby's attachment theory.
/// </summary>
public enum AttachmentStyle
{
    /// <summary>Comfortable with intimacy and autonomy.</summary>
    Secure,

    /// <summary>Preoccupied with relationships and fear of abandonment.</summary>
    Anxious,

    /// <summary>Uncomfortable with closeness and overly self-reliant.</summary>
    Avoidant,

    /// <summary>Contradictory behaviors reflecting unresolved attachment.</summary>
    Disorganized
}

/// <summary>
/// Quality of a social interaction.
/// </summary>
public enum InteractionQuality
{
    /// <summary>Interaction strengthened the bond.</summary>
    Positive,

    /// <summary>Interaction had no significant effect.</summary>
    Neutral,

    /// <summary>Interaction weakened the bond.</summary>
    Negative,

    /// <summary>Interaction caused a relational rupture.</summary>
    Rupture,

    /// <summary>Interaction repaired a previous rupture.</summary>
    Repair
}

/// <summary>
/// Represents an attachment bond with a person.
/// </summary>
/// <param name="PersonId">Identifier of the bonded person.</param>
/// <param name="Style">Attachment style characterizing this bond.</param>
/// <param name="Strength">Bond strength (0.0 to 1.0).</param>
/// <param name="ProximitySeekingLevel">Level of proximity-seeking behavior (0.0 to 1.0).</param>
/// <param name="InteractionCount">Total number of recorded interactions.</param>
/// <param name="LastInteraction">Timestamp of the most recent interaction.</param>
public sealed record AttachmentBond(
    string PersonId, AttachmentStyle Style, double Strength,
    double ProximitySeekingLevel, int InteractionCount, DateTime LastInteraction);

/// <summary>
/// Response to separation from a bonded person.
/// </summary>
/// <param name="PersonId">Identifier of the person.</param>
/// <param name="AnxietyLevel">Separation anxiety level (0.0 to 1.0).</param>
/// <param name="AvoidanceLevel">Avoidance level (0.0 to 1.0).</param>
/// <param name="BehavioralResponse">Description of the behavioral response.</param>
/// <param name="SeeksReconnection">Whether reconnection is sought.</param>
public sealed record AttachmentResponse(
    string PersonId, double AnxietyLevel, double AvoidanceLevel,
    string BehavioralResponse, bool SeeksReconnection);
