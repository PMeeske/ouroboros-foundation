using System.Runtime.CompilerServices;

namespace Ouroboros.Core.Steps;

/// <summary>
/// Synchronous computation step that transforms input of type TIn to output of type TOut.
/// Provides a bridge between pure functional operations and our async Step system.
/// </summary>
public readonly struct SyncStep<TIn, TOut> : IEquatable<SyncStep<TIn, TOut>>
{
    private readonly Func<TIn, TOut> _f;

    public SyncStep(Func<TIn, TOut> f)
        => _f = f ?? throw new ArgumentNullException(nameof(f));

    /// <summary>
    /// Execute the synchronous step
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TOut Invoke(TIn input) => _f(input);

    /// <summary>
    /// Convert to async Step
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Step<TIn, TOut> ToAsync()
    {
        Func<TIn, TOut> func = _f; // Capture to avoid struct 'this' issues
        return input => Task.FromResult(func(input));
    }

    /// <summary>
    /// Pipe composition (heterogeneous) - synchronous version
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SyncStep<TIn, TNext> Pipe<TNext>(SyncStep<TOut, TNext> next)
    {
        Func<TIn, TOut> func = _f; // Capture to avoid struct 'this' issues
        return new(input => next.Invoke(func(input)));
    }

    /// <summary>
    /// Pipe with async step
    /// </summary>
    public Step<TIn, TNext> Pipe<TNext>(Step<TOut, TNext> asyncNext)
    {
        Func<TIn, TOut> func = _f; // Capture to avoid struct 'this' issues
        return async input => await asyncNext(func(input));
    }

    /// <summary>
    /// Functor/Map operation
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SyncStep<TIn, TNext> Map<TNext>(Func<TOut, TNext> map)
    {
        Func<TIn, TOut> func = _f; // Capture to avoid struct 'this' issues
        return new(input => map(func(input)));
    }

    /// <summary>
    /// Bind operation for monadic composition
    /// </summary>
    public SyncStep<TIn, TNext> Bind<TNext>(Func<TOut, SyncStep<TIn, TNext>> binder)
    {
        Func<TIn, TOut> func = _f; // Capture to avoid struct 'this' issues
        return new(input =>
        {
            TOut? intermediate = func(input);
            SyncStep<TIn, TNext> nextStep = binder(intermediate);
            return nextStep.Invoke(input);
        });
    }

    /// <summary>
    /// Equality (by delegate reference)
    /// </summary>
    public bool Equals(SyncStep<TIn, TOut> other) => ReferenceEquals(_f, other._f);
    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is SyncStep<TIn, TOut> o && Equals(o);
    /// <inheritdoc/>
    public override int GetHashCode() => _f?.GetHashCode() ?? 0;

    /// <summary>
    /// Implicit conversion from function
    /// </summary>
    public static implicit operator SyncStep<TIn, TOut>(Func<TIn, TOut> f) => new(f);

    /// <summary>
    /// Implicit conversion to async Step
    /// </summary>
    public static implicit operator Step<TIn, TOut>(SyncStep<TIn, TOut> syncStep) => syncStep.ToAsync();

    /// <summary>
    /// Static helper for identity step
    /// </summary>
    public static SyncStep<TIn, TIn> Identity => new(x => x);
}