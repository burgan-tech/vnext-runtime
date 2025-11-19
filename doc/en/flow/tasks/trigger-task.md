# Trigger Task

Trigger Task is a unified task type that provides comprehensive workflow instance management capabilities. It enables workflows to start new instances, trigger transitions, launch subprocesses, and fetch instance data—all within the workflow execution context.

## Features

- ✅ Start new workflow instances programmatically
- ✅ Trigger transitions on existing instances (direct or correlation-based)
- ✅ Launch independent subprocess instances
- ✅ Fetch instance data with extension support
- ✅ Cross-workflow orchestration
- ✅ Dynamic workflow composition
- ✅ Flexible trigger type configuration
- ✅ Detailed response tracking

## Task Definition

### Basic Structure

```json
{
  "key": "trigger-workflow-action",
  "flow": "sys-tasks",
  "domain": "core",
  "version": "1.0.0",
  "tags": [
    "trigger",
    "workflow",
    "orchestration"
  ],
  "attributes": {
    "type": "11",
    "config": {
      "type": "Start",
      "domain": "target-domain",
      "flow": "target-flow",
      "key": "target-key",
      "version": "1.0.0"
    }
  }
}
```

### Configuration Fields

The following fields are defined in the config section of Trigger Task:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `type` | string | Yes | Trigger type: "Start", "Trigger", "SubProcess", "GetInstanceData" |
| `domain` | string | Yes | Target workflow domain |
| `flow` | string | Yes | Target workflow flow name |
| `key` | string | Conditional | Target workflow key (required for Start, SubProcess) |
| `instanceId` | string | Conditional | Target instance ID (required for Trigger, GetInstanceData) |
| `transitionName` | string | Conditional | Transition name to execute (required for Trigger type) |
| `version` | string | No | Target workflow version (optional) |
| `extensions` | string[] | No | Extensions to fetch (for GetInstanceData type) |
| `body` | object | No | Data to send with the request |

## Trigger Types

Trigger Task supports four distinct operation types through the `TriggerTransitionType` enum:

### 1. Start Instance (Type: Start = 1)

Creates a new workflow instance. Use this trigger type to programmatically start new workflow instances during workflow execution.

**Configuration Example:**
```json
{
  "key": "start-approval-workflow",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-tasks",
  "tags": ["workflow", "instance", "start"],
  "attributes": {
    "type": "11",
    "config": {
      "type": "Start",
      "domain": "approvals",
      "flow": "approval-flow",
      "key": "document-approval",
      "version": "1.0.0"
    }
  }
}
```

**Use Cases:**
- Starting approval workflows from main business processes
- Creating audit workflows for transaction logging
- Initiating notification workflows
- Launching parallel processing workflows

### 2. Trigger Transition (Type: Trigger = 2)

Executes a specific transition on an existing workflow instance. Use this to trigger state transitions in other workflow instances.

**Configuration Example:**
```json
{
  "key": "trigger-approval-action",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-tasks",
  "tags": ["transition", "trigger"],
  "attributes": {
    "type": "11",
    "config": {
      "type": "Trigger",
      "domain": "approvals",
      "flow": "approval-flow",
      "transitionName": "approve"
    }
  }
}
```

**Use Cases:**
- Approving or rejecting workflows from external systems
- Triggering status updates in dependent workflows
- Coordinating multi-workflow processes
- Implementing workflow callbacks

### 3. Launch SubProcess (Type: SubProcess = 3)

Starts an independent subprocess instance that runs in parallel with the main workflow. Subprocesses are fire-and-forget operations that don't block the parent workflow.

**Configuration Example:**
```json
{
  "key": "start-audit-subprocess",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-tasks",
  "tags": ["subprocess", "audit"],
  "attributes": {
    "type": "11",
    "config": {
      "type": "SubProcess",
      "domain": "audit",
      "flow": "audit-flow",
      "key": "transaction-audit",
      "version": "1.0.0"
    }
  }
}
```

