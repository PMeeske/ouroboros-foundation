using Ouroboros.Core;

namespace Ouroboros.Tests;

[Trait("Category", "Unit")]
public sealed class MorphismTests
{
    [Fact]
    public void Morphism_CanWrapPureFunction()
    {
        Morphism<int, string> morphism = x => x.ToString();
        morphism(42).Should().Be("42");
    }

    [Fact]
    public void Morphism_CanBeComposed()
    {
        Morphism<int, int> doubleIt = x => x * 2;
        Morphism<int, string> toString = x => x.ToString();

        // Manual composition: toString(doubleIt(x))
        Morphism<int, string> composed = x => toString(doubleIt(x));
        composed(5).Should().Be("10");
    }

    [Fact]
    public void Morphism_Identity_ReturnsInput()
    {
        Morphism<int, int> identity = x => x;
        identity(42).Should().Be(42);
    }

    [Fact]
    public void Morphism_SupportsContravariance_InInput()
    {
        // Morphism<in TA, out TB> - TA is contravariant
        Morphism<object, string> objectToString = o => o.ToString()!;

        // A morphism that accepts object should be usable where string input is expected
        // (contravariance on input)
        Func<object, string> func = objectToString;
        func("hello").Should().Be("hello");
    }

    [Fact]
    public void Morphism_SupportsCovariance_InOutput()
    {
        // Morphism<in TA, out TB> - TB is covariant
        Morphism<int, string> intToString = x => x.ToString();

        // A morphism that returns string should be assignable where object output is expected
        // (covariance on output)
        Morphism<int, object> covariant = intToString;
        covariant(42).Should().Be("42");
    }

    [Fact]
    public void Morphism_WithReferenceTypes_Works()
    {
        Morphism<string, int> length = s => s.Length;
        length("hello").Should().Be(5);
    }

    [Fact]
    public void Morphism_IsDelegate_CanBeInvokedDirectly()
    {
        Morphism<int, bool> isEven = x => x % 2 == 0;

        isEven(4).Should().BeTrue();
        isEven(3).Should().BeFalse();
    }
}
