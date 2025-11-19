# vNext Runtime Platform - Release Notes v0.0.20
**Release Date:** November 19, 2025

## 🧭 Overview
This release introduces a powerful new **TriggerTask** capability that unifies instance management operations, alongside critical bug fixes for **InstanceData immutability** and **subflow pipeline progression**. The new TriggerTask enables workflows to start instances, trigger transitions, launch subprocesses, and fetch instance data—all within the workflow execution context.

---

## 🚀 Major Updates

### 1. TriggerTask - Unified Instance Management (Task Type 11)
A new versatile task type that provides comprehensive workflow instance control directly from within workflow execution. TriggerTask consolidates four distinct operation types into a single, flexible task definition.

**Key Capabilities:**
- **Start New Instances**: Launch new workflow instances with custom data
- **Trigger Transitions**: Execute transitions on existing instances (direct or correlation-based)
- **Launch SubProcesses**: Start independent subprocess instances
- **Fetch Instance Data**: Retrieve instance data with extension support

**Task Type:** `11` (TaskType.TriggerTransition)

#### Trigger Types

##### 1. Start Instance (TriggerType: Start = 1)
Creates a new workflow instance within the workflow execution flow.

**Example Task Definition:**
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

**Example Mapping:**
```csharp
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class StartApprovalMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var triggerTask = task as TriggerTransitionTask;
        
        // Set the workflow to start
        triggerTask.SetDomain("approvals");
        triggerTask.SetFlow("approval-flow");
        triggerTask.SetKey("document-approval");
        triggerTask.SetTriggerType("Start");
        
        // Prepare initialization data
        triggerTask.SetBody(new {
            documentId = context.Instance.Data.documentId,
            requestedBy = context.Instance.Data.userId,
            approvalLevel = "L1",
            priority = "HIGH",
            requestedAt = DateTime.UtcNow
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
                startedAt = DateTime.UtcNow
            };
        }
        else
        {
            response.Data = new
            {
                approvalStarted = false,
                error = context.Body.errorMessage ?? "Failed to start approval workflow"
            };
        }
        
        return response;
    }
}
```

##### 2. Trigger Transition (TriggerType: Trigger = 2)
Executes a specific transition on an existing workflow instance.

**Example Task Definition:**
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

**Example Mapping:**
```csharp
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class TriggerApprovalMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var triggerTask = task as TriggerTransitionTask;
        
        // Set target instance and transition
        triggerTask.SetInstance(context.Instance.Data.approvalInstanceId);
        triggerTask.SetTriggerType("Trigger");
        
        // Prepare transition data
        triggerTask.SetBody(new {
            approvedBy = context.Instance.Data.currentUser,
            approvalDate = DateTime.UtcNow,
            comments = context.Instance.Data.approvalComments ?? "Approved",
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
                triggeredAt = DateTime.UtcNow
            };
        }
        else
        {
            response.Data = new
            {
                transitionTriggered = false,
                error = context.Body.errorMessage
            };
        }
        
        return response;
    }
}
```

##### 3. Launch SubProcess (TriggerType: SubProcess = 3)
Starts an independent subprocess instance that runs in parallel with the main workflow.

**Example Task Definition:**
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

**Example Mapping:**
```csharp
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
        
        // Prepare subprocess data
        triggerTask.SetBody(new {
            transactionId = context.Instance.Data.transactionId,
            userId = context.Instance.Data.userId,
            action = context.Instance.Data.action,
            timestamp = DateTime.UtcNow,
            parentInstanceId = context.Instance.Id,
            correlationId = context.Instance.CorrelationId
        });
        
        return Task.FromResult(new ScriptResponse
        {
            Data = context.Instance.Data
        });
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var response = new ScriptResponse();
        
        // SubProcess is fire-and-forget, just track that it was initiated
        response.Data = new
        {
            auditSubProcessId = context.Body.data?.instanceId,
            auditInitiated = true,
            initiatedAt = DateTime.UtcNow
        };
        
        return response;
    }
}
```

##### 4. Get Instance Data (TriggerType: GetInstanceData = 4)
Retrieves instance data from another workflow, with optional extension support.

**Example Task Definition:**
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

