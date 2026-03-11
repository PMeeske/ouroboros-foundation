namespace Ouroboros.Core.Tests.Monads;

[Trait("Category", "Unit")]
public sealed class KleisliSetExtensionsAdditionalTests
{
    #region Then with multiple results from second

    [Fact]
    public void Then_MultipleResultsFromSecond_FlatMapsAll()
    {
        KleisliSet<int, int> f = x => new[] { x };
        KleisliSet<int, int> g = x => new[] { x, x + 10, x + 20 };

        var composed = f.Then(g);
        var results = composed(5).ToList();

        results.Should().BeEquivalentTo(new[] { 5, 15, 25 });
    }

    #endregion

    #region SelectMany edge cases

    [Fact]
    public void SelectMany_WithSelector_EmptyFirst_ReturnsEmpty()
    {
        KleisliSet<int, int> f = _ => Enumerable.Empty<int>();

        var composed = f.SelectMany(mid =>
            (KleisliSet<int, int>)(x => new[] { x * 10 }));
        var results = composed(3).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void SelectMany_WithCollectionAndResultSelector_EmptyFirst_ReturnsEmpty()
    {
        KleisliSet<int, int> f = _ => Enumerable.Empty<int>();

        var composed = f.SelectMany(
            mid => new[] { mid * 10 },
            (mid, result) => result);
        var results = composed(3).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void SelectMany_WithCollectionAndResultSelector_UsesProjection()
    {
        KleisliSet<int, int> f = x => new[] { x, x + 1 };

        var composed = f.SelectMany(
            mid => new[] { mid * 10 },
            (mid, result) => mid + result);
        var results = composed(3).ToList();

        // mid=3, result=30 => 33; mid=4, result=40 => 44
        results.Should().BeEquivalentTo(new[] { 33, 44 });
    }

    #endregion

    #region Union with empty

    [Fact]
    public void Union_WithEmpty_ReturnsFirstOnly()
    {
        KleisliSet<int, int> f = _ => new[] { 1, 2, 3 };
        KleisliSet<int, int> g = _ => Enumerable.Empty<int>();

        var combined = f.Union(g);
        var results = combined(0).ToList();

        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    #endregion

    #region Intersect with full overlap

    [Fact]
    public void Intersect_FullOverlap_ReturnsAll()
    {
        KleisliSet<int, int> f = _ => new[] { 1, 2, 3 };
        KleisliSet<int, int> g = _ => new[] { 1, 2, 3 };

        var intersection = f.Intersect(g);
        var results = intersection(0).ToList();

        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    #endregion

    #region Except with full overlap

    [Fact]
    public void Except_FullOverlap_ReturnsEmpty()
    {
        KleisliSet<int, int> f = _ => new[] { 1, 2, 3 };
        KleisliSet<int, int> g = _ => new[] { 1, 2, 3 };

        var diff = f.Except(g);
        var results = diff(0).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region Where with all matching

    [Fact]
    public void Where_AllMatch_ReturnsAll()
    {
        KleisliSet<int, int> source = n => Enumerable.Range(1, n);
        var filtered = source.Where<int, int>(_ => true);

        var results = filtered(3).ToList();
        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    #endregion

    #region Distinct with empty

    [Fact]
    public void Distinct_Empty_ReturnsEmpty()
    {
        KleisliSet<int, int> empty = _ => Enumerable.Empty<int>();
        var distinct = empty.Distinct();

        var results = distinct(0).ToList();
        results.Should().BeEmpty();
    }

    #endregion

    #region Map with type change

    [Fact]
    public void Map_ChangesType_TransformsCorrectly()
    {
        KleisliSet<int, int> source = n => new[] { n, n * 2 };
        var mapped = source.Map<int, int, bool>(x => x > 5);

        var results = mapped(3).ToList();
        results.Should().BeEquivalentTo(new[] { false, true }); // 3 > 5 = false, 6 > 5 = true
    }

    #endregion

    #region Lift with identity

    [Fact]
    public void Lift_IdentityFunction_ReturnsInput()
    {
        var identity = KleisliSetExtensions.Lift<int, int>(x => x);

        var results = identity(42).ToList();
        results.Should().ContainSingle().Which.Should().Be(42);
    }

    #endregion
}
