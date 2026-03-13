// <copyright file="TestExample.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Benchmarks;

/// <summary>
/// Represents a test example with input, expected output, and validation function.
/// </summary>
/// <param name="Input">The input to be processed.</param>
/// <param name="ExpectedOutput">The expected output for validation.</param>
/// <param name="Validator">Function to validate the actual output against the expected output.</param>
[ExcludeFromCodeCoverage]
public sealed record TestExample(
    string Input,
    string ExpectedOutput,
    Func<string, string, bool> Validator);
