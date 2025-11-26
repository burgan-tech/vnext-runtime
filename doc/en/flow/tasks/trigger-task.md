# Trigger Task Types

Four separate task types are available for workflow instance management. Each task type supports a different workflow interaction scenario and has its own type number.

## Task Types

- **StartTask** (Type: "11") - Starts new workflow instances
- **DirectTriggerTask** (Type: "12") - Triggers transitions on existing instances
- **GetInstanceDataTask** (Type: "13") - Retrieves instance data
- **SubProcessTask** (Type: "14") - Launches independent subprocess instances

## Features

- ✅ Start new workflow instances programmatically
- ✅ Trigger transitions on existing instances (direct or key-based)
- ✅ Launch independent subprocess instances
- ✅ Fetch instance data with extension support
- ✅ Cross-workflow orchestration
- ✅ Dynamic workflow composition
- ✅ Detailed response tracking

## 1. StartTask (Type: "11")

Creates a new workflow instance. Use this to programmatically start new workflow instances during workflow execution.

### Task Definition

```json
{
  "key": "start-approval-workflow",
  "flow": "sys-tasks",
  "domain": "core",
  "version": "1.0.0",
  "tags": ["workflow", "instance", "start"],
  "attributes": {
    "type": "11",
    "config": {
      "domain": "approvals",
      "flow": "approval-flow",
      "body": {
        "documentId": "doc-12345",
        "requestedBy": "user-123"
      }
    }
  }
}
```

### Configuration Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `domain` | string | Yes | Target workflow domain |
| `flow` | string | Yes | Target workflow flow name |
| `body` | object | No | Data to send with the request |

### Use Cases

- Starting approval workflows from main business processes
- Creating audit workflows for transaction logging
- Initiating notification workflows
- Launching parallel processing workflows

### Mapping Example

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class StartApprovalMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var startTask = task as StartTask;
        
        // Configure target workflow
        startTask.SetDomain("approvals");
        startTask.SetFlow("approval-flow");
        
        // Prepare initialization data
        startTask.SetBody(new {
            documentId = context.Instance.Data.documentId,
            documentType = context.Instance.Data.documentType,
            requestedBy = context.Instance.Data.userId,
            approvalLevel = "L1",
            priority = "HIGH",
            requestedAt = DateTime.UtcNow,
            metadata = new {
                sourceInstanceId = context.Instance.Id,
            }
        });
        
        return Task.FromResult(new ScriptResponse
        {
            Data = context.Instance.Data
        });
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var response = new ScriptResponse();
        
        if (context.Body.isSuccess)
        {
            response.Data = new
            {
                approvalInstanceId = context.Body.data.instanceId,
                approvalStarted = true,
                startedAt = DateTime.UtcNow,
                status = "APPROVAL_INITIATED"
            };
        }
        else
        {
            response.Data = new
            {
                approvalStarted = false,
                error = context.Body.errorMessage ?? "Failed to start approval workflow",
                shouldRetry = true
            };
        }
        
        return response;
    }
}
```

### Successful Response

```json
{
  "isSuccess": true,
  "data": {
    "instanceId": "550e8400-e29b-41d4-a716-446655440000",
    "state": "initial-state",
    "createdAt": "2025-11-19T10:30:00Z"
  }
}
```

## 2. DirectTriggerTask (Type: "12")

Executes a specific transition on an existing workflow instance. Use this to trigger state transitions in other workflow instances.

### Task Definition

```json
{
  "key": "trigger-approval-action",
  "flow": "sys-tasks",
  "domain": "core",
  "version": "1.0.0",
  "tags": ["transition", "trigger"],
  "attributes": {
    "type": "12",
    "config": {
      "domain": "approvals",
      "flow": "approval-flow",
      "transitionName": "approve",
      "instanceId": "550e8400-e29b-41d4-a716-446655440000",
      "body": {
        "approvedBy": "manager123",
        "approvalDate": "2024-01-15T10:30:00Z"
      }
    }
  }
}
```

### Configuration Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `domain` | string | Yes | Target workflow domain |
| `flow` | string | Yes | Target workflow flow name |
| `transitionName` | string | Yes | Transition name to execute |
| `key` | string | Conditional | Target instance key (used if instanceId is not provided) |
| `instanceId` | string | Conditional | Target instance ID (takes priority) |
| `body` | object | No | Data to send with the request |

**Note:** Either `instanceId` or `key` must be provided. `instanceId` takes priority. If neither is provided, the current instance ID is used.

### Use Cases

- Approving or rejecting workflows from external systems
- Triggering status updates in dependent workflows
- Coordinating multi-workflow processes
- Implementing workflow callbacks

### Mapping Example

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class TriggerApprovalMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var directTriggerTask = task as DirectTriggerTask;
        
        // Set target instance
        directTriggerTask.SetInstance(context.Instance.Data.approvalInstanceId);
        directTriggerTask.SetTransitionName("approve");
        
        // Prepare transition payload
        directTriggerTask.SetBody(new {
            action = "approve",
            approvedBy = context.Instance.Data.currentUser,
            approvalDate = DateTime.UtcNow,
            comments = context.Instance.Data.approvalComments ?? "Approved",
            signature = context.Instance.Data.digitalSignature,
            status = "APPROVED"
        });
        
        return Task.FromResult(new ScriptResponse
        {
            Data = context.Instance.Data
        });
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var response = new ScriptResponse();
        
        if (context.Body.isSuccess)
        {
            response.Data = new
            {
                transitionTriggered = true,
                triggeredAt = DateTime.UtcNow,
                newState = context.Body.data?.currentState,
                status = "TRANSITION_SUCCESS"
            };
        }
        else
        {
            response.Data = new
            {
                transitionTriggered = false,
                error = context.Body.errorMessage,
                status = "TRANSITION_FAILED"
            };
        }
        
        return response;
    }
}
```

