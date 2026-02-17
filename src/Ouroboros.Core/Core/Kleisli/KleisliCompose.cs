namespace Ouroboros.Core.Kleisli;

/// <summary>
/// Represents Kleisli composition as a higher-order function.
/// Takes two Kleisli arrows and returns their composition.
/// This enables functional composition patterns and currying.
/// </summary>
/// <typeparam name="TIn">The input type of the first arrow.</typeparam>
/// <typeparam name="TMid">The intermediate type between arrows.</typeparam>
/// <typeparam name="TOut">The output type of the second arrow.</typeparam>
/// <param name="f">The first Kleisli arrow.</param>
/// <param name="g">The second Kleisli arrow.</param>
/// <returns>A composed Kleisli arrow.</returns>
public delegate Kleisli<TIn, TOut> KleisliCompose<TIn, TMid, TOut>(
    Kleisli<TIn, TMid> f,
    Kleisli<TMid, TOut> g);