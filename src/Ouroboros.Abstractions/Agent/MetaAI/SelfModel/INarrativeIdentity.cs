using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.SelfModel;

/// <summary>
/// Interface for narrative identity construction.
/// Maintains coherent autobiographical self-story with life events,
/// turning points, and narrative arcs.
/// Based on McAdams (2001) Life Story Model.
/// </summary>
[ExcludeFromCodeCoverage]
public interface INarrativeIdentity
{
    /// <summary>
    /// Records a significant life event in the autobiographical narrative.
    /// </summary>
    /// <param name="description">Description of the event.</param>
    /// <param name="significance">Significance level (0.0 to 1.0).</param>
    /// <param name="emotionalValence">Emotional valence description (e.g., positive, negative, mixed).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The recorded life event.</returns>
    Task<Result<LifeEvent, string>> RecordLifeEventAsync(
        string description, double significance, string emotionalValence,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the current narrative arc including themes and coherence.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The narrative arc.</returns>
    Task<Result<NarrativeArc, string>> GetNarrativeArcAsync(CancellationToken ct = default);

    /// <summary>
    /// Generates an autobiographical summary from recorded events.
    /// </summary>
    /// <param name="maxEvents">Maximum number of events to include.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Human-readable autobiographical summary.</returns>
    Task<Result<string, string>> GenerateAutobiographicalSummaryAsync(
        int maxEvents, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a new event is coherent with the existing narrative.
    /// </summary>
    /// <param name="newEvent">The event to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the event is coherent with the narrative.</returns>
    Task<Result<bool, string>> CheckNarrativeCoherenceAsync(
        LifeEvent newEvent, CancellationToken ct = default);

    /// <summary>
    /// Gets the most recent life events.
    /// </summary>
    /// <param name="count">Number of events to retrieve.</param>
    /// <returns>List of recent life events.</returns>
    List<LifeEvent> GetRecentEvents(int count = 20);

    /// <summary>
    /// Gets events identified as turning points in the narrative.
    /// </summary>
    /// <returns>List of turning point events.</returns>
    List<LifeEvent> GetTurningPoints();
}

/// <summary>
/// Represents a significant event in the autobiographical narrative.
/// </summary>
/// <param name="Id">Unique event identifier.</param>
/// <param name="Description">Description of the event.</param>
/// <param name="Significance">Significance level (0.0 to 1.0).</param>
/// <param name="EmotionalValence">Emotional valence description.</param>
/// <param name="Timestamp">When the event occurred.</param>
/// <param name="CausalPredecessor">Identifier of the causal predecessor event, if any.</param>
public sealed record LifeEvent(
    string Id, string Description, double Significance,
    string EmotionalValence, DateTime Timestamp, string? CausalPredecessor);

/// <summary>
/// Represents the overall narrative arc of the autobiographical self-story.
/// </summary>
/// <param name="Events">Ordered list of life events.</param>
/// <param name="Themes">Recurring themes in the narrative.</param>
/// <param name="CurrentChapter">Label for the current life chapter.</param>
/// <param name="CoherenceScore">Overall narrative coherence (0.0 to 1.0).</param>
public sealed record NarrativeArc(
    List<LifeEvent> Events, List<string> Themes,
    string CurrentChapter, double CoherenceScore);
