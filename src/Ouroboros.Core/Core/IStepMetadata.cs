namespace Ouroboros.Core;

/// <summary>
/// Marker interface for step metadata.
/// </summary>
[Obsolete("No implementations exist. Scheduled for removal.")]
public interface IStepMetadata
{
    /// <summary>
    /// Gets the name of the step.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the step.
    /// </summary>
    string Description { get; }
}