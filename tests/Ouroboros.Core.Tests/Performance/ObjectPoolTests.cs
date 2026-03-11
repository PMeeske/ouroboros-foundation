using Ouroboros.Core.Performance;

namespace Ouroboros.Core.Tests.Performance;

[Trait("Category", "Unit")]
public class ObjectPoolTests
{
    [Fact]
    public void Constructor_WithNullFactory_ThrowsArgumentNullException()
    {
        var act = () => new ObjectPool<object>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Rent_WhenPoolEmpty_CreatesNewObject()
    {
        var pool = new ObjectPool<List<int>>(() => new List<int>());
        var obj = pool.Rent();
        obj.Should().NotBeNull();
        obj.Should().BeOfType<List<int>>();
    }

    [Fact]
    public void Rent_WhenObjectAvailable_ReturnsPooledObject()
    {
        var pool = new ObjectPool<List<int>>(() => new List<int>());
        var original = pool.Rent();
        original.Add(42);
        pool.Return(original);

        var rented = pool.Rent();
        rented.Should().BeSameAs(original);
    }

    [Fact]
    public void Return_NullObject_DoesNotThrow()
    {
        var pool = new ObjectPool<object>(() => new object());
        var act = () => pool.Return(null!);
        act.Should().NotThrow();
    }

    [Fact]
    public void Return_WithResetAction_ResetsObject()
    {
        var pool = new ObjectPool<List<int>>(
            () => new List<int>(),
            list => list.Clear());

        var obj = pool.Rent();
        obj.Add(1);
        obj.Add(2);
        pool.Return(obj);

        var rented = pool.Rent();
        rented.Should().BeEmpty();
    }

    [Fact]
    public void Return_WhenAtMaxCapacity_DoesNotAddToPool()
    {
        var pool = new ObjectPool<object>(() => new object(), maxPoolSize: 1);

        var obj1 = new object();
        var obj2 = new object();

        pool.Return(obj1);
        pool.Count.Should().Be(1);

        pool.Return(obj2);
        pool.Count.Should().Be(1); // Still 1, second was discarded
    }

    [Fact]
    public void Count_InitiallyZero()
    {
        var pool = new ObjectPool<object>(() => new object());
        pool.Count.Should().Be(0);
    }

    [Fact]
    public void Count_IncreasesAfterReturn()
    {
        var pool = new ObjectPool<object>(() => new object());
        pool.Return(new object());
        pool.Count.Should().Be(1);
    }

    [Fact]
    public void Count_DecreasesAfterRent()
    {
        var pool = new ObjectPool<object>(() => new object());
        pool.Return(new object());
        pool.Rent();
        pool.Count.Should().Be(0);
    }

    [Fact]
    public void Clear_RemovesAllObjects()
    {
        var pool = new ObjectPool<object>(() => new object());
        pool.Return(new object());
        pool.Return(new object());
        pool.Return(new object());

        pool.Clear();
        pool.Count.Should().Be(0);
    }

    [Fact]
    public void Clear_OnEmptyPool_DoesNotThrow()
    {
        var pool = new ObjectPool<object>(() => new object());
        var act = () => pool.Clear();
        act.Should().NotThrow();
    }
}

[Trait("Category", "Unit")]
public class PooledObjectTests
{
    [Fact]
    public void Object_ReturnsPooledInstance()
    {
        var pool = new ObjectPool<List<int>>(() => new List<int>());
        using var pooled = pool.RentDisposable();
        pooled.Object.Should().NotBeNull();
        pooled.Object.Should().BeOfType<List<int>>();
    }

    [Fact]
    public void Dispose_ReturnsObjectToPool()
    {
        var pool = new ObjectPool<List<int>>(() => new List<int>());
        var pooled = pool.RentDisposable();
        var obj = pooled.Object;
        pooled.Dispose();

        pool.Count.Should().Be(1);
        var rented = pool.Rent();
        rented.Should().BeSameAs(obj);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var pool = new ObjectPool<List<int>>(() => new List<int>());
        var pooled = pool.RentDisposable();
        pooled.Dispose();
        var act = () => pooled.Dispose();
        act.Should().NotThrow();
    }
}

[Trait("Category", "Unit")]
public class ObjectPoolExtensionsTests
{
    [Fact]
    public void RentDisposable_ReturnsPooledObjectWrapper()
    {
        var pool = new ObjectPool<StringBuilder>(() => new StringBuilder());
        using var pooled = pool.RentDisposable();
        pooled.Object.Should().NotBeNull();
    }
}

[Trait("Category", "Unit")]
public class CommonPoolsTests
{
    [Fact]
    public void StringBuilder_RentAndReturn_Works()
    {
        var sb = CommonPools.StringBuilder.Rent();
        sb.Should().NotBeNull();
        sb.Append("test");
        CommonPools.StringBuilder.Return(sb);
    }

    [Fact]
    public void StringList_RentAndReturn_Works()
    {
        var list = CommonPools.StringList.Rent();
        list.Should().NotBeNull();
        list.Add("test");
        CommonPools.StringList.Return(list);
    }

    [Fact]
    public void StringDictionary_RentAndReturn_Works()
    {
        var dict = CommonPools.StringDictionary.Rent();
        dict.Should().NotBeNull();
        dict["key"] = "value";
        CommonPools.StringDictionary.Return(dict);
    }

    [Fact]
    public void MemoryStream_RentAndReturn_Works()
    {
        var ms = CommonPools.MemoryStream.Rent();
        ms.Should().NotBeNull();
        ms.WriteByte(42);
        CommonPools.MemoryStream.Return(ms);
    }
}

[Trait("Category", "Unit")]
public class PooledHelpersTests
{
    [Fact]
    public void WithStringBuilder_ExecutesActionAndReturnsResult()
    {
        var result = PooledHelpers.WithStringBuilder(sb =>
        {
            sb.Append("Hello ");
            sb.Append("World");
        });
        result.Should().Be("Hello World");
    }

    [Fact]
    public void WithStringList_ExecutesFuncAndReturnsResult()
    {
        var result = PooledHelpers.WithStringList<int>(list =>
        {
            list.Add("a");
            list.Add("b");
            return list.Count;
        });
        result.Should().Be(2);
    }

    [Fact]
    public void WithStringDictionary_ExecutesFuncAndReturnsResult()
    {
        var result = PooledHelpers.WithStringDictionary<bool>(dict =>
        {
            dict["key"] = "value";
            return dict.ContainsKey("key");
        });
        result.Should().BeTrue();
    }

    [Fact]
    public void WithMemoryStream_ExecutesFuncAndReturnsResult()
    {
        var result = PooledHelpers.WithMemoryStream<long>(ms =>
        {
            ms.WriteByte(1);
            ms.WriteByte(2);
            return ms.Length;
        });
        result.Should().Be(2);
    }
}
