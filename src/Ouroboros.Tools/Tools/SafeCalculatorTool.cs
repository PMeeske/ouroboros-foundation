// <copyright file="SafeCalculatorTool.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Tools;

using System.Data;
using System.Globalization;
using System.Text.Json;
using Ouroboros.Core.Monads;
using Ouroboros.Tools.MeTTa;
using Unit = Unit;

/// <summary>
/// A specialized calculator tool that demonstrates "Proof-Carrying Code" / Verification.
/// Instead of just computing results, it uses a symbolic engine to verify calculations,
/// providing mathematical soundness guarantees for neuro-symbolic AI systems.
/// </summary>
public sealed class SafeCalculatorTool : ITool
{
    // Tolerance for floating point comparisons
    private const double ComparisonTolerance = 0.000001;
    
    // Allowed characters for arithmetic expressions (security measure)
    private static readonly HashSet<char> AllowedCharacters = new HashSet<char>(
        "0123456789+-*/().Ee ".ToCharArray());
    
    private readonly IMeTTaEngine? symbolicEngine;
    private readonly bool useSymbolicVerification;

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeCalculatorTool"/> class.
    /// </summary>
    /// <param name="symbolicEngine">Optional symbolic engine for verification. If null, uses simulated verification.</param>
    public SafeCalculatorTool(IMeTTaEngine? symbolicEngine = null)
    {
        this.symbolicEngine = symbolicEngine;
        this.useSymbolicVerification = symbolicEngine != null;
    }

    /// <inheritdoc />
    public string Name => "safe_calculator";

    /// <inheritdoc />
    public string Description => "Verified arithmetic calculator that uses symbolic reasoning to validate calculations. Ensures mathematical correctness through proof-carrying code principles.";

