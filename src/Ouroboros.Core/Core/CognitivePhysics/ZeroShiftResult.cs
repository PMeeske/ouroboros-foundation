namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Result of a ZeroShift context transition operation.
/// </summary>
public sealed record ZeroShiftResult
{
    public required bool Success { get; init; }
    public required CognitiveState State { get; init; }
    public required double Cost { get; init; }
    public string? FailureReason { get; init; }

    public static ZeroShiftResult Succeeded(CognitiveState state, double cost) =>
        new() { Success = true, State = state, Cost = cost };

    public static ZeroShiftResult Failed(CognitiveState state, string reason) =>
        new() { Success = false, State = state, Cost = 0.0, FailureReason = reason };
}