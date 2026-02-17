namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Represents an input-output example for program synthesis.
/// </summary>
/// <param name="Input">The input value for the example.</param>
/// <param name="ExpectedOutput">The expected output value for the given input.</param>
/// <param name="TimeoutSeconds">Optional timeout for execution of this example.</param>
public sealed record InputOutputExample(
    object Input,
    object ExpectedOutput,
    double? TimeoutSeconds = null);