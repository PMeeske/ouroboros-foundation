using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class SubstitutionTests
{
    [Fact]
    public void Empty_HasNoBindings()
    {
        var sub = Substitution.Empty;

        sub.IsEmpty.Should().BeTrue();
        sub.Count.Should().Be(0);
    }

    [Fact]
    public void Of_CreatesSingleBinding()
    {
        var sub = Substitution.Of("x", Atom.Sym("Socrates"));

        sub.Count.Should().Be(1);
        sub.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Lookup_ExistingVariable_ReturnsSome()
    {
        var sub = Substitution.Of("x", Atom.Sym("Socrates"));

        var result = sub.Lookup("x");

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(Atom.Sym("Socrates"));
    }

    [Fact]
    public void Lookup_NonExistingVariable_ReturnsNone()
    {
        var sub = Substitution.Of("x", Atom.Sym("Socrates"));

        var result = sub.Lookup("y");

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Bind_AddsNewBinding()
    {
        var sub = Substitution.Empty
            .Bind("x", Atom.Sym("A"))
            .Bind("y", Atom.Sym("B"));

        sub.Count.Should().Be(2);
        sub.Lookup("x").Value.Should().Be(Atom.Sym("A"));
        sub.Lookup("y").Value.Should().Be(Atom.Sym("B"));
    }

    [Fact]
    public void Bind_OverwritesExistingBinding()
    {
        var sub = Substitution.Of("x", Atom.Sym("A"))
            .Bind("x", Atom.Sym("B"));

        sub.Count.Should().Be(1);
        sub.Lookup("x").Value.Should().Be(Atom.Sym("B"));
    }

    [Fact]
    public void Compose_NonConflicting_CombinesBindings()
    {
        var a = Substitution.Of("x", Atom.Sym("A"));
        var b = Substitution.Of("y", Atom.Sym("B"));

        var result = a.Compose(b);

        result.Should().NotBeNull();
        result!.Count.Should().Be(2);
        result.Lookup("x").Value.Should().Be(Atom.Sym("A"));
        result.Lookup("y").Value.Should().Be(Atom.Sym("B"));
    }

    [Fact]
    public void Compose_SameBindingsSameValue_Succeeds()
    {
        var a = Substitution.Of("x", Atom.Sym("A"));
        var b = Substitution.Of("x", Atom.Sym("A"));

        var result = a.Compose(b);

        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
    }

    [Fact]
    public void Compose_ConflictingBindings_ReturnsNull()
    {
        var a = Substitution.Of("x", Atom.Sym("A"));
        var b = Substitution.Of("x", Atom.Sym("B"));

        var result = a.Compose(b);

        result.Should().BeNull();
    }

    [Fact]
    public void Compose_EmptyWithNonEmpty_ReturnsNonEmpty()
    {
        var empty = Substitution.Empty;
        var nonEmpty = Substitution.Of("x", Atom.Sym("A"));

        var result = empty.Compose(nonEmpty);

        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
    }

    [Fact]
    public void Compose_NonEmptyWithEmpty_ReturnsSame()
    {
        var nonEmpty = Substitution.Of("x", Atom.Sym("A"));
        var empty = Substitution.Empty;

        var result = nonEmpty.Compose(empty);

        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
    }

    [Fact]
    public void Apply_Variable_ReplacesWithBoundValue()
    {
        var sub = Substitution.Of("x", Atom.Sym("Socrates"));

        var result = sub.Apply(Atom.Var("x"));

        result.Should().Be(Atom.Sym("Socrates"));
    }

    [Fact]
    public void Apply_UnboundVariable_ReturnsOriginal()
    {
        var sub = Substitution.Of("x", Atom.Sym("Socrates"));

        var result = sub.Apply(Atom.Var("y"));

        result.Should().Be(Atom.Var("y"));
    }

    [Fact]
    public void Apply_Symbol_ReturnsOriginal()
    {
        var sub = Substitution.Of("x", Atom.Sym("A"));

        var result = sub.Apply(Atom.Sym("test"));

        result.Should().Be(Atom.Sym("test"));
    }

    [Fact]
    public void Apply_Expression_ReplacesVariablesInChildren()
    {
        var sub = Substitution.Of("x", Atom.Sym("Socrates"));
        var expr = Atom.Expr(Atom.Sym("Human"), Atom.Var("x"));

        var result = sub.Apply(expr);

        result.Should().Be(Atom.Expr(Atom.Sym("Human"), Atom.Sym("Socrates")));
    }

    [Fact]
    public void Apply_NestedExpression_ReplacesDeep()
    {
        var sub = Substitution.Of("x", Atom.Sym("A"));
        var expr = Atom.Expr(Atom.Sym("f"), Atom.Expr(Atom.Sym("g"), Atom.Var("x")));

        var result = sub.Apply(expr);

        result.Should().Be(Atom.Expr(Atom.Sym("f"), Atom.Expr(Atom.Sym("g"), Atom.Sym("A"))));
    }

    [Fact]
    public void ToString_Empty_ReturnsBraces()
    {
        Substitution.Empty.ToString().Should().Be("{}");
    }

    [Fact]
    public void ToString_WithBindings_FormatsCorrectly()
    {
        var sub = Substitution.Of("x", Atom.Sym("A"));

        var str = sub.ToString();

        str.Should().Contain("$x");
        str.Should().Contain("A");
        str.Should().StartWith("{");
        str.Should().EndWith("}");
    }

    [Fact]
    public void Equality_SameBindings_AreEqual()
    {
        var a = Substitution.Of("x", Atom.Sym("A"));
        var b = Substitution.Of("x", Atom.Sym("A"));

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentBindings_AreNotEqual()
    {
        var a = Substitution.Of("x", Atom.Sym("A"));
        var b = Substitution.Of("x", Atom.Sym("B"));

        a.Should().NotBe(b);
    }

    [Fact]
    public void IsEmpty_AfterBinding_ReturnsFalse()
    {
        var sub = Substitution.Empty.Bind("x", Atom.Sym("A"));

        sub.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Apply_FormAtom_ReturnsOriginal()
    {
        var sub = Substitution.Of("x", Atom.Sym("A"));
        Atom formAtom = new Ouroboros.Core.Hyperon.FormAtom(Ouroboros.Core.LawsOfForm.Form.Mark);

        var result = sub.Apply(formAtom);

        result.Should().Be(formAtom);
    }
}
