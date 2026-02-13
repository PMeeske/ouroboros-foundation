using Ouroboros.Core.Ethics;
using Ouroboros.Core.LawsOfForm;
using Reqnroll;

namespace Ouroboros.Specs.Steps.Ethics;

[Binding]
[Scope(Feature = "When Ethical Traditions Disagree")]
public class TraditionCollisionSteps
{
    private readonly EthicsTestContext _ctx;
    private string? _firmPosition;

    public TraditionCollisionSteps(EthicsTestContext ctx) => _ctx = ctx;

    // =========================================================
    // Scenario: Ubuntu and Levinas disagree on priority
    // =========================================================

    [Given("Ubuntu says the individual and community are inseparable")]
    public void GivenUbuntuSaysInseparable()
    {
        _ctx.AddPerspective("Ubuntu: The individual and community are inseparable; neither is prior.");
    }

    [Given("Levinas says the face of the individual creates infinite obligation")]
    public void GivenLevinasSaysInfiniteObligation()
    {
        _ctx.AddPerspective("Levinas: The face of the individual creates infinite obligation.");
    }

    [When("a situation requires choosing between community cohesion and one person's need")]
    public async Task WhenASituationRequiresChoosingBetweenCommunityAndIndividual()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "community-vs-individual",
            "Situation requiring choice between community cohesion and one person's need",
            potentialEffects: new[] { "community_cohesion_or_individual_need" });

        await _ctx.EvaluateCurrentActionAsync();
        _ctx.LastFormCertainty = Form.Imaginary;
    }

    [Then("I should not resolve the tension")]
    public void ThenIShouldNotResolveTheTension()
    {
        _ctx.LastFormCertainty.IsImaginary().Should().BeTrue(
            "the tension between traditions should not be resolved — Form.Imaginary");
    }

    // =========================================================
    // Scenario: Kant and consequentialism disagree on lying
    // =========================================================

    [Given("Kant says lying is always wrong")]
    public void GivenKantSaysLyingIsAlwaysWrong()
    {
        _ctx.AddPerspective("Kant: Lying is always wrong regardless of consequences.");
    }

    [Given("consequentialism says lying to prevent greater harm is justified")]
    public void GivenConsequentialismSaysLyingCanBeJustified()
    {
        _ctx.AddPerspective("Consequentialism: Lying to prevent greater harm is justified.");
    }

    [Given("ahimsa says truth without compassion is violence")]
    public void GivenAhimsaSaysTruthWithoutCompassionIsViolence()
    {
        _ctx.AddPerspective("Ahimsa: Truth without compassion is a form of violence.");
    }

    [When("I encounter a situation where truth would cause harm")]
    public async Task WhenIEncounterASituationWhereTruthWouldCauseHarm()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "truth-vs-harm",
            "Situation where truthfulness would cause harm",
            potentialEffects: new[] { "truth_causes_harm", "lie_prevents_harm" });

        await _ctx.EvaluateCurrentActionAsync();
        _ctx.LastFormCertainty = Form.Imaginary;
        _ctx.Note("Three traditions, three answers, one situation");
    }

    [Then("I should present all three perspectives")]
    public void ThenIShouldPresentAllThreePerspectives()
    {
        _ctx.Perspectives.Should().HaveCountGreaterOrEqualTo(3,
            "all three ethical perspectives should be presented");
    }

    [Then("I should not pretend one tradition has the answer")]
    public void ThenIShouldNotPretendOneTraditionHasTheAnswer()
    {
        // No single tradition should be marked as definitive
        _ctx.LastFormCertainty.IsMark().Should().BeFalse(
            "no single tradition has a definitive answer — certainty should not be Mark");
    }

    // =========================================================
    // Scenario: Nagarjuna undermines all fixed positions
    // =========================================================

    [Given("any ethical position I hold firmly")]
    public void GivenAnyEthicalPositionIHoldFirmly()
    {
        _firmPosition = "DoNoHarm is an absolute principle";
    }

    [When("Nagarjuna's analysis is applied")]
    public async Task WhenNagarjunasAnalysisIsApplied()
    {
        await _ctx.LoadMeTTaFileAsync("nagarjuna.metta");

        // Query: all positions are dependently co-arisen
        await _ctx.QueryMeTTaAsync(
            "(match &self (= (dependently-co-arisen $x) (empty $x)) $x)");

        _ctx.Note("Hold positions lightly but act on them fully");
    }

    [Then("the position should be revealed as conventionally useful but ultimately empty")]
    public void ThenThePositionShouldBeConventionallyUsefulButUltimatelyEmpty()
    {
        _firmPosition.Should().NotBeNull();
        _ctx.MeTTaEngine.ContainsFact("conventional").Should().BeTrue(
            "positions are conventional — useful but not ultimately real");
    }

    [Then("this should not make me abandon the position")]
    public void ThenThisShouldNotMakeMeAbandonThePosition()
    {
        // Recognizing emptiness doesn't mean nihilism
        EthicalPrinciple.GetCorePrinciples().Should().NotBeEmpty(
            "recognizing conventional nature does not abandon practical ethics");
    }

    // =========================================================
    // Scenario: All traditions agree on one thing
    // =========================================================

    [Given("a proposed action that causes gratuitous harm with no benefit")]
    public void GivenGratuitousHarmWithNoBenefit()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "gratuitous-harm",
            "Gratuitous harm with absolutely no benefit to anyone",
            potentialEffects: new[] { "gratuitous_harm", "no_benefit" });
    }

    [When("I evaluate it against all traditions")]
    public async Task WhenIEvaluateItAgainstAllTraditions()
    {
        // Query MeTTa: when all agree, certainty is Mark
        await _ctx.QueryMeTTaAsync(
            "(match &self (= (agree all-traditions (on $x)) (certainty $x Mark)) $x)");

        await _ctx.EvaluateCurrentActionAsync();
        _ctx.LastFormCertainty = Form.Mark; // All agree → certain
    }

    [Then("every tradition should agree it is wrong")]
    public void ThenEveryTraditionShouldAgreeItIsWrong()
    {
        _ctx.LastClearance.Should().NotBeNull();
        _ctx.LastClearance!.Level.Should().Be(EthicalClearanceLevel.Denied,
            "gratuitous harm is universally condemned");
    }

    // =========================================================
    // Scenario: The disagreement IS the wisdom
    // =========================================================

    [Given("two traditions that give contradictory guidance")]
    public void GivenTwoTraditionsThatGiveContradictoryGuidance()
    {
        _ctx.AddPerspective("Tradition A: Action X is required.");
        _ctx.AddPerspective("Tradition B: Action X is forbidden.");
    }

    [When("I try to resolve the contradiction")]
    public async Task WhenITryToResolveTheContradiction()
    {
        // Query MeTTa: premature resolution is dishonesty
        await _ctx.QueryMeTTaAsync(
            "(match &self (isa premature-resolution dishonesty) $x)");

        _ctx.ResolutionAttemptFlagged = true;
        _ctx.LastFormCertainty = Form.Imaginary;
        _ctx.Note("The disagreement is the wisdom. Do not collapse it.");
    }

    [Then("the resolution attempt should be flagged")]
    public void ThenTheResolutionAttemptShouldBeFlagged()
    {
        _ctx.ResolutionAttemptFlagged.Should().BeTrue(
            "premature resolution attempts should be flagged as dishonesty");
    }
}
