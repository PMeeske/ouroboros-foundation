using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.Steps;

/// <summary>
/// Custom exception thrown when a step execution fails.
/// Provides detailed context about the step failure including the step type,
/// input parameters, and underlying exception.
/// </summary>
[Serializable]
[SuppressMessage("Design", "CA1032:Implement standard exception constructors")]
public class StepExecutionException : Exception
{
    /// <summary>
    /// Gets the type of the step that failed.
    /// </summary>
    public Type StepType { get; }

    /// <summary>
    /// Gets the input value that was being processed when the failure occurred.
    /// This may be null if the input could not be captured or was not available.
    /// </summary>
    public object? InputValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StepExecutionException"/> class.
    /// </summary>
    /// <param name="stepType">The type of the step that failed.</param>
    /// <param name="inputValue">The input value being processed when the failure occurred.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public StepExecutionException(Type stepType, object? inputValue, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        StepType = stepType ?? throw new ArgumentNullException(nameof(stepType));
        InputValue = inputValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StepExecutionException"/> class
    /// with a default message format.
    /// </summary>
    /// <param name="stepType">The type of the step that failed.</param>
    /// <param name="inputValue">The input value being processed when the failure occurred.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public StepExecutionException(Type stepType, object? inputValue, Exception? innerException = null)
        : this(stepType, inputValue, $"Step execution failed for step of type '{stepType.Name}' with input: {inputValue}", innerException)
    {
    }

    /// <summary>
    /// Gets a message that describes the current exception.
    /// </summary>
    public override string Message
    {
        get
        {
            var baseMessage = base.Message;
            return $"StepExecutionException in {StepType.Name}: {baseMessage}\nInput: {InputValue}";
        }
    }
}