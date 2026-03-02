namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Result of a ZeroShift context transition operation.
/// </summary>
public sealed record ZeroShiftResult
{
    /// <summary>Gets a value indicating whether the shift succeeded.</summary>
    public required bool Success { get; init; }

    /// <summary>Gets the cognitive state after the shift attempt.</summary>
    public required CognitiveState State { get; init; }

    /// <summary>Gets the resource cost of the shift.</summary>
    public required double Cost { get; init; }

    /// <summary>Gets the reason for failure, or null when the shift succeeded.</summary>
    public string? FailureReason { get; init; }

    /// <summary>Creates a successful shift result with the resulting state and its cost.</summary>
    /// <param name="state">The new cognitive state after the shift.</param>
    /// <param name="cost">The resource cost incurred.</param>
    public static ZeroShiftResult Succeeded(CognitiveState state, double cost) =>
        new() { Success = true, State = state, Cost = cost };

    /// <summary>Creates a failed shift result with the unchanged state and failure reason.</summary>
    /// <param name="state">The cognitive state after the failed attempt.</param>
    /// <param name="reason">The reason the shift was rejected.</param>
    public static ZeroShiftResult Failed(CognitiveState state, string reason) =>
        new() { Success = false, State = state, Cost = 0.0, FailureReason = reason };
}
