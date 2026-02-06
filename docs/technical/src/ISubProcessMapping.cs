namespace BBT.Workflow.Scripting;

/// <summary>
/// Defines the contract for implementing data binding operations for subprocess execution within workflows.
/// This interface enables custom logic for preparing and transforming input data required to initiate
/// subprocess instances, facilitating seamless integration between parent workflows and their subprocesses.
/// </summary>
/// <remarks>
/// <para>
/// ISubProcessMapping is specifically designed for subprocess initialization and data preparation.
/// Unlike subflows which have both input and output handlers, subprocesses typically only require
/// input data binding since they execute independently and may not return data to the parent workflow.
/// </para>
/// <para>
/// <strong>Key Differences from ISubFlowMapping:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Subprocess</strong>: Independent process execution, typically for fire-and-forget operations</description></item>
/// <item><description><strong>Subflow</strong>: Integrated workflow execution with result integration back to parent</description></item>
/// <item><description><strong>Input Only</strong>: Subprocesses only need input data binding, no output processing</description></item>
/// <item><description><strong>Autonomy</strong>: Subprocesses operate independently without parent workflow dependency</description></item>
/// </list>
/// <para>
/// <strong>Subprocess Execution Flow:</strong>
/// </para>
/// <list type="number">
/// <item><description>Parent workflow triggers subprocess creation</description></item>
/// <item><description>InputHandler prepares subprocess initialization data</description></item>
/// <item><description>Subprocess is created and executed independently</description></item>
/// <item><description>Parent workflow continues without waiting for subprocess completion</description></item>
/// </list>
/// <para>
/// <strong>Implementation Guidelines:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Extract and transform parent workflow data for subprocess consumption</description></item>
/// <item><description>Provide all necessary data for autonomous subprocess execution</description></item>
/// <item><description>Return ScriptResponse with complete initialization data</description></item>
/// <item><description>Use ScriptContext to access comprehensive parent workflow state</description></item>
/// <item><description>Ensure subprocess can operate independently with provided data</description></item>
/// </list>
/// <para>
/// <strong>Common Use Cases:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Background data processing tasks</description></item>
/// <item><description>Audit log generation</description></item>
/// <item><description>External system notifications</description></item>
/// <item><description>Data synchronization processes</description></item>
/// <item><description>Batch processing operations</description></item>
/// <item><description>Cleanup and maintenance tasks</description></item>
/// </list>
/// </remarks>
public interface ISubProcessMapping
{
    /// <summary>
    /// Prepares and transforms input data required for subprocess initialization and execution.
    /// This method extracts relevant data from the parent workflow context and formats it
    /// appropriately for creating and starting an independent subprocess instance.
    /// </summary>
    /// <param name="context">
    /// The ScriptContext from the parent workflow containing:
    /// - Parent workflow instance data and current state
    /// - Task execution context and results
    /// - Headers, route values, and other contextual information
    /// - Runtime information and workflow definitions
    /// - User session and authentication data
    /// </param>
    /// <returns>
    /// A ScriptResponse containing the complete data package for subprocess creation and execution.
    /// The response should include:
    /// - Data: Comprehensive input parameters and initialization data for the subprocess
    /// - Key: Unique identifier for tracking and correlating this subprocess
    /// - Headers: Authentication, session, or context headers needed by the subprocess
    /// - RouteValues: Routing parameters or configuration values for subprocess execution
    /// - Tags: Categorization tags for monitoring, auditing, and operational tracking
    /// </returns>
    /// <remarks>
    /// <para>
    /// The InputHandler is responsible for:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Extracting all relevant data from the parent workflow instance</description></item>
    /// <item><description>Transforming data into the format required by the subprocess workflow</description></item>
    /// <item><description>Providing complete initialization parameters for autonomous execution</description></item>
    /// <item><description>Setting up correlation and tracking information</description></item>
    /// <item><description>Ensuring the subprocess has all necessary context for independent operation</description></item>
    /// </list>
    /// <para>
    /// Since subprocesses operate independently, the InputHandler must provide all the data
    /// the subprocess will need throughout its entire execution lifecycle. Consider:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Authentication and authorization information</description></item>
    /// <item><description>Configuration parameters and settings</description></item>
    /// <item><description>Reference data and lookup values</description></item>
    /// <item><description>Correlation identifiers for tracking and auditing</description></item>
    /// <item><description>Any business data required for subprocess operations</description></item>
    /// </list>
    /// <para>
    /// The returned ScriptResponse.Data will be used as the initial instance data for the
    /// newly created subprocess workflow. The subprocess will execute independently and
    /// asynchronously from the parent workflow.
    /// </para>
    /// <para>
    /// Common implementation patterns include:
    /// - Background processing tasks (data transformation, report generation)
    /// - Audit and logging operations
    /// - External system integration and synchronization
    /// - Notification and communication workflows
    /// - Cleanup and maintenance operations
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the parent workflow context lacks required data for subprocess initialization.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the context parameter is null.
    /// </exception>
    Task<ScriptResponse> InputHandler(
        ScriptContext context);
}