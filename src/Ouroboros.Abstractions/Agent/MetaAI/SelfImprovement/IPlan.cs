
namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Base interface shared by all plan types in the MetaAI subsystem.
/// </summary>
public interface IPlan
{
    /// <summary>
    /// Gets the goal this plan is intended to achieve.
    /// </summary>
    string Goal { get; }

    /// <summary>
    /// Gets the timestamp when this plan was created.
    /// </summary>
    DateTime CreatedAt { get; }
}
