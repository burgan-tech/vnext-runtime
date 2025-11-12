# Workflow Definition

Workflows are fundamental components that model business processes and run at runtime. Each workflow is designed as a state machine for a specific purpose and can be defined in different types in the system.

:::highlight green 💡
Workflows are defined in JSON format and automatically loaded by the system. Each workflow is defined with a domain-specific key and supports version management.
:::

## Workflow Types

The system currently supports 4 different workflow types:

| Workflow Type | Code | Description | Use Cases |
|---------------|------|-------------|-----------|
| **Core** | C | Platform core workflows | System operations, platform services |
| **Flow** | F | Main workflows | Main business processes, user interaction |
| **SubFlow** | S | Sub workflows | Reusable process parts |
| **SubProcess** | P | Sub processes | Parallel and independent operations |

## Workflow Components

### Basic Properties

#### Key
- Unique identifier of the workflow
- Must be unique within the domain
- Should be compatible with the file name

#### Domain
- Specifies which domain the workflow belongs to
- Provides namespace for microservice architecture
- Offers multi-tenant support

#### Version
- Uses semantic versioning (SemVer) standard
- Example: `"1.0.0"`, `"2.1.3"`
- Used for backward compatibility control

#### Type
- Specifies what type the workflow is
- Affects runtime behavior
- Defined as `workflowType` in schema

### Optional Components

#### Labels
Used for multi-language support:
```json
"labels": [
  {
    "label": "User Registration Process",
    "language": "en"
  },
  {
    "label": "Kullanıcı Kayıt Süreci",
    "language": "tr"
  }
]
```

#### Timeout
For automatic workflow termination:
```json
"timeout": {
  "key": "registration-timeout",
  "target": "timeout-state",
  "versionStrategy": "Minor",
  "timer": {
    "reset": "workflow-start",
    "duration": "PT24H"
  }
}
```

#### Functions
Function references for platform services:
```json
"functions": [
  {
    "ref": "Functions/user-validation.json"
  }
]
```

#### Features
Common component references:
```json
"features": [
  {
    "ref": "Features/document-upload.json"
  }
]
```

#### Extensions
Workflow instance enrichment tasks:
```json
"extensions": [
  {
    "ref": "Extensions/audit-logger.json"
  }
]
```

#### SharedTransitions
Common transitions (example: Cancel, Approve):
```json
"sharedTransitions": [
  {
    "key": "cancel",
    "target": "cancelled",
    "triggerType": 0,
    "labels": [...]
  }
]
```

## Start Transition

:::warning Required Component
All workflows must have a `startTransition` component. This defines how the workflow is started.
:::

### Start Transition Properties

```json
"startTransition": {
  "key": "start",
  "target": "initial-state",
  "triggerType": 0,
  "versionStrategy": "Major",
  "schema": {
    "ref": "Schemas/start-schema.json"
  },
  "labels": [
    {
      "label": "Start Process",
      "language": "en"
    }
  ],
  "onExecutionTasks": [
    {
      "order": 1,
      "task": {
        "ref": "Tasks/initialize-data.json"
      },
      "mapping": {
        "location": "./src/InitializeDataMapping.csx",
        "code": "<BASE64>"
      }
    }
  ]
}
```

**Important Notes:**
- Start transition has no `from` property
- Should redirect to initial state (`target`)
- Usually uses `Manual` trigger type (0)
- Can validate start data with schema

## States

Represents different stages of the workflow. For detailed information: [📄 State Documentation](./state.md)

### State Types
- **Initial (1)**: Starting state
- **Intermediate (2)**: Processing states  
- **Finish (3)**: Termination states
- **SubFlow (4)**: States that run sub workflows

### Example State Definition
```json
"states": [
  {
    "key": "user-verification",
    "stateType": 2,
    "versionStrategy": "Minor",
    "labels": [...],
    "transitions": [...],
    "onEntries": [...],
    "onExits": [...]
  }
]
```

## Workflow Schema

