namespace Ouroboros.Tools;

/// <summary>
/// Interface for LLM providers.
/// </summary>
[Obsolete("No implementations exist. Scheduled for removal.")]
public interface ILlmProvider
{
    Task<string> GenerateAsync(string prompt);
}