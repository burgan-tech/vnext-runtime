# Interfaces

These are the basic interfaces used in script-based code writing for definitions on the platform. The technology used for developing scripts is the [Roslyn](https://github.com/dotnet/roslyn) product.

## Source Code References

Interface definitions can be found in the following files:
- **Main Interfaces**: [`/src`](./src/) folder
- **Documentation Copies**: [`src/`](src/) folder
- **ScriptContext and ScriptResponse**: [`../src/Models.cs`](../src/Models.cs)

> **Note**: The code examples in this document are for reference purposes. Please check the source files above for current definitions.

## Interface Types

### IMapping
General mapping interface. Used for input and output bindings of tasks.

> **Source**: [`../src/IMapping.cs`](../src/IMapping.cs)

**Usage Areas:**
- Input data preparation and transformation before task execution
- Output data processing after task execution
- Data validation and transformation
- Audit logging and metadata management

**Methods:**
```csharp
Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context);
Task<ScriptResponse> OutputHandler(ScriptContext context);
```

**Method Descriptions:**
- `InputHandler`: Prepares input data before the task is executed, configures the WorkflowTask object
- `OutputHandler`: Processes output data after the task is executed and merges it into the workflow instance

### ITimerMapping
Used for schedule mapping. Special interface for timer-based workflows and scheduling operations.

> **Source**: [`../src/ITimerMapping.cs`](../src/ITimerMapping.cs)

**Usage Areas:**
- DateTime-based scheduling
- Periodic operations with cron expressions
- Duration-based delays
- Immediate execution
- Business logic-based scheduling calculations

**Method:**
```csharp
Task<TimerSchedule> Handler(ScriptContext context);
```

**Method Description:**
- `Handler`: Calculates timer schedule according to script context and returns TimerSchedule object

### ISubProcessMapping
Used for input binding for sub processes. For starting independent sub-processes.

> **Source**: [`../src/ISubProcessMapping.cs`](../src/ISubProcessMapping.cs)

**Usage Areas:**
- Background data processing
- Audit log generation
- External system notifications
- Data synchronization
- Fire-and-forget operations

**Method:**
```csharp
Task<ScriptResponse> InputHandler(ScriptContext context);
```

**Method Description:**
- `InputHandler`: Prepares the necessary input data to start a subprocess and returns ScriptResponse

**Note:** Since subprocesses run independently, only input binding is required, no output binding.

### ISubFlowMapping
Used for input binding for sub flows and output binding to transfer data to the parent flow when completed.

> **Source**: [`../src/ISubFlowMapping.cs`](../src/ISubFlowMapping.cs)

**Usage Areas:**
- Approval workflows
- Data processing workflows
- Validation processes
- Computed value generation
- Parent-child workflow integration

**Methods:**
```csharp
Task<ScriptResponse> InputHandler(ScriptContext context);
Task<ScriptResponse> OutputHandler(ScriptContext context);
```

**Method Descriptions:**
- `InputHandler`: Prepares input data to start subflow and returns ScriptResponse
- `OutputHandler`: Transfers results to parent workflow when subflow is completed and returns ScriptResponse

**Note:** Since subflows run integrated with the parent workflow, both input and output binding are required.

### IConditionMapping
Used for decision parts like auto transitions. Provides condition checking in automatic transitions.

> **Source**: [`../src/IConditionMapping.cs`](../src/IConditionMapping.cs)

**Usage Areas:**
- Data validation checks
- Business rule validation
- Time-based conditions
- External system status checks
- User role/permission verification
- Auto-transition decision logic

**Method:**
```csharp
Task<bool> Handler(ScriptContext context);
```

**Method Description:**
- `Handler`: Returns boolean value according to the given context (true: allow transition, false: block transition)

### ITransitionMapping
Used for mapping transition payloads to instance data. Allows custom transformation of transition request data before merging into workflow instance.

> **Source**: [`../src/ITransitionMapping.cs`](../src/ITransitionMapping.cs)

**Usage Areas:**
- Custom payload transformation before saving to instance data
- Data validation and sanitization during transitions
- Enriching transition data with additional context
- Filtering or restructuring incoming data
- Default behavior: If no mapping is defined, payload is written as-is to instance data

**Method:**
```csharp
Task<dynamic> Handler(ScriptContext context);
```

**Method Description:**
- `Handler`: Transforms transition payload and returns the data to be merged into workflow instance data

**Transition Schema with Mapping:**
```json
{
  "key": "transition-name",
  "source": "source-state",
  "target": "target-state",
  "mapping": {
    "code": "BASE64_ENCODED_CSX_CONTENT",
    "location": "./src/TransitionMappingFile.csx"
  }
}
```
