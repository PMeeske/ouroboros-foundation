// <copyright file="ProgramSynthesisEngine.Helpers.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Private helper methods for the ProgramSynthesisEngine.
/// </summary>
public sealed partial class ProgramSynthesisEngine
{
    private List<ASTNode> InitializeBeam(DomainSpecificLanguage dsl)
    {
        return dsl.Primitives
            .Select(p => new ASTNode("Primitive", p.Name, new List<ASTNode>()))
            .ToList();
    }

    private async Task<List<ASTNode>> ExpandBeamAsync(
        List<ASTNode> currentBeam,
        DomainSpecificLanguage dsl,
        int targetDepth,
        CancellationToken ct)
    {
        List<ASTNode> expandedBeam = new List<ASTNode>();

        foreach (ASTNode node in currentBeam)
        {
            ct.ThrowIfCancellationRequested();

            int nodeDepth = CalculateDepth(node);
            if (nodeDepth < targetDepth)
            {
                // Infer the output type of the current candidate node
                string candidateOutputType = InferOutputType(node);

                foreach (Primitive primitive in dsl.Primitives)
                {
                    // Type-directed pruning: skip composition when types are incompatible
                    if (!IsTypeCompatible(primitive.Type, candidateOutputType))
                    {
                        continue;
                    }

                    ASTNode applicationNode = new ASTNode(
                        "Apply",
                        primitive.Name,
                        new List<ASTNode> { node });
                    expandedBeam.Add(applicationNode);

                    foreach (ASTNode otherNode in currentBeam)
                    {
                        if (node != otherNode)
                        {
                            ASTNode compositionNode = new ASTNode(
                                "Apply",
                                primitive.Name,
                                new List<ASTNode> { node, otherNode });
                            expandedBeam.Add(compositionNode);
                        }
                    }
                }
            }
        }

        await Task.CompletedTask;
        return expandedBeam.Any() ? expandedBeam : currentBeam;
    }

    private async Task<List<Program>> EvaluateBeamAsync(
        List<ASTNode> beam,
        List<InputOutputExample> examples,
        DomainSpecificLanguage dsl,
        CancellationToken ct)
    {
        List<Program> validPrograms = new List<Program>();

        foreach (ASTNode node in beam)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                bool allExamplesPass = true;
                List<ExecutionStep> trace = new List<ExecutionStep>();

                foreach (InputOutputExample example in examples)
                {
                    object? result = await ExecuteProgramAsync(node, example.Input, dsl, ct);
                    if (result == null || !result.Equals(example.ExpectedOutput))
                    {
                        allExamplesPass = false;
                        break;
                    }

                    trace.Add(new ExecutionStep(node.Value, new List<object> { example.Input }, result));
                }

                if (allExamplesPass)
                {
                    Program program = CreateProgram(node, trace);
                    validPrograms.Add(program);
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (InvalidOperationException)
            {
                continue;
            }
        }

        return validPrograms;
    }

    private async Task<object?> ExecuteProgramAsync(ASTNode node, object input, DomainSpecificLanguage dsl, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (node.NodeType == "Primitive")
        {
            var primitive = dsl.Primitives.FirstOrDefault(p => p.Name == node.Value);
            if (primitive?.Implementation != null)
            {
                return primitive.Implementation(new object[] { input });
            }

            return input; // fallback if no matching implementation
        }

        if (node.NodeType == "Apply")
        {
            List<object> childResults = new List<object>();
            foreach (ASTNode child in node.Children)
            {
                object? result = await ExecuteProgramAsync(child, input, dsl, ct);
                if (result != null)
                {
                    childResults.Add(result);
                }
            }

            // Look up the primitive for the Apply node and invoke it with child results
            var applyPrimitive = dsl.Primitives.FirstOrDefault(p => p.Name == node.Value);
            if (applyPrimitive?.Implementation != null && childResults.Count > 0)
            {
                return applyPrimitive.Implementation(childResults.ToArray());
            }

            return childResults.LastOrDefault() ?? input;
        }

        return null;
    }

