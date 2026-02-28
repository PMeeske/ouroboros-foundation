// <copyright file="CausalReasoningEngine.GraphTraversal.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Reasoning;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ouroboros.Core.Monads;

/// <summary>
/// Graph traversal partial -- narrative generation, intervention planning, and path-finding.
/// </summary>
public sealed partial class CausalReasoningEngine
{
    private static string GenerateNarrativeExplanation(
        string effect,
        Dictionary<string, double> attributions,
        List<CausalPath> paths)
    {
        List<KeyValuePair<string, double>> sortedCauses = attributions.OrderByDescending(kv => kv.Value).ToList();

        string narrative = $"The effect on '{effect}' is primarily caused by: ";

        List<string> topCauses = sortedCauses.Take(3).Select(kv =>
            $"{kv.Key} ({kv.Value:P1})").ToList();

        narrative += string.Join(", ", topCauses);

        if (paths.Any(p => p.IsDirect))
        {
            narrative += ". Direct causal paths were identified.";
        }
        else
        {
            narrative += ". The causal effects are mediated through intermediate variables.";
        }

        return narrative;
    }

    private Result<Intervention, string> PlanOptimalIntervention(
        string desiredOutcome,
        CausalGraph model,
        List<string> controllableVariables,
        CancellationToken ct)
    {
        // Find best intervention by evaluating effect of each controllable variable
        Intervention bestIntervention = new Intervention(string.Empty, 0, 0, 0, new List<string>());
        double maxEffect = 0.0;

        foreach (string variable in controllableVariables)
        {
            List<CausalPath> paths = this.FindAllPaths(variable, desiredOutcome, model);

            if (paths.Count == 0)
            {
                continue;
            }

            // Compute total effect
            double effect = paths.Sum(p => p.TotalEffect);

            if (Math.Abs(effect) > Math.Abs(maxEffect))
            {
                maxEffect = effect;

                // Find variables affected by this intervention
                List<string> sideEffects = FindAffectedVariables(variable, desiredOutcome, model);

                Variable? targetVar = model.Variables.FirstOrDefault(v => v.Name == variable);
                object newValue = targetVar?.PossibleValues.LastOrDefault() ?? 1.0;

                bestIntervention = new Intervention(
                    variable,
                    newValue,
                    effect,
                    0.8, // Simplified confidence
                    sideEffects);
            }
        }

        if (string.IsNullOrEmpty(bestIntervention.TargetVariable))
        {
            return Result<Intervention, string>.Failure("No effective intervention found");
        }

        return Result<Intervention, string>.Success(bestIntervention);
    }

    private static List<string> FindAffectedVariables(string source, string target, CausalGraph model)
    {
        List<string> affected = new List<string>();

        // Find all variables reachable from source (excluding target)
        HashSet<string> visited = new HashSet<string>();
        Queue<string> queue = new Queue<string>();
        queue.Enqueue(source);
        visited.Add(source);

        while (queue.Count > 0)
        {
            string current = queue.Dequeue();

            foreach (string effect in model.Edges.Where(e => e.Cause == current && !visited.Contains(e.Effect) && e.Effect != target).Select(edge => edge.Effect))
            {
                affected.Add(effect);
                visited.Add(effect);
                queue.Enqueue(effect);
            }
        }

        return affected;
    }

    private List<CausalPath> FindAllPaths(string source, string target, CausalGraph model)
    {
        List<CausalPath> paths = new List<CausalPath>();
        List<string> currentPath = new List<string> { source };
        List<CausalEdge> currentEdges = new List<CausalEdge>();

        this.FindPathsRecursive(source, target, model, currentPath, currentEdges, paths, new HashSet<string> { source });

        return paths;
    }

    private void FindPathsRecursive(
        string current,
        string target,
        CausalGraph model,
        List<string> currentPath,
        List<CausalEdge> currentEdges,
        List<CausalPath> allPaths,
        HashSet<string> visited)
    {
        if (current == target)
        {
            // Found a path
            double totalEffect = currentEdges.Aggregate(1.0, (acc, edge) => acc * edge.Strength);
            bool isDirect = currentPath.Count == 2;

            allPaths.Add(new CausalPath(
                new List<string>(currentPath),
                totalEffect,
                isDirect,
                new List<CausalEdge>(currentEdges)));

            return;
        }

        // Explore neighbors
        foreach (CausalEdge? edge in model.Edges.Where(e => e.Cause == current && !visited.Contains(e.Effect)))
        {
            visited.Add(edge.Effect);
            currentPath.Add(edge.Effect);
            currentEdges.Add(edge);

            this.FindPathsRecursive(edge.Effect, target, model, currentPath, currentEdges, allPaths, visited);

            currentPath.RemoveAt(currentPath.Count - 1);
            currentEdges.RemoveAt(currentEdges.Count - 1);
            visited.Remove(edge.Effect);
        }
    }
}
