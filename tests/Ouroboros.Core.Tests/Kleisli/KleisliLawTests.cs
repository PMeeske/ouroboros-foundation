// <copyright file="KleisliLawTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using FsCheck.Xunit;
using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Kleisli;
using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.Kleisli;

/// <summary>
/// Verifies the three Kleisli category laws using the actual <c>Then</c> extension
/// methods from <see cref="KleisliExtensions"/> and the <c>Arrow.Identity</c> factory.
///
/// The three laws (for a Kleisli category over a monad M) are:
/// <list type="number">
///   <item>
///     <term>Left Identity</term>
///     <description>id >=> f  is equivalent to  f</description>
///   </item>
///   <item>
///     <term>Right Identity</term>
///     <description>f >=> id  is equivalent to  f</description>
///   </item>
///   <item>
///     <term>Associativity</term>
///     <description>(f >=> g) >=> h  is equivalent to  f >=> (g >=> h)</description>
///   </item>
/// </list>
///
/// Each law is tested for three arrow families:
/// <see cref="Step{TA,TB}"/> (Task monad),
/// <see cref="KleisliResult{TInput,TOutput,TError}"/> (Result monad), and
/// <see cref="KleisliOption{TInput,TOutput}"/> (Option monad).
///
/// Tests use both concrete <c>[Fact]</c> cases with representative inputs and
/// FsCheck <c>[Property]</c> tests with arbitrary integer inputs.
/// </summary>
[Trait("Category", "Property")]
public class KleisliLawTests
{
    // ---------------------------------------------------------------
    //  Step<T,U> laws  (Task monad Kleisli arrows)
    // ---------------------------------------------------------------

    #region Step — Left Identity

    /// <summary>
    /// Left Identity for Step: Arrow.Identity().Then(f) produces the same
    /// result as applying f directly.
    /// Law: id >=> f is equivalent to f.
    /// </summary>
    [Fact]
    public async Task Step_LeftIdentity_WithConcreteInput()
    {
        // Arrange
        Step<int, int> f = x => Task.FromResult(x * 2 + 3);
        var composed = Arrow.Identity<int>().Then(f);

        // Act
        int composedResult = await composed(7);
        int directResult = await f(7);

        // Assert
        composedResult.Should().Be(directResult);
    }

    /// <summary>
    /// Left Identity for Step across arbitrary integer inputs.
    /// </summary>
    [Property(MaxTest = 1000)]
    public async Task<bool> Step_LeftIdentity_HoldsForArbitraryInts(int input)
    {
        Step<int, int> f = x => Task.FromResult(x * 2 + 3);
        var composed = Arrow.Identity<int>().Then(f);

        int composedResult = await composed(input);
        int directResult = await f(input);

        return composedResult == directResult;
    }

    /// <summary>
    /// Left Identity for Step with a type-changing arrow (int to string).
    /// </summary>
    [Fact]
    public async Task Step_LeftIdentity_WithTypeChange()
    {
        // Arrange
        Step<int, string> f = x => Task.FromResult($"value:{x}");
        var composed = Arrow.Identity<int>().Then(f);

        // Act
        string composedResult = await composed(42);
        string directResult = await f(42);

        // Assert
        composedResult.Should().Be(directResult);
    }

    #endregion

    #region Step — Right Identity

    /// <summary>
    /// Right Identity for Step: f.Then(Arrow.Identity()) produces the same
    /// result as applying f directly.
    /// Law: f >=> id is equivalent to f.
    /// </summary>
    [Fact]
    public async Task Step_RightIdentity_WithConcreteInput()
    {
        // Arrange
        Step<int, int> f = x => Task.FromResult(x * 3 - 1);
        var composed = f.Then(Arrow.Identity<int>());

        // Act
        int composedResult = await composed(5);
        int directResult = await f(5);

        // Assert
        composedResult.Should().Be(directResult);
    }

    /// <summary>
    /// Right Identity for Step across arbitrary integer inputs.
    /// </summary>
    [Property(MaxTest = 1000)]
    public async Task<bool> Step_RightIdentity_HoldsForArbitraryInts(int input)
    {
        Step<int, int> f = x => Task.FromResult(x * 3 - 1);
        var composed = f.Then(Arrow.Identity<int>());

        int composedResult = await composed(input);
        int directResult = await f(input);

        return composedResult == directResult;
    }

