// <copyright file="ToolExecution.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain;

/// <summary>
/// Represents the execution result of a tool invocation in the pipeline.
/// Immutable record that captures tool name, arguments, output, and timestamp.
/// </summary>
/// <param name="ToolName">The name of the tool that was executed</param>
/// <param name="Arguments">The arguments passed to the tool</param>
/// <param name="Output">The output/result produced by the tool</param>
/// <param name="Timestamp">When the tool was executed</param>
[ExcludeFromCodeCoverage]
public sealed record ToolExecution(
    string ToolName,
    string Arguments,
    string Output,
    DateTime Timestamp);