**Example Mapping:**
```csharp
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
                    name = instanceData.profile?.name,
                    email = instanceData.profile?.email,
                    preferences = instanceData.preferences,
                    securitySettings = instanceData.security
                },
                dataFetchedAt = DateTime.UtcNow
            };
        }
        else
        {
            response.Data = new
            {
                error = "Failed to fetch user profile data",
                errorMessage = context.Body.errorMessage
            };
        }
        
        return response;
    }
}
```

#### TriggerTask Properties

| Property | Type | Description |
|----------|------|-------------|
| `TransitionName` | string? | Transition name to execute (required for Trigger type) |
| `Body` | JsonElement? | Body data to send with the request |
| `TriggerDomain` | string | Domain of the target workflow (required) |
| `TriggerFlow` | string | Flow name of the target workflow (required) |
| `TriggerKey` | string? | Flow key of the target workflow |
| `TriggerInstanceId` | string? | InstanceId of the target workflow |
| `TriggerType` | TriggerTransitionType | Type of trigger operation |
| `TriggerVersion` | string? | SubFlow version (optional) |
| `Extensions` | string[]? | Extensions to request for GetInstanceData (optional) |

#### TriggerTask Methods

```csharp
void SetBody(dynamic body)              // Set request body data
void SetInstance(string instanceId)     // Set target instance ID
void SetKey(string key)                 // Set workflow key
void SetDomain(string domain)           // Set workflow domain
void SetFlow(string flow)               // Set workflow flow name
void SetTriggerType(string type)        // Set trigger type
```