    /// <summary>
    /// Right Identity for Step with a type-changing arrow (int to string).
    /// </summary>
    [Fact]
    public async Task Step_RightIdentity_WithTypeChange()
    {
        // Arrange
        Step<int, string> f = x => Task.FromResult(x.ToString());
        var composed = f.Then(Arrow.Identity<string>());

        // Act
        string composedResult = await composed(99);
        string directResult = await f(99);

        // Assert
        composedResult.Should().Be(directResult);
    }

    #endregion

    #region Step — Associativity

    /// <summary>
    /// Associativity for Step: (f.Then(g)).Then(h) produces the same result
    /// as f.Then(g.Then(h)).
    /// Law: (f >=> g) >=> h is equivalent to f >=> (g >=> h).
    /// </summary>
    [Fact]
    public async Task Step_Associativity_WithConcreteInput()
    {
        // Arrange
        Step<int, int> f = x => Task.FromResult(x + 1);
        Step<int, int> g = x => Task.FromResult(x * 2);
        Step<int, int> h = x => Task.FromResult(x - 3);

        var leftGrouped = f.Then(g).Then(h);
        var rightGrouped = f.Then(g.Then(h));

        // Act
        int leftResult = await leftGrouped(10);
        int rightResult = await rightGrouped(10);

        // Assert
        leftResult.Should().Be(rightResult);
    }

    /// <summary>
    /// Associativity for Step across arbitrary integer inputs.
    /// </summary>
    [Property(MaxTest = 1000)]
    public async Task<bool> Step_Associativity_HoldsForArbitraryInts(int input)
    {
        Step<int, int> f = x => Task.FromResult(x + 1);
        Step<int, int> g = x => Task.FromResult(x * 2);
        Step<int, int> h = x => Task.FromResult(x - 3);

        var leftGrouped = f.Then(g).Then(h);
        var rightGrouped = f.Then(g.Then(h));

        int leftResult = await leftGrouped(input);
        int rightResult = await rightGrouped(input);

        return leftResult == rightResult;
    }

    /// <summary>
    /// Associativity for Step with heterogeneous types (int to string to int to bool).
    /// </summary>
    [Fact]
    public async Task Step_Associativity_WithMixedTypes()
    {
        // Arrange
        Step<int, string> f = x => Task.FromResult(x.ToString());
        Step<string, int> g = s => Task.FromResult(s.Length);
        Step<int, bool> h = n => Task.FromResult(n > 1);

        var leftGrouped = f.Then(g).Then(h);
        var rightGrouped = f.Then(g.Then(h));

        // Act
        bool leftResult = await leftGrouped(42);
        bool rightResult = await rightGrouped(42);

        // Assert
        leftResult.Should().Be(rightResult);
    }

    #endregion

    // ---------------------------------------------------------------
    //  KleisliResult<T,U,TError> laws  (Result monad Kleisli arrows)
    // ---------------------------------------------------------------

    #region KleisliResult — Left Identity

    /// <summary>
    /// Left Identity for KleisliResult: composing a Result-identity arrow
    /// on the left of f produces the same result as f alone.
    /// Law: id >=> f is equivalent to f  (in the Result monad).
    /// </summary>
    [Fact]
    public async Task KleisliResult_LeftIdentity_WithConcreteInput()
    {
        // Arrange
        KleisliResult<int, int, string> identity =
            x => Task.FromResult(Result<int, string>.Success(x));

        KleisliResult<int, int, string> f =
            x => Task.FromResult(Result<int, string>.Success(x * 2 + 5));

        var composed = identity.Then(f);

        // Act
        Result<int, string> composedResult = await composed(7);
        Result<int, string> directResult = await f(7);

        // Assert
        composedResult.IsSuccess.Should().Be(directResult.IsSuccess);
        composedResult.Value.Should().Be(directResult.Value);
    }

