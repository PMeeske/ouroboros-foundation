namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Types of neurons in the Ouroboros neural network.
/// </summary>
public enum NeuronType
{
    /// <summary>The neuron responsible for processing incoming messages and executing associated logic.</summary>
    Processor,

    /// <summary>The neuron responsible for aggregating data from multiple sources and synthesizing it into a unified representation.</summary>
    Aggregator,

    /// <summary>The neuron responsible for monitoring and observing system states, events, or changes in the environment.</summary>
    Observer,

    /// <summary>The neuron responsible for generating responses to incoming messages or requests.</summary>
    Responder,

    /// <summary>The core reasoning neuron.</summary>
    Core,

    /// <summary>Handles memory operations.</summary>
    Memory,

    /// <summary>Manages code reflection and modification.</summary>
    CodeReflection,

    /// <summary>Handles MeTTa symbolic reasoning.</summary>
    Symbolic,

    /// <summary>Manages user interaction.</summary>
    Communication,

    /// <summary>Handles safety and ethics.</summary>
    Safety,

    /// <summary>Manages emotional state.</summary>
    Affect,

    /// <summary>Handles goal and task management.</summary>
    Executive,

    /// <summary>Specialized for learning from experience.</summary>
    Learning,

    /// <summary>Simulates user behavior for training.</summary>
    Cognitive,

    /// <summary>Custom/plugin neuron.</summary>
    Custom,
}