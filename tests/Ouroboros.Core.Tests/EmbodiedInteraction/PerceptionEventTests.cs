using Ouroboros.Core.EmbodiedInteraction;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class PerceptionEventTests
{
    // PerceptionEvent is abstract, test through concrete subclass
    [Fact]
    public void TextPerception_Construction_SetsAllProperties()
    {
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var perception = new TextPerception(id, timestamp, 0.95, "Hello world", "user");

        perception.Id.Should().Be(id);
        perception.Modality.Should().Be(SensorModality.Text);
        perception.Timestamp.Should().Be(timestamp);
        perception.Confidence.Should().Be(0.95);
    }
}

[Trait("Category", "Unit")]
public class VisualSensorConfigTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var config = new VisualSensorConfig();

        config.Width.Should().Be(640);
        config.Height.Should().Be(480);
        config.FrameRate.Should().Be(30);
        config.EnableObjectDetection.Should().BeTrue();
        config.EnableFaceDetection.Should().BeTrue();
        config.EnableSceneClassification.Should().BeTrue();
        config.EnableEmotionDetection.Should().BeTrue();
        config.ProcessEveryNthFrame.Should().Be(5);
        config.MaxObjectsToDetect.Should().Be(20);
    }

    [Fact]
    public void CustomValues_OverrideDefaults()
    {
        var config = new VisualSensorConfig(
            Width: 1920,
            Height: 1080,
            FrameRate: 60,
            EnableObjectDetection: false,
            EnableFaceDetection: false,
            EnableSceneClassification: false,
            EnableEmotionDetection: false,
            ProcessEveryNthFrame: 10,
            MaxObjectsToDetect: 50);

        config.Width.Should().Be(1920);
        config.Height.Should().Be(1080);
        config.FrameRate.Should().Be(60);
        config.EnableObjectDetection.Should().BeFalse();
        config.EnableFaceDetection.Should().BeFalse();
        config.EnableSceneClassification.Should().BeFalse();
        config.EnableEmotionDetection.Should().BeFalse();
        config.ProcessEveryNthFrame.Should().Be(10);
        config.MaxObjectsToDetect.Should().Be(50);
    }

    [Fact]
    public void WithExpression_CreatesNewInstance()
    {
        var original = new VisualSensorConfig();
        var modified = original with { Width = 800, Height = 600 };

        modified.Width.Should().Be(800);
        modified.Height.Should().Be(600);
        modified.FrameRate.Should().Be(30); // unchanged
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var config1 = new VisualSensorConfig(Width: 640, Height: 480);
        var config2 = new VisualSensorConfig(Width: 640, Height: 480);

        config1.Should().Be(config2);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var config1 = new VisualSensorConfig(Width: 640);
        var config2 = new VisualSensorConfig(Width: 800);

        config1.Should().NotBe(config2);
    }
}
