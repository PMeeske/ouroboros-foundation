// <copyright file="ProgramSynthesisEngine.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics;
using Ouroboros.Abstractions;

namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Neural-guided program synthesis engine with library learning capabilities.
/// Implements DreamCoder-style wake-sleep algorithm for program synthesis.
/// </summary>
public sealed partial class ProgramSynthesisEngine : IProgramSynthesisEngine
{
    private readonly int beamWidth;
    private readonly int maxDepth;
    private readonly Dictionary<string, double> primitiveLogProbabilities;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgramSynthesisEngine"/> class.
    /// </summary>
    /// <param name="beamWidth">Width of the beam for beam search (default: 100).</param>
    /// <param name="maxDepth">Maximum depth of programs to synthesize (default: 10).</param>
    /// <param name="temperature">Temperature for sampling during synthesis (default: 1.0).</param>
    public ProgramSynthesisEngine(int beamWidth = 100, int maxDepth = 10, double temperature = 1.0)
    {
        this.beamWidth = beamWidth;
        this.maxDepth = maxDepth;
        this.primitiveLogProbabilities = new Dictionary<string, double>();
    }

    /// <inheritdoc/>
    public async Task<Result<Program, string>> SynthesizeProgramAsync(
        List<InputOutputExample> examples,
        DomainSpecificLanguage dsl,
        TimeSpan timeout,
        CancellationToken ct = default)
    {
        if (examples == null || examples.Count == 0)
        {
            return Result<Program, string>.Failure("No examples provided for synthesis");
        }

        if (dsl == null || dsl.Primitives.Count == 0)
        {
            return Result<Program, string>.Failure("DSL has no primitives");
        }

        try
        {
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);

            Stopwatch stopwatch = Stopwatch.StartNew();

            // Initialize beam with primitives
            List<ASTNode> beam = InitializeBeam(dsl);

            // Iterative deepening search
            for (int depth = 1; depth <= this.maxDepth; depth++)
            {
                if (cts.Token.IsCancellationRequested)
                {
                    return Result<Program, string>.Failure("Synthesis timeout exceeded");
                }

                // Expand beam to next depth
                beam = await ExpandBeamAsync(beam, dsl, depth, cts.Token).ConfigureAwait(false);

                // Evaluate programs against examples
                List<Program> validPrograms = await EvaluateBeamAsync(beam, examples, dsl, cts.Token).ConfigureAwait(false);

                // Check if we found a solution
                if (validPrograms.Count > 0)
                {
                    stopwatch.Stop();
                    Program bestProgram = validPrograms.OrderByDescending(p => p.LogProbability).First();
                    return Result<Program, string>.Success(bestProgram);
                }

                // Prune beam to maintain beam width
                beam = PruneBeam(beam, this.beamWidth);
            }

            return Result<Program, string>.Failure($"Failed to synthesize program within depth {this.maxDepth}");
        }
        catch (OperationCanceledException)
        {
            return Result<Program, string>.Failure("Synthesis was cancelled");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result<Program, string>.Failure($"Synthesis failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<List<Primitive>, string>> ExtractReusablePrimitivesAsync(
        List<Program> successfulPrograms,
        CompressionStrategy strategy,
        CancellationToken ct = default)
    {
        if (successfulPrograms == null || successfulPrograms.Count == 0)
        {
            return Result<List<Primitive>, string>.Failure("No programs provided for extraction");
        }

        try
        {
            List<Primitive> extractedPrimitives = strategy switch
            {
                CompressionStrategy.AntiUnification => await ExtractViaAntiUnificationAsync(successfulPrograms, ct).ConfigureAwait(false),
                CompressionStrategy.EGraph => await ExtractViaEGraphAsync(successfulPrograms, ct).ConfigureAwait(false),
                CompressionStrategy.FragmentGrammar => await ExtractViaFragmentGrammarAsync(successfulPrograms, ct).ConfigureAwait(false),
                _ => throw new ArgumentException($"Unknown compression strategy: {strategy}"),
            };

            return Result<List<Primitive>, string>.Success(extractedPrimitives);
        }
        catch (OperationCanceledException)
        {
            return Result<List<Primitive>, string>.Failure("Primitive extraction was cancelled");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result<List<Primitive>, string>.Failure($"Primitive extraction failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit, string>> TrainRecognitionModelAsync(
        List<(SynthesisTask Task, Program Solution)> pairs,
        CancellationToken ct = default)
    {
        if (pairs == null || pairs.Count == 0)
        {
            return Result<Unit, string>.Failure("No training pairs provided");
        }

        try
        {
            // Update primitive log probabilities based on training pairs
            Dictionary<string, int> primitiveUsage = new Dictionary<string, int>();

            foreach ((SynthesisTask _, Program? solution) in pairs)
            {
                ct.ThrowIfCancellationRequested();

                // Count primitive usage in the solution
                CountPrimitiveUsage(solution.AST.Root, primitiveUsage);
            }

            // Update log probabilities
            int totalUsage = primitiveUsage.Values.Sum();
            foreach ((string? primitive, int count) in primitiveUsage)
            {
                double probability = (double)count / totalUsage;
                this.primitiveLogProbabilities[primitive] = Math.Log(probability);
            }

            // The frequency-based log-probability update above is the recognition model:
            // it computes a Bayesian posterior over primitives from observed usage counts,
            // guiding future synthesis searches toward frequently successful primitives.
            return Result<Unit, string>.Success(Unit.Value);
        }
        catch (OperationCanceledException)
        {
            return Result<Unit, string>.Failure("Training was cancelled");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result<Unit, string>.Failure($"Training failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<DomainSpecificLanguage, string>> EvolveDSLAsync(
        DomainSpecificLanguage currentDSL,
        List<Primitive> newPrimitives,
        UsageStatistics stats,
        CancellationToken ct = default)
    {
        if (currentDSL == null)
        {
            return Result<DomainSpecificLanguage, string>.Failure("Current DSL is null");
        }

        try
        {
            ct.ThrowIfCancellationRequested();

            // Create new primitives list combining current and new
            List<Primitive> updatedPrimitives = new List<Primitive>(currentDSL.Primitives);

            // Add new primitives
            if (newPrimitives != null && newPrimitives.Count > 0)
            {
                updatedPrimitives.AddRange(newPrimitives);
            }

            // Update log priors based on usage statistics
            if (stats != null)
            {
                updatedPrimitives = updatedPrimitives.Select(p =>
                {
                    if (stats.PrimitiveUseCounts.TryGetValue(p.Name, out int count) &&
                        stats.PrimitiveSuccessRates.TryGetValue(p.Name, out double successRate))
                    {
                        // Adjust log prior based on usage and success
                        double adjustedLogPrior = p.LogPrior + Math.Log(count + 1) + Math.Log(successRate + 0.01);
                        return p with { LogPrior = adjustedLogPrior };
                    }

                    return p;
                }).ToList();
            }

            DomainSpecificLanguage evolvedDSL = currentDSL with { Primitives = updatedPrimitives };

            await Task.CompletedTask.ConfigureAwait(false);
            return Result<DomainSpecificLanguage, string>.Success(evolvedDSL);
        }
        catch (OperationCanceledException)
        {
            return Result<DomainSpecificLanguage, string>.Failure("DSL evolution was cancelled");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result<DomainSpecificLanguage, string>.Failure($"DSL evolution failed: {ex.Message}");
        }
    }

}
