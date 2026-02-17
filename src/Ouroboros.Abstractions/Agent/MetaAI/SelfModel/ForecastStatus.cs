namespace Ouroboros.Agent.MetaAI.SelfModel;

/// <summary>
/// Status of a forecast.
/// </summary>
public enum ForecastStatus
{
    /// <summary>Forecast is pending - target time not reached</summary>
    Pending,
    
    /// <summary>Forecast verified - outcome matches prediction</summary>
    Verified,
    
    /// <summary>Forecast failed - outcome differs from prediction</summary>
    Failed,
    
    /// <summary>Forecast cancelled - no longer relevant</summary>
    Cancelled
}