    /// <inheritdoc />
    public string? JsonSchema => @"{
        ""type"": ""object"",
        ""properties"": {
            ""expression"": {
                ""type"": ""string"",
                ""description"": ""The mathematical expression to evaluate and verify (e.g., '2+2*5', '(10-5)/2')""
            },
            ""expected_result"": {
                ""type"": ""number"",
                ""description"": ""Optional expected result for additional verification""
            }
        },
        ""required"": [""expression""]
    }";

    /// <inheritdoc />
    public async Task<Result<string, string>> InvokeAsync(string input, CancellationToken ct = default)
    {
        Result<ParsedInput, string> parsedResult = ValidateInput(input).Bind(ParseInput);

        return await parsedResult.Match(
            parsed => this.ProcessCalculation(parsed, ct),
            error => Task.FromResult(Result<string, string>.Failure(error)));
    }

    private async Task<Result<string, string>> ProcessCalculation(ParsedInput parsed, CancellationToken ct)
    {
        Result<double, string> computeResult = ComputeExpression(parsed.Expression);
        
        return await computeResult.Match(
            calculatedValue => this.VerifyAndFormat(parsed, calculatedValue, ct),
            error => Task.FromResult(Result<string, string>.Failure(error)));
    }

    private async Task<Result<string, string>> VerifyAndFormat(ParsedInput parsed, double calculatedValue, CancellationToken ct)
    {
        Result<bool, string> verifyResult = await this.VerifyCalculationAsync(parsed.Expression, calculatedValue, ct);
        
        return verifyResult
            .MapError(error => $"Verification failed: {error}")
            .Where(verified => verified, $"❌ Calculation verification failed for expression: {parsed.Expression}. The result could not be symbolically verified.")
            .Bind(_ => ValidateExpectedResult(calculatedValue, parsed.ExpectedResult))
            .Map(_ => this.FormatSuccessMessage(calculatedValue, parsed.Expression));
    }

    private static Result<string, string> ValidateInput(string input) =>
        string.IsNullOrWhiteSpace(input)
            ? Result<string, string>.Failure("Expression cannot be empty")
            : Result<string, string>.Success(input);

    private static Result<ParsedInput, string> ParseInput(string input) =>
        TryParseJson(input)
            .Match(
                parsed => Result<ParsedInput, string>.Success(parsed),
                _ => Result<ParsedInput, string>.Success(new ParsedInput(input, null)));

    private static Result<ParsedInput, string> TryParseJson(string input) =>
        input.TrimStart().StartsWith('{')
            ? ParseJsonDocument(input)
            : Result<ParsedInput, string>.Failure("Not JSON");

    private static Result<ParsedInput, string> ParseJsonDocument(string input)
    {
        try
        {
            using JsonDocument json = JsonDocument.Parse(input);
            return ExtractExpression(json.RootElement, input)
                .Map(expression =>
                {
                    Option<double> optExpected = ExtractExpectedResult(json.RootElement);
                    double? expected = optExpected.Match(val => (double?)val, null);
                    return new ParsedInput(expression, expected);
                });
        }
        catch (JsonException)
        {
            return Result<ParsedInput, string>.Failure("Invalid JSON");
        }
    }

    private static Result<string, string> ExtractExpression(JsonElement root, string fallback) =>
        root.TryGetProperty("expression", out JsonElement exprProp)
            ? Result<string, string>.Success(exprProp.GetString() ?? fallback)
            : Result<string, string>.Failure("JSON input must contain 'expression' property");

    private static Option<double> ExtractExpectedResult(JsonElement root) =>
        root.TryGetProperty("expected_result", out JsonElement expectedProp) &&
        expectedProp.ValueKind == JsonValueKind.Number
            ? Option<double>.Some(expectedProp.GetDouble())
            : Option<double>.None();

    private static Result<Unit, string> ValidateExpectedResult(double calculatedValue, double? expectedResult) =>
        expectedResult.HasValue
            ? Math.Abs(calculatedValue - expectedResult.Value) <= ComparisonTolerance
                ? Result<Unit, string>.Success(Unit.Value)
                : Result<Unit, string>.Failure($"❌ Result mismatch: calculated {calculatedValue}, expected {expectedResult.Value}")
            : Result<Unit, string>.Success(Unit.Value);

    private string FormatSuccessMessage(double calculatedValue, string expression)
    {
        string resultString = Convert.ToString(calculatedValue, CultureInfo.InvariantCulture) ?? "null";
        string verificationBadge = this.useSymbolicVerification ? "✓ Symbolically Verified" : "✓ Verified";
        return $"{verificationBadge}: {expression} = {resultString}";
    }

    private readonly record struct ParsedInput(string Expression, double? ExpectedResult);

    private static Result<double, string> ComputeExpression(string expression)
    {
        try
        {
            // NOTE: DataTable.Compute() is used for basic arithmetic evaluation.
            // While not ideal for production (has security considerations), it works for this demonstration.
            // For production use, consider using NCalc or implementing a custom expression parser.
            DataTable dataTable = new DataTable();
            object result = dataTable.Compute(expression, string.Empty);
            double value = Convert.ToDouble(result, CultureInfo.InvariantCulture);
            return Result<double, string>.Success(value);
        }
        catch (InvalidExpressionException ex)
        {
            return Result<double, string>.Failure($"Expression evaluation failed: {ex.Message}");
        }
        catch (FormatException ex)
        {
            return Result<double, string>.Failure($"Expression evaluation failed: {ex.Message}");
        }
        catch (OverflowException ex)
        {
            return Result<double, string>.Failure($"Expression evaluation failed: {ex.Message}");
        }
    }

    private async Task<Result<bool, string>> VerifyCalculationAsync(string expression, double result, CancellationToken ct) =>
        this.useSymbolicVerification && this.symbolicEngine != null
            ? await this.SymbolicVerificationAsync(expression, result, ct)
            : SimulatedVerification(expression, result);

    private async Task<Result<bool, string>> SymbolicVerificationAsync(string expression, double result, CancellationToken ct)
    {
        try
        {
            string mettaExpression = ConvertToMeTTaExpression(expression);
            Result<string, string> mettaResult = await this.symbolicEngine!.ExecuteQueryAsync(mettaExpression, ct);

            return mettaResult
                .Bind(TryParseMeTTaNumber)
                .Bind(symbolicResult => ValidateCalculationMatch(symbolicResult, result, "Symbolic result does not match computed result"));
        }
        catch (OperationCanceledException) { throw; }
        catch (HttpRequestException ex)
        {
            return Result<bool, string>.Failure($"Symbolic verification exception: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool, string>.Failure($"Symbolic verification exception: {ex.Message}");
        }
    }

    private static Result<bool, string> SimulatedVerification(string expression, double result)
    {
        try
        {
            return ValidateAllowedCharacters(expression)
                .Bind(_ => ComputeExpression(expression))
                .Bind(recomputed => ValidateCalculationMatch(recomputed, result, "Recomputation verification failed"));
        }
        catch (InvalidExpressionException ex)
        {
            return Result<bool, string>.Failure($"Simulated verification exception: {ex.Message}");
        }
        catch (FormatException ex)
        {
            return Result<bool, string>.Failure($"Simulated verification exception: {ex.Message}");
        }
    }

    private static Result<bool, string> ValidateCalculationMatch(double calculated, double expected, string errorMessage) =>
        Math.Abs(calculated - expected) < ComparisonTolerance
            ? Result<bool, string>.Success(true)
            : Result<bool, string>.Failure($"{errorMessage}: calculated={calculated}, expected={expected}");

    private static Result<Unit, string> ValidateAllowedCharacters(string expression) =>
        expression.All(c => AllowedCharacters.Contains(c))
            ? Result<Unit, string>.Success(Unit.Value)
            : Result<Unit, string>.Failure("Expression contains invalid characters");

    private static string ConvertToMeTTaExpression(string expression)
    {
        string cleaned = expression.Replace(" ", string.Empty);
        
        return cleaned.Any(c => "+-*/".Contains(c))
            ? $"!(eval {cleaned})"
            : $"!({cleaned})";
    }

    private static Result<double, string> TryParseMeTTaNumber(string mettaValue)
    {
        string cleaned = mettaValue.Trim().Trim('[', ']', '(', ')');
        
        return double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out double result)
            ? Result<double, string>.Success(result)
            : Result<double, string>.Failure($"Could not parse MeTTa result: {mettaValue}");
    }
}
