// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

/// <summary>
/// Tests for <see cref="SpeechQueue"/>. Since SpeechQueue uses a private constructor
/// (singleton pattern), we test the static Instance and public API.
/// </summary>
[Trait("Category", "Unit")]
public class SpeechQueueTests
{
    [Fact]
    public void Instance_ReturnsSameInstance()
    {
        // Act
        SpeechQueue instance1 = SpeechQueue.Instance;
        SpeechQueue instance2 = SpeechQueue.Instance;

        // Assert
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void Instance_IsNotNull()
    {
        // Act
        SpeechQueue instance = SpeechQueue.Instance;

        // Assert
        instance.Should().NotBeNull();
    }

    [Fact]
    public void SetSynthesizer_DoesNotThrow()
    {
        // Arrange
        SpeechQueue instance = SpeechQueue.Instance;
        Func<string, string, CancellationToken, Task> synthesizer = (_, _, _) => Task.CompletedTask;

        // Act
        Action act = () => instance.SetSynthesizer(synthesizer);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Enqueue_WithNullOrWhitespace_DoesNotThrow()
    {
        // Arrange
        SpeechQueue instance = SpeechQueue.Instance;

        // Act & Assert - should be no-ops for empty/whitespace
        Action act1 = () => instance.Enqueue("");
        Action act2 = () => instance.Enqueue("   ");

        act1.Should().NotThrow();
        act2.Should().NotThrow();
    }

    [Fact]
    public void Enqueue_WithValidText_DoesNotThrow()
    {
        // Arrange
        SpeechQueue instance = SpeechQueue.Instance;

        // Act
        Action act = () => instance.Enqueue("Hello world", "TestPersona");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task EnqueueAndWaitAsync_WithEmptyText_ReturnsImmediately()
    {
        // Arrange
        SpeechQueue instance = SpeechQueue.Instance;

        // Act & Assert - should return immediately for empty text
        await instance.EnqueueAndWaitAsync("", "TestPersona");
        await instance.EnqueueAndWaitAsync("   ", "TestPersona");
    }

    [Fact]
    public async Task EnqueueAndWaitAsync_WithSynthesizer_CompletesSuccessfully()
    {
        // Arrange
        SpeechQueue instance = SpeechQueue.Instance;
        bool synthesizerCalled = false;
        instance.SetSynthesizer((text, persona, ct) =>
        {
            synthesizerCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await instance.EnqueueAndWaitAsync("Test speech", "Ouroboros");

        // Assert
        synthesizerCalled.Should().BeTrue();
    }
}
