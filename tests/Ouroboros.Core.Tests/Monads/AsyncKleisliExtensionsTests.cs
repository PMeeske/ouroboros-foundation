// <copyright file="AsyncKleisliExtensionsTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Core.Monads;

namespace Ouroboros.Core.Tests.Monads;

/// <summary>
/// Tests for AsyncKleisli arrows and their extension methods.
/// </summary>
[Trait("Category", "Unit")]
public class AsyncKleisliExtensionsTests
{
    // --- Identity ---

    [Fact]
    public async Task Identity_ReturnsInputAsSingletonStream()
    {
        // Arrange
        var identity = AsyncKleisliExtensions.Identity<int>();

        // Act
        var results = await CollectAsync(identity(42));

        // Assert
        results.Should().ContainSingle().Which.Should().Be(42);
    }

    // --- Lift ---

    [Fact]
    public async Task Lift_PureFunction_WrapsInSingletonStream()
    {
        // Arrange
        var doubled = AsyncKleisliExtensions.Lift<int, int>(x => x * 2);

        // Act
        var results = await CollectAsync(doubled(5));

        // Assert
        results.Should().ContainSingle().Which.Should().Be(10);
    }

    // --- LiftAsync ---

    [Fact]
    public async Task LiftAsync_AsyncFunction_WrapsInSingletonStream()
    {
        // Arrange
        var asyncDoubled = AsyncKleisliExtensions.LiftAsync<int, int>(
            async x =>
            {
                await Task.Yield();
                return x * 2;
            });

        // Act
        var results = await CollectAsync(asyncDoubled(7));

        // Assert
        results.Should().ContainSingle().Which.Should().Be(14);
    }

    // --- LiftMany ---

    [Fact]
    public async Task LiftMany_ReturnsMultipleElements()
    {
        // Arrange
        var expand = AsyncKleisliExtensions.LiftMany<int, int>(
            x => ToAsyncEnumerable(Enumerable.Range(0, x)));

        // Act
        var results = await CollectAsync(expand(3));

        // Assert
        results.Should().BeEquivalentTo(new[] { 0, 1, 2 });
    }

    // --- Then (Kleisli composition) ---

    [Fact]
    public async Task Then_ComposesArrows()
    {
        // Arrange: f: int -> doubled, g: int -> string
        AsyncKleisli<int, int> doubler = x => ToAsyncEnumerable(new[] { x * 2 });
        AsyncKleisli<int, string> toString = x => ToAsyncEnumerable(new[] { x.ToString() });

        var composed = doubler.Then(toString);

        // Act
        var results = await CollectAsync(composed(5));

        // Assert
        results.Should().ContainSingle().Which.Should().Be("10");
    }

    [Fact]
    public async Task Then_FlatMapsMultipleResults()
    {
        // Arrange: f produces multiple, g processes each
        AsyncKleisli<int, int> expand = x => ToAsyncEnumerable(new[] { x, x + 1 });
        AsyncKleisli<int, int> doubler = x => ToAsyncEnumerable(new[] { x * 2 });

        var composed = expand.Then(doubler);

        // Act
        var results = await CollectAsync(composed(3));

        // Assert
        results.Should().BeEquivalentTo(new[] { 6, 8 }); // 3*2=6, 4*2=8
    }

    // --- Map ---

    [Fact]
    public async Task Map_TransformsResults()
    {
        // Arrange
        AsyncKleisli<int, int> source = x => ToAsyncEnumerable(new[] { x, x + 1, x + 2 });
        var mapped = source.Map<int, int, string>(n => $"value:{n}");

        // Act
        var results = await CollectAsync(mapped(10));

        // Assert
        results.Should().BeEquivalentTo(new[] { "value:10", "value:11", "value:12" });
    }

    // --- MapAsync ---

    [Fact]
    public async Task MapAsync_TransformsResultsAsync()
    {
        // Arrange
        AsyncKleisli<int, int> source = x => ToAsyncEnumerable(new[] { x });
        var mapped = source.MapAsync<int, int, string>(async n =>
        {
            await Task.Yield();
            return $"async:{n}";
        });

        // Act
        var results = await CollectAsync(mapped(42));

        // Assert
        results.Should().ContainSingle().Which.Should().Be("async:42");
    }

    // --- Where (filter) ---

    [Fact]
    public async Task Where_FiltersResults()
    {
        // Arrange
        AsyncKleisli<int, int> source = x => ToAsyncEnumerable(Enumerable.Range(x, 5));
        var evens = source.Where<int, int>(n => n % 2 == 0);

        // Act
        var results = await CollectAsync(evens(1)); // 1,2,3,4,5

        // Assert
        results.Should().BeEquivalentTo(new[] { 2, 4 });
    }

    // --- WhereAsync ---

    [Fact]
    public async Task WhereAsync_FiltersWithAsyncPredicate()
    {
        // Arrange
        AsyncKleisli<int, int> source = x => ToAsyncEnumerable(new[] { 1, 2, 3, 4 });
        var filtered = source.WhereAsync<int, int>(async n =>
        {
            await Task.Yield();
            return n > 2;
        });

        // Act
        var results = await CollectAsync(filtered(0));

        // Assert
        results.Should().BeEquivalentTo(new[] { 3, 4 });
    }

    // --- Take ---

    [Fact]
    public async Task Take_LimitsResults()
    {
        // Arrange
        AsyncKleisli<int, int> source = x => ToAsyncEnumerable(Enumerable.Range(0, 100));
        var limited = source.Take<int, int>(3);

        // Act
        var results = await CollectAsync(limited(0));

        // Assert
        results.Should().HaveCount(3);
        results.Should().BeEquivalentTo(new[] { 0, 1, 2 });
    }

    [Fact]
    public async Task Take_Zero_ReturnsEmpty()
    {
        AsyncKleisli<int, int> source = x => ToAsyncEnumerable(new[] { 1, 2, 3 });
        var limited = source.Take<int, int>(0);

        var results = await CollectAsync(limited(0));
        results.Should().BeEmpty();
    }

    // --- Distinct ---

    [Fact]
    public async Task Distinct_RemovesDuplicates()
    {
        // Arrange
        AsyncKleisli<int, int> source = x => ToAsyncEnumerable(new[] { 1, 2, 2, 3, 1, 3 });
        var distinct = source.Distinct<int, int>();

        // Act
        var results = await CollectAsync(distinct(0));

        // Assert
        results.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    // --- Union ---

    [Fact]
    public async Task Union_MergesStreams()
    {
        // Arrange
        AsyncKleisli<int, int> first = x => ToAsyncEnumerable(new[] { 1, 2 });
        AsyncKleisli<int, int> second = x => ToAsyncEnumerable(new[] { 3, 4 });
        var merged = first.Union(second);

        // Act
        var results = await CollectAsync(merged(0));

        // Assert
        results.Should().HaveCount(4);
        results.Should().Contain(new[] { 1, 2, 3, 4 });
    }

    // --- Helpers ---

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
}
