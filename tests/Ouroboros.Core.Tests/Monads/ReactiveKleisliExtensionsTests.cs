using System.Reactive.Linq;
using Ouroboros.Core.Monads;

namespace Ouroboros.Core.Tests.Monads;

[Trait("Category", "Unit")]
public class ReactiveKleisliExtensionsTests
{
    #region Identity

    [Fact]
    public async Task Identity_ReturnsSingletonObservable()
    {
        var identity = ReactiveKleisliExtensions.Identity<int>();

        var results = await identity(42).ToList();

        results.Should().ContainSingle().Which.Should().Be(42);
    }

    #endregion

    #region Lift

    [Fact]
    public async Task Lift_PureFunction_WrapsInSingletonObservable()
    {
        var doubled = ReactiveKleisliExtensions.Lift<int, int>(x => x * 2);

        var results = await doubled(5).ToList();

        results.Should().ContainSingle().Which.Should().Be(10);
    }

    [Fact]
    public async Task Lift_StringTransformation_Works()
    {
        var toUpper = ReactiveKleisliExtensions.Lift<string, string>(s => s.ToUpperInvariant());

        var results = await toUpper("hello").ToList();

        results.Should().ContainSingle().Which.Should().Be("HELLO");
    }

    #endregion

    #region LiftAsync

    [Fact]
    public async Task LiftAsync_AsyncFunction_ReturnsResult()
    {
        var asyncArrow = ReactiveKleisliExtensions.LiftAsync<int, int>(
            async x => { await Task.Yield(); return x * 3; });

        var results = await asyncArrow(7).ToList();

        results.Should().ContainSingle().Which.Should().Be(21);
    }

    #endregion

    #region LiftObservable

