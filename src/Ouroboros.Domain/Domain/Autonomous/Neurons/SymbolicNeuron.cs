namespace Ouroboros.Domain.Autonomous.Neurons;

/// <summary>
/// The symbolic neuron handles MeTTa-based reasoning.
/// </summary>
public sealed class SymbolicNeuron : Neuron
{
    private readonly List<string> _facts = [];
    private readonly List<string> _rules = [];

    /// <inheritdoc/>
    public override string Id => "neuron.symbolic";

    /// <inheritdoc/>
    public override string Name => "MeTTa Symbolic Reasoning";

    /// <inheritdoc/>
    public override NeuronType Type => NeuronType.Symbolic;

    /// <inheritdoc/>
    public override IReadOnlySet<string> SubscribedTopics => new HashSet<string>
    {
        "metta.*",
        "reasoning.*",
        "logic.*",
        "reflection.request",
        "dag.*",
    };

    /// <summary>
    /// Reference to the MeTTa engine.
    /// </summary>
    public object? MeTTaEngine { get; set; } // IMeTTaEngine when available

    /// <summary>
    /// Delegate for executing MeTTa queries.
    /// </summary>
    public Func<string, CancellationToken, Task<string>>? MeTTaQueryFunction { get; set; }

    /// <summary>
    /// Delegate for adding MeTTa facts.
    /// </summary>
    public Func<string, CancellationToken, Task<bool>>? MeTTaAddFactFunction { get; set; }

    /// <inheritdoc/>
    protected override async Task ProcessMessageAsync(NeuronMessage message, CancellationToken ct)
    {
        switch (message.Topic)
        {
            case "metta.fact":
                var fact = message.Payload?.ToString();
                if (!string.IsNullOrEmpty(fact))
                {
                    _facts.Add(fact);
                    // Also add to real MeTTa engine if available
                    if (MeTTaAddFactFunction != null)
                    {
                        await MeTTaAddFactFunction(fact, ct);
                    }

                    SendMessage("metta.fact_added", new { Fact = fact, TotalFacts = _facts.Count });
                }

                break;

            case "metta.rule":
                var rule = message.Payload?.ToString();
                if (!string.IsNullOrEmpty(rule))
                {
                    _rules.Add(rule);
                    // Also add to real MeTTa engine if available
                    if (MeTTaAddFactFunction != null)
                    {
                        await MeTTaAddFactFunction(rule, ct);
                    }
                }

                break;

            case "metta.query":
                // Execute symbolic query
                var query = message.Payload?.ToString() ?? "";
                var result = await ExecuteSymbolicQueryAsync(query, ct);
                SendResponse(message, new { Query = query, Result = result });
                break;

            case "reasoning.request":
                // Request symbolic reasoning support
                var context = message.Payload?.ToString() ?? "";
                var reasoning = await PerformSymbolicReasoningAsync(context, ct);
                SendResponse(message, reasoning);
                break;

            case "dag.verify":
                // Verify DAG constraints
                var verifyPayload = message.Payload as dynamic;
                var branchName = verifyPayload?.BranchName?.ToString() ?? "main";
                var constraint = verifyPayload?.Constraint?.ToString() ?? "acyclic";
                var verifyResult = await VerifyDagConstraintAsync(branchName, constraint, ct);
                SendResponse(message, new { BranchName = branchName, Constraint = constraint, IsValid = verifyResult });
                break;

            case "dag.facts":
                // Receive DAG facts from reification
                var dagFacts = message.Payload as IEnumerable<string>;
                if (dagFacts != null)
                {
                    foreach (var dagFact in dagFacts)
                    {
                        if (!string.IsNullOrEmpty(dagFact))
                        {
                            _facts.Add(dagFact);
                            if (MeTTaAddFactFunction != null)
                            {
                                await MeTTaAddFactFunction(dagFact, ct);
                            }
                        }
                    }
                }

                break;

            case "reflection.request":
                SendResponse(message, new { Facts = _facts.Count, Rules = _rules.Count });
                break;
        }
    }

    private async Task<string> ExecuteSymbolicQueryAsync(string query, CancellationToken ct)
    {
        // Use real MeTTa engine if available
        if (MeTTaQueryFunction != null)
        {
            try
            {
                return await MeTTaQueryFunction(query, ct);
            }
            catch (Exception ex)
            {
                return $"MeTTa query error: {ex.Message}";
            }
        }

        // Fallback: simplified symbolic query
        var matchingFacts = _facts
            .Where(f => f.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matchingFacts.Count > 0
            ? string.Join("; ", matchingFacts)
            : "No matching facts found";
    }

    private async Task<object> PerformSymbolicReasoningAsync(string context, CancellationToken ct)
    {
        string? mettaResult = null;

        // Try real MeTTa reasoning if available
        if (MeTTaQueryFunction != null)
        {
            try
            {
                // Query for relevant facts about the context
                var relevantQuery = $"!(match &self ($rel \"{context}\" $obj) ($rel $obj))";
                mettaResult = await MeTTaQueryFunction(relevantQuery, ct);
            }
            catch
            {
                // Ignore errors
            }
        }

        return new
        {
            Context = context,
            RelevantFacts = _facts.TakeLast(5),
            RelevantRules = _rules.TakeLast(3),
            MeTTaResult = mettaResult,
            Inference = "Reasoning based on available facts and rules"
        };
    }

    private async Task<bool> VerifyDagConstraintAsync(string branchName, string constraint, CancellationToken ct)
    {
        if (MeTTaQueryFunction == null)
        {
            return true; // No validation possible
        }

        try
        {
            // Build constraint query based on type
            var query = constraint.ToLowerInvariant() switch
            {
                "acyclic" => $"!(and (BelongsToBranch $e1 (Branch \"{branchName}\")) (Acyclic $e1 $e1))",
                "valid-ordering" => $"!(and (Before $e1 $e2) (EventAtIndex $e1 $i1) (EventAtIndex $e2 $i2) (< $i1 $i2))",
                _ => $"!(CheckConstraint \"{constraint}\" (Branch \"{branchName}\"))"
            };

            var result = await MeTTaQueryFunction(query, ct);

            // Empty or true-like result means constraint is satisfied
            return string.IsNullOrWhiteSpace(result) ||
                   result.Trim() == "[]" ||
                   result.Trim() == "()" ||
                   result.Contains("True", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return true; // On error, allow
        }
    }
}