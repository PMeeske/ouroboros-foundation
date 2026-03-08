using Ouroboros.Core.Learning;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public sealed class FeedbackSignalTests
{
    [Fact]
    public void UserCorrection_SetsTypeAndScore()
    {
        var sut = FeedbackSignal.UserCorrection("corrected text");

        sut.Type.Should().Be(FeedbackType.UserCorrection);
        sut.Score.Should().Be(1.0);
        sut.Correction.Should().Be("corrected text");
    }

    [Fact]
    public void Success_DefaultScore_IsOne()
    {
        var sut = FeedbackSignal.Success();

        sut.Type.Should().Be(FeedbackType.SuccessSignal);
        sut.Score.Should().Be(1.0);
    }

    [Fact]
    public void Success_CustomScore_ClampsToRange()
    {
        var sut = FeedbackSignal.Success(1.5);

        sut.Score.Should().Be(1.0);
    }

    [Fact]
    public void Failure_DefaultScore_IsNegativeOne()
    {
        var sut = FeedbackSignal.Failure();

        sut.Type.Should().Be(FeedbackType.FailureSignal);
        sut.Score.Should().Be(-1.0);
    }

    [Fact]
    public void Failure_CustomScore_ClampsToRange()
    {
        var sut = FeedbackSignal.Failure(-2.0);

        sut.Score.Should().Be(-1.0);
    }

    [Fact]
    public void Preference_ClampsScore()
    {
        var sut = FeedbackSignal.Preference(0.75);

        sut.Type.Should().Be(FeedbackType.PreferenceRanking);
        sut.Score.Should().Be(0.75);
    }

    [Fact]
    public void Preference_AboveMax_ClampsToOne()
    {
        var sut = FeedbackSignal.Preference(5.0);

        sut.Score.Should().Be(1.0);
    }

    [Fact]
    public void Validate_ValidSignal_ReturnsSuccess()
    {
        var sut = FeedbackSignal.Success(0.8);

        var result = sut.Validate();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_UserCorrectionWithoutText_ReturnsFailure()
    {
        var sut = new FeedbackSignal(FeedbackType.UserCorrection, 1.0, null);

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("correction text");
    }

    [Fact]
    public void Validate_UserCorrectionWithEmptyText_ReturnsFailure()
    {
        var sut = new FeedbackSignal(FeedbackType.UserCorrection, 1.0, "   ");

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Validate_ScoreOutOfRange_ReturnsFailure()
    {
        var sut = new FeedbackSignal(FeedbackType.SuccessSignal, 2.0);

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Score");
    }

    [Fact]
    public void FeedbackType_HasAllExpectedValues()
    {
        Enum.GetValues<FeedbackType>().Should().HaveCount(4);
    }
}
