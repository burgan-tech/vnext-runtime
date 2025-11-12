# vNext Runtime Platform - Release Notes v0.0.19
**Release Date:** November 11, 2025

## 🧭 Overview
This release introduces significant enhancements to the **vNext Runtime Platform**, focusing on improved flexibility in transition handling, enhanced header propagation in service-to-service communication, and important refinements to the view extension system.  
Key features include **transition input mapping**, **$self target support for shared transitions**, and critical fixes for **DaprServiceTask header handling**.

---

## 🚀 Major Updates

### 1. Transition Input Mapping Support
Transitions now support input mapping configuration, enabling data transformation before state transitions. This enhancement provides the same mapping capabilities as task mappings.

**Key Benefits:**
- Transform input data before state transitions
- Map external API responses to internal workflow data structures
- Validate and sanitize input data before processing
- Apply business logic to input data during transitions

**Example Usage:**
```json
{
  "key": "process-payment-transition",
  "source": "payment-validation",
  "target": "payment-processing",
  "labels": [...],
  "mapping": {
    "location": "./src/PaymentTransitionMapping.csx",
    "code": "base64-encoded-csx-content"
  }
}
```

**Mapping Implementation:**
```csharp
using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class PaymentTransitionMapping : ITransitionMapping
{
    public async Task<dynamic> Handler(ScriptContext context)
    {
        dynamic data = new ExpandoObject();
        data.amount = context.Body.amount;
        data.currency = context.Body.currency ?? "USD";
        data.validatedAt = DateTime.UtcNow;
        return data;
    }
}
```

