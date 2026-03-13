// <copyright file="IPipelineContext.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core;

/// <summary>
/// Represents a context that can flow through a pipeline with tracing capabilities.
/// </summary>
[ExcludeFromCodeCoverage]
public interface IPipelineContext
{
    /// <summary>
    /// Gets or sets a value indicating whether tracing is enabled.
    /// </summary>
    bool Trace { get; set; }

    /// <summary>
    /// Gets the current output from the pipeline.
    /// </summary>
    string Output { get; }
}