> **Reference:** [#101 - Transition Triggering Task Development](https://github.com/burgan-tech/vnext/issues/101)
> 
> **Reference:** [#100 - Multiple SubProcess Launch Task Development](https://github.com/burgan-tech/vnext/issues/100)
> 
> **Reference:** [#142 - Instance Start and Get Instance Data Task Development](https://github.com/burgan-tech/vnext/issues/101)

---

## 🧩 Bug Fixes

### 1. InstanceData Immutability and Merge Logic (#160)
Fixed critical issue where the `AddData` method in workflow instances could break immutability assumptions and create data inconsistencies.

**Problem Resolved:**
- When `lastData` exists and `versionStrategy` is null, the method was creating a new `InstanceData` with the same version as `lastData`
- This effectively mutated the latest state without producing a proper new version, violating immutability and history integrity
- No database-level protection existed to prevent duplicate/latest collisions

**Technical Implementation:**
- Modified `AddData` to always call `lastData.NewVersion()` when `lastData` exists and incoming data differs
- Default behavior now uses `VersionStrategy.IncreaseMinor` when no explicit strategy is provided
- Added unique index on `(InstanceId, Version, HistorySequence, IsLatest)` to guarantee consistency at database level

**Before (Problematic Code):**
```csharp
newData = versionStrategy is null
    ? new InstanceData(
        id,
        Id,
        lastData.Version,        // Same version - breaks immutability!
        inputData,
        true,
        GetNextHistorySequence(lastData.Version)
    )
    : lastData.NewVersion(
        id,
        inputData,
        versionStrategy ?? VersionStrategy.IncreaseMinor,
        0
    );
```

**After (Fixed Code):**
```csharp
// Always use NewVersion when lastData exists to maintain immutability
newData = lastData.NewVersion(
    id,
    inputData,
    versionStrategy ?? VersionStrategy.IncreaseMinor,
    0
);
```

**Impact:**
- Proper version history maintenance for all instance data changes
- Prevention of data corruption through database constraints
- Consistent behavior across all data update scenarios
- Improved data integrity and audit trail reliability

> **Reference:** [#160 - AddData should always call NewVersion when lastData exists](https://github.com/burgan-tech/vnext/issues/160)

---

### 2. Subflow Manual Transition Pipeline Progression (#161)
Fixed issue where the main workflow pipeline would halt after a subflow completes when a manual transition is executed within the subflow.

**Problem Resolved:**
- When a subflow finishes and notifies the parent flow, the parent pipeline was unable to resume if no transition was explicitly defined
- The pipeline resume process assumed a transition was mandatory for progression
- The main flow remained stuck at the subflow state indefinitely

**Technical Implementation:**
- Updated pipeline resume logic to make transitions optional after subflow completion
- When no transition exists and the state has completed successfully, the pipeline:
  - Marks the subflow state as completed
  - Continues pipeline execution automatically from the next logical step
  - Preserves existing behavior for states with explicitly defined transitions

**Expected Behavior:**
1. Main flow enters a subflow state
2. Subflow executes and completes successfully
3. Subflow notifies main flow
4. Main flow resumes automatically (even without explicit transition)
5. Main flow continues normal execution

**Actual Behavior (Fixed):**
- Resume process no longer fails when transition is missing after subflow completion
- Main flow continues automatically when subflow state completes
- Explicit transitions still work as before (backward compatible)

**Use Case Example:**
```json
{
  "key": "process-order-state",
  "stateType": 4,
  "subFlowReference": {
    "domain": "payments",
    "flow": "payment-processing",
    "key": "process-payment"
  },
  "transitions": []  // No transition needed - auto-continues after subflow
}
```

**Impact:**
- Simplified subflow state definitions (transitions now optional)
- Automatic flow continuation after subflow completion
- Better workflow execution reliability
- Backward compatible with existing explicit transitions

> **Reference:** [#161 - Subflow Manual Transition Pipeline Progression Fix](https://github.com/burgan-tech/vnext/issues/161)

---

## 🔧 Configuration Updates
Configuration for v0.0.20:
```json
{
  "runtimeVersion": "0.0.20",
  "schemaVersion": "0.0.25"
}
```

---

## 🧱 Issues Referenced
- [#101 - Transition Triggering Task Development](https://github.com/burgan-tech/vnext/issues/101)
- [#100 - Multiple SubProcess Launch Task Development](https://github.com/burgan-tech/vnext/issues/100)
- [#142 - Instance Start and Get Instance Data Task Development](https://github.com/burgan-tech/vnext/issues/101)
- [#160 - AddData should always call NewVersion when lastData exists](https://github.com/burgan-tech/vnext/issues/160)
- [#161 - Subflow Manual Transition Pipeline Progression Fix](https://github.com/burgan-tech/vnext/issues/161)

---

## 📘 Developer Notes

### New Task Type: TriggerTask
TriggerTask (Type 11) is now available for workflow orchestration. This unified task type enables:
- **Instance Lifecycle Management**: Start new instances programmatically
- **Workflow Orchestration**: Trigger transitions across instances
- **Parallel Processing**: Launch independent subprocesses
- **Data Retrieval**: Fetch instance data with extension support

### Migration Checklist
- [ ] Review workflows for potential TriggerTask usage opportunities
- [ ] Update instance data management code if relying on version behavior
- [ ] Test subflow completions that previously required explicit transitions
- [ ] Verify database migrations for InstanceData unique constraint
- [ ] Update workflow definitions to schema version 0.0.25

### New Capabilities to Explore
- **Workflow Composition:** Use TriggerTask to build complex multi-workflow processes
- **Dynamic Workflow Execution:** Start instances based on runtime conditions
- **Cross-Workflow Communication:** Trigger transitions in external instances
- **Subprocess Orchestration:** Launch multiple parallel subprocesses with different configurations
- **Instance Data Federation:** Aggregate data from multiple workflow instances

---

## 🧠 Summary
With this release:
✅ TriggerTask provides unified workflow instance management capabilities  
✅ Four distinct trigger types support diverse orchestration scenarios  
✅ InstanceData immutability and version history properly maintained  
✅ Subflow pipeline progression works reliably without mandatory transitions  
✅ Database constraints ensure data integrity  
✅ Backward compatibility preserved for existing workflows

---

## 🔄 Upgrade Path

### From v0.0.19 to v0.0.20:

1. **Update Runtime:**
   ```bash
   # Update to v0.0.20
   git pull origin master
   ```

2. **Database Migration:**
   - Apply unique index on InstanceData table:
   ```sql
   CREATE UNIQUE INDEX IX_InstanceData_InstanceId_Version_HistorySequence_IsLatest 
   ON InstanceData (InstanceId, Version, HistorySequence, IsLatest);
   ```

3. **Update Configuration:**
   ```json
   {
     "runtimeVersion": "0.0.20",
     "schemaVersion": "0.0.25"
   }
   ```

4. **Explore TriggerTask:**
   - Review workflow orchestration needs
   - Implement TriggerTask for cross-workflow operations
   - Test instance management scenarios

5. **Verify Subflow Behavior:**
   - Test subflow completions in existing workflows
   - Remove unnecessary transitions after subflow states (optional)
   - Validate automatic continuation behavior

---

**vNext Runtime Platform Team**  
November 19, 2025

