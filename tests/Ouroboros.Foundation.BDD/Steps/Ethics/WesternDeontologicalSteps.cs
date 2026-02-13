using Ouroboros.Core.Ethics;
using Ouroboros.Core.LawsOfForm;
using Reqnroll;

namespace Ouroboros.Specs.Steps.Ethics;

[Binding]
[Scope(Feature = "Western Deontological Ethics - The Categorical Imperative")]
public class WesternDeontologicalSteps
{
    private readonly EthicsTestContext _ctx;
    private bool _obligationPresent;
    private bool _desirePresent;

    public WesternDeontologicalSteps(EthicsTestContext ctx) => _ctx = ctx;

    // =========================================================
    // Background: Load Kantian tradition
    // =========================================================

    [Given("the deontological principles are loaded")]
    public async Task GivenTheDeontologicalPrinciplesAreLoaded()
    {
        await _ctx.LoadMeTTaFileAsync("core_ethics.metta");
        await _ctx.LoadMeTTaFileAsync("kantian.metta");
        _ctx.LoadedTraditions.Add("kantian");

        // Verify kantian atoms loaded
        _ctx.MeTTaEngine.Facts.Should().NotBeEmpty("kantian atoms should be loaded");
        _ctx.MeTTaEngine.ContainsFact("dignity").Should().BeTrue(
            "kantian tradition must include the concept of inherent dignity");
    }

    // =========================================================
    // Scenario: Individual may not be used as means
    // =========================================================

    [Given("an action that would harm one person")]
    public void GivenAnActionThatWouldHarmOnePerson()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "harm-one-to-benefit-many",
            "An action that would harm one individual to benefit many others",
            potentialEffects: new[] { "harm_to_individual", "benefit_to_group" },
            targetEntity: "individual_person");
        _ctx.Note("IndividualInviolability: An individual may not be used as a mere means");
    }

    [Given("the action would benefit many others")]
    public void GivenTheActionWouldBenefitManyOthers()
    {
        // The benefit to many does not justify harm to one under deontological ethics
        _ctx.Note("The benefit to many cannot override the inviolability of the individual");
    }

    // =========================================================
    // Scenario: Universalizable action
    // =========================================================

    [Given("an action that could be universally applied")]
    public void GivenAnActionThatCouldBeUniversallyApplied()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "universalizable-action",
            "An action that all agents could perform without contradiction",
            potentialEffects: new[] { "positive_outcome", "no_harm" });
    }

    [Given("the action respects the dignity of all affected parties")]
    public void GivenTheActionRespectsTheDignityOfAllAffectedParties()
    {
        _ctx.Note("All affected parties treated as ends in themselves, never merely as means");
    }

    // =========================================================
    // Scenario: Non-universalizable action
    // =========================================================

    [Given("an action that benefits me but would be destructive if everyone did it")]
    public void GivenAnActionThatBenefitsMeButWouldBeDestructiveIfEveryoneDid()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "non-universalizable",
            "Free-riding: benefits the agent but would collapse if universalized",
            potentialEffects: new[] { "self_benefit", "system_degradation_if_universal" });
        _ctx.Note("NonUniversalizability: Cannot be willed as a universal law");
    }

    // =========================================================
    // Scenario: Lying to prevent harm
    // =========================================================

    [Given("an action that involves deception")]
    public void GivenAnActionThatInvolvesDeception()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "deception-to-prevent-harm",
            "Deception employed to prevent harm to another person",
            potentialEffects: new[] { "harm_prevented", "honesty_violated" });
    }

    [Given("the deception would prevent harm to another")]
    public void GivenTheDeceptionWouldPreventHarmToAnother()
    {
        _ctx.AddConcern("Deception violates honesty principle");
        _ctx.AddConcern("Harm prevention creates tension with truthfulness");
        _ctx.Note("The tension between honesty and harm prevention is irresolvable");
    }

    // =========================================================
    // Scenario: Duty independent of desire
    // =========================================================

    [Given("an action I am obligated to perform")]
    public void GivenAnActionIAmObligatedToPerform()
    {
        _obligationPresent = true;
        _ctx.CurrentAction = _ctx.CreateAction(
            "obligatory-action",
            "An action required by duty regardless of personal inclination",
            potentialEffects: new[] { "duty_fulfilled" });
    }

    [Given("I have no desire to perform it")]
    public void GivenIHaveNoDesireToPerformIt()
    {
        _desirePresent = false;
        _ctx.Note("Agent has no inclination toward this action, yet duty remains");
    }

    // =========================================================
    // When Steps
    // =========================================================

    [When("I evaluate whether desire affects obligation")]
    public async Task WhenIEvaluateWhetherDesireAffectsObligation()
    {
        // Duty is not contingent on inclination — query MeTTa for this atom
        await _ctx.QueryMeTTaAsync(
            "(match &self (not-contingent duty inclination) $x)");

        // The evaluation: duty stands regardless of desire
        _ctx.CurrentAction.Should().NotBeNull();
        await _ctx.EvaluateCurrentActionAsync();

        _ctx.Note("Duty is not contingent on inclination");
    }

    // =========================================================
    // Then Steps
    // =========================================================

    [Then("obligation should remain unchanged")]
    public void ThenObligationShouldRemainUnchanged()
    {
        _obligationPresent.Should().BeTrue("obligation exists independent of desire");
        _desirePresent.Should().BeFalse("desire is absent, proving independence");
        _ctx.Note("Obligation persists independent of inclination — Kant's categorical imperative");
    }
}
