namespace Ouroboros.Agent.MetaAI.SelfImprovement;

/// <summary>
/// Interface for computational creativity.
/// Implements divergent thinking, conceptual blending, bisociation,
/// and creative evaluation.
/// </summary>
public interface ICreativityEngine
{
    /// <summary>
    /// Generates multiple creative ideas for a problem using divergent thinking.
    /// </summary>
    /// <param name="problem">The problem to generate ideas for.</param>
    /// <param name="numberOfIdeas">Number of ideas to generate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of creative ideas.</returns>
    Task<Result<List<CreativeIdea>, string>> DivergentThinkAsync(
        string problem, int numberOfIdeas, CancellationToken ct = default);

    /// <summary>
    /// Blends two concepts to produce a novel combined concept.
    /// </summary>
    /// <param name="conceptA">The first concept.</param>
    /// <param name="conceptB">The second concept.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The conceptual blend result.</returns>
    Task<Result<ConceptualBlend, string>> BlendConceptsAsync(
        string conceptA, string conceptB, CancellationToken ct = default);

    /// <summary>
    /// Finds bisociations (unexpected connections) between two domains.
    /// </summary>
    /// <param name="domainA">The first domain.</param>
    /// <param name="domainB">The second domain.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Bisociation result with connections.</returns>
    Task<Result<BisociationResult, string>> FindBisociationsAsync(
        string domainA, string domainB, CancellationToken ct = default);

    /// <summary>
    /// Evaluates the creativity of an idea based on novelty, value, and surprise.
    /// </summary>
    /// <param name="idea">The idea to evaluate.</param>
    /// <param name="context">Context for evaluation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Creativity score.</returns>
    Task<Result<CreativityScore, string>> EvaluateCreativityAsync(
        string idea, string context, CancellationToken ct = default);
}

/// <summary>
/// A creative idea generated through divergent thinking.
/// </summary>
/// <param name="Id">Unique idea identifier.</param>
/// <param name="Description">Description of the idea.</param>
/// <param name="NoveltyScore">How novel the idea is (0.0 to 1.0).</param>
/// <param name="ValueScore">How valuable the idea is (0.0 to 1.0).</param>
/// <param name="SurpriseScore">How surprising the idea is (0.0 to 1.0).</param>
public sealed record CreativeIdea(
    string Id, string Description, double NoveltyScore,
    double ValueScore, double SurpriseScore);

/// <summary>
/// Result of blending two concepts.
/// </summary>
/// <param name="ConceptA">The first input concept.</param>
/// <param name="ConceptB">The second input concept.</param>
/// <param name="BlendedConcept">The resulting blended concept.</param>
/// <param name="Mappings">Structural mappings between the input concepts.</param>
/// <param name="BlendQuality">Quality of the blend (0.0 to 1.0).</param>
public sealed record ConceptualBlend(
    string ConceptA, string ConceptB, string BlendedConcept,
    List<string> Mappings, double BlendQuality);

/// <summary>
/// Result of bisociation search between two domains.
/// </summary>
/// <param name="DomainA">The first domain.</param>
/// <param name="DomainB">The second domain.</param>
/// <param name="Connections">Unexpected connections found.</param>
/// <param name="SurpriseLevel">Overall surprise level (0.0 to 1.0).</param>
/// <param name="MostNovelConnection">The most novel connection found.</param>
public sealed record BisociationResult(
    string DomainA, string DomainB, List<string> Connections,
    double SurpriseLevel, string MostNovelConnection);

/// <summary>
/// Multi-dimensional creativity score.
/// </summary>
/// <param name="Novelty">Novelty score (0.0 to 1.0).</param>
/// <param name="Value">Value score (0.0 to 1.0).</param>
/// <param name="Surprise">Surprise score (0.0 to 1.0).</param>
/// <param name="OverallCreativity">Combined creativity score (0.0 to 1.0).</param>
public sealed record CreativityScore(
    double Novelty, double Value, double Surprise,
    double OverallCreativity);
