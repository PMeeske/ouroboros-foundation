namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Tool for applying MeTTa inference rules.
/// </summary>
public sealed class MeTTaRuleTool : ITool
{
    private readonly IMeTTaEngine engine;

    /// <inheritdoc />
    public string Name => "metta_rule";

    /// <inheritdoc />
    public string Description => "Apply a MeTTa inference rule to derive new knowledge from existing facts.";

    /// <inheritdoc />
    public string? JsonSchema => @"{
        ""type"": ""object"",
        ""properties"": {
            ""rule"": {
                ""type"": ""string"",
                ""description"": ""The MeTTa rule expression to apply""
            }
        },
        ""required"": [""rule""]
    }";

    /// <summary>
    /// Initializes a new instance of the <see cref="MeTTaRuleTool"/> class.
    /// Creates a new MeTTa rule tool.
    /// </summary>
    /// <param name="engine">The MeTTa engine to use for rule application.</param>
    public MeTTaRuleTool(IMeTTaEngine engine)
    {
        this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    /// <inheritdoc />
    public async Task<Result<string, string>> InvokeAsync(string input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<string, string>.Failure("Rule cannot be empty");
        }

        // Parse JSON if provided
        string rule;
        try
        {
            System.Text.Json.JsonDocument json = System.Text.Json.JsonDocument.Parse(input);
            if (json.RootElement.TryGetProperty("rule", out System.Text.Json.JsonElement ruleProp))
            {
                rule = ruleProp.GetString() ?? input;
            }
            else
            {
                rule = input;
            }
        }
        catch
        {
            rule = input;
        }

        return await this.engine.ApplyRuleAsync(rule, ct);
    }
}