**Use Cases:**
- Background audit logging
- Asynchronous notification sending
- Parallel data processing
- Independent reporting workflows
- Event-driven side effects

**SubProcess vs SubFlow:**
- **SubProcess**: Fire-and-forget, runs independently, doesn't block parent
- **SubFlow**: Blocks parent, returns data, tightly integrated

### 4. Get Instance Data (Type: GetInstanceData = 4)

Retrieves instance data from another workflow instance. Supports optional extensions to fetch additional related data.

**Configuration Example:**
```json
{
  "key": "get-user-profile-data",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-tasks",
  "tags": ["instance", "data", "fetch"],
  "attributes": {
    "type": "11",
    "config": {
      "type": "GetInstanceData",
      "domain": "users",
      "flow": "user-profile",
      "extensions": ["profile", "preferences", "security"]
    }
  }
}
```

**Use Cases:**
- Fetching user profile data for personalization
- Retrieving configuration from central workflows
- Loading reference data from master workflows
- Aggregating data from multiple workflow instances
- Cross-workflow data federation

## Property Access

Properties in the TriggerTransitionTask class are accessed through setter methods:

- **TriggerDomain**: Set with `SetDomain(string domain)` method
- **TriggerFlow**: Set with `SetFlow(string flow)` method
- **TriggerKey**: Set with `SetKey(string key)` method
- **TriggerInstanceId**: Set with `SetInstance(string instanceId)` method
- **TriggerType**: Set with `SetTriggerType(string type)` method
- **Body**: Set with `SetBody(dynamic body)` method

## Mapping Examples

