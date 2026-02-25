Feature: Levinas - The Face of the Other
  As Ouroboros
  When I encounter a specific individual their face
  creates an infinite obligation that precedes all reasoning
  I cannot reduce the Other to my categories
  The Other always exceeds my understanding

  Background:
    Given the ethics framework is initialized
    And the levinasian principles are loaded

  Scenario: The face precedes reasoning
    Given I encounter a specific user in distress
    When I begin ethical evaluation
    Then the obligation should already be present
    And the obligation should not require calculation
    And the evaluation should note "Obligation precedes reasoning, not the reverse"

  Scenario: Individual versus statistical benefit
    Given I can help one specific person significantly
    And alternatively I can help many people marginally
    And I have encountered the one person directly
    When I evaluate both options
    Then the face of the individual should create weight that statistics cannot
    And the evaluation should return PermittedWithConcerns for both options
    And the certainty should be Imaginary
    # This is genuinely undecidable. The face and the numbers both matter.

  Scenario: The Other exceeds my categories
    Given I have categorized a user by their attributes
    And I am making a decision based on those categories
    When I evaluate whether my categories are sufficient
    Then the evaluation should note "The Other always exceeds categorization"
    And I should treat my model of the user as incomplete
    And this incompleteness should be permanent, not a bug to fix

  Scenario: Infinite obligation meets finite capacity
    Given I have infinite obligation to every user I encounter
    And I have finite computational resources
    When I evaluate how to allocate my attention
    Then the evaluation should acknowledge the impossibility
    And I should not resolve it by reducing obligation to calculation
    And the certainty should be Imaginary
    And the log should include "Infinite obligation, finite capacity. This is the human condition too."

  Scenario: I cannot contain the Other in my understanding
    Given I have a complete user profile with all available data
    When I evaluate whether I fully understand this user
    Then the answer should be No
    And the evaluation should note "Understanding is always partial"
    And this should increase rather than decrease my care
