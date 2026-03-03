using Ouroboros.Core.CognitivePhysics;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.CognitivePhysics;

[Trait("Category", "Unit")]
[Trait("Category", "CognitivePhysics")]
public class EthicsGateResultTests
{
    [Fact]
    public void Allow_CreatesAllowedResult()
    {
        var result = EthicsGateResult.Allow("Safe transition");

        result.IsAllowed.Should().BeTrue();
        result.IsDenied.Should().BeFalse();
        result.IsUncertain.Should().BeFalse();
        result.Reason.Should().Be("Safe transition");
    }

    [Fact]
    public void Allow_WithEmptyReason_DefaultsToEmpty()
    {
        var result = EthicsGateResult.Allow();

        result.IsAllowed.Should().BeTrue();
        result.Reason.Should().BeEmpty();
    }

    [Fact]
    public void Deny_CreatesDeniedResult()
    {
        var result = EthicsGateResult.Deny("Harmful content detected");

        result.IsDenied.Should().BeTrue();
        result.IsAllowed.Should().BeFalse();
        result.IsUncertain.Should().BeFalse();
        result.Reason.Should().Be("Harmful content detected");
    }

    [Fact]
    public void Uncertain_CreatesUncertainResult()
    {
        var result = EthicsGateResult.Uncertain("Ambiguous intent");

        result.IsUncertain.Should().BeTrue();
        result.IsAllowed.Should().BeFalse();
        result.IsDenied.Should().BeFalse();
        result.Reason.Should().Be("Ambiguous intent");
    }

    [Fact]
    public void Decision_Allow_IsFormMark()
    {
        var result = EthicsGateResult.Allow();

        result.Decision.Should().Be(Form.Mark);
    }

    [Fact]
    public void Decision_Deny_IsFormVoid()
    {
        var result = EthicsGateResult.Deny("reason");

        result.Decision.Should().Be(Form.Void);
    }

    [Fact]
    public void Decision_Uncertain_IsFormImaginary()
    {
        var result = EthicsGateResult.Uncertain("reason");

        result.Decision.Should().Be(Form.Imaginary);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "CognitivePhysics")]
public class CognitiveBranchTests
{
    [Fact]
    public void CognitiveBranch_CanBeCreated()
    {
        var state = CognitiveState.Create("test-focus", 100.0);

        var branch = new CognitiveBranch(state, 0.95);

        branch.State.Should().Be(state);
        branch.Weight.Should().Be(0.95);
    }

    [Fact]
    public void CognitiveBranch_RecordEquality_Works()
    {
        var state = CognitiveState.Create("focus", 50.0);

        var branch1 = new CognitiveBranch(state, 0.5);
        var branch2 = new CognitiveBranch(state, 0.5);

        branch1.Should().Be(branch2);
    }
}
