// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

using Ouroboros.Abstractions;

namespace Ouroboros.Tests.Tools.MeTTa;

/// <summary>
/// Unit tests for SubprocessMeTTaEngine covering constructor behavior, interface compliance,
/// uninitiated engine error handling, and disposal.
/// Note: These tests do not require the MeTTa executable or Docker to be present;
/// they exercise the engine's behavior when the subprocess fails to start.
/// </summary>
[Trait("Category", "Unit")]
public class SubprocessMeTTaEngineTests
{
    // ========================================================================
    // Constructor
    // ========================================================================

    [Fact]
    public void Constructor_DefaultPath_DoesNotThrow()
    {
        // Act - constructor catches Win32Exception/InvalidOperationException if docker not found
        var act = () =>
        {
            using var engine = new SubprocessMeTTaEngine();
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_CustomPath_DoesNotThrow()
    {
        // Act - even with an invalid path, constructor catches exceptions
        var act = () =>
        {
            using var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_NullPath_UsesDockerMode()
    {
        // Act - null path triggers docker mode
        var act = () =>
        {
            using var engine = new SubprocessMeTTaEngine(null);
        };

        // Assert
        act.Should().NotThrow();
    }

    // ========================================================================
    // Interface compliance
    // ========================================================================

    [Fact]
    public void SubprocessMeTTaEngine_ImplementsIMeTTaEngine()
    {
        using var engine = new SubprocessMeTTaEngine("/nonexistent/path");
        engine.Should().BeAssignableTo<IMeTTaEngine>();
    }

    [Fact]
    public void SubprocessMeTTaEngine_ImplementsIDisposable()
    {
        using var engine = new SubprocessMeTTaEngine("/nonexistent/path");
        engine.Should().BeAssignableTo<IDisposable>();
    }

    // ========================================================================
    // Uninitialized engine behavior (when process fails to start)
    // ========================================================================

    [Fact]
    public async Task ExecuteQueryAsync_UninitializedEngine_ReturnsFailure()
    {
        // Arrange - use invalid path so process is null
        using var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");

        // Act
        var result = await engine.ExecuteQueryAsync("(test query)");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not initialized");
    }

    [Fact]
    public async Task AddFactAsync_UninitializedEngine_ReturnsFailure()
    {
        // Arrange
        using var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");

        // Act
        var result = await engine.AddFactAsync("(test fact)");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not initialized");
    }

    [Fact]
    public async Task ApplyRuleAsync_UninitializedEngine_ReturnsFailure()
    {
        // Arrange
        using var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");

        // Act
        var result = await engine.ApplyRuleAsync("(= (a) (b))");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not initialized");
    }

    [Fact]
    public async Task VerifyPlanAsync_UninitializedEngine_ReturnsFailure()
    {
        // Arrange - VerifyPlanAsync delegates to ExecuteQueryAsync
        using var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");

        // Act
        var result = await engine.VerifyPlanAsync("(plan step1)");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ResetAsync_UninitializedEngine_ReturnsFailure()
    {
        // Arrange
        using var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");

        // Act
        var result = await engine.ResetAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not initialized");
    }

    // ========================================================================
    // Dispose
    // ========================================================================

    [Fact]
    public void Dispose_CalledOnce_DoesNotThrow()
    {
        // Arrange
        var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");

        // Act
        var act = () => engine.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");

        // Act
        var act = () =>
        {
            engine.Dispose();
            engine.Dispose();
            engine.Dispose();
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_UninitializedEngine_DoesNotThrow()
    {
        // Arrange - engine that never started a process
        var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");

        // Act
        var act = () => engine.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    // ========================================================================
    // CancellationToken support
    // ========================================================================

    [Fact]
    public async Task ExecuteQueryAsync_CancelledToken_UninitializedEngine_ReturnsFailure()
    {
        // Arrange
        using var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act - uninitialized engine returns failure before checking cancellation
        var result = await engine.ExecuteQueryAsync("(test)", cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task AddFactAsync_CancelledToken_UninitializedEngine_ReturnsFailure()
    {
        // Arrange
        using var engine = new SubprocessMeTTaEngine("/nonexistent/path/metta");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await engine.AddFactAsync("(test)", cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
