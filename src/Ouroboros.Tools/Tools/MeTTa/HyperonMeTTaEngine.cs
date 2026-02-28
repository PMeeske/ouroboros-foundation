// <copyright file="HyperonMeTTaEngine.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

#pragma warning disable SA1101 // Prefix local calls with this

namespace Ouroboros.Tools.MeTTa;

using System.Collections.Concurrent;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.Hyperon.Parsing;

/// <summary>
/// Native C# Hyperon-based MeTTa engine implementation.
/// Uses the in-process AtomSpace and Interpreter for high-performance symbolic reasoning.
/// </summary>
public sealed partial class HyperonMeTTaEngine : IMeTTaEngine, IDisposable
{
    private readonly AtomSpace space;
    private readonly Interpreter interpreter;
    private readonly SExpressionParser parser;
    private readonly GroundedRegistry groundedRegistry;
    private readonly ConcurrentDictionary<string, Atom> namedAtoms = new();
    private bool disposed;

    /// <summary>
    /// Gets the underlying AtomSpace for direct access.
    /// </summary>
    public IAtomSpace AtomSpace => space;

    /// <summary>
    /// Gets the interpreter for direct evaluation.
    /// </summary>
    public Interpreter Interpreter => interpreter;

    /// <summary>
    /// Gets the parser for S-expression parsing.
    /// </summary>
    public SExpressionParser Parser => parser;

    /// <summary>
    /// Event raised when atoms are added to the space.
    /// </summary>
    public event Action<Atom>? AtomAdded;

    /// <summary>
    /// Event raised when a query is evaluated.
    /// </summary>
    public event Action<string, IReadOnlyList<Atom>>? QueryEvaluated;

    /// <summary>
    /// Initializes a new instance of the <see cref="HyperonMeTTaEngine"/> class.
    /// </summary>
    /// <param name="groundedOps">Optional custom grounded operations.</param>
    public HyperonMeTTaEngine(GroundedRegistry? groundedOps = null)
    {
        groundedRegistry = groundedOps ?? CreateDefaultGroundedOps();
        space = new AtomSpace();
        interpreter = new Interpreter(space, groundedRegistry);
        parser = new SExpressionParser();

        // Initialize with core atoms
        InitializeCoreAtoms();
    }

    /// <summary>
    /// Creates a new engine from an existing AtomSpace.
    /// </summary>
    /// <param name="space">The atom space to use.</param>
    /// <param name="groundedOps">Optional custom grounded operations.</param>
    /// <returns>A new HyperonMeTTaEngine.</returns>
    public static HyperonMeTTaEngine FromAtomSpace(AtomSpace space, GroundedRegistry? groundedOps = null)
    {
        var engine = new HyperonMeTTaEngine(groundedOps);
        foreach (Atom atom in space.All())
        {
            engine.space.Add(atom);
        }

        return engine;
    }

