# vNext Runtime Platform - Release Notes v0.0.36
**Release Date:** February 6, 2026

## Overview
This release includes critical hotfixes, bug fixes, and a new incident retry feature. Key highlights include gateway base URL support for cross-domain routing, domain replacement fixes in init service, task execution pipeline improvements, and a new retry endpoint for faulted instances.

---

## Hotfixes

### 1. Support Gateway Base URL in InstanceUrlTemplates for Cross-Domain Routing

Added support for configurable URL templates in Hateoas-style responses when using a gateway. This enables proper URL generation in gateway-based deployments.

**Configuration:**

Add the following configuration to your `appsettings.json`:

```json
{
  "UrlTemplates": {
    "Start": "/api/{0}/workflows/{1}/instances/start",
    "Transition": "/api/{0}/workflows/{1}/instances/{2}/transitions/{3}",
    "FunctionList": "/api/{0}/workflows/{1}/functions/{2}",
    "InstanceList": "/api/{0}/workflows/{1}/instances",
    "Instance": "/api/{0}/workflows/{1}/instances/{2}",
    "InstanceHistory": "/api/{0}/workflows/{1}/instances/{2}/transitions",
    "Data": "/api/{0}/workflows/{1}/instances/{2}/functions/data",
    "View": "/api/{0}/workflows/{1}/instances/{2}/functions/view",
    "Schema": "/api/{0}/workflows/{1}/instances/{2}/functions/schema?transitionKey={3}"
  }
}
```

**Template Parameters:**
- `{0}` - Domain
- `{1}` - Workflow/Flow name
- `{2}` - Instance ID
- `{3}` - Transition key or other context-specific parameter

**Use Case:**
When using an API gateway in front of the vNext platform, configure these templates to match your gateway's routing rules.

**Documentation Updated:**
- `doc/en/services/init-service.md`
- `doc/tr/services/init-service.md`

