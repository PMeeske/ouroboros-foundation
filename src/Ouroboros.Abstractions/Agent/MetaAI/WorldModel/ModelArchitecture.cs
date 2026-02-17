namespace Ouroboros.Agent.MetaAI.WorldModel;

/// <summary>
/// Supported model architectures for world model learning.
/// </summary>
public enum ModelArchitecture
{
    /// <summary>Multi-layer perceptron (simple feed-forward network).</summary>
    MLP,

    /// <summary>Transformer-based architecture with attention mechanisms.</summary>
    Transformer,

    /// <summary>Graph neural network for structured state spaces.</summary>
    GNN,

    /// <summary>Hybrid architecture combining multiple approaches.</summary>
    Hybrid,
}