```json
{
  "type": "C (Core)|F (Flow)|S (SubFlow)|P (SubProcess)",
  "timeout": {
    "key": "string",
    "target": "string",
    "versionStrategy": "Major|Minor",
    "timer": {
      "reset": "string",
      "duration": "string (ISO 8601)"
    }
  },
  "labels": [
    {
      "label": "string",
      "language": "string"
    }
  ],
  "functions": [
    {
      "ref": "string"
    }
  ],
  "features": [
    {
      "ref": "string"
    }
  ],
  "extensions": [
    {
      "ref": "string"
    }
  ],
  "sharedTransitions": [
    {
      "key": "string",
      "target": "string",
      "triggerType": "0 (Manual)|1 (Automatic)| 2 (Scheduled)|3 (Event)",
      "versionStrategy": "Major|Minor",
      "availableIn": ["state1", "state2"],
      "labels": [...],
      "onExecutionTasks": [...]
    }
  ],
  "startTransition": {
    "key": "start",
    "target": "string",
    "triggerType": 0,
    "versionStrategy": "Major|Minor",
    "schema": {
      "ref": "string"
    },
    "labels": [...],
    "onExecutionTasks": [...]
  },
  "states": [
    {
      "key": "string",
      "stateType": "1 (Initial)|2 (Intermediate)|3 (Finish)|4 (SubFlow)",
      "versionStrategy": "Major|Minor",
      "labels": [...],
      "transitions": [...],
      "onEntries": [...],
      "onExits": [...],
      "view": {
        "ref": "string"
      },
      "subFlow": {
        "type": "S (SubFlow)|P (SubProcess)",
        "process": {
          "ref": "string"
        },
        "mapping": {
          "location": "string",
          "code": "string (BASE64)"
        }
      }
    }
  ]
}
```

## Workflow Development Guide

### 1. Workflow Planning

Before designing a workflow, answer the following questions:

**Basic Questions:**
- What stages does the business process consist of?
- What transitions exist between states?
- What are the automatic and manual transitions?
- Are there timeout situations?
- Are sub-processes needed?

**Technical Questions:**
- What is the start data schema?
- Which tasks need to be executed?
- Are there external system integrations?
- Are data transformations needed?

### 2. Workflow Structure

#### Domain and Key Definition
```json
{
  "key": "user-registration",
  "domain": "authentication"
}
```

**Important Notes:**
- Key value must be unique within domain
- Should be compatible with file name
- Kebab-case usage is recommended

#### Type Selection
- **Flow (F)**: For main user processes
- **SubFlow (S)**: For reusable sub-processes  
- **SubProcess (P)**: For parallel operations
- **Core (C)**: For platform operations (system only)

#### Version Strategy
```json
{
  "version": "1.0.0",
  "versionStrategy": "Major"
}
```

### 3. State Design

#### Initial State
Each workflow should have only one initial state:
```json
{
  "key": "start",
  "stateType": 1,
  "transitions": [
    {
      "key": "begin-verification",
      "target": "verification",
      "triggerType": 1
    }
  ]
}
```

#### Intermediate States
States where business logic is executed:
```json
{
  "key": "verification",
  "stateType": 2,
  "onEntries": [
    {
      "order": 1,
      "task": {
        "ref": "Tasks/send-verification-email.json"
      }
    }
  ],
  "transitions": [
    {
      "key": "verify-email",
      "target": "verified", 
      "triggerType": 0
    },
    {
      "key": "timeout",
      "target": "failed",
      "triggerType": 2,
      "timer": {...}
    }
  ]
}
```

#### Finish States
Process termination states:
```json
{
  "key": "completed",
  "stateType": 3,
  "onEntries": [
    {
      "order": 1,
      "task": {
        "ref": "Tasks/send-welcome-email.json"
      }
    }
  ]
}
```

### 4. Transition Design

For detailed information: [📄 Transition Documentation](./transition.md)

#### Manual Transitions
Transitions requiring user interaction:
```json
{
  "key": "approve",
  "target": "approved",
  "triggerType": 0,
  "schema": {
    "ref": "Schemas/approval-schema.json"
  }
}
```

#### Automatic Transitions  
Conditional automatic transitions:
```json
{
  "key": "auto-approve",
  "target": "approved", 
  "triggerType": 1,
  "rule": {
    "location": "./src/AutoApprovalRule.csx",
    "code": "<BASE64>"
  }
}
```

#### Scheduled Transitions
Time-based transitions:
```json
{
  "key": "daily-check",
  "target": "checking",
  "triggerType": 2, 
  "timer": {
    "location": "./src/DailyCheckTimer.csx",
    "code": "<BASE64>"
  }
}
```

### 5. Task Integration

For detailed information: [📄 Task Documentation](./task.md)

#### OnExecutionTasks
Tasks running during transition:
```json
"onExecutionTasks": [
  {
    "order": 1,
    "task": {
      "ref": "Tasks/validate-user.json"
    },
    "mapping": {
      "location": "./src/ValidateUserMapping.csx", 
      "code": "<BASE64>"
    }
  },
  {
    "order": 2,
    "task": {
      "ref": "Tasks/send-notification.json"
    },
    "mapping": {
      "location": "./src/NotificationMapping.csx",
      "code": "<BASE64>"
    }
  }
]
```

