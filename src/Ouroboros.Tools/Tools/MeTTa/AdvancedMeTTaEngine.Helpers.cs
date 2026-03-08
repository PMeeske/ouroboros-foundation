// <copyright file="AdvancedMeTTaEngine.Helpers.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Helper and private methods for the AdvancedMeTTaEngine.
/// </summary>
public sealed partial class AdvancedMeTTaEngine
{
    private static Pattern? ExtractPattern(List<Fact> facts)
    {
        if (facts.Count == 0)
        {
            return null;
        }

        var first = facts[0];
        var variables = new List<string>();

        // Simple pattern: use first fact as template with variables
        for (int i = 0; i < first.Arguments.Count; i++)
        {
            variables.Add($"$x{i}");
        }

        var template = $"({first.Predicate} {string.Join(" ", variables)})";
        return new Pattern(template, variables);
    }

    private static double CalculateConfidence(List<Fact> facts)
    {
        if (facts.Count == 0)
        {
            return 0.0;
        }

        return facts.Average(f => f.Confidence);
    }

    private static Fact? ParseFactPattern(string factString)
    {
        // Parse (predicate arg1 arg2 ...)
        var match = Regex.Match(factString, @"\((\w+)\s+(.+)\)");
        if (!match.Success)
        {
            return null;
        }

        var predicate = match.Groups[1].Value;
        var argsString = match.Groups[2].Value;
        var args = argsString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        return new Fact(predicate, args);
    }

    private static Hypothesis? GenerateHypothesis(Fact observation, string knowledge)
    {
        // Simple hypothesis generation: if knowledge implies observation
        var knowledgeFact = ParseFactPattern(knowledge);
        if (knowledgeFact != null)
        {
            return new Hypothesis(
                $"If {knowledge} then ({observation.Predicate} {string.Join(" ", observation.Arguments)})",
                0.7,
                new List<Fact> { knowledgeFact, observation });
        }

        return null;
    }

    private static string InferTypeFromPattern(string atom, TypeContext context)
    {
        // Check if atom is in context bindings
        if (context.Bindings.TryGetValue(atom, out var boundType))
        {
            return boundType;
        }

        // Simple type inference based on patterns
        if (Regex.IsMatch(atom, @"^\d+$"))
        {
            return "Int";
        }

        if (Regex.IsMatch(atom, @"^\d+\.\d+$"))
        {
            return "Float";
        }

        if (atom.StartsWith('"') && atom.EndsWith('"'))
        {
            return "String";
        }

        if (atom.StartsWith('$'))
        {
            return "Var";
        }

        if (atom.StartsWith('(') && atom.EndsWith(')'))
        {
            return "Expr";
        }

        return "Unknown";
    }

    private static List<Dictionary<string, string>> FindMatchingFacts(List<Pattern> premises, List<Fact> facts)
    {
        var matches = new List<Dictionary<string, string>>();

        if (premises.Count == 0)
        {
            return matches;
        }

        // Simple unification for single premise
        var premise = premises[0];
        foreach (var fact in facts)
        {
            var bindings = TryUnify(premise, fact);
            if (bindings != null)
            {
                matches.Add(bindings);
            }
        }

        return matches;
    }

    private static Dictionary<string, string>? TryUnify(Pattern pattern, Fact fact)
    {
        var bindings = new Dictionary<string, string>();

        // Extract predicate from pattern
        var match = Regex.Match(pattern.Template, @"\((\w+)\s+(.+)\)");
        if (!match.Success)
        {
            return null;
        }

        var predicate = match.Groups[1].Value;
        if (predicate != fact.Predicate)
        {
            return null;
        }

        var patternVars = match.Groups[2].Value.Split(' ');
        if (patternVars.Length != fact.Arguments.Count)
        {
            return null;
        }

        for (int i = 0; i < patternVars.Length; i++)
        {
            var patternVar = patternVars[i];
            if (patternVar.StartsWith('$'))
            {
                bindings[patternVar] = fact.Arguments[i];
            }
            else if (patternVar != fact.Arguments[i])
            {
                return null;
            }
        }

        return bindings;
    }

    private static Fact? ApplyRule(Rule rule, Dictionary<string, string> bindings)
    {
        // Apply bindings to conclusion
        var match = Regex.Match(rule.Conclusion.Template, @"\((\w+)\s+(.+)\)");
        if (!match.Success)
        {
            return null;
        }

        var predicate = match.Groups[1].Value;
        var vars = match.Groups[2].Value.Split(' ');
        var args = vars.Select(v => bindings.ContainsKey(v) ? bindings[v] : v).ToList();

        return new Fact(predicate, args, rule.Confidence);
    }

    private static bool BackwardChainRecursive(
        Fact goal,
        List<Rule> rules,
        List<Fact> knownFacts,
        List<Fact> requiredFacts,
        HashSet<Fact> visited)
    {
        if (visited.Contains(goal))
        {
            return false;
        }

        visited.Add(goal);

        // Check if goal is in known facts
        if (knownFacts.Any(f => FactsMatch(f, goal)))
        {
            requiredFacts.Add(goal);
            return true;
        }

        // Try to prove goal using rules
        foreach (var rule in rules)
        {
            var bindings = TryUnify(rule.Conclusion, goal);
            if (bindings != null)
            {
                var allPremisesSatisfied = true;
                foreach (var premise in rule.Premises)
                {
                    var premiseFact = InstantiatePremise(premise, bindings);
                    if (premiseFact != null && !BackwardChainRecursive(premiseFact, rules, knownFacts, requiredFacts, visited))
                    {
                        allPremisesSatisfied = false;
                        break;
                    }
                }

                if (allPremisesSatisfied)
                {
                    requiredFacts.Add(goal);
                    return true;
                }
            }
        }

        return false;
    }

    private static bool FactsMatch(Fact f1, Fact f2)
    {
        return f1.Predicate == f2.Predicate &&
               f1.Arguments.Count == f2.Arguments.Count &&
               f1.Arguments.SequenceEqual(f2.Arguments);
    }

    private static Fact? InstantiatePremise(Pattern premise, Dictionary<string, string> bindings)
    {
        var match = Regex.Match(premise.Template, @"\((\w+)\s+(.+)\)");
        if (!match.Success)
        {
            return null;
        }

        var predicate = match.Groups[1].Value;
        var vars = match.Groups[2].Value.Split(' ');
        var args = vars.Select(v => bindings.ContainsKey(v) ? bindings[v] : v).ToList();

        return new Fact(predicate, args);
    }

    private static bool TryResolve(List<string> clauseSet, List<ProofStep> steps)
    {
        // Simplified resolution - check for basic contradictions
        for (int i = 0; i < clauseSet.Count; i++)
        {
            for (int j = i + 1; j < clauseSet.Count; j++)
            {
                if (AreContradictory(clauseSet[i], clauseSet[j]))
                {
                    var rule = new Rule("resolution", new List<Pattern>(), new Pattern("[]", new List<string>()));
                    steps.Add(new ProofStep($"Resolved {clauseSet[i]} with {clauseSet[j]}", rule, new List<Fact>()));
                    return true;
                }
            }
        }

        return false;
    }

    private static bool AreContradictory(string clause1, string clause2)
    {
        // Simple check: one is NOT of the other
        return (clause1.StartsWith("NOT (") && clause1.Contains(clause2)) ||
               (clause2.StartsWith("NOT (") && clause2.Contains(clause1));
    }
}
