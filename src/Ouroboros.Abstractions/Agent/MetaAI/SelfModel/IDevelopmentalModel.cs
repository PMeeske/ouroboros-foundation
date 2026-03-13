
namespace Ouroboros.Agent.MetaAI.SelfModel;

/// <summary>
/// Interface for developmental stage tracking.
/// Models maturation, skill acquisition curves, and developmental milestones.
/// Based on Piaget's stages and Dreyfus Skill Acquisition Model.
/// </summary>
public interface IDevelopmentalModel
{
    /// <summary>
    /// Gets the current developmental stage for a specific domain.
    /// </summary>
    /// <param name="domain">The domain to query.</param>
    /// <returns>Current developmental stage.</returns>
    DevelopmentalStage GetCurrentStage(string domain);

    /// <summary>
    /// Checks whether a specific milestone has been achieved.
    /// </summary>
    /// <param name="domain">The domain of the milestone.</param>
    /// <param name="milestone">Description of the milestone.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the milestone has been achieved.</returns>
    Task<Result<bool, string>> CheckMilestoneAsync(
        string domain, string milestone, CancellationToken ct = default);

    /// <summary>
    /// Records a skill performance score for developmental tracking.
    /// </summary>
    /// <param name="domain">The domain of the skill.</param>
    /// <param name="performanceScore">Performance score (0.0 to 1.0).</param>
    void RecordSkillProgress(string domain, double performanceScore);

    /// <summary>
    /// Gets the current learning rate for a domain (higher = faster learning).
    /// </summary>
    /// <param name="domain">The domain to query.</param>
    /// <returns>Learning rate.</returns>
    double GetLearningRate(string domain);

    /// <summary>
    /// Gets developmental stages across all tracked domains.
    /// </summary>
    /// <returns>Mapping of domain to developmental stage.</returns>
    Dictionary<string, DevelopmentalStage> GetAllDomainStages();

    /// <summary>
    /// Gets milestones that have been achieved, optionally filtered by domain.
    /// </summary>
    /// <param name="domain">Optional domain filter.</param>
    /// <returns>List of achieved milestones.</returns>
    List<DevelopmentalMilestone> GetAchievedMilestones(string? domain = null);
}

/// <summary>
/// Developmental stages based on Dreyfus Skill Acquisition Model.
/// </summary>
public enum DevelopmentalStage
{
    /// <summary>Initial stage with no prior experience.</summary>
    Nascent,

    /// <summary>Building foundational understanding.</summary>
    Developing,

    /// <summary>Able to handle standard situations independently.</summary>
    Competent,

    /// <summary>Deep understanding with intuitive grasp.</summary>
    Proficient,

    /// <summary>Mastery with fluid, effortless performance.</summary>
    Expert,

    /// <summary>Transcendent understanding with meta-level insight.</summary>
    Wise
}

/// <summary>
/// Represents a developmental milestone achievement.
/// </summary>
/// <param name="Domain">The domain of the milestone.</param>
/// <param name="Description">Description of the milestone.</param>
/// <param name="Stage">The developmental stage at which it was achieved.</param>
/// <param name="AchievedAt">When the milestone was achieved.</param>
/// <param name="PerformanceAtAchievement">Performance score when achieved (0.0 to 1.0).</param>
public sealed record DevelopmentalMilestone(
    string Domain, string Description, DevelopmentalStage Stage,
    DateTime AchievedAt, double PerformanceAtAchievement);
