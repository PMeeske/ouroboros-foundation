// <copyright file="CausalReasoningEngine.Inference.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Reasoning;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ouroboros.Core.Monads;

/// <summary>
/// Inference partial -- causal discovery, counterfactual estimation, and path-finding helpers.
/// </summary>
public sealed partial class CausalReasoningEngine
{
    private Result<CausalGraph, string> DiscoverUsingPC(List<Observation> data, CancellationToken ct)
    {
        if (data.Count == 0)
        {
            return Result<CausalGraph, string>.Failure("Cannot discover causal structure: observation data is empty");
        }

        // Extract variable names from first observation
        List<string> variableNames = data[0].Values.Keys.ToList();
        int n = variableNames.Count;

        // Initialize complete graph (all edges present)
        bool[,] adjacencyMatrix = new bool[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i != j)
                {
                    adjacencyMatrix[i, j] = true;
                }
            }
        }

        // Phase 1: Skeleton discovery using conditional independence tests
        for (int conditioningSize = 0; conditioningSize <= MaxConditioningSetSize; conditioningSize++)
        {
            ct.ThrowIfCancellationRequested();

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (!adjacencyMatrix[i, j])
                    {
                        continue;
                    }

                    // Get neighbors of i (excluding j)
                    List<int> neighbors = new List<int>();
                    for (int k = 0; k < n; k++)
                    {
                        if (k != i && k != j && adjacencyMatrix[i, k])
                        {
                            neighbors.Add(k);
                        }
                    }

                    // Test all conditioning sets of current size
                    if (GetCombinations(neighbors, conditioningSize)
                        .Any(condSet => TestConditionalIndependence(data, i, j, condSet, variableNames)))
                    {
                        // Remove edge between i and j
                        adjacencyMatrix[i, j] = false;
                        adjacencyMatrix[j, i] = false;
                    }
                }
            }
        }

        // Phase 2: Orient edges to create DAG
        List<CausalEdge> edges = this.OrientEdges(adjacencyMatrix, variableNames, data);

        // Create variables
        List<Variable> variables = variableNames.Select(name => CreateVariable(name, data)).ToList();

        // Create structural equations (simplified)
        Dictionary<string, StructuralEquation> equations = variables.ToDictionary(
            variable => variable.Name,
            variable => new StructuralEquation(
                variable.Name,
                edges.Where(e => e.Effect == variable.Name).Select(e => e.Cause).ToList(),
                values => ComputeVariableValue(variable.Name, values, data),
                1.0));

        CausalGraph causalGraph = new CausalGraph(variables, edges, equations);
        return Result<CausalGraph, string>.Success(causalGraph);
    }

    private static Variable CreateVariable(string name, List<Observation> data)
    {
        List<object> values = data.Select(o => o.Values.TryGetValue(name, out var val) ? val : (object)0.0).Distinct().ToList();
        VariableType type = InferVariableType(values);

        return new Variable(name, type, values);
    }

    private static VariableType InferVariableType(List<object> values)
    {
        if (values.All(v => v is bool || (v is int i && (i == 0 || i == 1))))
        {
            return VariableType.Binary;
        }

        if (values.All(v => v is double || v is int))
        {
            return VariableType.Continuous;
        }

        return VariableType.Categorical;
    }

    private static bool TestConditionalIndependence(
        List<Observation> data,
        int varI,
        int varJ,
        List<int> condSet,
        List<string> variableNames)
    {
        if (data.Count < 5)
        {
            return true; // Insufficient data for reliable independence testing
        }

        string nameI = variableNames[varI];
        string nameJ = variableNames[varJ];

        if (condSet.Count == 0)
        {
            // No conditioning: simple Pearson correlation
            double[] dataI = data.Select(o => o.Values.TryGetValue(nameI, out var val) ? SafeToDouble(val) : 0.0).ToArray();
            double[] dataJ = data.Select(o => o.Values.TryGetValue(nameJ, out var val) ? SafeToDouble(val) : 0.0).ToArray();
            double correlation = ComputeCorrelation(dataI, dataJ);
            return Math.Abs(correlation) < SignificanceLevel;
        }

        // Partial correlation: partition observations by conditioning set values
        List<string> condNames = condSet.Select(idx => variableNames[idx]).ToList();

        var partitions = data.GroupBy(obs =>
            string.Join("|", condNames.Select(c =>
                obs.Values.TryGetValue(c, out var v) ? v?.ToString() ?? string.Empty : string.Empty)));

        double totalCorrelation = 0;
        int partitionCount = 0;

        foreach (var partition in partitions)
        {
            List<Observation> partObs = partition.ToList();
            if (partObs.Count < 3)
            {
                continue; // Too few observations in this partition
            }

            double[] partDataI = partObs.Select(o => o.Values.TryGetValue(nameI, out var val) ? SafeToDouble(val) : 0.0).ToArray();
            double[] partDataJ = partObs.Select(o => o.Values.TryGetValue(nameJ, out var val) ? SafeToDouble(val) : 0.0).ToArray();
            double correlation = ComputeCorrelation(partDataI, partDataJ);
            totalCorrelation += Math.Abs(correlation);
            partitionCount++;
        }

        if (partitionCount == 0)
        {
            return true; // No valid partitions with enough data
        }

        return (totalCorrelation / partitionCount) < SignificanceLevel;
    }

    private static double ComputeCorrelation(double[] x, double[] y)
    {
        if (x.Length != y.Length || x.Length == 0)
        {
            return 0;
        }

        double meanX = x.Average();
        double meanY = y.Average();

        double numerator = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum();
        double denomX = Math.Sqrt(x.Select(xi => Math.Pow(xi - meanX, 2)).Sum());
        double denomY = Math.Sqrt(y.Select(yi => Math.Pow(yi - meanY, 2)).Sum());

        if (denomX == 0 || denomY == 0)
        {
            return 0;
        }

        return numerator / (denomX * denomY);
    }

    private List<CausalEdge> OrientEdges(bool[,] adjacencyMatrix, List<string> variableNames, List<Observation> data)
    {
        int n = variableNames.Count;

        // Build mutable edge state: (from, to) -> (strength, type, oriented)
        // Track undirected edges as pairs keyed by canonical ordering (i < j)
        var edgeStrength = new Dictionary<(int From, int To), double>();
        var edgeType = new Dictionary<(int From, int To), EdgeType>();
        var edgeOriented = new Dictionary<(int From, int To), bool>();

        // Initialize all skeleton edges as undirected with computed correlation strength
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (adjacencyMatrix[i, j])
                {
                    string nameI = variableNames[i];
                    string nameJ = variableNames[j];

                    double[] dataI = data.Select(o => o.Values.TryGetValue(nameI, out var valI) ? SafeToDouble(valI) : 0.0).ToArray();
                    double[] dataJ = data.Select(o => o.Values.TryGetValue(nameJ, out var valJ) ? SafeToDouble(valJ) : 0.0).ToArray();

                    double strength = Math.Abs(ComputeCorrelation(dataI, dataJ));

                    // Store with canonical key (i < j), default orientation i -> j
                    edgeStrength[(i, j)] = strength;
                    edgeType[(i, j)] = EdgeType.Direct;
                    edgeOriented[(i, j)] = false;
                }
            }
        }

        // Phase 1: V-structure detection (colliders)
        // For each triple X-Z-Y where X-Z and Z-Y are adjacent but X-Y are NOT adjacent,
        // orient as X -> Z <- Y (collider at Z)
        for (int z = 0; z < n; z++)
        {
            // Find all neighbors of z in the skeleton
            List<int> neighbors = new List<int>();
            for (int k = 0; k < n; k++)
            {
                if (k == z)
                {
                    continue;
                }

                (int lo, int hi) = k < z ? (k, z) : (z, k);
                if (edgeStrength.ContainsKey((lo, hi)))
                {
                    neighbors.Add(k);
                }
            }

            // Check all pairs of neighbors
            for (int ni = 0; ni < neighbors.Count; ni++)
            {
                for (int nj = ni + 1; nj < neighbors.Count; nj++)
                {
                    int x = neighbors[ni];
                    int y = neighbors[nj];

                    // Check that X and Y are NOT adjacent
                    (int xLo, int xHi) = x < y ? (x, y) : (y, x);
                    if (edgeStrength.ContainsKey((xLo, xHi)))
                    {
                        continue; // X-Y adjacent, not a v-structure
                    }

                    // Orient X -> Z: ensure edge key points from x to z
                    (int xzLo, int xzHi) = x < z ? (x, z) : (z, x);
                    edgeType[(xzLo, xzHi)] = EdgeType.Collider;
                    edgeOriented[(xzLo, xzHi)] = true;
                    // Store direction: cause=x, effect=z. If x < z, canonical key is (x,z) so From=x, To=z is correct.
                    // If z < x, canonical key is (z,x) and we need to flip direction.
                    // We'll track actual direction separately.

                    // Orient Y -> Z similarly
                    (int yzLo, int yzHi) = y < z ? (y, z) : (z, y);
                    edgeType[(yzLo, yzHi)] = EdgeType.Collider;
                    edgeOriented[(yzLo, yzHi)] = true;
                }
            }
        }

        // For v-structures, track the actual direction: cause -> effect
        // We need a direction map: canonical key -> actual (cause, effect) indices
        var edgeDirection = new Dictionary<(int, int), (int Cause, int Effect)>();

        foreach (var key in edgeStrength.Keys)
        {
            // Default: cause=key.From (lower index), effect=key.To (higher index)
            edgeDirection[key] = (key.From, key.To);
        }

        // Re-apply v-structure directions: for X->Z, cause=X, effect=Z
        for (int z = 0; z < n; z++)
        {
            List<int> neighbors = new List<int>();
            for (int k = 0; k < n; k++)
            {
                if (k == z)
                {
                    continue;
                }

                (int lo, int hi) = k < z ? (k, z) : (z, k);
                if (edgeStrength.ContainsKey((lo, hi)))
                {
                    neighbors.Add(k);
                }
            }

            for (int ni = 0; ni < neighbors.Count; ni++)
            {
                for (int nj = ni + 1; nj < neighbors.Count; nj++)
                {
                    int x = neighbors[ni];
                    int y = neighbors[nj];

                    (int xLo, int xHi) = x < y ? (x, y) : (y, x);
                    if (edgeStrength.ContainsKey((xLo, xHi)))
                    {
                        continue;
                    }

                    // X -> Z: cause=x, effect=z
                    (int xzLo, int xzHi) = x < z ? (x, z) : (z, x);
                    edgeDirection[(xzLo, xzHi)] = (x, z);

                    // Y -> Z: cause=y, effect=z
                    (int yzLo, int yzHi) = y < z ? (y, z) : (z, y);
                    edgeDirection[(yzLo, yzHi)] = (y, z);
                }
            }
        }

        // Phase 2: Meek Rules -- iteratively orient edges to complete the CPDAG
        // Helper: check whether two node indices are adjacent in the skeleton
        bool AreAdjacent(int x, int y)
        {
            (int lo, int hi) = x < y ? (x, y) : (y, x);
            return edgeStrength.ContainsKey((lo, hi));
        }

        // Helper: check if there is an oriented edge from 'cause' to 'effect'
        bool HasDirectedEdge(int cause, int effect)
        {
            (int lo, int hi) = cause < effect ? (cause, effect) : (effect, cause);
            return edgeStrength.ContainsKey((lo, hi))
                && edgeOriented[(lo, hi)]
                && edgeDirection[(lo, hi)].Cause == cause
                && edgeDirection[(lo, hi)].Effect == effect;
        }

        // Helper: orient an unoriented edge from 'cause' to 'effect'
        bool TryOrient(int cause, int effect, ref bool changedFlag)
        {
            (int lo, int hi) = cause < effect ? (cause, effect) : (effect, cause);
            if (!edgeStrength.ContainsKey((lo, hi)) || edgeOriented[(lo, hi)])
            {
                return false;
            }

            edgeDirection[(lo, hi)] = (cause, effect);
            edgeType[(lo, hi)] = EdgeType.Direct;
            edgeOriented[(lo, hi)] = true;
            changedFlag = true;
            return true;
        }

        // All node indices for iteration
        var allNodes = Enumerable.Range(0, n).ToList();

        bool changed = true;
        int maxIterations = n * n;
        while (changed && maxIterations-- > 0)
        {
            changed = false;

            foreach (var key in edgeStrength.Keys)
            {
                if (edgeOriented[key])
                {
                    continue; // Already oriented
                }

                int a = key.From;
                int b = key.To;

                // Meek Rule 1: If X->Z-Y and X is NOT adjacent to Y, orient Z->Y
                // Check direction a->b: is there X->a where X is not adjacent to b?
                bool rule1AB = allNodes.Any(x =>
                    x != a && x != b && HasDirectedEdge(x, a) && !AreAdjacent(x, b));
                if (rule1AB)
                {
                    TryOrient(a, b, ref changed);
                    continue;
                }

                // Check direction b->a: is there X->b where X is not adjacent to a?
                bool rule1BA = allNodes.Any(x =>
                    x != a && x != b && HasDirectedEdge(x, b) && !AreAdjacent(x, a));
                if (rule1BA)
                {
                    TryOrient(b, a, ref changed);
                    continue;
                }

                // Meek Rule 2: If X->Z->Y and X-Y unoriented, orient X->Y
                // Check a->b: is there Z such that a->Z and Z->b?
                bool rule2AB = allNodes.Any(z =>
                    z != a && z != b && HasDirectedEdge(a, z) && HasDirectedEdge(z, b));
                if (rule2AB)
                {
                    TryOrient(a, b, ref changed);
                    continue;
                }

                // Check b->a: is there Z such that b->Z and Z->a?
                bool rule2BA = allNodes.Any(z =>
                    z != a && z != b && HasDirectedEdge(b, z) && HasDirectedEdge(z, a));
                if (rule2BA)
                {
                    TryOrient(b, a, ref changed);
                    continue;
                }

                // Meek Rule 3: If X-Y, and there exist Z1,Z2 both adjacent to X and both
                // with oriented edges Z1->Y and Z2->Y, and Z1 not adjacent to Z2, orient X->Y
                // Check a->b direction
                var neighborsOfA = allNodes.Where(x => x != a && x != b && AreAdjacent(x, a)).ToList();
                var directedIntoB = neighborsOfA.Where(z => HasDirectedEdge(z, b)).ToList();
                bool rule3AB = false;
                for (int zi = 0; zi < directedIntoB.Count && !rule3AB; zi++)
                {
                    for (int zj = zi + 1; zj < directedIntoB.Count && !rule3AB; zj++)
                    {
                        if (!AreAdjacent(directedIntoB[zi], directedIntoB[zj]))
                        {
                            rule3AB = true;
                        }
                    }
                }

                if (rule3AB)
                {
                    TryOrient(a, b, ref changed);
                    continue;
                }

                // Check b->a direction
                var neighborsOfB = allNodes.Where(x => x != a && x != b && AreAdjacent(x, b)).ToList();
                var directedIntoA = neighborsOfB.Where(z => HasDirectedEdge(z, a)).ToList();
                bool rule3BA = false;
                for (int zi = 0; zi < directedIntoA.Count && !rule3BA; zi++)
                {
                    for (int zj = zi + 1; zj < directedIntoA.Count && !rule3BA; zj++)
                    {
                        if (!AreAdjacent(directedIntoA[zi], directedIntoA[zj]))
                        {
                            rule3BA = true;
                        }
                    }
                }

                if (rule3BA)
                {
                    TryOrient(b, a, ref changed);
                    continue;
                }

                // Meek Rule 4: If X-Y, and there exists Z such that Z->Y, and W such that
                // W->Z and W-X (undirected) and W not adjacent to Y, orient X->Y
                // Check a->b direction
                bool rule4AB = allNodes.Any(z =>
                    z != a && z != b && HasDirectedEdge(z, b) &&
                    allNodes.Any(w =>
                        w != a && w != b && w != z &&
                        HasDirectedEdge(w, z) && AreAdjacent(w, a) && !AreAdjacent(w, b)));
                if (rule4AB)
                {
                    TryOrient(a, b, ref changed);
                    continue;
                }

                // Check b->a direction
                bool rule4BA = allNodes.Any(z =>
                    z != a && z != b && HasDirectedEdge(z, a) &&
                    allNodes.Any(w =>
                        w != a && w != b && w != z &&
                        HasDirectedEdge(w, z) && AreAdjacent(w, b) && !AreAdjacent(w, a)));
                if (rule4BA)
                {
                    TryOrient(b, a, ref changed);
                    continue;
                }
            }
        }

        // Build final edge list
        List<CausalEdge> edges = new List<CausalEdge>();
        foreach (var key in edgeStrength.Keys)
        {
            var (cause, effect) = edgeDirection[key];
            string causeName = variableNames[cause];
            string effectName = variableNames[effect];
            edges.Add(new CausalEdge(causeName, effectName, edgeStrength[key], edgeType[key]));
        }

        return edges;
    }

    private static object ComputeVariableValue(string variable, Dictionary<string, object> values, List<Observation> data)
    {
        // Simplified computation - in practice, use learned structural equation
        if (values.ContainsKey(variable))
        {
            return values[variable];
        }

        // Return mean value from data
        double[] samples = data.Select(o => o.Values.TryGetValue(variable, out var val) ? SafeToDouble(val) : 0.0).ToArray();
        return samples.Average();
    }

    private IEnumerable<List<int>> GetCombinations(List<int> items, int size)
    {
        if (size == 0)
        {
            yield return new List<int>();
            yield break;
        }

        if (items.Count < size)
        {
            yield break;
        }

        for (int i = 0; i <= items.Count - size; i++)
        {
            int item = items[i];
            List<int> remainingItems = items.Skip(i + 1).ToList();

            foreach (List<int> combination in this.GetCombinations(remainingItems, size - 1))
            {
                yield return new List<int> { item }.Concat(combination).ToList();
            }
        }
    }

    private Result<double, string> ComputeInterventionEffect(
        string intervention,
        string outcome,
        CausalGraph model,
        CancellationToken ct)
    {
        // Find path from intervention to outcome
        List<CausalPath> paths = this.FindAllPaths(intervention, outcome, model);

        if (paths.Count == 0)
        {
            return Result<double, string>.Success(0.0); // No causal effect
        }

        // Compute total causal effect (sum of path effects)
        double totalEffect = 0;

        foreach (CausalPath path in paths)
        {
            double pathEffect = 1.0;
            foreach (CausalEdge edge in path.Edges)
            {
                pathEffect *= edge.Strength;
            }

            totalEffect += pathEffect;
        }

        return Result<double, string>.Success(totalEffect);
    }

    private static Result<Distribution, string> ComputeCounterfactual(
        string intervention,
        string outcome,
        Observation factual,
        CausalGraph model,
        CancellationToken ct)
    {
        // Twin network approach: abduct exogenous variables, modify intervention, predict outcome

        // Step 1: Abduct exogenous variables from factual observation
        Dictionary<string, object> exogenousVars = AbductExogenousVariables(factual, model);

        // Step 2: Simulate intervention in counterfactual world
        Dictionary<string, object> counterfactualValues = new Dictionary<string, object>(factual.Values);

        // Find the intervention variable and set it to counterfactual value
        // (Simplified - should parse intervention string properly)
        Variable? interventionVar = model.Variables.FirstOrDefault(v => v.Name == intervention);
        if (interventionVar != null)
        {
            counterfactualValues[intervention] = interventionVar.PossibleValues.FirstOrDefault() ?? 0;
        }

        // Step 3: Propagate effects using structural equations (hold intervened variables fixed)
        var interventionSet = interventionVar != null ? new HashSet<string> { intervention } : null;
        object counterfactualOutcome = PropagateEffects(outcome, counterfactualValues, model, exogenousVars, interventionSet);

        // Step 4: Create distribution (simplified as point estimate)
        Distribution distribution = new Distribution(
            DistributionType.Empirical,
            Convert.ToDouble(counterfactualOutcome),
            0.1,
            new List<double> { Convert.ToDouble(counterfactualOutcome) },
            new Dictionary<object, double> { { counterfactualOutcome, 1.0 } });

        return Result<Distribution, string>.Success(distribution);
    }

    private static Dictionary<string, object> AbductExogenousVariables(Observation factual, CausalGraph model)
    {
        // Compute exogenous (noise) variables as residuals: U_i = actual_i - predicted_i
        // For variables with structural equations, the residual captures the unexplained
        // component. For root variables (no equation or no parents), the exogenous value
        // is the observed value itself.
        return model.Variables.ToDictionary(
            variable => variable.Name,
            variable => AbductSingleVariable(variable.Name, factual, model));
    }

    private static object AbductSingleVariable(string variableName, Observation factual, CausalGraph model)
    {
        if (model.Equations.TryGetValue(variableName, out StructuralEquation? eq)
            && eq.Function != null
            && eq.Parents.Count > 0)
        {
            try
            {
                // Compute predicted value from structural equation
                double predicted = Convert.ToDouble(eq.Function(factual.Values));
                double actual = Convert.ToDouble(factual.Values[variableName]);

                // Residual = actual - predicted (the exogenous noise term)
                return actual - predicted;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Non-numeric or missing values: fall back to observed value
                System.Diagnostics.Trace.TraceWarning($"Abduction failed for variable: {ex.Message}");
                return factual.Values.TryGetValue(variableName, out var v)
                    ? v
                    : 0.0;
            }
        }

        // No structural equation or no parents: exogenous IS the observed value
        return factual.Values.TryGetValue(variableName, out var val)
            ? val
            : 0.0;
    }

    private static object PropagateEffects(
        string outcome,
        Dictionary<string, object> values,
        CausalGraph model,
        Dictionary<string, object> exogenousVars,
        HashSet<string>? interventions = null)
    {
        // Propagate effects through ALL downstream variables in topological order,
        // not just the single outcome variable. This ensures intermediate variables
        // are re-evaluated with intervened values before the outcome is computed.
        var visited = new HashSet<string>();
        var order = new List<string>();
        var inStack = new HashSet<string>();

        foreach (var v in model.Variables)
            TopologicalSort(v.Name, model, visited, order, inStack);

        // Re-evaluate all variables in topological order using structural equations
        double lastValue = 0;
        foreach (var varName in order)
        {
            if (model.Equations.TryGetValue(varName, out StructuralEquation? eq) && eq.Parents.Count > 0)
            {
                // Skip variables whose value was set by intervention (do-operator: hold fixed)
                if (interventions != null && interventions.Contains(varName))
                    continue;

                try
                {
                    double predicted = Convert.ToDouble(eq.Function(values));

                    // Add exogenous noise term (residual from abduction step)
                    if (exogenousVars.TryGetValue(varName, out var noise))
                    {
                        predicted += Convert.ToDouble(noise);
                    }

                    values[varName] = predicted;
                    lastValue = predicted;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Non-numeric variable -- skip but log for diagnostics
                    System.Diagnostics.Trace.TraceWarning($"PropagateEffects: skipping variable due to error: {ex.Message}");
                }
            }
        }

        // Return the outcome variable's value specifically
        if (values.TryGetValue(outcome, out var outcomeValue))
        {
            return outcomeValue;
        }

        return lastValue;
    }

    private static void TopologicalSort(string varName, CausalGraph model,
        HashSet<string> visited, List<string> order, HashSet<string>? inStack = null)
    {
        if (visited.Contains(varName)) return;

        inStack ??= new HashSet<string>();

        if (!inStack.Add(varName))
        {
            // Cycle detected — break it to prevent infinite recursion
            System.Diagnostics.Trace.TraceWarning(
                $"[CausalReasoningEngine] Cycle detected in causal graph at variable '{varName}'. Breaking cycle.");
            return;
        }

        if (model.Equations.TryGetValue(varName, out StructuralEquation? eq) && eq.Parents != null)
        {
            foreach (var parent in eq.Parents)
                TopologicalSort(parent, model, visited, order, inStack);
        }

        inStack.Remove(varName);
        visited.Add(varName);
        order.Add(varName);
    }

    /// <summary>
    /// Safely converts a value to double, returning 0.0 for non-numeric or null values
    /// instead of throwing FormatException or InvalidCastException.
    /// </summary>
    private static double SafeToDouble(object? value)
    {
        if (value == null)
        {
            return 0.0;
        }

        if (value is double d)
        {
            return d;
        }

        if (value is int i)
        {
            return i;
        }

        if (value is float f)
        {
            return f;
        }

        if (value is long l)
        {
            return l;
        }

        if (value is bool b)
        {
            return b ? 1.0 : 0.0;
        }

        return double.TryParse(value.ToString(), out double result) ? result : 0.0;
    }

    private Result<Explanation, string> GenerateCausalExplanation(
        string effect,
        List<string> possibleCauses,
        CausalGraph model,
        CancellationToken ct)
    {
        // Find all causal paths from potential causes to effect
        List<CausalPath> allPaths = new List<CausalPath>();
        Dictionary<string, double> attributions = new Dictionary<string, double>();

        foreach (string cause in possibleCauses)
        {
            List<CausalPath> paths = this.FindAllPaths(cause, effect, model);
            allPaths.AddRange(paths);

            // Compute attribution as total effect from this cause
            double attribution = 0;
            foreach (CausalPath path in paths)
            {
                attribution += path.TotalEffect;
            }

            attributions[cause] = attribution;
        }

        // Normalize attributions
        double totalAttribution = attributions.Values.Sum();
        if (totalAttribution > 0)
        {
            Dictionary<string, double> normalizedAttributions = attributions.ToDictionary(
                kv => kv.Key,
                kv => kv.Value / totalAttribution);
            attributions = normalizedAttributions;
        }

        // Generate narrative explanation
        string narrative = GenerateNarrativeExplanation(effect, attributions, allPaths);

        Explanation explanation = new Explanation(effect, allPaths, attributions, narrative);
        return Result<Explanation, string>.Success(explanation);
    }

}
