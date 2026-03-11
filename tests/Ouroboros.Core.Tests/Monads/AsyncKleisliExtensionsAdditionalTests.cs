namespace Ouroboros.Core.Tests.Monads;

[Trait("Category", "Unit")]
public sealed class AsyncKleisliExtensionsAdditionalTests
{
    #region SelectMany (monadic bind)

    [Fact]
    public async Task SelectMany_WithSelector_ComposesArrows()
    {
        AsyncKleisli<int, int> f = x => ToAsyncEnumerable(new[] { x, x + 1 });

        var composed = f.SelectMany(mid =>
            (AsyncKleisli<int, int>)(x => ToAsyncEnumerable(new[] { x * 10 })));
        var results = await CollectAsync(composed(3));

        results.Should().BeEquivalentTo(new[] { 30, 40 });
    }

    #endregion

    #region SelectMany with result selector

    [Fact]
    public async Task SelectMany_WithCollectionAndResultSelector_Projects()
    {
        AsyncKleisli<int, int> f = x => ToAsyncEnumerable(new[] { x, x + 1 });

        var composed = f.SelectMany(
            mid => ToAsyncEnumerable(new[] { mid * 10 }),
            (mid, result) => result);
        var results = await CollectAsync(composed(3));

        results.Should().BeEquivalentTo(new[] { 30, 40 });
    }

    [Fact]
    public async Task SelectMany_WithResultSelector_UsesProjection()
    {
        AsyncKleisli<int, int> f = x => ToAsyncEnumerable(new[] { x, x + 1 });

        var composed = f.SelectMany(
            mid => ToAsyncEnumerable(new[] { mid * 10 }),
            (mid, result) => mid + result);
        var results = await CollectAsync(composed(3));

        // mid=3, result=30 => 33; mid=4, result=40 => 44
        results.Should().BeEquivalentTo(new[] { 33, 44 });
    }

    #endregion

    #region Map with empty

    [Fact]
    public async Task Map_EmptySource_ReturnsEmpty()
    {
        AsyncKleisli<int, int> empty = _ => ToAsyncEnumerable(Array.Empty<int>());
        var mapped = empty.Map<int, int, string>(x => x.ToString());

        var results = await CollectAsync(mapped(1));
        results.Should().BeEmpty();
    }

    #endregion

    #region MapAsync

    [Fact]
    public async Task MapAsync_MultipleItems_TransformsEach()
    {
        AsyncKleisli<int, int> source = x => ToAsyncEnumerable(new[] { x, x + 1, x + 2 });
        var mapped = source.MapAsync<int, int, string>(async n =>
        {
            await Task.Yield();
            return $"v:{n}";
        });

        var results = await CollectAsync(mapped(10));
        results.Should().BeEquivalentTo(new[] { "v:10", "v:11", "v:12" });
    }

    #endregion

    #region Where with all matching

    [Fact]
    public async Task Where_AllMatch_ReturnsAll()
    {
        AsyncKleisli<int, int> source = x => ToAsyncEnumerable(new[] { 1, 2, 3 });
        var filtered = source.Where<int, int>(_ => true);

        var results = await CollectAsync(filtered(0));
        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task Where_NoneMatch_ReturnsEmpty()
    {
        AsyncKleisli<int, int> source = x => ToAsyncEnumerable(new[] { 1, 2, 3 });
        var filtered = source.Where<int, int>(_ => false);

        var results = await CollectAsync(filtered(0));
        results.Should().BeEmpty();
    }

    #endregion

    #region WhereAsync edge cases

    [Fact]
    public async Task WhereAsync_NoneMatch_ReturnsEmpty()
    {
        AsyncKleisli<int, int> source = x => ToAsyncEnumerable(new[] { 1, 2, 3 });
        var filtered = source.WhereAsync<int, int>(async n =>
        {
            await Task.Yield();
            return false;
        });

        var results = await CollectAsync(filtered(0));
        results.Should().BeEmpty();
    }

    #endregion

    #region Take edge cases

    [Fact]
    public async Task Take_MoreThanAvailable_ReturnsAll()
    {
        AsyncKleisli<int, int> source = x => ToAsyncEnumerable(new[] { 1, 2, 3 });
        var limited = source.Take<int, int>(100);

        var results = await CollectAsync(limited(0));
        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    #endregion

    #region Distinct with all unique

    [Fact]
    public async Task Distinct_AllUnique_ReturnsAll()
    {
        AsyncKleisli<int, int> source = x => ToAsyncEnumerable(new[] { 1, 2, 3 });
        var distinct = source.Distinct<int, int>();

        var results = await CollectAsync(distinct(0));
        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task Distinct_Empty_ReturnsEmpty()
    {
        AsyncKleisli<int, int> source = _ => ToAsyncEnumerable(Array.Empty<int>());
        var distinct = source.Distinct<int, int>();

        var results = await CollectAsync(distinct(0));
        results.Should().BeEmpty();
    }

    #endregion

    #region Union edge cases

    [Fact]
    public async Task Union_WithEmpty_ReturnsFirstOnly()
    {
        AsyncKleisli<int, int> f = x => ToAsyncEnumerable(new[] { 1, 2 });
        AsyncKleisli<int, int> g = _ => ToAsyncEnumerable(Array.Empty<int>());

        var merged = f.Union(g);
        var results = await CollectAsync(merged(0));

        results.Should().Contain(new[] { 1, 2 });
    }

    [Fact]
    public async Task Union_BothEmpty_ReturnsEmpty()
    {
        AsyncKleisli<int, int> f = _ => ToAsyncEnumerable(Array.Empty<int>());
        AsyncKleisli<int, int> g = _ => ToAsyncEnumerable(Array.Empty<int>());

        var merged = f.Union(g);
        var results = await CollectAsync(merged(0));

        results.Should().BeEmpty();
    }

    #endregion

    #region Then with empty source

    [Fact]
    public async Task Then_EmptyFirstArrow_ProducesEmpty()
    {
        AsyncKleisli<int, int> empty = _ => ToAsyncEnumerable(Array.Empty<int>());
        AsyncKleisli<int, string> toString = x => ToAsyncEnumerable(new[] { x.ToString() });

        var composed = empty.Then(toString);
        var results = await CollectAsync(composed(5));

        results.Should().BeEmpty();
    }

    #endregion

    #region LiftMany

    [Fact]
    public async Task LiftMany_EmptyEnumerable_ReturnsEmpty()
    {
        var arrow = AsyncKleisliExtensions.LiftMany<int, int>(_ =>
            ToAsyncEnumerable(Array.Empty<int>()));

        var results = await CollectAsync(arrow(5));
        results.Should().BeEmpty();
    }

    #endregion

    #region Helpers

    private static async Task<List<T>> CollectAsync<T>(IAsyncEnumerable<T> source)
    {
        var items = new List<T>();
        await foreach (var item in source)
        {
            items.Add(item);
        }
        return items;
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
        }
    }

    #endregion
}
