using Ouroboros.Abstractions.Errors;

namespace Ouroboros.Abstractions.Tests.Errors;

[Trait("Category", "Unit")]
public class ErrorCodesTests
{
    [Theory]
    [InlineData(nameof(ErrorCodes.EthicsViolation), "ETHICS_001")]
    [InlineData(nameof(ErrorCodes.GovernanceDenied), "GOV_001")]
    [InlineData(nameof(ErrorCodes.GoalDecompositionFailed), "GOAL_001")]
    [InlineData(nameof(ErrorCodes.LlmParseFailure), "LLM_001")]
    [InlineData(nameof(ErrorCodes.ConsistencyCheckFailed), "CONSISTENCY_001")]
    [InlineData(nameof(ErrorCodes.ReasoningFailed), "REASONING_001")]
    [InlineData(nameof(ErrorCodes.SecurityViolation), "SEC_001")]
    [InlineData(nameof(ErrorCodes.ValidationFailed), "VAL_001")]
    [InlineData(nameof(ErrorCodes.TimeoutExpired), "TIMEOUT_001")]
    [InlineData(nameof(ErrorCodes.ResourceNotFound), "NOTFOUND_001")]
    [InlineData(nameof(ErrorCodes.ConfigurationError), "CONFIG_001")]
    [InlineData(nameof(ErrorCodes.SerializationFailed), "SERIAL_001")]
    [InlineData(nameof(ErrorCodes.ToolExecutionFailed), "TOOL_001")]
    [InlineData(nameof(ErrorCodes.ToolNotAuthorized), "TOOL_002")]
    [InlineData(nameof(ErrorCodes.IoOperationFailed), "IO_001")]
    [InlineData(nameof(ErrorCodes.NetworkOperationFailed), "NET_001")]
    [InlineData(nameof(ErrorCodes.MemoryOperationFailed), "MEM_001")]
    [InlineData(nameof(ErrorCodes.ParseOperationFailed), "PARSE_001")]
    [InlineData(nameof(ErrorCodes.EmbeddingOperationFailed), "EMBED_001")]
    [InlineData(nameof(ErrorCodes.PipelineStepFailed), "PIPE_001")]
    [InlineData(nameof(ErrorCodes.QdrantOperationFailed), "QDRANT_001")]
    public void ErrorCode_HasExpectedValue(string fieldName, string expectedValue)
    {
        // Arrange & Act
        string actual = (string)typeof(ErrorCodes)
            .GetField(fieldName)!
            .GetValue(null)!;

        // Assert
        actual.Should().Be(expectedValue);
    }

    [Fact]
    public void ErrorCodes_FollowNamingConvention()
    {
        // Arrange
        var fields = typeof(ErrorCodes).GetFields(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        // Act & Assert - all codes follow CATEGORY_NNN pattern
        foreach (var field in fields)
        {
            string value = (string)field.GetValue(null)!;
            value.Should().MatchRegex(@"^[A-Z]+_\d{3}$",
                $"Error code '{field.Name}' with value '{value}' should follow CATEGORY_NNN pattern");
        }
    }

    [Fact]
    public void ErrorCodes_AreAllUnique()
    {
        // Arrange
        var fields = typeof(ErrorCodes).GetFields(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        // Act
        var values = fields.Select(f => (string)f.GetValue(null)!).ToList();

        // Assert
        values.Should().OnlyHaveUniqueItems();
    }
}
