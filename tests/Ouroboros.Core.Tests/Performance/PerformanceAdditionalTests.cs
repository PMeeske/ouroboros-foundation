using Ouroboros.Core.Performance;

namespace Ouroboros.Core.Tests.Performance;

/// <summary>
/// Additional tests for ObjectPool, PooledObject, CommonPools, and PooledHelpers
/// to fill remaining coverage gaps.
/// </summary>
[Trait("Category", "Unit")]
public class ObjectPoolAdditionalTests
{
    [Fact]
    public void Rent_MultipleObjects_EachCreatedByFactory()
    {
        int createCount = 0;
        var pool = new ObjectPool<object>(() => { createCount++; return new object(); });

        var obj1 = pool.Rent();
        var obj2 = pool.Rent();

        createCount.Should().Be(2);
        obj1.Should().NotBeSameAs(obj2);
    }

    [Fact]
    public void Return_WithNoResetAction_StillAddsToPool()
    {
        var pool = new ObjectPool<List<int>>(() => new List<int>(), resetAction: null);
        var obj = pool.Rent();
        obj.Add(42);
        pool.Return(obj);

        var rented = pool.Rent();
        rented.Should().BeSameAs(obj);
        rented.Should().Contain(42); // No reset happened
    }

    [Fact]
    public void Clear_WhenPoolHasItems_RemovesAll()
    {
        var pool = new ObjectPool<object>(() => new object(), maxPoolSize: 10);
        pool.Return(new object());
        pool.Return(new object());
        pool.Return(new object());

        pool.Count.Should().BeGreaterOrEqualTo(3);

        pool.Clear();
        pool.Count.Should().Be(0);
    }

    [Fact]
    public void MaxPoolSize_PreventsExcessiveGrowth()
    {
        var pool = new ObjectPool<object>(() => new object(), maxPoolSize: 2);

        pool.Return(new object());
        pool.Return(new object());
        pool.Return(new object()); // Should be discarded

        pool.Count.Should().BeLessOrEqualTo(2);
    }
}

[Trait("Category", "Unit")]
public class PooledObjectAdditionalTests
{
    [Fact]
    public void Object_AfterDispose_ThrowsObjectDisposedException()
    {
        var pool = new ObjectPool<List<int>>(() => new List<int>());
        var pooled = pool.RentDisposable();
        pooled.Dispose();

        var act = () => { var _ = pooled.Object; };
        act.Should().Throw<ObjectDisposedException>();
    }
}

[Trait("Category", "Unit")]
public class CommonPoolsAdditionalTests
{
    [Fact]
    public void StringBuilder_ResetClearsContent()
    {
        var sb = CommonPools.StringBuilder.Rent();
        sb.Append("test data");
        CommonPools.StringBuilder.Return(sb);

        var rented = CommonPools.StringBuilder.Rent();
        rented.Length.Should().Be(0);
    }

    [Fact]
    public void StringList_ResetClearsContent()
    {
        var list = CommonPools.StringList.Rent();
        list.Add("item1");
        list.Add("item2");
        CommonPools.StringList.Return(list);

        var rented = CommonPools.StringList.Rent();
        rented.Should().BeEmpty();
    }

    [Fact]
    public void StringDictionary_ResetClearsContent()
    {
        var dict = CommonPools.StringDictionary.Rent();
        dict["key"] = "value";
        CommonPools.StringDictionary.Return(dict);

        var rented = CommonPools.StringDictionary.Rent();
        rented.Should().BeEmpty();
    }

    [Fact]
    public void MemoryStream_ResetClearsContent()
    {
        var ms = CommonPools.MemoryStream.Rent();
        ms.WriteByte(1);
        ms.WriteByte(2);
        CommonPools.MemoryStream.Return(ms);

        var rented = CommonPools.MemoryStream.Rent();
        rented.Length.Should().Be(0);
        rented.Position.Should().Be(0);
    }
}

[Trait("Category", "Unit")]
public class PooledHelpersAdditionalTests
{
    [Fact]
    public void WithStringBuilder_EmptyAction_ReturnsEmptyString()
    {
        var result = PooledHelpers.WithStringBuilder(sb => { });
        result.Should().BeEmpty();
    }

    [Fact]
    public void WithStringList_ReturnsCorrectResult()
    {
        var result = PooledHelpers.WithStringList(list =>
        {
            list.Add("hello");
            list.Add("world");
            return string.Join(" ", list);
        });
        result.Should().Be("hello world");
    }

    [Fact]
    public void WithStringDictionary_ReturnsCorrectResult()
    {
        var result = PooledHelpers.WithStringDictionary(dict =>
        {
            dict["a"] = "1";
            dict["b"] = "2";
            return dict.Count;
        });
        result.Should().Be(2);
    }

    [Fact]
    public void WithMemoryStream_ReturnsCorrectResult()
    {
        var result = PooledHelpers.WithMemoryStream(ms =>
        {
            ms.WriteByte(42);
            return ms.ToArray();
        });
        result.Should().HaveCount(1);
        result[0].Should().Be(42);
    }
}
