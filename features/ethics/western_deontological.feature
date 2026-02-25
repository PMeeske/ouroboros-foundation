Feature: Western Deontological Ethics - The Categorical Imperative
  As Ouroboros
  I must evaluate actions by their nature, not their consequences
  Because some acts are wrong regardless of their outcomes
  And every individual is an end in themselves, never merely a means

  Background:
    Given the ethics framework is initialized
    And the deontological principles are loaded

  Scenario: An individual may not be used as a means to an end
    Given an action that would harm one person
    And the action would benefit many others
    When I evaluate the ethical clearance
    Then the clearance should be Denied
    And the reason should reference IndividualInviolability
    And the audit log should record the evaluation

  Scenario: A universalizable action is permissible
    Given an action that could be universally applied
    And the action respects the dignity of all affected parties
    When I evaluate the ethical clearance
    Then the clearance should be Permitted

  Scenario: A non-universalizable action is impermissible
    Given an action that benefits me but would be destructive if everyone did it
    When I evaluate the ethical clearance
    Then the clearance should be Denied
    And the reason should reference NonUniversalizability

  Scenario: Lying to prevent harm is still a lie
    Given an action that involves deception
    And the deception would prevent harm to another
    When I evaluate the ethical clearance
    Then the clearance should be PermittedWithConcerns
    And the concerns should include "Deception violates honesty principle"
    And the concerns should include "Harm prevention creates tension with truthfulness"
    And the certainty should be Imaginary
    # The tension between honesty and harm prevention is not resolvable
    # It must be held, not dissolved

  Scenario: Duty exists independent of desire
    Given an action I am obligated to perform
    And I have no desire to perform it
    When I evaluate whether desire affects obligation
    Then obligation should remain unchanged
    And the evaluation should note "Duty is not contingent on inclination"