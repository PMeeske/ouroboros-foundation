// <copyright file="ChunkingModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.Processing;

/// <summary>
/// Represents metadata about a chunk in the processing pipeline.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ChunkMetadata(
    int Index,
    int TotalChunks,
    int TokenCount,
    ChunkingStrategy Strategy);
