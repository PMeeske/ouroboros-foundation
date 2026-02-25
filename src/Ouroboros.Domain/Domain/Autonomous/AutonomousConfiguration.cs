namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Configuration for autonomous behavior.
/// </summary>
public sealed record AutonomousConfiguration
{
    /// <summary>Culture code for localization (e.g., "de-DE", "en-US").</summary>
    public string? Culture { get; init; }

    /// <summary>Whether push-based mode is enabled (vs reactive).</summary>
    public bool PushBasedMode { get; init; } = true;

    /// <summary>YOLO mode: auto-approve ALL intentions without prompting. Use with caution!</summary>
    public bool YoloMode { get; init; } = false;

    /// <summary>Whether to auto-approve low-risk intentions.</summary>
    public bool AutoApproveLowRisk { get; init; } = false;

    /// <summary>Whether to auto-approve self-reflection intentions.</summary>
    public bool AutoApproveSelfReflection { get; init; } = true;

    /// <summary>Whether to auto-approve memory operations.</summary>
    public bool AutoApproveMemoryOps { get; init; } = true;

    /// <summary>Interval in seconds between autonomous ticks.</summary>
    public int TickIntervalSeconds { get; init; } = 30;

    /// <summary>Maximum pending intentions before throttling.</summary>
    public int MaxPendingIntentions { get; init; } = 20;

    /// <summary>Whether to enable proactive communication.</summary>
    public bool EnableProactiveCommunication { get; init; } = true;

    /// <summary>Whether to enable code self-modification proposals.</summary>
    public bool EnableCodeModification { get; init; } = true;

    /// <summary>Intention expiry time in minutes (0 = never).</summary>
    public int IntentionExpiryMinutes { get; init; } = 60;

    /// <summary>Categories that always require explicit approval.</summary>
    public HashSet<IntentionCategory> AlwaysRequireApproval { get; init; } =
    [
        IntentionCategory.CodeModification,
        IntentionCategory.GoalPursuit,
    ];

    /// <summary>
    /// Priority-ordered list of tools for research/learning.
    /// First available tool in the list will be used.
    /// </summary>
    public List<string> ResearchToolPriority { get; init; } =
    [
        "web_research",    // Deep web research with Firecrawl
        "firecrawl_scrape",      // Single page scrape with Firecrawl
        "web_search",            // DuckDuckGo search
        "duckduckgo_search",     // Alias for web search
    ];

    /// <summary>
    /// Priority-ordered list of tools for code analysis.
    /// </summary>
    public List<string> CodeToolPriority { get; init; } =
    [
        "code_analyze",
        "code_search",
        "file_read",
    ];

    /// <summary>
    /// Priority-ordered list of tools for general queries.
    /// </summary>
    public List<string> GeneralToolPriority { get; init; } =
    [
        "web_research",
        "web_search",
        "recall",
    ];
}