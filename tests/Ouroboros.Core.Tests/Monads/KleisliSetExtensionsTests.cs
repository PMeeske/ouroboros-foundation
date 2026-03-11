using Ouroboros.Core.Monads;

namespace Ouroboros.Core.Tests.Monads;

[Trait("Category", "Unit")]
public class KleisliSetExtensionsTests
{
    #region Identity

    [Fact]
    public void Identity_ReturnsSingletonSequence()
    {
        var identity = KleisliSetExtensions.Identity<int>();

        var results = identity(42).ToList();

        results.Should().ContainSingle().Which.Should().Be(42);
    }

    [Fact]
    public void Identity_WithString_ReturnsSingleton()
    {
        var identity = KleisliSetExtensions.Identity<string>();

        var results = identity("hello").ToList();

        results.Should().ContainSingle().Which.Should().Be("hello");
    }

    #endregion

    #region Lift

    [Fact]
    public void Lift_PureFunction_WrapsInSingleton()
    {
        var doubled = KleisliSetExtensions.Lift<int, int>(x => x * 2);

        var results = doubled(5).ToList();

        results.Should().ContainSingle().Which.Should().Be(10);
    }

    [Fact]
    public void Lift_StringTransformation_Works()
    {
        var toUpper = KleisliSetExtensions.Lift<string, string>(s => s.ToUpperInvariant());

        var results = toUpper("hello").ToList();

        results.Should().ContainSingle().Which.Should().Be("HELLO");
    }

    #endregion

    #region LiftMany

    [Fact]
    public void LiftMany_EnumerableFunction_ReturnsAllElements()
    {
        var range = KleisliSetExtensions.LiftMany<int, int>(n => Enumerable.Range(0, n));

        var results = range(3).ToList();

        results.Should().BeEquivalentTo(new[] { 0, 1, 2 });
    }

