// <copyright file="StepTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Core;

using FluentAssertions;
using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Kleisli;
using Ouroboros.Core.Steps;
using Xunit;

/// <summary>
/// Comprehensive tests for the Step and Kleisli arrow implementations.
/// Tests monadic composition, laws, and async behavior.
/// </summary>
[Trait("Category", "Unit")]
public class StepTests
{
    #region Step Delegate Tests

    [Fact]
    public async Task Step_SimpleExecution_ReturnsExpectedResult()
    {
        // Arrange
        Step<int, int> double_ = x => Task.FromResult(x * 2);

        // Act
        var result = await double_(5);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public async Task Step_WithTypeConversion_WorksCorrectly()
    {
        // Arrange
        Step<int, string> toString = x => Task.FromResult(x.ToString());

        // Act
        var result = await toString(42);

        // Assert
        result.Should().Be("42");
    }

    [Fact]
    public async Task Step_WithAsyncOperation_CompletesCorrectly()
    {
        // Arrange
        Step<int, int> delayedDouble = async x =>
        {
            await Task.Delay(10);
            return x * 2;
        };

        // Act
        var result = await delayedDouble(5);

        // Assert
        result.Should().Be(10);
    }

    #endregion

    #region Arrow Factory Method Tests

    [Fact]
    public async Task Arrow_Identity_ReturnsInputUnchanged()
    {
        // Arrange
        Step<int, int> identity = Arrow.Identity<int>();

        // Act
        var result = await identity(42);

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public async Task Arrow_Lift_WrapsFunction()
    {
        // Arrange
        Step<int, int> lifted = Arrow.Lift<int, int>(x => x * 2);

        // Act
        var result = await lifted(5);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public async Task Arrow_LiftAsync_WrapsAsyncFunction()
    {
        // Arrange
        Step<int, int> lifted = Arrow.LiftAsync<int, int>(x => Task.FromResult(x * 2));

        // Act
        var result = await lifted(5);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public async Task Arrow_TryLift_OnSuccess_ReturnsSuccess()
    {
        // Arrange
        KleisliResult<int, int, Exception> arrow = Arrow.TryLift<int, int>(x => x * 2);

        // Act
        Result<int, Exception> result = await arrow(5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task Arrow_TryLift_OnException_ReturnsFailure()
    {
        // Arrange
        KleisliResult<int, int, Exception> arrow = Arrow.TryLift<int, int>(x =>
        {
            if (x < 0) throw new ArgumentException("Must be positive");
            return x * 2;
        });

        // Act
        Result<int, Exception> result = await arrow(-1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public async Task Arrow_TryLiftAsync_OnSuccess_ReturnsSuccess()
    {
        // Arrange
        KleisliResult<int, int, Exception> arrow = Arrow.TryLiftAsync<int, int>(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        // Act
        Result<int, Exception> result = await arrow(5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task Arrow_TryLiftAsync_OnException_ReturnsFailure()
    {
        // Arrange
        KleisliResult<int, int, Exception> arrow = Arrow.TryLiftAsync<int, int>(async x =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Test error");
        });

        // Act
        Result<int, Exception> result = await arrow(5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task Arrow_Success_AlwaysReturnsSuccess()
    {
        // Arrange
        KleisliResult<int, string, string> arrow = Arrow.Success<int, string, string>("constant");

        // Act
        Result<string, string> result = await arrow(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("constant");
    }

    [Fact]
    public async Task Arrow_Failure_AlwaysReturnsFailure()
    {
        // Arrange
        KleisliResult<int, string, string> arrow = Arrow.Failure<int, string, string>("error");

        // Act
        Result<string, string> result = await arrow(42);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("error");
    }

    [Fact]
    public async Task Arrow_Some_AlwaysReturnsSome()
    {
        // Arrange
        KleisliOption<int, string> arrow = Arrow.Some<int, string>("value");

        // Act
        Option<string> result = await arrow(42);

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("value");
    }

    [Fact]
    public async Task Arrow_None_AlwaysReturnsNone()
    {
        // Arrange
        KleisliOption<int, string> arrow = Arrow.None<int, string>();

        // Act
        Option<string> result = await arrow(42);

        // Assert
        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region Compose Tests

    [Fact]
    public async Task Arrow_Compose_CombinesTwoArrows()
    {
        // Arrange
        KleisliCompose<int, int, string> compose = Arrow.Compose<int, int, string>();
        Kleisli<int, int> double_ = x => Task.FromResult(x * 2);
        Kleisli<int, string> toString = x => Task.FromResult(x.ToString());

        // Act
        Kleisli<int, string> composed = compose(double_, toString);
        var result = await composed(5);

        // Assert
        result.Should().Be("10");
    }

    [Fact]
    public async Task Arrow_ComposeWith_CurriesComposition()
    {
        // Arrange
        Kleisli<int, int> double_ = x => Task.FromResult(x * 2);
        Kleisli<int, string> toString = x => Task.FromResult(x.ToString());

        Func<Kleisli<int, string>, Kleisli<int, string>> curriedCompose = Arrow.ComposeWith<int, int, string>(double_);

        // Act
        Kleisli<int, string> composed = curriedCompose(toString);
        var result = await composed(5);

        // Assert
        result.Should().Be("10");
    }

    #endregion

    #region SyncStep Tests

    [Fact]
    public void SyncStep_Invoke_ExecutesSynchronously()
    {
        // Arrange
        SyncStep<int, int> step = new SyncStep<int, int>(x => x * 2);

        // Act
        var result = step.Invoke(5);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public async Task SyncStep_ToAsync_ConvertsToAsyncStep()
    {
        // Arrange
        SyncStep<int, int> syncStep = new SyncStep<int, int>(x => x * 2);

        // Act
        Step<int, int> asyncStep = syncStep.ToAsync();
        var result = await asyncStep(5);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public void SyncStep_Pipe_ComposesSynchronously()
    {
        // Arrange
        SyncStep<int, int> double_ = new SyncStep<int, int>(x => x * 2);
        SyncStep<int, int> addThree = new SyncStep<int, int>(x => x + 3);

        // Act
        SyncStep<int, int> composed = double_.Pipe(addThree);
        var result = composed.Invoke(5);

        // Assert
        result.Should().Be(13); // (5 * 2) + 3
    }

    [Fact]
    public async Task SyncStep_PipeWithAsync_ComposesWithAsyncStep()
    {
        // Arrange
        SyncStep<int, int> double_ = new SyncStep<int, int>(x => x * 2);
        Step<int, string> toString = x => Task.FromResult(x.ToString());

        // Act
        Step<int, string> composed = double_.Pipe(toString);
        var result = await composed(5);

        // Assert
        result.Should().Be("10");
    }

    [Fact]
    public void SyncStep_Map_TransformsOutput()
    {
        // Arrange
        SyncStep<int, int> step = new SyncStep<int, int>(x => x * 2);

        // Act
        SyncStep<int, string> mapped = step.Map(x => x.ToString());
        var result = mapped.Invoke(5);

        // Assert
        result.Should().Be("10");
    }

    [Fact]
    public void SyncStep_Identity_ReturnsInputUnchanged()
    {
        // Arrange
        SyncStep<int, int> identity = SyncStep<int, int>.Identity;

        // Act
        var result = identity.Invoke(42);

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public void SyncStep_ImplicitFromFunc_Works()
    {
        // Arrange
        Func<int, int> func = x => x * 2;

        // Act
        SyncStep<int, int> step = func;
        var result = step.Invoke(5);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public async Task SyncStep_ImplicitToStep_Works()
    {
        // Arrange
        SyncStep<int, int> syncStep = new SyncStep<int, int>(x => x * 2);

        // Act
        Step<int, int> asyncStep = syncStep;
        var result = await asyncStep(5);

        // Assert
        result.Should().Be(10);
    }

    #endregion

    #region SyncStepExtensions Tests

    [Fact]
    public void ToSyncStep_ConvertsFuncToSyncStep()
    {
        // Arrange
        Func<int, int> func = x => x * 2;

        // Act
        SyncStep<int, int> step = func.ToSyncStep();
        var result = step.Invoke(5);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public async Task SyncStep_Then_WithAsyncStep_Composes()
    {
        // Arrange
        SyncStep<int, int> syncStep = new SyncStep<int, int>(x => x * 2);
        Step<int, string> asyncStep = x => Task.FromResult(x.ToString());

        // Act
        Step<int, string> composed = syncStep.Then(asyncStep);
        var result = await composed(5);

        // Assert
        result.Should().Be("10");
    }

    [Fact]
    public async Task Step_Then_WithSyncStep_Composes()
    {
        // Arrange
        Step<int, int> asyncStep = x => Task.FromResult(x * 2);
        SyncStep<int, string> syncStep = new SyncStep<int, string>(x => x.ToString());

        // Act
        Step<int, string> composed = asyncStep.Then(syncStep);
        var result = await composed(5);

        // Assert
        result.Should().Be("10");
    }

    [Fact]
    public void SyncStep_TrySync_OnSuccess_ReturnsSuccess()
    {
        // Arrange
        SyncStep<int, int> step = new SyncStep<int, int>(x => x * 2);

        // Act
        SyncStep<int, Result<int, Exception>> wrapped = step.TrySync();
        Result<int, Exception> result = wrapped.Invoke(5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void SyncStep_TrySync_OnException_ReturnsFailure()
    {
        // Arrange
        SyncStep<int, int> step = new SyncStep<int, int>(x => throw new InvalidOperationException("Test"));

        // Act
        SyncStep<int, Result<int, Exception>> wrapped = step.TrySync();
        Result<int, Exception> result = wrapped.Invoke(5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void SyncStep_TryOption_WhenPredicatePasses_ReturnsSome()
    {
        // Arrange
        SyncStep<int, int> step = new SyncStep<int, int>(x => x * 2);

        // Act
        SyncStep<int, Option<int>> wrapped = step.TryOption(x => x > 0);
        Option<int> result = wrapped.Invoke(5);

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void SyncStep_TryOption_WhenPredicateFails_ReturnsNone()
    {
        // Arrange
        SyncStep<int, int> step = new SyncStep<int, int>(x => x * 2);

        // Act
        SyncStep<int, Option<int>> wrapped = step.TryOption(x => x > 100);
        Option<int> result = wrapped.Invoke(5);

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void SyncStep_TryOption_OnException_ReturnsNone()
    {
        // Arrange
        SyncStep<int, int> step = new SyncStep<int, int>(x => throw new InvalidOperationException());

        // Act
        SyncStep<int, Option<int>> wrapped = step.TryOption(x => true);
        Option<int> result = wrapped.Invoke(5);

        // Assert
        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region Monadic Laws Tests

    [Fact]
    public async Task Kleisli_LeftIdentity_Holds()
    {
        // Left identity: return >=> f = f
        var a = 5;
        Kleisli<int, int> f = x => Task.FromResult(x * 2);
        Step<int, int> identity = Arrow.Identity<int>();

        // Compose identity then f using explicit Kleisli arrows
        Kleisli<int, int> identityKleisli = x => identity(x);
        Kleisli<int, int> composed = Arrow.Compose<int, int, int>()(identityKleisli, f);

        // Act
        var composedResult = await composed(a);
        var directResult = await f(a);

        // Assert
        composedResult.Should().Be(directResult);
    }

    [Fact]
    public async Task Kleisli_RightIdentity_Holds()
    {
        // Right identity: f >=> return = f
        var a = 5;
        Kleisli<int, int> f = x => Task.FromResult(x * 2);
        Step<int, int> identity = Arrow.Identity<int>();

        // Compose f then identity
        Kleisli<int, int> identityKleisli = x => identity(x);
        Kleisli<int, int> composed = Arrow.Compose<int, int, int>()(f, identityKleisli);

        // Act
        var composedResult = await composed(a);
        var directResult = await f(a);

        // Assert
        composedResult.Should().Be(directResult);
    }

    [Fact]
    public async Task Kleisli_Associativity_Holds()
    {
        // Associativity: (f >=> g) >=> h = f >=> (g >=> h)
        var a = 5;
        Kleisli<int, int> f = x => Task.FromResult(x * 2);
        Kleisli<int, int> g = x => Task.FromResult(x + 3);
        Kleisli<int, int> h = x => Task.FromResult(x - 1);

        // (f >=> g) >=> h
        Kleisli<int, int> fg = Arrow.Compose<int, int, int>()(f, g);
        Kleisli<int, int> fg_h = Arrow.Compose<int, int, int>()(fg, h);

        // f >=> (g >=> h)
        Kleisli<int, int> gh = Arrow.Compose<int, int, int>()(g, h);
        Kleisli<int, int> f_gh = Arrow.Compose<int, int, int>()(f, gh);

        // Act
        var leftResult = await fg_h(a);
        var rightResult = await f_gh(a);

        // Assert
        leftResult.Should().Be(rightResult);
    }

    #endregion

    #region SyncStep Equality Tests

    [Fact]
    public void SyncStep_SameDelegate_AreEqual()
    {
        // Arrange
        Func<int, int> func = x => x * 2;
        SyncStep<int, int> step1 = new SyncStep<int, int>(func);
        SyncStep<int, int> step2 = new SyncStep<int, int>(func);

        // Assert
        step1.Equals(step2).Should().BeTrue();
    }

    [Fact]
    public void SyncStep_DifferentDelegates_AreNotEqual()
    {
        // Arrange
        SyncStep<int, int> step1 = new SyncStep<int, int>(x => x * 2);
        SyncStep<int, int> step2 = new SyncStep<int, int>(x => x * 2);

        // Assert - Different lambda instances are different delegates
        step1.Equals(step2).Should().BeFalse();
    }

    [Fact]
    public void SyncStep_GetHashCode_SameDelegateReturnsSameHash()
    {
        // Arrange
        Func<int, int> func = x => x * 2;
        SyncStep<int, int> step1 = new SyncStep<int, int>(func);
        SyncStep<int, int> step2 = new SyncStep<int, int>(func);

        // Assert
        step1.GetHashCode().Should().Be(step2.GetHashCode());
    }

    #endregion

    #region Complex Pipeline Tests

    [Fact]
    public async Task Step_ComplexPipeline_WorksCorrectly()
    {
        // Arrange - Build a complex pipeline
        KleisliResult<string, int, Exception> parseNumber = Arrow.TryLift<string, int>(int.Parse);
        Step<int, int> doubleNumber = Arrow.Lift<int, int>(x => x * 2);
        Step<int, string> formatResult = Arrow.Lift<int, string>(x => $"Result: {x}");

        // Act - This demonstrates a real-world pattern
        var input = "21";
        Result<int, Exception> parseResult = await parseNumber(input);

        // Build the final result
        string result;
        if (parseResult.IsSuccess)
        {
            var doubled = await doubleNumber(parseResult.Value);
            var formatted = await formatResult(doubled);
            result = formatted;
        }
        else
        {
            result = "Error";
        }

        // Assert
        result.Should().Be("Result: 42");
    }

    [Fact]
    public async Task SyncStep_ChainedOperations_WorkCorrectly()
    {
        // Arrange
        SyncStep<int, string> step = SyncStep<int, int>.Identity
            .Pipe(new SyncStep<int, int>(x => x * 2))
            .Pipe(new SyncStep<int, int>(x => x + 3))
            .Pipe(new SyncStep<int, string>(x => x.ToString()));

        // Act
        var result = step.Invoke(5);

        // Assert
        result.Should().Be("13");
    }

    [Theory]
    [InlineData(1, "5")]
    [InlineData(5, "13")]
    [InlineData(10, "23")]
    [InlineData(0, "3")]
    public void SyncStep_WithVariousInputs_ProducesExpectedResults(int input, string expected)
    {
        // Arrange
        // (input * 2) + 3 = expected
        SyncStep<int, string> step = new SyncStep<int, int>(x => x * 2)
            .Pipe(new SyncStep<int, int>(x => x + 3))
            .Map(x => x.ToString());

        // Act
        var result = step.Invoke(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
