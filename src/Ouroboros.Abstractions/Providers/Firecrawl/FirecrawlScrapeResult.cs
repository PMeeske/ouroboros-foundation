// <copyright file="FirecrawlModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.Firecrawl;

/// <summary>
/// Result of scraping a single URL.
/// </summary>
public sealed record FirecrawlScrapeResult
{
    /// <summary>Gets the scraped URL.</summary>
    public required string Url { get; init; }

    /// <summary>Gets the page title.</summary>
    public string? Title { get; init; }

    /// <summary>Gets the markdown content.</summary>
    public string? Markdown { get; init; }

    /// <summary>Gets the raw HTML content (if requested).</summary>
    public string? Html { get; init; }

    /// <summary>Gets the extracted metadata.</summary>
    public FirecrawlMetadata? Metadata { get; init; }
}