    /// <summary>
    /// Left Identity for KleisliResult across arbitrary integer inputs.
    /// </summary>
    [Property(MaxTest = 1000)]
    public async Task<bool> KleisliResult_LeftIdentity_HoldsForArbitraryInts(int input)
    {
        KleisliResult<int, int, string> identity =
            x => Task.FromResult(Result<int, string>.Success(x));

        KleisliResult<int, int, string> f =
            x => Task.FromResult(Result<int, string>.Success(x * 2 + 5));

        Result<int, string> composedResult = await identity.Then(f)(input);
        Result<int, string> directResult = await f(input);

        return composedResult.IsSuccess == directResult.IsSuccess
               && composedResult.Value == directResult.Value;
    }

    /// <summary>
    /// Left Identity for KleisliResult when the arrow may fail.
    /// The identity on the left should not interfere with failure propagation.
    /// </summary>
    [Fact]
    public async Task KleisliResult_LeftIdentity_WithFailing_Arrow()
    {
        // Arrange
        KleisliResult<int, int, string> identity =
            x => Task.FromResult(Result<int, string>.Success(x));

        KleisliResult<int, int, string> f = x =>
            Task.FromResult(x >= 0
                ? Result<int, string>.Success(x * 2)
                : Result<int, string>.Failure("negative"));

        // Act  — positive input
        Result<int, string> composedPos = await identity.Then(f)(5);
        Result<int, string> directPos = await f(5);

        // Act  — negative input
        Result<int, string> composedNeg = await identity.Then(f)(-3);
        Result<int, string> directNeg = await f(-3);

        // Assert
        composedPos.IsSuccess.Should().Be(directPos.IsSuccess);
        composedPos.Value.Should().Be(directPos.Value);

        composedNeg.IsSuccess.Should().Be(directNeg.IsSuccess);
        composedNeg.Error.Should().Be(directNeg.Error);
    }

    #endregion

    #region KleisliResult — Right Identity

    /// <summary>
    /// Right Identity for KleisliResult: composing a Result-identity arrow
    /// on the right of f produces the same result as f alone.
    /// Law: f >=> id is equivalent to f  (in the Result monad).
    /// </summary>
    [Fact]
    public async Task KleisliResult_RightIdentity_WithConcreteInput()
    {
        // Arrange
        KleisliResult<int, int, string> f =
            x => Task.FromResult(Result<int, string>.Success(x * 3 + 7));

        KleisliResult<int, int, string> identity =
            x => Task.FromResult(Result<int, string>.Success(x));

        var composed = f.Then(identity);

        // Act
        Result<int, string> composedResult = await composed(4);
        Result<int, string> directResult = await f(4);

        // Assert
        composedResult.IsSuccess.Should().Be(directResult.IsSuccess);
        composedResult.Value.Should().Be(directResult.Value);
    }

    /// <summary>
    /// Right Identity for KleisliResult across arbitrary integer inputs.
    /// </summary>
    [Property(MaxTest = 1000)]
    public async Task<bool> KleisliResult_RightIdentity_HoldsForArbitraryInts(int input)
    {
        KleisliResult<int, int, string> f =
            x => Task.FromResult(Result<int, string>.Success(x * 3 + 7));

        KleisliResult<int, int, string> identity =
            x => Task.FromResult(Result<int, string>.Success(x));

        Result<int, string> composedResult = await f.Then(identity)(input);
        Result<int, string> directResult = await f(input);

        return composedResult.IsSuccess == directResult.IsSuccess
               && composedResult.Value == directResult.Value;
    }

    /// <summary>
    /// Right Identity for KleisliResult when the arrow may fail.
    /// Failure from f should be propagated unchanged (identity is never called).
    /// </summary>
    [Fact]
    public async Task KleisliResult_RightIdentity_WithFailing_Arrow()
    {
        // Arrange
        KleisliResult<int, int, string> f = x =>
            Task.FromResult(x >= 0
                ? Result<int, string>.Success(x * 2)
                : Result<int, string>.Failure("negative"));

        KleisliResult<int, int, string> identity =
            x => Task.FromResult(Result<int, string>.Success(x));

        // Act  — positive input
        Result<int, string> composedPos = await f.Then(identity)(5);
        Result<int, string> directPos = await f(5);

        // Act  — negative input
        Result<int, string> composedNeg = await f.Then(identity)(-1);
        Result<int, string> directNeg = await f(-1);

        // Assert
        composedPos.IsSuccess.Should().Be(directPos.IsSuccess);
        composedPos.Value.Should().Be(directPos.Value);

        composedNeg.IsSuccess.Should().Be(directNeg.IsSuccess);
        composedNeg.Error.Should().Be(directNeg.Error);
    }

