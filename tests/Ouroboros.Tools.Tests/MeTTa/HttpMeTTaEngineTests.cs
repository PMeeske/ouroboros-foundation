namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Tools.MeTTa;

/// <summary>
/// Tests for HttpMeTTaEngine. Tests focus on construction and dispose behavior
/// since actual HTTP calls require a running server.
/// </summary>
[Trait("Category", "Unit")]
public class HttpMeTTaEngineTests
{
    [Fact]
    public void Constructor_ValidUrl_CreatesEngine()
    {
        using var engine = new HttpMeTTaEngine("http://localhost:5000");
        engine.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithApiKey_CreatesEngine()
    {
        using var engine = new HttpMeTTaEngine("http://localhost:5000", "test-api-key");
        engine.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_TrailingSlash_TrimsUrl()
    {
        var act = () => new HttpMeTTaEngine("http://localhost:5000/");
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        var engine = new HttpMeTTaEngine("http://localhost:5000");
        engine.Dispose();
        var act = () => engine.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ExecuteQueryAsync_InvalidServer_ReturnsFailure()
    {
        using var engine = new HttpMeTTaEngine("http://localhost:99999");
        var result = await engine.ExecuteQueryAsync("test");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AddFactAsync_InvalidServer_ReturnsFailure()
    {
        using var engine = new HttpMeTTaEngine("http://localhost:99999");
        var result = await engine.AddFactAsync("(fact a)");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ApplyRuleAsync_InvalidServer_ReturnsFailure()
    {
        using var engine = new HttpMeTTaEngine("http://localhost:99999");
        var result = await engine.ApplyRuleAsync("(rule a)");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyPlanAsync_InvalidServer_ReturnsFailure()
    {
        using var engine = new HttpMeTTaEngine("http://localhost:99999");
        var result = await engine.VerifyPlanAsync("plan");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ResetAsync_InvalidServer_ReturnsFailure()
    {
        using var engine = new HttpMeTTaEngine("http://localhost:99999");
        var result = await engine.ResetAsync();
        result.IsSuccess.Should().BeFalse();
    }
}
