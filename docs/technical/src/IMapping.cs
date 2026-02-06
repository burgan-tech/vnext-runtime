using BBT.Workflow.Definitions;

namespace BBT.Workflow.Scripting;

/// <summary>
/// Defines the contract for task input and output data binding operations in workflow execution.
/// This interface enables custom mapping logic for transforming data before and after task execution,
/// providing flexibility for data manipulation, validation, and audit logging.
/// </summary>
/// <remarks>
/// <para>
/// IMapping is the primary interface for implementing custom data binding logic in workflow tasks.
/// It provides two key extension points in the task execution lifecycle:
/// </para>
/// <list type="number">
/// <item>
/// <description>
/// <strong>InputHandler</strong>: Called before task execution to prepare and transform input data.
/// This allows modification of the WorkflowTask object and generation of audit data.
/// </description>
/// </item>
/// <item>
/// <description>
/// <strong>OutputHandler</strong>: Called after task execution to process results and prepare data for instance merging.
/// The returned data becomes part of the workflow instance state.
/// </description>
/// </item>
/// </list>
/// <para>
/// <strong>Implementation Guidelines:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>InputHandler should modify the WorkflowTask object directly for task configuration</description></item>
/// <item><description>Both handlers should return ScriptResponse for audit logging and data capture</description></item>
/// <item><description>OutputHandler data is automatically merged into the workflow instance</description></item>
/// <item><description>Use ScriptContext to access workflow state, instance data, and runtime information</description></item>
/// <item><description>Implement proper error handling and logging within handlers</description></item>
/// </list>
/// </remarks>
public interface IMapping
{
    /// <summary>
    /// Handles input data binding and task configuration before task execution.
    /// This method is called prior to executing the workflow task and allows for:
    /// - Modifying task parameters and configuration
    /// - Validating input data
    /// - Preparing audit information
    /// - Transforming data for task consumption
    /// </summary>
    /// <param name="task">
    /// The WorkflowTask object that will be executed. This object can be modified directly
    /// to change task behavior, endpoint URLs, headers, or other configuration parameters.
    /// </param>
    /// <param name="context">
    /// The ScriptContext containing workflow state, instance data, headers, route values,
    /// and other contextual information needed for input processing.
    /// </param>
    /// <returns>
    /// A ScriptResponse containing audit data and metadata about the input processing.
    /// The response data is logged for task audit purposes and can include:
    /// - Input validation results
    /// - Data transformation logs
    /// - Processing timestamps
    /// - Custom audit information
    /// </returns>
    /// <remarks>
    /// <para>
    /// The InputHandler is invoked during the task preparation phase and should:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Validate and transform input data as needed</description></item>
    /// <item><description>Configure the WorkflowTask object for execution</description></item>
    /// <item><description>Generate comprehensive audit information</description></item>
    /// <item><description>Handle any input-related errors gracefully</description></item>
    /// </list>
    /// <para>
    /// Common use cases include dynamic endpoint generation, input validation,
    /// authentication token preparation, and custom header configuration.
    /// </para>
    /// </remarks>
    Task<ScriptResponse> InputHandler(
        WorkflowTask task,
        ScriptContext context);

    /// <summary>
    /// Handles output data processing and transformation after task execution.
    /// This method is called after the workflow task completes and processes the results to:
    /// - Transform task output data
    /// - Prepare data for instance state merging
    /// - Generate audit information
    /// - Handle post-processing logic
    /// </summary>
    /// <param name="context">
    /// The ScriptContext containing the task execution results in TaskResponse collection,
    /// along with current workflow state, instance data, and other contextual information
    /// needed for output processing.
    /// </param>
    /// <returns>
    /// A ScriptResponse containing the processed output data that will be merged into the
    /// workflow instance state. The response data should include:
    /// - Transformed task results
    /// - Computed values based on task output
    /// - State updates for the workflow instance
    /// - Post-processing audit information
    /// </returns>
    /// <remarks>
    /// <para>
    /// The OutputHandler is invoked after successful task execution and should:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Extract and transform relevant data from task results</description></item>
    /// <item><description>Prepare data in the format expected by the workflow instance</description></item>
    /// <item><description>Generate audit information about the transformation process</description></item>
    /// <item><description>Handle any output-related errors or data inconsistencies</description></item>
    /// </list>
    /// <para>
    /// The returned ScriptResponse.Data will be automatically merged into the workflow
    /// instance data, making it available for subsequent tasks and workflow logic.
    /// </para>
    /// </remarks>
    Task<ScriptResponse> OutputHandler(
        ScriptContext context);
}