### Successful Response

```json
{
  "isSuccess": true,
  "data": {
    "instanceId": "550e8400-e29b-41d4-a716-446655440000",
    "currentState": "approved",
    "transitionExecuted": "approve",
    "executedAt": "2025-11-19T10:30:00Z"
  }
}
```

## 3. GetInstanceDataTask (Type: "13")

Retrieves instance data from another workflow instance. Supports optional extensions to fetch additional related data.

### Task Definition

```json
{
  "key": "get-user-profile-data",
  "flow": "sys-tasks",
  "domain": "core",
  "version": "1.0.0",
  "tags": ["instance", "data", "fetch"],
  "attributes": {
    "type": "13",
    "config": {
      "domain": "users",
      "flow": "user-profile",
      "instanceId": "660e8400-e29b-41d4-a716-446655440001",
      "extensions": ["profile", "preferences", "security"]
    }
  }
}
```

### Configuration Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `domain` | string | Yes | Target workflow domain |
| `flow` | string | Yes | Target workflow flow name |
| `key` | string | Conditional | Target instance key (used if instanceId is not provided, used directly as key) |
| `instanceId` | string | Conditional | Target instance ID (takes priority) |
| `extensions` | string[] | No | Extensions to fetch |

**Note:** Either `instanceId` or `key` must be provided. `instanceId` takes priority. If neither is provided, the current instance ID is used.

### Use Cases

- Fetching user profile data for personalization
- Retrieving configuration from central workflows
- Loading reference data from master workflows
- Aggregating data from multiple workflow instances
- Cross-workflow data federation

