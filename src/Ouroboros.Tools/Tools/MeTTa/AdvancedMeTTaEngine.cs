// <copyright file="AdvancedMeTTaEngine.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Tools.MeTTa;

using System.Text.RegularExpressions;

/// <summary>
/// Advanced MeTTa engine implementation with rule learning, theorem proving, and abductive reasoning.
/// </summary>
public sealed partial class AdvancedMeTTaEngine : IAdvancedMeTTaEngine
{
    private readonly IMeTTaEngine baseEngine;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvancedMeTTaEngine"/> class.
    /// </summary>
    /// <param name="baseEngine">Base MeTTa engine for basic operations.</param>
    public AdvancedMeTTaEngine(IMeTTaEngine baseEngine)
    {
        this.baseEngine = baseEngine ?? throw new ArgumentNullException(nameof(baseEngine));
    }

    /// <inheritdoc />
    public async Task<Result<List<Rule>, string>> InduceRulesAsync(
        List<Fact> observations,
        InductionStrategy strategy,
        CancellationToken ct = default)
    {
        if (disposed)
        {
            return Result<List<Rule>, string>.Failure("Engine disposed");
        }

        if (observations == null || observations.Count == 0)
        {
            return Result<List<Rule>, string>.Failure("No observations provided");
        }

        try
        {
            return strategy switch
            {
                InductionStrategy.FOIL => await InduceFoilRulesAsync(observations, ct),
                InductionStrategy.GOLEM => await InduceGolemRulesAsync(observations, ct),
                InductionStrategy.Progol => await InduceProgolRulesAsync(observations, ct),
                InductionStrategy.ILP => await InduceIlpRulesAsync(observations, ct),
                _ => Result<List<Rule>, string>.Failure($"Unknown induction strategy: {strategy}"),
            };
        }
        catch (OperationCanceledException) { throw; }
        catch (InvalidOperationException ex)
        {
            return Result<List<Rule>, string>.Failure($"Rule induction failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<ProofTrace, string>> ProveTheoremAsync(
        string theorem,
        List<string> axioms,
        ProofStrategy strategy,
        CancellationToken ct = default)
    {
        if (disposed)
        {
            return Result<ProofTrace, string>.Failure("Engine disposed");
        }

        if (string.IsNullOrWhiteSpace(theorem))
        {
            return Result<ProofTrace, string>.Failure("Theorem cannot be empty");
        }

        try
        {
            return strategy switch
            {
                ProofStrategy.Resolution => await ProveByResolutionAsync(theorem, axioms, ct),
                ProofStrategy.Tableaux => await ProveByTableauxAsync(theorem, axioms, ct),
                ProofStrategy.NaturalDeduction => await ProveByNaturalDeductionAsync(theorem, axioms, ct),
                _ => Result<ProofTrace, string>.Failure($"Unknown proof strategy: {strategy}"),
            };
        }
        catch (OperationCanceledException) { throw; }
        catch (InvalidOperationException ex)
        {
            return Result<ProofTrace, string>.Failure($"Theorem proving failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<Hypothesis>, string>> GenerateHypothesesAsync(
        string observation,
        List<string> backgroundKnowledge,
        CancellationToken ct = default)
    {
        if (disposed)
        {
            return Result<List<Hypothesis>, string>.Failure("Engine disposed");
        }

        if (string.IsNullOrWhiteSpace(observation))
        {
            return Result<List<Hypothesis>, string>.Failure("Observation cannot be empty");
        }

        try
        {
            var hypotheses = new List<Hypothesis>();

            // Parse observation
            var obsPattern = ParseFactPattern(observation);
            if (obsPattern == null)
            {
                return Result<List<Hypothesis>, string>.Failure("Invalid observation format");
            }

            // Generate abductive hypotheses
            foreach (var knowledge in backgroundKnowledge ?? new List<string>())
            {
                var hypothesis = GenerateHypothesis(obsPattern, knowledge);
                if (hypothesis != null)
                {
                    hypotheses.Add(hypothesis);
                }
            }

            // If no hypotheses from background knowledge, generate generic ones
            if (hypotheses.Count == 0)
            {
                hypotheses.Add(new Hypothesis(
                    $"If X then {observation}",
                    0.5,
                    new List<Fact> { obsPattern }));
            }

            return Result<List<Hypothesis>, string>.Success(hypotheses);
        }
        catch (OperationCanceledException) { throw; }
        catch (InvalidOperationException ex)
        {
            return Result<List<Hypothesis>, string>.Failure($"Hypothesis generation failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public Task<Result<TypedAtom, string>> InferTypeAsync(
        string atom,
        TypeContext context,
        CancellationToken ct = default)
    {
        if (disposed)
        {
            return Task.FromResult(Result<TypedAtom, string>.Failure("Engine disposed"));
        }

        if (string.IsNullOrWhiteSpace(atom))
        {
            return Task.FromResult(Result<TypedAtom, string>.Failure("Atom cannot be empty"));
        }

        try
        {
            // Simple type inference based on pattern matching
            var inferredType = InferTypeFromPattern(atom, context);
            var typedAtom = new TypedAtom(atom, inferredType, new Dictionary<string, string>());

            return Task.FromResult(Result<TypedAtom, string>.Success(typedAtom));
        }
        catch (InvalidOperationException ex)
        {
            return Task.FromResult(Result<TypedAtom, string>.Failure($"Type inference failed: {ex.Message}"));
        }
        catch (FormatException ex)
        {
            return Task.FromResult(Result<TypedAtom, string>.Failure($"Type inference failed: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<Fact>, string>> ForwardChainAsync(
        List<Rule> rules,
        List<Fact> facts,
        int maxSteps = 10,
        CancellationToken ct = default)
    {
        if (disposed)
        {
            return Result<List<Fact>, string>.Failure("Engine disposed");
        }

        if (rules == null || rules.Count == 0)
        {
            return Result<List<Fact>, string>.Success(facts ?? new List<Fact>());
        }

        try
        {
            var derivedFacts = new HashSet<Fact>(facts ?? new List<Fact>());
            var step = 0;

            while (step < maxSteps)
            {
                var newFacts = new List<Fact>();
                var factsAdded = false;

                foreach (var rule in rules)
                {
                    var matches = FindMatchingFacts(rule.Premises, derivedFacts.ToList());
                    foreach (var match in matches)
                    {
                        var newFact = ApplyRule(rule, match);
                        if (newFact != null && !derivedFacts.Contains(newFact))
                        {
                            newFacts.Add(newFact);
                            factsAdded = true;
                        }
                    }
                }

                if (!factsAdded)
                {
                    break;
                }

                foreach (var fact in newFacts)
                {
                    derivedFacts.Add(fact);
                }

                step++;
            }

            return Result<List<Fact>, string>.Success(derivedFacts.ToList());
        }
        catch (OperationCanceledException) { throw; }
        catch (InvalidOperationException ex)
        {
            return Result<List<Fact>, string>.Failure($"Forward chaining failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<Fact>, string>> BackwardChainAsync(
        Fact goal,
        List<Rule> rules,
        List<Fact> knownFacts,
        CancellationToken ct = default)
    {
        if (disposed)
        {
            return Result<List<Fact>, string>.Failure("Engine disposed");
        }

        if (goal == null)
        {
            return Result<List<Fact>, string>.Failure("Goal cannot be null");
        }

        try
        {
            var requiredFacts = new List<Fact>();
            var visited = new HashSet<Fact>();

            if (BackwardChainRecursive(goal, rules, knownFacts ?? new List<Fact>(), requiredFacts, visited))
            {
                return Result<List<Fact>, string>.Success(requiredFacts);
            }

            return Result<List<Fact>, string>.Failure("Goal cannot be proved with given rules and facts");
        }
        catch (OperationCanceledException) { throw; }
        catch (InvalidOperationException ex)
        {
            return Result<List<Fact>, string>.Failure($"Backward chaining failed: {ex.Message}");
        }
    }

    // IMeTTaEngine implementations (delegated to base engine)
    /// <inheritdoc />
    public Task<Result<string, string>> ExecuteQueryAsync(string query, CancellationToken ct = default)
        => baseEngine.ExecuteQueryAsync(query, ct);

    /// <inheritdoc />
    public Task<Result<Unit, string>> AddFactAsync(string fact, CancellationToken ct = default)
        => baseEngine.AddFactAsync(fact, ct);

    /// <inheritdoc />
    public Task<Result<string, string>> ApplyRuleAsync(string rule, CancellationToken ct = default)
        => baseEngine.ApplyRuleAsync(rule, ct);

    /// <inheritdoc />
    public Task<Result<bool, string>> VerifyPlanAsync(string plan, CancellationToken ct = default)
        => baseEngine.VerifyPlanAsync(plan, ct);

    /// <inheritdoc />
    public Task<Result<Unit, string>> ResetAsync(CancellationToken ct = default)
        => baseEngine.ResetAsync(ct);

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        baseEngine?.Dispose();
        disposed = true;
    }

    // FOIL Algorithm Implementation
    private static async Task<Result<List<Rule>, string>> InduceFoilRulesAsync(List<Fact> observations, CancellationToken ct)
    {
        var rules = new List<Rule>();

        // Group observations by predicate
        var grouped = observations.GroupBy(f => f.Predicate);

        foreach (var group in grouped)
        {
            var predicate = group.Key;
            var facts = group.ToList();

            if (facts.Count < 2)
            {
                continue;
            }

            // Extract common patterns
            var pattern = ExtractPattern(facts);
            if (pattern != null)
            {
                var rule = new Rule(
                    $"induced_{predicate}",
                    new List<Pattern> { pattern },
                    pattern,
                    CalculateConfidence(facts));

                rules.Add(rule);
            }
        }

        return Result<List<Rule>, string>.Success(rules);
    }

    private static Task<Result<List<Rule>, string>> InduceGolemRulesAsync(List<Fact> observations, CancellationToken ct)
    {
        // Simplified GOLEM - uses least general generalization
        return InduceFoilRulesAsync(observations, ct);
    }

    private static Task<Result<List<Rule>, string>> InduceProgolRulesAsync(List<Fact> observations, CancellationToken ct)
    {
        // Simplified Progol - similar to FOIL for this implementation
        return InduceFoilRulesAsync(observations, ct);
    }

    private static Task<Result<List<Rule>, string>> InduceIlpRulesAsync(List<Fact> observations, CancellationToken ct)
    {
        // Generic ILP approach
        return InduceFoilRulesAsync(observations, ct);
    }

    // Resolution-based theorem proving
    private static async Task<Result<ProofTrace, string>> ProveByResolutionAsync(
        string theorem,
        List<string> axioms,
        CancellationToken ct)
    {
        var steps = new List<ProofStep>();

        // Convert theorem to negated clausal form
        var negatedTheorem = $"NOT ({theorem})";

        // Try to derive contradiction
        var clauseSet = new List<string> { negatedTheorem };
        clauseSet.AddRange(axioms ?? new List<string>());

        var proved = TryResolve(clauseSet, steps);

        return Result<ProofTrace, string>.Success(new ProofTrace(steps, proved));
    }

    private static Task<Result<ProofTrace, string>> ProveByTableauxAsync(
        string theorem,
        List<string> axioms,
        CancellationToken ct)
    {
        // Simplified tableaux - delegate to resolution
        return ProveByResolutionAsync(theorem, axioms, ct);
    }

    private static Task<Result<ProofTrace, string>> ProveByNaturalDeductionAsync(
        string theorem,
        List<string> axioms,
        CancellationToken ct)
    {
        // Simplified natural deduction - delegate to resolution
        return ProveByResolutionAsync(theorem, axioms, ct);
    }

}
