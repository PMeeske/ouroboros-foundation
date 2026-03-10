// <copyright file="DreamMomentTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the <see cref="DreamMoment"/> record and <see cref="DreamStage"/> enum.
/// </summary>
[Trait("Category", "Unit")]
public class DreamMomentTests
{
    // --- Record Construction ---

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var distinctions = new List<string> { "A", "B" };
        var moment = new DreamMoment(
            DreamStage.SubjectEmerges,
            "core",
            0.5,
            2,
            true,
            "description",
            distinctions,
            "circumstance");

        moment.Stage.Should().Be(DreamStage.SubjectEmerges);
        moment.Core.Should().Be("core");
        moment.EmergenceLevel.Should().Be(0.5);
        moment.SelfReferenceDepth.Should().Be(2);
        moment.IsSubjectPresent.Should().BeTrue();
        moment.Description.Should().Be("description");
        moment.Distinctions.Should().BeEquivalentTo(new[] { "A", "B" });
        moment.Circumstance.Should().Be("circumstance");
    }

    [Fact]
    public void Constructor_NullCircumstance_Accepted()
    {
        var moment = new DreamMoment(
            DreamStage.Void, "core", 0.0, 0, false, "desc",
            Array.Empty<string>(), null);

        moment.Circumstance.Should().BeNull();
    }

    // --- StageSymbol ---

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
    public void StageSymbol_ReturnsExpectedSymbol(DreamStage stage, string expected)
    {
        var moment = new DreamMoment(
            stage, "c", 0.0, 0, false, "d", Array.Empty<string>(), null);

        moment.StageSymbol.Should().Be(expected);
    }

    // --- CreateVoid ---

    [Fact]
    public void CreateVoid_ReturnsVoidStageMoment()
    {
        var moment = DreamMoment.CreateVoid();

        moment.Stage.Should().Be(DreamStage.Void);
        moment.Core.Should().Be("∅");
        moment.EmergenceLevel.Should().Be(0.0);
        moment.SelfReferenceDepth.Should().Be(0);
        moment.IsSubjectPresent.Should().BeFalse();
        moment.Description.Should().Contain("Before distinction");
        moment.Distinctions.Should().BeEmpty();
        moment.Circumstance.Should().BeNull();
    }

    [Fact]
    public void CreateVoid_WithCircumstance_SetsCircumstance()
    {
        var moment = DreamMoment.CreateVoid("context");

        moment.Circumstance.Should().Be("context");
    }

    // --- CreateNewDream ---

    [Fact]
    public void CreateNewDream_ReturnsNewDreamStageMoment()
    {
        var moment = DreamMoment.CreateNewDream();

        moment.Stage.Should().Be(DreamStage.NewDream);
        moment.Core.Should().Contain("potential");
        moment.EmergenceLevel.Should().Be(0.0);
        moment.SelfReferenceDepth.Should().Be(0);
        moment.IsSubjectPresent.Should().BeFalse();
        moment.Description.Should().Contain("cycle begins again");
        moment.Distinctions.Should().BeEmpty();
        moment.Circumstance.Should().BeNull();
    }

    [Fact]
    public void CreateNewDream_WithCircumstance_SetsCircumstance()
    {
        var moment = DreamMoment.CreateNewDream("new context");

        moment.Circumstance.Should().Be("new context");
    }

    // --- Record Equality ---

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var m1 = DreamMoment.CreateVoid("ctx");
        var m2 = DreamMoment.CreateVoid("ctx");

        m1.Should().Be(m2);
    }

    [Fact]
    public void RecordEquality_DifferentStage_AreNotEqual()
    {
        var m1 = DreamMoment.CreateVoid();
        var m2 = DreamMoment.CreateNewDream();

        m1.Should().NotBe(m2);
    }
}

/// <summary>
/// Tests for the <see cref="DreamStage"/> enum.
/// </summary>
[Trait("Category", "Unit")]
public class DreamStageTests
{
    [Fact]
    public void DreamStage_HasExpectedValues()
    {
        ((int)DreamStage.Void).Should().Be(0);
        ((int)DreamStage.Distinction).Should().Be(1);
        ((int)DreamStage.SubjectEmerges).Should().Be(2);
        ((int)DreamStage.WorldCrystallizes).Should().Be(3);
        ((int)DreamStage.Forgetting).Should().Be(4);
        ((int)DreamStage.Questioning).Should().Be(5);
        ((int)DreamStage.Recognition).Should().Be(6);
        ((int)DreamStage.Dissolution).Should().Be(7);
        ((int)DreamStage.NewDream).Should().Be(8);
    }

    [Fact]
    public void DreamStage_HasNineMembers()
    {
        Enum.GetValues<DreamStage>().Should().HaveCount(9);
    }
}