### Mapping Example

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class GetUserProfileDataMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var getDataTask = task as GetInstanceDataTask;
        
        // Set target instance to fetch data from
        getDataTask.SetInstance(context.Instance.Data.userProfileInstanceId);
        
        // Set extensions (optional)
        getDataTask.SetExtensions(new[] { "profile", "preferences", "security" });
        
        return Task.FromResult(new ScriptResponse
        {
            Data = context.Instance.Data
        });
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var response = new ScriptResponse();
        
        if (context.Body.isSuccess)
        {
            var instanceData = context.Body.data;
            
            response.Data = new
            {
                userProfile = new
                {
                    userId = instanceData.userId,
                    name = instanceData.profile?.name,
                    email = instanceData.profile?.email,
                    phone = instanceData.profile?.phone,
                    preferences = instanceData.preferences,
                    securitySettings = instanceData.security,
                    accountType = instanceData.profile?.accountType
                },
                dataFetchedAt = DateTime.UtcNow,
                status = "PROFILE_DATA_LOADED"
            };
        }
        else
        {
            response.Data = new
            {
                error = "Failed to fetch user profile data",
                errorMessage = context.Body.errorMessage,
                shouldRetry = false,
                status = "PROFILE_DATA_LOAD_FAILED"
            };
        }
        
        return response;
    }
}
```

### Successful Response

```json
{
  "isSuccess": true,
  "data": {
    "userId": "user123",
    "profile": {
      "name": "John Doe",
      "email": "john.doe@example.com"
    },
    "preferences": {
      "language": "en-US",
      "theme": "dark"
    }
  }
}
```

## 4. SubProcessTask (Type: "14")

Starts an independent subprocess instance that runs in parallel with the main workflow. Subprocesses are fire-and-forget operations that don't block the parent workflow.

### Task Definition

```json
{
  "key": "start-audit-subprocess",
  "flow": "sys-tasks",
  "domain": "core",
  "version": "1.0.0",
  "tags": ["subprocess", "audit"],
  "attributes": {
    "type": "14",
    "config": {
      "domain": "audit",
      "key": "transaction-audit",
      "version": "1.0.0",
      "body": {
        "transactionId": "txn-12345",
        "userId": "user-123"
      }
    }
  }
}
```

### Configuration Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `domain` | string | Yes | Target workflow domain |
| `key` | string | Yes | Target workflow key |
| `version` | string | No | SubFlow version |
| `body` | object | No | Data to send with the request |

### Use Cases

- Background audit logging
- Asynchronous notification sending
- Parallel data processing
- Independent reporting workflows
- Event-driven side effects

**SubProcess vs SubFlow:**
- **SubProcess**: Fire-and-forget, runs independently, doesn't block parent
- **SubFlow**: Blocks parent, returns data, tightly integrated

### Mapping Example

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class StartAuditSubProcessMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var subProcessTask = task as SubProcessTask;
        
        // Configure subprocess
        subProcessTask.SetDomain("audit");
        subProcessTask.SetKey("transaction-audit");
        subProcessTask.SetVersion("1.0.0");
        
        // Prepare subprocess initialization data
        subProcessTask.SetBody(new {
            transactionId = context.Instance.Data.transactionId,
            transactionType = context.Instance.Data.transactionType,
            amount = context.Instance.Data.amount,
            currency = context.Instance.Data.currency,
            userId = context.Instance.Data.userId,
            action = context.Instance.Data.action,
            timestamp = DateTime.UtcNow,
            parentInstanceId = context.Instance.Id,
            auditDetails = new {
                ipAddress = context.Headers["x-forwarded-for"],
                userAgent = context.Headers["user-agent"],
                sessionId = context.Instance.Data.sessionId
            }
        });
        
        return Task.FromResult(new ScriptResponse
        {
            Data = context.Instance.Data
        });
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var response = new ScriptResponse();
        
        // SubProcess is fire-and-forget
        // Just track that it was initiated
        response.Data = new
        {
            auditSubProcessId = context.Body.data?.instanceId,
            auditInitiated = true,
            initiatedAt = DateTime.UtcNow,
            status = "AUDIT_SUBPROCESS_LAUNCHED"
        };
        
        return response;
    }
}
```

### Successful Response

```json
{
  "isSuccess": true,
  "data": {
    "instanceId": "660e8400-e29b-41d4-a716-446655440001",
    "state": "initial-state",
    "launched": true
  }
}
```

## Property Access

Each task type has its own setter methods:

### StartTask Setter Methods

- **SetDomain(string domain)**: Sets the target workflow domain
- **SetFlow(string flow)**: Sets the target workflow flow name
- **SetBody(dynamic body)**: Sets the request body

### DirectTriggerTask Setter Methods

- **SetDomain(string domain)**: Sets the target workflow domain
- **SetFlow(string flow)**: Sets the target workflow flow name
- **SetTransitionName(string transitionName)**: Sets the transition name to execute
- **SetInstance(string instanceId)**: Sets the target instance ID
- **SetKey(string key)**: Sets the target instance key (used if instanceId is not provided)
- **SetBody(dynamic body)**: Sets the request body

### GetInstanceDataTask Setter Methods

- **SetDomain(string domain)**: Sets the target workflow domain
- **SetFlow(string flow)**: Sets the target workflow flow name
- **SetInstance(string instanceId)**: Sets the target instance ID
- **SetKey(string key)**: Sets the target instance key (used if instanceId is not provided, used directly as key)
- **SetExtensions(string[] extensions)**: Sets the extensions to fetch

