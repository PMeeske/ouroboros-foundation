namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Actions to take when homeostasis is violated.
/// </summary>
public enum HomeostasisAction
{
    /// <summary>Log the violation but take no action</summary>
    Log,
    
    /// <summary>Send an alert notification</summary>
    Alert,
    
    /// <summary>Reduce workload or task complexity</summary>
    Throttle,
    
    /// <summary>Increase workload or seek challenges</summary>
    Boost,
    
    /// <summary>Pause non-critical operations</summary>
    Pause,
    
    /// <summary>Reset to baseline state</summary>
    Reset,
    
    /// <summary>Execute custom corrective action</summary>
    Custom
}