namespace Ouroboros.Core.Learning;

/// <summary>
/// Types of feedback signals for continual learning.
/// </summary>
public enum FeedbackType
{
    /// <summary>
    /// User provided a correction to the generated output.
    /// </summary>
    UserCorrection,

    /// <summary>
    /// Signal indicating successful task completion.
    /// </summary>
    SuccessSignal,

    /// <summary>
    /// Signal indicating task failure.
    /// </summary>
    FailureSignal,

    /// <summary>
    /// Preference ranking between multiple outputs.
    /// </summary>
    PreferenceRanking,
}