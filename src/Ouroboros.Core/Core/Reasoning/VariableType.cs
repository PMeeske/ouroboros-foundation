namespace Ouroboros.Core.Reasoning;

/// <summary>
/// Defines the type of a variable in the causal graph.
/// </summary>
public enum VariableType
{
    /// <summary>
    /// Binary variable (true/false, 0/1).
    /// </summary>
    Binary,

    /// <summary>
    /// Categorical variable with discrete categories.
    /// </summary>
    Categorical,

    /// <summary>
    /// Continuous variable with real values.
    /// </summary>
    Continuous,

    /// <summary>
    /// Ordinal variable with ordered categories.
    /// </summary>
    Ordinal,
}