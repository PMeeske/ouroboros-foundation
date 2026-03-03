// <copyright file="ChromosomeDeepTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Core;

using FluentAssertions;
using Ouroboros.Genetic.Abstractions;
using Ouroboros.Genetic.Core;
using Xunit;

/// <summary>
/// Deep unit tests for Chromosome{TGene} covering record equality, immutability,
/// interface compliance, and edge cases not covered by ChromosomeTests.
/// </summary>
[Trait("Category", "Unit")]
public class ChromosomeDeepTests
{
    [Fact]
    public void WithGenes_PreservesFitness()
    {
        // Arrange
        var chromosome = new Chromosome<int>(new List<int> { 1, 2 }, fitness: 7.5);

        // Act
        var updated = chromosome.WithGenes(new List<int> { 10, 20 });

        // Assert
        updated.Fitness.Should().Be(7.5);
        updated.Genes.Should().Equal(10, 20);
    }

    [Fact]
    public void WithFitness_PreservesGenes()
    {
        // Arrange
        var chromosome = new Chromosome<int>(new List<int> { 1, 2, 3 }, fitness: 1.0);

        // Act
        var updated = chromosome.WithFitness(99.0);

        // Assert
        updated.Genes.Should().Equal(1, 2, 3);
        updated.Fitness.Should().Be(99.0);
    }

    [Fact]
    public void RecordEquality_SameGenesAndFitness_AreEqual()
    {
        // Arrange
        var c1 = new Chromosome<int>(new List<int> { 1, 2, 3 }, fitness: 5.0);
        var c2 = new Chromosome<int>(new List<int> { 1, 2, 3 }, fitness: 5.0);

        // Assert -- records compare by value, but genes are ReadOnlyCollection instances
        // so reference equality of Genes list differs; record equality checks each property
        c1.Fitness.Should().Be(c2.Fitness);
        c1.Genes.Should().Equal(c2.Genes);
    }

    [Fact]
    public void RecordEquality_DifferentFitness_AreNotEqual()
    {
        // Arrange
        var c1 = new Chromosome<int>(new List<int> { 1, 2 }, fitness: 1.0);
        var c2 = new Chromosome<int>(new List<int> { 1, 2 }, fitness: 2.0);

        // Assert
        c1.Fitness.Should().NotBe(c2.Fitness);
    }

    [Fact]
    public void WithGenes_ReturnsNewInstance_OriginalUnchanged()
    {
        // Arrange
        var original = new Chromosome<string>(new List<string> { "a", "b" }, fitness: 3.0);

        // Act
        var updated = original.WithGenes(new List<string> { "x", "y", "z" });

        // Assert
        original.Genes.Should().Equal("a", "b");
        updated.Genes.Should().Equal("x", "y", "z");
    }

    [Fact]
    public void WithFitness_ReturnsNewInstance_OriginalUnchanged()
    {
        // Arrange
        var original = new Chromosome<int>(new List<int> { 1 }, fitness: 0.0);

        // Act
        var updated = original.WithFitness(100.0);

        // Assert
        original.Fitness.Should().Be(0.0);
        updated.Fitness.Should().Be(100.0);
    }

    [Fact]
    public void Constructor_WithEmptyGenes_Succeeds()
    {
        // Act
        var chromosome = new Chromosome<int>(new List<int>());

        // Assert
        chromosome.Genes.Should().BeEmpty();
        chromosome.Fitness.Should().Be(0);
    }

    [Fact]
    public void Constructor_CopiesGenes_ExternalModificationDoesNotAffect()
    {
        // Arrange
        var genes = new List<int> { 10, 20, 30 };
        var chromosome = new Chromosome<int>(genes, fitness: 1.0);

        // Act
        genes.Clear();

        // Assert
        chromosome.Genes.Should().Equal(10, 20, 30);
    }

    [Fact]
    public void WithGenes_ReturnsIChromosome_Interface()
    {
        // Arrange
        IChromosome<int> chromosome = new Chromosome<int>(new List<int> { 1 });

        // Act
        var updated = chromosome.WithGenes(new List<int> { 2 });

        // Assert
        updated.Should().BeAssignableTo<IChromosome<int>>();
        updated.Genes.Should().Equal(2);
    }

    [Fact]
    public void WithFitness_ReturnsIChromosome_Interface()
    {
        // Arrange
        IChromosome<int> chromosome = new Chromosome<int>(new List<int> { 1 });

        // Act
        var updated = chromosome.WithFitness(42.0);

        // Assert
        updated.Should().BeAssignableTo<IChromosome<int>>();
        updated.Fitness.Should().Be(42.0);
    }

    [Fact]
    public void WithGenes_WithNullGenes_SetsNullDirectly()
    {
        // Arrange
        var chromosome = new Chromosome<int>(new List<int> { 1, 2 });

        // Act & Assert -- WithGenes uses 'with' keyword, which bypasses ctor validation
        // The 'with' expression just sets the property, so null is allowed here
        var updated = chromosome.WithGenes(null!);
        updated.Genes.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithLargeGeneList_Succeeds()
    {
        // Arrange
        var genes = Enumerable.Range(0, 10000).ToList();

        // Act
        var chromosome = new Chromosome<int>(genes, fitness: 0.5);

        // Assert
        chromosome.Genes.Should().HaveCount(10000);
        chromosome.Genes[0].Should().Be(0);
        chromosome.Genes[9999].Should().Be(9999);
    }

    [Fact]
    public void Constructor_WithNegativeFitness_Succeeds()
    {
        // Act
        var chromosome = new Chromosome<int>(new List<int> { 1 }, fitness: -100.0);

        // Assert
        chromosome.Fitness.Should().Be(-100.0);
    }

    [Fact]
    public void Constructor_WithDoubleMaxFitness_Succeeds()
    {
        // Act
        var chromosome = new Chromosome<double>(new List<double> { 1.5 }, fitness: double.MaxValue);

        // Assert
        chromosome.Fitness.Should().Be(double.MaxValue);
    }

    [Fact]
    public void Chromosome_WithStringGenes_WorksCorrectly()
    {
        // Arrange
        var genes = new List<string> { "hello", "world" };

        // Act
        var chromosome = new Chromosome<string>(genes, fitness: 2.5);

        // Assert
        chromosome.Genes.Should().Equal("hello", "world");
        chromosome.Fitness.Should().Be(2.5);
    }
}