### Example 1: Start New Instance

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class StartApprovalMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var triggerTask = task as TriggerTransitionTask;
        
        // Configure target workflow
        triggerTask.SetDomain("approvals");
        triggerTask.SetFlow("approval-flow");
        triggerTask.SetKey("document-approval");
        triggerTask.SetTriggerType("Start");
        
        // Prepare initialization data
        triggerTask.SetBody(new {
            documentId = context.Instance.Data.documentId,
            documentType = context.Instance.Data.documentType,
            requestedBy = context.Instance.Data.userId,
            approvalLevel = "L1",
            priority = "HIGH",
            requestedAt = DateTime.UtcNow,
            metadata = new {
                sourceInstanceId = context.Instance.Id,
                correlationId = context.Instance.CorrelationId
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

### Example 2: Trigger Transition

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class TriggerApprovalMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var triggerTask = task as TriggerTransitionTask;
        
        // Set target instance
        triggerTask.SetInstance(context.Instance.Data.approvalInstanceId);
        triggerTask.SetTriggerType("Trigger");
        
        // Prepare transition payload
        triggerTask.SetBody(new {
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

### Example 3: Launch SubProcess

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class StartAuditSubProcessMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var triggerTask = task as TriggerTransitionTask;
        
        // Configure subprocess
        triggerTask.SetDomain("audit");
        triggerTask.SetFlow("audit-flow");
        triggerTask.SetKey("transaction-audit");
        triggerTask.SetTriggerType("SubProcess");
        
        // Prepare subprocess initialization data
        triggerTask.SetBody(new {
            transactionId = context.Instance.Data.transactionId,
            transactionType = context.Instance.Data.transactionType,
            amount = context.Instance.Data.amount,
            currency = context.Instance.Data.currency,
            userId = context.Instance.Data.userId,
            action = context.Instance.Data.action,
            timestamp = DateTime.UtcNow,
            parentInstanceId = context.Instance.Id,
            correlationId = context.Instance.CorrelationId,
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

### Example 4: Get Instance Data

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class GetUserProfileDataMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var triggerTask = task as TriggerTransitionTask;
        
        // Set target instance to fetch data from
        triggerTask.SetInstance(context.Instance.Data.userProfileInstanceId);
        triggerTask.SetTriggerType("GetInstanceData");
        
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

## Standard Response

Trigger Task returns the following standard response structure:

```csharp
{
    "Data": {
        "instanceId": "guid",           // For Start and SubProcess types
        "currentState": "state-name",   // For Trigger type
        "data": { /* instance data */ } // For GetInstanceData type
    },
    "IsSuccess": true,
    "ErrorMessage": null,
    "Metadata": {
        "TriggerType": "Start",
        "TargetDomain": "approvals",
        "TargetFlow": "approval-flow"
    },
    "ExecutionDurationMs": 145,
    "TaskType": "TriggerTransition"
}
```

### Successful Response by Type

#### Start Instance Response
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

#### Trigger Transition Response
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

#### SubProcess Response
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

#### GetInstanceData Response
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

### 1. Trigger Type Selection
```csharp
// ✅ Correct - Use Start for new instances
triggerTask.SetTriggerType("Start");
triggerTask.SetKey("new-workflow");

// ✅ Correct - Use Trigger for existing instances
triggerTask.SetTriggerType("Trigger");
triggerTask.SetInstance(existingInstanceId);

// ❌ Wrong - Don't use Start with instanceId
triggerTask.SetTriggerType("Start");
triggerTask.SetInstance(existingInstanceId); // This will fail
```

### 2. Data Preparation
```csharp
// ✅ Correct - Provide complete data for SubProcess
triggerTask.SetBody(new {
    // All required data for independent execution
    userId = context.Instance.Data.userId,
    transactionId = context.Instance.Data.transactionId,
    parentInstanceId = context.Instance.Id,
    correlationId = context.Instance.CorrelationId
});

// ❌ Wrong - Incomplete data for SubProcess
triggerTask.SetBody(new {
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

### 4. SubProcess vs Start Instance
```csharp
// ✅ Use SubProcess for fire-and-forget background tasks
// Parent workflow doesn't need to wait for completion
triggerTask.SetTriggerType("SubProcess");

// ✅ Use Start for workflows that may need future interaction
// You get instanceId back and can trigger transitions later
triggerTask.SetTriggerType("Start");
```

### 5. Extensions Usage
```csharp
// ✅ Correct - Request only needed extensions
var triggerTask = task as TriggerTransitionTask;
// Extensions are already configured in task definition
// No need to set them again in mapping

// ✅ Best Practice - Define extensions in task config
// Task JSON:
{
  "config": {
    "type": "GetInstanceData",
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
        var triggerTask = task as TriggerTransitionTask;
        
        triggerTask.SetTriggerType("Start");
        triggerTask.SetDomain("approvals");
        triggerTask.SetFlow("multi-stage-approval");
        triggerTask.SetKey("document-approval");
        
        triggerTask.SetBody(new {
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
        var triggerTask = task as TriggerTransitionTask;
        
        // Trigger commit on payment workflow
        triggerTask.SetInstance(context.Instance.Data.paymentInstanceId);
        triggerTask.SetTriggerType("Trigger");
        
        triggerTask.SetBody(new {
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
        var triggerTask = task as TriggerTransitionTask;
        
        triggerTask.SetTriggerType("SubProcess");
        triggerTask.SetDomain("audit");
        triggerTask.SetFlow("audit-trail");
        triggerTask.SetKey("transaction-audit");
        
        triggerTask.SetBody(new {
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

## Common Problems

### Problem: Trigger Type Mismatch
**Solution:** Ensure the trigger type matches the operation. Use "Start" for new instances, "Trigger" for transitions.

### Problem: Missing Required Fields
**Solution:** Verify that domain and flow are always provided. Key is required for Start/SubProcess, instanceId for Trigger/GetInstanceData.

### Problem: SubProcess Not Independent
**Solution:** Provide complete data in the body. SubProcesses don't have access to parent workflow data after launch.

### Problem: Extensions Not Loading
**Solution:** Ensure extensions are defined in the task configuration, not in the mapping code.

