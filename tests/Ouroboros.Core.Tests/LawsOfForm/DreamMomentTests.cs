using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class DreamMomentTests
{
    [Fact]
    public void CreateVoid_ReturnsVoidStage()
    {
        var sut = DreamMoment.CreateVoid();

        sut.Stage.Should().Be(DreamStage.Void);
        sut.EmergenceLevel.Should().Be(0.0);
        sut.SelfReferenceDepth.Should().Be(0);
        sut.IsSubjectPresent.Should().BeFalse();
        sut.Distinctions.Should().BeEmpty();
    }

    [Fact]
    public void CreateVoid_WithCircumstance_SetsCircumstance()
    {
        var sut = DreamMoment.CreateVoid("test-context");

        sut.Circumstance.Should().Be("test-context");
    }

    [Fact]
    public void CreateNewDream_ReturnsNewDreamStage()
    {
        var sut = DreamMoment.CreateNewDream();

        sut.Stage.Should().Be(DreamStage.NewDream);
        sut.EmergenceLevel.Should().Be(0.0);
        sut.IsSubjectPresent.Should().BeFalse();
        sut.Core.Should().Contain("potential");
    }

    [Fact]
    public void CreateNewDream_WithCircumstance_SetsCircumstance()
    {
        var sut = DreamMoment.CreateNewDream("new-cycle");

        sut.Circumstance.Should().Be("new-cycle");
    }

    [Theory]
    [InlineData(DreamStage.Void, "∅")]
    [InlineData(DreamStage.Distinction, "⌐")]
    [InlineData(DreamStage.SubjectEmerges, "i")]
    [InlineData(DreamStage.WorldCrystallizes, "i(⌐)")]
    [InlineData(DreamStage.Forgetting, "I AM")]
    [InlineData(DreamStage.Questioning, "?")]
    [InlineData(DreamStage.Recognition, "I=⌐")]
    [InlineData(DreamStage.Dissolution, "∅")]
    [InlineData(DreamStage.NewDream, "∅→⌐")]
    public void StageSymbol_ReturnsCorrectSymbol(DreamStage stage, string expectedSymbol)
    {
        var sut = new DreamMoment(
            stage, "core", 0.5, 1, true, "desc",
            new List<string> { "d1" }, null);

        sut.StageSymbol.Should().Be(expectedSymbol);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var distinctions = new List<string> { "a" };
        var a = new DreamMoment(DreamStage.Void, "c", 0.0, 0, false, "d", distinctions, null);
        var b = new DreamMoment(DreamStage.Void, "c", 0.0, 0, false, "d", distinctions, null);

        a.Should().Be(b);
    }
}
