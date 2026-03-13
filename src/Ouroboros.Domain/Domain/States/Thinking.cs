using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.States;

/// <summary>
/// Represents the thinking/reasoning phase where the model analyzes the problem before drafting.
/// </summary>
[ExcludeFromCodeCoverage]
public record Thinking(string Text) : ReasoningState("Thinking", Text);