### SubProcessTask Setter Methods

- **SetDomain(string domain)**: Sets the target workflow domain
- **SetKey(string key)**: Sets the target workflow key
- **SetVersion(string version)**: Sets the SubFlow version
- **SetBody(dynamic body)**: Sets the request body

### Configuration vs Dynamic Setting

Required fields for tasks can be provided in **two ways**:

1. **Static Configuration**: Specified in the config section of the task JSON definition
2. **Dynamic Setting**: Set at runtime using setter methods in the InputHandler

**Priority Rule:** If the same field is defined in both JSON config and InputHandler mapping, **the value set in InputHandler takes precedence**. This allows dynamic runtime values to override static configuration.

**Usage Strategies:**

```csharp
// Scenario 1: Domain and flow defined in JSON, not overridden in mapping
// Task JSON: "config": { "domain": "approvals", "flow": "approval-flow" }
public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var startTask = task as StartTask;
    // Domain and flow already defined in config, no need to change
    startTask.SetBody(new { /* data */ });
    return Task.FromResult(new ScriptResponse());
}

// Scenario 2: No domain in JSON, set dynamically in mapping
// Task JSON: "config": { "flow": "approval-flow" }
public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var startTask = task as StartTask;
    // Domain determined dynamically at runtime
    var targetDomain = context.Instance.Data.approvalType == "document" 
        ? "document-approvals" 
        : "standard-approvals";
    startTask.SetDomain(targetDomain);
    startTask.SetBody(new { /* data */ });
    return Task.FromResult(new ScriptResponse());
}

// Scenario 3: No instanceId in JSON, retrieved from context in mapping
// Task JSON: "config": { "domain": "approvals", "flow": "approval-flow", "transitionName": "approve" }
public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var directTriggerTask = task as DirectTriggerTask;
    // Instance ID retrieved from workflow data
    directTriggerTask.SetInstance(context.Instance.Data.approvalInstanceId);
    directTriggerTask.SetBody(new { /* data */ });
    return Task.FromResult(new ScriptResponse());
}

// Scenario 4: instanceId in JSON, but overridden in mapping (Mapping takes priority!)
// Task JSON: "config": { "instanceId": "default-instance-id" }
public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var directTriggerTask = task as DirectTriggerTask;
    // Default value in JSON is overridden - mapping value is used!
    directTriggerTask.SetInstance(context.Instance.Data.targetInstanceId);
    directTriggerTask.SetBody(new { /* data */ });
    return Task.FromResult(new ScriptResponse());
}
```

## Standard Response

Each task type has its own response structure:

### StartTask Response

```json
{
  "isSuccess": true,
  "data": {
    "instanceId": "550e8400-e29b-41d4-a716-446655440000",
    "state": "initial-state",
    "createdAt": "2025-11-19T10:30:00Z"
  },
  "metadata": {
    "TaskType": "StartTrigger",
    "Domain": "approvals",
    "Flow": "approval-flow"
  }
}
```

### DirectTriggerTask Response

```json
{
  "isSuccess": true,
  "data": {
    "instanceId": "550e8400-e29b-41d4-a716-446655440000",
    "currentState": "approved",
    "transitionExecuted": "approve",
    "executedAt": "2025-11-19T10:30:00Z"
  },
  "metadata": {
    "TaskType": "DirectTrigger",
    "Domain": "approvals",
    "Flow": "approval-flow",
    "TransitionName": "approve"
  }
}
```

### GetInstanceDataTask Response

```json
{
  "isSuccess": true,
  "data": {
    "userId": "user123",
    "profile": {
      "name": "John Doe",
      "email": "john.doe@example.com"
    },
    "preferences": {
      "language": "en-US",
      "theme": "dark"
    }
  },
  "metadata": {
    "TaskType": "GetInstanceData",
    "Domain": "users",
    "Flow": "user-profile"
  }
}
```

### SubProcessTask Response

```json
{
  "isSuccess": true,
  "data": {
    "instanceId": "660e8400-e29b-41d4-a716-446655440001",
    "state": "initial-state",
    "launched": true
  },
  "metadata": {
    "TaskType": "SubProcess",
    "Domain": "audit",
    "Key": "transaction-audit"
  }
}
```

### Error Response

