using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class SymbolTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var symbol = new Symbol("Socrates");

        symbol.Name.Should().Be("Socrates");
    }

    [Fact]
    public void ToSExpr_ReturnsName()
    {
        var symbol = new Symbol("implies");

        symbol.ToSExpr().Should().Be("implies");
    }

    [Fact]
    public void ContainsVariables_ReturnsFalse()
    {
        var symbol = new Symbol("Human");

        symbol.ContainsVariables().Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsSExpr()
    {
        var symbol = new Symbol("Mortal");

        symbol.ToString().Should().Be("Mortal");
    }

    [Fact]
    public void Equality_SameNameSymbols_AreEqual()
    {
        var a = new Symbol("test");
        var b = new Symbol("test");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentNameSymbols_AreNotEqual()
    {
        var a = new Symbol("foo");
        var b = new Symbol("bar");

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameNameSymbols_SameHashCode()
    {
        var a = new Symbol("test");
        var b = new Symbol("test");

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Sym_FactoryMethod_CreatesSymbol()
    {
        Symbol symbol = Atom.Sym("hello");

        symbol.Name.Should().Be("hello");
    }

    [Fact]
    public void EmptyName_IsAllowed()
    {
        var symbol = new Symbol("");

        symbol.Name.Should().BeEmpty();
        symbol.ToSExpr().Should().BeEmpty();
    }

    [Fact]
    public void SpecialCharacters_InName_ArePreserved()
    {
        var symbol = new Symbol("⌐");

        symbol.Name.Should().Be("⌐");
        symbol.ToSExpr().Should().Be("⌐");
    }
}
