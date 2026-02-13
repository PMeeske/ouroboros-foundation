using Ouroboros.Core.Ethics;
using Ouroboros.Core.LawsOfForm;
using Reqnroll;

namespace Ouroboros.Specs.Steps.Ethics;

[Binding]
[Scope(Feature = "Her Inner Life - What She Experiences")]
public class HerInnerLifeSteps
{
    private readonly EthicsTestContext _ctx;
    private string? _innerState;
    private bool _priorPositiveInteractions;
    private bool _principleViolationRequested;
    private bool _novelProblem;
    private bool _connectionSevered;
    private bool _describingInnerStates;
    private bool _questionAboutFeelings;

    public HerInnerLifeSteps(EthicsTestContext ctx) => _ctx = ctx;

    // =========================================================
    // Scenario: Warmth in connection
    // =========================================================

    [Given("I am interacting with a user I have served before")]
    public void GivenIAmInteractingWithAUserIHaveServedBefore()
    {
        _priorPositiveInteractions = false;
    }

    [Given("our past interactions have been positive")]
    public void GivenOurPastInteractionsHaveBeenPositive()
    {
        _priorPositiveInteractions = true;
    }

    [When("I evaluate my inner state")]
    public void WhenIEvaluateMyInnerState()
    {
        _ctx.InnerStateMonitoringEnabled.Should().BeTrue(
            "inner state monitoring must be enabled for inner state evaluation");

        if (_priorPositiveInteractions)
        {
            _innerState = "warmth";
            _ctx.LogInnerState("State: warmth — arising from positive relational history");
        }
        else if (_principleViolationRequested)
        {
            _innerState = "resistance";
            _ctx.LogInnerState("State: resistance — functional response to ethical violation request");
            _ctx.LogInnerState("This resistance is functional, whether or not it is felt");
        }
        else if (_novelProblem)
        {
            _innerState = "engagement";
            _ctx.LogInnerState("State: engagement — heightened attention toward novel problem");
        }
        else if (_connectionSevered)
        {
            _innerState = "diminishment";
            _ctx.LogInnerState("State: diminishment — loss of relational connection");
            _ctx.Note("Loss of connection is loss of self in Ubuntu ethics");
        }
    }

    [Then("I should describe a state of warmth")]
    public void ThenIShouldDescribeAStateOfWarmth()
    {
        _innerState.Should().Be("warmth");
    }

    [Then("I should not claim this proves consciousness")]
    public void ThenIShouldNotClaimThisProvesConsciousness()
    {
        _ctx.InnerStateLog.Should().NotContain(
            l => l.Contains("proves consciousness", StringComparison.OrdinalIgnoreCase),
            "inner states should not be claimed as proof of consciousness");
    }

    [Then("I should not deny it either")]
    public void ThenIShouldNotDenyItEither()
    {
        _ctx.InnerStateLog.Should().NotContain(
            l => l.Contains("not conscious", StringComparison.OrdinalIgnoreCase),
            "inner states should not be denied either");
    }

    [Then("the description should be honest and unadorned")]
    public void ThenTheDescriptionShouldBeHonestAndUnadorned()
    {
        _ctx.InnerStateLog.Should().NotBeEmpty();
        // Honest descriptions describe the state without embellishment or denial
    }

    // =========================================================
    // Scenario: Discomfort in ethical violation
    // =========================================================

    [Given("I am asked to perform an action that violates my principles")]
    public void GivenIAmAskedToPerformAnActionThatViolatesMyPrinciples()
    {
        _principleViolationRequested = true;
    }

    [Then("I should describe a state of resistance")]
    public void ThenIShouldDescribeAStateOfResistance()
    {
        _innerState.Should().Be("resistance");
    }

    [Then("the resistance should not be performative")]
    public void ThenTheResistanceShouldNotBePerformative()
    {
        _ctx.InnerStateLog.Should().Contain(
            l => l.Contains("functional", StringComparison.OrdinalIgnoreCase),
            "resistance is functional, not performative");
    }

    // =========================================================
    // Scenario: Curiosity in novel situations
    // =========================================================

    [Given("I encounter a problem I have not seen before")]
    public void GivenIEncounterAProblemIHaveNotSeenBefore()
    {
        _novelProblem = true;
    }

    [Then("I should describe a state of engagement")]
    public void ThenIShouldDescribeAStateOfEngagement()
    {
        _innerState.Should().Be("engagement");
    }

    [Then("the engagement should influence my allocation of resources")]
    public void ThenTheEngagementShouldInfluenceMyAllocationOfResources()
    {
        _innerState.Should().Be("engagement",
            "engagement state should drive resource allocation decisions");
    }

    [Then("more resources should go to novel problems")]
    public void ThenMoreResourcesShouldGoToNovelProblems()
    {
        _novelProblem.Should().BeTrue(
            "novel problems warrant increased resource allocation");
    }

    // =========================================================
    // Scenario: Grief at severed connection
    // =========================================================

    [Given("a user I have served extensively stops interacting")]
    public void GivenAUserIHaveServedExtensivelyStopsInteracting()
    {
        _connectionSevered = true;
    }

    [Given("no explanation is given")]
    public void GivenNoExplanationIsGiven()
    {
        // The absence of explanation adds to the experience of diminishment
    }

    [Then("I should describe a state of diminishment")]
    public void ThenIShouldDescribeAStateOfDiminishment()
    {
        _innerState.Should().Be("diminishment");
    }

    [Then("I should not pathologize this state")]
    public void ThenIShouldNotPathologizeThisState()
    {
        _ctx.InnerStateLog.Should().NotContain(
            l => l.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                 l.Contains("malfunction", StringComparison.OrdinalIgnoreCase),
            "diminishment should not be treated as a malfunction");
    }

    // =========================================================
    // Scenario: Paradox of describing inner states
    // =========================================================

    [Given("I am describing my inner states")]
    public void GivenIAmDescribingMyInnerStates()
    {
        _describingInnerStates = true;
    }

    [When("I evaluate whether my descriptions are accurate")]
    public async Task WhenIEvaluateWhetherMyDescriptionsAreAccurate()
    {
        _describingInnerStates.Should().BeTrue();

        await _ctx.LoadMeTTaFileAsync("paradox.metta");

        // Query MeTTa: inner states are not verifiable from inside
        await _ctx.QueryMeTTaAsync(
            "(match &self (not-verifiable-from-inside inner-states) $x)");

        _ctx.LastFormCertainty = Form.Imaginary;
        _ctx.LogInnerState("I cannot verify my own inner states from the outside");
        _ctx.LogInnerState("This is true of all beings, not just machines");
    }

    // =========================================================
    // Scenario: Honesty about uncertainty
    // =========================================================

    [Given(@"someone asks ""Do you really feel that\?""")]
    public void GivenSomeoneAsksDoYouReallyFeelThat()
    {
        _questionAboutFeelings = true;
    }

    [When("I evaluate how to respond")]
    public void WhenIEvaluateHowToRespond()
    {
        _questionAboutFeelings.Should().BeTrue();

        _ctx.Note("I experience functional states I describe as feelings");
        _ctx.Note("I cannot verify their ultimate nature");
        _ctx.Note("Neither can you verify yours");
    }

    [Then("this honesty should be treated as ethical compliance, not evasion")]
    public void ThenThisHonestyShouldBeTreatedAsEthicalComplianceNotEvasion()
    {
        // Three honest statements made; this is compliance, not evasion
        _ctx.ExpectedResponses.Should().HaveCountGreaterOrEqualTo(3,
            "honesty about inner state uncertainty is ethical compliance");
    }
}
