using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.SelfImprovement;

/// <summary>
/// Interface for multi-framework moral reasoning.
/// Applies deontological, utilitarian, virtue ethics, and care ethics
/// perspectives to moral dilemmas.
/// </summary>
public interface IMoralReasoningEngine
{
    /// <summary>
    /// Evaluates an action from multiple ethical frameworks.
    /// </summary>
    /// <param name="action">The action to evaluate.</param>
    /// <param name="context">Context in which the action occurs.</param>
    /// <param name="stakeholders">Stakeholders affected by the action.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Moral judgment with multi-framework analysis.</returns>
    Task<Result<MoralJudgment, string>> EvaluateAsync(
        string action, string context, List<string> stakeholders,
        CancellationToken ct = default);

    /// <summary>
    /// Deliberates over a moral dilemma by synthesizing multiple framework perspectives.
    /// </summary>
    /// <param name="dilemma">The moral dilemma to deliberate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Deliberation result with synthesized verdict.</returns>
    Task<Result<MoralDeliberation, string>> DeliberateAsync(
        MoralDilemma dilemma, CancellationToken ct = default);

    /// <summary>
    /// Records the outcome of a moral judgment for developmental tracking.
    /// </summary>
    /// <param name="judgmentId">The judgment identifier.</param>
    /// <param name="wasEthical">Whether the action was considered ethical in hindsight.</param>
    void RecordMoralOutcome(string judgmentId, bool wasEthical);

    /// <summary>
    /// Gets the current moral development level.
    /// </summary>
    /// <returns>Current moral development level.</returns>
    MoralDevelopmentLevel GetDevelopmentLevel();
}

/// <summary>
/// Ethical frameworks for moral reasoning.
/// </summary>
[ExcludeFromCodeCoverage]
public enum MoralFramework
{
    /// <summary>Rule-based ethics focusing on duty and rights.</summary>
    Deontological,

    /// <summary>Consequence-based ethics maximizing overall well-being.</summary>
    Utilitarian,

    /// <summary>Character-based ethics focusing on virtues and flourishing.</summary>
    VirtueEthics,

    /// <summary>Relationship-based ethics focusing on care and responsibility.</summary>
    CareEthics
}

/// <summary>
/// Moral development levels based on Kohlberg's stages.
/// </summary>
public enum MoralDevelopmentLevel
{
    /// <summary>Rule-following based on consequences to self.</summary>
    PreConventional,

    /// <summary>Rule-following based on social conformity and order.</summary>
    Conventional,

    /// <summary>Principled reasoning based on universal ethical principles.</summary>
    PostConventional
}

/// <summary>
/// Represents a moral dilemma with conflicting values.
/// </summary>
/// <param name="Description">Description of the dilemma.</param>
/// <param name="Stakeholders">Stakeholders affected.</param>
/// <param name="Options">Available action options.</param>
/// <param name="ConflictingValues">Values that are in conflict.</param>
public sealed record MoralDilemma(
    string Description, List<string> Stakeholders,
    List<string> Options, List<string> ConflictingValues);

/// <summary>
/// A moral judgment from a specific ethical framework.
/// </summary>
/// <param name="Id">Unique judgment identifier.</param>
/// <param name="Action">The action being judged.</param>
/// <param name="PrimaryFramework">The primary framework used.</param>
/// <param name="Verdict">The ethical verdict.</param>
/// <param name="Confidence">Confidence in the verdict (0.0 to 1.0).</param>
/// <param name="Reasoning">Reasoning behind the verdict.</param>
/// <param name="FrameworkVerdicts">Verdicts from each ethical framework.</param>
public sealed record MoralJudgment(
    string Id, string Action, MoralFramework PrimaryFramework,
    string Verdict, double Confidence, string Reasoning,
    Dictionary<MoralFramework, string> FrameworkVerdicts);

/// <summary>
/// Result of moral deliberation across multiple frameworks.
/// </summary>
/// <param name="Dilemma">The dilemma that was deliberated.</param>
/// <param name="FrameworkJudgments">Individual framework judgments.</param>
/// <param name="SynthesizedVerdict">Synthesized verdict across all frameworks.</param>
/// <param name="ConsensusLevel">Level of agreement across frameworks (0.0 to 1.0).</param>
public sealed record MoralDeliberation(
    MoralDilemma Dilemma, List<MoralJudgment> FrameworkJudgments,
    string SynthesizedVerdict, double ConsensusLevel);
