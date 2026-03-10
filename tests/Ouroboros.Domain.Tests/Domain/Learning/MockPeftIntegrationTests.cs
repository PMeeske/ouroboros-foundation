// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Learning;

using Ouroboros.Core.LawsOfForm;
using Ouroboros.Core.Learning;
using Ouroboros.Core.Monads;
using Ouroboros.Core.Randomness;
using Ouroboros.Domain.Learning;
using Ouroboros.Providers.Random;

/// <summary>
/// Tests for <see cref="MockPeftIntegration"/>.
/// </summary>
[Trait("Category", "Unit")]
public class MockPeftIntegrationTests
{
    private readonly MockPeftIntegration _sut;

    public MockPeftIntegrationTests()
    {
        _sut = new MockPeftIntegration(randomProvider: new SeededRandomProvider(42));
    }

    // ----------------------------------------------------------------
    // InitializeAdapterAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task InitializeAdapterAsync_ReturnsSuccessWithWeights()
    {
        // Arrange
        var config = new AdapterConfig { Rank = 4 };

        // Act
        Result<byte[], string> result = await _sut.InitializeAdapterAsync("test-model", config);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Length.Should().Be(4 * 1024); // Rank * 1024
    }

    [Fact]
    public async Task InitializeAdapterAsync_DifferentRanks_ProducesDifferentSizes()
    {
        // Arrange
        var config2 = new AdapterConfig { Rank = 2 };
        var config8 = new AdapterConfig { Rank = 8 };

        // Act
        Result<byte[], string> result2 = await _sut.InitializeAdapterAsync("model", config2);
        Result<byte[], string> result8 = await _sut.InitializeAdapterAsync("model", config8);

        // Assert
        result2.Value.Length.Should().Be(2 * 1024);
        result8.Value.Length.Should().Be(8 * 1024);
    }

    // ----------------------------------------------------------------
    // TrainAdapterAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task TrainAdapterAsync_ReturnsModifiedWeights()
    {
        // Arrange
        byte[] initialWeights = new byte[100];
        Array.Fill<byte>(initialWeights, 50);
        var examples = new List<TrainingExample>
        {
            new("input1", "output1"),
            new("input2", "output2"),
        };
        var config = new TrainingConfig { Epochs = 1 };

        // Act
        Result<byte[], string> result = await _sut.TrainAdapterAsync("model", initialWeights, examples, config);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Length.Should().Be(initialWeights.Length);
    }

    // ----------------------------------------------------------------
    // GenerateAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task GenerateAsync_WithAdapter_ReturnsAdaptedResponse()
    {
        // Arrange
        byte[] adapterWeights = new byte[] { 1, 2, 3 };

        // Act
        Result<string, string> result = await _sut.GenerateAsync("model", adapterWeights, "Hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("[ADAPTED]");
        result.Value.Should().Contain("Hello");
    }

    [Fact]
    public async Task GenerateAsync_WithoutAdapter_ReturnsBaseResponse()
    {
        // Act
        Result<string, string> result = await _sut.GenerateAsync("model", null, "Hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("[BASE]");
    }

    // ----------------------------------------------------------------
    // MergeAdaptersAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task MergeAdaptersAsync_MultipleAdapters_ReturnsMergedWeights()
    {
        // Arrange
        var adapters = new List<byte[]>
        {
            new byte[] { 10, 20, 30 },
            new byte[] { 20, 40, 60 },
        };

        // Act
        Result<byte[], string> result = await _sut.MergeAdaptersAsync("model", adapters, MergeStrategy.Average);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Length.Should().Be(3);
        result.Value[0].Should().Be(15); // (10+20)/2
        result.Value[1].Should().Be(30); // (20+40)/2
        result.Value[2].Should().Be(45); // (30+60)/2
    }

    [Fact]
    public async Task MergeAdaptersAsync_EmptyList_ReturnsFailure()
    {
        // Act
        Result<byte[], string> result = await _sut.MergeAdaptersAsync("model", new List<byte[]>(), MergeStrategy.Average);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No adapters");
    }

    // ----------------------------------------------------------------
    // ValidateAdapterAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task ValidateAdapterAsync_ValidWeights_ReturnsSize()
    {
        // Arrange
        byte[] weights = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        Result<long, string> result = await _sut.ValidateAdapterAsync(weights);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task ValidateAdapterAsync_EmptyWeights_ReturnsFailure()
    {
        // Act
        Result<long, string> result = await _sut.ValidateAdapterAsync(Array.Empty<byte>());

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAdapterAsync_NullWeights_ReturnsFailure()
    {
        // Act
        Result<long, string> result = await _sut.ValidateAdapterAsync(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // TrainOnDistinctionAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task TrainOnDistinctionAsync_ReturnsModifiedWeights()
    {
        // Arrange
        byte[] weights = new byte[100];
        Array.Fill<byte>(weights, 128);
        var example = new DistinctionTrainingExample(
            "arithmetic",
            "2+2=4",
            DreamStage.Distinction);
        var config = new DistinctionTrainingConfig();

        // Act
        Result<byte[], string> result = await _sut.TrainOnDistinctionAsync("model", weights, example, config);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Length.Should().Be(100);
    }

    // ----------------------------------------------------------------
    // ApplyDissolutionAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task ApplyDissolutionAsync_ReturnsReducedWeights()
    {
        // Arrange
        byte[] weights = new byte[] { 100, 200, 100 };
        float[] mask = new float[] { 0.5f, 0.5f, 0.5f };

        // Act
        Result<byte[], string> result = await _sut.ApplyDissolutionAsync("model", weights, mask, 0.5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Length.Should().Be(3);
        // Each weight should be reduced: weight * (1 - mask * strength)
        result.Value[0].Should().BeLessThan(100);
    }

    // ----------------------------------------------------------------
    // MergeOnRecognitionAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task MergeOnRecognitionAsync_MultipleWeights_ReturnsMerged()
    {
        // Arrange
        var weightsList = new List<byte[]>
        {
            new byte[] { 16, 81, 64 },
            new byte[] { 16, 81, 64 },
        };
        float[] selfEmbedding = new float[] { 1.0f, 0.5f };

        // Act
        Result<byte[], string> result = await _sut.MergeOnRecognitionAsync("model", weightsList, selfEmbedding);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Length.Should().Be(3);
    }

    [Fact]
    public async Task MergeOnRecognitionAsync_EmptyWeightsList_ReturnsFailure()
    {
        // Act
        Result<byte[], string> result = await _sut.MergeOnRecognitionAsync(
            "model", new List<byte[]>(), new float[] { 1.0f });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No weights");
    }
}
