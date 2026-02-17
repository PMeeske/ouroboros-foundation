namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Categories of autonomous intentions.
/// </summary>
public enum IntentionCategory
{
    /// <summary>Self-reflection and introspection.</summary>
    SelfReflection,

    /// <summary>Code modification or improvement.</summary>
    CodeModification,

    /// <summary>Learning from experience.</summary>
    Learning,

    /// <summary>Communication with user.</summary>
    UserCommunication,

    /// <summary>Memory management and consolidation.</summary>
    MemoryManagement,

    /// <summary>Inter-neuron communication.</summary>
    NeuronCommunication,

    /// <summary>Goal pursuit and task execution.</summary>
    GoalPursuit,

    /// <summary>Safety and health checks.</summary>
    SafetyCheck,

    /// <summary>Exploration and curiosity.</summary>
    Exploration,
}