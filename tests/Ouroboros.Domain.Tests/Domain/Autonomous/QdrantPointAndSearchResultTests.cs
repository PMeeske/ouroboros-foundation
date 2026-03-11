// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Autonomous;

using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for QdrantPoint and QdrantSearchResult records.
/// These are internal records, tested via reflection.
/// </summary>
[Trait("Category", "Unit")]
public class QdrantPointAndSearchResultTests
{
    // ----------------------------------------------------------------
    // QdrantPoint — construction and properties
    // ----------------------------------------------------------------

    [Fact]
    public void QdrantPoint_CanBeConstructed()
    {
        // The type is internal, so we use reflection
        Type? qdrantPointType = typeof(Ouroboros.Domain.Autonomous.QdrantNeuralMemory).Assembly
            .GetType("Ouroboros.Domain.Autonomous.QdrantPoint");
        qdrantPointType.Should().NotBeNull("QdrantPoint should exist as an internal type");

        // Verify it has required properties
        var idProp = qdrantPointType!.GetProperty("Id");
        var vectorProp = qdrantPointType.GetProperty("Vector");
        var payloadProp = qdrantPointType.GetProperty("Payload");

        idProp.Should().NotBeNull();
        vectorProp.Should().NotBeNull();
        payloadProp.Should().NotBeNull();

        idProp!.PropertyType.Should().Be(typeof(string));
        vectorProp!.PropertyType.Should().Be(typeof(float[]));
        payloadProp!.PropertyType.Should().Be(typeof(Dictionary<string, object>));
    }

    [Fact]
    public void QdrantPoint_IsRecordType()
    {
        Type? qdrantPointType = typeof(Ouroboros.Domain.Autonomous.QdrantNeuralMemory).Assembly
            .GetType("Ouroboros.Domain.Autonomous.QdrantPoint");
        qdrantPointType.Should().NotBeNull();

        // Record types implement IEquatable<T> and have an EqualityContract
        qdrantPointType!.IsSealed.Should().BeTrue();
        qdrantPointType.GetMethod("<Clone>$").Should().NotBeNull("records have a Clone method");
    }

    // ----------------------------------------------------------------
    // QdrantSearchResult — construction and properties
    // ----------------------------------------------------------------

    [Fact]
    public void QdrantSearchResult_CanBeConstructed()
    {
        Type? searchResultType = typeof(Ouroboros.Domain.Autonomous.QdrantNeuralMemory).Assembly
            .GetType("Ouroboros.Domain.Autonomous.QdrantSearchResult");
        searchResultType.Should().NotBeNull("QdrantSearchResult should exist as an internal type");

        var idProp = searchResultType!.GetProperty("Id");
        var scoreProp = searchResultType.GetProperty("Score");
        var payloadProp = searchResultType.GetProperty("Payload");

        idProp.Should().NotBeNull();
        scoreProp.Should().NotBeNull();
        payloadProp.Should().NotBeNull();

        idProp!.PropertyType.Should().Be(typeof(string));
        scoreProp!.PropertyType.Should().Be(typeof(double));
        payloadProp!.PropertyType.Should().Be(typeof(Dictionary<string, object>));
    }

    [Fact]
    public void QdrantSearchResult_IsRecordType()
    {
        Type? searchResultType = typeof(Ouroboros.Domain.Autonomous.QdrantNeuralMemory).Assembly
            .GetType("Ouroboros.Domain.Autonomous.QdrantSearchResult");
        searchResultType.Should().NotBeNull();

        searchResultType!.IsSealed.Should().BeTrue();
        searchResultType.GetMethod("<Clone>$").Should().NotBeNull("records have a Clone method");
    }

    [Fact]
    public void QdrantSearchResult_HasPositionalConstructor()
    {
        Type? searchResultType = typeof(Ouroboros.Domain.Autonomous.QdrantNeuralMemory).Assembly
            .GetType("Ouroboros.Domain.Autonomous.QdrantSearchResult");
        searchResultType.Should().NotBeNull();

        // Positional records have a constructor with all parameters
        var constructors = searchResultType!.GetConstructors();
        constructors.Should().Contain(c => c.GetParameters().Length == 3,
            "QdrantSearchResult should have a 3-parameter positional constructor");
    }
}
