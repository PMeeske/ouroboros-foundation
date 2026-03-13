// <copyright file="FormMeTTaBridge.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Bridge between Laws of Form distinctions and MeTTa/Hyperon symbolic reasoning.
/// Uses events to connect the two paradigms, enabling:
/// - Distinction-gated inference
/// - Form-based pattern matching
/// - Meta-level reasoning about certainty
/// - Self-referential loops via re-entry.
/// </summary>
public sealed partial class FormMeTTaBridge : IDisposable
{
    private readonly IAtomSpace _space;
    private readonly Interpreter _interpreter;
    private readonly ConcurrentDictionary<string, Form> _distinctionContext;
    private readonly ConcurrentDictionary<string, FormAtom> _formCache;
    private bool _disposed;

    /// <summary>
    /// Raised when a distinction is drawn, crossed, or otherwise modified.
    /// </summary>
    public event EventHandler<DistinctionEventArgs>? DistinctionChanged;

    /// <summary>
    /// Raised when a truth value is evaluated.
    /// </summary>
    public event EventHandler<TruthValueEventArgs>? TruthValueEvaluated;

    /// <summary>
    /// Raised when meta-level reasoning occurs.
    /// </summary>
    public event EventHandler<MetaReasoningEventArgs>? MetaReasoningPerformed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormMeTTaBridge"/> class.
    /// </summary>
    /// <param name="space">The atom space for MeTTa operations.</param>
    /// <param name="groundedOps">Optional custom grounded operations.</param>
    public FormMeTTaBridge(IAtomSpace space, GroundedRegistry? groundedOps = null)
    {
        _space = space ?? throw new ArgumentNullException(nameof(space));
        _distinctionContext = new ConcurrentDictionary<string, Form>();
        _formCache = new ConcurrentDictionary<string, FormAtom>();

        // Create registry with Laws of Form operations
        GroundedRegistry registry = groundedOps ?? GroundedRegistry.CreateStandard();
        RegisterFormOperations(registry);

        _interpreter = new Interpreter(space, registry);

        // Add Laws of Form axioms to the space
        InitializeFormAxioms();
    }

    /// <summary>
    /// Draws a distinction in the given context.
    /// </summary>
    /// <param name="context">The context name.</param>
    /// <param name="reason">Optional reason for the distinction.</param>
    /// <returns>The resulting form.</returns>
    public Form DrawDistinction(string context, Atom? reason = null)
    {
        Form previous = _distinctionContext.GetValueOrDefault(context, Form.Void);
        Form newForm = Form.Mark;

        _distinctionContext[context] = newForm;
        _formCache[context] = new FormAtom(newForm);

        // Add to atom space
        Expression distinctionAtom = Atom.Expr(
            Atom.Sym("Distinction"),
            Atom.Sym(context),
            Atom.Sym("Mark"));
        _space.Add(distinctionAtom);

        OnDistinctionChanged(new DistinctionEventArgs
        {
            EventType = DistinctionEventType.DistinctionDrawn,
            PreviousState = previous,
            CurrentState = newForm,
            TriggerAtom = reason,
            Context = context
        });

        return newForm;
    }

    /// <summary>
    /// Crosses (negates) a distinction in the given context.
    /// </summary>
    /// <param name="context">The context name.</param>
    /// <returns>The crossed form.</returns>
    public Form CrossDistinction(string context)
    {
        Form previous = _distinctionContext.GetValueOrDefault(context, Form.Void);
        Form newForm = previous.Not();

        _distinctionContext[context] = newForm;
        _formCache[context] = new FormAtom(newForm);

        // Determine if this is a cancellation (double crossing)
        DistinctionEventType eventType = newForm.IsVoid() && previous.IsMarked()
            ? DistinctionEventType.Cancelled
            : DistinctionEventType.Crossed;

        // Update atom space
        Expression distinctionAtom = Atom.Expr(
            Atom.Sym("Distinction"),
            Atom.Sym(context),
            newForm.ToMeTTa());
        _space.Add(distinctionAtom);

        OnDistinctionChanged(new DistinctionEventArgs
        {
            EventType = eventType,
            PreviousState = previous,
            CurrentState = newForm,
            Context = context
        });

        return newForm;
    }

    /// <summary>
    /// Creates a re-entry (self-referential form) in the given context.
    /// </summary>
    /// <param name="context">The context name.</param>
    /// <returns>The imaginary (re-entrant) form.</returns>
    public Form CreateReEntry(string context)
    {
        Form previous = _distinctionContext.GetValueOrDefault(context, Form.Void);
        Form newForm = Form.Imaginary;

        _distinctionContext[context] = newForm;
        _formCache[context] = new FormAtom(newForm);

        // Add re-entry to atom space
        Expression reentryAtom = Atom.Expr(
            Atom.Sym("ReEntry"),
            Atom.Sym(context),
            Atom.Sym("Imaginary"));
        _space.Add(reentryAtom);

        OnDistinctionChanged(new DistinctionEventArgs
        {
            EventType = DistinctionEventType.ReEntryCreated,
            PreviousState = previous,
            CurrentState = newForm,
            Context = context
        });

        return newForm;
    }