> **Reference:** [#98 - Add Input Mapping for Transitions](https://github.com/burgan-tech/vnext/issues/98)

---

### 2. $self Target Support in Shared Transitions
Shared transitions can now use `$self` as a special target value, allowing transitions to remain in the current state. This is particularly useful for state-internal operations and event handling without state changes.

**Key Features:**
- `$self` as a special target value in transitions
- Enhanced shared transition capabilities
- Support for operations that remain in the current state

**Example Usage:**
```json
{
  "key": "refresh-data-transition",
  "target": "$self",
  "triggerType": 0,
  "labels": [
    {"language": "en-US", "label": "Refresh Data"},
    {"language": "tr-TR", "label": "Verileri Yenile"}
  ],
  "schema": null,
  "rule": null
}
```

> **Reference:** [#130 - Add Support for $self Target in Shared Transitions](https://github.com/burgan-tech/vnext/issues/130)

---

### 3. DaprServiceTask Header Mapping Support (Hotfix)
Fixed critical issue where `DaprServiceTask` headers were not propagated to downstream services. The `DaprServiceTaskExecutor` now correctly maps headers from the task definition to outgoing Dapr service invocations.

**Problem Resolved:**
- HTTP requests via Dapr now include user-defined headers
- Contextual and authorization metadata is preserved during service-to-service communication
- Headers property is now properly utilized in Dapr invocations

**Technical Implementation:**
The executor now enumerates headers before Dapr invocation:
```csharp
if (task.Headers is { } headers)
{
    foreach (var property in headers.Value.EnumerateObject())
    {
        request.Headers.TryAddWithoutValidation(property.Name, property.Value.GetString());
    }
}
```

**Example Task Definition:**
```json
{
  "key": "call-user-service",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-tasks",
  "attributes": {
    "type": "7",
    "config": {
      "appId": "user-service",
      "methodName": "api/users",
      "httpVerb": "POST"
    }
  }
}
```

**Example Mapping with Headers:**
```csharp
public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var daprTask = task as DaprServiceTask;
    
    daprTask.SetBody(new {
        userId = context.Instance.Data.userId
    });
    
    var headers = new Dictionary<string, string?>
    {
        ["X-Request-Id"] = context.Headers["x-request-id"],
        ["Authorization"] = $"Bearer {context.Instance.Data.token}",
        ["X-Correlation-Id"] = context.Instance.CorrelationId
    };
    daprTask.SetHeaders(headers);
    
    return Task.FromResult(new ScriptResponse
    {
        Data = context.Instance.Data
    });
}
```

> **Reference:** [#155 - Hotfix: Add Header Mapping Support in DaprServiceTask](https://github.com/burgan-tech/vnext/issues/155)

---

## 🧩 Bug Fixes

### View Extension Parameter Consistency
Fixed inconsistency in view URL parameters:
- Changed parameter name from `extension` to `extensions` for consistency
- Properly bound `extensions` parameter in Long Polling endpoint
- Properly bound `extensions` parameter in View endpoint

**Impact:**
- Consistent parameter naming across all view-related endpoints
- Better alignment with schema definition from v0.0.18
- Improved API consistency

> **Reference:** [#147 - Adding "extensions" parameter in View Url](https://github.com/burgan-tech/vnext/issues/147)

---

## ⚠️ Breaking Changes

### 1. DaprServiceTask Property Rename
The `data` property in `DaprServiceTask` has been renamed to `body` for better semantic clarity and consistency with HTTP terminology.

**Migration Required:**
```csharp
// ❌ Old (v0.0.18 and earlier):
var daprTask = task as DaprServiceTask;
daprTask.SetData(new { userId = 123 });

// ✅ New (v0.0.19+):
var daprTask = task as DaprServiceTask;
daprTask.SetBody(new { userId = 123 });
```

**Impact:**
- All mapping scripts using `SetData` must be updated to use `SetBody`
- The internal property name has changed from `data` to `body`
- No changes to task JSON definitions required

---

### 2. DaprHttpEndpointTask Headers Removal
The `headers` property has been removed from `DaprHttpEndpointTask`. Header management is now consolidated in the mapping layer.

**Migration Required:**
```csharp
// ❌ Old (v0.0.18 and earlier):
var httpTask = task as DaprHttpEndpointTask;
httpTask.Headers = headers; // No longer available

// ✅ New (v0.0.19+):
// Use SetHeaders method in mapping instead
var httpTask = task as DaprHttpEndpointTask;
httpTask.SetHeaders(headers);
```

**Rationale:**
- Consistent header handling across all task types
- Better control through mapping layer
- Simplified task model

---

## 🔧 Configuration Updates
Configuration remains compatible with v0.0.19:
```json
{
  "runtimeVersion": "0.0.19",
  "schemaVersion": "0.0.24"
}
```

---

## 🧱 Issues Referenced
- [#155 - Hotfix: Add Header Mapping Support in DaprServiceTask](https://github.com/burgan-tech/vnext/issues/155)
- [#98 - Add Input Mapping for Transitions](https://github.com/burgan-tech/vnext/issues/98)
- [#130 - Add Support for $self Target in Shared Transitions](https://github.com/burgan-tech/vnext/issues/130)
- [#147 - Adding "extensions" parameter in View Url](https://github.com/burgan-tech/vnext/issues/147)

---

## 📘 Developer Notes

### Migration Checklist
- [ ] Update all `DaprServiceTask` mappings: replace `SetData` with `SetBody`
- [ ] Update all `DaprHttpEndpointTask` mappings: use `SetHeaders` method
- [ ] Review transition definitions for potential input mapping opportunities
- [ ] Test header propagation in service-to-service calls
- [ ] Update view endpoints to use `extensions` parameter
- [ ] Consider using `$self` target for in-state operations

### New Capabilities to Explore
- **Transition Mapping:** Add data validation and transformation logic to transitions
- **$self Transitions:** Implement refresh/update operations without state changes
- **Enhanced Headers:** Utilize proper header propagation for authentication and tracing

---

## 🧠 Summary
With this release:
✅ Transitions now support input mapping for enhanced flexibility  
✅ Shared transitions can use `$self` target for in-state operations  
✅ DaprServiceTask now properly propagates headers to downstream services  
✅ View endpoint parameters are now consistent (`extensions`)  
✅ Task API improved with `SetBody` for better semantic clarity  
✅ Consolidated header management across task types

---

## 🔄 Upgrade Path

### From v0.0.18 to v0.0.19:

1. **Update Runtime:**
   ```bash
   # Update to v0.0.19
   git pull origin master
   ```

2. **Update Mapping Scripts:**
   ```csharp
   // Find and replace in all mapping files
   // Old: daprTask.SetData(...)
   // New: daprTask.SetBody(...)
   ```

3. **Update Configuration:**
   ```json
   {
     "runtimeVersion": "0.0.19",
     "schemaVersion": "0.0.24"
   }
   ```

4. **Test Service Communication:**
   - Verify headers are properly propagated in Dapr calls
   - Test authentication flows with header-based auth
   - Validate tracing headers (correlation-id, request-id)

---

**vNext Runtime Platform Team**  
November 11, 2025

