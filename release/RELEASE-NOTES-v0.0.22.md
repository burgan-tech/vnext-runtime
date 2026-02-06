# vNext Runtime Platform - Release Notes v0.0.22
**Release Date:** December 3, 2025

## 🧭 Overview
This release introduces the powerful **Cascade Cancel** feature for subflows, enabling automatic cancellation of all related active jobs, tasks, and correlations when a main instance is canceled. Additionally, this release includes **Inbox and Outbox Worker** infrastructure, enhanced **Function Filter** capabilities, **Instance ID and Key support** across all endpoints, and significant improvements to **Aspect/Trace/Logging** architecture. Critical bug fixes for async validation and nested JSON path filtering are also included.

> ⚠️ **Breaking Change:** The `subType` property has been added to the state schema. You need to update your existing workflows.

---

## 🚀 Major Updates

### 1. Cascade Cancel for Subflows on Main Instance Cancellation

A new `cancel` transition definition has been introduced for workflows. When a cancel transition is executed, all active jobs, tasks, and correlations associated with the flow are automatically canceled in a cascading manner.

**Key Features:**
- ✅ Automatic cancellation of all active subflows when main instance is canceled
- ✅ Recursive cancellation for nested subflows
- ✅ Idempotent operation (safe for repeated cancel requests)
- ✅ Proper status propagation across workflow hierarchy
- ✅ Incident and error tracking for cascade cancellation events

**Cancel Transition Definition:**
```json
{
  "cancel": {
    "key": "cancel-account-opening",
    "target": "cancelled",
    "triggerType": 0,
    "versionStrategy": "Minor",
    "labels": [
      {
        "language": "en-US",
        "label": "Cancel Account Opening"
      }
    ],
    "onExecutionTasks": [],
    "availableIn": []
  }
}
```

**Cancellation Flow:**
1. Main instance receives cancel request
2. All active subflows are identified
3. Cancel propagates recursively to nested subflows
4. Related tasks and correlations are canceled
5. Status updates are tracked for auditing

**Benefits:**
- Ensures workflow consistency by preventing orphaned flows
- Aligns with expected workflow lifecycle management
- Provides comprehensive audit trail for cancellation events
- Simplifies cleanup of complex workflow hierarchies

