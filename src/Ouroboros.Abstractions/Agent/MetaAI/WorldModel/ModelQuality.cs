namespace Ouroboros.Agent.MetaAI.WorldModel;

/// <summary>
/// Represents quality metrics for a world model.
/// Used to evaluate model accuracy and calibration.
/// </summary>
/// <param name="PredictionAccuracy">Accuracy of state predictions (0-1).</param>
/// <param name="RewardCorrelation">Correlation of predicted vs actual rewards (0-1).</param>
/// <param name="TerminalAccuracy">Accuracy of terminal state predictions (0-1).</param>
/// <param name="CalibrationError">Mean calibration error for uncertainty estimates.</param>
/// <param name="TestSamples">Number of samples used in evaluation.</param>
public sealed record ModelQuality(
    double PredictionAccuracy,
    double RewardCorrelation,
    double TerminalAccuracy,
    double CalibrationError,
    int TestSamples);