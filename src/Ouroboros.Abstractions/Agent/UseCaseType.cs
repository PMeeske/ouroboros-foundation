using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent;

/// <summary>
/// Primary use case types for model selection.
/// </summary>
[ExcludeFromCodeCoverage]
public enum UseCaseType
{
    /// <summary>Generating or completing source code.</summary>
    CodeGeneration,

    /// <summary>Multi-step logical reasoning tasks.</summary>
    Reasoning,

    /// <summary>Creative writing, story generation, or open-ended generation.</summary>
    Creative,

    /// <summary>Condensing or abstracting larger texts into summaries.</summary>
    Summarization,

    /// <summary>Structured analysis of data, text, or artefacts.</summary>
    Analysis,

    /// <summary>Open-ended conversational interaction.</summary>
    Conversation,

    /// <summary>Tasks that require invoking external tools or function calls.</summary>
    ToolUse
}
