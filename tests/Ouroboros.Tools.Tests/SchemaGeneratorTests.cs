// <copyright file="SchemaGeneratorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools;

using System.Text.Json;
using Ouroboros.Tools;

/// <summary>
/// Tests for SchemaGenerator static schema generation covering type mapping and nullability.
/// </summary>
[Trait("Category", "Unit")]
public class SchemaGeneratorTests
{
    // --- Null guard ---

    [Fact]
    public void GenerateSchema_NullType_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => SchemaGenerator.GenerateSchema(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // --- String property ---

    [Fact]
    public void GenerateSchema_StringProperty_MapsToString()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(StringModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        doc.RootElement
            .GetProperty("properties")
            .GetProperty("Value")
            .GetProperty("type")
            .GetString()
            .Should().Be("string");
    }

    // --- Integer properties ---

    [Fact]
    public void GenerateSchema_IntProperty_MapsToInteger()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(IntModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        doc.RootElement
            .GetProperty("properties")
            .GetProperty("Count")
            .GetProperty("type")
            .GetString()
            .Should().Be("integer");
    }

    [Fact]
    public void GenerateSchema_LongProperty_MapsToInteger()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(LongModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        doc.RootElement
            .GetProperty("properties")
            .GetProperty("BigCount")
            .GetProperty("type")
            .GetString()
            .Should().Be("integer");
    }

    // --- Number properties ---

    [Fact]
    public void GenerateSchema_DoubleProperty_MapsToNumber()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(DoubleModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        doc.RootElement
            .GetProperty("properties")
            .GetProperty("Rate")
            .GetProperty("type")
            .GetString()
            .Should().Be("number");
    }

    [Fact]
    public void GenerateSchema_FloatProperty_MapsToNumber()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(FloatModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        doc.RootElement
            .GetProperty("properties")
            .GetProperty("Score")
            .GetProperty("type")
            .GetString()
            .Should().Be("number");
    }

    [Fact]
    public void GenerateSchema_DecimalProperty_MapsToNumber()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(DecimalModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        doc.RootElement
            .GetProperty("properties")
            .GetProperty("Price")
            .GetProperty("type")
            .GetString()
            .Should().Be("number");
    }

    // --- Boolean property ---

    [Fact]
    public void GenerateSchema_BoolProperty_MapsToBoolean()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(BoolModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        doc.RootElement
            .GetProperty("properties")
            .GetProperty("Active")
            .GetProperty("type")
            .GetString()
            .Should().Be("boolean");
    }

    // --- Array property ---

    [Fact]
    public void GenerateSchema_ArrayProperty_MapsToArray()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(ArrayModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        doc.RootElement
            .GetProperty("properties")
            .GetProperty("Tags")
            .GetProperty("type")
            .GetString()
            .Should().Be("array");
    }

    [Fact]
    public void GenerateSchema_ListProperty_MapsToArray()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(ListModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        doc.RootElement
            .GetProperty("properties")
            .GetProperty("Items")
            .GetProperty("type")
            .GetString()
            .Should().Be("array");
    }

    // --- Object property ---

    [Fact]
    public void GenerateSchema_ComplexProperty_MapsToObject()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(NestedModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        doc.RootElement
            .GetProperty("properties")
            .GetProperty("Child")
            .GetProperty("type")
            .GetString()
            .Should().Be("object");
    }

    // --- Required/nullable ---

    [Fact]
    public void GenerateSchema_NonNullableValueType_IncludedInRequired()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(RequiredModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        var required = doc.RootElement.GetProperty("required");
        var requiredNames = new List<string>();
        foreach (var item in required.EnumerateArray())
        {
            requiredNames.Add(item.GetString()!);
        }

        requiredNames.Should().Contain("Id");
    }

    [Fact]
    public void GenerateSchema_NullableValueType_ExcludedFromRequired()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(NullableModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        var required = doc.RootElement.GetProperty("required");
        var requiredNames = new List<string>();
        foreach (var item in required.EnumerateArray())
        {
            requiredNames.Add(item.GetString()!);
        }

        requiredNames.Should().NotContain("OptionalAge");
    }

    [Fact]
    public void GenerateSchema_NullableReferenceType_ExcludedFromRequired()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(NullableRefModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        var required = doc.RootElement.GetProperty("required");
        var requiredNames = new List<string>();
        foreach (var item in required.EnumerateArray())
        {
            requiredNames.Add(item.GetString()!);
        }

        requiredNames.Should().NotContain("OptionalName");
    }

    [Fact]
    public void GenerateSchema_NonNullableReferenceType_IncludedInRequired()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(RequiredRefModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        var required = doc.RootElement.GetProperty("required");
        var requiredNames = new List<string>();
        foreach (var item in required.EnumerateArray())
        {
            requiredNames.Add(item.GetString()!);
        }

        requiredNames.Should().Contain("Name");
    }

    // --- Schema structure ---

    [Fact]
    public void GenerateSchema_RootType_IsObject()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(StringModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        doc.RootElement.GetProperty("type").GetString().Should().Be("object");
    }

    [Fact]
    public void GenerateSchema_ContainsPropertiesKey()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(StringModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        doc.RootElement.TryGetProperty("properties", out _).Should().BeTrue();
    }

    [Fact]
    public void GenerateSchema_ContainsRequiredKey()
    {
        // Act
        string schema = SchemaGenerator.GenerateSchema(typeof(IntModel));
        using var doc = JsonDocument.Parse(schema);

        // Assert
        doc.RootElement.TryGetProperty("required", out _).Should().BeTrue();
    }

    // --- Helper model types ---

    private class StringModel
    {
        public string Value { get; set; } = string.Empty;
    }

    private class IntModel
    {
        public int Count { get; set; }
    }

    private class LongModel
    {
        public long BigCount { get; set; }
    }

    private class DoubleModel
    {
        public double Rate { get; set; }
    }

    private class FloatModel
    {
        public float Score { get; set; }
    }

    private class DecimalModel
    {
        public decimal Price { get; set; }
    }

    private class BoolModel
    {
        public bool Active { get; set; }
    }

    private class ArrayModel
    {
        public string[] Tags { get; set; } = Array.Empty<string>();
    }

    private class ListModel
    {
        public List<int> Items { get; set; } = new();
    }

    private class NestedModel
    {
        public StringModel Child { get; set; } = new();
    }

    private class RequiredModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private class NullableModel
    {
        public int Id { get; set; }

        public int? OptionalAge { get; set; }
    }

    private class NullableRefModel
    {
        public int Id { get; set; }

        public string? OptionalName { get; set; }
    }

    private class RequiredRefModel
    {
        public string Name { get; set; } = string.Empty;

        public int Id { get; set; }
    }
}
