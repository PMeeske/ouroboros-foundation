using System.Reactive.Linq;

namespace Ouroboros.Core.Tests.Monads;

[Trait("Category", "Unit")]
public sealed class ReactiveKleisliExtensionsAdditionalTests
{
    #region Throttle

    [Fact]
    public async Task Throttle_ThrottlesStream()
    {
        ReactiveKleisli<int, int> source = x => Observable.Return(x);
        var throttled = source.Throttle<int, int>(TimeSpan.FromMilliseconds(10));

        var results = await throttled(42).ToList();
        // Single item should pass through after the throttle window
        results.Should().ContainSingle().Which.Should().Be(42);
    }

    #endregion

    #region Debounce

    [Fact]
    public async Task Debounce_BehavesLikeThrottle()
    {
        ReactiveKleisli<int, int> source = x => Observable.Return(x);
        var debounced = source.Debounce<int, int>(TimeSpan.FromMilliseconds(10));

        var results = await debounced(42).ToList();
        results.Should().ContainSingle().Which.Should().Be(42);
    }

    #endregion

    #region SelectMany with result selector - different projection

    [Fact]
    public async Task SelectMany_WithResultSelector_ProjectsWithBothValues()
    {
        ReactiveKleisli<int, int> f = x => new[] { x, x + 1 }.ToObservable();

        var composed = f.SelectMany(
            mid => Observable.Return(mid * 10),
            (mid, result) => mid + result);
        var results = await composed(3).ToList();

        // mid=3, result=30 => 33; mid=4, result=40 => 44
        results.Should().BeEquivalentTo(new[] { 33, 44 });
    }

    #endregion

    #region Compose multiple chaining

    [Fact]
    public async Task Compose_MultipleSteps_ChainsCorrectly()
    {
        ReactiveKleisli<int, int> addOne = x => Observable.Return(x + 1);
        ReactiveKleisli<int, int> multiplyTwo = x => Observable.Return(x * 2);
        ReactiveKleisli<int, string> toString = x => Observable.Return(x.ToString());

        var composed = addOne.Compose(multiplyTwo).Compose(toString);
        var results = await composed(4).ToList();

        results.Should().ContainSingle().Which.Should().Be("10"); // (4+1)*2 = 10
    }

    #endregion

    #region Map with empty stream

    [Fact]
    public async Task Map_EmptyStream_ReturnsEmpty()
    {
        ReactiveKleisli<int, int> empty = _ => Observable.Empty<int>();
        var mapped = empty.Map<int, int, string>(x => x.ToString());

        var results = await mapped(1).ToList();
        results.Should().BeEmpty();
    }

    #endregion

    #region Where with all-matching predicate

    [Fact]
    public async Task Where_AllMatch_ReturnsAll()
    {
        ReactiveKleisli<int, int> source = n => Observable.Range(1, n);
        var filtered = source.Where<int, int>(_ => true);

        var results = await filtered(3).ToList();
        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task Where_NoneMatch_ReturnsEmpty()
    {
        ReactiveKleisli<int, int> source = n => Observable.Range(1, n);
        var filtered = source.Where<int, int>(_ => false);

        var results = await filtered(3).ToList();
        results.Should().BeEmpty();
    }

    #endregion

    #region Buffer with odd count

    [Fact]
    public async Task Buffer_OddCount_HandlesLastPartialBuffer()
    {
        ReactiveKleisli<int, int> range = n => Observable.Range(1, n);
        var buffered = range.Buffer<int, int>(2);

        var results = await buffered(5).ToList();
        results.Should().HaveCount(3);
        results[0].Should().BeEquivalentTo(new[] { 1, 2 });
        results[1].Should().BeEquivalentTo(new[] { 3, 4 });
        results[2].Should().BeEquivalentTo(new[] { 5 });
    }

    #endregion

    #region Scan with string accumulator

    [Fact]
    public async Task Scan_StringAccumulator_AccumulatesCorrectly()
    {
        ReactiveKleisli<int, int> source = _ => new[] { 1, 2, 3 }.ToObservable();
        var scanned = source.Scan<int, int, string>("", (acc, x) => acc + x);

        var results = await scanned(0).ToList();
        results.Should().BeEquivalentTo(new[] { "1", "12", "123" });
    }

    #endregion

    #region Do with multiple items

    [Fact]
    public async Task Do_MultipleItems_ExecutesSideEffectForEach()
    {
        ReactiveKleisli<int, int> source = _ => new[] { 1, 2, 3 }.ToObservable();
        var sideEffects = new List<int>();

        var tapped = source.Do<int, int>(x => sideEffects.Add(x));
        var results = await tapped(0).ToList();

        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        sideEffects.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    #endregion

    #region Skip edge case

    [Fact]
    public async Task Skip_MoreThanAvailable_ReturnsEmpty()
    {
        ReactiveKleisli<int, int> source = n => Observable.Range(1, n);
        var skipped = source.Skip<int, int>(10);

        var results = await skipped(3).ToList();
        results.Should().BeEmpty();
    }

    #endregion

    #region Take edge case

    [Fact]
    public async Task Take_MoreThanAvailable_ReturnsAll()
    {
        ReactiveKleisli<int, int> source = n => Observable.Range(1, n);
        var taken = source.Take<int, int>(100);

        var results = await taken(3).ToList();
        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    #endregion

    #region Union with empty

    [Fact]
    public async Task Union_WithEmpty_ReturnsFirstOnly()
    {
        ReactiveKleisli<int, int> f = x => Observable.Return(x);
        ReactiveKleisli<int, int> g = _ => Observable.Empty<int>();

        var merged = f.Union(g);
        var results = await merged(42).ToList();

        results.Should().ContainSingle().Which.Should().Be(42);
    }

    #endregion

    #region Merge with empty

    [Fact]
    public async Task Merge_WithEmpty_ReturnsFirstOnly()
    {
        ReactiveKleisli<int, int> f = x => Observable.Return(x);
        ReactiveKleisli<int, int> g = _ => Observable.Empty<int>();

        var merged = f.Merge(g);
        var results = await merged(42).ToList();

        results.Should().ContainSingle().Which.Should().Be(42);
    }

    #endregion
}
