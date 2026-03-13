namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Tools.MeTTa;

/// <summary>
/// Tests for SubprocessMeTTaEngine. Since the engine depends on external processes,
/// tests verify behavior when the process is not available.
/// </summary>
[Trait("Category", "Unit")]
public class SubprocessMeTTaEngineTests
{
    [Fact]
    public void Constructor_InvalidPath_DoesNotThrow()
    {
        var act = () => new SubprocessMeTTaEngine("/nonexistent/path/metta");
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ExecuteQueryAsync_NoProcess_ReturnsFailure()
    {
        using var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");
        var result = await engine.ExecuteQueryAsync("(+ 1 2)");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AddFactAsync_NoProcess_ReturnsFailure()
    {
        using var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");
        var result = await engine.AddFactAsync("(fact a b)");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ApplyRuleAsync_NoProcess_ReturnsFailure()
    {
        using var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");
        var result = await engine.ApplyRuleAsync("(rule a b)");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyPlanAsync_NoProcess_ReturnsFailure()
    {
        using var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");
        var result = await engine.VerifyPlanAsync("plan");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ResetAsync_NoProcess_ReturnsFailure()
    {
        using var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");
        var result = await engine.ResetAsync();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");
        engine.Dispose();
        var act = () => engine.Dispose();
        act.Should().NotThrow();
    }
}
