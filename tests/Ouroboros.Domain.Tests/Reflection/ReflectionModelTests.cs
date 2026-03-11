namespace Ouroboros.Tests.Reflection;

using Ouroboros.Domain.Reflection;

[Trait("Category", "Unit")]
public sealed class ReflectionModelTests
{
    #region BenchmarkTask Tests

    [Fact]
    public void BenchmarkTask_Constructor_SetsAllProperties()
    {
        // Arrange
        Func<Task<bool>> execute = () => Task.FromResult(true);
        var timeout = TimeSpan.FromSeconds(30);

        // Act
        var task = new BenchmarkTask("ReasoningTest", CognitiveDimension.Reasoning, execute, timeout);

        // Assert
        task.Name.Should().Be("ReasoningTest");
        task.Dimension.Should().Be(CognitiveDimension.Reasoning);
        task.Execute.Should().Be(execute);
        task.Timeout.Should().Be(timeout);
    }

    [Theory]
    [InlineData(CognitiveDimension.Reasoning)]
    [InlineData(CognitiveDimension.Planning)]
    [InlineData(CognitiveDimension.Learning)]
    [InlineData(CognitiveDimension.Memory)]
    [InlineData(CognitiveDimension.Generalization)]
    [InlineData(CognitiveDimension.Creativity)]
    [InlineData(CognitiveDimension.SocialIntelligence)]
    public void BenchmarkTask_AcceptsAllCognitiveDimensions(CognitiveDimension dimension)
    {
        // Act
        var task = new BenchmarkTask(
            "Test",
            dimension,
            () => Task.FromResult(true),
            TimeSpan.FromSeconds(10));

        // Assert
        task.Dimension.Should().Be(dimension);
    }