    #endregion

    #region KleisliResult — Associativity

    /// <summary>
    /// Associativity for KleisliResult: (f.Then(g)).Then(h) produces the same
    /// result as f.Then(g.Then(h)).
    /// Law: (f >=> g) >=> h is equivalent to f >=> (g >=> h)  (in the Result monad).
    /// </summary>
    [Fact]
    public async Task KleisliResult_Associativity_WithConcreteInput()
    {
        // Arrange
        KleisliResult<int, int, string> f =
            x => Task.FromResult(Result<int, string>.Success(x * 2));
        KleisliResult<int, int, string> g =
            x => Task.FromResult(Result<int, string>.Success(x + 10));
        KleisliResult<int, int, string> h =
            x => Task.FromResult(Result<int, string>.Success(x - 5));

        var leftGrouped = f.Then(g).Then(h);
        var rightGrouped = f.Then(g.Then(h));

        // Act
        Result<int, string> leftResult = await leftGrouped(3);
        Result<int, string> rightResult = await rightGrouped(3);

        // Assert
        leftResult.IsSuccess.Should().Be(rightResult.IsSuccess);
        leftResult.Value.Should().Be(rightResult.Value);
    }

    /// <summary>
    /// Associativity for KleisliResult across arbitrary integer inputs.
    /// </summary>
    [Property(MaxTest = 1000)]
    public async Task<bool> KleisliResult_Associativity_HoldsForArbitraryInts(int input)
    {
        KleisliResult<int, int, string> f =
            x => Task.FromResult(Result<int, string>.Success(x * 2));
        KleisliResult<int, int, string> g =
            x => Task.FromResult(Result<int, string>.Success(x + 10));
        KleisliResult<int, int, string> h =
            x => Task.FromResult(Result<int, string>.Success(x - 5));

        Result<int, string> leftResult = await f.Then(g).Then(h)(input);
        Result<int, string> rightResult = await f.Then(g.Then(h))(input);

        return leftResult.IsSuccess == rightResult.IsSuccess
               && leftResult.Value == rightResult.Value;
    }

    /// <summary>
    /// Associativity for KleisliResult when arrows may fail.
    /// Both groupings must produce identical success/failure outcomes and values.
    /// </summary>
    [Fact]
    public async Task KleisliResult_Associativity_WithFailures()
    {
        // Arrange
        KleisliResult<int, int, string> f = x =>
            Task.FromResult(x >= 0
                ? Result<int, string>.Success(x * 2)
                : Result<int, string>.Failure("negative"));

        KleisliResult<int, int, string> g = x =>
            Task.FromResult(x < 100
                ? Result<int, string>.Success(x + 10)
                : Result<int, string>.Failure("too large"));

        KleisliResult<int, int, string> h = x =>
            Task.FromResult(x % 2 == 0
                ? Result<int, string>.Success(x / 2)
                : Result<int, string>.Failure("odd"));

        var leftGrouped = f.Then(g).Then(h);
        var rightGrouped = f.Then(g.Then(h));

        // Test several representative inputs
        foreach (int input in new[] { 0, 5, -1, 50, 99 })
        {
            Result<int, string> leftResult = await leftGrouped(input);
            Result<int, string> rightResult = await rightGrouped(input);

            leftResult.IsSuccess.Should().Be(rightResult.IsSuccess,
                $"associativity violated for input {input}");

            if (leftResult.IsSuccess)
            {
                leftResult.Value.Should().Be(rightResult.Value,
                    $"value mismatch for input {input}");
            }
            else
            {
                leftResult.Error.Should().Be(rightResult.Error,
                    $"error mismatch for input {input}");
            }
        }
    }

    #endregion

    // ---------------------------------------------------------------
    //  KleisliOption<T,U> laws  (Option monad Kleisli arrows)
    // ---------------------------------------------------------------

    #region KleisliOption — Left Identity