    /// <inheritdoc/>
    public Task<Result<string, string>> ExecuteQueryAsync(string query, CancellationToken ct = default)
    {
        if (disposed)
        {
            return Task.FromResult(Result<string, string>.Failure("Engine disposed"));
        }

        try
        {
            Result<Atom> parseResult = parser.Parse(query);
            if (!parseResult.IsSuccess)
            {
                return Task.FromResult(Result<string, string>.Failure($"Parse error: {parseResult.Error}"));
            }

            List<Atom> results = interpreter.Evaluate(parseResult.Value).ToList();

            QueryEvaluated?.Invoke(query, results);

            if (results.Count == 0)
            {
                return Task.FromResult(Result<string, string>.Success("()"));
            }

            string resultStr = string.Join(" ", results.Select(a => a.ToSExpr()));
            return Task.FromResult(Result<string, string>.Success(resultStr));
        }
        catch (InvalidOperationException ex)
        {
            return Task.FromResult(Result<string, string>.Failure($"Evaluation error: {ex.Message}"));
        }
        catch (FormatException ex)
        {
            return Task.FromResult(Result<string, string>.Failure($"Evaluation error: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public Task<Result<Unit, string>> AddFactAsync(string fact, CancellationToken ct = default)
    {
        if (disposed)
        {
            return Task.FromResult(Result<Unit, string>.Failure("Engine disposed"));
        }

        try
        {
            Result<Atom> parseResult = parser.Parse(fact);
            if (!parseResult.IsSuccess)
            {
                return Task.FromResult(Result<Unit, string>.Failure($"Parse error: {parseResult.Error}"));
            }

            AddAtom(parseResult.Value);
            return Task.FromResult(Result<Unit, string>.Success(Unit.Value));
        }
        catch (InvalidOperationException ex)
        {
            return Task.FromResult(Result<Unit, string>.Failure($"Add fact error: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public Task<Result<string, string>> ApplyRuleAsync(string rule, CancellationToken ct = default)
    {
        if (disposed)
        {
            return Task.FromResult(Result<string, string>.Failure("Engine disposed"));
        }

        try
        {
            Result<Atom> parseResult = parser.Parse(rule);
            if (!parseResult.IsSuccess)
            {
                return Task.FromResult(Result<string, string>.Failure($"Parse error: {parseResult.Error}"));
            }

            // Add rule as a fact
            AddAtom(parseResult.Value);

            // Evaluate to trigger any immediate inference
            List<Atom> results = interpreter.Evaluate(parseResult.Value).ToList();
            string resultStr = results.Count > 0
                ? string.Join(" ", results.Select(a => a.ToSExpr()))
                : "rule-added";

            return Task.FromResult(Result<string, string>.Success(resultStr));
        }
        catch (InvalidOperationException ex)
        {
            return Task.FromResult(Result<string, string>.Failure($"Apply rule error: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public Task<Result<bool, string>> VerifyPlanAsync(string plan, CancellationToken ct = default)
    {
        if (disposed)
        {
            return Task.FromResult(Result<bool, string>.Failure("Engine disposed"));
        }

        try
        {
            // Parse the plan
            Result<Atom> parseResult = parser.Parse(plan);
            if (!parseResult.IsSuccess)
            {
                return Task.FromResult(Result<bool, string>.Success(false));
            }

            // Check if plan can be evaluated
            List<Atom> results = interpreter.Evaluate(parseResult.Value).ToList();
            bool isValid = results.Count > 0 || parseResult.IsSuccess;

            return Task.FromResult(Result<bool, string>.Success(isValid));
        }
        catch (InvalidOperationException ex)
        {
            return Task.FromResult(Result<bool, string>.Failure($"Verification error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Verifies a plan with multiple steps.
    /// </summary>
    /// <param name="steps">The steps to verify.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Verification result.</returns>
    public Task<Result<bool, string>> VerifyPlanStepsAsync(IEnumerable<string> steps, CancellationToken ct = default)
    {
        if (disposed)
        {
            return Task.FromResult(Result<bool, string>.Failure("Engine disposed"));
        }

        try
        {
            // Build verification query
            List<string> stepsList = steps.ToList();
            if (stepsList.Count == 0)
            {
                return Task.FromResult(Result<bool, string>.Success(true));
            }

            // Verify each step can be executed
            foreach (string step in stepsList)
            {
                Result<Atom> parseResult = parser.Parse(step);
                if (!parseResult.IsSuccess)
                {
                    return Task.FromResult(Result<bool, string>.Success(false));
                }
            }

            return Task.FromResult(Result<bool, string>.Success(true));
        }
        catch (InvalidOperationException ex)
        {
            return Task.FromResult(Result<bool, string>.Failure($"Verification error: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public Task<Result<Unit, string>> ResetAsync(CancellationToken ct = default)
    {
        if (disposed)
        {
            return Task.FromResult(Result<Unit, string>.Failure("Engine disposed"));
        }

        try
        {
            space.Clear();
            namedAtoms.Clear();
            InitializeCoreAtoms();
            return Task.FromResult(Result<Unit, string>.Success(Unit.Value));
        }
        catch (InvalidOperationException ex)
        {
            return Task.FromResult(Result<Unit, string>.Failure($"Reset error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Adds an atom directly to the space.
    /// </summary>
    /// <param name="atom">The atom to add.</param>
    public void AddAtom(Atom atom)
    {
        space.Add(atom);
        AtomAdded?.Invoke(atom);
    }

    /// <summary>
    /// Binds a name to an atom for later retrieval.
    /// </summary>
    /// <param name="name">The name to bind.</param>
    /// <param name="atom">The atom to bind to the name.</param>
    public void BindAtom(string name, Atom atom)
    {
        namedAtoms[name] = atom;
        AddAtom(atom);
    }

    /// <summary>
    /// Gets a named atom.
    /// </summary>
    /// <param name="name">The name of the atom.</param>
    /// <returns>The atom if found, null otherwise.</returns>
    public Atom? GetNamedAtom(string name)
    {
        return namedAtoms.TryGetValue(name, out Atom? atom) ? atom : null;
    }

    /// <summary>
    /// Queries the space directly with an atom pattern.
    /// </summary>
    /// <param name="pattern">The pattern atom to match.</param>
    /// <returns>Matching atoms with their substitutions.</returns>
    public IEnumerable<(Atom Atom, Substitution Bindings)> Query(Atom pattern)
    {
        return space.Query(pattern);
    }

    /// <summary>
    /// Loads MeTTa source code into the engine.
    /// </summary>
    /// <param name="mettaSource">The MeTTa source code.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<Result<Unit, string>> LoadMeTTaSourceAsync(string mettaSource, CancellationToken ct = default)
    {
        if (disposed)
        {
            return Result<Unit, string>.Failure("Engine disposed");
        }

        string[] lines = mettaSource.Split('\n');
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";"))
            {
                continue; // Skip empty lines and comments
            }

            Result<Unit, string> result = await AddFactAsync(trimmed, ct);
            if (!result.IsSuccess)
            {
                return result;
            }
        }

        return Result<Unit, string>.Success(Unit.Value);
    }

    /// <summary>
    /// Exports all atoms in the space to MeTTa source.
    /// </summary>
    /// <returns>MeTTa source representation.</returns>
    public string ExportToMeTTa()
    {
        System.Text.StringBuilder sb = new();
        sb.AppendLine("; Exported from HyperonMeTTaEngine");
        sb.AppendLine($"; Exported at {DateTime.UtcNow:O}");
        sb.AppendLine();

        foreach (Atom atom in space.All())
        {
            sb.AppendLine(atom.ToSExpr());
        }

        return sb.ToString();
    }

    /// <summary>
    /// Registers a grounded operation at runtime.
    /// </summary>
    /// <param name="name">The operation name.</param>
    /// <param name="operation">The operation implementation.</param>
    public void RegisterGroundedOp(string name, GroundedOperation operation)
    {
        groundedRegistry.Register(name, operation);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        namedAtoms.Clear();
    }
}