```json
{
  "isSuccess": false,
  "errorMessage": "Target workflow not found",
  "metadata": {
    "errorCode": "WORKFLOW_NOT_FOUND",
    "targetDomain": "approvals",
    "targetFlow": "approval-flow"
  }
}
```

## Error Scenarios

### Workflow Not Found

```json
{
  "IsSuccess": false,
  "ErrorMessage": "Workflow 'approval-flow' not found in domain 'approvals'",
  "Metadata": {
    "ErrorCode": "WORKFLOW_NOT_FOUND",
    "TargetDomain": "approvals",
    "TargetFlow": "approval-flow"
  }
}
```

### Instance Not Found

```json
{
  "IsSuccess": false,
  "ErrorMessage": "Instance '550e8400-e29b-41d4-a716-446655440000' not found",
  "Metadata": {
    "ErrorCode": "INSTANCE_NOT_FOUND",
    "InstanceId": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

### Transition Not Available

```json
{
  "IsSuccess": false,
  "ErrorMessage": "Transition 'approve' not available in current state",
  "Metadata": {
    "ErrorCode": "TRANSITION_NOT_AVAILABLE",
    "TransitionName": "approve",
    "CurrentState": "draft"
  }
}
```

## Best Practices

### 1. Task Type Selection

```csharp
// ✅ Correct - Use StartTask for new instances
var startTask = task as StartTask;
startTask.SetDomain("approvals");
startTask.SetFlow("approval-flow");

// ✅ Correct - Use DirectTriggerTask for transitions on existing instances
var directTriggerTask = task as DirectTriggerTask;
directTriggerTask.SetInstance(existingInstanceId);
directTriggerTask.SetTransitionName("approve");

// ❌ Wrong - Don't use StartTask with instanceId
var startTask = task as StartTask;
startTask.SetInstance(existingInstanceId); // StartTask doesn't have SetInstance method
```

### 2. Data Preparation

```csharp
// ✅ Correct - Provide complete data for SubProcessTask
var subProcessTask = task as SubProcessTask;
subProcessTask.SetBody(new {
    // All required data for independent execution
    userId = context.Instance.Data.userId,
    transactionId = context.Instance.Data.transactionId,
    parentInstanceId = context.Instance.Id,
});

// ❌ Wrong - Incomplete data for SubProcessTask
subProcessTask.SetBody(new {
    userId = context.Instance.Data.userId
    // Missing other required fields
});
```

### 3. Error Handling

```csharp
// ✅ Correct - Handle errors appropriately
public async Task<ScriptResponse> OutputHandler(ScriptContext context)
{
    var response = new ScriptResponse();
    
    if (context.Body.isSuccess)
    {
        response.Data = new {
            success = true,
            instanceId = context.Body.data.instanceId
        };
    }
    else
    {
        response.Data = new {
            success = false,
            error = context.Body.errorMessage,
            shouldRetry = ShouldRetryError(context.Body.errorMessage)
        };
    }
    
    return response;
}

private bool ShouldRetryError(string errorMessage)
{
    return errorMessage?.Contains("timeout") == true ||
           errorMessage?.Contains("unavailable") == true;
}
```

### 4. SubProcessTask vs StartTask

```csharp
// ✅ Use SubProcessTask for fire-and-forget background tasks
// Parent workflow doesn't need to wait for completion
var subProcessTask = task as SubProcessTask;
subProcessTask.SetDomain("audit");
subProcessTask.SetKey("transaction-audit");

// ✅ Use StartTask for workflows that may need future interaction
// You get instanceId back and can trigger transitions later with DirectTriggerTask
var startTask = task as StartTask;
startTask.SetDomain("approvals");
startTask.SetFlow("approval-flow");
```

### 5. Extensions Usage

```csharp
// ✅ Correct - Request only needed extensions
var getDataTask = task as GetInstanceDataTask;
getDataTask.SetExtensions(new[] { "profile", "preferences" });

