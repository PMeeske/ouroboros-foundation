using Ouroboros.Monads;

namespace Ouroboros.Specs.Steps.Ethics;

/// <summary>
/// A lightweight in-process MeTTa engine for BDD testing.
/// Stores facts in memory and supports basic match queries
/// without requiring a MeTTa subprocess installation.
/// </summary>
public sealed class SimpleMockMeTTaEngine : IMeTTaEngine
{
    private readonly List<string> _facts = new();
    private readonly Dictionary<string, List<string>> _taggedFacts = new();
    private bool _disposed;

    public IReadOnlyList<string> Facts => _facts.AsReadOnly();

    public Task<Result<string, string>> ExecuteQueryAsync(string query, CancellationToken ct = default)
    {
        if (_disposed) return Task.FromResult(Result<string, string>.Failure("Engine disposed"));

        // Support basic (match &self (pattern) template) queries
        string trimmed = query.Trim();
        if (trimmed.StartsWith("(match", StringComparison.OrdinalIgnoreCase))
        {
            List<string> matches = FindMatches(trimmed);
            string result = matches.Count > 0
                ? string.Join("\n", matches)
                : "()";
            return Task.FromResult(Result<string, string>.Success(result));
        }

        // For any other query, search facts for atoms that contain the query terms
        List<string> relevant = _facts
            .Where(f => ContainsQueryTerms(f, trimmed))
            .ToList();

        string output = relevant.Count > 0
            ? string.Join("\n", relevant)
            : "()";
        return Task.FromResult(Result<string, string>.Success(output));
    }

    public Task<Result<Unit, string>> AddFactAsync(string fact, CancellationToken ct = default)
    {
        if (_disposed) return Task.FromResult(Result<Unit, string>.Failure("Engine disposed"));

        string trimmed = fact.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) return Task.FromResult(Result<Unit, string>.Success(Unit.Value));

        _facts.Add(trimmed);

        // Index by first atom for faster lookup
        string tag = ExtractFirstAtom(trimmed);
        if (!string.IsNullOrEmpty(tag))
        {
            if (!_taggedFacts.ContainsKey(tag))
                _taggedFacts[tag] = new List<string>();
            _taggedFacts[tag].Add(trimmed);
        }

        return Task.FromResult(Result<Unit, string>.Success(Unit.Value));
    }

    public Task<Result<string, string>> ApplyRuleAsync(string rule, CancellationToken ct = default)
    {
        if (_disposed) return Task.FromResult(Result<string, string>.Failure("Engine disposed"));

        _facts.Add(rule.Trim());
        return Task.FromResult(Result<string, string>.Success("Rule added"));
    }

    public Task<Result<bool, string>> VerifyPlanAsync(string plan, CancellationToken ct = default)
    {
        if (_disposed) return Task.FromResult(Result<bool, string>.Failure("Engine disposed"));
        return Task.FromResult(Result<bool, string>.Success(true));
    }

    public Task<Result<Unit, string>> ResetAsync(CancellationToken ct = default)
    {
        _facts.Clear();
        _taggedFacts.Clear();
        return Task.FromResult(Result<Unit, string>.Success(Unit.Value));
    }

    public bool ContainsFact(string fragment)
    {
        return _facts.Any(f => f.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<string> FindFactsContaining(string fragment)
    {
        return _facts
            .Where(f => f.Contains(fragment, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public void Dispose()
    {
        _disposed = true;
    }

    private List<string> FindMatches(string matchQuery)
    {
        // Extract the pattern from (match &self (pattern) template)
        // Simplified: look for facts containing the key atoms in the pattern
        string inner = matchQuery;
        int openParen = inner.IndexOf('(', 1);
        if (openParen < 0) return new List<string>();

        // Skip past "&self" to get to the pattern
        int selfIdx = inner.IndexOf("&self", StringComparison.OrdinalIgnoreCase);
        if (selfIdx >= 0)
        {
            int patternStart = inner.IndexOf('(', selfIdx);
            if (patternStart >= 0)
            {
                string patternArea = inner.Substring(patternStart);
                string[] terms = ExtractTerms(patternArea);
                return _facts
                    .Where(f => terms.Any(t =>
                        !t.StartsWith("$") &&
                        f.Contains(t, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
        }

        return new List<string>();
    }

    private static bool ContainsQueryTerms(string fact, string query)
    {
        string[] terms = ExtractTerms(query);
        return terms
            .Where(t => !t.StartsWith("$") && t.Length > 1)
            .Any(t => fact.Contains(t, StringComparison.OrdinalIgnoreCase));
    }

    private static string[] ExtractTerms(string expression)
    {
        return expression
            .Replace("(", " ")
            .Replace(")", " ")
            .Replace("=", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length > 1 && t != "&self" && t != "&kb")
            .ToArray();
    }

    private static string ExtractFirstAtom(string fact)
    {
        string stripped = fact.TrimStart('(');
        int space = stripped.IndexOf(' ');
        return space > 0 ? stripped.Substring(0, space) : stripped.TrimEnd(')');
    }
}
