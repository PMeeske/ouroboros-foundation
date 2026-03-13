// <copyright file="StepPropertyTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Core.PropertyBased;

using FsCheck.Xunit;
using Ouroboros.Core.Kleisli;
using Ouroboros.Core.Steps;

/// <summary>
/// Property-based tests for Step (Kleisli arrow) monad laws using FsCheck.
/// Step{TA,TB} is unified with Kleisli{TA,TB} — both represent Task monad arrows.
/// Verifies that monad laws and functor laws hold for arbitrary inputs.
/// </summary>
[Trait("Category", "Property")]
public class StepPropertyTests
{
    // Pure: the identity Kleisli arrow (return in Haskell)
    private static Step<T, T> Pure<T>() => x => Task.FromResult(x);

    #region Monad Laws (expressed as Kleisli arrow laws)

    /// <summary>
    /// Left Identity: pure >=> f ≡ f
    /// Composing the identity arrow before f produces the same result as just f.
    /// </summary>
    [Property(MaxTest = 1000)]
    public bool Step_LeftIdentity_HoldsForAllInts_TotalFunction(int a)
    {
        Step<int, int> f = x => Task.FromResult(x * 2 + 1);

        var left = Pure<int>().Then(f);
        var right = f;

        return left(a).Result == right(a).Result;
    }

    /// <summary>
    /// Left Identity with partial function (function that may transform conditionally).
    /// </summary>
    [Property(MaxTest = 1000)]
    public bool Step_LeftIdentity_HoldsForAllInts_PartialFunction(int a)
    {
        Step<int, string> f = x => Task.FromResult(x >= 0 ? x.ToString() : "negative");

        var left = Pure<int>().Then(f);
        var right = f;

        return left(a).Result == right(a).Result;
    }

    /// <summary>
    /// Right Identity: f >=> pure ≡ f
    /// Composing f before the identity arrow produces the same result as just f.
    /// </summary>
    [Property(MaxTest = 1000)]
    public bool Step_RightIdentity_HoldsForAllInts(int a)
    {
        Step<int, int> f = x => Task.FromResult(x * 3);

        var left = f.Then(Pure<int>());
        var right = f;

        return left(a).Result == right(a).Result;
    }

    /// <summary>
    /// Right Identity with type conversion.
    /// </summary>
    [Property(MaxTest = 1000)]
    public bool Step_RightIdentity_HoldsWithTypeConversion(int a)
    {
        Step<int, string> f = x => Task.FromResult($"value:{x}");

        var left = f.Then(Pure<string>());
        var right = f;

        return left(a).Result == right(a).Result;
    }

    /// <summary>
    /// Associativity: (f >=> g) >=> h ≡ f >=> (g >=> h)
    /// The order of grouping Kleisli composition does not matter.
    /// </summary>
    [Property(MaxTest = 1000)]
    public bool Step_Associativity_HoldsForAllInts(int a)
    {
        Step<int, int> f = x => Task.FromResult(x + 1);
        Step<int, int> g = x => Task.FromResult(x * 2);
        Step<int, int> h = x => Task.FromResult(x - 3);

        var left = f.Then(g).Then(h);
        var right = f.Then(g.Then(h));

        return left(a).Result == right(a).Result;
    }

    /// <summary>
    /// Associativity with heterogeneous types.
    /// </summary>
    [Property(MaxTest = 1000)]
    public bool Step_Associativity_HoldsWithMixedTypes(int a)
    {
        Step<int, string> f = x => Task.FromResult(x.ToString());
        Step<string, int> g = s => Task.FromResult(s.Length);
        Step<int, bool> h = n => Task.FromResult(n > 3);

        var left = f.Then(g).Then(h);
        var right = f.Then(g.Then(h));

        return left(a).Result == right(a).Result;
    }

    #endregion

    #region Functor Laws (via Map)

    /// <summary>
    /// Functor Identity: step.Map(id) ≡ step
    /// Mapping the identity function does not change the result.
    /// </summary>
    [Property(MaxTest = 1000)]
    public bool Step_FunctorIdentity_HoldsForAllInts(int a)
    {
        Step<int, int> step = x => Task.FromResult(x * 5);

        var mapped = step.Map<int, int, int>(x => x);
        var original = step;

        return mapped(a).Result == original(a).Result;
    }

    /// <summary>
    /// Functor Composition: step.Map(f).Map(g) ≡ step.Map(x => g(f(x)))
    /// Sequential mapping is equivalent to mapping the composed function.
    /// </summary>
    [Property(MaxTest = 1000)]
    public bool Step_FunctorComposition_HoldsForAllInts(int a)
    {
        Step<int, int> step = x => Task.FromResult(x + 10);
        Func<int, int> f = x => x * 2;
        Func<int, string> g = x => x.ToString();

        var left = step.Map(f).Map(g);
        var right = step.Map<int, int, string>(x => g(f(x)));

        return left(a).Result == right(a).Result;
    }

    #endregion

    #region Map-Then Relationship

    /// <summary>
    /// Map in terms of Then: step.Map(f) ≡ step.Then(x => Task.FromResult(f(x)))
    /// Map can be derived from Then (Bind), which is a requirement for a proper monad.
    /// </summary>
    [Property(MaxTest = 1000)]
    public bool Step_MapThenRelationship_HoldsForAllInts(int a)
    {
        Step<int, int> step = x => Task.FromResult(x * 7);
        Func<int, string> f = x => $"mapped:{x}";

        var viaMap = step.Map(f);
        Step<int, string> liftedF = x => Task.FromResult(f(x));
        var viaThen = step.Then(liftedF);

        return viaMap(a).Result == viaThen(a).Result;
    }

    #endregion

    #region Composition Preserves Properties

    /// <summary>
    /// Composition of pure functions preserves determinism.
    /// Running the same composition twice with the same input gives the same result.
    /// </summary>
    [Property(MaxTest = 1000)]
    public bool Step_Composition_IsDeterministic(int a)
    {
        Step<int, int> pipeline = Pure<int>()
            .Then<int, int, int>((Step<int, int>)(x => Task.FromResult(x + 1)))
            .Then<int, int, int>((Step<int, int>)(x => Task.FromResult(x * 2)))
            .Then<int, int, int>((Step<int, int>)(x => Task.FromResult(x - 3)));

        var result1 = pipeline(a).Result;
        var result2 = pipeline(a).Result;

        return result1 == result2;
    }

    #endregion
}
