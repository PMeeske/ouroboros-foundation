namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Represents a piece of evidence in a decision trail.
/// </summary>
public sealed record Evidence
{
    /// <summary>
    /// Gets the name of the criterion being evaluated.
    /// </summary>
    public string CriterionName { get; init; }

    /// <summary>
    /// Gets the evaluation result as a Form.
    /// </summary>
    public Form Evaluation { get; init; }

    /// <summary>
    /// Gets a human-readable description of the evidence.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Gets the timestamp when the evidence was collected.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Evidence"/> class.
    /// </summary>
    /// <param name="criterionName">The criterion name.</param>
    /// <param name="evaluation">The evaluation result.</param>
    /// <param name="description">The description.</param>
    /// <param name="timestamp">Optional timestamp.</param>
    public Evidence(
        string criterionName,
        Form evaluation,
        string description,
        DateTime? timestamp = null)
    {
        this.CriterionName = criterionName;
        this.Evaluation = evaluation;
        this.Description = description;
        this.Timestamp = timestamp ?? DateTime.UtcNow;
    }
}