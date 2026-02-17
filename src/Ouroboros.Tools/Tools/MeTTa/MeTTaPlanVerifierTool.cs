namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Tool for verifying plans using symbolic reasoning.
/// </summary>
public sealed class MeTTaPlanVerifierTool : ITool
{
    private readonly IMeTTaEngine engine;

    /// <inheritdoc />
    public string Name => "metta_verify_plan";

    /// <inheritdoc />
    public string Description => "Verify a plan using MeTTa symbolic reasoning. Returns true/false with reasoning.";

    /// <inheritdoc />
    public string? JsonSchema => @"{
        ""type"": ""object"",
        ""properties"": {
            ""plan"": {
                ""type"": ""string"",
                ""description"": ""The plan to verify in MeTTa format""
            }
        },
        ""required"": [""plan""]
    }";

    /// <summary>
    /// Initializes a new instance of the <see cref="MeTTaPlanVerifierTool"/> class.
    /// Creates a new MeTTa plan verifier tool.
    /// </summary>
    /// <param name="engine">The MeTTa engine to use for plan verification.</param>
    public MeTTaPlanVerifierTool(IMeTTaEngine engine)
    {
        this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    /// <inheritdoc />
    public async Task<Result<string, string>> InvokeAsync(string input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<string, string>.Failure("Plan cannot be empty");
        }

        // Parse JSON if provided
        string plan;
        try
        {
            System.Text.Json.JsonDocument json = System.Text.Json.JsonDocument.Parse(input);
            if (json.RootElement.TryGetProperty("plan", out System.Text.Json.JsonElement planProp))
            {
                plan = planProp.GetString() ?? input;
            }
            else
            {
                plan = input;
            }
        }
        catch
        {
            plan = input;
        }

        Result<bool, string> result = await this.engine.VerifyPlanAsync(plan, ct);

        return result.Match(
            isValid => Result<string, string>.Success(isValid ? "Plan is valid" : "Plan is invalid"),
            error => Result<string, string>.Failure(error));
    }
}