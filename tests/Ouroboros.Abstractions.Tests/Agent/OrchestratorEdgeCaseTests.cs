using Ouroboros.Agent;

namespace Ouroboros.Abstractions.Tests.Agent;

/// <summary>
/// Additional edge case tests for Orchestrator types covering
/// record equality, boundary conditions, and method logic.
/// </summary>
[Trait("Category", "Unit")]
public class OrchestratorEdgeCaseTests
{
    // --- OrchestratorConfig ---

    [Fact]
    public void OrchestratorConfig_Default_AllBoolsAreTrue()
    {
        // Act
        var config = OrchestratorConfig.Default();

        // Assert
        config.EnableTracing.Should().BeTrue();
        config.EnableMetrics.Should().BeTrue();
        config.EnableSafetyChecks.Should().BeTrue();
    }

    [Fact]
    public void OrchestratorConfig_Default_TimeoutIsNull()
    {
        // Act
        var config = OrchestratorConfig.Default();

        // Assert
        config.ExecutionTimeout.Should().BeNull();
        config.RetryConfig.Should().BeNull();
    }

    [Fact]
    public void OrchestratorConfig_Default_CustomSettingsIsEmpty()
    {
        // Act
        var config = OrchestratorConfig.Default();

        // Assert
        config.CustomSettings.Should().BeEmpty();
    }

    [Fact]
    public void OrchestratorConfig_GetSetting_ExistingKey_ReturnsValue()
    {
        // Arrange
        var config = new OrchestratorConfig
        {
            CustomSettings = new Dictionary<string, object> { ["maxTokens"] = 1000 }
        };

        // Act
        var result = config.GetSetting<int>("maxTokens");

        // Assert
        result.Should().Be(1000);
    }

