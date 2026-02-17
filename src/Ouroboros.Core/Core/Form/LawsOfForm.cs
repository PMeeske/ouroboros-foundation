// <copyright file="LawsOfForm.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

namespace Ouroboros.Core.SpencerBrown;

/// <summary>
/// Implementation of George Spencer-Brown's Laws of Form in a monadic context.
/// The calculus of indications provides a foundational algebra for distinction and self-reference.
/// </summary>
/// <remarks>
/// <para>The two fundamental axioms:</para>
/// <list type="bullet">
///   <item><description>Law of Calling: ⊢⊢ = ⊢ (The form of condensation - idempotence)</description></item>
///   <item><description>Law of Crossing: ⊢⊢ = ∅ (The form of cancellation - involution)</description></item>
/// </list>
/// <para>These map to monadic laws as follows:</para>
/// <list type="bullet">
///   <item><description>Mark (⊢) ≅ Unit/Return (η): entering the marked state</description></item>
///   <item><description>Cross ≅ Join/Flatten (μ): collapsing nested distinctions</description></item>
///   <item><description>The unmarked state ≅ Identity morphism</description></item>
/// </list>
/// </remarks>
public static class LawsOfForm
{
    /// <summary>
    /// The Mark (⊢) - Creates a distinction, entering the marked state.
    /// This is the fundamental act of indication in Spencer-Brown's calculus.
    /// Corresponds to monadic unit/return (η).
    /// </summary>
    /// <typeparam name="T">The type being marked.</typeparam>
    /// <param name="value">The value to mark.</param>
    /// <returns>A marked form containing the value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Form<T> Mark<T>(T value) => Form<T>.Mark(value);

    /// <summary>
    /// The Void (∅) - The unmarked state, representing undifferentiated potential.
    /// </summary>
    /// <typeparam name="T">The phantom type.</typeparam>
    /// <returns>An unmarked/void form.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Form<T> Void<T>() => Form<T>.Void();

    /// <summary>
    /// Cross (⊢→) - Crosses the boundary of a form.
    /// Entering a marked space from the unmarked, or vice versa.
    /// </summary>
    /// <typeparam name="T">The type being crossed.</typeparam>
    /// <param name="form">The form to cross.</param>
    /// <returns>The crossed form.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Form<T> Cross<T>(Form<T> form) => form.Cross();

    /// <summary>
    /// Law of Calling (Condensation): ⊢⊢ = ⊢
    /// Calling from a marked state to a marked state remains marked.
    /// This expresses idempotence: marking what is already marked changes nothing.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="form">The form to condense.</param>
    /// <returns>The condensed form.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Form<T> Call<T>(Form<T> form) => form.Call();

    /// <summary>
    /// Law of Crossing (Cancellation): ⊢⊢ = ∅
    /// Two crossings return to the unmarked state.
    /// This expresses involution: distinction of distinction yields void.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="form">The form to apply double crossing.</param>
    /// <returns>The result after double crossing.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Form<T> Recross<T>(Form<T> form) => form.Recross();

    /// <summary>
    /// Creates a Step that applies the Mark operation.
    /// </summary>
    /// <typeparam name="T">The type to mark.</typeparam>
    /// <returns>A step that marks its input.</returns>
    public static Step<T, Form<T>> MarkArrow<T>() =>
        input => Task.FromResult(Form<T>.Mark(input));

    /// <summary>
    /// Creates a Step that applies the Cross operation.
    /// </summary>
    /// <typeparam name="T">The type being crossed.</typeparam>
    /// <returns>A step that crosses its input form.</returns>
    public static Step<Form<T>, Form<T>> CrossArrow<T>() =>
        input => Task.FromResult(input.Cross());

    /// <summary>
    /// The Cross Product (×) - Combines two forms into a product form.
    /// In category theory, this is the categorical product in the category of forms.
    /// </summary>
    /// <typeparam name="T1">The first type.</typeparam>
    /// <typeparam name="T2">The second type.</typeparam>
    /// <param name="form1">The first form.</param>
    /// <param name="form2">The second form.</param>
    /// <returns>The product form.</returns>
    public static Form<(T1, T2)> Product<T1, T2>(Form<T1> form1, Form<T2> form2)
    {
        return (form1.IsMarked, form2.IsMarked) switch
        {
            (true, true) => Form<(T1, T2)>.Mark((form1.Value!, form2.Value!)),
            _ => Form<(T1, T2)>.Void()
        };
    }

    /// <summary>
    /// Creates a parallel cross product arrow that runs two steps and combines results.
    /// This is the bifunctor action in the category of forms.
    /// </summary>
    /// <typeparam name="TIn">The shared input type.</typeparam>
    /// <typeparam name="T1">The first output type.</typeparam>
    /// <typeparam name="T2">The second output type.</typeparam>
    /// <param name="step1">The first step.</param>
    /// <param name="step2">The second step.</param>
    /// <returns>A step producing the product of both results.</returns>
    public static Step<TIn, Form<(T1, T2)>> CrossProduct<TIn, T1, T2>(
        Step<TIn, Form<T1>> step1,
        Step<TIn, Form<T2>> step2) =>
        async input =>
        {
            var task1 = step1(input);
            var task2 = step2(input);
            await Task.WhenAll(task1, task2).ConfigureAwait(false);
            return Product(await task1, await task2);
        };

    /// <summary>
    /// Re-entry operator (⟲) - Creates self-referential forms.
    /// This models the imaginary value that emerges from self-distinction.
    /// f = ⊢f (the form that equals its own mark)
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="generator">Function that generates from the re-entrant value.</param>
    /// <returns>A form representing the fixed point.</returns>
    public static Form<T> ReEntry<T>(Func<Form<T>, Form<T>> generator)
    {
        // The imaginary state: neither marked nor unmarked, but oscillating
        // We approximate with a lazy thunk that represents the self-reference
        Form<T> seed = Form<T>.Void();
        return generator(seed);
    }

    /// <summary>
    /// Creates a re-entrant step that feeds its output back as input.
    /// Models the temporal dimension of Spencer-Brown's calculus.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="iterations">Number of re-entry cycles.</param>
    /// <param name="step">The step to iterate.</param>
    /// <returns>A step that performs re-entry iteration.</returns>
    public static Step<Form<T>, Form<T>> ReEntryArrow<T>(int iterations, Step<Form<T>, Form<T>> step)
    {
        return async input =>
        {
            Form<T> current = input;
            for (int i = 0; i < iterations; i++)
            {
                current = await step(current).ConfigureAwait(false);
            }
            return current;
        };
    }
}