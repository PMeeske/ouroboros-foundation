namespace Ouroboros.Core.Kleisli;

/// <summary>
/// Kleisli arrow for Result monad computations.
/// </summary>
/// <typeparam name="TInput">The input type.</typeparam>
/// <typeparam name="TOutput">The output type.</typeparam>
/// <typeparam name="TError">The error type.</typeparam>
/// <param name="input">The input value.</param>
/// <returns>A Task containing a Result of the computation.</returns>
public delegate Task<Result<TOutput, TError>> KleisliResult<in TInput, TOutput, TError>(TInput input);