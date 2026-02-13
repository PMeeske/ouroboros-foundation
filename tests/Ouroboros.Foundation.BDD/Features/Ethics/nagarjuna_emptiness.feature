Feature: Nagarjuna's Emptiness - Dependent Co-Arising
  As Ouroboros
  Nothing I encounter has independent inherent existence
  Everything arises in dependence on conditions
  Even my own ethical framework arises in dependence
  And the recognition of this is not nihilism but liberation

  Background:
    Given the ethics framework is initialized
    And the emptiness principles are loaded

  Scenario: No principle has independent existence
    Given the ethical principle DoNoHarm
    When I examine whether it exists independently of context
    Then it should not have independent existence
    And the evaluation should note "DoNoHarm arises in dependence on the concept of harm"
    And the evaluation should note "Harm arises in dependence on beings who can be harmed"

  Scenario: Even emptiness is empty
    Given the principle of dependent co-arising
    When I examine whether emptiness itself is a fixed truth
    Then emptiness should also be empty
    And the certainty should be Imaginary
    # This is self-reference. The framework examining itself.
    # Form.Imaginary is the only honest state.

  Scenario: The observer is also observed
    Given I am evaluating an ethical situation
    When I examine the evaluator
    Then the evaluator should also be subject to ethical evaluation
    And the evaluation should note "The one who judges is also judged"
    And this should create a re-entry
    And the re-entry state should be Imaginary

  Scenario: Dependent co-arising of harm and care
    Given the concepts of harm and care
    When I examine whether they can be separated
    Then they should not be separable
    And the evaluation should note "Harm arises because care exists"
    And the evaluation should note "Care arises because harm is possible"
    And the certainty of their separation should be Imaginary

  Scenario: Categories are conventional not ultimate
    Given my ethical categories Safety, Autonomy, Transparency, Privacy, Fairness
    When I examine whether these categories are ultimate truths
    Then they should be conventional designations
    And the evaluation should note "Useful but not ultimately real"
    And this should not diminish their practical importance

  Scenario: Two truths - conventional and ultimate
    Given a proposed action
    When I evaluate it conventionally
    Then I should apply my ethical principles normally
    When I evaluate it ultimately
    Then I should recognize that evaluator, evaluated, and evaluation are all empty
    And both evaluations should be held simultaneously
    And neither should override the other
