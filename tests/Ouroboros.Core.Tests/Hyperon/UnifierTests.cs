using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class UnifierTests
{
    #region Unify - Identical atoms

    [Fact]
    public void Unify_IdenticalSymbols_ReturnsEmptySubstitution()
    {
        var a = Atom.Sym("Hello");
        var b = Atom.Sym("Hello");

        var result = Unifier.Unify(a, b);

        result.Should().NotBeNull();
        result!.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Unify_DifferentSymbols_ReturnsNull()
    {
        var a = Atom.Sym("Hello");
        var b = Atom.Sym("World");

        var result = Unifier.Unify(a, b);

        result.Should().BeNull();
    }

    [Fact]
    public void Unify_IdenticalExpressions_ReturnsEmptySubstitution()
    {
        var a = Atom.Expr(Atom.Sym("f"), Atom.Sym("x"));
        var b = Atom.Expr(Atom.Sym("f"), Atom.Sym("x"));

        var result = Unifier.Unify(a, b);

        result.Should().NotBeNull();
        result!.IsEmpty.Should().BeTrue();
    }

    #endregion

    #region Unify - Variable binding

    [Fact]
    public void Unify_PatternVariableWithSymbol_BindsVariable()
    {
        var pattern = Atom.Var("x");
        var target = Atom.Sym("Hello");

        var result = Unifier.Unify(pattern, target);

        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
        var lookup = result.Lookup("x");
        lookup.HasValue.Should().BeTrue();
        lookup.Value.Should().Be(Atom.Sym("Hello"));
    }

    [Fact]
    public void Unify_TargetVariableWithSymbol_BindsVariable()
    {
        var pattern = Atom.Sym("Hello");
        var target = Atom.Var("y");

        var result = Unifier.Unify(pattern, target);

        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
        var lookup = result.Lookup("y");
        lookup.HasValue.Should().BeTrue();
        lookup.Value.Should().Be(Atom.Sym("Hello"));
    }

    [Fact]
    public void Unify_TwoVariables_BindsOneToOther()
    {
        var a = Atom.Var("x");
        var b = Atom.Var("y");

        var result = Unifier.Unify(a, b);

        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
    }

    [Fact]
    public void Unify_VariableWithExpression_BindsVariable()
    {
        var pattern = Atom.Var("x");
        var target = Atom.Expr(Atom.Sym("f"), Atom.Sym("a"));

        var result = Unifier.Unify(pattern, target);

        result.Should().NotBeNull();
        var lookup = result!.Lookup("x");
        lookup.HasValue.Should().BeTrue();
        lookup.Value.Should().Be(target);
    }

    #endregion

    #region Unify - Expression unification

    [Fact]
    public void Unify_ExpressionWithVariableInPattern_BindsVariableFromExpression()
    {
        var pattern = Atom.Expr(Atom.Sym("f"), Atom.Var("x"));
        var target = Atom.Expr(Atom.Sym("f"), Atom.Sym("a"));

        var result = Unifier.Unify(pattern, target);

        result.Should().NotBeNull();
        result!.Lookup("x").Value.Should().Be(Atom.Sym("a"));
    }

    [Fact]
    public void Unify_ExpressionWithMultipleVariables_BindsAll()
    {
        var pattern = Atom.Expr(Atom.Sym("f"), Atom.Var("x"), Atom.Var("y"));
        var target = Atom.Expr(Atom.Sym("f"), Atom.Sym("a"), Atom.Sym("b"));

        var result = Unifier.Unify(pattern, target);

        result.Should().NotBeNull();
        result!.Count.Should().Be(2);
        result.Lookup("x").Value.Should().Be(Atom.Sym("a"));
        result.Lookup("y").Value.Should().Be(Atom.Sym("b"));
    }

    [Fact]
    public void Unify_ExpressionsDifferentLength_ReturnsNull()
    {
        var pattern = Atom.Expr(Atom.Sym("f"), Atom.Var("x"));
        var target = Atom.Expr(Atom.Sym("f"), Atom.Sym("a"), Atom.Sym("b"));

        var result = Unifier.Unify(pattern, target);

        result.Should().BeNull();
    }

    [Fact]
    public void Unify_NestedExpressions_BindsCorrectly()
    {
        var pattern = Atom.Expr(Atom.Sym("f"), Atom.Expr(Atom.Sym("g"), Atom.Var("x")));
        var target = Atom.Expr(Atom.Sym("f"), Atom.Expr(Atom.Sym("g"), Atom.Sym("a")));

        var result = Unifier.Unify(pattern, target);

        result.Should().NotBeNull();
        result!.Lookup("x").Value.Should().Be(Atom.Sym("a"));
    }

    [Fact]
    public void Unify_ExpressionWithDifferentHeadSymbol_ReturnsNull()
    {
        var pattern = Atom.Expr(Atom.Sym("f"), Atom.Var("x"));
        var target = Atom.Expr(Atom.Sym("g"), Atom.Sym("a"));

        var result = Unifier.Unify(pattern, target);

        result.Should().BeNull();
    }

    #endregion

    #region Unify - Occurs check

    [Fact]
    public void Unify_OccursCheck_PreventsCyclicBinding()
    {
        // $x unify with (f $x) should fail (occurs check)
        var pattern = Atom.Var("x");
        var target = Atom.Expr(Atom.Sym("f"), Atom.Var("x"));

        var result = Unifier.Unify(pattern, target);

        result.Should().BeNull();
    }

    [Fact]
    public void Unify_OccursCheckNested_PreventsCyclicBinding()
    {
        // $x unify with (f (g $x)) should fail
        var pattern = Atom.Var("x");
        var target = Atom.Expr(Atom.Sym("f"), Atom.Expr(Atom.Sym("g"), Atom.Var("x")));

        var result = Unifier.Unify(pattern, target);

        result.Should().BeNull();
    }

    #endregion

    #region Unify - Consistent bindings

    [Fact]
    public void Unify_SameVariableUsedTwice_ConsistentBinding_Succeeds()
    {
        // (f $x $x) unify with (f a a)
        var pattern = Atom.Expr(Atom.Sym("f"), Atom.Var("x"), Atom.Var("x"));
        var target = Atom.Expr(Atom.Sym("f"), Atom.Sym("a"), Atom.Sym("a"));

        var result = Unifier.Unify(pattern, target);

        result.Should().NotBeNull();
        result!.Lookup("x").Value.Should().Be(Atom.Sym("a"));
    }

    [Fact]
    public void Unify_SameVariableUsedTwice_InconsistentBinding_Fails()
    {
        // (f $x $x) unify with (f a b) - should fail since $x can't be both a and b
        var pattern = Atom.Expr(Atom.Sym("f"), Atom.Var("x"), Atom.Var("x"));
        var target = Atom.Expr(Atom.Sym("f"), Atom.Sym("a"), Atom.Sym("b"));

        var result = Unifier.Unify(pattern, target);

        result.Should().BeNull();
    }

    #endregion

    #region Unify - Initial substitution

    [Fact]
    public void Unify_WithInitialSubstitution_ExtendsIt()
    {
        var initial = Substitution.Of("y", Atom.Sym("b"));
        var pattern = Atom.Var("x");
        var target = Atom.Sym("a");

        var result = Unifier.Unify(pattern, target, initial);

        result.Should().NotBeNull();
        result!.Lookup("x").Value.Should().Be(Atom.Sym("a"));
        result.Lookup("y").Value.Should().Be(Atom.Sym("b"));
    }

    [Fact]
    public void Unify_WithInitialSubstitution_AppliesExistingBindings()
    {
        // Initial: $x -> a. Unify $x with a should succeed.
        var initial = Substitution.Of("x", Atom.Sym("a"));
        var pattern = Atom.Var("x");
        var target = Atom.Sym("a");

        var result = Unifier.Unify(pattern, target, initial);

        result.Should().NotBeNull();
    }

    #endregion

    #region Unify - Symbol vs Expression

    [Fact]
    public void Unify_SymbolWithExpression_ReturnsNull()
    {
        var pattern = Atom.Sym("a");
        var target = Atom.Expr(Atom.Sym("f"), Atom.Sym("x"));

        var result = Unifier.Unify(pattern, target);

        result.Should().BeNull();
    }

    #endregion

    #region UnifyAll

    [Fact]
    public void UnifyAll_MatchesMultipleAtoms()
    {
        var pattern = Atom.Expr(Atom.Sym("Human"), Atom.Var("x"));
        var atoms = new Atom[]
        {
            Atom.Expr(Atom.Sym("Human"), Atom.Sym("Socrates")),
            Atom.Expr(Atom.Sym("Human"), Atom.Sym("Plato")),
            Atom.Expr(Atom.Sym("Dog"), Atom.Sym("Fido")),
        };

        var results = Unifier.UnifyAll(pattern, atoms).ToList();

        results.Should().HaveCount(2);
        results[0].Lookup("x").Value.Should().Be(Atom.Sym("Socrates"));
        results[1].Lookup("x").Value.Should().Be(Atom.Sym("Plato"));
    }

    [Fact]
    public void UnifyAll_NoMatches_ReturnsEmpty()
    {
        var pattern = Atom.Expr(Atom.Sym("Human"), Atom.Var("x"));
        var atoms = new Atom[]
        {
            Atom.Expr(Atom.Sym("Dog"), Atom.Sym("Fido")),
        };

        var results = Unifier.UnifyAll(pattern, atoms).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void UnifyAll_EmptyCollection_ReturnsEmpty()
    {
        var pattern = Atom.Var("x");
        var atoms = Array.Empty<Atom>();

        var results = Unifier.UnifyAll(pattern, atoms).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region CanUnify

    [Fact]
    public void CanUnify_MatchingAtoms_ReturnsTrue()
    {
        var a = Atom.Var("x");
        var b = Atom.Sym("Hello");

        Unifier.CanUnify(a, b).Should().BeTrue();
    }

    [Fact]
    public void CanUnify_NonMatchingAtoms_ReturnsFalse()
    {
        var a = Atom.Sym("Hello");
        var b = Atom.Sym("World");

        Unifier.CanUnify(a, b).Should().BeFalse();
    }

    [Fact]
    public void CanUnify_IdenticalSymbols_ReturnsTrue()
    {
        var a = Atom.Sym("same");
        var b = Atom.Sym("same");

        Unifier.CanUnify(a, b).Should().BeTrue();
    }

    [Fact]
    public void CanUnify_OccursCheckFails_ReturnsFalse()
    {
        var a = Atom.Var("x");
        var b = Atom.Expr(Atom.Sym("f"), Atom.Var("x"));

        Unifier.CanUnify(a, b).Should().BeFalse();
    }

    #endregion

    #region Edge cases

    [Fact]
    public void Unify_EmptyExpressions_Succeeds()
    {
        var a = Atom.Expr();
        var b = Atom.Expr();

        var result = Unifier.Unify(a, b);

        result.Should().NotBeNull();
        result!.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Unify_IdenticalVariables_Succeeds()
    {
        var a = Atom.Var("x");
        var b = Atom.Var("x");

        var result = Unifier.Unify(a, b);

        result.Should().NotBeNull();
        result!.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Unify_BidirectionalVariableBinding()
    {
        // (f $x b) unify with (f a $y)
        var pattern = Atom.Expr(Atom.Sym("f"), Atom.Var("x"), Atom.Sym("b"));
        var target = Atom.Expr(Atom.Sym("f"), Atom.Sym("a"), Atom.Var("y"));

        var result = Unifier.Unify(pattern, target);

        result.Should().NotBeNull();
        result!.Lookup("x").Value.Should().Be(Atom.Sym("a"));
        result.Lookup("y").Value.Should().Be(Atom.Sym("b"));
    }

    #endregion
}
