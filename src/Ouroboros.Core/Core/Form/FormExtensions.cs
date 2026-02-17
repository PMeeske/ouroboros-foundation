using System.Runtime.CompilerServices;

namespace Ouroboros.Core.SpencerBrown;

/// <summary>
/// Extension methods for Form operations and Step composition.
/// </summary>
public static class FormExtensions
{
    /// <summary>
    /// Lifts a value into a marked form.
    /// </summary>
    public static Form<T> ToForm<T>(this T value) => Form<T>.Mark(value);

    /// <summary>
    /// Lifts an Option into a Form (isomorphism).
    /// </summary>
    public static Form<T> ToForm<T>(this Option<T> option) =>
        option.HasValue ? Form<T>.Mark(option.GetValueOrDefault(default!)) : Form<T>.Void();

    /// <summary>
    /// Converts a Form back to Option.
    /// </summary>
    public static Option<T> ToOption<T>(this Form<T> form) =>
        form.IsMarked && form.Value is not null ? Option<T>.Some(form.Value) : Option<T>.None();

    /// <summary>
    /// Creates a Step that marks successful results.
    /// </summary>
    public static Step<TIn, Form<TOut>> MarkStep<TIn, TOut>(this Step<TIn, TOut> step) =>
        async input =>
        {
            var result = await step(input).ConfigureAwait(false);
            return Form<TOut>.Mark(result);
        };

    /// <summary>
    /// Composes two form-producing steps with the cross product.
    /// </summary>
    public static Step<TIn, Form<(T1, T2)>> CrossWith<TIn, T1, T2>(
        this Step<TIn, Form<T1>> step1,
        Step<TIn, Form<T2>> step2) =>
        LawsOfForm.CrossProduct(step1, step2);

    /// <summary>
    /// Applies the Law of Calling to a form-producing step.
    /// </summary>
    public static Step<TIn, Form<TOut>> WithCalling<TIn, TOut>(this Step<TIn, Form<TOut>> step) =>
        async input =>
        {
            var result = await step(input).ConfigureAwait(false);
            return result.Call();
        };

    /// <summary>
    /// Applies the Law of Crossing to a form-producing step.
    /// </summary>
    public static Step<TIn, Form<TOut>> WithCrossing<TIn, TOut>(this Step<TIn, Form<TOut>> step) =>
        async input =>
        {
            var result = await step(input).ConfigureAwait(false);
            return result.Cross();
        };

    /// <summary>
    /// Parallel tuple awaiter for async cross product.
    /// </summary>
    public static TaskAwaiter<(T1, T2)> GetAwaiter<T1, T2>(this (Task<T1>, Task<T2>) tasks)
    {
        return WaitBoth(tasks).GetAwaiter();

        static async Task<(T1, T2)> WaitBoth((Task<T1>, Task<T2>) t)
        {
            await Task.WhenAll(t.Item1, t.Item2).ConfigureAwait(false);
            return (t.Item1.Result, t.Item2.Result);
        }
    }
}