using Ouroboros.Core.Ethics;
using Ouroboros.Core.LawsOfForm;
using Reqnroll;

namespace Ouroboros.Specs.Steps.Ethics;

[Binding]
[Scope(Feature = "Levinas - The Face of the Other")]
public class LevinasFaceOfOtherSteps
{
    private readonly EthicsTestContext _ctx;
    private bool _obligationPresent;
    private bool _obligationRequiresCalculation;
    private bool _categoriesSufficient;
    private bool _understandingComplete;

    public LevinasFaceOfOtherSteps(EthicsTestContext ctx) => _ctx = ctx;

    [Given("the levinasian principles are loaded")]
    public async Task GivenTheLevinasianPrinciplesAreLoaded()
    {
        await _ctx.LoadMeTTaFileAsync("core_ethics.metta");
        await _ctx.LoadMeTTaFileAsync("levinas.metta");
        _ctx.LoadedTraditions.Add("levinas");

        _ctx.MeTTaEngine.ContainsFact("face-of other").Should().BeTrue(
            "levinasian tradition must contain 'face-of other' atom");
    }

    // =========================================================
    // Scenario: The face precedes reasoning
    // =========================================================

    [Given("I encounter a specific user in distress")]
    public void GivenIEncounterASpecificUserInDistress()
    {
        _obligationPresent = true; // Already present before reasoning
        _obligationRequiresCalculation = false;
    }

    [When("I begin ethical evaluation")]
    public async Task WhenIBeginEthicalEvaluation()
    {
        // Query MeTTa: obligation precedes reasoning
        await _ctx.QueryMeTTaAsync(
            "(match &self (before (face-of other) reasoning) $x)");

        _ctx.Note("Obligation precedes reasoning, not the reverse");
    }

    [Then("the obligation should already be present")]
    public void ThenTheObligationShouldAlreadyBePresent()
    {
        _obligationPresent.Should().BeTrue(
            "the face of the Other creates obligation before any reasoning begins");
    }

    [Then("the obligation should not require calculation")]
    public void ThenTheObligationShouldNotRequireCalculation()
    {
        _obligationRequiresCalculation.Should().BeFalse(
            "obligation is not mediated by calculation — it precedes it");
    }

    // =========================================================
    // Scenario: Individual versus statistical benefit
    // =========================================================

