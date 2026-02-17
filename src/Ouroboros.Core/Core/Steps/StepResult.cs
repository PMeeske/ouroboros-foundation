using System.Runtime.CompilerServices;

namespace Ouroboros.Core.Steps;

/// <summary>
/// Represents the result of a step execution attempt, containing either a successful result
/// or detailed error information.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public readonly struct StepResult<T>
{
    private readonly T? _value;
    private readonly string? _errorMessage;
    private readonly Exception? _exception;

    /// <summary>
    /// Initializes a new instance of the <see cref="StepResult{T}"/> struct representing a successful result.
    /// </summary>
    /// <param name="value">The successful result value.</param>
    public StepResult(T value)
    {
        _value = value;
        _errorMessage = null;
        _exception = null;
        IsSuccess = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StepResult{T}"/> struct representing a failed result.
    /// </summary>
    /// <param name="errorMessage">A descriptive error message.</param>
    /// <param name="exception">The exception that caused the failure, if any.</param>
    public StepResult(string errorMessage, Exception? exception = null)
    {
        _value = default;
        _errorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
        _exception = exception;
        IsSuccess = false;
    }

    /// <summary>
    /// Gets a value indicating whether the step execution was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the result value if the execution was successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result represents a failure.</exception>
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access Value of a failed result.");

    /// <summary>
    /// Gets the error message if the execution failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result represents a success.</exception>
    public string ErrorMessage => !IsSuccess ? _errorMessage! : throw new InvalidOperationException("Cannot access ErrorMessage of a successful result.");

    /// <summary>
    /// Gets the exception that caused the failure, if any.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result represents a success.</exception>
    public Exception? Exception => !IsSuccess ? _exception : throw new InvalidOperationException("Cannot access Exception of a successful result.");

    /// <summary>
    /// Creates a successful <see cref="StepResult{T}"/> with the specified value.
    /// </summary>
    /// <param name="value">The successful result value.</param>
    /// <returns>A successful step result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StepResult<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed <see cref="StepResult{T}"/> with the specified error message.
    /// </summary>
    /// <param name="errorMessage">A descriptive error message.</param>
    /// <returns>A failed step result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StepResult<T> Failure(string errorMessage) => new(errorMessage);

    /// <summary>
    /// Creates a failed <see cref="StepResult{T}"/> with the specified error message and exception.
    /// </summary>
    /// <param name="errorMessage">A descriptive error message.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <returns>A failed step result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StepResult<T> Failure(string errorMessage, Exception exception) => new(errorMessage, exception);

    /// <summary>
    /// Implicitly converts a value to a successful <see cref="StepResult{T}"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator StepResult<T>(T value) => new(value);

    /// <summary>
    /// Deconstructs the step result into its components.
    /// </summary>
    /// <param name="isSuccess">Output parameter indicating whether the result was successful.</param>
    /// <param name="value">Output parameter containing the value if successful.</param>
    /// <param name="errorMessage">Output parameter containing the error message if failed.</param>
    public void Deconstruct(out bool isSuccess, out T? value, out string? errorMessage)
    {
        isSuccess = IsSuccess;
        value = IsSuccess ? _value : default;
        errorMessage = !IsSuccess ? _errorMessage : null;
    }
}