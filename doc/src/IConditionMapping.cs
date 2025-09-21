namespace BBT.Workflow.Scripting;

/// <summary>
/// Defines the contract for implementing conditional logic in automatic workflow transitions.
/// This interface enables custom decision-making logic to determine whether an auto-transition
/// should be executed based on the current workflow state and context.
/// </summary>
/// <remarks>
/// <para>
/// IConditionMapping is specifically designed for auto-transitions in workflow execution.
/// Auto-transitions are transitions that can occur automatically without user intervention,
/// but only when certain conditions are met. This interface provides the mechanism to
/// implement those conditional checks.
/// </para>
/// <para>
/// <strong>Key Concepts:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Auto-transitions</strong>: Transitions that execute automatically when conditions are satisfied</description></item>
/// <item><description><strong>Conditional Logic</strong>: Custom business rules that determine transition eligibility</description></item>
/// <item><description><strong>Context-based Decisions</strong>: Decisions based on workflow instance data, task results, and runtime state</description></item>
/// </list>
/// <para>
/// <strong>Implementation Guidelines:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Return true to allow the auto-transition to proceed</description></item>
/// <item><description>Return false to prevent the auto-transition from executing</description></item>
/// <item><description>Use ScriptContext to access all workflow state and data for decision making</description></item>
/// <item><description>Implement efficient logic as this may be called frequently</description></item>
/// <item><description>Handle exceptions gracefully and consider default behavior for error cases</description></item>
/// </list>
/// <para>
/// <strong>Common Use Cases:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Data validation checks before proceeding to next state</description></item>
/// <item><description>Business rule validation (e.g., credit limits, approval thresholds)</description></item>
/// <item><description>Time-based conditions (e.g., business hours, deadlines)</description></item>
/// <item><description>External system status checks</description></item>
/// <item><description>User role or permission verification</description></item>
/// </list>
/// </remarks>
public interface IConditionMapping
{
    /// <summary>
    /// Evaluates whether an auto-transition should be executed based on current workflow context.
    /// This method contains the conditional logic that determines if the workflow can proceed
    /// with an automatic transition to the next state.
    /// </summary>
    /// <param name="context">
    /// The ScriptContext containing comprehensive workflow information including:
    /// - Current workflow instance data and state
    /// - Task execution results and responses
    /// - Workflow definition and metadata
    /// - Runtime information and headers
    /// - Route values and other contextual data
    /// </param>
    /// <returns>
    /// A boolean value indicating whether the auto-transition should proceed:
    /// - <c>true</c>: The condition is satisfied, allow the auto-transition to execute
    /// - <c>false</c>: The condition is not met, prevent the auto-transition from executing
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is called by the workflow engine when evaluating auto-transitions.
    /// The implementation should:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Evaluate business rules and conditions efficiently</description></item>
    /// <item><description>Access necessary data from the ScriptContext</description></item>
    /// <item><description>Return a clear boolean decision without side effects</description></item>
    /// <item><description>Handle any data access or processing errors gracefully</description></item>
    /// <item><description>Consider performance implications as this may be called frequently</description></item>
    /// </list>
    /// <para>
    /// The method should be stateless and deterministic - given the same context,
    /// it should return the same result. Avoid modifying the context or performing
    /// actions that change the workflow state within this method.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the context data is insufficient or invalid for condition evaluation.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the context parameter is null.
    /// </exception>
    Task<bool> Handler(ScriptContext context);
}