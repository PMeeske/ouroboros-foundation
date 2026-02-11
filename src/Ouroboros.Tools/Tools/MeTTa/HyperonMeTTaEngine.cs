// <copyright file="HyperonMeTTaEngine.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

#pragma warning disable SA1101 // Prefix local calls with this

namespace Ouroboros.Tools.MeTTa;

using System.Collections.Concurrent;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.Hyperon.Parsing;

/// <summary>
/// Native C# Hyperon-based MeTTa engine implementation.
/// Uses the in-process AtomSpace and Interpreter for high-performance symbolic reasoning.
/// </summary>
public sealed class HyperonMeTTaEngine : IMeTTaEngine, IDisposable
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
            Core.Monads.Result<Atom> parseResult = parser.Parse(query);
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
        catch (Exception ex)
        {
            return Task.FromResult(Result<string, string>.Failure($"Evaluation error: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public Task<Result<MeTTaUnit, string>> AddFactAsync(string fact, CancellationToken ct = default)
    {
        if (disposed)
        {
            return Task.FromResult(Result<MeTTaUnit, string>.Failure("Engine disposed"));
        }

        try
        {
            Core.Monads.Result<Atom> parseResult = parser.Parse(fact);
            if (!parseResult.IsSuccess)
            {
                return Task.FromResult(Result<MeTTaUnit, string>.Failure($"Parse error: {parseResult.Error}"));
            }

            AddAtom(parseResult.Value);
            return Task.FromResult(Result<MeTTaUnit, string>.Success(MeTTaUnit.Value));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<MeTTaUnit, string>.Failure($"Add fact error: {ex.Message}"));
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
            Core.Monads.Result<Atom> parseResult = parser.Parse(rule);
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
        catch (Exception ex)
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
            Core.Monads.Result<Atom> parseResult = parser.Parse(plan);
            if (!parseResult.IsSuccess)
            {
                return Task.FromResult(Result<bool, string>.Success(false));
            }

            // Check if plan can be evaluated
            List<Atom> results = interpreter.Evaluate(parseResult.Value).ToList();
            bool isValid = results.Count > 0 || parseResult.IsSuccess;

            return Task.FromResult(Result<bool, string>.Success(isValid));
        }
        catch (Exception ex)
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
                Core.Monads.Result<Atom> parseResult = parser.Parse(step);
                if (!parseResult.IsSuccess)
                {
                    return Task.FromResult(Result<bool, string>.Success(false));
                }
            }

            return Task.FromResult(Result<bool, string>.Success(true));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<bool, string>.Failure($"Verification error: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public Task<Result<MeTTaUnit, string>> ResetAsync(CancellationToken ct = default)
    {
        if (disposed)
        {
            return Task.FromResult(Result<MeTTaUnit, string>.Failure("Engine disposed"));
        }

        try
        {
            space.Clear();
            namedAtoms.Clear();
            InitializeCoreAtoms();
            return Task.FromResult(Result<MeTTaUnit, string>.Success(MeTTaUnit.Value));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<MeTTaUnit, string>.Failure($"Reset error: {ex.Message}"));
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
    public async Task<Result<MeTTaUnit, string>> LoadMeTTaSourceAsync(string mettaSource, CancellationToken ct = default)
    {
        if (disposed)
        {
            return Result<MeTTaUnit, string>.Failure("Engine disposed");
        }

        string[] lines = mettaSource.Split('\n');
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";"))
            {
                continue; // Skip empty lines and comments
            }

            Result<MeTTaUnit, string> result = await AddFactAsync(trimmed, ct);
            if (!result.IsSuccess)
            {
                return result;
            }
        }

        return Result<MeTTaUnit, string>.Success(MeTTaUnit.Value);
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

    private static GroundedRegistry CreateDefaultGroundedOps()
    {
        GroundedRegistry registry = new();

        // Basic arithmetic
        registry.Register("+", (space, args) =>
        {
            if (args.Children.Count >= 2 &&
                double.TryParse(args.Children[0].ToSExpr(), out double a) &&
                double.TryParse(args.Children[1].ToSExpr(), out double b))
            {
                return new[] { Atom.Sym((a + b).ToString()) };
            }

            return Enumerable.Empty<Atom>();
        });

        registry.Register("-", (space, args) =>
        {
            if (args.Children.Count >= 2 &&
                double.TryParse(args.Children[0].ToSExpr(), out double a) &&
                double.TryParse(args.Children[1].ToSExpr(), out double b))
            {
                return new[] { Atom.Sym((a - b).ToString()) };
            }

            return Enumerable.Empty<Atom>();
        });

        registry.Register("*", (space, args) =>
        {
            if (args.Children.Count >= 2 &&
                double.TryParse(args.Children[0].ToSExpr(), out double a) &&
                double.TryParse(args.Children[1].ToSExpr(), out double b))
            {
                return new[] { Atom.Sym((a * b).ToString()) };
            }

            return Enumerable.Empty<Atom>();
        });

        registry.Register("/", (space, args) =>
        {
            if (args.Children.Count >= 2 &&
                double.TryParse(args.Children[0].ToSExpr(), out double a) &&
                double.TryParse(args.Children[1].ToSExpr(), out double b) &&
                b != 0)
            {
                return new[] { Atom.Sym((a / b).ToString()) };
            }

            return Enumerable.Empty<Atom>();
        });

        // Comparisons
        registry.Register("==", (space, args) =>
        {
            if (args.Children.Count >= 2)
            {
                bool equal = args.Children[0].ToSExpr() == args.Children[1].ToSExpr();
                return new[] { Atom.Sym(equal ? "True" : "False") };
            }

            return Enumerable.Empty<Atom>();
        });

        registry.Register("!=", (space, args) =>
        {
            if (args.Children.Count >= 2)
            {
                bool notEqual = args.Children[0].ToSExpr() != args.Children[1].ToSExpr();
                return new[] { Atom.Sym(notEqual ? "True" : "False") };
            }

            return Enumerable.Empty<Atom>();
        });

        // Logic
        registry.Register("and-all", (space, args) =>
        {
            bool result = args.Children.All(a => a.ToSExpr() == "True");
            return new[] { Atom.Sym(result ? "True" : "False") };
        });

        registry.Register("or-any", (space, args) =>
        {
            bool result = args.Children.Any(a => a.ToSExpr() == "True");
            return new[] { Atom.Sym(result ? "True" : "False") };
        });

        registry.Register("negate", (space, args) =>
        {
            if (args.Children.Count >= 1)
            {
                bool result = args.Children[0].ToSExpr() != "True";
                return new[] { Atom.Sym(result ? "True" : "False") };
            }

            return Enumerable.Empty<Atom>();
        });

        // String operations
        registry.Register("concat-str", (space, args) =>
        {
            string result = string.Concat(args.Children.Select(a => a.ToSExpr().Trim('"')));
            return new[] { Atom.Sym($"\"{result}\"") };
        });

        // List operations
        registry.Register("cons", (space, args) =>
        {
            if (args.Children.Count >= 2)
            {
                return new[] { Atom.Expr(args.Children.ToArray()) };
            }

            return Enumerable.Empty<Atom>();
        });

        registry.Register("car", (space, args) =>
        {
            if (args.Children.Count >= 1 && args.Children[0] is Expression expr && expr.Children.Count > 0)
            {
                return new[] { expr.Children[0] };
            }

            return Enumerable.Empty<Atom>();
        });

        registry.Register("cdr", (space, args) =>
        {
            if (args.Children.Count >= 1 && args.Children[0] is Expression expr && expr.Children.Count > 1)
            {
                return new[] { Atom.Expr(expr.Children.Skip(1).ToArray()) };
            }

            return Enumerable.Empty<Atom>();
        });

        // Identity
        registry.Register("identity", (space, args) =>
        {
            return args.Children.Count >= 1 ? new[] { args.Children[0] } : Enumerable.Empty<Atom>();
        });

        // Print (for debugging)
        registry.Register("println", (space, args) =>
        {
            string output = string.Join(" ", args.Children.Select(a => a.ToSExpr()));
            Console.WriteLine($"[MeTTa] {output}");
            return new[] { Atom.Sym("()") };
        });

        return registry;
    }

    private void InitializeCoreAtoms()
    {
        // Core type atoms
        AddAtom(Atom.Sym("Type"));
        AddAtom(Atom.Sym("Atom"));
        AddAtom(Atom.Sym("Symbol"));
        AddAtom(Atom.Sym("Variable"));
        AddAtom(Atom.Sym("Expression"));

        // Boolean constants
        AddAtom(Atom.Sym("True"));
        AddAtom(Atom.Sym("False"));

        // Type declarations
        AddAtom(Atom.Expr(Atom.Sym(":"), Atom.Sym("True"), Atom.Sym("Bool")));
        AddAtom(Atom.Expr(Atom.Sym(":"), Atom.Sym("False"), Atom.Sym("Bool")));

        // Function type constructor
        AddAtom(Atom.Expr(Atom.Sym(":"), Atom.Sym("->"), Atom.Expr(Atom.Sym("->"), Atom.Sym("Type"), Atom.Sym("Type"), Atom.Sym("Type"))));

        // Basic inference rule
        AddAtom(Atom.Expr(
            Atom.Sym("="),
            Atom.Expr(Atom.Sym("if"), Atom.Sym("True"), Atom.Var("then"), Atom.Var("else")),
            Atom.Var("then")));
        AddAtom(Atom.Expr(
            Atom.Sym("="),
            Atom.Expr(Atom.Sym("if"), Atom.Sym("False"), Atom.Var("then"), Atom.Var("else")),
            Atom.Var("else")));
    }
}