// ✅ Best Practice - Define extensions in task config
// Task JSON:
{
  "config": {
    "domain": "users",
    "flow": "user-profile",
    "extensions": ["profile", "preferences"]
  }
}
```

## Common Use Cases

### Use Case 1: Multi-Stage Approval Workflow

```csharp
// Start approval workflow when document is submitted
public class StartApprovalWorkflow : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var startTask = task as StartTask;
        
        startTask.SetDomain("approvals");
        startTask.SetFlow("multi-stage-approval");
        
        startTask.SetBody(new {
            documentId = context.Instance.Data.documentId,
            approvalStages = new[] { "L1", "L2", "L3" },
            currentStage = 0,
            requester = context.Instance.Data.userId
        });
        
        return Task.FromResult(new ScriptResponse());
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        return new ScriptResponse
        {
            Data = new {
                approvalInstanceId = context.Body.data.instanceId,
                status = "APPROVAL_STARTED"
            }
        };
    }
}
```

### Use Case 2: Distributed Transaction Coordination

```csharp
// Trigger transitions in multiple workflow instances
public class CoordinateTransactionMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var directTriggerTask = task as DirectTriggerTask;
        
        // Trigger commit on payment workflow
        directTriggerTask.SetInstance(context.Instance.Data.paymentInstanceId);
        directTriggerTask.SetTransitionName("commit");
        
        directTriggerTask.SetBody(new {
            action = "commit",
            transactionId = context.Instance.Data.transactionId,
            timestamp = DateTime.UtcNow
        });
        
        return Task.FromResult(new ScriptResponse());
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        return new ScriptResponse
        {
            Data = new {
                commitSuccessful = context.Body.isSuccess,
                coordinationComplete = true
            }
        };
    }
}
```

### Use Case 3: Audit Trail Creation

```csharp
// Launch subprocess for audit logging
public class CreateAuditTrail : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var subProcessTask = task as SubProcessTask;
        
        subProcessTask.SetDomain("audit");
        subProcessTask.SetKey("transaction-audit");
        
        subProcessTask.SetBody(new {
            transactionId = context.Instance.Data.transactionId,
            userId = context.Instance.Data.userId,
            action = context.Instance.Data.action,
            timestamp = DateTime.UtcNow,
            details = context.Instance.Data
        });
        
        return Task.FromResult(new ScriptResponse());
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        // Fire-and-forget - just confirm launch
        return new ScriptResponse
        {
            Data = new { auditLaunched = true }
        };
    }
}
```

### Use Case 4: Fetch User Profile Data

```csharp
// Fetch user profile data
public class GetUserProfileMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var getDataTask = task as GetInstanceDataTask;
        
        getDataTask.SetInstance(context.Instance.Data.userProfileInstanceId);
        getDataTask.SetExtensions(new[] { "profile", "preferences", "security" });
        
        return Task.FromResult(new ScriptResponse());
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        if (context.Body.isSuccess)
        {
            return new ScriptResponse
            {
                Data = new {
                    userProfile = context.Body.data,
                    loadedAt = DateTime.UtcNow
                }
            };
        }
        
        return new ScriptResponse
        {
            Data = new {
                error = "Failed to fetch profile data",
                errorMessage = context.Body.errorMessage
            }
        };
    }
}
```

## Common Problems

### Problem: Wrong Task Type Usage
**Solution:** Each task type has its own purpose. Use StartTask for new instances, DirectTriggerTask for transitions.

### Problem: Missing Required Fields
**Solution:** Verify that domain and flow are always provided. DirectTriggerTask requires transitionName. SubProcessTask requires key.

### Problem: SubProcessTask Not Independent
**Solution:** Provide complete data in the body. SubProcessTask instances don't have access to parent workflow data after launch.

### Problem: Extensions Not Loading
**Solution:** Ensure extensions are defined in the task configuration or set via SetExtensions() in the mapping.

## Migration Notes

If you are using the old TriggerTransitionTask (type "11" with nested "type" field), you need to migrate to the new task types:

| Old Type | Old Nested Type | New Task Type | New Type Number |
|----------|-----------------|---------------|-----------------|
| "11" | "Start" | StartTask | "11" |
| "11" | "Trigger" | DirectTriggerTask | "12" |
| "11" | "GetInstanceData" | GetInstanceDataTask | "13" |
| "11" | "SubProcess" | SubProcessTask | "14" |

**Migration Example:**

**Old (TriggerTransitionTask):**
```json
{
  "type": "11",
  "config": {
    "type": "Start",
    "domain": "approvals",
    "flow": "approval-flow"
  }
}
```

**New (StartTask):**
```json
{
  "type": "11",
  "config": {
    "domain": "approvals",
    "flow": "approval-flow"
  }
}
```