    /// <summary>
    /// Left Identity for KleisliOption: composing an Option-identity arrow
    /// on the left of f produces the same result as f alone.
    /// Law: id >=> f is equivalent to f  (in the Option monad).
    /// </summary>
    [Fact]
    public async Task KleisliOption_LeftIdentity_WithConcreteInput()
    {
        // Arrange
        KleisliOption<int, int> identity =
            x => Task.FromResult(Option<int>.Some(x));

        KleisliOption<int, int> f =
            x => Task.FromResult(Option<int>.Some(x * 2 + 1));

        var composed = identity.Then(f);

        // Act
        Option<int> composedResult = await composed(7);
        Option<int> directResult = await f(7);

        // Assert
        composedResult.HasValue.Should().Be(directResult.HasValue);
        composedResult.Value.Should().Be(directResult.Value);
    }

    /// <summary>
    /// Left Identity for KleisliOption across arbitrary integer inputs.
    /// </summary>
    [Property(MaxTest = 1000)]
    public async Task<bool> KleisliOption_LeftIdentity_HoldsForArbitraryInts(int input)
    {
        KleisliOption<int, int> identity =
            x => Task.FromResult(Option<int>.Some(x));

        KleisliOption<int, int> f =
            x => Task.FromResult(Option<int>.Some(x * 2 + 1));

        Option<int> composedResult = await identity.Then(f)(input);
        Option<int> directResult = await f(input);

        return composedResult.HasValue == directResult.HasValue
               && (!composedResult.HasValue || composedResult.Value!.Equals(directResult.Value));
    }

    /// <summary>
    /// Left Identity for KleisliOption when the arrow may return None.
    /// Identity on the left should not interfere with None propagation.
    /// </summary>
    [Fact]
    public async Task KleisliOption_LeftIdentity_WithNone()
    {
        // Arrange
        KleisliOption<int, int> identity =
            x => Task.FromResult(Option<int>.Some(x));

        KleisliOption<int, int> f = x =>
            Task.FromResult(x > 0
                ? Option<int>.Some(x * 2)
                : Option<int>.None());

        // Act  — positive input
        Option<int> composedPos = await identity.Then(f)(5);
        Option<int> directPos = await f(5);

        // Act  — non-positive input
        Option<int> composedNeg = await identity.Then(f)(-3);
        Option<int> directNeg = await f(-3);

        // Assert
        composedPos.HasValue.Should().Be(directPos.HasValue);
        composedPos.Value.Should().Be(directPos.Value);

        composedNeg.HasValue.Should().Be(directNeg.HasValue);
        composedNeg.HasValue.Should().BeFalse();
    }

    #endregion

    #region KleisliOption — Right Identity

    /// <summary>
    /// Right Identity for KleisliOption: composing an Option-identity arrow
    /// on the right of f produces the same result as f alone.
    /// Law: f >=> id is equivalent to f  (in the Option monad).
    /// </summary>
    [Fact]
    public async Task KleisliOption_RightIdentity_WithConcreteInput()
    {
        // Arrange
        KleisliOption<int, int> f =
            x => Task.FromResult(Option<int>.Some(x * 3 + 7));

        KleisliOption<int, int> identity =
            x => Task.FromResult(Option<int>.Some(x));

        var composed = f.Then(identity);

        // Act
        Option<int> composedResult = await composed(4);
        Option<int> directResult = await f(4);

        // Assert
        composedResult.HasValue.Should().Be(directResult.HasValue);
        composedResult.Value.Should().Be(directResult.Value);
    }

    /// <summary>
    /// Right Identity for KleisliOption across arbitrary integer inputs.
    /// </summary>
    [Property(MaxTest = 1000)]
    public async Task<bool> KleisliOption_RightIdentity_HoldsForArbitraryInts(int input)
    {
        KleisliOption<int, int> f =
            x => Task.FromResult(Option<int>.Some(x * 3 + 7));

        KleisliOption<int, int> identity =
            x => Task.FromResult(Option<int>.Some(x));

        Option<int> composedResult = await f.Then(identity)(input);
        Option<int> directResult = await f(input);

        return composedResult.HasValue == directResult.HasValue
               && (!composedResult.HasValue || composedResult.Value!.Equals(directResult.Value));
    }

