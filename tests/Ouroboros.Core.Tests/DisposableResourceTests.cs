using Ouroboros.Core;

namespace Ouroboros.Core.Tests;

[Trait("Category", "Unit")]
public class DisposableResourceTests
{
    private class TestResource : DisposableResource
    {
        public bool ManagedReleased { get; private set; }
        public bool UnmanagedReleased { get; private set; }
        public bool AsyncManagedReleased { get; private set; }
        public new bool IsDisposed => base.IsDisposed;

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

        public void CallThrowIfDisposed() => ThrowIfDisposed();
    }

    [Fact]
    public void Dispose_ReleasesManagedAndUnmanagedResources()
    {
        var resource = new TestResource();
        resource.Dispose();

        resource.ManagedReleased.Should().BeTrue();
        resource.UnmanagedReleased.Should().BeTrue();
        resource.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_CalledTwice_OnlyReleasesOnce()
    {
        var resource = new TestResource();
        resource.Dispose();
        resource.ManagedReleased = false;
        resource.Dispose();

        resource.ManagedReleased.Should().BeFalse(); // Not called again
    }

    [Fact]
    public async Task DisposeAsync_ReleasesAllResources()
    {
        var resource = new TestResource();
        await resource.DisposeAsync();

        resource.AsyncManagedReleased.Should().BeTrue();
        resource.ManagedReleased.Should().BeTrue();
        resource.UnmanagedReleased.Should().BeTrue();
        resource.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_OnlyReleasesOnce()
    {
        var resource = new TestResource();
        await resource.DisposeAsync();
        resource.AsyncManagedReleased = false;
        await resource.DisposeAsync();

        resource.AsyncManagedReleased.Should().BeFalse();
    }

    [Fact]
    public void ThrowIfDisposed_WhenNotDisposed_DoesNotThrow()
    {
        var resource = new TestResource();
        var act = () => resource.CallThrowIfDisposed();
        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfDisposed_WhenDisposed_Throws()
    {
        var resource = new TestResource();
        resource.Dispose();

        var act = () => resource.CallThrowIfDisposed();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void IsDisposed_InitiallyFalse()
    {
        var resource = new TestResource();
        resource.IsDisposed.Should().BeFalse();
    }
}
