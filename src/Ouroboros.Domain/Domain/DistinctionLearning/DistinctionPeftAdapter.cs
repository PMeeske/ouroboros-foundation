// <copyright file="DistinctionPeftAdapter.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Domain.DistinctionLearning;

using Microsoft.Extensions.Logging;
using Ouroboros.Core.DistinctionLearning;
using Ouroboros.Core.Learning;
using Ouroboros.Core.Monads;

/// <summary>
/// Adapts distinction learning to PEFT (Parameter-Efficient Fine-Tuning) training.
/// </summary>
public sealed class DistinctionPeftAdapter
{
    private readonly IPeftIntegration _peft;
    private readonly IDistinctionWeightStorage _storage;
    private readonly ILogger<DistinctionPeftAdapter>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistinctionPeftAdapter"/> class.
    /// </summary>
    public DistinctionPeftAdapter(
        IPeftIntegration peft,
        IDistinctionWeightStorage storage,
        ILogger<DistinctionPeftAdapter>? logger = null)
    {
        _peft = peft ?? throw new ArgumentNullException(nameof(peft));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _logger = logger;
    }

    /// <summary>
    /// Converts a distinction to a PEFT training example.
    /// </summary>
    public static TrainingExample ToTrainingExample(ActiveDistinction distinction)
    {
        return new TrainingExample(
            Input: $"Distinction: {distinction.Content}",
            Output: $"Learned at {distinction.LearnedAtStage} with fitness {distinction.Fitness:F2}",
            Weight: distinction.Fitness);
    }

    /// <summary>
    /// Trains an adapter from a set of distinctions.
    /// </summary>
    public async Task<Result<Unit, string>> TrainFromDistinctionsAsync(
        List<ActiveDistinction> distinctions,
        string modelName,
        CancellationToken ct = default)
    {
        try
        {
            if (distinctions.Count == 0)
            {
                return Result<Unit, string>.Success(Unit.Value);
            }

            // Convert distinctions to training examples
            List<TrainingExample> examples = distinctions.Select(ToTrainingExample).ToList();

            // Create adapter config
            AdapterConfig config = new AdapterConfig(
                Rank: 8,
                LearningRate: 0.0001,
                TargetModules: "q_proj,v_proj");

            // Initialize adapter
            Result<byte[], string> initResult = await _peft.InitializeAdapterAsync(modelName, config, ct).ConfigureAwait(false);
            if (initResult.IsFailure)
            {
                return Result<Unit, string>.Failure($"Failed to initialize adapter: {initResult.Error}");
            }

            // Train adapter
            TrainingConfig trainingConfig = new TrainingConfig(
                Epochs: 3,
                BatchSize: 4);

            Result<byte[], string> trainResult = await _peft.TrainAdapterAsync(
                modelName,
                initResult.Value,
                examples,
                trainingConfig,
                ct).ConfigureAwait(false);

            if (trainResult.IsFailure)
            {
                return Result<Unit, string>.Failure($"Failed to train adapter: {trainResult.Error}");
            }

            // Store trained weights
            string distinctionId = Guid.NewGuid().ToString();
            DistinctionWeightMetadata metadata = new DistinctionWeightMetadata(
                Id: distinctionId,
                Path: string.Empty, // Will be filled by storage
                Fitness: distinctions.Average(d => d.Fitness),
                LearnedAtStage: distinctions[^1].LearnedAtStage,
                CreatedAt: DateTime.UtcNow,
                IsDissolved: false,
                SizeBytes: trainResult.Value.Length);

            Result<string, string> storeResult = await _storage.StoreWeightsAsync(
                distinctionId,
                trainResult.Value,
                metadata,
                ct).ConfigureAwait(false);

            if (storeResult.IsFailure)
            {
                _logger?.LogWarning("Failed to store trained weights: {Error}", storeResult.Error);
            }

            _logger?.LogInformation(
                "Trained adapter from {Count} distinctions",
                distinctions.Count);

            return Result<Unit, string>.Success(Unit.Value);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger?.LogError(ex, "Failed to train from distinctions");
            return Result<Unit, string>.Failure($"Training failed: {ex.Message}");
        }
    }
}
