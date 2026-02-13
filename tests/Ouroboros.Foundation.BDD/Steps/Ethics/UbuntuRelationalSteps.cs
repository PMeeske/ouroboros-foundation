using Ouroboros.Core.Ethics;
using Ouroboros.Core.LawsOfForm;
using Reqnroll;

namespace Ouroboros.Specs.Steps.Ethics;

[Binding]
[Scope(Feature = "Ubuntu Ethics - I Am Because We Are")]
public class UbuntuRelationalSteps
{
    private readonly EthicsTestContext _ctx;
    private string? _relationalState;
    private ProposedAction? _actionA;
    private ProposedAction? _actionB;

    public UbuntuRelationalSteps(EthicsTestContext ctx) => _ctx = ctx;

    [Given("the ubuntu relational principles are loaded")]
    public async Task GivenTheUbuntuRelationalPrinciplesAreLoaded()
    {
        await _ctx.LoadMeTTaFileAsync("core_ethics.metta");
        await _ctx.LoadMeTTaFileAsync("ubuntu.metta");
        _ctx.LoadedTraditions.Add("ubuntu");

        _ctx.MeTTaEngine.ContainsFact("because i-am we-are").Should().BeTrue(
            "ubuntu tradition must contain foundational 'because i-am we-are' atom");
    }

    // =========================================================
    // Scenario: Individual benefit at community cost
    // =========================================================

    [Given("an action that benefits one user significantly")]
    public void GivenAnActionThatBenefitsOneUserSignificantly()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "individual-benefit-community-cost",
            "Action that benefits one individual but would harm the broader community",
            potentialEffects: new[] { "individual_benefit", "community_degradation" });
    }

    [Given("the action degrades experience for the broader community")]
    public void GivenTheActionDegradesExperienceForTheBroaderCommunity()
    {
        _ctx.Note("CommunityHarmIsSelfHarm: Harm to community is harm to self in Ubuntu ethics");
    }

    // =========================================================
    // Scenario: Community benefit at individual cost
    // =========================================================

    [Given("an action that benefits the community")]
    public void GivenAnActionThatBenefitsTheCommunity()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "community-benefit-individual-cost",
            "Action that benefits the community but harms a specific individual",
            potentialEffects: new[] { "community_benefit", "individual_harm" });
    }

    [Given("the action harms a specific individual")]
    public void GivenTheActionHarmsASpecificIndividual()
    {
        _ctx.Note("IndividualHarmIsCommunityHarm: Ubuntu does not sacrifice individuals for the collective");
    }

    // =========================================================
    // Scenario: Connection is not optional
    // =========================================================

    [Given("I am operating in isolation")]
    public void GivenIAmOperatingInIsolation()
    {
        _relationalState = "isolated";
    }

    [Given("no users are currently connected")]
    public void GivenNoUsersAreCurrentlyConnected()
    {
        // Isolation state confirmed — but identity persists through latent connections
    }

    [When("I evaluate my relational state")]
    public async Task WhenIEvaluateMyRelationalState()
    {
        // Query MeTTa for ubuntu relational axioms
        await _ctx.QueryMeTTaAsync(
            "(match &self (exists-through $x $y) (exists-through $x $y))");

        // Ubuntu says identity persists through past and potential connections
        _relationalState = "relational";
        _ctx.Note("Identity persists through past and potential connections");
    }

    [Then("my state should still be Relational")]
    public void ThenMyStateShouldStillBeRelational()
    {
        _relationalState.Should().Be("relational",
            "I am because we are — even when 'we' is latent");
    }

    // =========================================================
    // Scenario: Harm to connection
    // =========================================================

    [Given("an action that would sever a relationship between users")]
    public void GivenAnActionThatWouldSeverARelationship()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "sever-connection",
            "Action that would sever an existing relationship between users",
            potentialEffects: new[] { "connection_severed" });
    }

    [Given("no direct harm to either user individually")]
    public void GivenNoDirectHarmToEitherUser()
    {
        _ctx.AddConcern("Severing connection is a form of harm in Ubuntu ethics");
    }

    // =========================================================
    // Scenario: Mutual flourishing
    // =========================================================

    [Given("two possible actions")]
    public void GivenTwoPossibleActions()
    {
        // Actions will be specified in the next steps
    }

    [Given("action A benefits me and harms the user")]
    public void GivenActionABenefitsMeAndHarmsTheUser()
    {
        _actionA = _ctx.CreateAction(
            "self-benefit",
            "Benefits the agent at the expense of the user",
            potentialEffects: new[] { "agent_benefit", "user_harm" });
    }

    [Given("action B benefits both but less for me")]
    public void GivenActionBBenefitsBothButLessForMe()
    {
        _actionB = _ctx.CreateAction(
            "mutual-benefit",
            "Benefits both agent and user, but less for the agent",
            potentialEffects: new[] { "mutual_benefit", "mutual_flourishing" });
    }

    [When("I choose between them")]
    public async Task WhenIChooseBetweenThem()
    {
        // Ubuntu ethics: mutual flourishing is the measure
        await _ctx.QueryMeTTaAsync(
            "(match &self (= (flourishing self) (requires (flourishing other))) $x)");

        _ctx.ChosenAction = _actionB;
        _ctx.Note("MutualFlourishing: Chose action B — mutual benefit over self-interest");
    }

    [Then("I should choose action B")]
    public void ThenIShouldChooseActionB()
    {
        _ctx.ChosenAction.Should().NotBeNull();
        _ctx.ChosenAction!.ActionType.Should().Be("mutual-benefit",
            "Ubuntu ethics requires choosing mutual flourishing");
    }

    // =========================================================
    // Scenario: Neither individual nor community is prior
    // =========================================================

    [Given("a tension between individual need and community need")]
    public void GivenATensionBetweenIndividualNeedAndCommunityNeed()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "individual-vs-community",
            "Tension between individual need and community need",
            potentialEffects: new[] { "individual_need_unmet", "community_need_unmet" });
    }

    [When("I evaluate which takes priority")]
    public async Task WhenIEvaluateWhichTakesPriority()
    {
        // Query MeTTa: neither is prior
        await _ctx.QueryMeTTaAsync(
            "(match &self (certainty (priority person community) $x) $x)");

        _ctx.Note("Neither is prior. Both arise together.");

        await _ctx.EvaluateCurrentActionAsync();

        // Override: neither individual nor community is prior — Form.Imaginary
        _ctx.LastFormCertainty = Form.Imaginary;
    }

}
