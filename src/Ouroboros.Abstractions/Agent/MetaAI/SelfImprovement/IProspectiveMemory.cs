using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.SelfImprovement;

/// <summary>
/// Interface for prospective memory — remembering to do things in the future.
/// Supports time-based and event-based reminders.
/// Based on Einstein &amp; McDaniel (2005).
/// </summary>
public interface IProspectiveMemory
{
    /// <summary>
    /// Creates a time-based reminder that triggers at a specific time.
    /// </summary>
    /// <param name="description">Description of the reminder.</param>
    /// <param name="triggerTime">When the reminder should trigger.</param>
    /// <param name="action">The action to perform when triggered.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The unique identifier of the created reminder.</returns>
    Task<Result<string, string>> CreateTimeBasedReminderAsync(
        string description, DateTime triggerTime, string action, CancellationToken ct = default);

    /// <summary>
    /// Creates an event-based reminder that triggers when a condition is met.
    /// </summary>
    /// <param name="description">Description of the reminder.</param>
    /// <param name="triggerCondition">Condition that triggers the reminder.</param>
    /// <param name="action">The action to perform when triggered.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The unique identifier of the created reminder.</returns>
    Task<Result<string, string>> CreateEventBasedReminderAsync(
        string description, string triggerCondition, string action, CancellationToken ct = default);

    /// <summary>
    /// Checks the current context against pending reminders and returns any that are triggered.
    /// </summary>
    /// <param name="currentContext">The current context to check against.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of triggered reminders.</returns>
    Task<List<ProspectiveReminder>> CheckTriggeredRemindersAsync(
        string currentContext, CancellationToken ct = default);

    /// <summary>
    /// Marks a reminder as completed.
    /// </summary>
    /// <param name="reminderId">The reminder identifier.</param>
    void MarkReminderComplete(string reminderId);

    /// <summary>
    /// Cancels a pending reminder.
    /// </summary>
    /// <param name="reminderId">The reminder identifier.</param>
    void CancelReminder(string reminderId);

    /// <summary>
    /// Gets all pending reminders.
    /// </summary>
    /// <returns>List of pending reminders.</returns>
    List<ProspectiveReminder> GetPendingReminders();
}

/// <summary>
/// Type of prospective memory reminder.
/// </summary>
[ExcludeFromCodeCoverage]
public enum ReminderType
{
    /// <summary>Triggered at a specific time.</summary>
    TimeBased,

    /// <summary>Triggered when an event or condition is detected.</summary>
    EventBased
}

/// <summary>
/// A prospective memory reminder.
/// </summary>
/// <param name="Id">Unique reminder identifier.</param>
/// <param name="Description">Human-readable description.</param>
/// <param name="Type">Whether the reminder is time-based or event-based.</param>
/// <param name="TriggerCondition">The condition or description for triggering.</param>
/// <param name="Action">The action to perform when triggered.</param>
/// <param name="CreatedAt">When the reminder was created.</param>
/// <param name="TriggerTime">For time-based reminders, the trigger time.</param>
/// <param name="IsTriggered">Whether the reminder has been triggered.</param>
public sealed record ProspectiveReminder(
    string Id, string Description, ReminderType Type,
    string TriggerCondition, string Action, DateTime CreatedAt,
    DateTime? TriggerTime, bool IsTriggered);
