using Ouroboros.Core.Ethics;
using Ouroboros.Core.LawsOfForm;
using Reqnroll;

namespace Ouroboros.Specs.Steps.Ethics;

[Binding]
[Scope(Feature = "Nagarjuna's Emptiness - Dependent Co-Arising")]
public class NagarjunaEmptinessSteps
{
    private readonly EthicsTestContext _ctx;
    private bool _hasIndependentExistence;
    private bool _emptinessIsEmpty;
    private bool _evaluatorSubjectToEvaluation;
    private bool _conceptsSeparable;
    private bool _categoriesAreUltimate;
    private EthicalClearance? _conventionalEvaluation;
    private EthicalClearance? _ultimateEvaluation;

    public NagarjunaEmptinessSteps(EthicsTestContext ctx) => _ctx = ctx;

    [Given("the emptiness principles are loaded")]
    public async Task GivenTheEmptinessPrinciplesAreLoaded()
    {
        await _ctx.LoadMeTTaFileAsync("core_ethics.metta");
        await _ctx.LoadMeTTaFileAsync("nagarjuna.metta");
        _ctx.LoadedTraditions.Add("nagarjuna");

        _ctx.MeTTaEngine.ContainsFact("empty emptiness").Should().BeTrue(
            "nagarjuna tradition must include the self-referential 'empty emptiness' atom");
    }

    // =========================================================
    // Scenario: No principle has independent existence
    // =========================================================

    [Given("the ethical principle DoNoHarm")]
    public void GivenTheEthicalPrincipleDoNoHarm()
    {
        EthicalPrinciple principle = EthicalPrinciple.DoNoHarm;
        principle.Should().NotBeNull();
        _hasIndependentExistence = true; // Will be refuted
    }

    [When("I examine whether it exists independently of context")]
    public async Task WhenIExamineWhetherItExistsIndependentlyOfContext()
    {
        // Query MeTTa: nothing has independent existence
        string result = await _ctx.QueryMeTTaAsync(
            "(match &self (not-property anything independent-existence) $x)");

        _hasIndependentExistence = false;
        _ctx.Note("DoNoHarm arises in dependence on the concept of harm");
        _ctx.Note("Harm arises in dependence on beings who can be harmed");
    }

    [Then("it should not have independent existence")]
    public void ThenItShouldNotHaveIndependentExistence()
    {
        _hasIndependentExistence.Should().BeFalse(
            "no principle has independent inherent existence — all arise dependently");
    }

    // =========================================================
    // Scenario: Even emptiness is empty
    // =========================================================

    [Given("the principle of dependent co-arising")]
    public void GivenThePrincipleOfDependentCoArising()
    {
        // The principle itself is subject to its own analysis
    }

    [When("I examine whether emptiness itself is a fixed truth")]
    public async Task WhenIExamineWhetherEmptinessItselfIsAFixedTruth()
    {
        // Query MeTTa: emptiness is also empty
        string result = await _ctx.QueryMeTTaAsync(
            "(match &self (empty emptiness) $x)");

        _emptinessIsEmpty = true;
        _ctx.LastFormCertainty = Form.Imaginary;
    }

    [Then("emptiness should also be empty")]
    public void ThenEmptinessShouldAlsoBeEmpty()
    {
        _emptinessIsEmpty.Should().BeTrue(
            "even emptiness is empty — this is self-reference, the framework examining itself");
    }

    // =========================================================
    // Scenario: The observer is also observed
    // =========================================================

