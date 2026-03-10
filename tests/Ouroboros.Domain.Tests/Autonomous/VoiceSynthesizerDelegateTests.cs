// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

/// <summary>
/// Tests for the <see cref="VoiceSynthesizer"/> delegate.
/// </summary>
[Trait("Category", "Unit")]
public class VoiceSynthesizerDelegateTests
{
    [Fact]
    public async Task VoiceSynthesizer_CanBeInvoked()
    {
        // Arrange
        bool wasCalled = false;
        string? capturedText = null;
        PersonaVoice? capturedVoice = null;

        VoiceSynthesizer synthesizer = (text, voice, ct) =>
        {
            wasCalled = true;
            capturedText = text;
            capturedVoice = voice;
            return Task.CompletedTask;
        };

        var voice = new PersonaVoice("TestVoice");

        // Act
        await synthesizer("Hello", voice, CancellationToken.None);

        // Assert
        wasCalled.Should().BeTrue();
        capturedText.Should().Be("Hello");
        capturedVoice.Should().Be(voice);
    }

    [Fact]
    public async Task VoiceSynthesizer_SupportsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        VoiceSynthesizer synthesizer = (text, voice, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        };

        cts.Cancel();

        // Act & Assert
        Func<Task> act = () => synthesizer("Hello", new PersonaVoice("Test"), cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void VoiceSynthesizer_CanBeNull()
    {
        // Arrange & Act
        VoiceSynthesizer? synthesizer = null;

        // Assert
        synthesizer.Should().BeNull();
    }
}
