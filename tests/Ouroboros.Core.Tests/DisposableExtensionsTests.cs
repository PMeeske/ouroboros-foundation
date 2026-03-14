using Ouroboros.Abstractions;
using Ouroboros.Core;

namespace Ouroboros.Core.Tests;

[Trait("Category", "Unit")]
public class DisposableExtensionsTests
{
    private class TestDisposable : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }

    private class TestAsyncDisposable : IAsyncDisposable, IDisposable
    {
        public bool Disposed { get; private set; }
        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return default;
        }

        public void Dispose() => Disposed = true;
    }

    [Fact]
    public void Use_Success_ReturnsSuccessResult()
    {
        using var resource = new TestDisposable();
        var result = resource.Use(r => 42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Use_Success_DisposesResource()
    {
        using var resource = new TestDisposable();
        resource.Use(r => 42);

        resource.Disposed.Should().BeTrue();
    }

    [Fact]
    public void Use_WhenActionThrows_ReturnsFailureResult()
    {
        using var resource = new TestDisposable();
        var result = resource.Use<TestDisposable, int>(r => throw new InvalidOperationException("test error"));

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Use_WhenActionThrows_StillDisposesResource()
    {
        using var resource = new TestDisposable();
        resource.Use<TestDisposable, int>(r => throw new InvalidOperationException("test"));

        resource.Disposed.Should().BeTrue();
    }

    [Fact]
    public async Task UseAsync_Success_ReturnsSuccessResult()
    {
        var resource = new TestAsyncDisposable();
        await using (resource)
        {
            var result = await resource.UseAsync(async r =>
            {
                await Task.Yield();
                return 42;
            });

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(42);
        }
    }

    [Fact]
    public async Task UseAsync_Success_DisposesResource()
    {
        var resource = new TestAsyncDisposable();
        await using (resource)
        {
            await resource.UseAsync(async r =>
            {
                await Task.Yield();
                return 42;
            });
        }

        resource.Disposed.Should().BeTrue();
    }

    [Fact]
    public async Task UseAsync_WhenActionThrows_ReturnsFailureResult()
    {
        var resource = new TestAsyncDisposable();
        await using (resource)
        {
            var result = await resource.UseAsync<TestAsyncDisposable, int>(
                r => throw new InvalidOperationException("test error"));

            result.IsSuccess.Should().BeFalse();
        }
    }

    [Fact]
    public async Task UseAsync_WhenActionThrows_StillDisposesResource()
    {
        var resource = new TestAsyncDisposable();
        await using (resource)
        {
            await resource.UseAsync<TestAsyncDisposable, int>(
                r => throw new InvalidOperationException("test"));
        }

        resource.Disposed.Should().BeTrue();
    }
}