    [Fact]
    public void OrchestratorConfig_GetSetting_MissingKey_ReturnsDefault()
    {
        // Arrange
        var config = OrchestratorConfig.Default();

        // Act
        var result = config.GetSetting<int>("nonExistent", 42);

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public void OrchestratorConfig_GetSetting_WrongType_ReturnsDefault()
    {
        // Arrange
        var config = new OrchestratorConfig
        {
            CustomSettings = new Dictionary<string, object> { ["maxTokens"] = "not-an-int" }
        };

        // Act
        var result = config.GetSetting<int>("maxTokens", 99);

        // Assert
        result.Should().Be(99);
    }

    [Fact]
    public void OrchestratorConfig_WithExpression_DisablesFeatures()
    {
        // Arrange
        var original = OrchestratorConfig.Default();

        // Act
        var modified = original with
        {
            EnableTracing = false,
            EnableMetrics = false,
            EnableSafetyChecks = false
        };

        // Assert
        modified.EnableTracing.Should().BeFalse();
        modified.EnableMetrics.Should().BeFalse();
        modified.EnableSafetyChecks.Should().BeFalse();
    }

    [Fact]
    public void OrchestratorConfig_WithExpression_SetsTimeout()
    {
        // Arrange
        var original = OrchestratorConfig.Default();

        // Act
        var modified = original with { ExecutionTimeout = TimeSpan.FromSeconds(30) };

        // Assert
        modified.ExecutionTimeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    // --- OrchestratorContext ---

    [Fact]
    public void OrchestratorContext_Create_GeneratesOperationId()
    {
        // Act
        var context = OrchestratorContext.Create();

        // Assert
        context.OperationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void OrchestratorContext_Create_WithNullMetadata_CreatesEmptyDictionary()
    {
        // Act
        var context = OrchestratorContext.Create(metadata: null);

        // Assert
        context.Metadata.Should().NotBeNull();
        context.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void OrchestratorContext_Create_TwoCallsGenerateDifferentIds()
    {
        // Act
        var a = OrchestratorContext.Create();
        var b = OrchestratorContext.Create();

        // Assert
        a.OperationId.Should().NotBe(b.OperationId);
    }

    [Fact]
    public void OrchestratorContext_GetMetadata_ExistingKey_ReturnsValue()
    {
        // Arrange
        var context = OrchestratorContext.Create(
            new Dictionary<string, object> { ["user"] = "admin" });

        // Act
        var result = context.GetMetadata<string>("user");

        // Assert
        result.Should().Be("admin");
    }

    [Fact]
    public void OrchestratorContext_GetMetadata_MissingKey_ReturnsDefault()
    {
        // Arrange
        var context = OrchestratorContext.Create();

        // Act
        var result = context.GetMetadata<string>("missing", "fallback");

        // Assert
        result.Should().Be("fallback");
    }

    [Fact]
    public void OrchestratorContext_WithMetadata_AddsNewEntry()
    {
        // Arrange
        var context = OrchestratorContext.Create();

        // Act
        var updated = context.WithMetadata("key", "value");

        // Assert
        updated.GetMetadata<string>("key").Should().Be("value");
        updated.OperationId.Should().Be(context.OperationId);
    }

    [Fact]
    public void OrchestratorContext_WithMetadata_DoesNotMutateOriginal()
    {
        // Arrange
        var context = OrchestratorContext.Create();

        // Act
        _ = context.WithMetadata("key", "value");

        // Assert
        context.Metadata.Should().NotContainKey("key");
    }

    [Fact]
    public void OrchestratorContext_WithMetadata_OverwritesExistingKey()
    {
        // Arrange
        var context = OrchestratorContext.Create(
            new Dictionary<string, object> { ["key"] = "original" });

        // Act
        var updated = context.WithMetadata("key", "overwritten");

        // Assert
        updated.GetMetadata<string>("key").Should().Be("overwritten");
    }

    // --- OrchestratorMetrics ---

    [Fact]
    public void OrchestratorMetrics_Initial_AllCountsAreZero()
    {
        // Act
        var metrics = OrchestratorMetrics.Initial("TestOrch");

        // Assert
        metrics.OrchestratorName.Should().Be("TestOrch");
        metrics.TotalExecutions.Should().Be(0);
        metrics.SuccessfulExecutions.Should().Be(0);
        metrics.FailedExecutions.Should().Be(0);
        metrics.AverageLatencyMs.Should().Be(0.0);
        metrics.SuccessRate.Should().Be(0.0);
    }

    [Fact]
    public void OrchestratorMetrics_CalculatedSuccessRate_ZeroExecutions_ReturnsZero()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch");

        // Assert
        metrics.CalculatedSuccessRate.Should().Be(0.0);
    }

    [Fact]
    public void OrchestratorMetrics_CalculatedSuccessRate_AfterExecutions_ReturnsCorrectRate()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch")
            .RecordExecution(100, true)
            .RecordExecution(200, true)
            .RecordExecution(300, false);

        // Assert
        metrics.CalculatedSuccessRate.Should().BeApproximately(2.0 / 3.0, 0.001);
    }

    [Fact]
    public void OrchestratorMetrics_RecordExecution_Success_IncrementsCorrectCounters()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch");

        // Act
        var updated = metrics.RecordExecution(150.0, true);

        // Assert
        updated.TotalExecutions.Should().Be(1);
        updated.SuccessfulExecutions.Should().Be(1);
        updated.FailedExecutions.Should().Be(0);
        updated.AverageLatencyMs.Should().Be(150.0);
        updated.SuccessRate.Should().Be(1.0);
    }

    [Fact]
    public void OrchestratorMetrics_RecordExecution_Failure_IncrementsCorrectCounters()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch");

        // Act
        var updated = metrics.RecordExecution(500.0, false);

        // Assert
        updated.TotalExecutions.Should().Be(1);
        updated.SuccessfulExecutions.Should().Be(0);
        updated.FailedExecutions.Should().Be(1);
        updated.SuccessRate.Should().Be(0.0);
    }

    [Fact]
    public void OrchestratorMetrics_RecordExecution_AverageLatency_ComputedCorrectly()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch");

        // Act
        var updated = metrics
            .RecordExecution(100.0, true)
            .RecordExecution(200.0, true);

