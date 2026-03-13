
namespace Ouroboros.Agent;

/// <summary>
/// Classification of model types for orchestration decisions.
/// </summary>
public enum ModelType
{
    /// <summary>General-purpose model suitable for a broad range of tasks.</summary>
    General,

    /// <summary>Model specialised for code generation and understanding.</summary>
    Code,

    /// <summary>Model optimised for multi-step reasoning and logical inference.</summary>
    Reasoning,

    /// <summary>Model oriented towards creative writing and generation tasks.</summary>
    Creative,

    /// <summary>Model suited for producing concise summaries of longer content.</summary>
    Summary,

    /// <summary>Model focused on data analysis and structured interpretation tasks.</summary>
    Analysis
}
