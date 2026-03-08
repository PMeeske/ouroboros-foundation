using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class AtomSpaceTests
{
    private readonly AtomSpace _sut = new();

    [Fact]
    public void Add_NewAtom_ReturnsTrue()
    {
        var sym = Atom.Sym("test");

        _sut.Add(sym).Should().BeTrue();
    }

    [Fact]
    public void Add_DuplicateAtom_ReturnsFalse()
    {
        var sym = Atom.Sym("test");
        _sut.Add(sym);

        _sut.Add(sym).Should().BeFalse();
    }

    [Fact]
    public void Count_AfterAdding_ReflectsCorrectCount()
    {
        _sut.Add(Atom.Sym("a"));
        _sut.Add(Atom.Sym("b"));
        _sut.Add(Atom.Sym("c"));

        _sut.Count.Should().Be(3);
    }

    [Fact]
    public void Contains_AddedAtom_ReturnsTrue()
    {
        var sym = Atom.Sym("test");
        _sut.Add(sym);

        _sut.Contains(sym).Should().BeTrue();
    }

    [Fact]
    public void Contains_NotAddedAtom_ReturnsFalse()
    {
        _sut.Contains(Atom.Sym("missing")).Should().BeFalse();
    }

    [Fact]
    public void Remove_ExistingAtom_ReturnsTrue()
    {
        var sym = Atom.Sym("test");
        _sut.Add(sym);

        _sut.Remove(sym).Should().BeTrue();
        _sut.Contains(sym).Should().BeFalse();
    }

    [Fact]
    public void Remove_NonExistingAtom_ReturnsFalse()
    {
        _sut.Remove(Atom.Sym("missing")).Should().BeFalse();
    }

    [Fact]
    public void AddRange_MultipleAtoms_ReturnsCountAdded()
    {
        var atoms = new Atom[] { Atom.Sym("a"), Atom.Sym("b"), Atom.Sym("c") };

        int count = _sut.AddRange(atoms);

        count.Should().Be(3);
        _sut.Count.Should().Be(3);
    }

    [Fact]
    public void AddRange_WithDuplicates_ReturnsUniqueCount()
    {
        var atoms = new Atom[] { Atom.Sym("a"), Atom.Sym("a"), Atom.Sym("b") };

        int count = _sut.AddRange(atoms);

        count.Should().Be(2);
    }

    [Fact]
    public void All_ReturnsAllAddedAtoms()
    {
        _sut.Add(Atom.Sym("x"));
        _sut.Add(Atom.Sym("y"));

        var all = _sut.All().ToList();

        all.Should().HaveCount(2);
    }

    [Fact]
    public void Clear_RemovesAllAtoms()
    {
        _sut.Add(Atom.Sym("a"));
        _sut.Add(Atom.Sym("b"));

        _sut.Clear();

        _sut.Count.Should().Be(0);
    }

    [Fact]
    public void Snapshot_ReturnsImmutableList()
    {
        _sut.Add(Atom.Sym("x"));
        _sut.Add(Atom.Sym("y"));

        var snapshot = _sut.Snapshot();

        snapshot.Should().HaveCount(2);
    }

    // --- Query tests ---

    [Fact]
    public void Query_GroundAtomPresent_ReturnsMatch()
    {
        _sut.Add(Atom.Sym("fact"));

        var results = _sut.Query(Atom.Sym("fact")).ToList();

        results.Should().HaveCount(1);
        results[0].Bindings.Should().Be(Substitution.Empty);
    }

    [Fact]
    public void Query_GroundAtomAbsent_ReturnsEmpty()
    {
        var results = _sut.Query(Atom.Sym("missing")).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Query_VariablePattern_MatchesAndReturnsBindings()
    {
        _sut.Add(Atom.Expr(Atom.Sym("color"), Atom.Sym("red")));
        _sut.Add(Atom.Expr(Atom.Sym("color"), Atom.Sym("blue")));

        var pattern = Atom.Expr(Atom.Sym("color"), Atom.Var("c"));
        var results = _sut.Query(pattern).ToList();

        results.Should().HaveCount(2);
    }

    [Fact]
    public void Query_ExpressionWithIndex_UsesSymbolIndex()
    {
        // Add expressions with different head symbols
        _sut.Add(Atom.Expr(Atom.Sym("person"), Atom.Sym("alice")));
        _sut.Add(Atom.Expr(Atom.Sym("animal"), Atom.Sym("dog")));

        var pattern = Atom.Expr(Atom.Sym("person"), Atom.Var("name"));
        var results = _sut.Query(pattern).ToList();

        results.Should().HaveCount(1);
    }

    [Fact]
    public void Add_Expression_IndexedByHeadSymbol()
    {
        var expr = Atom.Expr(Atom.Sym("parent"), Atom.Sym("alice"), Atom.Sym("bob"));
        _sut.Add(expr);

        // Should be findable via pattern query
        var pattern = Atom.Expr(Atom.Sym("parent"), Atom.Var("p"), Atom.Var("c"));
        var results = _sut.Query(pattern).ToList();

        results.Should().HaveCount(1);
    }
}
