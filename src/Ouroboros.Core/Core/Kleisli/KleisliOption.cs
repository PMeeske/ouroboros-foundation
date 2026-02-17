namespace Ouroboros.Core.Kleisli;

/// <summary>
/// Kleisli arrow for Option monad computations.
/// </summary>
/// <typeparam name="TInput">The input type.</typeparam>
/// <typeparam name="TOutput">The output type.</typeparam>
/// <param name="input">The input value.</param>
/// <returns>A Task containing an Option of the computation result.</returns>
public delegate Task<Option<TOutput>> KleisliOption<in TInput, TOutput>(TInput input);