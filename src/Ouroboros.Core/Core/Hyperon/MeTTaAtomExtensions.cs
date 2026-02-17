using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Extension methods for creating MeTTa-compliant atoms.
/// </summary>
public static class MeTTaAtomExtensions
{
    /// <summary>
    /// Annotates an atom with a type.
    /// </summary>
    /// <param name="atom">The atom to annotate.</param>
    /// <param name="type">The type.</param>
    /// <returns>Type annotation expression.</returns>
    public static Expression WithType(this Atom atom, Atom type)
        => MeTTaSpec.TypeOf(atom, type);

    /// <summary>
    /// Creates a function type from this atom as input.
    /// </summary>
    /// <param name="inputType">The input type.</param>
    /// <param name="outputType">The output type.</param>
    /// <returns>Function type expression.</returns>
    public static Expression To(this Atom inputType, Atom outputType)
        => MeTTaSpec.FunctionType(inputType, outputType);

    /// <summary>
    /// Quotes an atom to prevent evaluation.
    /// </summary>
    /// <param name="atom">The atom to quote.</param>
    /// <returns>Quoted atom.</returns>
    public static Expression Quoted(this Atom atom)
        => MeTTaSpec.Quote(atom);

    /// <summary>
    /// Creates an implication from this atom as the condition.
    /// </summary>
    /// <param name="condition">The condition.</param>
    /// <param name="conclusion">The conclusion.</param>
    /// <returns>Implication expression.</returns>
    public static Expression ImpliesThat(this Atom condition, Atom conclusion)
        => MeTTaSpec.Implies(condition, conclusion);

    /// <summary>
    /// Converts a Form to a MeTTa atom representing its state.
    /// </summary>
    /// <param name="form">The form to convert.</param>
    /// <returns>MeTTa atom representing the form.</returns>
    public static Atom ToMeTTa(this Form form)
        => form.Match(
            onMark: () => Atom.Sym("Mark"),
            onVoid: () => Atom.Sym("Void"),
            onImaginary: () => Atom.Sym("Imaginary"));

    /// <summary>
    /// Converts a MeTTa atom to a Form.
    /// </summary>
    /// <param name="atom">The atom to convert.</param>
    /// <returns>Option containing the form if conversion is valid.</returns>
    public static Option<Form> ToForm(this Atom atom)
    {
        if (atom is Symbol sym)
        {
            return sym.Name switch
            {
                "Mark" or "True" or "⌐" => Option<Form>.Some(Form.Mark),
                "Void" or "False" or "∅" => Option<Form>.Some(Form.Void),
                "Imaginary" or "i" => Option<Form>.Some(Form.Imaginary),
                _ => Option<Form>.None()
            };
        }

        return Option<Form>.None();
    }
}