using System.Text;
using Ouroboros.Core.Performance;

namespace Ouroboros.Core.Tests.Performance;

[Trait("Category", "Unit")]
public class CommonPoolsTests
{
    [Fact]
    public void StringBuilder_RentsAndReturns()
    {
        var sb = CommonPools.StringBuilder.Rent();

        sb.Should().NotBeNull();
        sb.Should().BeOfType<StringBuilder>();

        CommonPools.StringBuilder.Return(sb);
    }

    [Fact]
    public void StringBuilder_ResetClearsContent()
    {
        var sb = CommonPools.StringBuilder.Rent();
        sb.Append("test data");
        CommonPools.StringBuilder.Return(sb);

        var reused = CommonPools.StringBuilder.Rent();

        reused.ToString().Should().BeEmpty();
        CommonPools.StringBuilder.Return(reused);
    }

    [Fact]
    public void StringList_RentsAndReturns()
    {
        var list = CommonPools.StringList.Rent();

        list.Should().NotBeNull();

        CommonPools.StringList.Return(list);
    }

    [Fact]
    public void StringList_ResetClearsList()
    {
        var list = CommonPools.StringList.Rent();
        list.Add("item1");
        list.Add("item2");
        CommonPools.StringList.Return(list);

        var reused = CommonPools.StringList.Rent();

        reused.Should().BeEmpty();
        CommonPools.StringList.Return(reused);
    }

    [Fact]
    public void StringDictionary_RentsAndReturns()
    {
        var dict = CommonPools.StringDictionary.Rent();

        dict.Should().NotBeNull();

        CommonPools.StringDictionary.Return(dict);
    }

    [Fact]
    public void StringDictionary_ResetClearsDictionary()
    {
        var dict = CommonPools.StringDictionary.Rent();
        dict["key"] = "value";
        CommonPools.StringDictionary.Return(dict);

        var reused = CommonPools.StringDictionary.Rent();

        reused.Should().BeEmpty();
        CommonPools.StringDictionary.Return(reused);
    }

    [Fact]
    public void MemoryStream_RentsAndReturns()
    {
        var ms = CommonPools.MemoryStream.Rent();

        ms.Should().NotBeNull();
        ms.Should().BeOfType<MemoryStream>();

        CommonPools.MemoryStream.Return(ms);
    }

    [Fact]
    public void MemoryStream_ResetClearsStream()
    {
        var ms = CommonPools.MemoryStream.Rent();
        ms.Write(new byte[] { 1, 2, 3 });
        CommonPools.MemoryStream.Return(ms);

        var reused = CommonPools.MemoryStream.Rent();

        reused.Length.Should().Be(0);
        reused.Position.Should().Be(0);
        CommonPools.MemoryStream.Return(reused);
    }
}
