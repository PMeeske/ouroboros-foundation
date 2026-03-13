namespace Ouroboros.Core.Tests;

[Trait("Category", "Unit")]
public class DisposableResourceTests
{
    private sealed class TestDisposableResource : DisposableResource
    {
        public bool ManagedReleased { get; private set; }
        public bool UnmanagedReleased { get; private set; }
        public bool AsyncManagedReleased { get; private set; }

        public new bool IsDisposed => base.IsDisposed;

        public void CallThrowIfDisposed() => ThrowIfDisposed();

        protected override void ReleaseManagedResources()
        {
            ManagedReleased = true;
        }

        protected override void ReleaseUnmanagedResources()
        {
            UnmanagedReleased = true;
        }

        protected override ValueTask ReleaseManagedResourcesAsync()
        {
            AsyncManagedReleased = true;
            return default;
        }
    }

    [Fact]
    public void Dispose_ReleasesManagedAndUnmanagedResources()
    {
        var sut = new TestDisposableResource();

        sut.Dispose();

        sut.ManagedReleased.Should().BeTrue();
        sut.UnmanagedReleased.Should().BeTrue();
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotDoubleRelease()
    {
        var sut = new TestDisposableResource();

        sut.Dispose();
        sut.ManagedReleased = false;
        sut.Dispose();

        sut.ManagedReleased.Should().BeFalse();
    }

    [Fact]
    public async Task DisposeAsync_ReleasesAllResources()
    {
        var sut = new TestDisposableResource();

        await sut.DisposeAsync();

        sut.AsyncManagedReleased.Should().BeTrue();
        sut.ManagedReleased.Should().BeTrue();
        sut.UnmanagedReleased.Should().BeTrue();
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_DoesNotDoubleRelease()
    {
        var sut = new TestDisposableResource();

        await sut.DisposeAsync();
        sut.AsyncManagedReleased = false;
        await sut.DisposeAsync();

        sut.AsyncManagedReleased.Should().BeFalse();
    }

    [Fact]
    public void ThrowIfDisposed_BeforeDispose_DoesNotThrow()
    {
        var sut = new TestDisposableResource();

        var act = () => sut.CallThrowIfDisposed();

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfDisposed_AfterDispose_ThrowsObjectDisposedException()
    {
        var sut = new TestDisposableResource();
        sut.Dispose();

        var act = () => sut.CallThrowIfDisposed();

        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void IsDisposed_InitiallyFalse()
    {
        var sut = new TestDisposableResource();

        sut.IsDisposed.Should().BeFalse();
    }
}
