using Ouroboros.Domain.Autonomous;

namespace Ouroboros.Tests.Autonomous;

[Trait("Category", "Unit")]
public class SpeechRequestTests
{
    [Fact]
    public void Constructor_SetsTextAndPersona()
    {
        var request = new SpeechRequest("Hello world", "Ouroboros");

        request.Text.Should().Be("Hello world");
        request.Persona.Should().Be("Ouroboros");
        request.Completion.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithCompletion_SetsCompletion()
    {
        var tcs = new TaskCompletionSource<bool>();
        var request = new SpeechRequest("Test", "Agent", tcs);

        request.Text.Should().Be("Test");
        request.Persona.Should().Be("Agent");
        request.Completion.Should().BeSameAs(tcs);
    }

    [Fact]
    public void Default_Completion_IsNull()
    {
        var request = new SpeechRequest("Text", "Persona");

        request.Completion.Should().BeNull();
    }

    [Fact]
    public void RecordEquality_SameValues_WithoutCompletion()
    {
        var a = new SpeechRequest("Hello", "P1");
        var b = new SpeechRequest("Hello", "P1");

        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentText_NotEqual()
    {
        var a = new SpeechRequest("Hello", "P1");
        var b = new SpeechRequest("Goodbye", "P1");

        a.Should().NotBe(b);
    }

    [Fact]
    public void RecordEquality_DifferentPersona_NotEqual()
    {
        var a = new SpeechRequest("Hello", "P1");
        var b = new SpeechRequest("Hello", "P2");

        a.Should().NotBe(b);
    }

    [Fact]
    public void WithExpression_ChangesText()
    {
        var original = new SpeechRequest("Original", "Persona");
        var modified = original with { Text = "Modified" };

        modified.Text.Should().Be("Modified");
        original.Text.Should().Be("Original");
    }

    [Fact]
    public void WithExpression_ChangesPersona()
    {
        var original = new SpeechRequest("Text", "OldPersona");
        var modified = original with { Persona = "NewPersona" };

        modified.Persona.Should().Be("NewPersona");
    }

    [Fact]
    public void ToString_ContainsTextAndPersona()
    {
        var request = new SpeechRequest("Hello world", "Agent");

        var str = request.ToString();
        str.Should().Contain("Hello world");
        str.Should().Contain("Agent");
    }
}

[Trait("Category", "Unit")]
public class VoicePriorityTests
{
    [Theory]
    [InlineData(VoicePriority.Low, 0)]
    [InlineData(VoicePriority.Normal, 1)]
    [InlineData(VoicePriority.High, 2)]
    [InlineData(VoicePriority.Critical, 3)]
    public void Values_HaveExpectedIntegerValues(VoicePriority priority, int expected)
    {
        ((int)priority).Should().Be(expected);
    }

    [Fact]
    public void HasFourValues()
    {
        Enum.GetValues<VoicePriority>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(VoicePriority.Low)]
    [InlineData(VoicePriority.Normal)]
    [InlineData(VoicePriority.High)]
    [InlineData(VoicePriority.Critical)]
    public void AllValues_AreDefined(VoicePriority priority)
    {
        Enum.IsDefined(priority).Should().BeTrue();
    }

    [Fact]
    public void Ordering_LowIsLessThanCritical()
    {
        VoicePriority.Low.Should().BeLessThan(VoicePriority.Critical);
        VoicePriority.Normal.Should().BeLessThan(VoicePriority.High);
    }
}

[Trait("Category", "Unit")]
public class NeuronTypeTests
{
    [Fact]
    public void HasExpectedCount()
    {
        Enum.GetValues<NeuronType>().Should().HaveCount(15);
    }

    [Theory]
    [InlineData(NeuronType.Processor)]
    [InlineData(NeuronType.Aggregator)]
    [InlineData(NeuronType.Observer)]
    [InlineData(NeuronType.Responder)]
    [InlineData(NeuronType.Core)]
    [InlineData(NeuronType.Memory)]
    [InlineData(NeuronType.CodeReflection)]
    [InlineData(NeuronType.Symbolic)]
    [InlineData(NeuronType.Communication)]
    [InlineData(NeuronType.Safety)]
    [InlineData(NeuronType.Affect)]
    [InlineData(NeuronType.Executive)]
    [InlineData(NeuronType.Learning)]
    [InlineData(NeuronType.Cognitive)]
    [InlineData(NeuronType.Custom)]
    public void AllValues_AreDefined(NeuronType neuronType)
    {
        Enum.IsDefined(neuronType).Should().BeTrue();
    }

    [Fact]
    public void Processor_IsFirstValue()
    {
        ((int)NeuronType.Processor).Should().Be(0);
    }

    [Fact]
    public void CanParse_FromString()
    {
        Enum.Parse<NeuronType>("Core").Should().Be(NeuronType.Core);
        Enum.Parse<NeuronType>("Safety").Should().Be(NeuronType.Safety);
    }
}

[Trait("Category", "Unit")]
public class ProactiveMessageEventArgsTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var timestamp = DateTime.UtcNow;
        var args = new ProactiveMessageEventArgs(
            "Hello user!",
            IntentionPriority.High,
            "CoreNeuron",
            timestamp);

        args.Message.Should().Be("Hello user!");
        args.Priority.Should().Be(IntentionPriority.High);
        args.Source.Should().Be("CoreNeuron");
        args.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var timestamp = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        var a = new ProactiveMessageEventArgs("Msg", IntentionPriority.Normal, "Src", timestamp);
        var b = new ProactiveMessageEventArgs("Msg", IntentionPriority.Normal, "Src", timestamp);

        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentMessage_NotEqual()
    {
        var timestamp = DateTime.UtcNow;

        var a = new ProactiveMessageEventArgs("Msg1", IntentionPriority.Normal, "Src", timestamp);
        var b = new ProactiveMessageEventArgs("Msg2", IntentionPriority.Normal, "Src", timestamp);

        a.Should().NotBe(b);
    }

    [Fact]
    public void RecordEquality_DifferentPriority_NotEqual()
    {
        var timestamp = DateTime.UtcNow;

        var a = new ProactiveMessageEventArgs("Msg", IntentionPriority.Low, "Src", timestamp);
        var b = new ProactiveMessageEventArgs("Msg", IntentionPriority.High, "Src", timestamp);

        a.Should().NotBe(b);
    }

    [Fact]
    public void WithExpression_ChangesMessage()
    {
        var args = new ProactiveMessageEventArgs("Original", IntentionPriority.Normal, "Src", DateTime.UtcNow);
        var modified = args with { Message = "Updated" };

        modified.Message.Should().Be("Updated");
        args.Message.Should().Be("Original");
    }

    [Fact]
    public void WithExpression_ChangesPriority()
    {
        var args = new ProactiveMessageEventArgs("Msg", IntentionPriority.Low, "Src", DateTime.UtcNow);
        var modified = args with { Priority = IntentionPriority.Critical };

        modified.Priority.Should().Be(IntentionPriority.Critical);
    }

    [Fact]
    public void ToString_ContainsMessage()
    {
        var args = new ProactiveMessageEventArgs("TestMessage", IntentionPriority.Normal, "Source", DateTime.UtcNow);

        args.ToString().Should().Contain("TestMessage");
    }
}

[Trait("Category", "Unit")]
public class QdrantCollectionStatsTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var stats = new QdrantCollectionStats
        {
            Name = "test_collection",
            Exists = true,
            PointCount = 1000,
            VectorSize = 384,
        };

