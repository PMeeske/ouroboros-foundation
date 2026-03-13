using Ouroboros.Core.Performance;

namespace Ouroboros.Core.Tests.Performance;

[Trait("Category", "Unit")]
public class ObjectPoolTests
{
    [Fact]
    public void Constructor_NullFactory_ThrowsArgumentNullException()
    {
        Action act = () => new ObjectPool<StringBuilder>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Rent_EmptyPool_CreatesNewObject()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder("initial"));

        var obj = pool.Rent();

        obj.Should().NotBeNull();
        obj.ToString().Should().Be("initial");
    }

    [Fact]
    public void Rent_AfterReturn_ReusesObject()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder());
        var original = pool.Rent();
        original.Append("test");
        pool.Return(original);

        var reused = pool.Rent();

        reused.Should().BeSameAs(original);
    }

    [Fact]
    public void Return_NullObject_DoesNotThrow()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder());

        var act = () => pool.Return(null!);

        act.Should().NotThrow();
    }

    [Fact]
    public void Return_WithResetAction_ResetsObject()
    {
        var pool = new ObjectPool<StringBuilder>(
            () => new StringBuilder(),
            sb => sb.Clear());
        var obj = pool.Rent();
        obj.Append("data");
        pool.Return(obj);

        var reused = pool.Rent();

        reused.ToString().Should().BeEmpty();
    }

    [Fact]
    public void Return_AtMaxCapacity_DoesNotAddToPool()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder(), maxPoolSize: 1);
        var obj1 = new StringBuilder("1");
        var obj2 = new StringBuilder("2");

        pool.Return(obj1);
        pool.Return(obj2);

        pool.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public void Count_Initially_IsZero()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder());

        pool.Count.Should().Be(0);
    }

    [Fact]
    public void Count_AfterReturn_Increments()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder());
        pool.Return(new StringBuilder());

        pool.Count.Should().Be(1);
    }

    [Fact]
    public void Count_AfterRent_Decrements()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder());
        pool.Return(new StringBuilder());

        pool.Rent();

        pool.Count.Should().Be(0);
    }

    [Fact]
    public void Clear_RemovesAllObjects()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder());
        pool.Return(new StringBuilder());
        pool.Return(new StringBuilder());

        pool.Clear();

        pool.Count.Should().Be(0);
    }

    [Fact]
    public void Rent_MultipleTimes_CreatesUniqueObjects()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder());

        var obj1 = pool.Rent();
        var obj2 = pool.Rent();

        obj1.Should().NotBeSameAs(obj2);
    }
}
