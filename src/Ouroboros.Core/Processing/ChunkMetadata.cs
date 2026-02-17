// <copyright file="ChunkingModels.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Processing;

/// <summary>
/// Represents metadata about a chunk in the processing pipeline.
/// </summary>
public sealed record ChunkMetadata(
    int Index,
    int TotalChunks,
    int TokenCount,
    ChunkingStrategy Strategy);