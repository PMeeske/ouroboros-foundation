namespace Ouroboros.Tests.MetaLearning;

using Ouroboros.Domain.MetaLearning;
using Ouroboros.Providers.Random;

[Trait("Category", "Unit")]
public sealed class MetaLearningModelTests
{
    #region AdaptedModel Tests

    [Fact]
    public void AdaptedModel_Constructor_SetsAllProperties()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var time = TimeSpan.FromSeconds(5);

        // Act
        var adapted = new AdaptedModel(mockModel.Object, 50, 0.92, time);

        // Assert
        adapted.Model.Should().Be(mockModel.Object);
        adapted.AdaptationSteps.Should().Be(50);
        adapted.ValidationPerformance.Should().Be(0.92);
        adapted.AdaptationTime.Should().Be(time);
    }

    [Fact]
    public void AdaptedModel_Create_ReturnsCorrectInstance()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var time = TimeSpan.FromMinutes(2);

        // Act
        var adapted = AdaptedModel.Create(mockModel.Object, 200, 0.78, time);

        // Assert
        adapted.Model.Should().Be(mockModel.Object);
        adapted.AdaptationSteps.Should().Be(200);
        adapted.ValidationPerformance.Should().Be(0.78);
        adapted.AdaptationTime.Should().Be(time);
    }

    [Fact]
    public void AdaptedModel_StepsPerSecond_CalculatesCorrectly()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var adapted = new AdaptedModel(mockModel.Object, 500, 0.9, TimeSpan.FromSeconds(25));

        // Act & Assert
        adapted.StepsPerSecond.Should().BeApproximately(20.0, 0.001);
    }

    [Fact]
    public void AdaptedModel_StepsPerSecond_ZeroDuration_ReturnsZero()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var adapted = new AdaptedModel(mockModel.Object, 100, 0.85, TimeSpan.Zero);

        // Act & Assert
        adapted.StepsPerSecond.Should().Be(0.0);
    }

    [Fact]
    public void AdaptedModel_StepsPerSecond_SmallDuration_ReturnsHighRate()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var adapted = new AdaptedModel(mockModel.Object, 1000, 0.9, TimeSpan.FromMilliseconds(100));

        // Act & Assert
        adapted.StepsPerSecond.Should().BeApproximately(10000.0, 1.0);
    }

    [Fact]
    public void AdaptedModel_IsSuccessful_AboveThreshold_ReturnsTrue()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var adapted = new AdaptedModel(mockModel.Object, 50, 0.95, TimeSpan.FromSeconds(1));

        // Act & Assert
        adapted.IsSuccessful(0.9).Should().BeTrue();
    }

    [Fact]
    public void AdaptedModel_IsSuccessful_BelowThreshold_ReturnsFalse()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var adapted = new AdaptedModel(mockModel.Object, 50, 0.5, TimeSpan.FromSeconds(1));

        // Act & Assert
        adapted.IsSuccessful(0.8).Should().BeFalse();
    }

    [Fact]
    public void AdaptedModel_IsSuccessful_ExactlyAtThreshold_ReturnsTrue()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var adapted = new AdaptedModel(mockModel.Object, 50, 0.8, TimeSpan.FromSeconds(1));

        // Act & Assert
        adapted.IsSuccessful(0.8).Should().BeTrue();
    }

    [Fact]
    public void AdaptedModel_IsSuccessful_ZeroThreshold_AlwaysTrue()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var adapted = new AdaptedModel(mockModel.Object, 10, 0.0, TimeSpan.FromSeconds(1));

        // Act & Assert
        adapted.IsSuccessful(0.0).Should().BeTrue();
    }

    #endregion

    #region Example Tests

    [Fact]
    public void Example_Constructor_SetsAllProperties()
    {
        // Arrange
        var metadata = new Dictionary<string, object> { ["difficulty"] = "hard" };

        // Act
        var example = new Example("What is 2+2?", "4", metadata);

        // Assert
        example.Input.Should().Be("What is 2+2?");
        example.Output.Should().Be("4");
        example.Metadata.Should().ContainKey("difficulty");
    }

    [Fact]
    public void Example_Constructor_DefaultMetadataIsNull()
    {
        // Act
        var example = new Example("input", "output");

        // Assert
        example.Metadata.Should().BeNull();
    }

    [Fact]
    public void Example_Create_ReturnsExampleWithNullMetadata()
    {
        // Act
        var example = Example.Create("hello", "world");

        // Assert
        example.Input.Should().Be("hello");
        example.Output.Should().Be("world");
        example.Metadata.Should().BeNull();
    }

    [Fact]
    public void Example_WithMetadata_AddsToNullMetadata()
    {
        // Arrange
        var example = Example.Create("input", "output");

        // Act
        var withMeta = example.WithMetadata("domain", "math");

        // Assert
        withMeta.Metadata.Should().ContainKey("domain");
        withMeta.Metadata!["domain"].Should().Be("math");
        example.Metadata.Should().BeNull(); // original unchanged
    }

    [Fact]
    public void Example_WithMetadata_AddsToExistingMetadata()
    {
        // Arrange
        var example = new Example("in", "out", new Dictionary<string, object> { ["key1"] = "val1" });

        // Act
        var withMeta = example.WithMetadata("key2", "val2");

        // Assert
        withMeta.Metadata.Should().HaveCount(2);
        withMeta.Metadata!["key1"].Should().Be("val1");
        withMeta.Metadata["key2"].Should().Be("val2");
    }

    [Fact]
    public void Example_WithMetadata_OverwritesExistingKey()
    {
        // Arrange
        var example = new Example("in", "out", new Dictionary<string, object> { ["key"] = "old" });

        // Act
        var withMeta = example.WithMetadata("key", "new");

        // Assert
        withMeta.Metadata!["key"].Should().Be("new");
    }

    [Fact]
    public void Example_WithMetadata_DoesNotMutateOriginal()
    {
        // Arrange
        var originalMeta = new Dictionary<string, object> { ["a"] = 1 };
        var example = new Example("in", "out", originalMeta);

        // Act
        example.WithMetadata("b", 2);

        // Assert
        example.Metadata.Should().HaveCount(1);
        example.Metadata.Should().NotContainKey("b");
    }

    [Fact]
    public void Example_RecordEquality_WorksWithSameValues()
    {
        // Act
        var ex1 = Example.Create("a", "b");
        var ex2 = Example.Create("a", "b");

        // Assert
        ex1.Should().Be(ex2);
    }

    #endregion

    #region MetaAlgorithm Enum Tests

    [Theory]
    [InlineData(MetaAlgorithm.MAML)]
    [InlineData(MetaAlgorithm.Reptile)]
    [InlineData(MetaAlgorithm.ProtoNet)]
    [InlineData(MetaAlgorithm.MetaSGD)]
    [InlineData(MetaAlgorithm.LEO)]
    public void MetaAlgorithm_AllValues_AreDefined(MetaAlgorithm algorithm)
    {
        Enum.IsDefined(algorithm).Should().BeTrue();
    }

    [Fact]
    public void MetaAlgorithm_HasFiveValues()
    {
        Enum.GetValues<MetaAlgorithm>().Should().HaveCount(5);
    }

    [Fact]
    public void MetaAlgorithm_UndefinedValue_IsNotDefined()
    {
        Enum.IsDefined((MetaAlgorithm)999).Should().BeFalse();
    }

    #endregion

    #region MetaLearningConfig Tests

    [Fact]
    public void MetaLearningConfig_Constructor_SetsAllProperties()
    {
        // Act
        var config = new MetaLearningConfig(
            MetaAlgorithm.ProtoNet, 0.005, 0.0005, 3, 8, 500);

        // Assert
        config.Algorithm.Should().Be(MetaAlgorithm.ProtoNet);
        config.InnerLearningRate.Should().Be(0.005);
        config.OuterLearningRate.Should().Be(0.0005);
        config.InnerSteps.Should().Be(3);
        config.TaskBatchSize.Should().Be(8);
        config.MetaIterations.Should().Be(500);
    }

    [Fact]
    public void MetaLearningConfig_DefaultMAML_HasCorrectValues()
    {
        // Act
        var config = MetaLearningConfig.DefaultMAML;

        // Assert
        config.Algorithm.Should().Be(MetaAlgorithm.MAML);
        config.InnerLearningRate.Should().Be(0.01);
        config.OuterLearningRate.Should().Be(0.001);
        config.InnerSteps.Should().Be(5);
        config.TaskBatchSize.Should().Be(4);
        config.MetaIterations.Should().Be(1000);
    }

    [Fact]
    public void MetaLearningConfig_DefaultReptile_HasCorrectValues()
    {
        // Act
        var config = MetaLearningConfig.DefaultReptile;

        // Assert
        config.Algorithm.Should().Be(MetaAlgorithm.Reptile);
        config.InnerLearningRate.Should().Be(0.01);
        config.OuterLearningRate.Should().Be(0.001);
        config.InnerSteps.Should().Be(10);
        config.TaskBatchSize.Should().Be(1);
        config.MetaIterations.Should().Be(2000);
    }

    [Fact]
    public void MetaLearningConfig_DefaultMAML_And_DefaultReptile_AreDifferent()
    {
        // Act
        var maml = MetaLearningConfig.DefaultMAML;
        var reptile = MetaLearningConfig.DefaultReptile;

        // Assert
        maml.Should().NotBe(reptile);
        maml.Algorithm.Should().NotBe(reptile.Algorithm);
    }

    [Fact]
    public void MetaLearningConfig_RecordEquality_WorksCorrectly()
    {
        // Act
        var c1 = new MetaLearningConfig(MetaAlgorithm.LEO, 0.01, 0.001, 5, 4, 1000);
        var c2 = new MetaLearningConfig(MetaAlgorithm.LEO, 0.01, 0.001, 5, 4, 1000);

        // Assert
        c1.Should().Be(c2);
    }

    #endregion

    #region MetaModel Tests

    [Fact]
    public void MetaModel_Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var mockModel = new Mock<IModel>();
        var config = MetaLearningConfig.DefaultMAML;
        var metaParams = new Dictionary<string, object> { ["lr"] = 0.01 };
        var trainedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var meta = new MetaModel(id, mockModel.Object, config, metaParams, trainedAt);

        // Assert
        meta.Id.Should().Be(id);
        meta.InnerModel.Should().Be(mockModel.Object);
        meta.Config.Should().Be(config);
        meta.MetaParameters.Should().ContainKey("lr");
        meta.TrainedAt.Should().Be(trainedAt);
    }

    [Fact]
    public void MetaModel_Create_GeneratesNewIdAndSetsTimestamp()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var config = MetaLearningConfig.DefaultMAML;
        var metaParams = new Dictionary<string, object>();
        var before = DateTime.UtcNow;

        // Act
        var meta = MetaModel.Create(mockModel.Object, config, metaParams);

        // Assert
        meta.Id.Should().NotBe(Guid.Empty);
        meta.InnerModel.Should().Be(mockModel.Object);
        meta.Config.Should().Be(config);
        meta.TrainedAt.Should().BeOnOrAfter(before);
        meta.TrainedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void MetaModel_Create_GeneratesUniqueIds()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var config = MetaLearningConfig.DefaultMAML;

        // Act
        var m1 = MetaModel.Create(mockModel.Object, config, new Dictionary<string, object>());
        var m2 = MetaModel.Create(mockModel.Object, config, new Dictionary<string, object>());

        // Assert
        m1.Id.Should().NotBe(m2.Id);
    }

    [Fact]
    public void MetaModel_GetMetaParameter_ExistingKey_ReturnsValue()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var meta = new MetaModel(
            Guid.NewGuid(),
            mockModel.Object,
            MetaLearningConfig.DefaultMAML,
            new Dictionary<string, object> { ["alpha"] = 0.5 },
            DateTime.UtcNow);

        // Act
        var result = meta.GetMetaParameter("alpha");

        // Assert
        result.Should().Be(0.5);
    }

    [Fact]
    public void MetaModel_GetMetaParameter_MissingKey_ReturnsNull()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var meta = new MetaModel(
            Guid.NewGuid(),
            mockModel.Object,
            MetaLearningConfig.DefaultMAML,
            new Dictionary<string, object>(),
            DateTime.UtcNow);

        // Act
        var result = meta.GetMetaParameter("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void MetaModel_WithMetaParameter_AddsNewParameter()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var meta = new MetaModel(
            Guid.NewGuid(),
            mockModel.Object,
            MetaLearningConfig.DefaultMAML,
            new Dictionary<string, object> { ["a"] = 1 },
            DateTime.UtcNow);

        // Act
        var updated = meta.WithMetaParameter("b", 2);

        // Assert
        updated.MetaParameters.Should().HaveCount(2);
        updated.GetMetaParameter("a").Should().Be(1);
        updated.GetMetaParameter("b").Should().Be(2);
    }

    [Fact]
    public void MetaModel_WithMetaParameter_OverwritesExistingParameter()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var meta = new MetaModel(
            Guid.NewGuid(),
            mockModel.Object,
            MetaLearningConfig.DefaultMAML,
            new Dictionary<string, object> { ["lr"] = 0.01 },
            DateTime.UtcNow);

        // Act
        var updated = meta.WithMetaParameter("lr", 0.001);

        // Assert
        updated.GetMetaParameter("lr").Should().Be(0.001);
    }

    [Fact]
    public void MetaModel_WithMetaParameter_DoesNotMutateOriginal()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var meta = new MetaModel(
            Guid.NewGuid(),
            mockModel.Object,
            MetaLearningConfig.DefaultMAML,
            new Dictionary<string, object> { ["x"] = 1 },
            DateTime.UtcNow);

        // Act
        meta.WithMetaParameter("y", 2);

        // Assert
        meta.MetaParameters.Should().HaveCount(1);
        meta.GetMetaParameter("y").Should().BeNull();
    }

    [Fact]
    public void MetaModel_Age_ReturnsPositiveTimeSpan()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var trainedAt = DateTime.UtcNow.AddHours(-2);
        var meta = new MetaModel(
            Guid.NewGuid(),
            mockModel.Object,
            MetaLearningConfig.DefaultMAML,
            new Dictionary<string, object>(),
            trainedAt);

        // Act & Assert
        meta.Age.TotalHours.Should().BeGreaterOrEqualTo(1.9);
    }

    #endregion

    #region SynthesisTask Tests

    [Fact]
    public void SynthesisTask_Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var training = new List<Example> { Example.Create("a", "b") };
        var validation = new List<Example> { Example.Create("c", "d") };

        // Act
        var task = new SynthesisTask(id, "Translation", "nlp", training, validation, "Translate EN to FR");

        // Assert
        task.Id.Should().Be(id);
        task.Name.Should().Be("Translation");
        task.Domain.Should().Be("nlp");
        task.TrainingExamples.Should().HaveCount(1);
        task.ValidationExamples.Should().HaveCount(1);
        task.Description.Should().Be("Translate EN to FR");
    }

    [Fact]
    public void SynthesisTask_Constructor_DefaultDescriptionIsNull()
    {
        // Act
        var task = new SynthesisTask(
            Guid.NewGuid(), "Task", "domain",
            new List<Example>(), new List<Example>());

        // Assert
        task.Description.Should().BeNull();
    }

    [Fact]
    public void SynthesisTask_Create_GeneratesNewId()
    {
        // Act
        var task = SynthesisTask.Create(
            "MyTask", "code",
            new List<Example>(), new List<Example>());

        // Assert
        task.Id.Should().NotBe(Guid.Empty);
        task.Name.Should().Be("MyTask");
        task.Domain.Should().Be("code");
    }

    [Fact]
    public void SynthesisTask_Create_WithDescription_SetsDescription()
    {
        // Act
        var task = SynthesisTask.Create(
            "MyTask", "code",
            new List<Example>(), new List<Example>(),
            "A code task");

        // Assert
        task.Description.Should().Be("A code task");
    }

    [Fact]
    public void SynthesisTask_Create_GeneratesUniqueIds()
    {
        // Act
        var t1 = SynthesisTask.Create("T1", "d", new List<Example>(), new List<Example>());
        var t2 = SynthesisTask.Create("T2", "d", new List<Example>(), new List<Example>());

        // Assert
        t1.Id.Should().NotBe(t2.Id);
    }

    [Fact]
    public void SynthesisTask_TotalExamples_SumsBothSets()
    {
        // Arrange
        var training = new List<Example>
        {
            Example.Create("a", "1"),
            Example.Create("b", "2"),
            Example.Create("c", "3"),
        };
        var validation = new List<Example>
        {
            Example.Create("d", "4"),
            Example.Create("e", "5"),
        };

        // Act
        var task = SynthesisTask.Create("T", "d", training, validation);

        // Assert
        task.TotalExamples.Should().Be(5);
    }

    [Fact]
    public void SynthesisTask_TotalExamples_EmptyLists_ReturnsZero()
    {
        // Act
        var task = SynthesisTask.Create("T", "d", new List<Example>(), new List<Example>());

        // Assert
        task.TotalExamples.Should().Be(0);
    }

    [Fact]
    public void SynthesisTask_SplitExamples_DefaultSplit_80_20()
    {
        // Arrange
        var examples = Enumerable.Range(0, 10)
            .Select(i => Example.Create($"in{i}", $"out{i}"))
            .ToList();

        // Act
        var (training, validation) = SynthesisTask.SplitExamples(examples);

        // Assert
        training.Should().HaveCount(8);
        validation.Should().HaveCount(2);
    }

    [Fact]
    public void SynthesisTask_SplitExamples_CustomSplit()
    {
        // Arrange
        var examples = Enumerable.Range(0, 10)
            .Select(i => Example.Create($"in{i}", $"out{i}"))
            .ToList();

        // Act
        var (training, validation) = SynthesisTask.SplitExamples(examples, 0.5);

        // Assert
        training.Should().HaveCount(5);
        validation.Should().HaveCount(5);
    }

    [Fact]
    public void SynthesisTask_SplitExamples_AllTraining()
    {
        // Arrange
        var examples = new List<Example> { Example.Create("a", "b") };

        // Act
        var (training, validation) = SynthesisTask.SplitExamples(examples, 1.0);

        // Assert
        training.Should().HaveCount(1);
        validation.Should().BeEmpty();
    }

    [Fact]
    public void SynthesisTask_SplitExamples_AllValidation()
    {
        // Arrange
        var examples = new List<Example> { Example.Create("a", "b") };

        // Act
        var (training, validation) = SynthesisTask.SplitExamples(examples, 0.0);

        // Assert
        training.Should().BeEmpty();
        validation.Should().HaveCount(1);
    }

    [Fact]
    public void SynthesisTask_SplitExamples_InvalidSplit_ThrowsArgumentException()
    {
        // Arrange
        var examples = new List<Example> { Example.Create("a", "b") };

        // Act
        var actNeg = () => SynthesisTask.SplitExamples(examples, -0.1);
        var actOver = () => SynthesisTask.SplitExamples(examples, 1.1);

        // Assert
        actNeg.Should().Throw<ArgumentException>();
        actOver.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SynthesisTask_SplitExamples_EmptyList_ReturnsBothEmpty()
    {
        // Act
        var (training, validation) = SynthesisTask.SplitExamples(new List<Example>());

        // Assert
        training.Should().BeEmpty();
        validation.Should().BeEmpty();
    }

    #endregion

    #region TaskEmbedding Tests

    [Fact]
    public void TaskEmbedding_Constructor_SetsAllProperties()
    {
        // Arrange
        var vector = new float[] { 0.1f, 0.2f, 0.3f };
        var chars = new Dictionary<string, double> { ["complexity"] = 0.7 };

        // Act
        var embedding = new TaskEmbedding(vector, chars, "Test task");

        // Assert
        embedding.Vector.Should().HaveCount(3);
        embedding.Characteristics.Should().ContainKey("complexity");
        embedding.TaskDescription.Should().Be("Test task");
    }

    [Fact]
    public void TaskEmbedding_Dimension_ReturnsVectorLength()
    {
        // Arrange
        var embedding = new TaskEmbedding(
            new float[] { 1f, 2f, 3f, 4f, 5f },
            new Dictionary<string, double>(),
            "desc");

        // Assert
        embedding.Dimension.Should().Be(5);
    }

    [Fact]
    public void TaskEmbedding_CosineSimilarity_IdenticalVectors_ReturnsOne()
    {
        // Arrange
        var vec = new float[] { 1f, 2f, 3f };
        var e1 = new TaskEmbedding(vec, new Dictionary<string, double>(), "a");
        var e2 = new TaskEmbedding(new float[] { 1f, 2f, 3f }, new Dictionary<string, double>(), "b");

        // Act
        var similarity = e1.CosineSimilarity(e2);

        // Assert
        similarity.Should().BeApproximately(1.0, 0.0001);
    }

    [Fact]
    public void TaskEmbedding_CosineSimilarity_OrthogonalVectors_ReturnsZero()
    {
        // Arrange
        var e1 = new TaskEmbedding(new float[] { 1f, 0f }, new Dictionary<string, double>(), "a");
        var e2 = new TaskEmbedding(new float[] { 0f, 1f }, new Dictionary<string, double>(), "b");

        // Act
        var similarity = e1.CosineSimilarity(e2);

        // Assert
        similarity.Should().BeApproximately(0.0, 0.0001);
    }

    [Fact]
    public void TaskEmbedding_CosineSimilarity_ZeroVector_ReturnsZero()
    {
        // Arrange
        var e1 = new TaskEmbedding(new float[] { 0f, 0f }, new Dictionary<string, double>(), "a");
        var e2 = new TaskEmbedding(new float[] { 1f, 1f }, new Dictionary<string, double>(), "b");

        // Act
        var similarity = e1.CosineSimilarity(e2);

        // Assert
        similarity.Should().Be(0.0);
    }

    [Fact]
    public void TaskEmbedding_CosineSimilarity_DifferentDimensions_ThrowsArgumentException()
    {
        // Arrange
        var e1 = new TaskEmbedding(new float[] { 1f, 2f }, new Dictionary<string, double>(), "a");
        var e2 = new TaskEmbedding(new float[] { 1f, 2f, 3f }, new Dictionary<string, double>(), "b");

        // Act
        var act = () => e1.CosineSimilarity(e2);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TaskEmbedding_CosineSimilarity_NullOther_ThrowsArgumentNullException()
    {
        // Arrange
        var e1 = new TaskEmbedding(new float[] { 1f }, new Dictionary<string, double>(), "a");

        // Act
        var act = () => e1.CosineSimilarity(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TaskEmbedding_EuclideanDistance_IdenticalVectors_ReturnsZero()
    {
        // Arrange
        var e1 = new TaskEmbedding(new float[] { 1f, 2f, 3f }, new Dictionary<string, double>(), "a");
        var e2 = new TaskEmbedding(new float[] { 1f, 2f, 3f }, new Dictionary<string, double>(), "b");

        // Act
        var distance = e1.EuclideanDistance(e2);

        // Assert
        distance.Should().BeApproximately(0.0, 0.0001);
    }

    [Fact]
    public void TaskEmbedding_EuclideanDistance_KnownValues()
    {
        // Arrange - distance between (0,0) and (3,4) should be 5
        var e1 = new TaskEmbedding(new float[] { 0f, 0f }, new Dictionary<string, double>(), "a");
        var e2 = new TaskEmbedding(new float[] { 3f, 4f }, new Dictionary<string, double>(), "b");

        // Act
        var distance = e1.EuclideanDistance(e2);

        // Assert
        distance.Should().BeApproximately(5.0, 0.0001);
    }

    [Fact]
    public void TaskEmbedding_EuclideanDistance_DifferentDimensions_ThrowsArgumentException()
    {
        // Arrange
        var e1 = new TaskEmbedding(new float[] { 1f, 2f }, new Dictionary<string, double>(), "a");
        var e2 = new TaskEmbedding(new float[] { 1f }, new Dictionary<string, double>(), "b");

        // Act
        var act = () => e1.EuclideanDistance(e2);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TaskEmbedding_EuclideanDistance_NullOther_ThrowsArgumentNullException()
    {
        // Arrange
        var e1 = new TaskEmbedding(new float[] { 1f }, new Dictionary<string, double>(), "a");

        // Act
        var act = () => e1.EuclideanDistance(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TaskEmbedding_FromCharacteristics_CreatesEmbeddingFromValues()
    {
        // Arrange
        var chars = new Dictionary<string, double>
        {
            ["complexity"] = 0.8,
            ["length"] = 0.5,
        };

        // Act
        var embedding = TaskEmbedding.FromCharacteristics(chars, "complex task");

        // Assert
        embedding.Dimension.Should().Be(2);
        embedding.TaskDescription.Should().Be("complex task");
        embedding.Characteristics.Should().HaveCount(2);
        embedding.Vector.Should().HaveCount(2);
    }

    [Fact]
    public void TaskEmbedding_FromCharacteristics_VectorContainsCharacteristicValues()
    {
        // Arrange
        var chars = new Dictionary<string, double>
        {
            ["a"] = 0.1,
            ["b"] = 0.9,
        };

        // Act
        var embedding = TaskEmbedding.FromCharacteristics(chars, "test");

        // Assert
        embedding.Vector.Should().Contain(0.1f);
        embedding.Vector.Should().Contain(0.9f);
    }

    #endregion

    #region TaskDistribution Tests

    [Fact]
    public void TaskDistribution_Constructor_SetsAllProperties()
    {
        // Arrange
        var sampler = new Func<IRandomProvider, SynthesisTask>(
            _ => SynthesisTask.Create("T", "d", new List<Example>(), new List<Example>()));

        // Act
        var dist = new TaskDistribution(
            "TestDist",
            new Dictionary<string, object> { ["key"] = "val" },
            sampler);

        // Assert
        dist.Name.Should().Be("TestDist");
        dist.Parameters.Should().ContainKey("key");
        dist.Sampler.Should().NotBeNull();
    }

    [Fact]
    public void TaskDistribution_Uniform_SetsNameAndTaskCount()
    {
        // Arrange
        var tasks = new List<SynthesisTask>
        {
            SynthesisTask.Create("T1", "d", new List<Example>(), new List<Example>()),
            SynthesisTask.Create("T2", "d", new List<Example>(), new List<Example>()),
        };

        // Act
        var dist = TaskDistribution.Uniform(tasks);

        // Assert
        dist.Name.Should().Be("Uniform");
        dist.Parameters["TaskCount"].Should().Be(2);
    }

    [Fact]
    public void TaskDistribution_Uniform_EmptyList_ThrowsArgumentException()
    {
        // Act
        var act = () => TaskDistribution.Uniform(new List<SynthesisTask>());

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TaskDistribution_Uniform_NullList_ThrowsArgumentNullException()
    {
        // Act
        var act = () => TaskDistribution.Uniform(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TaskDistribution_Uniform_Sample_ReturnsTaskFromList()
    {
        // Arrange
        var t1 = SynthesisTask.Create("T1", "d", new List<Example>(), new List<Example>());
        var t2 = SynthesisTask.Create("T2", "d", new List<Example>(), new List<Example>());
        var tasks = new List<SynthesisTask> { t1, t2 };
        var dist = TaskDistribution.Uniform(tasks);
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.Next(2)).Returns(0);

        // Act
        var sampled = dist.Sample(mockRandom.Object);

        // Assert
        sampled.Should().Be(t1);
    }

    [Fact]
    public void TaskDistribution_Weighted_SetsNameAndParameters()
    {
        // Arrange
        var task = SynthesisTask.Create("T", "d", new List<Example>(), new List<Example>());
        var weighted = new Dictionary<SynthesisTask, double> { [task] = 1.0 };

        // Act
        var dist = TaskDistribution.Weighted(weighted);

        // Assert
        dist.Name.Should().Be("Weighted");
        dist.Parameters["TaskCount"].Should().Be(1);
        dist.Parameters["TotalWeight"].Should().Be(1.0);
    }

    [Fact]
    public void TaskDistribution_Weighted_EmptyDict_ThrowsArgumentException()
    {
        // Act
        var act = () => TaskDistribution.Weighted(new Dictionary<SynthesisTask, double>());

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TaskDistribution_Weighted_NullDict_ThrowsArgumentNullException()
    {
        // Act
        var act = () => TaskDistribution.Weighted(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TaskDistribution_Weighted_Sample_ReturnsWeightedTask()
    {
        // Arrange
        var t1 = SynthesisTask.Create("T1", "d", new List<Example>(), new List<Example>());
        var t2 = SynthesisTask.Create("T2", "d", new List<Example>(), new List<Example>());
        var weighted = new Dictionary<SynthesisTask, double>
        {
            [t1] = 0.1,
            [t2] = 0.9,
        };
        var dist = TaskDistribution.Weighted(weighted);

        // Use a mock that returns a high value, should select t2
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.5);

        // Act
        var sampled = dist.Sample(mockRandom.Object);

        // Assert
        sampled.Should().Be(t2);
    }

    [Fact]
    public void TaskDistribution_SampleBatch_ReturnsCorrectCount()
    {
        // Arrange
        var task = SynthesisTask.Create("T", "d", new List<Example>(), new List<Example>());
        var dist = TaskDistribution.Uniform(new List<SynthesisTask> { task });
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.Next(1)).Returns(0);

        // Act
        var batch = dist.SampleBatch(5, mockRandom.Object);

        // Assert
        batch.Should().HaveCount(5);
        batch.Should().AllSatisfy(t => t.Should().Be(task));
    }

    [Fact]
    public void TaskDistribution_SampleBatch_ZeroCount_ReturnsEmpty()
    {
        // Arrange
        var task = SynthesisTask.Create("T", "d", new List<Example>(), new List<Example>());
        var dist = TaskDistribution.Uniform(new List<SynthesisTask> { task });

        // Act
        var batch = dist.SampleBatch(0);

        // Assert
        batch.Should().BeEmpty();
    }

    #endregion

    #region TaskFamily Tests

    [Fact]
    public void TaskFamily_Constructor_SetsAllProperties()
    {
        // Arrange
        var training = new List<SynthesisTask>
        {
            SynthesisTask.Create("T1", "nlp", new List<Example>(), new List<Example>()),
        };
        var validation = new List<SynthesisTask>
        {
            SynthesisTask.Create("V1", "nlp", new List<Example>(), new List<Example>()),
        };
        var dist = TaskDistribution.Uniform(training);

        // Act
        var family = new TaskFamily("nlp", training, validation, dist);

        // Assert
        family.Domain.Should().Be("nlp");
        family.TrainingTasks.Should().HaveCount(1);
        family.ValidationTasks.Should().HaveCount(1);
        family.Distribution.Should().Be(dist);
    }

    [Fact]
    public void TaskFamily_TotalTasks_SumsBothSets()
    {
        // Arrange
        var training = new List<SynthesisTask>
        {
            SynthesisTask.Create("T1", "d", new List<Example>(), new List<Example>()),
            SynthesisTask.Create("T2", "d", new List<Example>(), new List<Example>()),
            SynthesisTask.Create("T3", "d", new List<Example>(), new List<Example>()),
        };
        var validation = new List<SynthesisTask>
        {
            SynthesisTask.Create("V1", "d", new List<Example>(), new List<Example>()),
        };
        var dist = TaskDistribution.Uniform(training);
        var family = new TaskFamily("d", training, validation, dist);

        // Act & Assert
        family.TotalTasks.Should().Be(4);
    }

    [Fact]
    public void TaskFamily_Create_SplitsTasksCorrectly()
    {
        // Arrange
        var tasks = Enumerable.Range(0, 10)
            .Select(i => SynthesisTask.Create($"T{i}", "d", new List<Example>(), new List<Example>()))
            .ToList();

        // Act - default 0.2 validation split
        var family = TaskFamily.Create("domain", tasks);

        // Assert
        family.TrainingTasks.Should().HaveCount(8);
        family.ValidationTasks.Should().HaveCount(2);
        family.Domain.Should().Be("domain");
    }

    [Fact]
    public void TaskFamily_Create_CustomValidationSplit()
    {
        // Arrange
        var tasks = Enumerable.Range(0, 10)
            .Select(i => SynthesisTask.Create($"T{i}", "d", new List<Example>(), new List<Example>()))
            .ToList();

        // Act
        var family = TaskFamily.Create("d", tasks, 0.5);

        // Assert
        family.TrainingTasks.Should().HaveCount(5);
        family.ValidationTasks.Should().HaveCount(5);
    }

    [Fact]
    public void TaskFamily_Create_InvalidSplit_ThrowsArgumentException()
    {
        // Arrange
        var tasks = new List<SynthesisTask>
        {
            SynthesisTask.Create("T", "d", new List<Example>(), new List<Example>()),
        };

        // Act
        var actNeg = () => TaskFamily.Create("d", tasks, -0.1);
        var actOver = () => TaskFamily.Create("d", tasks, 1.1);

        // Assert
        actNeg.Should().Throw<ArgumentException>();
        actOver.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TaskFamily_Create_NullTasks_ThrowsArgumentNullException()
    {
        // Act
        var act = () => TaskFamily.Create("d", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TaskFamily_GetAllTrainingExamples_FlattensExamples()
    {
        // Arrange
        var t1 = SynthesisTask.Create("T1", "d",
            new List<Example> { Example.Create("a", "1"), Example.Create("b", "2") },
            new List<Example>());
        var t2 = SynthesisTask.Create("T2", "d",
            new List<Example> { Example.Create("c", "3") },
            new List<Example>());
        var dist = TaskDistribution.Uniform(new List<SynthesisTask> { t1, t2 });
        var family = new TaskFamily("d", new List<SynthesisTask> { t1, t2 }, new List<SynthesisTask>(), dist);

        // Act
        var examples = family.GetAllTrainingExamples();

        // Assert
        examples.Should().HaveCount(3);
    }

    [Fact]
    public void TaskFamily_GetAllValidationExamples_FlattensExamples()
    {
        // Arrange
        var v1 = SynthesisTask.Create("V1", "d",
            new List<Example>(),
            new List<Example> { Example.Create("x", "1") });
        var v2 = SynthesisTask.Create("V2", "d",
            new List<Example>(),
            new List<Example> { Example.Create("y", "2"), Example.Create("z", "3") });
        var t1 = SynthesisTask.Create("T1", "d", new List<Example>(), new List<Example>());
        var dist = TaskDistribution.Uniform(new List<SynthesisTask> { t1 });
        var family = new TaskFamily("d", new List<SynthesisTask> { t1 }, new List<SynthesisTask> { v1, v2 }, dist);

        // Act
        var examples = family.GetAllValidationExamples();

        // Assert
        examples.Should().HaveCount(3);
    }

    [Fact]
    public void TaskFamily_SampleTrainingBatch_ReturnsRequestedCount()
    {
        // Arrange
        var task = SynthesisTask.Create("T", "d", new List<Example>(), new List<Example>());
        var dist = TaskDistribution.Uniform(new List<SynthesisTask> { task });
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.Next(1)).Returns(0);
        var family = new TaskFamily("d", new List<SynthesisTask> { task }, new List<SynthesisTask>(), dist);

        // Act
        var batch = family.SampleTrainingBatch(3, mockRandom.Object);

        // Assert
        batch.Should().HaveCount(3);
    }

    #endregion
}
