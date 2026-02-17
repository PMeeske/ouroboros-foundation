using System.Runtime.CompilerServices;

namespace Ouroboros.Core.Steps;

/// <summary>
/// Extensions for integrating SyncStep with our existing async system
/// </summary>
public static class SyncStepExtensions
{
    /// <summary>
    /// Lift a pure function to SyncStep
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SyncStep<TIn, TOut> ToSyncStep<TIn, TOut>(this Func<TIn, TOut> func)
        => new(func);

    /// <summary>
    /// Convert async Step to sync (blocking - use with caution).
    /// WARNING: This can cause deadlocks if called from a synchronization context.
    /// Prefer using async/await throughout your call stack.
    /// Uses Task.Run to avoid capturing the synchronization context.
    /// </summary>
    public static SyncStep<TIn, TOut> ToSync<TIn, TOut>(this Step<TIn, TOut> asyncStep)
        => new(input => Task.Run(() => asyncStep(input)).GetAwaiter().GetResult());

    /// <summary>
    /// Compose sync step with async step
    /// </summary>
    public static Step<TIn, TNext> Then<TIn, TMid, TNext>(
        this SyncStep<TIn, TMid> syncStep,
        Step<TMid, TNext> asyncStep)
        => async input =>
        {
            TMid? intermediate = syncStep.Invoke(input);
            return await asyncStep(intermediate);
        };

    /// <summary>
    /// Compose async step with sync step
    /// </summary>
    public static Step<TIn, TNext> Then<TIn, TMid, TNext>(
        this Step<TIn, TMid> asyncStep,
        SyncStep<TMid, TNext> syncStep)
        => async input =>
        {
            TMid? intermediate = await asyncStep(input);
            return syncStep.Invoke(intermediate);
        };

    /// <summary>
    /// Convert SyncStep to Result-based operation
    /// </summary>
    public static SyncStep<TIn, Result<TOut, Exception>> TrySync<TIn, TOut>(
        this SyncStep<TIn, TOut> syncStep)
        => new(input =>
        {
            try
            {
                TOut? result = syncStep.Invoke(input);
                return Result<TOut, Exception>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<TOut, Exception>.Failure(ex);
            }
        });

    /// <summary>
    /// Convert SyncStep to Option-based operation
    /// </summary>
    public static SyncStep<TIn, Option<TOut>> TryOption<TIn, TOut>(
        this SyncStep<TIn, TOut> syncStep,
        Func<TOut, bool> predicate)
        => new(input =>
        {
            try
            {
                TOut? result = syncStep.Invoke(input);
                return predicate(result) ? Option<TOut>.Some(result) : Option<TOut>.None();
            }
            catch
            {
                return Option<TOut>.None();
            }
        });
}