using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class VariableTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var variable = new Variable("x");

        variable.Name.Should().Be("x");
    }

    [Fact]
    public void ToSExpr_ReturnsDollarPrefixedName()
    {
        var variable = new Variable("person");

        variable.ToSExpr().Should().Be("$person");
    }

    [Fact]
    public void ContainsVariables_ReturnsTrue()
    {
        var variable = new Variable("y");

        variable.ContainsVariables().Should().BeTrue();
    }

    [Fact]
    public void ToString_ReturnsSExpr()
    {
        var variable = new Variable("x");

        variable.ToString().Should().Be("$x");
    }

    [Fact]
    public void Equality_SameNameVariables_AreEqual()
    {
        var a = new Variable("x");
        var b = new Variable("x");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentNameVariables_AreNotEqual()
    {
        var a = new Variable("x");
        var b = new Variable("y");

        a.Should().NotBe(b);
    }

    [Fact]
    public void GetHashCode_SameNameVariables_SameHashCode()
    {
        var a = new Variable("x");
        var b = new Variable("x");

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Var_FactoryMethod_CreatesVariable()
    {
        Variable variable = Atom.Var("name");

        variable.Name.Should().Be("name");
    }

    [Fact]
    public void Variable_IsAtom()
    {
        Atom atom = new Variable("x");

        atom.Should().BeOfType<Variable>();
        atom.ContainsVariables().Should().BeTrue();
    }
}
