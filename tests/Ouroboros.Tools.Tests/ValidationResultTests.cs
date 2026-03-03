namespace Ouroboros.Tests;

using Ouroboros.Tools;

[Trait("Category", "Unit")]
public class ValidationResultTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Act
        var result = new ValidationResult(true, new[] { "error1" }, new[] { "suggestion1" });

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be("error1");
        result.Suggestions.Should().ContainSingle().Which.Should().Be("suggestion1");
    }

    [Fact]
    public void Constructor_ValidWithNoErrors()
    {
        // Act
        var result = new ValidationResult(true, Array.Empty<string>(), Array.Empty<string>());

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Suggestions.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_InvalidWithErrors()
    {
        // Act
        var errors = new[] { "Missing field", "Invalid format" };
        var result = new ValidationResult(false, errors, Array.Empty<string>());

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}

[Trait("Category", "Unit")]
public class ToolExecutionResultTests
{
    [Fact]
    public void Constructor_Success()
    {
        // Act
        var result = new ToolExecutionResult(true, "Success output");

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().Be("Success output");
    }

    [Fact]
    public void Constructor_Failure()
    {
        // Act
        var result = new ToolExecutionResult(false, "Error message");

        // Assert
        result.Success.Should().BeFalse();
        result.Result.Should().Be("Error message");
    }
}

[Trait("Category", "Unit")]
public class ToolInfoTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Act
        var schema = new { type = "object" };
        var info = new ToolInfo("my_tool", "Does something", schema);

        // Assert
        info.Name.Should().Be("my_tool");
        info.Description.Should().Be("Does something");
        info.InputSchema.Should().Be(schema);
    }
}