    /// <summary>
    /// Evaluates an atom's truth value using Laws of Form semantics.
    /// </summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <returns>The truth value as a Form.</returns>
    public Form EvaluateTruthValue(Atom expression)
    {
        ImmutableList<string>.Builder trace = ImmutableList.CreateBuilder<string>();
        Form result = EvaluateTruthValueInternal(expression, trace);

        OnTruthValueEvaluated(new TruthValueEventArgs
        {
            Expression = expression,
            TruthValue = result,
            ReasoningTrace = trace.ToImmutable()
        });

        return result;
    }

    /// <summary>
    /// Performs distinction-gated inference.
    /// Only performs inference if the guard distinction is marked.
    /// </summary>
    /// <param name="guard">The guard context name.</param>
    /// <param name="query">The query to execute if guard is marked.</param>
    /// <returns>Results if guard is marked, empty otherwise.</returns>
    public IEnumerable<Atom> DistinctionGatedInference(string guard, Atom query)
    {
        Form guardForm = _distinctionContext.GetValueOrDefault(guard, Form.Void);

        if (!guardForm.IsMarked())
        {
            yield break;
        }

        foreach (Atom result in _interpreter.Evaluate(query))
        {
            OnDistinctionChanged(new DistinctionEventArgs
            {
                EventType = DistinctionEventType.InferenceDerived,
                CurrentState = guardForm,
                TriggerAtom = query,
                Context = guard
            });

            yield return result;
        }
    }

    /// <summary>
    /// Matches a pattern and returns results with their form states.
    /// </summary>
    /// <param name="pattern">The pattern to match.</param>
    /// <returns>Matching results with associated form states.</returns>
    public IEnumerable<(Atom Result, Form State, Substitution Bindings)> FormGatedMatch(Atom pattern)
    {
        foreach ((Atom? result, Substitution? bindings) in _interpreter.EvaluateWithBindings(pattern))
        {
            // Compute the form state based on certainty of bindings
            Form form = ComputeBindingCertainty(bindings);

            OnDistinctionChanged(new DistinctionEventArgs
            {
                EventType = DistinctionEventType.PatternMatched,
                CurrentState = form,
                TriggerAtom = pattern
            });

            yield return (result, form, bindings);
        }
    }

    /// <summary>
    /// Performs meta-reasoning about an expression.
    /// Creates a quoted representation and reasons about its structure.
    /// </summary>
    /// <param name="expression">The expression to reason about.</param>
    /// <returns>Meta-level insights as atoms.</returns>
    public IEnumerable<Atom> MetaReason(Atom expression)
    {
        // Create meta-level representation
        Expression metaAtom = MeTTaSpec.Quote(expression);

        // Add meta-level facts about the expression
        List<Atom> metaFacts = new List<Atom>
        {
            Atom.Expr(Atom.Sym("is-expression"), metaAtom),
            Atom.Expr(Atom.Sym("has-variables"), Atom.Sym(expression.ContainsVariables() ? "True" : "False"))
        };

        if (expression is Expression expr && expr.Children.Count > 0)
        {
            metaFacts.Add(Atom.Expr(Atom.Sym("head"), metaAtom, expr.Children[0]));
            metaFacts.Add(Atom.Expr(Atom.Sym("arity"), metaAtom, Atom.Sym(expr.Children.Count.ToString())));
        }

        foreach (Atom fact in metaFacts)
        {
            _space.Add(fact);

            OnMetaReasoningPerformed(new MetaReasoningEventArgs
            {
                Operation = "analyze",
                ObjectLevel = expression,
                MetaLevel = fact
            });

            yield return fact;
        }
    }

    /// <summary>
    /// Gets the current form state for a context.
    /// </summary>
    /// <param name="context">The context name.</param>
    /// <returns>The current form, or Void if not set.</returns>
    public Form GetFormState(string context)
        => _distinctionContext.GetValueOrDefault(context, Form.Void);

    /// <summary>
    /// Gets all active distinctions.
    /// </summary>
    /// <returns>Dictionary of context names to forms.</returns>
    public IReadOnlyDictionary<string, Form> GetAllDistinctions()
        => _distinctionContext.ToImmutableDictionary();

    /// <summary>
    /// Clears a distinction context.
    /// </summary>
    /// <param name="context">The context to clear.</param>
    public void ClearDistinction(string context)
    {
        _distinctionContext.TryRemove(context, out _);
        _formCache.TryRemove(context, out _);
    }

    /// <summary>
    /// Gets the underlying interpreter.
    /// </summary>
    public Interpreter Interpreter => _interpreter;

