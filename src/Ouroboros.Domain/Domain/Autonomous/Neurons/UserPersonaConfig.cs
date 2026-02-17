namespace Ouroboros.Domain.Autonomous.Neurons;

/// <summary>
/// Configuration for a simulated user persona used in auto-training.
/// </summary>
public sealed record UserPersonaConfig
{
    /// <summary>Name of the simulated user.</summary>
    public string Name { get; init; } = "AutoUser";

    /// <summary>Personality traits that influence question generation.</summary>
    public List<string> Traits { get; init; } = ["curious", "thoughtful", "creative", "analytical"];

    /// <summary>Topics the user is interested in - diverse range beyond just development.</summary>
    public List<string> Interests { get; init; } = [
        // Technology
        "artificial intelligence", "machine learning", "quantum computing", "cybersecurity",
        // Science
        "astronomy", "biology", "physics", "climate science", "neuroscience",
        // Philosophy & Culture
        "philosophy of mind", "ethics", "history", "art", "music theory",
        // Practical
        "productivity", "health", "cooking", "travel", "economics",
        // Creative
        "creative writing", "storytelling", "game design", "architecture"
    ];

    /// <summary>Skill level: beginner, intermediate, expert.</summary>
    public string SkillLevel { get; init; } = "intermediate";

    /// <summary>Communication style: formal, casual, terse, verbose.</summary>
    public string CommunicationStyle { get; init; } = "casual";

    /// <summary>How often to ask follow-up questions (0-1).</summary>
    public double FollowUpProbability { get; init; } = 0.6;

    /// <summary>How often to challenge or question responses (0-1).</summary>
    public double ChallengeProbability { get; init; } = 0.2;

    /// <summary>Interval between automatic messages in seconds.</summary>
    public int MessageIntervalSeconds { get; init; } = 30;

    /// <summary>Maximum training sessions before pausing.</summary>
    public int MaxSessionMessages { get; init; } = 50;

    /// <summary>Self-dialogue mode: Ouroboros talks to itself (two Ouroboros personas debate).</summary>
    public bool SelfDialogueMode { get; init; } = false;

    /// <summary>Name for the second Ouroboros persona in self-dialogue mode.</summary>
    public string SecondPersonaName { get; init; } = "Ouroboros-B";

    /// <summary>Traits for the second persona (contrasting viewpoint).</summary>
    public List<string> SecondPersonaTraits { get; init; } = ["skeptical", "pragmatic", "devil's advocate", "grounded"];

    /// <summary>Problem-solving mode: work together to solve a specific problem.</summary>
    public bool ProblemSolvingMode { get; init; } = false;

    /// <summary>The problem to solve (required when ProblemSolvingMode=true).</summary>
    public string? Problem { get; init; }

    /// <summary>Expected deliverable type: code, plan, analysis, design, document.</summary>
    public string DeliverableType { get; init; } = "plan";

    /// <summary>Whether to use tools during problem solving.</summary>
    public bool UseTools { get; init; } = true;

    /// <summary>YOLO mode: auto-approve all actions, no human confirmation needed.</summary>
    public bool YoloMode { get; init; } = false;
}