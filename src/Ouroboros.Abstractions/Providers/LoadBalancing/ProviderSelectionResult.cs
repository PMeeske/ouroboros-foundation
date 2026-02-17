namespace Ouroboros.Providers.LoadBalancing;

/// <summary>
/// Result of provider selection including the selected provider and reasoning.
/// </summary>
public sealed record ProviderSelectionResult<T>(
    T Provider,
    string ProviderId,
    string Strategy,
    string Reason,
    ProviderHealthStatus Health);