    [Fact]
    public async Task LiftObservable_ReturnsAllItems()
    {
        var arrow = ReactiveKleisliExtensions.LiftObservable<int, int>(
            x => Observable.Range(x, 3));

        var results = await arrow(1).ToList();

        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    #endregion

    #region FromEnumerable

    [Fact]
    public async Task FromEnumerable_ConvertsEnumerableToObservable()
    {
        var arrow = ReactiveKleisliExtensions.FromEnumerable<int, int>(
            n => Enumerable.Range(0, n));

        var results = await arrow(3).ToList();

        results.Should().BeEquivalentTo(new[] { 0, 1, 2 });
    }

    #endregion

    #region Compose

    [Fact]
    public async Task Compose_ChainsArrows()
    {
        ReactiveKleisli<int, int> doubleIt = x => Observable.Return(x * 2);
        ReactiveKleisli<int, string> toString = x => Observable.Return(x.ToString());

        var composed = doubleIt.Compose(toString);
        var results = await composed(5).ToList();

        results.Should().ContainSingle().Which.Should().Be("10");
    }

    [Fact]
    public async Task Compose_FlatMapsResults()
    {
        ReactiveKleisli<int, int> expand = x => new[] { x, x + 1 }.ToObservable();
        ReactiveKleisli<int, int> doubleIt = x => Observable.Return(x * 2);

        var composed = expand.Compose(doubleIt);
        var results = await composed(3).ToList();

        results.Should().BeEquivalentTo(new[] { 6, 8 });
    }

    #endregion

    #region Then

    [Fact]
    public async Task Then_BehavesLikeCompose()
    {
        ReactiveKleisli<int, int> doubleIt = x => Observable.Return(x * 2);
        ReactiveKleisli<int, string> toString = x => Observable.Return(x.ToString());

        var composed = doubleIt.Then(toString);
        var results = await composed(5).ToList();

        results.Should().ContainSingle().Which.Should().Be("10");
    }

    #endregion

    #region Map

    [Fact]
    public async Task Map_TransformsResults()
    {
        ReactiveKleisli<int, int> range = n => Observable.Range(1, n);

        var mapped = range.Map<int, int, string>(x => $"item{x}");
        var results = await mapped(3).ToList();

        results.Should().BeEquivalentTo(new[] { "item1", "item2", "item3" });
    }

    #endregion

    #region Union / Merge

    [Fact]
    public async Task Union_MergesBothStreams()
    {
        ReactiveKleisli<int, int> f = x => Observable.Return(x);
        ReactiveKleisli<int, int> g = x => Observable.Return(x + 100);

        var merged = f.Union(g);
        var results = await merged(1).ToList();

        results.Should().HaveCount(2);
        results.Should().Contain(1);
        results.Should().Contain(101);
    }

    [Fact]
    public async Task Merge_BehavesLikeUnion()
    {
        ReactiveKleisli<int, int> f = x => Observable.Return(x);
        ReactiveKleisli<int, int> g = x => Observable.Return(x + 100);

        var merged = f.Merge(g);
        var results = await merged(1).ToList();

        results.Should().HaveCount(2);
        results.Should().Contain(1);
        results.Should().Contain(101);
    }

    #endregion

    #region Distinct

    [Fact]
    public async Task Distinct_RemovesDuplicates()
    {
        ReactiveKleisli<int, int> withDups = _ => new[] { 1, 2, 2, 3, 3, 3 }.ToObservable();

        var distinct = withDups.Distinct();
        var results = await distinct(0).ToList();

        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    #endregion

    #region DistinctUntilChanged

    [Fact]
    public async Task DistinctUntilChanged_RemovesConsecutiveDuplicates()
    {
        ReactiveKleisli<int, int> stream = _ => new[] { 1, 1, 2, 2, 1 }.ToObservable();

        var distinct = stream.DistinctUntilChanged();
        var results = await distinct(0).ToList();

        results.Should().BeEquivalentTo(new[] { 1, 2, 1 });
    }

    #endregion

    #region Where

    [Fact]
    public async Task Where_FiltersStream()
    {
        ReactiveKleisli<int, int> range = n => Observable.Range(1, n);

        var filtered = range.Where<int, int>(x => x % 2 == 0);
        var results = await filtered(6).ToList();

        results.Should().BeEquivalentTo(new[] { 2, 4, 6 });
    }

    #endregion

    #region Take

    [Fact]
    public async Task Take_LimitsResults()
    {
        ReactiveKleisli<int, int> range = n => Observable.Range(1, n);

        var taken = range.Take<int, int>(3);
        var results = await taken(10).ToList();

        results.Should().HaveCount(3);
        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    #endregion

    #region Skip

    [Fact]
    public async Task Skip_SkipsFirstNResults()
    {
        ReactiveKleisli<int, int> range = n => Observable.Range(1, n);

        var skipped = range.Skip<int, int>(3);
        var results = await skipped(5).ToList();

        results.Should().BeEquivalentTo(new[] { 4, 5 });
    }

    #endregion

    #region Buffer

    [Fact]
    public async Task Buffer_GroupsResultsByCount()
    {
        ReactiveKleisli<int, int> range = n => Observable.Range(1, n);

        var buffered = range.Buffer<int, int>(2);
        var results = await buffered(4).ToList();

        results.Should().HaveCount(2);
        results[0].Should().BeEquivalentTo(new[] { 1, 2 });
        results[1].Should().BeEquivalentTo(new[] { 3, 4 });
    }

    #endregion

    #region Scan

    [Fact]
    public async Task Scan_AccumulatesResults()
    {
        ReactiveKleisli<int, int> range = n => Observable.Range(1, n);

        var scanned = range.Scan<int, int, int>(0, (acc, x) => acc + x);
        var results = await scanned(4).ToList();

        // Partial sums: 1, 3, 6, 10
        results.Should().BeEquivalentTo(new[] { 1, 3, 6, 10 });
    }

    #endregion

    #region Catch

    [Fact]
    public async Task Catch_OnError_SwitchesToFallback()
    {
        ReactiveKleisli<int, int> failing = _ => Observable.Throw<int>(new InvalidOperationException("boom"));

        var caught = failing.Catch<int, int>(ex => Observable.Return(-1));
        var results = await caught(0).ToList();

        results.Should().ContainSingle().Which.Should().Be(-1);
    }

    [Fact]
    public async Task Catch_NoError_ReturnsNormally()
    {
        ReactiveKleisli<int, int> ok = x => Observable.Return(x);

        var caught = ok.Catch<int, int>(ex => Observable.Return(-1));
        var results = await caught(42).ToList();

        results.Should().ContainSingle().Which.Should().Be(42);
    }

    #endregion

    #region Do

    [Fact]
    public async Task Do_ExecutesSideEffectWithoutModifying()
    {
        ReactiveKleisli<int, int> source = x => Observable.Return(x);
        var sideEffects = new List<int>();

        var tapped = source.Do<int, int>(x => sideEffects.Add(x));
        var results = await tapped(42).ToList();

        results.Should().ContainSingle().Which.Should().Be(42);
        sideEffects.Should().ContainSingle().Which.Should().Be(42);
    }

    #endregion

    #region SelectMany (LINQ support)

    [Fact]
    public async Task SelectMany_WithSelector_ComposesArrows()
    {
        ReactiveKleisli<int, int> f = x => new[] { x, x + 1 }.ToObservable();

        var composed = f.SelectMany(mid => (ReactiveKleisli<int, int>)(x => Observable.Return(x * 10)));
        var results = await composed(3).ToList();

        results.Should().BeEquivalentTo(new[] { 30, 40 });
    }

    [Fact]
    public async Task SelectMany_WithCollectionAndResultSelector_Projects()
    {
        ReactiveKleisli<int, int> f = x => new[] { x, x + 1 }.ToObservable();

        var composed = f.SelectMany(
            mid => Observable.Return(mid * 10),
            (mid, result) => result);
        var results = await composed(3).ToList();

        results.Should().BeEquivalentTo(new[] { 30, 40 });
    }

    #endregion
}