> **Reference:** [#327 - Support gateway base URL in InstanceUrlTemplates for cross-domain routing](https://github.com/burgan-tech/vnext/issues/327)

---

## Bug Fixes

### 1. Preserve Config and Process Objects During Domain Replacement in Package-API-Server

Fixed an issue where the domain replacement operation in the init service was incorrectly replacing domain values within nested component data such as "process" and "config" objects during package deployment.

**Issue:**
When deploying components via init service, the domain control and replacement operation was too aggressive, replacing domain references in nested component configurations where they should have been preserved.

**Fix:**
Domain replacement now correctly preserves "process" and "config" sub-component data integrity during deployment.

> **Reference:** [#373 - fix: Preserve config and process objects during domain replacement in package-api-server](https://github.com/burgan-tech/vnext/issues/373)

---

### 2. Task Execution Pipeline and Cache Synchronization Improvements

Multiple improvements to the task execution pipeline and cache synchronization:

**Changes:**

1. **SubProcess Header Handling Fix**  
   Resolved an error in subprocess startup caused by incorrect header usage.

2. **TriggerTask Extensions**  
   Added `Headers` and `TimeoutSeconds` fields to all TriggerTask types:
   - StartTask (Type 11)
   - DirectTriggerTask (Type 12)
   - GetInstanceDataTask (Type 13)
   - SubProcessTask (Type 14)

3. **Cache Invalidation Consistency**  
   Fixed inconsistencies in cache invalidation after publish operations.

4. **Task Error Logging Fix**  
   Corrected error logging issues in task execution.

5. **Minor Improvements**  
   Various other minor enhancements and refinements.

**Documentation Updated:**
- `doc/en/flow/tasks/trigger-task.md`
- `doc/tr/flow/tasks/trigger-task.md`

> **Reference:** [#377 - Refactor: Task Execution Pipeline and Cache Synchronization Improvements](https://github.com/burgan-tech/vnext/issues/377)

---

## Features

### 1. Incident Retry Endpoint (Resume from Failed Task)

Added a new retry endpoint that allows resuming workflow instances that have entered a "Faulted" state. This endpoint enables recovery from failed tasks by retrying from the point of failure.

**Endpoint:**

```http
POST /api/v1/{domain}/workflows/{workflow}/instances/{instanceId}/retry?version={version}&sync={sync}
Content-Type: application/json

{
  "key": "",
  "tags": [],
  "attributes": {}
}
```

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `domain` | string (path) | ✅ | Workflow domain |
| `workflow` | string (path) | ✅ | Workflow name |
| `instanceId` | string (path) | ✅ | Instance ID |
| `version` | string (query) | ❌ | Workflow version (e.g., "1.0.0") |
| `sync` | boolean (query) | ❌ | Synchronous execution (default: false) |

**Request Body:**

All fields are optional and follow the standard instance payload schema:

```json
{
  "key": "",           // Instance key (optional)
  "tags": [],          // Instance tags (optional)
  "attributes": {}     // Additional data to merge with instance state (optional)
}
```

**Requirements:**

- ✅ Instance must be in **"Faulted"** state
- ✅ Only available for faulted instances
- ❌ Cannot be used on instances in other states (Active, Completed, etc.)

**Example Request:**

```http
POST /api/v1/ecommerce/workflows/payment-processing/instances/18075ad5-e5b2-4437-b884-21d733339113/retry?version=1.0.0&sync=true
Content-Type: application/json

{
  "tags": ["retry-attempt-1"],
  "attributes": {
    "retryReason": "Network timeout recovered",
    "retriedBy": "system-admin",
    "retryTimestamp": "2026-02-06T10:30:00Z"
  }
}
```

**Example Response (Success):**

```json
{
  "id": "18075ad5-e5b2-4437-b884-21d733339113",
  "status": "A"
}
```

**Error Scenarios:**

**Not Faulted:**
```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Bad Request",
  "status": 400,
  "detail": "Instance is not in Faulted state. Current state: Active",
  "instance": "/api/v1/ecommerce/workflows/payment-processing/instances/18075ad5-e5b2-4437-b884-21d733339113/retry"
}
```

**Instance Not Found:**
```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Not Found",
  "status": 404,
  "detail": "Instance not found",
  "instance": "/api/v1/ecommerce/workflows/payment-processing/instances/18075ad5-e5b2-4437-b884-21d733339113/retry"
}
```

**Use Cases:**

1. **Transient Failure Recovery**  
   Retry after temporary infrastructure issues (network timeout, service unavailable)

2. **Data Correction and Retry**  
   Fix data issues and retry the failed task

3. **Manual Intervention**  
   Allow operators to manually resume workflows after investigating failures

4. **Automated Retry Strategies**  
   Implement retry logic in external monitoring systems

**Documentation Updated:**
- `doc/en/how-to/start-instance.md`
- `doc/tr/how-to/start-instance.md`

> **Reference:** [#290 - Incident Retry Endpoint (Resume from Failed Task)](https://github.com/burgan-tech/vnext/issues/290)

---

## Configuration Updates

Configuration for v0.0.36:

```json
{
  "runtimeVersion": "0.0.36",
  "schemaVersion": "0.0.35"
}
```

> **Note:** Schema version remains unchanged from v0.0.35 (v0.0.36 runtime is backward compatible with schema v0.0.35).

---

## Issues Referenced

- [#327 - Support gateway base URL in InstanceUrlTemplates for cross-domain routing](https://github.com/burgan-tech/vnext/issues/327)
- [#373 - fix: Preserve config and process objects during domain replacement in package-api-server](https://github.com/burgan-tech/vnext/issues/373)
- [#377 - Refactor: Task Execution Pipeline and Cache Synchronization Improvements](https://github.com/burgan-tech/vnext/issues/377)
- [#290 - Incident Retry Endpoint (Resume from Failed Task)](https://github.com/burgan-tech/vnext/issues/290)

---

## Summary

With this release:
- ✅ Added gateway base URL support for cross-domain routing via configurable URL templates
- ✅ Fixed domain replacement issues in init service during package deployment
- ✅ Enhanced TriggerTask types with Headers and TimeoutSeconds support
- ✅ Improved cache invalidation consistency and task error logging
- ✅ Added new retry endpoint for recovering faulted workflow instances

---

**vNext Runtime Platform Team**  
February 6, 2026