    private Program CreateProgram(ASTNode node, List<ExecutionStep> trace)
    {
        string sourceCode = ASTToSourceCode(node);
        int depth = CalculateDepth(node);
        int nodeCount = CountNodes(node);
        AbstractSyntaxTree ast = new AbstractSyntaxTree(node, depth, nodeCount);
        double logProb = CalculateLogProbability(node);

        return new Program(
            sourceCode,
            ast,
            new DomainSpecificLanguage("temp", new List<Primitive>(), new List<TypeRule>(), new List<RewriteRule>()),
            logProb,
            new ExecutionTrace(trace, trace.LastOrDefault()?.Output ?? new object(), TimeSpan.Zero));
    }

    private string ASTToSourceCode(ASTNode node)
    {
        if (node.NodeType == "Primitive")
        {
            return node.Value;
        }

        if (node.NodeType == "Apply" && node.Children.Count > 0)
        {
            string childrenCode = string.Join(" ", node.Children.Select(ASTToSourceCode));
            return $"({node.Value} {childrenCode})";
        }

        return node.Value;
    }

    private int CalculateDepth(ASTNode node)
    {
        if (node.Children.Count == 0)
        {
            return 1;
        }

        return 1 + node.Children.Max(CalculateDepth);
    }

    private int CountNodes(ASTNode node)
    {
        return 1 + node.Children.Sum(CountNodes);
    }

    private double CalculateLogProbability(ASTNode node)
    {
        double logProb = 0.0;

        if (this.primitiveLogProbabilities.TryGetValue(node.Value, out double prob))
        {
            logProb += prob;
        }
        else
        {
            logProb += Math.Log(0.1);
        }

        foreach (ASTNode child in node.Children)
        {
            logProb += CalculateLogProbability(child);
        }

        return logProb;
    }

    private List<ASTNode> PruneBeam(List<ASTNode> beam, int maxSize)
    {
        if (beam.Count <= maxSize)
        {
            return beam;
        }

        return beam
            .OrderByDescending(CalculateLogProbability)
            .Take(maxSize)
            .ToList();
    }

    private void CountPrimitiveUsage(ASTNode node, Dictionary<string, int> usage)
    {
        if (!usage.ContainsKey(node.Value))
        {
            usage[node.Value] = 0;
        }

        usage[node.Value]++;

        foreach (ASTNode child in node.Children)
        {
            CountPrimitiveUsage(child, usage);
        }
    }

    private async Task<List<Primitive>> ExtractViaAntiUnificationAsync(List<Program> programs, CancellationToken ct)
    {
        List<Primitive> extractedPrimitives = new List<Primitive>();

        List<(Program, Program)> programPairs = new List<(Program, Program)>();
        for (int i = 0; i < programs.Count; i++)
        {
            for (int j = i + 1; j < programs.Count; j++)
            {
                ct.ThrowIfCancellationRequested();
                programPairs.Add((programs[i], programs[j]));
            }
        }

        foreach ((Program? prog1, Program? prog2) in programPairs)
        {
            ct.ThrowIfCancellationRequested();

            ASTNode? commonPattern = AntiUnify(prog1.AST.Root, prog2.AST.Root);
            if (commonPattern != null && CountNodes(commonPattern) > 2)
            {
                string primitiveName = $"learned_{extractedPrimitives.Count}";
                string primitiveType = InferType(commonPattern);
                Primitive primitive = new Primitive(
                    primitiveName,
                    primitiveType,
                    args => args.FirstOrDefault() ?? new object(),
                    Math.Log(0.5));

                extractedPrimitives.Add(primitive);
            }
        }

        await Task.CompletedTask;
        return extractedPrimitives.DistinctBy(p => p.Name).ToList();
    }

    private ASTNode? AntiUnify(ASTNode node1, ASTNode node2)
    {
        if (node1.NodeType == node2.NodeType && node1.Value == node2.Value)
        {
            if (node1.Children.Count == node2.Children.Count)
            {
                List<ASTNode> unifiedChildren = new List<ASTNode>();
                for (int i = 0; i < node1.Children.Count; i++)
                {
                    ASTNode? unified = AntiUnify(node1.Children[i], node2.Children[i]);
                    if (unified != null)
                    {
                        unifiedChildren.Add(unified);
                    }
                    else
                    {
                        unifiedChildren.Add(new ASTNode("Variable", $"$x{i}", new List<ASTNode>()));
                    }
                }

                return new ASTNode(node1.NodeType, node1.Value, unifiedChildren);
            }
        }

        return null;
    }

