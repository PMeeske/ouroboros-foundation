using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.MetaLearning;

/// <summary>
/// Represents an example for few-shot task adaptation.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record TaskExample(
    string Input,
    string ExpectedOutput,
    Dictionary<string, object>? Context = null,
    double? Importance = null);
