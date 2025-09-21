# Task Document

Tasks are independent components that perform specific operations during workflow runtime. Each task can be defined in different types according to its own special purpose and can be executed at different points in the workflow.

:::highlight green 💡
Tasks are stored as an independent workflow. They are defined as references to the points where they will be used.
A workflow named `tasks` is created in each domain deployment. All tasks used within the domain are record instances in this workflow.
:::

## Task Types

The system currently supports 9 different task types:

| Task Type | Description | Detail Document |
|-----------|-------------|-----------------|
| **DaprService** | Dapr service invocation calls | [📄 DaprService README](./tasks/dapr-service.md) |
| **DaprPubSub** | Dapr pub/sub messaging | [📄 DaprPubSub README](./tasks/dapr-pubsub.md) |
| **Http** | HTTP web service calls | [📄 Http README](./tasks/http-task.md) |
| **Script** | C# Roslyn script execution | [📄 Script README](./tasks/script-task.md) |
| **Condition** | Condition checking (system only) | [📄 Condition README](./tasks/condition-task.md) |
| **Timer** | Timer tasks (system only) | [📄 Timer README](./tasks/timer-task.md) |

## Task Usage

Tasks are used by being referenced by other modules. In each task usage, `order`, `task` reference, and `mapping` information are defined.

### Example Task Definition

```json
"onExecutionTasks": [
  {
    "order": 1,
    "task": {
      "ref": "Tasks/invalidate-cache.json"
    },
    "mapping": {
      "location": "./src/InvalideCacheMapping.csx",
      "code": "<BASE64>"
    }
  }
]
```

### Execution Order
- `order` values are grouped among themselves
- Those with the same order are executed **in parallel**
- Those with different orders are executed **sequentially**

### Data Management
- If tasks have output data as a result of their execution, they increase the master data as a patch version
- Input and output binding is done with the `mapping` field

### Task Execution Points

**Within the workflow:**
- `Transition.OnExecutionTasks`: Executed when transition is triggered
- `State.OnEntries`: Executed on first entry to a stage
- `State.OnExits`: Executed on first exit from a stage

**Outside the workflow:**
- `Functions.OnExecutionTasks`: Executed within platform services
- `Extensions.OnExecutionTasks`: Workflow record instance tasks

## Standard Task Response

All task types use the same standard response structure:

```csharp
public sealed class StandardTaskResponse
{
    /// <summary>
    /// Data returned from task execution
    /// </summary>
    public dynamic? Data { get; set; }

    /// <summary>
    /// Status code for HTTP-based tasks
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Whether the task execution was successful
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Error message in case of error
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Response headers for HTTP-based tasks
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Additional metadata about task execution
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Task execution time (milliseconds)
    /// </summary>
    public long? ExecutionDurationMs { get; set; }

    /// <summary>
    /// Task type identifier
    /// </summary>
    public string? TaskType { get; set; }
}
```
