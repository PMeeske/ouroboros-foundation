// <copyright file="SynthesisTypes.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Represents a synthesized program with its source code, AST, and metadata.
/// </summary>
/// <param name="SourceCode">The source code representation of the program.</param>
/// <param name="AST">The abstract syntax tree of the program.</param>
/// <param name="Language">The domain-specific language used to write the program.</param>
/// <param name="LogProbability">The log probability of this program under the learned model.</param>
/// <param name="Trace">Optional execution trace for debugging and analysis.</param>
public sealed record Program(
    string SourceCode,
    AbstractSyntaxTree AST,
    DomainSpecificLanguage Language,
    double LogProbability,
    ExecutionTrace? Trace = null);