    private string InferType(ASTNode node)
    {
        return node.NodeType switch
        {
            "Primitive" => "a -> a",
            "Apply" => "a -> b",
            "Variable" => "a",
            _ => "a -> a",
        };
    }

    private Task<List<Primitive>> ExtractViaEGraphAsync(List<Program> programs, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var extracted = new List<Primitive>();

        // Build hash-consed representation of all program ASTs
        var subtreeHashes = new Dictionary<string, List<ASTNode>>();

        foreach (var program in programs)
        {
            ct.ThrowIfCancellationRequested();
            CollectSubtrees(program.AST.Root, subtreeHashes);
        }

        // Find common subexpressions (appearing in 2+ programs)
        foreach (var (hash, nodes) in subtreeHashes)
        {
            ct.ThrowIfCancellationRequested();

            // Only extract non-trivial shared subtrees (has children and appears multiple times)
            if (nodes.Count >= 2 && nodes[0].Children.Count > 0)
            {
                var representative = nodes[0];
                var name = $"extracted_{hash[..Math.Min(8, hash.Length)]}";

                // Skip if we already extracted a primitive with this name
                if (extracted.Any(p => p.Name == name))
                {
                    continue;
                }

                // Create new primitive from the shared subtree
                var capturedNode = representative;
                extracted.Add(new Primitive(
                    Name: name,
                    Type: InferSubtreeType(capturedNode),
                    Implementation: args => EvaluateSubtree(capturedNode, args),
                    LogPrior: Math.Log(nodes.Count))); // higher frequency = higher prior
            }
        }

        return Task.FromResult(extracted);
    }

    private Task<List<Primitive>> ExtractViaFragmentGrammarAsync(List<Program> programs, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var extracted = new List<Primitive>();
        var patternCounts = new Dictionary<string, (ASTNode Node, int Count)>();

        // Find recurring AST subtree patterns across programs
        foreach (var program in programs)
        {
            ct.ThrowIfCancellationRequested();
            var seen = new HashSet<string>(); // avoid counting same pattern twice per program
            CountPatterns(program.AST.Root, patternCounts, seen);
        }

        // Extract patterns with frequency >= 2 that are non-trivial
        foreach (var (pattern, (node, count)) in patternCounts)
        {
            ct.ThrowIfCancellationRequested();

            if (count >= 2 && node.Children.Count > 0)
            {
                var name = $"fragment_{pattern[..Math.Min(8, pattern.Length)]}";
                if (extracted.Any(p => p.Name == name))
                {
                    continue;
                }

                var capturedNode = node;
                extracted.Add(new Primitive(
                    Name: name,
                    Type: InferSubtreeType(capturedNode),
                    Implementation: args => EvaluateSubtree(capturedNode, args),
                    LogPrior: Math.Log(count))); // frequency-based prior
            }
        }

        return Task.FromResult(extracted);
    }

    /// <summary>
    /// Recursively collects all subtrees of a node, grouped by structural hash.
    /// Used by e-graph extraction to find common subexpressions.
    /// </summary>
    private void CollectSubtrees(ASTNode node, Dictionary<string, List<ASTNode>> map)
    {
        var hash = ComputeSubtreeHash(node);
        if (!map.TryGetValue(hash, out var nodeList))
        {
            nodeList = new List<ASTNode>();
            map[hash] = nodeList;
        }

        nodeList.Add(node);

        foreach (var child in node.Children)
        {
            CollectSubtrees(child, map);
        }
    }

    /// <summary>
    /// Computes a structural hash for a subtree to identify identical AST fragments.
    /// Two subtrees with the same hash are structurally equivalent.
    /// </summary>
    private string ComputeSubtreeHash(ASTNode node)
    {
        if (node.Children.Count == 0)
        {
            return $"{node.NodeType}:{node.Value}";
        }

        var childHashes = node.Children.Select(ComputeSubtreeHash);
        return $"{node.NodeType}:{node.Value}({string.Join(",", childHashes)})";
    }