    [Fact]
    public async Task BenchmarkTask_ExecuteWithTimeoutAsync_SuccessfulTask_ReturnsTrue()
    {
        // Arrange
        var task = new BenchmarkTask(
            "FastTest",
            CognitiveDimension.Reasoning,
            () => Task.FromResult(true),
            TimeSpan.FromSeconds(5));

        // Act
        var result = await task.ExecuteWithTimeoutAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task BenchmarkTask_ExecuteWithTimeoutAsync_FailedTask_ReturnsFalse()
    {
        // Arrange
        var task = new BenchmarkTask(
            "FailTest",
            CognitiveDimension.Planning,
            () => Task.FromResult(false),
            TimeSpan.FromSeconds(5));

        // Act
        var result = await task.ExecuteWithTimeoutAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task BenchmarkTask_ExecuteWithTimeoutAsync_TimedOutTask_ReturnsFalse()
    {
        // Arrange
        var task = new BenchmarkTask(
            "SlowTest",
            CognitiveDimension.Memory,
            async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                return true;
            },
            TimeSpan.FromMilliseconds(50));

        // Act
        var result = await task.ExecuteWithTimeoutAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task BenchmarkTask_ExecuteWithTimeoutAsync_ThrowingTask_ReturnsFalse()
    {
        // Arrange
        var task = new BenchmarkTask(
            "ErrorTest",
            CognitiveDimension.Learning,
            () => throw new InvalidOperationException("test error"),
            TimeSpan.FromSeconds(5));

        // Act
        var result = await task.ExecuteWithTimeoutAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task BenchmarkTask_ExecuteWithTimeoutAsync_CancelledToken_ReturnsFalse()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var task = new BenchmarkTask(
            "CancelTest",
            CognitiveDimension.Generalization,
            async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                return true;
            },
            TimeSpan.FromSeconds(10));

        // Act
        var result = await task.ExecuteWithTimeoutAsync(cts.Token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void BenchmarkTask_RecordEquality_SameExecuteDelegate()
    {
        // Arrange
        Func<Task<bool>> execute = () => Task.FromResult(true);
        var timeout = TimeSpan.FromSeconds(5);

        // Act
        var t1 = new BenchmarkTask("Test", CognitiveDimension.Reasoning, execute, timeout);
        var t2 = new BenchmarkTask("Test", CognitiveDimension.Reasoning, execute, timeout);

        // Assert
        t1.Should().Be(t2);
    }

    [Fact]
    public void BenchmarkTask_With_CreatesModifiedCopy()
    {
        // Arrange
        var original = new BenchmarkTask(
            "Original",
            CognitiveDimension.Reasoning,
            () => Task.FromResult(true),
            TimeSpan.FromSeconds(5));

        // Act
        var modified = original with { Name = "Modified", Timeout = TimeSpan.FromSeconds(10) };

        // Assert
        modified.Name.Should().Be("Modified");
        modified.Timeout.Should().Be(TimeSpan.FromSeconds(10));
        modified.Dimension.Should().Be(CognitiveDimension.Reasoning);
        original.Name.Should().Be("Original");
    }

    #endregion

    #region CognitiveDimension Enum Tests

    [Theory]
    [InlineData(CognitiveDimension.Reasoning)]
    [InlineData(CognitiveDimension.Planning)]
    [InlineData(CognitiveDimension.Learning)]
    [InlineData(CognitiveDimension.Memory)]
    [InlineData(CognitiveDimension.Generalization)]
    [InlineData(CognitiveDimension.Creativity)]
    [InlineData(CognitiveDimension.SocialIntelligence)]
    public void CognitiveDimension_AllValues_AreDefined(CognitiveDimension dimension)
    {
        Enum.IsDefined(dimension).Should().BeTrue();
    }

    [Fact]
    public void CognitiveDimension_HasSevenValues()
    {
        Enum.GetValues<CognitiveDimension>().Should().HaveCount(7);
    }

    [Fact]
    public void CognitiveDimension_UndefinedValue_IsNotDefined()
    {
        Enum.IsDefined((CognitiveDimension)999).Should().BeFalse();
    }

    #endregion

    #region ErrorPattern Tests

    [Fact]
    public void ErrorPattern_Constructor_SetsAllProperties()
    {
        // Arrange
        var episode = new FailedEpisode(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Goal",
            "Reason",
            "trace",
            new Dictionary<string, object>());
        var examples = new List<FailedEpisode> { episode };

        // Act
        var pattern = new ErrorPattern("Timeout errors", 5, examples, "Add retry logic");

        // Assert
        pattern.Description.Should().Be("Timeout errors");
        pattern.Frequency.Should().Be(5);
        pattern.Examples.Should().HaveCount(1);
        pattern.SuggestedFix.Should().Be("Add retry logic");
    }

    [Fact]
    public void ErrorPattern_NullSuggestedFix_IsAllowed()
    {
        // Act
        var pattern = new ErrorPattern(
            "Unknown error",
            1,
            new List<FailedEpisode>(),
            null);

        // Assert
        pattern.SuggestedFix.Should().BeNull();
    }

    [Fact]
    public void ErrorPattern_SeverityScore_EmptyExamples_ReturnsZero()
    {
        // Act
        var pattern = new ErrorPattern(
            "No examples",
            10,
            new List<FailedEpisode>(),
            null);

        // Assert
        pattern.SeverityScore.Should().Be(0.0);
    }

    [Fact]
    public void ErrorPattern_SeverityScore_HighFrequency_ReturnsHighScore()
    {
        // Arrange - frequency of 10 gives frequencyScore of 1.0
        var episode = new FailedEpisode(
            Guid.NewGuid(),
            DateTime.UtcNow, // very recent
            "Goal",
            "Reason",
            "trace",
            new Dictionary<string, object>());

        // Act
        var pattern = new ErrorPattern(
            "Frequent error",
            10,
            new List<FailedEpisode> { episode },
            null);

        // Assert - frequencyScore=1.0 * 0.7 + recencyScore~1.0 * 0.3 = ~1.0
        pattern.SeverityScore.Should().BeGreaterThan(0.9);
    }

    [Fact]
    public void ErrorPattern_SeverityScore_LowFrequency_ReturnsLowerScore()
    {
        // Arrange - frequency of 1 gives frequencyScore of 0.1
        var episode = new FailedEpisode(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Goal",
            "Reason",
            "trace",
            new Dictionary<string, object>());

        // Act
        var pattern = new ErrorPattern(
            "Rare error",
            1,
            new List<FailedEpisode> { episode },
            null);

        // Assert - frequencyScore=0.1 * 0.7 + recencyScore~1.0 * 0.3 = ~0.37
        pattern.SeverityScore.Should().BeLessThan(0.5);
    }

    [Fact]
    public void ErrorPattern_SeverityScore_OldExamples_ReducesRecencyComponent()
    {
        // Arrange - errors from 60 days ago, recencyScore = max(0, 1 - 60/30) = 0
        var episode = new FailedEpisode(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-60),
            "Goal",
            "Reason",
            "trace",
            new Dictionary<string, object>());

        // Act
        var pattern = new ErrorPattern(
            "Old error",
            10,
            new List<FailedEpisode> { episode },
            null);

        // Assert - frequencyScore=1.0 * 0.7 + recencyScore=0 * 0.3 = 0.7
        pattern.SeverityScore.Should().BeApproximately(0.7, 0.05);
    }

    [Fact]
    public void ErrorPattern_SeverityScore_FrequencyCappedAtTen()
    {
        // Arrange - frequency of 20, but capped at 1.0 (10/10)
        var episode = new FailedEpisode(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Goal",
            "Reason",
            "trace",
            new Dictionary<string, object>());

        var patternAt10 = new ErrorPattern("A", 10, new List<FailedEpisode> { episode }, null);
        var patternAt20 = new ErrorPattern("B", 20, new List<FailedEpisode> { episode }, null);

        // Assert - both should have same score since frequency caps at 10
        patternAt10.SeverityScore.Should().BeApproximately(patternAt20.SeverityScore, 0.001);
    }

    [Fact]
    public void ErrorPattern_SeverityScore_MultipleExamples_AveragesRecency()
    {
        // Arrange - one recent, one old
        var recentEpisode = new FailedEpisode(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Goal",
            "Reason",
            "trace",
            new Dictionary<string, object>());
        var oldEpisode = new FailedEpisode(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-30),
            "Goal",
            "Reason",
            "trace",
            new Dictionary<string, object>());

        // Act
        var pattern = new ErrorPattern(
            "Mixed age errors",
            5,
            new List<FailedEpisode> { recentEpisode, oldEpisode },
            null);

        // Assert - average recency is ~15 days, so recencyScore = max(0, 1 - 15/30) = 0.5
        // frequencyScore = min(1, 5/10) = 0.5
        // total = 0.5 * 0.7 + 0.5 * 0.3 = 0.5
        pattern.SeverityScore.Should().BeApproximately(0.5, 0.1);
    }

    [Fact]
    public void ErrorPattern_RecordEquality_SameValues()
    {
        // Arrange
        var examples = new List<FailedEpisode>();

        // Act
        var p1 = new ErrorPattern("Error", 3, examples, "Fix");
        var p2 = new ErrorPattern("Error", 3, examples, "Fix");

        // Assert
        p1.Should().Be(p2);
    }

    #endregion

    #region FailedEpisode Tests

    [Fact]
    public void FailedEpisode_Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var timestamp = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var context = new Dictionary<string, object>
        {
            ["model"] = "gpt-4",
            ["temperature"] = 0.7,
        };

        // Act
        var episode = new FailedEpisode(
            id, timestamp, "Solve puzzle", "Out of memory",
            "step1 -> step2 -> fail", context);

        // Assert
        episode.Id.Should().Be(id);
        episode.Timestamp.Should().Be(timestamp);
        episode.Goal.Should().Be("Solve puzzle");
        episode.FailureReason.Should().Be("Out of memory");
        episode.ReasoningTrace.Should().Be("step1 -> step2 -> fail");
        episode.Context.Should().HaveCount(2);
        episode.Context["model"].Should().Be("gpt-4");
    }