**Task Ordering:**
- Tasks with the same `order` value run in parallel
- Different `order` values run sequentially
- Order values are processed in ascending order

### 6. Mapping and Interface Usage

For detailed information: [📄 Mapping Documentation](./mapping.md) and [📄 Interface Documentation](./interface.md)

#### Basic Mapping Example
```csharp
public class UserRegistrationMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        
        // Prepare input data
        var userData = new
        {
            email = context.Body?.email,
            name = context.Body?.name,
            timestamp = DateTime.UtcNow
        };
        
        httpTask.SetBody(userData);
        
        return Task.FromResult(new ScriptResponse());
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse
        {
            Data = new
            {
                userId = context.Body?.data?.userId,
                registrationDate = DateTime.UtcNow
            }
        });
    }
}
```

#### Timer Mapping Example
```csharp
public class RegistrationTimeoutRule : ITimerMapping
{
    public async Task<TimerSchedule> Handler(ScriptContext context)
    {
        // Timeout after 24 hours
        return TimerSchedule.FromDuration(TimeSpan.FromHours(24));
    }
}
```

#### Condition Mapping Example
```csharp
public class AutoApprovalRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        var amount = context.Instance.Data.amount != null
            ? Convert.ToDecimal(context.Instance.Data.amount)
            : 0m;
            
        // Auto approve under 1000 TL
        return amount < 1000;
    }
}
```

## Best Practices

### 1. Workflow Structure
- **Single Responsibility**: Each state should have a single responsibility
- **Clear Naming**: Use understandable state and transition names
- **Minimize States**: Avoid unnecessary states
- **Error Handling**: Define special states for error situations

### 2. Performance
- **Parallel Tasks**: Run tasks in parallel when possible
- **Async Operations**: Use async patterns for long-running operations
- **State Optimization**: Avoid unnecessary state transitions
- **Data Size**: Don't store large datasets in state

### 3. Security
- **Input Validation**: Validate all inputs
- **Schema Usage**: Use data schemas
- **Secret Management**: Manage sensitive data with ScriptBase
- **Authorization**: Add appropriate authorization controls

### 4. Maintenance and Debug
- **Logging**: Add adequate logging
- **Error Messages**: Use understandable error messages
- **Documentation**: Document the workflow
- **Testing**: Write unit tests

### 5. Version Management
- **Semantic Versioning**: Follow SemVer standard
- **Breaking Changes**: Be careful with major version changes
- **Backward Compatibility**: Maintain backward compatibility
- **Migration Strategy**: Plan data migration

## Examples

### Simple Approval Process
```json
{
  "type": "F",
  "startTransition": {
    "key": "start",
    "target": "pending",
    "triggerType": 0
  },
  "states": [
    {
      "key": "pending",
      "stateType": 1,
      "transitions": [
        {
          "key": "approve",
          "target": "approved",
          "triggerType": 0
        },
        {
          "key": "reject", 
          "target": "rejected",
          "triggerType": 0
        }
      ]
    },
    {
      "key": "approved",
      "stateType": 3
    },
    {
      "key": "rejected",
      "stateType": 3
    }
  ]
}
```

### Complex E-commerce Process
For real e-commerce example: `samples/ecommerce/Workflows/ecommerce-workflow.json`

### OAuth Authentication
For real OAuth example: `samples/oauth/Workflows/oauth-authentication-workflow.json`

### Scheduled Payment Process  
For real payment example: `samples/payments/Workflows/scheduled-payments-workflow.json`

## Common Errors

### 1. Missing Initial State
```
Error: Workflow must have exactly one Initial state
```
**Solution**: Define only one `stateType: "1"`

### 2. Invalid Transition Target
```
Error: Transition target 'invalid-state' not found
```
**Solution**: Ensure target state is defined in states array

### 3. Missing Start Transition
```
Error: Workflow must have startTransition
```
**Solution**: Define `startTransition` component

### 4. Invalid JSON Schema
```
Error: JSON schema validation failed
```
**Solution**: Check JSON format and schema compliance

## Related Documentation

- [📄 State Documentation](./state.md)
- [📄 Task Documentation](./task.md)  
- [📄 Transition Documentation](./transition.md)
- [📄 Interface Documentation](./interface.md)
- [📄 Mapping Documentation](./mapping.md)

This documentation provides a comprehensive guide for workflow definition. Developers can create effective and efficient workflows by following this guide.
