// <copyright file="QdrantCollectionAdmin.Health.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Text;
using Qdrant.Client.Grpc;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Health check, auto-heal, statistics, and memory map methods for QdrantCollectionAdmin.
/// </summary>
public sealed partial class QdrantCollectionAdmin
{
    /// <summary>
    /// Performs a health check on all collections, detecting dimension mismatches.
    /// </summary>
    public async Task<IReadOnlyList<CollectionHealthReport>> HealthCheckAsync(
        int expectedDimension = DefaultVectorSize,
        CancellationToken ct = default)
    {
        List<CollectionHealthReport> reports = new List<CollectionHealthReport>();
        await RefreshCollectionCacheAsync(ct).ConfigureAwait(false);

        foreach ((string? name, CollectionInfo? info) in _collectionCache)
        {
            bool mismatch = info.VectorSize != (ulong)expectedDimension && info.VectorSize > 0;
            string? issue = mismatch
                ? $"Dimension mismatch: expected {expectedDimension}, got {info.VectorSize}"
                : null;
            string? recommendation = mismatch
                ? $"Delete and recreate collection, or migrate vectors to {expectedDimension} dimensions"
                : null;

            reports.Add(new CollectionHealthReport(
                name,
                !mismatch && info.Status == CollectionStatus.Green,
                (ulong)expectedDimension,
                info.VectorSize,
                mismatch,
                issue,
                recommendation));
        }

        return reports.AsReadOnly();
    }

    /// <summary>
    /// Auto-heals collections with dimension mismatches by recreating them.
    /// WARNING: This will delete all data in mismatched collections!
    /// </summary>
    public async Task<IReadOnlyList<string>> AutoHealDimensionMismatchesAsync(
        int targetDimension = DefaultVectorSize,
        CancellationToken ct = default)
    {
        List<string> healed = new List<string>();
        IReadOnlyList<CollectionHealthReport> healthReports = await HealthCheckAsync(targetDimension, ct).ConfigureAwait(false);

        foreach (string collectionName in healthReports.Where(r => r.DimensionMismatch).Select(report => report.CollectionName))
        {
            try
            {
                CollectionInfo? info = _collectionCache.GetValueOrDefault(collectionName);
                Distance distance = info?.DistanceMetric ?? Distance.Cosine;

                await _client.DeleteCollectionAsync(collectionName, cancellationToken: ct).ConfigureAwait(false);

                await _client.CreateCollectionAsync(
                    collectionName,
                    new VectorParams { Size = (ulong)targetDimension, Distance = distance },
                    cancellationToken: ct).ConfigureAwait(false);

                healed.Add(collectionName);
            }
            catch (Grpc.Core.RpcException)
            {
                // Skip collections that can't be healed
            }
        }

        await RefreshCollectionCacheAsync(ct).ConfigureAwait(false);

        return healed.AsReadOnly();
    }

    /// <summary>
    /// Generates a memory map showing collection relationships.
    /// </summary>
    public async Task<string> GenerateMemoryMapAsync(CancellationToken ct = default)
    {
        await RefreshCollectionCacheAsync(ct).ConfigureAwait(false);

        StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("\u2554\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2557");
        sb.AppendLine("\u2551              OUROBOROS MEMORY ARCHITECTURE                       \u2551");
        sb.AppendLine("\u2560\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2563");

        List<string> thoughtCollections = _collectionCache.Keys.Where(k => k.Contains("thought")).ToList();
        List<string> skillCollections = _collectionCache.Keys.Where(k => k.Contains("skill") || k.Contains("tool")).ToList();
        List<string> knowledgeCollections = _collectionCache.Keys.Where(k => k.Contains("core") || k.Contains("code")).ToList();
        List<string> personalityCollections = _collectionCache.Keys.Where(k => k.Contains("person") || k.Contains("self")).ToList();
        List<string> otherCollections = _collectionCache.Keys
            .Except(thoughtCollections)
            .Except(skillCollections)
            .Except(knowledgeCollections)
            .Except(personalityCollections)
            .ToList();

        void AppendSection(string title, List<string> collections)
        {
            if (!collections.Any()) return;
            sb.AppendLine($"\u2551 {title,-64} \u2551");
            foreach (string col in collections)
            {
                CollectionInfo? info = _collectionCache.GetValueOrDefault(col);
                string status = info?.Status == CollectionStatus.Green ? "\u2713" : "\u26a0";
                ulong points = info?.PointsCount ?? 0;
                ulong dim = info?.VectorSize ?? 0;
                sb.AppendLine($"\u2551   {status} {col,-40} [{dim,4}d] {points,8} pts \u2551");
            }
        }

        AppendSection("\ud83e\udde0 THOUGHT SYSTEM", thoughtCollections);
        AppendSection("\ud83d\udee0\ufe0f SKILLS & TOOLS", skillCollections);
        AppendSection("\ud83d\udcda KNOWLEDGE BASE", knowledgeCollections);
        AppendSection("\ud83d\udc64 PERSONALITY & SELF", personalityCollections);
        AppendSection("\ud83d\udce6 OTHER", otherCollections);

        sb.AppendLine("\u2560\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2563");
        sb.AppendLine("\u2551 COLLECTION LINKS                                                 \u2551");

        foreach (CollectionLink? link in _collectionLinks.Take(10))
        {
            sb.AppendLine($"\u2551   {link.SourceCollection,-25} \u2500{link.RelationType,12}\u2192 {link.TargetCollection,-15} \u2551");
        }

        if (_collectionLinks.Count > 10)
        {
            sb.AppendLine($"\u2551   ... and {_collectionLinks.Count - 10} more links                                    \u2551");
        }

        sb.AppendLine("\u255a\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u255d");

        return sb.ToString();
    }

    /// <summary>
    /// Gets statistics about Ouroboros's memory usage.
    /// </summary>
    public async Task<MemoryStatistics> GetMemoryStatisticsAsync(CancellationToken ct = default)
    {
        await RefreshCollectionCacheAsync(ct).ConfigureAwait(false);

        int totalCollections = _collectionCache.Count;
        long totalPoints = _collectionCache.Values.Sum(c => (long)c.PointsCount);
        int healthyCollections = _collectionCache.Values.Count(c => c.Status == CollectionStatus.Green);
        Dictionary<ulong, int> dimensionGroups = _collectionCache.Values
            .GroupBy(c => c.VectorSize)
            .ToDictionary(g => g.Key, g => g.Count());

        return new MemoryStatistics(
            totalCollections,
            totalPoints,
            healthyCollections,
            totalCollections - healthyCollections,
            _collectionLinks.Count,
            dimensionGroups);
    }
}
