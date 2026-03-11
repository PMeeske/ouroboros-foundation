namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class SyncStepAdditionalTests
{
    [Fact]
    public void GetHashCode_DefaultStruct_ReturnsZero()
    {
        var step = default(SyncStep<int, int>);
        step.GetHashCode().Should().Be(0);
    }

    [Fact]
    public void Equals_DefaultStruct_EqualsDefault()
    {
        var step1 = default(SyncStep<int, int>);
        var step2 = default(SyncStep<int, int>);

        step1.Equals(step2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        var step = new SyncStep<int, int>(x => x);
        step.Equals(42).Should().BeFalse();
    }

    [Fact]
    public void Pipe_SyncStep_ChainMultiple()
    {
        var step1 = new SyncStep<int, int>(x => x + 1);
        var step2 = new SyncStep<int, int>(x => x * 2);
        var step3 = new SyncStep<int, string>(x => x.ToString());

        var composed = step1.Pipe(step2).Pipe(step3);
        composed.Invoke(4).Should().Be("10"); // (4+1)*2 = 10
    }

    [Fact]
    public async Task Pipe_AsyncStep_ChainsSyncThenAsync()
    {
        var syncStep = new SyncStep<int, int>(x => x + 1);
        Step<int, string> asyncStep = async x =>
        {
            await Task.Yield();
            return $"v={x}";
        };

        var composed = syncStep.Pipe(asyncStep);
        var result = await composed(9);
        result.Should().Be("v=10");
    }

    [Fact]
    public void Map_ChainMultiple()
    {
        var step = new SyncStep<int, int>(x => x + 1);
        var mapped = step.Map(x => x * 2).Map(x => x.ToString());

        mapped.Invoke(4).Should().Be("10"); // (4+1)*2 = 10
    }

    [Fact]
    public void Bind_ReusesOriginalInput()
    {
        var sut = new SyncStep<int, int>(x => x + 10);
        var bound = sut.Bind<string>(intermediate =>
            new SyncStep<int, string>(input => $"in={input},mid={intermediate}"));

        // input=5, intermediate = 5+10=15, nextStep receives input=5
        bound.Invoke(5).Should().Be("in=5,mid=15");
    }

    [Fact]
    public void Identity_IsIdempotent()
    {
        var identity = SyncStep<string, string>.Identity;
        identity.Invoke("hello").Should().Be("hello");
        identity.Invoke("").Should().Be("");
    }

    [Fact]
    public async Task ImplicitConversion_ToAsyncStep_PreservesBehavior()
    {
        SyncStep<string, int> syncStep = (Func<string, int>)(s => s.Length);
        Step<string, int> asyncStep = syncStep;

        var result = await asyncStep("hello");
        result.Should().Be(5);
    }
}