> **Documentation:** See [Cascade Cancel for Subflows](../docs/technical/flow/flow.md#cancel) for detailed implementation guide.

> **Reference:** [#62 - Feature: Cascade Cancel for Subflows on Main Instance Cancellation](https://github.com/burgan-tech/vnext/issues/62)

---

### 2. Inbox and Outbox Worker Images (NEW - INFRA)

New worker images have been introduced for better message processing, performance optimization, data loss prevention, and unified contract-based system operations.

**Architecture Overview:**
- **Dapr PubSub Integration**: Provides infrastructure isolation
- **Smart Message Routing**: Attempts PubSub delivery first; falls back to message box on failure
- **Custom Hook Support**: Execute hooks before PubSub; skip PubSub on successful hook completion (critical for sync processes)

**Message Flow Strategy:**
```
┌─────────────────────────────────────────────────────────────┐
│                    Message Processing Flow                   │
├─────────────────────────────────────────────────────────────┤
│  1. Check for custom hook definition                        │
│     ├── If hook exists → Execute hook                       │
│     │   ├── Success → Skip PubSub (return)                 │
│     │   └── Failure → Continue to PubSub                   │
│  2. Attempt PubSub delivery                                 │
│     ├── Success → Message delivered                         │
│     └── Failure → Write to message box for retry           │
└─────────────────────────────────────────────────────────────┘
```

**Benefits:**
- Improved message delivery reliability
- Better handling of synchronous processes
- Infrastructure isolation through Dapr PubSub
- Automatic retry mechanism via message box

---

### 3. Function Filter Enhancements

Extended Function endpoint with advanced filtering capabilities similar to the Data endpoint. All filter inputs are optional to ensure backward compatibility.

**Key Improvements:**
- ✅ Advanced filtering support on Function endpoint
- ✅ Optional filter inputs for backward compatibility
- ✅ Consistent filter syntax across endpoints

> **Reference:** [#54 - Function Filter Enhancements](https://github.com/burgan-tech/vnext/issues/54)

---

### 4. Support Instance ID and Key in All Instance Endpoints

All instance endpoints now accept both `InstanceId` and `Key` for instance identification, providing more flexibility in API interactions.

**Previous Behavior:**
- Only `InstanceId` (UUID) was accepted

**New Behavior:**
- `InstanceId` OR `Key` can be used interchangeably

**Example Usage:**

**Using Instance ID:**
```http
GET /core/workflows/order/instances/18075ad5-e5b2-4437-b884-21d733339113/data
```

**Using Instance Key:**
```http
GET /core/workflows/order/instances/ORDER-2024-001/data
```

**Benefits:**
- More readable and meaningful API calls
- Easier integration with external systems using business keys
- Consistent behavior across all instance-related endpoints

> **Reference:** [#140 - Support Instance ID and Key in All Instance Endpoints](https://github.com/burgan-tech/vnext/issues/140)

---

### 5. Support for _comment Property in Visual Editors

The `_comment` property is now supported in both schema and runtime, enabling better documentation within workflow definitions for visual editors.

**Example Usage:**
```json
{
  "key": "process-payment",
  "_comment": "This state handles payment processing and validation",
  "stateType": 1,
  "transitions": [...]
}
```

**Benefits:**
- Enhanced workflow documentation
- Better collaboration in visual editors
- Self-documenting workflow definitions

> **Reference:** [#145 - Support for _comment Property in Visual Editors](https://github.com/burgan-tech/vnext/issues/145)

---

### 6. Aspect/Trace/Logging Improvements

Significant architectural improvements have been made to code maintainability, refactoring, and organization through aspect-oriented architecture.

**Key Improvements:**
- ✅ Migration to aspect-oriented architecture for better code maintenance
- ✅ Span correlation in traces for better observability
- ✅ Reduced unnecessary log noise

---

### 7. Runtime Metadata Endpoint

A new endpoint has been added to expose runtime metadata for vnext tools and other system integrations.

**Endpoint:**
```http
GET api/v1/config
```

**Response:**
```json
{
  "version": "0.0.22",
  "domain": "core",
  "schemas": {
    "sys-flows": "sys_flows",
    ...
  }
}
```

**Benefits:**
- Easy integration with vnext tools
- Runtime version discovery
- Schema information access

---

## ⚠️ Breaking Changes

### State Schema - subType Property Added

The `subType` property has been added to the state schema. This change may affect your existing workflows.

**Required Action:**
- Review state definitions in your existing workflows
- Update `subType` definitions according to the documentation

**Example Usage:**
```json
{
  "key": "waiting-approval",
  "stateType": 1,
  "subType": 1,
  "_comment": "Waiting for approval state"
}
```

> **Documentation:** [State subType Usage Guide](../docs/technical/flow/state.md#subtype) - Please update your workflows according to this documentation.

---

## 🛠️ Hotfix

### Remove Live and Ready Endpoints from OpenTelemetry Traces

Health check endpoints (`/live` and `/ready`) are now excluded from OpenTelemetry traces to reduce noise in trace data.

**Problem Resolved:**
- Health and metric endpoints were creating unnecessary trace noise
- Frequent calls from orchestration systems (Kubernetes, Docker) added no business value to traces
- Increased trace volume and storage costs

**Solution:**
- Health, metric, and similar endpoints are now excluded from trace monitoring

> **Reference:** [#134 - Remove Live and Ready Endpoints from OpenTelemetry Traces](https://github.com/burgan-tech/vnext/issues/134)

---

## 🧩 Bug Fixes

### 1. Async Operations Validation Fix (#170)

Fixed validation issues in async operations that prevented proper workflow progression.

**Problem Resolved:**
- Validation problems in async operations caused workflow progression issues
- Lock mechanism has been improved for better reliability

**Impact:**
- Improved workflow execution reliability in async scenarios
- Enhanced lock mechanism performance

> **Reference:** [#170 - Bug sync=true](https://github.com/burgan-tech/vnext/issues/170)

---

### 2. Nested JSON Path Filtering and QueryParameter Propagation (#203)

Fixed issues with nested JSON path filtering and QueryParameter propagation to tasks.

**Problem Resolved:**
- Nested JSON path filtering was not working correctly
- QueryParameters were not properly propagated to tasks

**Impact:**
- Correct filtering on nested JSON paths
- Proper QueryParameter propagation across task executions

> **Reference:** [#203 - Fix nested JSON path filtering and QueryParameter propagation to tasks](https://github.com/burgan-tech/vnext/issues/203)

---

## 🔧 Configuration Updates

Configuration for v0.0.22:
```json
{
  "runtimeVersion": "0.0.22",
  "schemaVersion": "0.0.27"
}
```

---

## 🆕 Other Developments

### Multi-Platform Support
- Added multi-platform support for vnext-runtime (Podman, Docker, OrbStack, etc.)

### vnext-cli Deprecation
- `vnext-cli` tool has been moved to Legacy/Archived status
- New projects should use [vnext-template](https://github.com/burgan-tech/vnext-template/tree/master)
- Existing projects need migration for CI/CD and vnext tool compatibility

> **Migration Guide:** See [Migrating from vnext-cli to vnext-template](https://docs.flowlara.com/docs/migration/migrating-from-vnext-cli-to-vnext-template)

---

## 🧱 Issues Referenced

- [#62 - Feature: Cascade Cancel for Subflows on Main Instance Cancellation](https://github.com/burgan-tech/vnext/issues/62)
- [#54 - Function Filter Enhancements](https://github.com/burgan-tech/vnext/issues/54)
- [#134 - Remove Live and Ready Endpoints from OpenTelemetry Traces](https://github.com/burgan-tech/vnext/issues/134)
- [#140 - Support Instance ID and Key in All Instance Endpoints](https://github.com/burgan-tech/vnext/issues/140)
- [#145 - Support for _comment Property in Visual Editors](https://github.com/burgan-tech/vnext/issues/145)
- [#170 - Bug sync=true](https://github.com/burgan-tech/vnext/issues/170)
- [#203 - Fix nested JSON path filtering and QueryParameter propagation to tasks](https://github.com/burgan-tech/vnext/issues/203)

---

## 📘 Developer Notes

### Migration Checklist

- [ ] **⚠️ Breaking Change:** Update `subType` property in state definitions
- [ ] Review workflows for cascade cancel implementation opportunities
- [ ] Implement cancel transitions where workflow cleanup is needed
- [ ] Update API calls to use Instance Key where appropriate for better readability
- [ ] Consider using `_comment` property for workflow documentation
- [ ] Migrate from vnext-cli to vnext-template for new projects
- [ ] Update workflow definitions to schema version 0.0.27
- [ ] Review and test async operations after lock mechanism improvements

### New Capabilities to Explore

- **Cascade Cancel:** Implement proper workflow cleanup with automatic subflow cancellation
- **Inbox/Outbox Workers:** Leverage new message processing infrastructure for reliable delivery
- **Instance Key Usage:** Switch to business keys for more readable API interactions
- **Runtime Metadata:** Integrate with vnext tools using the new config endpoint
- **Multi-Platform:** Deploy on Podman, OrbStack, or other container platforms

### Infrastructure Updates

- **New Worker Images:** inbox and outbox worker images are now available
- **Platform Support:** Runtime now supports multiple container platforms beyond Docker

---

## 🧠 Summary

With this release:
⚠️ **Breaking Change:** `subType` property added to state schema - existing workflows need revision  
✅ Cascade cancel ensures automatic cleanup of subflows and related resources  
✅ New inbox and outbox worker infrastructure for reliable message processing  
✅ Function filter enhancements with backward compatibility  
✅ Instance ID and Key support across all instance endpoints  
✅ _comment property support for better workflow documentation  
✅ Aspect-oriented architecture for improved code maintainability  
✅ Health endpoints excluded from OpenTelemetry traces  
✅ Async validation and nested JSON path filtering bugs fixed  
✅ Multi-platform container support (Podman, Docker, OrbStack)  
✅ Runtime metadata endpoint for tool integrations  
✅ vnext-template as the new standard for project setup

---

## 🔄 Upgrade Path

### From v0.0.21 to v0.0.22:

1. **Update Runtime:**
   ```bash
   # Update to v0.0.22
   git pull origin master
   ```

2. **Update Configuration:**
   ```json
   {
     "runtimeVersion": "0.0.22",
     "schemaVersion": "0.0.27"
   }
   ```

3. **Implement Cascade Cancel (Optional):**
   - Add cancel transition definitions to workflows that need cleanup capabilities
   - Define target state for canceled instances
   - Configure labels and available states for cancel transition

4. **Migrate from vnext-cli (If Applicable):**
   - Follow the [migration guide](https://docs.flowlara.com/docs/migration/migrating-from-vnext-cli-to-vnext-template)
   - Update CI/CD pipelines for vnext-template compatibility

5. **Test Async Operations:**
   - Verify async workflow operations after lock mechanism improvements
   - Test nested JSON path filtering scenarios
   - Validate QueryParameter propagation in task executions

6. **Deploy Worker Images (If Using Message Processing):**
   - Deploy new inbox and outbox worker images
   - Configure Dapr PubSub components
   - Set up message box for retry scenarios

---

**vNext Runtime Platform Team**  
December 3, 2025

