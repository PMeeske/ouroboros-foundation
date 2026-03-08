namespace Ouroboros.Tests.Tools;

using System.Text.Json;
using Ouroboros.Tools;

/// <summary>
/// Deep tests for SchemaGenerator covering complex type hierarchies,
/// multiple properties, and edge cases in type mapping.
/// </summary>
[Trait("Category", "Unit")]
public class SchemaGeneratorDeepTests
{
    #region Multiple Properties

    [Fact]
    public void GenerateSchema_MultipleProperties_AllMapped()
    {
        var schema = SchemaGenerator.GenerateSchema(typeof(MultiPropModel));
        using var doc = JsonDocument.Parse(schema);
        var props = doc.RootElement.GetProperty("properties");

        props.TryGetProperty("Name", out _).Should().BeTrue();
        props.TryGetProperty("Age", out _).Should().BeTrue();
        props.TryGetProperty("Active", out _).Should().BeTrue();
        props.TryGetProperty("Score", out _).Should().BeTrue();
    }

    [Fact]
    public void GenerateSchema_MultipleProperties_CorrectTypes()
    {
        var schema = SchemaGenerator.GenerateSchema(typeof(MultiPropModel));
        using var doc = JsonDocument.Parse(schema);
        var props = doc.RootElement.GetProperty("properties");

        props.GetProperty("Name").GetProperty("type").GetString().Should().Be("string");
        props.GetProperty("Age").GetProperty("type").GetString().Should().Be("integer");
        props.GetProperty("Active").GetProperty("type").GetString().Should().Be("boolean");
        props.GetProperty("Score").GetProperty("type").GetString().Should().Be("number");
    }

    #endregion

    #region Nullable Value Types

    [Fact]
    public void GenerateSchema_NullableInt_NotRequired()
    {
        var schema = SchemaGenerator.GenerateSchema(typeof(NullableIntModel));
        using var doc = JsonDocument.Parse(schema);
        var required = GetRequiredArray(doc);

        required.Should().NotContain("Value");
    }

    [Fact]
    public void GenerateSchema_NullableDouble_NotRequired()
    {
        var schema = SchemaGenerator.GenerateSchema(typeof(NullableDoubleModel));
        using var doc = JsonDocument.Parse(schema);
        var required = GetRequiredArray(doc);

        required.Should().NotContain("Value");
    }

    [Fact]
    public void GenerateSchema_NullableBool_NotRequired()
    {
        var schema = SchemaGenerator.GenerateSchema(typeof(NullableBoolModel));
        using var doc = JsonDocument.Parse(schema);
        var required = GetRequiredArray(doc);

        required.Should().NotContain("Flag");
    }

    #endregion

    #region Array Types

    [Fact]
    public void GenerateSchema_StringArray_MapsToArray()
    {
        var schema = SchemaGenerator.GenerateSchema(typeof(StringArrayModel));
        using var doc = JsonDocument.Parse(schema);

        doc.RootElement.GetProperty("properties")
            .GetProperty("Items")
            .GetProperty("type")
            .GetString().Should().Be("array");
    }

    [Fact]
    public void GenerateSchema_IntList_MapsToArray()
    {
        var schema = SchemaGenerator.GenerateSchema(typeof(IntListModel));
        using var doc = JsonDocument.Parse(schema);

        doc.RootElement.GetProperty("properties")
            .GetProperty("Numbers")
            .GetProperty("type")
            .GetString().Should().Be("array");
    }

    #endregion

    #region Empty and Simple Types

    [Fact]
    public void GenerateSchema_NoProperties_EmptyPropertiesObject()
    {
        var schema = SchemaGenerator.GenerateSchema(typeof(EmptyModel));
        using var doc = JsonDocument.Parse(schema);

        doc.RootElement.GetProperty("type").GetString().Should().Be("object");
        var props = doc.RootElement.GetProperty("properties");
        props.EnumerateObject().Count().Should().Be(0);
    }

    [Fact]
    public void GenerateSchema_SingleProperty_HasOneProperty()
    {
        var schema = SchemaGenerator.GenerateSchema(typeof(SinglePropModel));
        using var doc = JsonDocument.Parse(schema);

        doc.RootElement.GetProperty("properties")
            .EnumerateObject().Count().Should().Be(1);
    }

    #endregion

    #region Required Array

    [Fact]
    public void GenerateSchema_ValueTypesAreRequired()
    {
        var schema = SchemaGenerator.GenerateSchema(typeof(MixedRequiredModel));
        using var doc = JsonDocument.Parse(schema);
        var required = GetRequiredArray(doc);

        required.Should().Contain("Id");
        required.Should().Contain("Count");
    }

    [Fact]
    public void GenerateSchema_ArraysAreOptional()
    {
        var schema = SchemaGenerator.GenerateSchema(typeof(ArrayFieldModel));
        using var doc = JsonDocument.Parse(schema);
        var required = GetRequiredArray(doc);

        required.Should().NotContain("Tags");
    }

    #endregion

    #region Output Format

    [Fact]
    public void GenerateSchema_OutputIsValidJson()
    {
        var schema = SchemaGenerator.GenerateSchema(typeof(MultiPropModel));

        Action act = () => JsonDocument.Parse(schema);

        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateSchema_ContainsTypeObjectRequired()
    {
        var schema = SchemaGenerator.GenerateSchema(typeof(MultiPropModel));
        using var doc = JsonDocument.Parse(schema);

        doc.RootElement.TryGetProperty("type", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("properties", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("required", out _).Should().BeTrue();
    }

    #endregion

    #region Helpers

    private static List<string> GetRequiredArray(JsonDocument doc)
    {
        var required = new List<string>();
        foreach (var item in doc.RootElement.GetProperty("required").EnumerateArray())
        {
            required.Add(item.GetString()!);
        }
        return required;
    }

    private class MultiPropModel
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public bool Active { get; set; }
        public double Score { get; set; }
    }

    private class NullableIntModel { public int? Value { get; set; } }
    private class NullableDoubleModel { public double? Value { get; set; } }
    private class NullableBoolModel { public bool? Flag { get; set; } }
    private class StringArrayModel { public string[] Items { get; set; } = []; }
    private class IntListModel { public List<int> Numbers { get; set; } = new(); }
    private class EmptyModel { }
    private class SinglePropModel { public string X { get; set; } = ""; }

    private class MixedRequiredModel
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public string? OptionalName { get; set; }
    }

    private class ArrayFieldModel
    {
        public int Id { get; set; }
        public string[] Tags { get; set; } = [];
    }

    #endregion
}
