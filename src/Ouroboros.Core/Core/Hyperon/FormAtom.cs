// <copyright file="FormAtom.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Represents a Laws of Form distinction as a MeTTa atom.
/// This creates a grounded type that embeds LoF reasoning into Hyperon.
/// </summary>
public sealed record FormAtom : Atom
{
    /// <summary>
    /// Gets the underlying Laws of Form value.
    /// </summary>
    public Form Form { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FormAtom"/> class.
    /// </summary>
    /// <param name="form">The Laws of Form value.</param>
    public FormAtom(Form form)
    {
        Form = form;
    }

    /// <summary>
    /// Creates a FormAtom from Mark (distinction).
    /// </summary>
    public static FormAtom Mark => new(Form.Mark);

    /// <summary>
    /// Creates a FormAtom from Void (no distinction).
    /// </summary>
    public static FormAtom Void => new(Form.Void);

    /// <summary>
    /// Creates a FormAtom from Imaginary (re-entrant).
    /// </summary>
    public static FormAtom Imaginary => new(Form.Imaginary);

    /// <inheritdoc/>
    public override string ToSExpr()
        => Form.Match(
            onMark: () => "⌐",
            onVoid: () => "∅",
            onImaginary: () => "ℑ");

    /// <inheritdoc/>
    public override bool ContainsVariables() => false;

    /// <summary>
    /// Applies the Law of Crossing (negation).
    /// </summary>
    /// <returns>The crossed form.</returns>
    public FormAtom Cross() => new(Form.Not());

    /// <summary>
    /// Applies the Law of Calling (idempotent operation).
    /// </summary>
    /// <param name="other">The form to call with.</param>
    /// <returns>The result of calling.</returns>
    public FormAtom Call(FormAtom other) => new(Form.Call(other.Form));

    /// <summary>
    /// Conjunction with another form.
    /// </summary>
    /// <param name="other">The other form.</param>
    /// <returns>The conjunction result.</returns>
    public FormAtom And(FormAtom other) => new(Form.And(other.Form));

    /// <summary>
    /// Disjunction with another form.
    /// </summary>
    /// <param name="other">The other form.</param>
    /// <returns>The disjunction result.</returns>
    public FormAtom Or(FormAtom other) => new(Form.Or(other.Form));

    /// <summary>
    /// Evaluates the form through Laws of Form reduction rules.
    /// </summary>
    /// <returns>The evaluated form.</returns>
    public FormAtom Eval() => new(Form.Eval());
}