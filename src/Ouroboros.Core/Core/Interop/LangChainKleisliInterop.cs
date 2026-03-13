// ==========================================================
// Kleisli <-> LangChain pipe interop
// Enhanced integration with our unified monadic operations
// ==========================================================

namespace Ouroboros.Core.Interop;

using Ouroboros.Core.Steps;

#region Core Integration - Minimal Interop Extensions

#endregion

#region Compatible Pipeline System

#endregion

#region Conversion Extensions

#endregion

#region Fluent Pipeline Builder

/// <summary>
/// Typed pipeline builder for fluent composition
/// </summary>
public class PipelineBuilder<TIn, TCurrent>
{
    private readonly string _name;
    private readonly PipeNode<TIn, TCurrent> _currentPipeline;

    /// <summary>Initialises the builder with a pipeline name and the initial pipe node.</summary>
    /// <param name="name">Display name for the pipeline.</param>
    /// <param name="pipeline">The initial pipe node to build from.</param>
    public PipelineBuilder(string name, PipeNode<TIn, TCurrent> pipeline)
    {
        _name = name;
        _currentPipeline = pipeline;
    }

    /// <summary>
    /// Add another step to the pipeline
    /// </summary>
    public PipelineBuilder<TIn, TOut> Then<TOut>(Step<TCurrent, TOut> step, string? stepName = null)
        => new(_name, _currentPipeline.Pipe(step.ToCompatNode(stepName)));

    /// <summary>
    /// Add a function step to the pipeline
    /// </summary>
    public PipelineBuilder<TIn, TOut> Then<TOut>(Func<TCurrent, TOut> func, string? stepName = null)
        => new(_name, _currentPipeline.Pipe(func.ToCompatNode(stepName)));

    /// <summary>
    /// Build the final pipeline
    /// </summary>
    public PipeNode<TIn, TCurrent> Build() => _currentPipeline;

    /// <summary>
    /// Build and execute the pipeline
    /// </summary>
    public async Task<TCurrent> ExecuteAsync(TIn input)
        => await (input | _currentPipeline).ConfigureAwait(false);
}

#endregion

#region Enhanced Examples with Monadic Operations

#endregion
