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
                foreach (Primitive primitive in dsl.Primitives)
                {
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
                    object? result = await ExecuteProgramAsync(node, example.Input, ct);
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

    private async Task<object?> ExecuteProgramAsync(ASTNode node, object input, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (node.NodeType == "Primitive")
        {
            await Task.CompletedTask;
            return input;
        }

        if (node.NodeType == "Apply")
        {
            List<object> childResults = new List<object>();
            foreach (ASTNode child in node.Children)
            {
                object? result = await ExecuteProgramAsync(child, input, ct);
                if (result != null)
                {
                    childResults.Add(result);
                }
            }

            await Task.CompletedTask;
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
        return extractedPrimitives.Distinct().ToList();
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

    private async Task<List<Primitive>> ExtractViaEGraphAsync(List<Program> programs, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Task.CompletedTask;
        return new List<Primitive>();
    }

    private async Task<List<Primitive>> ExtractViaFragmentGrammarAsync(List<Program> programs, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Task.CompletedTask;
        return new List<Primitive>();
    }
}
