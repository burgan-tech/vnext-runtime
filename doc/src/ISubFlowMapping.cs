namespace BBT.Workflow.Scripting;

/// <summary>
/// Defines the contract for implementing data binding operations for subflow execution within workflows.
/// This interface enables custom logic for preparing subflow input data and processing subflow completion results,
/// facilitating seamless integration between parent workflows and their subflows.
/// </summary>
/// <remarks>
/// <para>
/// ISubFlowMapping is designed to handle the complete lifecycle of subflow data integration:
/// </para>
/// <list type="number">
/// <item>
/// <description>
/// <strong>InputHandler</strong>: Prepares and transforms data needed to start a subflow.
/// This includes extracting relevant data from the parent workflow context and formatting it
/// for subflow initialization.
/// </description>
/// </item>
/// <item>
/// <description>
/// <strong>OutputHandler</strong>: Processes the completed subflow results and transforms them
/// for integration back into the parent workflow instance data.
/// </description>
/// </item>
/// </list>
/// <para>
/// <strong>Subflow Execution Flow:</strong>
/// </para>
/// <list type="number">
/// <item><description>Parent workflow triggers subflow creation</description></item>
/// <item><description>InputHandler prepares subflow initialization data</description></item>
/// <item><description>Subflow is created and executed independently</description></item>
/// <item><description>Upon subflow completion, OutputHandler processes results</description></item>
/// <item><description>Processed data is merged into parent workflow instance</description></item>
/// </list>
/// <para>
/// <strong>Implementation Guidelines:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>InputHandler should extract and transform parent workflow data for subflow consumption</description></item>
/// <item><description>OutputHandler should process completed subflow data for parent workflow integration</description></item>
/// <item><description>Both handlers return ScriptResponse with data used for subflow management</description></item>
/// <item><description>Use ScriptContext to access comprehensive workflow state and instance data</description></item>
/// <item><description>Ensure data consistency and proper error handling throughout the process</description></item>
/// </list>
/// </remarks>
public interface ISubFlowMapping
{
    /// <summary>
    /// Prepares and transforms input data required for subflow initialization.
    /// This method extracts relevant data from the parent workflow context and formats it
    /// appropriately for creating and starting a new subflow instance.
    /// </summary>
    /// <param name="context">
    /// The ScriptContext from the parent workflow containing:
    /// - Parent workflow instance data and state
    /// - Current task execution context
    /// - Headers, route values, and other contextual information
    /// - Runtime information and workflow definitions
    /// </param>
    /// <returns>
    /// A ScriptResponse containing the prepared data for subflow creation. The response should include:
    /// - Data: Input parameters and initial data for the subflow instance
    /// - Key: Identifier for tracking this subflow initialization
    /// - Headers: Any header information needed for subflow execution
    /// - RouteValues: Routing parameters for subflow creation
    /// - Tags: Categorization tags for audit and tracking purposes
    /// </returns>
    /// <remarks>
    /// <para>
    /// The InputHandler is responsible for:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Extracting relevant data from parent workflow instance</description></item>
    /// <item><description>Transforming data into the format expected by the subflow</description></item>
    /// <item><description>Providing initialization parameters for subflow creation</description></item>
    /// <item><description>Setting up any correlation data for tracking the subflow</description></item>
    /// </list>
    /// <para>
    /// The returned ScriptResponse.Data will be used as the initial instance data for the
    /// newly created subflow. Ensure all necessary information is included for the subflow
    /// to execute successfully.
    /// </para>
    /// </remarks>
    Task<ScriptResponse> InputHandler(
        ScriptContext context);

    /// <summary>
    /// Processes completed subflow results and transforms them for integration into the parent workflow.
    /// This method is called when a subflow reaches completion and handles the merging of
    /// subflow results back into the parent workflow instance data.
    /// </summary>
    /// <param name="context">
    /// The ScriptContext from the completed subflow containing:
    /// - Completed subflow instance data with final results
    /// - Subflow execution state and outcome
    /// - Any metadata generated during subflow execution
    /// - Runtime information from the subflow completion
    /// </param>
    /// <returns>
    /// A ScriptResponse containing the transformed data for parent workflow integration.
    /// The response should include:
    /// - Data: Processed subflow results formatted for parent workflow consumption
    /// - Key: Identifier for tracking this subflow completion
    /// - Headers: Any header information for parent workflow context
    /// - RouteValues: Routing parameters for parent workflow continuation
    /// - Tags: Categorization tags for audit and result tracking
    /// </returns>
    /// <remarks>
    /// <para>
    /// The OutputHandler is responsible for:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Extracting meaningful results from the completed subflow</description></item>
    /// <item><description>Transforming subflow data into the format expected by parent workflow</description></item>
    /// <item><description>Determining what data should be merged into parent instance</description></item>
    /// <item><description>Providing completion status and outcome information</description></item>
    /// </list>
    /// <para>
    /// The returned ScriptResponse.Data will be automatically merged into the parent workflow
    /// instance data, making the subflow results available for subsequent parent workflow
    /// tasks and logic. Consider the data schema expected by the parent workflow when
    /// formatting the response.
    /// </para>
    /// <para>
    /// This method typically handles scenarios such as:
    /// - Approval workflow results
    /// - Data processing outcomes
    /// - Validation results
    /// - Computed values from subflow execution
    /// </para>
    /// </remarks>
    Task<ScriptResponse> OutputHandler(
        ScriptContext context);
}