    [Fact]
    public void FailedEpisode_WithEmptyContext_IsAllowed()
    {
        // Act
        var episode = new FailedEpisode(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Goal",
            "Reason",
            "trace",
            new Dictionary<string, object>());

        // Assert
        episode.Context.Should().BeEmpty();
    }

    [Fact]
    public void FailedEpisode_WithComplexReasoningTrace_StoresObject()
    {
        // Arrange
        var complexTrace = new { Step = 1, Action = "analyze", Result = "error" };

        // Act
        var episode = new FailedEpisode(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Goal",
            "Reason",
            complexTrace,
            new Dictionary<string, object>());

        // Assert
        episode.ReasoningTrace.Should().Be(complexTrace);
    }

    [Fact]
    public void FailedEpisode_RecordEquality_SameValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var context = new Dictionary<string, object> { ["key"] = "val" };
        var trace = "trace data";

        // Act
        var e1 = new FailedEpisode(id, timestamp, "Goal", "Reason", trace, context);
        var e2 = new FailedEpisode(id, timestamp, "Goal", "Reason", trace, context);

        // Assert
        e1.Should().Be(e2);
    }

    [Fact]
    public void FailedEpisode_DifferentIds_AreNotEqual()
    {
        // Arrange
        var context = new Dictionary<string, object>();

        // Act
        var e1 = new FailedEpisode(Guid.NewGuid(), DateTime.UtcNow, "Goal", "Reason", "trace", context);
        var e2 = new FailedEpisode(Guid.NewGuid(), DateTime.UtcNow, "Goal", "Reason", "trace", context);

        // Assert
        e1.Should().NotBe(e2);
    }

    [Fact]
    public void FailedEpisode_With_CreatesModifiedCopy()
    {
        // Arrange
        var original = new FailedEpisode(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Original goal",
            "Original reason",
            "trace",
            new Dictionary<string, object>());

        // Act
        var modified = original with { Goal = "Modified goal", FailureReason = "New reason" };

        // Assert
        modified.Goal.Should().Be("Modified goal");
        modified.FailureReason.Should().Be("New reason");
        modified.Id.Should().Be(original.Id);
        original.Goal.Should().Be("Original goal");
    }

    [Fact]
    public void FailedEpisode_ContextIsReadOnly()
    {
        // Arrange
        var context = new Dictionary<string, object> { ["key"] = "val" };
        var episode = new FailedEpisode(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Goal",
            "Reason",
            "trace",
            context);

        // Assert - IReadOnlyDictionary does not expose Add/Remove
        episode.Context.Should().BeAssignableTo<IReadOnlyDictionary<string, object>>();
    }

    #endregion
}
