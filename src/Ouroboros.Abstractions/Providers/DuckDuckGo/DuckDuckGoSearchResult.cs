// <copyright file="DuckDuckGoModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.DuckDuckGo;

/// <summary>
/// A DuckDuckGo web search result.
/// </summary>
public sealed record DuckDuckGoSearchResult
{
    /// <summary>Gets the result title.</summary>
    public required string Title { get; init; }

    /// <summary>Gets the result URL.</summary>
    public required string Url { get; init; }

    /// <summary>Gets the text snippet (body/description).</summary>
    public string? Snippet { get; init; }
}