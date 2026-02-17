namespace Ouroboros.Core.Interop;

/// <summary>
/// Fluent pipeline builder for enhanced composition
/// </summary>
public class PipelineBuilder<TIn>
{
    private readonly string _name;

    public PipelineBuilder(string name)
    {
        _name = name;
    }

    /// <summary>
    /// Add a Step to the pipeline
    /// </summary>
    public PipelineBuilder<TIn, TOut> AddStep<TOut>(Step<TIn, TOut> step, string? stepName = null)
        => new(_name, step.ToCompatNode(stepName));

    /// <summary>
    /// Add a KleisliResult to the pipeline
    /// </summary>
    public PipelineBuilder<TIn, Result<TOut, TError>> AddResultStep<TOut, TError>(
        KleisliResult<TIn, TOut, TError> kleisliResult,
        string? stepName = null)
        => new(_name, kleisliResult.ToCompatNode(stepName));

    /// <summary>
    /// Add a function to the pipeline
    /// </summary>
    public PipelineBuilder<TIn, TOut> AddFunc<TOut>(Func<TIn, TOut> func, string? stepName = null)
        => new(_name, func.ToCompatNode(stepName));
}