    /// <summary>
    /// Right Identity for KleisliOption when the arrow may return None.
    /// None from f should be propagated unchanged (identity is never called).
    /// </summary>
    [Fact]
    public async Task KleisliOption_RightIdentity_WithNone()
    {
        // Arrange
        KleisliOption<int, int> f = x =>
            Task.FromResult(x > 0
                ? Option<int>.Some(x * 2)
                : Option<int>.None());

        KleisliOption<int, int> identity =
            x => Task.FromResult(Option<int>.Some(x));

        // Act  — positive input
        Option<int> composedPos = await f.Then(identity)(5);
        Option<int> directPos = await f(5);

        // Act  — non-positive input
        Option<int> composedNeg = await f.Then(identity)(-1);
        Option<int> directNeg = await f(-1);

        // Assert
        composedPos.HasValue.Should().Be(directPos.HasValue);
        composedPos.Value.Should().Be(directPos.Value);

        composedNeg.HasValue.Should().Be(directNeg.HasValue);
        composedNeg.HasValue.Should().BeFalse();
    }

    #endregion

    #region KleisliOption — Associativity

    /// <summary>
    /// Associativity for KleisliOption: (f.Then(g)).Then(h) produces the same
    /// result as f.Then(g.Then(h)).
    /// Law: (f >=> g) >=> h is equivalent to f >=> (g >=> h)  (in the Option monad).
    /// </summary>
    [Fact]
    public async Task KleisliOption_Associativity_WithConcreteInput()
    {
        // Arrange
        KleisliOption<int, int> f =
            x => Task.FromResult(Option<int>.Some(x * 2));
        KleisliOption<int, int> g =
            x => Task.FromResult(Option<int>.Some(x + 10));
        KleisliOption<int, int> h =
            x => Task.FromResult(Option<int>.Some(x - 5));

        var leftGrouped = f.Then(g).Then(h);
        var rightGrouped = f.Then(g.Then(h));

        // Act
        Option<int> leftResult = await leftGrouped(3);
        Option<int> rightResult = await rightGrouped(3);

        // Assert
        leftResult.HasValue.Should().Be(rightResult.HasValue);
        leftResult.Value.Should().Be(rightResult.Value);
    }

    /// <summary>
    /// Associativity for KleisliOption across arbitrary integer inputs.
    /// </summary>
    [Property(MaxTest = 1000)]
    public async Task<bool> KleisliOption_Associativity_HoldsForArbitraryInts(int input)
    {
        KleisliOption<int, int> f =
            x => Task.FromResult(Option<int>.Some(x * 2));
        KleisliOption<int, int> g =
            x => Task.FromResult(Option<int>.Some(x + 10));
        KleisliOption<int, int> h =
            x => Task.FromResult(Option<int>.Some(x - 5));

        Option<int> leftResult = await f.Then(g).Then(h)(input);
        Option<int> rightResult = await f.Then(g.Then(h))(input);

        return leftResult.HasValue == rightResult.HasValue
               && (!leftResult.HasValue || leftResult.Value!.Equals(rightResult.Value));
    }

    /// <summary>
    /// Associativity for KleisliOption when arrows may return None.
    /// Both groupings must produce identical Some/None outcomes and values.
    /// </summary>
    [Fact]
    public async Task KleisliOption_Associativity_WithNone()
    {
        // Arrange
        KleisliOption<int, int> f = x =>
            Task.FromResult(x >= 0
                ? Option<int>.Some(x * 2)
                : Option<int>.None());

        KleisliOption<int, int> g = x =>
            Task.FromResult(x < 100
                ? Option<int>.Some(x + 10)
                : Option<int>.None());

        KleisliOption<int, int> h = x =>
            Task.FromResult(x % 2 == 0
                ? Option<int>.Some(x / 2)
                : Option<int>.None());

        var leftGrouped = f.Then(g).Then(h);
        var rightGrouped = f.Then(g.Then(h));

        // Test several representative inputs
        foreach (int input in new[] { 0, 5, -1, 50, 99 })
        {
            Option<int> leftResult = await leftGrouped(input);
            Option<int> rightResult = await rightGrouped(input);

            leftResult.HasValue.Should().Be(rightResult.HasValue,
                $"associativity violated for input {input}");

            if (leftResult.HasValue)
            {
                leftResult.Value.Should().Be(rightResult.Value,
                    $"value mismatch for input {input}");
            }
        }
    }

    #endregion
}