    private void InitializeFormAxioms()
    {
        // Law of Crossing: (implies (cross (cross X)) X)
        _space.Add(MeTTaSpec.Implies(
            Atom.Expr(Atom.Sym("cross"), Atom.Expr(Atom.Sym("cross"), Atom.Var("X"))),
            Atom.Var("X")));

        // Law of Calling: (implies (call X X) X)
        _space.Add(MeTTaSpec.Implies(
            Atom.Expr(Atom.Sym("call"), Atom.Var("X"), Atom.Var("X")),
            Atom.Var("X")));

        // Mark crossed with Void equals Mark
        _space.Add(MeTTaSpec.Implies(
            Atom.Expr(Atom.Sym("cross"), Atom.Sym("Void")),
            Atom.Sym("Mark")));

        // Void is the identity for Call
        _space.Add(MeTTaSpec.Implies(
            Atom.Expr(Atom.Sym("call"), Atom.Var("X"), Atom.Sym("Void")),
            Atom.Var("X")));

        // Re-entry produces Imaginary
        _space.Add(MeTTaSpec.Implies(
            Atom.Expr(Atom.Sym("reentry"), Atom.Var("X")),
            Atom.Sym("Imaginary")));

        // Type declarations for Form values
        _space.Add(MeTTaSpec.TypeOf(Atom.Sym("Mark"), Atom.Sym("Form")));
        _space.Add(MeTTaSpec.TypeOf(Atom.Sym("Void"), Atom.Sym("Form")));
        _space.Add(MeTTaSpec.TypeOf(Atom.Sym("Imaginary"), Atom.Sym("Form")));
    }

    private static void RegisterFormOperations(GroundedRegistry registry)
    {
        // cross: Apply Law of Crossing (negation)
        registry.Register("cross", (space, args) =>
        {
            if (args.Children.Count < 2)
            {
                return Enumerable.Empty<Atom>();
            }

            Atom inner = args.Children[1];
            Option<Form> formOpt = inner.ToForm();

            if (formOpt.HasValue)
            {
                return new[] { formOpt.Value.Not().ToMeTTa() };
            }

            // Handle nested cross expressions
            if (inner is Expression expr &&
                expr.Children.Count > 0 &&
                expr.Children[0] is Symbol s &&
                s.Name == "cross" && expr.Children.Count > 1)
            {
                // Double crossing cancels: (cross (cross X)) = X
                return new[] { expr.Children[1] };
            }

            return new[] { FormExpression.Cross(inner) };
        });

        // call: Apply Law of Calling (indication)
        registry.Register("call", (space, args) =>
        {
            if (args.Children.Count < 3)
            {
                return Enumerable.Empty<Atom>();
            }

            Atom left = args.Children[1];
            Atom right = args.Children[2];

            Option<Form> leftForm = left.ToForm();
            Option<Form> rightForm = right.ToForm();

            if (leftForm.HasValue && rightForm.HasValue)
            {
                return new[] { leftForm.Value.Call(rightForm.Value).ToMeTTa() };
            }

            // Identity law: call X Void = X
            if (right is Symbol sym && sym.Name == "Void")
            {
                return new[] { left };
            }

            return new[] { FormExpression.Call(left, right) };
        });

        // reentry: Create self-referential form
        registry.Register("reentry", (_, args) =>
        {
            return new[] { Atom.Sym("Imaginary") };
        });

        // eval-form: Evaluate a form expression
        registry.Register("eval-form", (_, args) =>
        {
            if (args.Children.Count < 2)
            {
                return new[] { Atom.Sym("Void") };
            }

            Atom expr = args.Children[1];

            if (expr is FormExpression fe)
            {
                return new[] { fe.Evaluate() };
            }

            Option<Form> formOpt = expr.ToForm();
            return formOpt.HasValue
                ? new[] { formOpt.Value.Eval().ToMeTTa() }
                : new[] { Atom.Sym("Void") };
        });

        // is-marked: Check if a form is marked
        registry.Register("is-marked", (_, args) =>
        {
            if (args.Children.Count < 2)
            {
                return new[] { MeTTaSpec.False };
            }

            Option<Form> formOpt = args.Children[1].ToForm();
            return formOpt.HasValue && formOpt.Value.IsMarked()
                ? new[] { MeTTaSpec.True }
                : new[] { MeTTaSpec.False };
        });

        // is-certain: Check if a form is certain (not imaginary)
        registry.Register("is-certain", (_, args) =>
        {
            if (args.Children.Count < 2)
            {
                return new[] { MeTTaSpec.False };
            }

            Option<Form> formOpt = args.Children[1].ToForm();
            return formOpt.HasValue && formOpt.Value.IsCertain()
                ? new[] { MeTTaSpec.True }
                : new[] { MeTTaSpec.False };
        });
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _distinctionContext.Clear();
        _formCache.Clear();
        _disposed = true;
    }
}
