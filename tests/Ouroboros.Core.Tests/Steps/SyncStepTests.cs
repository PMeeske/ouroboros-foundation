using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class SyncStepTests
{
    [Fact]
    public void Constructor_NullFunc_ThrowsArgumentNullException()
    {
        Func<int, string> func = null!;
        var act = () => new SyncStep<int, string>(func);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Invoke_AppliesFunction()
    {
        var sut = new SyncStep<int, string>(x => x.ToString());
        sut.Invoke(42).Should().Be("42");
    }

    [Fact]
    public async Task ToAsync_ReturnsAsyncStep()
    {
        var sut = new SyncStep<int, int>(x => x * 2);
        Step<int, int> asyncStep = sut.ToAsync();

        var result = await asyncStep(5);
        result.Should().Be(10);
    }

    [Fact]
    public void Pipe_SyncStep_ComposesFunctions()
    {
        var step1 = new SyncStep<int, int>(x => x + 1);
        var step2 = new SyncStep<int, string>(x => x.ToString());

        var composed = step1.Pipe(step2);
        composed.Invoke(9).Should().Be("10");
    }

    [Fact]
    public async Task Pipe_AsyncStep_ComposesFunctions()
    {
        var syncStep = new SyncStep<int, int>(x => x * 3);
        Step<int, string> asyncStep = x => Task.FromResult($"value={x}");

        Step<int, string> composed = syncStep.Pipe(asyncStep);
        var result = await composed(4);
        result.Should().Be("value=12");
    }

    [Fact]
    public void Map_TransformsOutput()
    {
        var sut = new SyncStep<int, int>(x => x + 10);
        var mapped = sut.Map(x => x.ToString());

        mapped.Invoke(5).Should().Be("15");
    }

    [Fact]
    public void Bind_ComposesMonadically()
    {
        var sut = new SyncStep<int, int>(x => x + 1);
        var bound = sut.Bind<string>(intermediate =>
            new SyncStep<int, string>(input => $"{input}->{intermediate}"));

        bound.Invoke(10).Should().Be("10->11");
    }

    [Fact]
    public void Equals_SameDelegate_ReturnsTrue()
    {
        Func<int, int> f = x => x;
        var step1 = new SyncStep<int, int>(f);
        var step2 = new SyncStep<int, int>(f);

        step1.Equals(step2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentDelegate_ReturnsFalse()
    {
        var step1 = new SyncStep<int, int>(x => x);
        var step2 = new SyncStep<int, int>(x => x);

        step1.Equals(step2).Should().BeFalse();
    }

    [Fact]
    public void Equals_ObjectOverload_WorksCorrectly()
    {
        Func<int, int> f = x => x;
        var step1 = new SyncStep<int, int>(f);
        object step2 = new SyncStep<int, int>(f);

        step1.Equals(step2).Should().BeTrue();
        step1.Equals("not a step").Should().BeFalse();
        step1.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameDelegate_ReturnsSameHash()
    {
        Func<int, int> f = x => x;
        var step1 = new SyncStep<int, int>(f);
        var step2 = new SyncStep<int, int>(f);

        step1.GetHashCode().Should().Be(step2.GetHashCode());
    }

    [Fact]
    public void ImplicitConversion_FromFunc_CreatesSyncStep()
    {
        SyncStep<int, string> step = (Func<int, string>)(x => x.ToString());
        step.Invoke(7).Should().Be("7");
    }

    [Fact]
    public async Task ImplicitConversion_ToAsyncStep_Works()
    {
        var syncStep = new SyncStep<int, int>(x => x * 2);
        Step<int, int> asyncStep = syncStep;

        var result = await asyncStep(5);
        result.Should().Be(10);
    }

    [Fact]
    public void Identity_ReturnsInputUnchanged()
    {
        var identity = SyncStep<int, int>.Identity;
        identity.Invoke(42).Should().Be(42);
    }
}
