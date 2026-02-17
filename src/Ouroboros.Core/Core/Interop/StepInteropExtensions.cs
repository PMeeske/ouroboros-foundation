namespace Ouroboros.Core.Interop;

/// <summary>
/// Minimal interop-specific extensions for integration with external systems.
/// The main monadic operations are provided by unified KleisliExtensions.
/// </summary>
public static class StepInteropExtensions
{
    /// <summary>
    /// Lifts a pure function into a Step (Kleisli arrow)
    /// </summary>
    public static Step<TA, TB> ToStep<TA, TB>(this Func<TA, TB> func)
        => input => Task.FromResult(func(input));

    /// <summary>
    /// Lifts an async function into a Step
    /// </summary>
    public static Step<TA, TB> ToStep<TA, TB>(this Func<TA, Task<TB>> func)
        => new(func);
}