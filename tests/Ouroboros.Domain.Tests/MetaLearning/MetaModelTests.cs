using Ouroboros.Domain.MetaLearning;

namespace Ouroboros.Tests.MetaLearning;

[Trait("Category", "Unit")]
public class MetaModelTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var mockModel = new Mock<IModel>();
        var config = MetaLearningConfig.DefaultMAML;
        var metaParams = new Dictionary<string, object> { ["lr"] = 0.01 };

        var model = MetaModel.Create(mockModel.Object, config, metaParams);

        model.Id.Should().NotBeEmpty();
        model.InnerModel.Should().Be(mockModel.Object);
        model.Config.Should().Be(config);
        model.MetaParameters.Should().ContainKey("lr");
    }

    [Fact]
    public void GetMetaParameter_Existing_ShouldReturnValue()
    {
        var model = MetaModel.Create(new Mock<IModel>().Object, MetaLearningConfig.DefaultMAML,
            new Dictionary<string, object> { ["key"] = "value" });

        model.GetMetaParameter("key").Should().Be("value");
    }

    [Fact]
    public void GetMetaParameter_NonExistent_ShouldReturnNull()
    {
        var model = MetaModel.Create(new Mock<IModel>().Object, MetaLearningConfig.DefaultMAML,
            new Dictionary<string, object>());

        model.GetMetaParameter("missing").Should().BeNull();
    }

    [Fact]
    public void WithMetaParameter_ShouldReturnNewModelWithUpdatedParam()
    {
        var model = MetaModel.Create(new Mock<IModel>().Object, MetaLearningConfig.DefaultMAML,
            new Dictionary<string, object> { ["a"] = 1 });

        var updated = model.WithMetaParameter("b", 2);

        updated.GetMetaParameter("a").Should().Be(1);
        updated.GetMetaParameter("b").Should().Be(2);
        model.GetMetaParameter("b").Should().BeNull();
    }

    [Fact]
    public void Age_ShouldBeNonNegative()
    {
        var model = MetaModel.Create(new Mock<IModel>().Object, MetaLearningConfig.DefaultMAML,
            new Dictionary<string, object>());

        model.Age.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }
}
