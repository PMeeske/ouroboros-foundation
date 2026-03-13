using Ouroboros.Core.Performance;

namespace Ouroboros.Core.Tests.Performance;

[Trait("Category", "Unit")]
public class PooledHelpersTests
{
    [Fact]
    public void WithStringBuilder_ExecutesAction_ReturnsResult()
    {
        var result = PooledHelpers.WithStringBuilder(sb =>
        {
            sb.Append("Hello");
            sb.Append(" ");
            sb.Append("World");
        });

        result.Should().Be("Hello World");
    }

    [Fact]
    public void WithStringBuilder_EmptyAction_ReturnsEmptyString()
    {
        var result = PooledHelpers.WithStringBuilder(_ => { });

        result.Should().BeEmpty();
    }

    [Fact]
    public void WithStringList_ExecutesFunc_ReturnsResult()
    {
        var result = PooledHelpers.WithStringList<int>(list =>
        {
            list.Add("a");
            list.Add("b");
            list.Add("c");
            return list.Count;
        });

        result.Should().Be(3);
    }

    [Fact]
    public void WithStringList_ReturnsCorrectType()
    {
        var result = PooledHelpers.WithStringList<string>(list =>
        {
            list.Add("hello");
            return string.Join(",", list);
        });

        result.Should().Be("hello");
    }

    [Fact]
    public void WithStringDictionary_ExecutesFunc_ReturnsResult()
    {
        var result = PooledHelpers.WithStringDictionary<bool>(dict =>
        {
            dict["key1"] = "value1";
            return dict.ContainsKey("key1");
        });

        result.Should().BeTrue();
    }

    [Fact]
    public void WithStringDictionary_EmptyDictionary_StartsEmpty()
    {
        var result = PooledHelpers.WithStringDictionary<int>(dict => dict.Count);

        result.Should().Be(0);
    }

    [Fact]
    public void WithMemoryStream_ExecutesFunc_ReturnsResult()
    {
        var result = PooledHelpers.WithMemoryStream<long>(ms =>
        {
            ms.Write(new byte[] { 1, 2, 3 });
            return ms.Length;
        });

        result.Should().Be(3);
    }

    [Fact]
    public void WithMemoryStream_StreamStartsEmpty()
    {
        var result = PooledHelpers.WithMemoryStream<long>(ms => ms.Length);

        result.Should().Be(0);
    }
}
