// <copyright file="FormMeTTaBridge.Evaluation.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Truth value evaluation methods for the FormMeTTaBridge.
/// </summary>
public sealed partial class FormMeTTaBridge
{
    private Form EvaluateTruthValueInternal(Atom expression, ImmutableList<string>.Builder trace)
    {
        trace.Add($"Evaluating: {expression.ToSExpr()}");

        // Direct form atoms
        if (expression is FormAtom fa)
        {
            trace.Add($"Direct FormAtom: {fa.Form}");
            return fa.Form;
        }

        // Symbol to form conversion
        Option<Form> directForm = expression.ToForm();
        if (directForm.HasValue)
        {
            trace.Add($"Symbol to Form: {directForm.Value}");
            return directForm.Value;
        }

        // Expression evaluation
        if (expression is Expression expr && expr.Children.Count > 0)
        {
            if (expr.Children[0] is Symbol head)
            {
                return head.Name switch
                {
                    "cross" => EvaluateCross(expr, trace),
                    "call" => EvaluateCall(expr, trace),
                    "and" => EvaluateAnd(expr, trace),
                    "or" => EvaluateOr(expr, trace),
                    "not" => EvaluateNot(expr, trace),
                    "reentry" => EvaluateReentry(trace),
                    "implies" => EvaluateImplies(expr, trace),
                    _ => EvaluateGeneric(expr, trace)
                };
            }
        }

        // Query the interpreter
        List<Atom> results = _interpreter.Evaluate(expression).ToList();
        trace.Add($"Interpreter results: {results.Count}");

        if (results.Count == 0)
        {
            return Form.Void;
        }

        // If any result is marked, return Mark
        foreach (Atom result in results)
        {
            Option<Form> resultForm = result.ToForm();
            if (resultForm.HasValue && resultForm.Value.IsMarked())
            {
                return Form.Mark;
            }
        }

        return Form.Void;
    }

    private Form EvaluateCross(Expression expr, ImmutableList<string>.Builder trace)
    {
        if (expr.Children.Count < 2)
        {
            trace.Add("Cross with no argument = Mark");
            return Form.Mark;
        }

        Form innerForm = EvaluateTruthValueInternal(expr.Children[1], trace);
        Form result = innerForm.Not();
        trace.Add($"Cross({innerForm}) = {result}");
        return result;
    }

    private Form EvaluateCall(Expression expr, ImmutableList<string>.Builder trace)
    {
        if (expr.Children.Count < 3)
        {
            trace.Add("Call with insufficient arguments = Void");
            return Form.Void;
        }

        Form left = EvaluateTruthValueInternal(expr.Children[1], trace);
        Form right = EvaluateTruthValueInternal(expr.Children[2], trace);
        Form result = left.Call(right);
        trace.Add($"Call({left}, {right}) = {result}");
        return result;
    }

    private Form EvaluateAnd(Expression expr, ImmutableList<string>.Builder trace)
    {
        if (expr.Children.Count < 3)
        {
            return Form.Void;
        }

        Form left = EvaluateTruthValueInternal(expr.Children[1], trace);
        Form right = EvaluateTruthValueInternal(expr.Children[2], trace);
        Form result = left.And(right);
        trace.Add($"And({left}, {right}) = {result}");
        return result;
    }

    private Form EvaluateOr(Expression expr, ImmutableList<string>.Builder trace)
    {
        if (expr.Children.Count < 3)
        {
            return Form.Void;
        }

        Form left = EvaluateTruthValueInternal(expr.Children[1], trace);
        Form right = EvaluateTruthValueInternal(expr.Children[2], trace);
        Form result = left.Or(right);
        trace.Add($"Or({left}, {right}) = {result}");
        return result;
    }

    private Form EvaluateNot(Expression expr, ImmutableList<string>.Builder trace)
    {
        if (expr.Children.Count < 2)
        {
            return Form.Mark;
        }

        Form inner = EvaluateTruthValueInternal(expr.Children[1], trace);
        Form result = inner.Not();
        trace.Add($"Not({inner}) = {result}");
        return result;
    }

    private Form EvaluateReentry(ImmutableList<string>.Builder trace)
    {
        trace.Add("ReEntry = Imaginary");
        return Form.Imaginary;
    }

    private Form EvaluateImplies(Expression expr, ImmutableList<string>.Builder trace)
    {
        if (expr.Children.Count < 3)
        {
            return Form.Void;
        }

        Form condition = EvaluateTruthValueInternal(expr.Children[1], trace);

        // Short-circuit: false antecedent makes implication vacuously true
        if (condition.IsVoid())
        {
            trace.Add("Implies: condition is Void, vacuously true");
            return Form.Mark;
        }

        // Imaginary condition propagates uncertainty
        if (condition.IsImaginary())
        {
            trace.Add("Implies: condition is Imaginary, result uncertain");
            return Form.Imaginary;
        }

        Form conclusion = EvaluateTruthValueInternal(expr.Children[2], trace);
        trace.Add($"Implies({condition}, {conclusion}) = {conclusion}");
        return conclusion;
    }

    private Form EvaluateGeneric(Expression expr, ImmutableList<string>.Builder trace)
    {
        // Generic evaluation: check if expression matches anything
        List<(Atom Atom, Substitution Bindings)> matches = _space.Query(expr).ToList();
        trace.Add($"Generic query found {matches.Count} matches");
        return matches.Count > 0 ? Form.Mark : Form.Void;
    }

    private Form ComputeBindingCertainty(Substitution bindings)
    {
        if (bindings.IsEmpty)
        {
            return Form.Mark; // Ground match is certain
        }

        // Check if any binding contains variables (uncertain)
        foreach (KeyValuePair<string, Atom> kvp in bindings.Bindings)
        {
            if (kvp.Value.ContainsVariables())
            {
                return Form.Imaginary; // Still has unknowns
            }
        }

        return Form.Mark; // All bindings are ground
    }

    private void OnDistinctionChanged(DistinctionEventArgs e)
        => DistinctionChanged?.Invoke(this, e);

    private void OnTruthValueEvaluated(TruthValueEventArgs e)
        => TruthValueEvaluated?.Invoke(this, e);

    private void OnMetaReasoningPerformed(MetaReasoningEventArgs e)
        => MetaReasoningPerformed?.Invoke(this, e);
}
