# vNext Runtime Platform - Release Notes v0.0.38
**Release Date:** March 9, 2026

## Overview
This release adds **X-App-Version** in response headers and the Health endpoint, **AddHeader** and **RemoveHeader** methods on HttpTask, DaprService, and Trigger-family tasks for fine-grained header control, and **ParentInstanceId** in trace and logging for cross-domain subflow correlation.

---

## Features

### 1. App Version in Responses

API responses and the Health endpoint now include the application version in the **X-App-Version** response header. This allows clients and monitoring tools to identify the runtime version without parsing response bodies.

**Where it appears:**
- **Response headers:** All API responses include an `X-App-Version` header with the runtime version (e.g. `0.0.38`).
- **Health endpoint:** The `GET /health` response also includes the `X-App-Version` header.

**Example Health response headers:**
```http
HTTP/1.1 200 OK
Content-Type: application/json
X-App-Version: 0.0.38
```

> **Reference:** [#365 - App Version in Responses - Implementation](https://github.com/burgan-tech/vnext/issues/365)

---

### 2. AddHeader and RemoveHeader on Tasks with Headers Support

**HttpTask**, **DaprServiceTask**, and the **Trigger** task family (StartTask, DirectTriggerTask, GetInstanceDataTask, SubProcessTask) now support **AddHeader** and **RemoveHeader** methods in addition to **SetHeaders**. This allows mapping code to add or remove individual headers without replacing the entire header set.

**Available methods (v0.0.38+):**
- **AddHeader(string name, string value):** Adds or overwrites a single header.
- **RemoveHeader(string name):** Removes a header by name (case-insensitive where applicable).

**Usage example (HttpTask):**
```csharp
var httpTask = task as HttpTask;
// Add or override a single header
httpTask.AddHeader("X-Correlation-ID", context.Instance.Data.correlationId);
httpTask.AddHeader("Authorization", "Bearer " + token);
// Remove a header
httpTask.RemoveHeader("X-Optional-Header");
```

**Usage example (DirectTriggerTask):**
```csharp
var directTriggerTask = task as DirectTriggerTask;
directTriggerTask.SetInstance(context.Instance.Data.approvalInstanceId);
directTriggerTask.SetTransitionName("approve");
directTriggerTask.AddHeader("X-Request-Source", "parent-workflow");
directTriggerTask.AddHeader("X-Correlation-ID", context.Instance.Id);
```

**Behavior:**
- Headers defined in the task config (JSON) are applied first. AddHeader and RemoveHeader in mapping run afterward, so mapping can override or remove config headers.
- SetHeaders continues to replace the entire header set when called; use AddHeader/RemoveHeader when you need incremental changes.

> **Reference:** [#417 - feat(domain): Add AddHeader and RemoveHeader to tasks with Headers support](https://github.com/burgan-tech/vnext/issues/417)

---

### 3. ParentInstanceId in Trace and Log Correlation (Cross-Domain Subflow)

Trace and logging now include **ParentInstanceId** where applicable. When an instance is created or driven by a parent (e.g. subflow, cross-domain trigger), logs and trace metadata carry the parent instance id so you can correlate activity across the parent and child.

**Benefits:**
- **Log correlation:** Filter or search logs by parent instance id to see all activity for a given parent flow.
- **Distributed tracing:** Trace views can follow the relationship between parent and child instances across domains.

ParentInstanceId is included in the same places as InstanceId in the trace and log context (e.g. structured log properties, trace attributes). No configuration change is required; it is emitted automatically when a parent-child relationship exists.

> **Reference:** [#415 - Add InstanceId and Parent InstanceId to trace and log correlation (cross-domain subflow)](https://github.com/burgan-tech/vnext/issues/415)

---

## Configuration Updates

Configuration for v0.0.38:

```json
{
  "runtimeVersion": "0.0.38",
  "schemaVersion": "0.0.36"
}
```

> **Note:** Schema version remains at 0.0.36; runtime v0.0.38 is backward compatible with schema v0.0.36.

---

## Issues Referenced

- [#365 - App Version in Responses - Implementation](https://github.com/burgan-tech/vnext/issues/365)
- [#417 - feat(domain): Add AddHeader and RemoveHeader to tasks with Headers support](https://github.com/burgan-tech/vnext/issues/417)
- [#415 - Add InstanceId and Parent InstanceId to trace and log correlation (cross-domain subflow)](https://github.com/burgan-tech/vnext/issues/415)

---

## Summary

With this release:
- **X-App-Version** is returned in all API response headers and on the Health endpoint for version identification.
- **AddHeader** and **RemoveHeader** are available on HttpTask, DaprServiceTask, and Trigger-family tasks for fine-grained header control in mappings.
- **ParentInstanceId** is included in trace and logging for cross-domain subflow correlation, improving observability for parent-child instance flows.

---

**vNext Runtime Platform Team**  
March 9, 2026
