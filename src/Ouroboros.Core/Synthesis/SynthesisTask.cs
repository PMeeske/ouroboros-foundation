namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Represents a synthesis task with examples and a DSL.
/// </summary>
/// <param name="Description">A description of the synthesis task.</param>
/// <param name="Examples">The input-output examples defining the task.</param>
/// <param name="DSL">The domain-specific language to use for synthesis.</param>
public sealed record SynthesisTask(
    string Description,
    List<InputOutputExample> Examples,
    DomainSpecificLanguage DSL);