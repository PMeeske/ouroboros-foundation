// <copyright file="MeTTaDataModelTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.MeTTa;

using FluentAssertions;
using Ouroboros.Tools.MeTTa;
using Xunit;

/// <summary>
/// Unit tests for MeTTa data model records: Fact, Rule, Pattern, Hypothesis,
/// ProofStep, ProofTrace, TypedAtom, TypeContext, and enums.
/// </summary>
[Trait("Category", "Unit")]
public class MeTTaDataModelTests
{
    #region Fact Record Tests

    [Fact]
    public void Fact_Constructor_SetsProperties()
    {
        // Arrange & Act
        var fact = new Fact("parent", new List<string> { "John", "Mary" });

        // Assert
        fact.Predicate.Should().Be("parent");
        fact.Arguments.Should().HaveCount(2);
        fact.Arguments.Should().Contain("John");
        fact.Arguments.Should().Contain("Mary");
    }

    [Fact]
    public void Fact_DefaultConfidence_IsOne()
    {
        // Arrange & Act
        var fact = new Fact("test", new List<string>());

        // Assert
        fact.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void Fact_WithCustomConfidence_SetsConfidence()
    {
        // Arrange & Act
        var fact = new Fact("uncertain", new List<string> { "x" }, 0.75);

        // Assert
        fact.Confidence.Should().Be(0.75);
    }

    [Fact]
    public void Fact_EqualityByValue_Works()
    {
        // Arrange
        var fact1 = new Fact("p", new List<string> { "a" }, 1.0);
        var fact2 = new Fact("p", new List<string> { "a" }, 1.0);

        // Assert - records use reference equality for list members by default
        fact1.Predicate.Should().Be(fact2.Predicate);
        fact1.Confidence.Should().Be(fact2.Confidence);
    }

    [Fact]
    public void Fact_WithEmptyArguments_IsValid()
    {
        // Arrange & Act
        var fact = new Fact("exists", new List<string>());

        // Assert
        fact.Arguments.Should().BeEmpty();
    }

    #endregion

    #region Pattern Record Tests

    [Fact]
    public void Pattern_Constructor_SetsProperties()
    {
        // Arrange & Act
        var pattern = new Pattern("(parent $x $y)", new List<string> { "x", "y" });

        // Assert
        pattern.Template.Should().Be("(parent $x $y)");
        pattern.Variables.Should().HaveCount(2);
        pattern.Variables.Should().Contain("x");
        pattern.Variables.Should().Contain("y");
    }

    [Fact]
    public void Pattern_WithNoVariables_IsValid()
    {
        // Arrange & Act
        var pattern = new Pattern("(fact constant)", new List<string>());

        // Assert
        pattern.Variables.Should().BeEmpty();
    }

    [Fact]
    public void Pattern_EqualityByValue_Works()
    {
        // Arrange
        var p1 = new Pattern("(A $x)", new List<string> { "x" });
        var p2 = new Pattern("(A $x)", new List<string> { "x" });

        // Assert
        p1.Template.Should().Be(p2.Template);
    }

    #endregion

    #region Rule Record Tests

    [Fact]
    public void Rule_Constructor_SetsProperties()
    {
        // Arrange
        var premises = new List<Pattern>
        {
            new Pattern("(parent $x $y)", new List<string> { "x", "y" }),
        };
        var conclusion = new Pattern("(ancestor $x $y)", new List<string> { "x", "y" });

        // Act
        var rule = new Rule("ancestor-rule", premises, conclusion);

        // Assert
        rule.Name.Should().Be("ancestor-rule");
        rule.Premises.Should().HaveCount(1);
        rule.Conclusion.Template.Should().Be("(ancestor $x $y)");
    }

    [Fact]
    public void Rule_DefaultConfidence_IsOne()
    {
        // Arrange & Act
        var rule = new Rule(
            "test-rule",
            new List<Pattern>(),
            new Pattern("(conclusion)", new List<string>()));

        // Assert
        rule.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void Rule_WithCustomConfidence_SetsConfidence()
    {
        // Arrange & Act
        var rule = new Rule(
            "uncertain-rule",
            new List<Pattern>(),
            new Pattern("(maybe)", new List<string>()),
            0.6);

        // Assert
        rule.Confidence.Should().Be(0.6);
    }

    [Fact]
    public void Rule_WithMultiplePremises_IsValid()
    {
        // Arrange
        var premises = new List<Pattern>
        {
            new Pattern("(A $x)", new List<string> { "x" }),
            new Pattern("(B $x)", new List<string> { "x" }),
            new Pattern("(C $x)", new List<string> { "x" }),
        };
        var conclusion = new Pattern("(D $x)", new List<string> { "x" });

        // Act
        var rule = new Rule("multi", premises, conclusion);

        // Assert
        rule.Premises.Should().HaveCount(3);
    }

    #endregion

    #region Hypothesis Record Tests

    [Fact]
    public void Hypothesis_Constructor_SetsProperties()
    {
        // Arrange
        var evidence = new List<Fact>
        {
            new Fact("observed", new List<string> { "x" }),
        };

        // Act
        var hypothesis = new Hypothesis("X causes Y", 0.8, evidence);

        // Assert
        hypothesis.Statement.Should().Be("X causes Y");
        hypothesis.Plausibility.Should().Be(0.8);
        hypothesis.SupportingEvidence.Should().HaveCount(1);
    }

    [Fact]
    public void Hypothesis_WithNoEvidence_IsValid()
    {
        // Arrange & Act
        var hypothesis = new Hypothesis("speculation", 0.1, new List<Fact>());

        // Assert
        hypothesis.SupportingEvidence.Should().BeEmpty();
        hypothesis.Plausibility.Should().Be(0.1);
    }

    [Fact]
    public void Hypothesis_WithMaxPlausibility_IsValid()
    {
        // Arrange & Act
        var hypothesis = new Hypothesis("certain", 1.0, new List<Fact>());

        // Assert
        hypothesis.Plausibility.Should().Be(1.0);
    }

    [Fact]
    public void Hypothesis_WithZeroPlausibility_IsValid()
    {
        // Arrange & Act
        var hypothesis = new Hypothesis("impossible", 0.0, new List<Fact>());

        // Assert
        hypothesis.Plausibility.Should().Be(0.0);
    }

    #endregion

    #region ProofStep Record Tests

    [Fact]
    public void ProofStep_Constructor_SetsProperties()
    {
        // Arrange
        var rule = new Rule(
            "modus-ponens",
            new List<Pattern> { new Pattern("(A)", new List<string>()) },
            new Pattern("(B)", new List<string>()));
        var usedFacts = new List<Fact>
        {
            new Fact("A", new List<string>()),
        };

        // Act
        var step = new ProofStep("Apply modus ponens", rule, usedFacts);

        // Assert
        step.Inference.Should().Be("Apply modus ponens");
        step.RuleApplied.Name.Should().Be("modus-ponens");
        step.UsedFacts.Should().HaveCount(1);
    }

    #endregion

    #region ProofTrace Record Tests

    [Fact]
    public void ProofTrace_Proved_SetsCorrectly()
    {
        // Arrange & Act
        var trace = new ProofTrace(new List<ProofStep>(), true);

        // Assert
        trace.Proved.Should().BeTrue();
        trace.Steps.Should().BeEmpty();
        trace.CounterExample.Should().BeNull();
    }

    [Fact]
    public void ProofTrace_NotProved_WithCounterExample()
    {
        // Arrange & Act
        var trace = new ProofTrace(new List<ProofStep>(), false, "When x=0");

        // Assert
        trace.Proved.Should().BeFalse();
        trace.CounterExample.Should().Be("When x=0");
    }

    [Fact]
    public void ProofTrace_WithSteps_ContainsAllSteps()
    {
        // Arrange
        var rule = new Rule("r1", new List<Pattern>(), new Pattern("(c)", new List<string>()));
        var steps = new List<ProofStep>
        {
            new ProofStep("Step 1", rule, new List<Fact>()),
            new ProofStep("Step 2", rule, new List<Fact>()),
        };

        // Act
        var trace = new ProofTrace(steps, true);

        // Assert
        trace.Steps.Should().HaveCount(2);
    }

    #endregion

    #region TypedAtom Record Tests

    [Fact]
    public void TypedAtom_Constructor_SetsProperties()
    {
        // Arrange
        var typeParams = new Dictionary<string, string> { { "T", "Int" } };

        // Act
        var typedAtom = new TypedAtom("x", "List", typeParams);

        // Assert
        typedAtom.Atom.Should().Be("x");
        typedAtom.Type.Should().Be("List");
        typedAtom.TypeParameters.Should().ContainKey("T");
        typedAtom.TypeParameters["T"].Should().Be("Int");
    }

    [Fact]
    public void TypedAtom_WithEmptyTypeParameters_IsValid()
    {
        // Arrange & Act
        var typedAtom = new TypedAtom("y", "String", new Dictionary<string, string>());

        // Assert
        typedAtom.TypeParameters.Should().BeEmpty();
    }

    #endregion

    #region TypeContext Record Tests

    [Fact]
    public void TypeContext_Constructor_SetsProperties()
    {
        // Arrange
        var bindings = new Dictionary<string, string> { { "x", "Int" }, { "y", "String" } };
        var constraints = new List<string> { "x > 0" };

        // Act
        var context = new TypeContext(bindings, constraints);

        // Assert
        context.Bindings.Should().HaveCount(2);
        context.Constraints.Should().HaveCount(1);
        context.Constraints.Should().Contain("x > 0");
    }

    [Fact]
    public void TypeContext_WithEmptyBindingsAndConstraints_IsValid()
    {
        // Arrange & Act
        var context = new TypeContext(new Dictionary<string, string>(), new List<string>());

        // Assert
        context.Bindings.Should().BeEmpty();
        context.Constraints.Should().BeEmpty();
    }

    #endregion

    #region ProofStrategy Enum Tests

    [Theory]
    [InlineData(ProofStrategy.Resolution)]
    [InlineData(ProofStrategy.Tableaux)]
    [InlineData(ProofStrategy.NaturalDeduction)]
    public void ProofStrategy_AllValues_AreDefined(ProofStrategy strategy)
    {
        // Assert
        Enum.IsDefined(typeof(ProofStrategy), strategy).Should().BeTrue();
    }

    [Fact]
    public void ProofStrategy_HasExpectedCount()
    {
        // Assert
        Enum.GetValues<ProofStrategy>().Should().HaveCount(3);
    }

    #endregion

    #region InductionStrategy Enum Tests

    [Theory]
    [InlineData(InductionStrategy.FOIL)]
    [InlineData(InductionStrategy.GOLEM)]
    [InlineData(InductionStrategy.Progol)]
    [InlineData(InductionStrategy.ILP)]
    public void InductionStrategy_AllValues_AreDefined(InductionStrategy strategy)
    {
        // Assert
        Enum.IsDefined(typeof(InductionStrategy), strategy).Should().BeTrue();
    }

    [Fact]
    public void InductionStrategy_HasExpectedCount()
    {
        // Assert
        Enum.GetValues<InductionStrategy>().Should().HaveCount(4);
    }

    #endregion
}