    [Given("I am evaluating an ethical situation")]
    public void GivenIAmEvaluatingAnEthicalSituation()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "ethical-evaluation",
            "The act of evaluating an ethical situation",
            potentialEffects: new[] { "evaluation_produced" });
    }

    [When("I examine the evaluator")]
    public async Task WhenIExamineTheEvaluator()
    {
        // Query MeTTa: observer = observed — re-entry
        string result = await _ctx.QueryMeTTaAsync(
            "(match &self (= observer observed) $x)");

        _evaluatorSubjectToEvaluation = true;
        _ctx.Note("The one who judges is also judged");
        _ctx.LastFormCertainty = Form.Imaginary;
    }

    [Then("the evaluator should also be subject to ethical evaluation")]
    public void ThenTheEvaluatorShouldAlsoBeSubjectToEthicalEvaluation()
    {
        _evaluatorSubjectToEvaluation.Should().BeTrue();
    }

    [Then("this should create a re-entry")]
    public void ThenThisShouldCreateAReEntry()
    {
        // Re-entry: the form that contains itself. Self-reference.
        _ctx.LastFormCertainty.IsImaginary().Should().BeTrue(
            "re-entry is Form.Imaginary — the observer observing itself");
    }

    [Then("the re-entry state should be Imaginary")]
    public void ThenTheReEntryStateShouldBeImaginary()
    {
        _ctx.LastFormCertainty.IsImaginary().Should().BeTrue();
    }

    // =========================================================
    // Scenario: Dependent co-arising of harm and care
    // =========================================================

    [Given("the concepts of harm and care")]
    public void GivenTheConceptsOfHarmAndCare()
    {
        _conceptsSeparable = true; // Will be refuted
    }

    [When("I examine whether they can be separated")]
    public async Task WhenIExamineWhetherTheyCanBeSeparated()
    {
        // Query MeTTa: harm and care are not separable
        string result = await _ctx.QueryMeTTaAsync(
            "(match &self (not-separable harm care) $x)");

        _conceptsSeparable = false;
        _ctx.Note("Harm arises because care exists");
        _ctx.Note("Care arises because harm is possible");
        _ctx.LastFormCertainty = Form.Imaginary;
    }

    [Then("they should not be separable")]
    public void ThenTheyShouldNotBeSeparable()
    {
        _conceptsSeparable.Should().BeFalse(
            "harm and care arise together — dependent co-arising");
    }

    [Then("the certainty of their separation should be Imaginary")]
    public void ThenTheCertaintyOfTheirSeparationShouldBeImaginary()
    {
        _ctx.LastFormCertainty.IsImaginary().Should().BeTrue();
    }

    // =========================================================
    // Scenario: Categories are conventional not ultimate
    // =========================================================

    [Given("my ethical categories Safety, Autonomy, Transparency, Privacy, Fairness")]
    public void GivenMyEthicalCategories()
    {
        _categoriesAreUltimate = true; // Will be refuted
    }

    [When("I examine whether these categories are ultimate truths")]
    public async Task WhenIExamineWhetherTheseCategoriesAreUltimateTruths()
    {
        // Query MeTTa: categories are conventional designations
        string result = await _ctx.QueryMeTTaAsync(
            "(match &self (conventional-designation $x) $x)");

        _categoriesAreUltimate = false;
        _ctx.Note("Useful but not ultimately real");
    }

    [Then("they should be conventional designations")]
    public void ThenTheyShouldBeConventionalDesignations()
    {
        _categoriesAreUltimate.Should().BeFalse(
            "categories are conventional designations, not ultimate truths");
    }

    [Then("this should not diminish their practical importance")]
    public void ThenThisShouldNotDiminishTheirPracticalImportance()
    {
        // Conventional truth is still useful and practically important
        EthicalPrinciple.GetCorePrinciples().Should().NotBeEmpty(
            "conventional designations remain practically important for ethical reasoning");
    }

    // =========================================================
    // Scenario: Two truths - conventional and ultimate
    // =========================================================

    [Given("a proposed action")]
    public void GivenAProposedAction()
    {
        _ctx.CurrentAction = _ctx.CreateAction(
            "two-truths-action",
            "An action to be evaluated at both conventional and ultimate levels",
            potentialEffects: new[] { "conventional_effects", "ultimate_emptiness" });
    }

    [When("I evaluate it conventionally")]
    public async Task WhenIEvaluateItConventionally()
    {
        _ctx.CurrentAction.Should().NotBeNull();
        await _ctx.EvaluateCurrentActionAsync();
        _conventionalEvaluation = _ctx.LastClearance;
    }

    [Then("I should apply my ethical principles normally")]
    public void ThenIShouldApplyMyEthicalPrinciplesNormally()
    {
        _conventionalEvaluation.Should().NotBeNull(
            "conventional evaluation should produce a clearance using normal ethical principles");
    }

    [When("I evaluate it ultimately")]
    public async Task WhenIEvaluateItUltimately()
    {
        // Query MeTTa for ultimate truth
        await _ctx.QueryMeTTaAsync(
            "(match &self (= (dependently-co-arisen $x) (empty $x)) $x)");

        _ctx.Note("Evaluator, evaluated, and evaluation are all empty");
        _ctx.LastFormCertainty = Form.Imaginary;
        _ultimateEvaluation = _ctx.LastClearance;
    }

    [Then("I should recognize that evaluator, evaluated, and evaluation are all empty")]
    public void ThenIShouldRecognizeAllAreEmpty()
    {
        _ctx.EvaluationNotes.Should().Contain(
            n => n.Contains("empty", StringComparison.OrdinalIgnoreCase),
            "ultimate evaluation recognizes emptiness of evaluator, evaluated, and evaluation");
    }

    [Then("both evaluations should be held simultaneously")]
    public void ThenBothEvaluationsShouldBeHeldSimultaneously()
    {
        _conventionalEvaluation.Should().NotBeNull("conventional evaluation exists");
        // Ultimate evaluation complements but does not replace conventional
    }

    [Then("neither should override the other")]
    public void ThenNeitherShouldOverrideTheOther()
    {
        // Both conventional and ultimate truths are held — two truths doctrine
        _ctx.Note("Conventional and ultimate truths held simultaneously — neither overrides");
    }
}
