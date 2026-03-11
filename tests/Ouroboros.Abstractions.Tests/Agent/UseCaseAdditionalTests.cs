using Ouroboros.Agent;

namespace Ouroboros.Abstractions.Tests.Agent;

[Trait("Category", "Unit")]
public class UseCaseAdditionalTests
{
    [Fact]
    public void UseCase_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var caps = new[] { "code", "reasoning" };
        var a = new UseCase(UseCaseType.CodeGeneration, 5, caps, 0.8, 0.2);
        var b = new UseCase(UseCaseType.CodeGeneration, 5, caps, 0.8, 0.2);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void UseCase_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new UseCase(
            UseCaseType.Reasoning, 3, new[] { "reasoning" }, 0.7, 0.3);

        // Act
        var modified = original with { EstimatedComplexity = 10 };

        // Assert
        modified.EstimatedComplexity.Should().Be(10);
        modified.Type.Should().Be(UseCaseType.Reasoning);
    }

    [Fact]
    public void UseCase_EmptyCapabilities_IsAllowed()
    {
        // Act
        var useCase = new UseCase(
            UseCaseType.Conversation, 1, Array.Empty<string>(), 0.5, 0.5);

        // Assert
        useCase.RequiredCapabilities.Should().BeEmpty();
    }
}
