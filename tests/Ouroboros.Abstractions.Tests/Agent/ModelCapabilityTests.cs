using Ouroboros.Agent;

namespace Ouroboros.Abstractions.Tests.Agent;

[Trait("Category", "Unit")]
public class ModelCapabilityTests
{
    [Fact]
    public void ModelCapability_AllPropertiesSet()
    {
        // Act
        var capability = new ModelCapability(
            ModelName: "gpt-4",
            Strengths: new[] { "reasoning", "code" },
            MaxTokens: 8192,
            AverageCost: 0.03,
            AverageLatencyMs: 2000.0,
            Type: ModelType.Reasoning);

        // Assert
        capability.ModelName.Should().Be("gpt-4");
        capability.Strengths.Should().Contain("reasoning");
        capability.MaxTokens.Should().Be(8192);
        capability.AverageCost.Should().Be(0.03);
        capability.AverageLatencyMs.Should().Be(2000.0);
        capability.Type.Should().Be(ModelType.Reasoning);
    }

    [Fact]
    public void ModelType_AllValuesAreDefined()
    {
        // Act
        var values = Enum.GetValues<ModelType>();

        // Assert
        values.Should().Contain(ModelType.General);
        values.Should().Contain(ModelType.Code);
        values.Should().Contain(ModelType.Reasoning);
        values.Should().Contain(ModelType.Creative);
        values.Should().Contain(ModelType.Summary);
        values.Should().Contain(ModelType.Analysis);
    }

    [Fact]
    public void UseCase_AllPropertiesSet()
    {
        // Act
        var useCase = new UseCase(
            Type: UseCaseType.CodeGeneration,
            EstimatedComplexity: 7,
            RequiredCapabilities: new[] { "code", "reasoning" },
            PerformanceWeight: 0.8,
            CostWeight: 0.2);

        // Assert
        useCase.Type.Should().Be(UseCaseType.CodeGeneration);
        useCase.EstimatedComplexity.Should().Be(7);
        useCase.RequiredCapabilities.Should().HaveCount(2);
        useCase.PerformanceWeight.Should().Be(0.8);
        useCase.CostWeight.Should().Be(0.2);
    }

    [Fact]
    public void UseCaseType_AllValuesAreDefined()
    {
        // Act
        var values = Enum.GetValues<UseCaseType>();

        // Assert
        values.Should().Contain(UseCaseType.CodeGeneration);
        values.Should().Contain(UseCaseType.Reasoning);
        values.Should().Contain(UseCaseType.Creative);
        values.Should().Contain(UseCaseType.Summarization);
        values.Should().Contain(UseCaseType.Analysis);
        values.Should().Contain(UseCaseType.Conversation);
        values.Should().Contain(UseCaseType.ToolUse);
    }

    [Fact]
    public void PerformanceMetrics_Agent_AllPropertiesSet()
    {
        // Act
        var metrics = new Ouroboros.Agent.PerformanceMetrics(
            ResourceName: "gpt-4",
            ExecutionCount: 100,
            AverageLatencyMs: 1500.0,
            SuccessRate: 0.95,
            LastUsed: DateTime.UtcNow,
            CustomMetrics: new Dictionary<string, double> { ["tokens"] = 50000 });

        // Assert
        metrics.ResourceName.Should().Be("gpt-4");
        metrics.ExecutionCount.Should().Be(100);
        metrics.AverageLatencyMs.Should().Be(1500.0);
        metrics.SuccessRate.Should().Be(0.95);
        metrics.CustomMetrics.Should().ContainKey("tokens");
    }
}