    /// <summary>
    /// Counts recurring AST subtree patterns, avoiding double-counting within a single program.
    /// Used by fragment grammar extraction.
    /// </summary>
    private void CountPatterns(
        ASTNode node,
        Dictionary<string, (ASTNode Node, int Count)> counts,
        HashSet<string> seen)
    {
        var hash = ComputeSubtreeHash(node);
        if (seen.Add(hash))
        {
            if (counts.TryGetValue(hash, out var existing))
            {
                counts[hash] = (existing.Node, existing.Count + 1);
            }
            else
            {
                counts[hash] = (node, 1);
            }
        }

        foreach (var child in node.Children)
        {
            CountPatterns(child, counts, seen);
        }
    }

    /// <summary>
    /// Infers a simple type for a subtree based on its root node type.
    /// </summary>
    private string InferSubtreeType(ASTNode node)
    {
        return node.NodeType == "Primitive" ? (node.Value ?? "*") : "*";
    }

    /// <summary>
    /// Recursively evaluates a subtree given arguments.
    /// For leaf nodes, returns the first argument or the node value.
    /// For compound nodes, threads the result through each child sequentially.
    /// </summary>
    private static object EvaluateSubtree(ASTNode node, object[] args)
    {
        if (node.Children == null || node.Children.Count == 0)
        {
            return args.Length > 0 ? args[0] : (object)(node.Value ?? string.Empty);
        }

        // For compound nodes, recursively evaluate children in sequence
        object current = args.Length > 0 ? args[0] : (object)string.Empty;
        foreach (var child in node.Children)
        {
            current = EvaluateSubtree(child, new[] { current });
        }

        return current;
    }

    /// <summary>
    /// Infers the output type of an AST node for type-directed beam pruning.
    /// For Primitive nodes, returns the node value as the type.
    /// For Apply nodes, inspects the applied function's arrow type and returns its output part.
    /// </summary>
    private string InferOutputType(ASTNode node)
    {
        if (node.NodeType == "Primitive")
        {
            return node.Value ?? "*";
        }

        if (node.NodeType == "Apply" && node.Children?.Count > 0)
        {
            // The function being applied is identified by node.Value (the primitive name).
            // Look up its type from the DSL primitives via the log-probability table (which
            // contains the same primitives). Alternatively, use the arrow type convention:
            // if the value looks like "inputType -> outputType", extract the output part.
            var funcType = node.Value ?? "*";

            // Check for Unicode arrow (used in some DSL definitions)
            var parts = funcType.Split("\u2192", StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                return parts[1];
            }

            // Check for ASCII arrow
            var arrowIndex = funcType.IndexOf("->", StringComparison.Ordinal);
            if (arrowIndex >= 0)
            {
                return funcType[(arrowIndex + 2)..].Trim();
            }

            // If child is a primitive with an arrow type, infer from that
            var firstChild = node.Children[0];
            if (firstChild.NodeType == "Primitive")
            {
                var childType = firstChild.Value ?? "*";
                var childParts = childType.Split("\u2192", StringSplitOptions.TrimEntries);
                if (childParts.Length == 2)
                {
                    return childParts[1];
                }

                var childArrow = childType.IndexOf("->", StringComparison.Ordinal);
                if (childArrow >= 0)
                {
                    return childType[(childArrow + 2)..].Trim();
                }
            }
        }

        return "*";
    }

    /// <summary>
    /// Checks whether a primitive's input type is compatible with a candidate's output type.
    /// Used for type-directed beam pruning to skip incompatible compositions.
    /// </summary>
    private bool IsTypeCompatible(string primitiveType, string candidateOutputType)
    {
        // Wildcard types are always compatible
        if (candidateOutputType == "*")
        {
            return true;
        }

        if (string.IsNullOrEmpty(primitiveType))
        {
            return true;
        }

        // Split arrow types: "inputType -> outputType"
        var arrowIndex = primitiveType.IndexOf("->", StringComparison.Ordinal);
        if (arrowIndex < 0)
        {
            // Not an arrow type, always compatible
            return true;
        }

        var inputType = primitiveType[..arrowIndex].Trim();

        // Generic/wildcard input types are always compatible
        if (inputType is "*" or "a")
        {
            return true;
        }

        // Check if the candidate output matches the primitive input
        return string.Equals(inputType, candidateOutputType, StringComparison.Ordinal);
    }
}
