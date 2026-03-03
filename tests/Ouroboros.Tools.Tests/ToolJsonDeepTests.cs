namespace Ouroboros.Tests.Tools;

using System.Text.Json;
using Ouroboros.Tools;

/// <summary>
/// Deep tests for ToolJson covering nested objects, arrays, special characters,
/// numeric types, and round-trip serialization edge cases.
/// </summary>
[Trait("Category", "Unit")]
public class ToolJsonDeepTests
{
    #region Serialize - Complex Types

    [Fact]
    public void Serialize_NestedObject_ProducesValidJson()
    {
        var obj = new { Outer = new { Inner = "value" } };

        var json = ToolJson.Serialize(obj);

        json.Should().Contain("Outer");
        json.Should().Contain("Inner");
        json.Should().Contain("value");
    }

    [Fact]
    public void Serialize_Array_ProducesValidJson()
    {
        var arr = new[] { 1, 2, 3 };

        var json = ToolJson.Serialize(arr);

        json.Should().Be("[1,2,3]");
    }

    [Fact]
    public void Serialize_EmptyObject_ProducesEmptyJson()
    {
        var obj = new { };

        var json = ToolJson.Serialize(obj);

        json.Should().Be("{}");
    }

    [Fact]
    public void Serialize_BoolValues_ProducesCorrectJson()
    {
        var obj = new { Active = true, Deleted = false };

        var json = ToolJson.Serialize(obj);

        json.Should().Contain("true");
        json.Should().Contain("false");
    }

    [Fact]
    public void Serialize_SpecialCharacters_Escapes()
    {
        var obj = new { Text = "line1\nline2\ttab" };

        var json = ToolJson.Serialize(obj);

        json.Should().Contain("\\n");
        json.Should().Contain("\\t");
    }

    #endregion

    #region Deserialize - Complex Types

    [Fact]
    public void Deserialize_NestedObject_ParsesCorrectly()
    {
        var json = """{"Name":"Root","Child":{"Value":"inner"}}""";

        var result = ToolJson.Deserialize<ParentDto>(json);

        result.Name.Should().Be("Root");
        result.Child.Should().NotBeNull();
        result.Child!.Value.Should().Be("inner");
    }

    [Fact]
    public void Deserialize_ArrayProperty_ParsesCorrectly()
    {
        var json = """{"Tags":["a","b","c"]}""";

        var result = ToolJson.Deserialize<ArrayDto>(json);

        result.Tags.Should().HaveCount(3);
        result.Tags.Should().Contain("b");
    }

    [Fact]
    public void Deserialize_NumericTypes_ParsesCorrectly()
    {
        var json = """{"IntVal":42,"DoubleVal":3.14,"LongVal":9999999999}""";

        var result = ToolJson.Deserialize<NumericDto>(json);

        result.IntVal.Should().Be(42);
        result.DoubleVal.Should().BeApproximately(3.14, 0.001);
        result.LongVal.Should().Be(9999999999L);
    }

    [Fact]
    public void Deserialize_BoolProperty_ParsesCorrectly()
    {
        var json = """{"Active":true}""";

        var result = ToolJson.Deserialize<BoolDto>(json);

        result.Active.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_NullProperty_SetsToNull()
    {
        var json = """{"Name":null}""";

        var result = ToolJson.Deserialize<NullableDto>(json);

        result.Name.Should().BeNull();
    }

    #endregion

    #region Round Trip

    [Fact]
    public void RoundTrip_ComplexObject_PreservesData()
    {
        var original = new ParentDto
        {
            Name = "Test",
            Child = new ChildDto { Value = "child-value" }
        };

        var json = ToolJson.Serialize(original);
        var deserialized = ToolJson.Deserialize<ParentDto>(json);

        deserialized.Name.Should().Be("Test");
        deserialized.Child!.Value.Should().Be("child-value");
    }

    [Fact]
    public void RoundTrip_EmptyStringProperty_PreservesEmpty()
    {
        var original = new NullableDto { Name = "" };

        var json = ToolJson.Serialize(original);
        var deserialized = ToolJson.Deserialize<NullableDto>(json);

        deserialized.Name.Should().BeEmpty();
    }

    [Fact]
    public void RoundTrip_ArrayProperty_PreservesElements()
    {
        var original = new ArrayDto { Tags = new[] { "x", "y", "z" } };

        var json = ToolJson.Serialize(original);
        var deserialized = ToolJson.Deserialize<ArrayDto>(json);

        deserialized.Tags.Should().BeEquivalentTo(new[] { "x", "y", "z" });
    }

    #endregion

    #region Error Cases

    [Fact]
    public void Deserialize_TruncatedJson_Throws()
    {
        Action act = () => ToolJson.Deserialize<NullableDto>("""{"Name":"trunc""");

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_WrongType_ThrowsOrReturnsDefault()
    {
        // String where int expected - should throw
        Action act = () => ToolJson.Deserialize<NumericDto>("""{"IntVal":"notanumber"}""");

        act.Should().Throw<JsonException>();
    }

    #endregion

    #region Helper Types

    private class ParentDto
    {
        public string Name { get; set; } = "";
        public ChildDto? Child { get; set; }
    }

    private class ChildDto
    {
        public string Value { get; set; } = "";
    }

    private class ArrayDto
    {
        public string[] Tags { get; set; } = [];
    }

    private class NumericDto
    {
        public int IntVal { get; set; }
        public double DoubleVal { get; set; }
        public long LongVal { get; set; }
    }

    private class BoolDto
    {
        public bool Active { get; set; }
    }

    private class NullableDto
    {
        public string? Name { get; set; }
    }

    #endregion
}