        // Assert
        updated.AverageLatencyMs.Should().BeApproximately(150.0, 0.001);
    }

    [Fact]
    public void OrchestratorMetrics_RecordExecution_DoesNotMutateOriginal()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch");

        // Act
        _ = metrics.RecordExecution(100.0, true);

        // Assert
        metrics.TotalExecutions.Should().Be(0);
    }

    [Fact]
    public void OrchestratorMetrics_GetCustomMetric_ExistingKey_ReturnsValue()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch")
            .WithCustomMetric("throughput", 42.5);

        // Act
        var result = metrics.GetCustomMetric("throughput");

        // Assert
        result.Should().Be(42.5);
    }

    [Fact]
    public void OrchestratorMetrics_GetCustomMetric_MissingKey_ReturnsDefault()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch");

        // Act
        var result = metrics.GetCustomMetric("missing", 99.9);

        // Assert
        result.Should().Be(99.9);
    }

    [Fact]
    public void OrchestratorMetrics_WithCustomMetric_DoesNotMutateOriginal()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch");

        // Act
        _ = metrics.WithCustomMetric("key", 1.0);

        // Assert
        metrics.CustomMetrics.Should().NotContainKey("key");
    }

    // --- OrchestratorResult<T> ---

    [Fact]
    public void OrchestratorResult_Ok_SetsPropertiesCorrectly()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch");
        var time = TimeSpan.FromMilliseconds(250);

        // Act
        var result = OrchestratorResult<string>.Ok("output", metrics, time);

        // Assert
        result.Success.Should().BeTrue();
        result.Output.Should().Be("output");
        result.ErrorMessage.Should().BeNull();
        result.ExecutionTime.Should().Be(time);
        result.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void OrchestratorResult_Failure_SetsPropertiesCorrectly()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch");
        var time = TimeSpan.FromMilliseconds(100);

        // Act
        var result = OrchestratorResult<string>.Failure("error occurred", metrics, time);

        // Assert
        result.Success.Should().BeFalse();
        result.Output.Should().BeNull();
        result.ErrorMessage.Should().Be("error occurred");
    }

    [Fact]
    public void OrchestratorResult_Ok_WithMetadata_MetadataAccessible()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch");
        var metadata = new Dictionary<string, object> { ["traceId"] = "abc-123" };

        // Act
        var result = OrchestratorResult<int>.Ok(42, metrics, TimeSpan.Zero, metadata);

        // Assert
        result.GetMetadata<string>("traceId").Should().Be("abc-123");
    }

    [Fact]
    public void OrchestratorResult_ToResult_Success_ReturnsSuccessResult()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch");
        var result = OrchestratorResult<string>.Ok("data", metrics, TimeSpan.Zero);

        // Act
        var monadResult = result.ToResult();

        // Assert
        monadResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void OrchestratorResult_ToResult_Failure_ReturnsFailureResult()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch");
        var result = OrchestratorResult<string>.Failure("failed", metrics, TimeSpan.Zero);

        // Act
        var monadResult = result.ToResult();

        // Assert
        monadResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void OrchestratorResult_GetMetadata_MissingKey_ReturnsDefault()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("TestOrch");
        var result = OrchestratorResult<string>.Ok("data", metrics, TimeSpan.Zero);

        // Act
        var value = result.GetMetadata<int>("missing", -1);

        // Assert
        value.Should().Be(-1);
    }

    // --- RetryConfig ---

    [Fact]
    public void RetryConfig_DefaultConstructor_SetsExpectedDefaults()
    {
        // Act
        var config = new RetryConfig();

        // Assert
        config.MaxRetries.Should().Be(3);
        config.BackoffMultiplier.Should().Be(2.0);
        config.InitialDelay.Should().Be(TimeSpan.FromMilliseconds(100));
        config.MaxDelay.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void RetryConfig_Default_SameAsParameterlessConstructor()
    {
        // Act
        var fromDefault = RetryConfig.Default();
        var fromConstructor = new RetryConfig();

        // Assert
        fromDefault.MaxRetries.Should().Be(fromConstructor.MaxRetries);
        fromDefault.BackoffMultiplier.Should().Be(fromConstructor.BackoffMultiplier);
        fromDefault.InitialDelay.Should().Be(fromConstructor.InitialDelay);
        fromDefault.MaxDelay.Should().Be(fromConstructor.MaxDelay);
    }

    [Fact]
    public void RetryConfig_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = RetryConfig.Default();

        // Act
        var modified = original with { MaxRetries = 10, BackoffMultiplier = 3.0 };

        // Assert
        modified.MaxRetries.Should().Be(10);
        modified.BackoffMultiplier.Should().Be(3.0);
        modified.InitialDelay.Should().Be(original.InitialDelay);
    }

    [Fact]
    public void RetryConfig_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new RetryConfig();
        var b = new RetryConfig();

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void RetryConfig_RecordEquality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var a = new RetryConfig();
        var b = a with { MaxRetries = 99 };

        // Assert
        a.Should().NotBe(b);
    }
}
