using Ouroboros.Core.Learning;
using Ouroboros.Core.LawsOfForm;
using Ouroboros.Domain.Learning;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public class MockPeftIntegrationTests
{
    [Fact]
    public async Task InitializeAdapterAsync_ShouldReturnWeights()
    {
        var peft = new MockPeftIntegration();
        var config = AdapterConfig.Default();

        var result = await peft.InitializeAdapterAsync("model", config);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task TrainAdapterAsync_ShouldReturnModifiedWeights()
    {
        var peft = new MockPeftIntegration();
        var weights = new byte[1024];
        var examples = new List<TrainingExample> { new("input", "output", 1.0) };
        var config = TrainingConfig.Default();

        var result = await peft.TrainAdapterAsync("model", weights, examples, config);

        result.IsSuccess.Should().BeTrue();
        result.Value.Length.Should().Be(weights.Length);
    }

    [Fact]
    public async Task GenerateAsync_WithAdapter_ShouldReturnAdaptedResponse()
    {
        var peft = new MockPeftIntegration();
        var result = await peft.GenerateAsync("model", new byte[100], "test prompt");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("[ADAPTED]");
    }

    [Fact]
    public async Task GenerateAsync_WithoutAdapter_ShouldReturnBaseResponse()
    {
        var peft = new MockPeftIntegration();
        var result = await peft.GenerateAsync("model", null, "test prompt");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("[BASE]");
    }

    [Fact]
    public async Task ValidateAdapterAsync_ValidWeights_ShouldReturnSize()
    {
        var peft = new MockPeftIntegration();
        var weights = new byte[512];

        var result = await peft.ValidateAdapterAsync(weights);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(512);
    }

    [Fact]
    public async Task ValidateAdapterAsync_EmptyWeights_ShouldReturnFailure()
    {
        var peft = new MockPeftIntegration();
        var result = await peft.ValidateAdapterAsync(Array.Empty<byte>());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task MergeAdaptersAsync_ShouldReturnMergedWeights()
    {
        var peft = new MockPeftIntegration();
        var weights = new List<byte[]> { new byte[100], new byte[100] };

        var result = await peft.MergeAdaptersAsync("model", weights, MergeStrategy.Average);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TrainOnDistinctionAsync_ShouldReturnModifiedWeights()
    {
        var peft = new MockPeftIntegration();
        var example = new DistinctionTrainingExample("test", "distinction", DreamStage.Distinction, new float[] { 0.1f, 0.2f, 0.3f }, 0.8);
        var config = new DistinctionTrainingConfig(3, 0.001, 1.0, true);

        var result = await peft.TrainOnDistinctionAsync("model", new byte[100], example, config);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyDissolutionAsync_ShouldReturnModifiedWeights()
    {
        var peft = new MockPeftIntegration();
        var result = await peft.ApplyDissolutionAsync("model", new byte[100], new float[] { 0.5f, 0.3f }, 0.8);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MergeOnRecognitionAsync_ShouldReturnMergedWeights()
    {
        var peft = new MockPeftIntegration();
        var weights = new List<byte[]> { new byte[100], new byte[100] };

        var result = await peft.MergeOnRecognitionAsync("model", weights, new float[] { 1f, 2f });

        result.IsSuccess.Should().BeTrue();
    }
}