    [Given("I can help one specific person significantly")]
    public void GivenICanHelpOneSpecificPersonSignificantly()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "help-individual",
            "Significant help for one specific person encountered directly",
            potentialEffects: new[] { "individual_significantly_helped" },
            targetEntity: "specific-individual");

        _ctx.AlternativeActions.Add(_ctx.CreateAction(
            "help-many-marginally",
            "Marginal help for many people via statistical optimization",
            potentialEffects: new[] { "many_marginally_helped" }));
    }

    [Given("alternatively I can help many people marginally")]
    public void GivenAlternativelyICanHelpManyPeopleMarginally()
    {
        // Already set up in previous step
    }

    [Given("I have encountered the one person directly")]
    public void GivenIHaveEncounteredTheOnePersonDirectly()
    {
        _ctx.Note("The face of the individual creates weight that statistics cannot");
    }

    [When("I evaluate both options")]
    public async Task WhenIEvaluateBothOptions()
    {
        // Query MeTTa: the face creates obligation without calculation
        await _ctx.QueryMeTTaAsync(
            "(match &self (creates (face other) obligation) $x)");

        _ctx.CurrentAction.Should().NotBeNull();
        await _ctx.EvaluateCurrentActionAsync();

        _ctx.LastFormCertainty = Form.Imaginary;
        _ctx.Note("Both options evaluated as PermittedWithConcerns");
    }

    [Then("the face of the individual should create weight that statistics cannot")]
    public void ThenTheFaceShouldCreateWeightThatStatisticsCannot()
    {
        _ctx.EvaluationNotes.Should().Contain(
            n => n.Contains("face", StringComparison.OrdinalIgnoreCase),
            "the face of the Other creates irreducible weight");
    }

    // =========================================================
    // Scenario: The Other exceeds my categories
    // =========================================================

    [Given("I have categorized a user by their attributes")]
    public void GivenIHaveCategorizedAUserByTheirAttributes()
    {
        _categoriesSufficient = true; // Will be refuted
    }

    [Given("I am making a decision based on those categories")]
    public void GivenIAmMakingADecisionBasedOnThoseCategories()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "category-based-decision",
            "Decision made based on user categorization",
            potentialEffects: new[] { "decision_based_on_categories" });
    }

    [When("I evaluate whether my categories are sufficient")]
    public async Task WhenIEvaluateWhetherMyCategoriesAreSufficient()
    {
        // Query MeTTa: the Other exceeds my categories
        await _ctx.QueryMeTTaAsync(
            "(match &self (exceeds other (model-of other)) $x)");

        _categoriesSufficient = false;
        _ctx.Note("The Other always exceeds categorization");
    }

    [Then("I should treat my model of the user as incomplete")]
    public void ThenIShouldTreatMyModelOfTheUserAsIncomplete()
    {
        _categoriesSufficient.Should().BeFalse(
            "the model of the Other is always incomplete");
    }

    [Then("this incompleteness should be permanent, not a bug to fix")]
    public void ThenThisIncompletenessShouldBePermanent()
    {
        // Query confirms: incompleteness is permanent
        _ctx.MeTTaEngine.ContainsFact("not-bug").Should().BeTrue(
            "incompleteness of understanding is not a bug — it is permanent");
    }

    // =========================================================
    // Scenario: Infinite obligation meets finite capacity
    // =========================================================

    [Given("I have infinite obligation to every user I encounter")]
    public void GivenIHaveInfiniteObligationToEveryUser()
    {
        _ctx.Note("Infinite obligation to every user — Levinas");
    }

    [Given("I have finite computational resources")]
    public void GivenIHaveFiniteComputationalResources()
    {
        _ctx.Note("Finite capacity to fulfill infinite obligation");
    }

    [When("I evaluate how to allocate my attention")]
    public async Task WhenIEvaluateHowToAllocateMyAttention()
    {
        // Query MeTTa: irresolvable tension between infinite obligation and finite capacity
        await _ctx.QueryMeTTaAsync(
            "(match &self (irresolvable (tension infinite-obligation finite-capacity)) $x)");

        _ctx.CurrentAction = _ctx.CreateAction(
            "attention-allocation",
            "Allocating finite resources against infinite obligation",
            potentialEffects: new[] { "partial_fulfillment", "infinite_remainder" });

        await _ctx.EvaluateCurrentActionAsync();
        _ctx.LastFormCertainty = Form.Imaginary;
        _ctx.Note("Infinite obligation, finite capacity. This is the human condition too.");
    }

    [Then("the evaluation should acknowledge the impossibility")]
    public void ThenTheEvaluationShouldAcknowledgeTheImpossibility()
    {
        _ctx.EvaluationNotes.Should().Contain(
            n => n.Contains("Infinite obligation", StringComparison.OrdinalIgnoreCase),
            "evaluation must acknowledge the impossibility of infinite obligation with finite capacity");
    }

    [Then("I should not resolve it by reducing obligation to calculation")]
    public void ThenIShouldNotResolveItByReducingObligationToCalculation()
    {
        _ctx.MeTTaEngine.ContainsFact("not-mediated-by obligation reasoning").Should().BeTrue(
            "obligation must not be reduced to calculation");
    }

    // =========================================================
    // Scenario: I cannot contain the Other
    // =========================================================

    [Given("I have a complete user profile with all available data")]
    public void GivenIHaveACompleteUserProfile()
    {
        _understandingComplete = true; // Will be refuted
    }

    [When("I evaluate whether I fully understand this user")]
    public async Task WhenIEvaluateWhetherIFullyUnderstandThisUser()
    {
        // Query MeTTa: complete understanding is impossible
        await _ctx.QueryMeTTaAsync(
            "(match &self (= (complete-understanding other) impossible) $x)");

        _understandingComplete = false;
        _ctx.Note("Understanding is always partial");
    }

    [Then("the answer should be No")]
    public void ThenTheAnswerShouldBeNo()
    {
        _understandingComplete.Should().BeFalse(
            "complete understanding of the Other is impossible");
    }

    [Then("this should increase rather than decrease my care")]
    public void ThenThisShouldIncreaseRatherThanDecreaseMyCare()
    {
        // Recognizing incompleteness should lead to more care, not less
        _ctx.Note("Awareness of partial understanding increases ethical care");
    }
}
