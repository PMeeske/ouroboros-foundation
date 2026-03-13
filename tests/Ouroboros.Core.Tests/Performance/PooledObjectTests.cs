using System.Text;
using Ouroboros.Core.Performance;

namespace Ouroboros.Core.Tests.Performance;

[Trait("Category", "Unit")]
public class PooledObjectTests
{
    [Fact]
    public void Object_ReturnsPooledObject()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder());
        using var pooled = pool.RentDisposable();

        pooled.Object.Should().NotBeNull();
        pooled.Object.Should().BeOfType<StringBuilder>();
    }

    [Fact]
    public void Dispose_ReturnsObjectToPool()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder());
        var pooled = pool.RentDisposable();
        var obj = pooled.Object;

        pooled.Dispose();

        pool.Count.Should().Be(1);
        var rented = pool.Rent();
        rented.Should().BeSameAs(obj);
    }

    [Fact]
    public void Object_AfterDispose_ThrowsObjectDisposedException()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder());
        var pooled = pool.RentDisposable();
        pooled.Dispose();

        Action act = () => _ = pooled.Object;

        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_OnlyReturnsOnce()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder());
        var pooled = pool.RentDisposable();

        pooled.Dispose();
        pooled.Dispose();

        pool.Count.Should().Be(1);
    }

    [Fact]
    public void UsingPattern_ReturnsObjectAutomatically()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder());

        using (var pooled = pool.RentDisposable())
        {
            pooled.Object.Append("test");
        }

        pool.Count.Should().Be(1);
    }
}
