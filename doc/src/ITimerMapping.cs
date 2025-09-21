using BBT.Workflow.Definitions.Timer;

namespace BBT.Workflow.Scripting;

/// <summary>
/// Defines the contract for timer mapping handlers that process script contexts to create flexible timer schedules.
/// This interface is used for implementing custom timer logic within the workflow scripting engine with support 
/// for Dapr-compatible scheduling including DateTime, Cron expressions, Duration, and Immediate execution.
/// </summary>
/// <remarks>
/// Timer mapping implementations are responsible for interpreting script context data
/// and returning appropriate WorkflowTimerSchedule objects for scheduling, delays, and time-based workflow operations.
/// The enhanced interface provides the same flexibility as Dapr's job scheduling system.
/// </remarks>
public interface ITimerMapping
{
    /// <summary>
    /// Asynchronously handles timer calculation based on the provided script context.
    /// </summary>
    /// <param name="context">
    /// The script execution context containing instance data, workflow information,
    /// and other contextual data needed for timer calculation. This context provides
    /// access to workflow variables, instance state, and execution metadata.
    /// </param>
    /// <returns>
    /// A Task representing the asynchronous operation. The task result contains a WorkflowTimerSchedule
    /// that can represent DateTime, Cron expressions, Duration, or Immediate execution schedules.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the context parameter is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the timer calculation fails or produces an invalid result.
    /// </exception>
    /// <remarks>
    /// Implementations should handle various timer scenarios such as:
    /// <list type="bullet">
    /// <item>Absolute scheduling using WorkflowTimerSchedule.FromDateTime()</item>
    /// <item>Cron-based scheduling using WorkflowTimerSchedule.FromCronExpression()</item>
    /// <item>Relative delays using WorkflowTimerSchedule.FromDuration()</item>
    /// <item>Immediate execution using WorkflowTimerSchedule.Immediate()</item>
    /// <item>Business logic-based timing (calculated from workflow data)</item>
    /// <item>Recurring patterns using Cron expressions</item>
    /// </list>
    /// </remarks>
    Task<TimerSchedule> Handler(ScriptContext context);
}