        stats.Name.Should().Be("test_collection");
        stats.Exists.Should().BeTrue();
        stats.PointCount.Should().Be(1000);
        stats.VectorSize.Should().Be(384);
    }

    [Fact]
    public void DefaultValues_ExistsIsFalse_CountsAreZero()
    {
        var stats = new QdrantCollectionStats
        {
            Name = "empty",
        };

        stats.Exists.Should().BeFalse();
        stats.PointCount.Should().Be(0);
        stats.VectorSize.Should().Be(0);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var a = new QdrantCollectionStats { Name = "col", Exists = true, PointCount = 10, VectorSize = 128 };
        var b = new QdrantCollectionStats { Name = "col", Exists = true, PointCount = 10, VectorSize = 128 };

        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentName_NotEqual()
    {
        var a = new QdrantCollectionStats { Name = "col1" };
        var b = new QdrantCollectionStats { Name = "col2" };

        a.Should().NotBe(b);
    }

    [Fact]
    public void WithExpression_ChangesPointCount()
    {
        var stats = new QdrantCollectionStats { Name = "col", PointCount = 100 };
        var modified = stats with { PointCount = 200 };

        modified.PointCount.Should().Be(200);
        stats.PointCount.Should().Be(100);
    }

    [Fact]
    public void WithExpression_ChangesExists()
    {
        var stats = new QdrantCollectionStats { Name = "col", Exists = false };
        var modified = stats with { Exists = true };

        modified.Exists.Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class QdrantNeuralMemoryStatsTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var stats = new QdrantNeuralMemoryStats
        {
            IsConnected = true,
            NeuronMessagesCount = 500,
            IntentionsCount = 100,
            MemoriesCount = 2000,
            TotalPoints = 2600,
        };

        stats.IsConnected.Should().BeTrue();
        stats.NeuronMessagesCount.Should().Be(500);
        stats.IntentionsCount.Should().Be(100);
        stats.MemoriesCount.Should().Be(2000);
        stats.TotalPoints.Should().Be(2600);
    }

    [Fact]
    public void DefaultValues_AllZeroOrFalse()
    {
        var stats = new QdrantNeuralMemoryStats();

        stats.IsConnected.Should().BeFalse();
        stats.NeuronMessagesCount.Should().Be(0);
        stats.IntentionsCount.Should().Be(0);
        stats.MemoriesCount.Should().Be(0);
        stats.TotalPoints.Should().Be(0);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var a = new QdrantNeuralMemoryStats
        {
            IsConnected = true,
            NeuronMessagesCount = 10,
            IntentionsCount = 5,
            MemoriesCount = 20,
            TotalPoints = 35,
        };
        var b = new QdrantNeuralMemoryStats
        {
            IsConnected = true,
            NeuronMessagesCount = 10,
            IntentionsCount = 5,
            MemoriesCount = 20,
            TotalPoints = 35,
        };

        a.Should().Be(b);
    }

    [Fact]
    public void WithExpression_ChangesIsConnected()
    {
        var stats = new QdrantNeuralMemoryStats { IsConnected = false };
        var modified = stats with { IsConnected = true };

        modified.IsConnected.Should().BeTrue();
    }

    [Fact]
    public void WithExpression_ChangesMemoriesCount()
    {
        var stats = new QdrantNeuralMemoryStats { MemoriesCount = 100 };
        var modified = stats with { MemoriesCount = 500 };

        modified.MemoriesCount.Should().Be(500);
        stats.MemoriesCount.Should().Be(100);
    }
}
