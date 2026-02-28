// <copyright file="FileSystemDistinctionWeightStorage.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Domain.DistinctionLearning;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ouroboros.Core.DistinctionLearning;
using Ouroboros.Core.Monads;

/// <summary>
/// File system-based storage for distinction learning weights.
/// </summary>
public sealed class FileSystemDistinctionWeightStorage : IDistinctionWeightStorage
{
    private static readonly JsonSerializerOptions SharedJsonOptions = new() { WriteIndented = true };

    private readonly DistinctionStorageConfig _config;
    private readonly ILogger<FileSystemDistinctionWeightStorage>? _logger;
    private readonly string _metadataPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemDistinctionWeightStorage"/> class.
    /// </summary>
    public FileSystemDistinctionWeightStorage(
        DistinctionStorageConfig config,
        ILogger<FileSystemDistinctionWeightStorage>? logger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;
        _metadataPath = Path.Combine(_config.StoragePath, "metadata.json");

        // Ensure storage directory exists
        Directory.CreateDirectory(_config.StoragePath);
    }

    /// <inheritdoc/>
    public async Task<Result<string, string>> StoreWeightsAsync(
        string id,
        byte[] weights,
        DistinctionWeightMetadata metadata,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            string filePath = Path.Combine(_config.StoragePath, $"{id}.weights");
            await File.WriteAllBytesAsync(filePath, weights, ct);

            // Update metadata with actual file path
            DistinctionWeightMetadata updatedMetadata = metadata with { Path = filePath };
            await UpdateMetadataAsync(updatedMetadata, ct);

            _logger?.LogDebug("Stored weights {Id} at {Path}", id, filePath);
            return Result<string, string>.Success(filePath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            _logger?.LogError(ex, "I/O error storing weights {Id}", id);
            return Result<string, string>.Failure($"Storage I/O failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to store weights {Id}", id);
            return Result<string, string>.Failure($"Storage failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<byte[], string>> LoadWeightsAsync(
        string id,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            string filePath = Path.Combine(_config.StoragePath, $"{id}.weights");
            if (!File.Exists(filePath))
            {
                return Result<byte[], string>.Failure($"Weights file not found: {id}");
            }

            byte[] weights = await File.ReadAllBytesAsync(filePath, ct);
            return Result<byte[], string>.Success(weights);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            _logger?.LogError(ex, "I/O error loading weights {Id}", id);
            return Result<byte[], string>.Failure($"Load I/O failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load weights {Id}", id);
            return Result<byte[], string>.Failure($"Load failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<List<DistinctionWeightMetadata>, string>> ListWeightsAsync(
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            if (!File.Exists(_metadataPath))
            {
                return Result<List<DistinctionWeightMetadata>, string>.Success(new List<DistinctionWeightMetadata>());
            }

            string json = await File.ReadAllTextAsync(_metadataPath, ct);
            List<DistinctionWeightMetadata> metadata = JsonSerializer.Deserialize<List<DistinctionWeightMetadata>>(json)
                ?? new List<DistinctionWeightMetadata>();

            return Result<List<DistinctionWeightMetadata>, string>.Success(metadata);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            _logger?.LogError(ex, "I/O error listing weights");
            return Result<List<DistinctionWeightMetadata>, string>.Failure($"List I/O failed: {ex.Message}");
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "JSON parse error listing weights");
            return Result<List<DistinctionWeightMetadata>, string>.Failure($"List deserialization failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to list weights");
            return Result<List<DistinctionWeightMetadata>, string>.Failure($"List failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit, string>> DissolveWeightsAsync(
        string path,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            // Move file to .dissolved extension
            string dissolvedPath = path + ".dissolved";
            if (File.Exists(path))
            {
                File.Move(path, dissolvedPath, overwrite: true);
            }

            // Update metadata
            Result<List<DistinctionWeightMetadata>, string> listResult = await ListWeightsAsync(ct);
            if (listResult.IsSuccess)
            {
                List<DistinctionWeightMetadata> allMetadata = listResult.Value;
                List<DistinctionWeightMetadata> updated = allMetadata.Select(m =>
                    m.Path == path ? m with { IsDissolved = true, Path = dissolvedPath } : m).ToList();

                await SaveMetadataAsync(updated, ct);
            }

            _logger?.LogDebug("Dissolved weights at {Path}", path);
            return Result<Unit, string>.Success(Unit.Value);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            _logger?.LogError(ex, "I/O error dissolving weights at {Path}", path);
            return Result<Unit, string>.Failure($"Dissolution I/O failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to dissolve weights at {Path}", path);
            return Result<Unit, string>.Failure($"Dissolution failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<long, string>> GetTotalStorageSizeAsync(
        CancellationToken ct = default)
    {
        try
        {
            string[] files = Directory.GetFiles(_config.StoragePath, "*.weights");
            long totalSize = files.Sum(f => new FileInfo(f).Length);

            return Result<long, string>.Success(totalSize);
        }
        catch (IOException ex)
        {
            _logger?.LogError(ex, "I/O error getting total storage size");
            return Result<long, string>.Failure($"Failed to calculate size (I/O): {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get total storage size");
            return Result<long, string>.Failure($"Failed to calculate size: {ex.Message}");
        }
    }

    private async Task UpdateMetadataAsync(DistinctionWeightMetadata metadata, CancellationToken ct)
    {
        Result<List<DistinctionWeightMetadata>, string> listResult = await ListWeightsAsync(ct);
        List<DistinctionWeightMetadata> allMetadata = listResult.IsSuccess ? listResult.Value : new List<DistinctionWeightMetadata>();

        // Add or update metadata
        int existing = allMetadata.FindIndex(m => m.Id == metadata.Id);
        if (existing >= 0)
        {
            allMetadata[existing] = metadata;
        }
        else
        {
            allMetadata.Add(metadata);
        }

        await SaveMetadataAsync(allMetadata, ct);
    }

    private async Task SaveMetadataAsync(List<DistinctionWeightMetadata> metadata, CancellationToken ct)
    {
        string json = JsonSerializer.Serialize(metadata, SharedJsonOptions);
        await File.WriteAllTextAsync(_metadataPath, json, ct);
    }
}
