// <copyright file="ToolJsonTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools;

using System.Text.Json;
using Ouroboros.Tools;

/// <summary>
/// Tests for ToolJson static JSON utility covering serialization and deserialization.
/// </summary>
[Trait("Category", "Unit")]
public class ToolJsonTests
{
    // --- Serialize ---

    [Fact]
    public void Serialize_SimpleObject_ProducesValidJson()
    {
        // Arrange
        var obj = new { Name = "Alice", Age = 30 };

        // Act
        string json = ToolJson.Serialize(obj);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("\"Name\"");
        json.Should().Contain("\"Alice\"");
        json.Should().Contain("30");
    }

    [Fact]
    public void Serialize_Output_IsNotIndented()
    {
        // Arrange
        var obj = new { Name = "Alice", Age = 30, Active = true };

        // Act
        string json = ToolJson.Serialize(obj);

        // Assert
        json.Should().NotContain("\n");
        json.Should().NotContain("  ");
    }

    [Fact]
    public void Serialize_NullValue_ProducesNullJson()
    {
        // Act
        string json = ToolJson.Serialize<object?>(null);

        // Assert
        json.Should().Be("null");
    }

    // --- Deserialize ---

    [Fact]
    public void Deserialize_ValidJson_ReturnsCorrectObject()
    {
        // Arrange
        string json = """{"Name":"Bob","Age":25}""";

        // Act
        var result = ToolJson.Deserialize<TestDto>(json);

        // Assert
        result.Name.Should().Be("Bob");
        result.Age.Should().Be(25);
    }

    [Fact]
    public void Deserialize_RoundTrips_Correctly()
    {
        // Arrange
        var original = new TestDto { Name = "Charlie", Age = 42 };

        // Act
        string json = ToolJson.Serialize(original);
        var deserialized = ToolJson.Deserialize<TestDto>(json);

        // Assert
        deserialized.Name.Should().Be(original.Name);
        deserialized.Age.Should().Be(original.Age);
    }

    [Fact]
    public void Deserialize_CaseInsensitive_MatchesProperties()
    {
        // Arrange - all lowercase keys
        string json = """{"name":"Diana","age":35}""";

        // Act
        var result = ToolJson.Deserialize<TestDto>(json);

        // Assert
        result.Name.Should().Be("Diana");
        result.Age.Should().Be(35);
    }

    [Fact]
    public void Deserialize_MixedCase_MatchesProperties()
    {
        // Arrange - mixed case keys
        string json = """{"NAME":"Eve","aGe":28}""";

        // Act
        var result = ToolJson.Deserialize<TestDto>(json);

        // Assert
        result.Name.Should().Be("Eve");
        result.Age.Should().Be(28);
    }

    [Fact]
    public void Deserialize_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        string invalidJson = "this is not json";

        // Act
        Action act = () => ToolJson.Deserialize<TestDto>(invalidJson);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_EmptyString_ThrowsJsonException()
    {
        // Act
        Action act = () => ToolJson.Deserialize<TestDto>(string.Empty);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_MalformedJson_ThrowsJsonException()
    {
        // Arrange
        string malformed = """{"Name":"unclosed""";

        // Act
        Action act = () => ToolJson.Deserialize<TestDto>(malformed);

        // Assert
        act.Should().Throw<JsonException>();
    }

    // --- Helper types ---

    private class TestDto
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