    [Fact]
    public void LiftMany_EmptyEnumerable_ReturnsEmpty()
    {
        var empty = KleisliSetExtensions.LiftMany<int, int>(_ => Enumerable.Empty<int>());

        var results = empty(5).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region Then (Kleisli composition)

    [Fact]
    public void Then_ComposesArrows()
    {
        KleisliSet<int, int> doubleIt = x => new[] { x * 2 };
        KleisliSet<int, string> toString = x => new[] { x.ToString() };

        var composed = doubleIt.Then(toString);
        var results = composed(5).ToList();

        results.Should().ContainSingle().Which.Should().Be("10");
    }

    [Fact]
    public void Then_FlatMapsResults()
    {
        KleisliSet<int, int> expand = x => new[] { x, x + 1 };
        KleisliSet<int, int> doubleIt = x => new[] { x * 2 };

        var composed = expand.Then(doubleIt);
        var results = composed(3).ToList();

        results.Should().BeEquivalentTo(new[] { 6, 8 });
    }

    [Fact]
    public void Then_EmptyFirstArrow_ProducesEmpty()
    {
        KleisliSet<int, int> empty = _ => Enumerable.Empty<int>();
        KleisliSet<int, string> toString = x => new[] { x.ToString() };

        var composed = empty.Then(toString);
        var results = composed(5).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region Map

    [Fact]
    public void Map_TransformsAllResults()
    {
        KleisliSet<int, int> range = n => Enumerable.Range(1, n);

        var mapped = range.Map<int, int, string>(x => $"item{x}");
        var results = mapped(3).ToList();

        results.Should().BeEquivalentTo(new[] { "item1", "item2", "item3" });
    }

    [Fact]
    public void Map_EmptySource_ReturnsEmpty()
    {
        KleisliSet<int, int> empty = _ => Enumerable.Empty<int>();

        var mapped = empty.Map<int, int, string>(x => x.ToString());
        var results = mapped(5).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region Union

    [Fact]
    public void Union_CombinesResultsFromBothArrows()
    {
        KleisliSet<int, int> evens = x => new[] { x * 2 };
        KleisliSet<int, int> odds = x => new[] { x * 2 + 1 };

        var combined = evens.Union(odds);
        var results = combined(3).ToList();

        results.Should().Contain(6);
        results.Should().Contain(7);
    }

    [Fact]
    public void Union_DeduplicatesOverlappingResults()
    {
        KleisliSet<int, int> f = _ => new[] { 1, 2, 3 };
        KleisliSet<int, int> g = _ => new[] { 2, 3, 4 };

        var combined = f.Union(g);
        var results = combined(0).ToList();

        results.Should().BeEquivalentTo(new[] { 1, 2, 3, 4 });
    }

    #endregion

    #region Intersect

    [Fact]
    public void Intersect_ReturnsCommonResults()
    {
        KleisliSet<int, int> f = _ => new[] { 1, 2, 3 };
        KleisliSet<int, int> g = _ => new[] { 2, 3, 4 };

        var intersection = f.Intersect(g);
        var results = intersection(0).ToList();

        results.Should().BeEquivalentTo(new[] { 2, 3 });
    }

    [Fact]
    public void Intersect_NoOverlap_ReturnsEmpty()
    {
        KleisliSet<int, int> f = _ => new[] { 1, 2 };
        KleisliSet<int, int> g = _ => new[] { 3, 4 };

        var intersection = f.Intersect(g);
        var results = intersection(0).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region Except

    [Fact]
    public void Except_RemovesSecondFromFirst()
    {
        KleisliSet<int, int> f = _ => new[] { 1, 2, 3 };
        KleisliSet<int, int> g = _ => new[] { 2, 3, 4 };

        var diff = f.Except(g);
        var results = diff(0).ToList();

        results.Should().BeEquivalentTo(new[] { 1 });
    }

    [Fact]
    public void Except_NoOverlap_ReturnsAll()
    {
        KleisliSet<int, int> f = _ => new[] { 1, 2 };
        KleisliSet<int, int> g = _ => new[] { 3, 4 };

        var diff = f.Except(g);
        var results = diff(0).ToList();

        results.Should().BeEquivalentTo(new[] { 1, 2 });
    }

    #endregion

    #region Where

    [Fact]
    public void Where_FiltersResults()
    {
        KleisliSet<int, int> range = n => Enumerable.Range(1, n);

        var filtered = range.Where<int, int>(x => x % 2 == 0);
        var results = filtered(6).ToList();

        results.Should().BeEquivalentTo(new[] { 2, 4, 6 });
    }

    [Fact]
    public void Where_AllFiltered_ReturnsEmpty()
    {
        KleisliSet<int, int> range = n => Enumerable.Range(1, n);

        var filtered = range.Where<int, int>(_ => false);
        var results = filtered(3).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region Distinct

    [Fact]
    public void Distinct_RemovesDuplicates()
    {
        KleisliSet<int, int> withDups = _ => new[] { 1, 2, 2, 3, 3, 3 };

        var distinct = withDups.Distinct();
        var results = distinct(0).ToList();

        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Distinct_NoDuplicates_ReturnsAll()
    {
        KleisliSet<int, int> unique = _ => new[] { 1, 2, 3 };

        var distinct = unique.Distinct();
        var results = distinct(0).ToList();

        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    #endregion

    #region SelectMany (LINQ support)

    [Fact]
    public void SelectMany_WithSelector_ComposesArrows()
    {
        KleisliSet<int, int> f = x => new[] { x, x + 1 };

        var composed = f.SelectMany(mid => (KleisliSet<int, int>)(x => new[] { x * 10 }));
        var results = composed(3).ToList();

        results.Should().BeEquivalentTo(new[] { 30, 40 });
    }

    [Fact]
    public void SelectMany_WithCollectionAndResultSelector_Projects()
    {
        KleisliSet<int, int> f = x => new[] { x, x + 1 };

        var composed = f.SelectMany(
            mid => new[] { mid * 10 },
            (mid, result) => result);
        var results = composed(3).ToList();

        results.Should().BeEquivalentTo(new[] { 30, 40 });
    }

    #endregion
}
