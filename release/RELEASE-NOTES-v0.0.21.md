# vNext Runtime Platform - Release Notes v0.0.21
**Release Date:** November 27, 2025

## 🧭 Overview
This release introduces significant improvements to workflow instance management with the refactoring of Trigger Tasks into four distinct task types, enhanced Function API filtering capabilities, QueryParameters support in mapping context, and flexible instance identification using business keys. These changes improve developer experience, API usability, and cross-workflow orchestration capabilities.

---

## 🚀 Major Updates

### 1. Trigger Task Refactoring - Separate Task Types

The unified TriggerTransitionTask (Type 11) has been refactored into four distinct, specialized task types for better clarity and type safety. Each task type now has its own dedicated type number and clear purpose.

**New Task Types:**

| Task Type | Type Number | Purpose |
|-----------|-------------|---------|
| **StartTask** | "11" | Start new workflow instances |
| **DirectTriggerTask** | "12" | Trigger transitions on existing instances |
| **GetInstanceDataTask** | "13" | Retrieve instance data with extensions |
| **SubProcessTask** | "14" | Launch independent subprocess instances |

**Key Benefits:**
- ✅ Clear type distinction for different operations
- ✅ Improved type safety and validation
- ✅ Better developer experience with dedicated methods
- ✅ Simplified error handling per task type
- ✅ Enhanced code readability and maintainability

#### Migration Guide

**Old Structure (TriggerTransitionTask):**
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

**New Structure (StartTask):**
```json
{
  "type": "11",
  "config": {
    "domain": "approvals",
    "flow": "approval-flow"
  }
}
```

**Migration Matrix:**

| Old Config | New Task Type | New Type Number | Changes Required |
|------------|---------------|-----------------|------------------|
| `type: "Start"` | StartTask | "11" | Remove nested `type` field |
| `type: "Trigger"` | DirectTriggerTask | "12" | Change type to "12", remove nested `type` |
| `type: "GetInstanceData"` | GetInstanceDataTask | "13" | Change type to "13", remove nested `type` |
| `type: "SubProcess"` | SubProcessTask | "14" | Change type to "14", remove nested `type` |

**Code Changes:**
```csharp
// Old approach
var triggerTask = task as TriggerTransitionTask;
triggerTask.SetTriggerType("Start");

// New approach
var startTask = task as StartTask;
// No need to set type - it's implicit
```

> **Documentation:** See [Trigger Task Types](../docs/technical/flow/tasks/trigger-task.md) for complete usage examples and best practices.

---

### 2. Function Data Filtering

The Data Function now supports powerful filtering capabilities to query workflow instances based on their attributes. This enables efficient data retrieval and search operations directly through the Function API.

**Filter Syntax:**
```
GET /{domain}/workflows/{workflow}/instances/{instance}/functions/data?filter=attributes={field}={operator}:{value}
```

**Available Operators:**

| Operator | Description | Example |
|----------|-------------|---------|
| `eq` | Equal to | `filter=attributes=status=eq:active` |
| `ne` | Not equal to | `filter=attributes=status=ne:completed` |
| `gt` | Greater than | `filter=attributes=amount=gt:1000` |
| `ge` | Greater than or equal | `filter=attributes=score=ge:80` |
| `lt` | Less than | `filter=attributes=count=lt:10` |
| `le` | Less than or equal | `filter=attributes=age=le:65` |
| `between` | Between two values | `filter=attributes=amount=between:100,500` |
| `like` | Contains substring | `filter=attributes=name=like:john` |
| `startswith` | Starts with | `filter=attributes=email=startswith:test` |
| `endswith` | Ends with | `filter=attributes=email=endswith:.com` |
| `in` | Value in list | `filter=attributes=status=in:active,pending` |
| `nin` | Value not in list | `filter=attributes=type=nin:test,debug` |

**Example Usage:**

```http
# Single filter
GET /core/workflows/payment/instances/123/functions/data?filter=attributes=amount=gt:1000

# Multiple filters (AND logic)
GET /core/workflows/order/instances/456/functions/data?filter=attributes=status=eq:active&filter=attributes=priority=eq:high

# Range filtering
GET /core/workflows/transaction/instances/789/functions/data?filter=attributes=amount=between:100,500
```

**Benefits:**
- Efficient instance data querying without custom endpoints
- Support for complex filter combinations
- Consistent filter syntax across the platform
- Optimized for large datasets with pagination

> **Note:** Filtering is only available on the Data Function endpoint and works on instance attributes.

> **Documentation:** See [Function APIs](../docs/technical/flow/function.md#filtering-instance-data) for complete filtering documentation.

---

### 3. QueryParameters Support in ScriptContext

The `ScriptContext` class now includes a `QueryParameters` property, enabling access to query parameters in Function task handlers. This property is **specific to Function tasks** and provides access to query string parameters passed to function endpoints.

**New Property:**
```csharp
public sealed class ScriptContext
{
    public dynamic? QueryParameters { get; private set; }
    // ... other properties
}
```

**Usage Example:**
```csharp
public class FunctionTaskMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Access query parameters using indexer syntax
        var userId = context.QueryParameters?.["userId"];
        var cityId = context.QueryParameters?.["cityId"];
        var page = context.QueryParameters?.["page"];
        var filter = context.QueryParameters?.["filter"];
        
        // Use in function logic
        LogInformation("Processing function for userId: {0}, cityId: {1}", 
            args: new object?[] { userId, cityId });
        
        return Task.FromResult(new ScriptResponse());
    }
}
```

**Common Use Cases:**
- Accessing custom query parameters in Function task handlers
- Reading filter parameters passed to functions
- Getting pagination parameters from function calls
- Extracting user-specific identifiers from query strings

**Benefits:**
- Direct access to function query parameters in mapping handlers
- Consistent with existing Headers and Body properties
- Indexer-based access for flexible parameter retrieval
- Available in InputHandler for Function tasks

> **Documentation:** See [Mapping Guide](../docs/technical/flow/mapping.md#queryparameters) for complete usage examples.

---

### 4. Instance Key Support in Transitions

All transition endpoints now support using the instance Key as an alternative to the instance ID (UUID). This enables more readable and meaningful instance references using business identifiers.

**Updated Endpoint:**
```
PATCH /:domain/workflows/:flow/instances/:instanceIdOrKey/transitions/:transition
```

**The `:instanceIdOrKey` parameter accepts:**
- **Instance ID**: UUID returned at instance creation (e.g., `18075ad5-e5b2-4437-b884-21d733339113`)
- **Instance Key**: Business key provided during creation (e.g., `ORDER-2024-001`, `CUST-12345`)

**Example Usage:**

**Using Instance ID:**
```http
PATCH /ecommerce/workflows/order/instances/18075ad5-e5b2-4437-b884-21d733339113/transitions/approve
Content-Type: application/json

{
  "approvedBy": "manager",
  "approvalDate": "2025-11-27T10:00:00Z"
}
```

**Using Instance Key:**
```http
PATCH /ecommerce/workflows/order/instances/ORDER-2024-001/transitions/approve
Content-Type: application/json

{
  "approvedBy": "manager",
  "approvalDate": "2025-11-27T10:00:00Z"
}
```

**Benefits:**
- More readable and meaningful API calls
- Easier integration with external systems using business keys
- No need to store and manage UUIDs separately
- Consistent with instance creation `key` parameter
- Works with ALL transition endpoints

**Applies to:**
- All PATCH transition endpoints
- GET instance status endpoints
- Function API endpoints (state, data, view)

> **Documentation:** See [Instance Startup Guide](../docs/technical/how-to/start-instance.md#instance-transition) for complete examples.

---

## 📋 Complete Feature List

### Trigger Task Types
- ✅ StartTask (Type 11) - Start new workflow instances
- ✅ DirectTriggerTask (Type 12) - Trigger transitions on existing instances
- ✅ GetInstanceDataTask (Type 13) - Retrieve instance data with extensions
- ✅ SubProcessTask (Type 14) - Launch independent subprocess instances
- ✅ Dedicated setter methods for each task type
- ✅ Clear response structures per task type
- ✅ Improved error messages and handling
- ✅ Migration guide from old TriggerTransitionTask

### Function Data Filtering
- ✅ 12 filter operators (eq, ne, gt, ge, lt, le, between, like, startswith, endswith, in, nin)
- ✅ Multiple filter support with AND logic
- ✅ Efficient querying on instance attributes
- ✅ Compatible with pagination
- ✅ Consistent filter syntax

### QueryParameters Support
- ✅ New QueryParameters property in ScriptContext
- ✅ Available in Function task InputHandler methods
- ✅ Access to function query parameters via indexer syntax
- ✅ Dynamic type for flexible parameter access
- ✅ Consistent with Headers and Body properties

### Instance Key Support
- ✅ Support for business keys in all transition endpoints
- ✅ Support in GET instance endpoints
- ✅ Support in Function API endpoints
- ✅ Backward compatible with UUID-based access
- ✅ Improved API readability

---

## 🔧 Configuration Updates

Configuration for v0.0.21:
```json
{
  "runtimeVersion": "0.0.21",
  "schemaVersion": "0.0.26"
}
```

---

## 📘 Developer Notes

### Migration Checklist

- [ ] Review and update TriggerTransitionTask definitions to new task types
- [ ] Update task type numbers: Trigger→12, GetInstanceData→13, SubProcess→14
- [ ] Remove nested `type` field from task configurations
- [ ] Update task casting in mapping handlers (TriggerTransitionTask → specific task types)
- [ ] Consider using instance Keys for more readable API calls
- [ ] Explore Function data filtering for instance queries
- [ ] Update Function task mapping handlers to leverage QueryParameters when needed
- [ ] Update workflow definitions to schema version 0.0.26

### Breaking Changes

**Task Type Changes:**
- DirectTriggerTask, GetInstanceDataTask, and SubProcessTask now require different type numbers
- Old TriggerTransitionTask with nested type field is deprecated
- Mapping code needs to be updated to use specific task classes

**Backward Compatibility:**
- Instance ID-based endpoints continue to work unchanged
- Existing StartTask (Type 11) behavior is preserved
- ETag pattern and existing Function API behavior unchanged

### New Capabilities

**Enhanced Workflow Orchestration:**
- Use specific task types for clearer workflow definitions
- Leverage instance Keys for business-friendly identifiers
- Filter instance data directly through Function API
- Access query parameters in Function task mapping handlers for dynamic behavior

**Better Developer Experience:**
- Type-safe task definitions with dedicated classes
- More readable API calls using business keys
- Powerful filtering without custom endpoints
- Clearer error messages per task type

---

## 🔄 Upgrade Path

### From v0.0.20 to v0.0.21:

1. **Update Runtime:**
   ```bash
   # Update to v0.0.21
   git pull origin master
   ```

2. **Update Configuration:**
   ```json
   {
     "runtimeVersion": "0.0.21",
     "schemaVersion": "0.0.26"
   }
   ```

3. **Migrate Trigger Tasks:**
   - Identify all TriggerTransitionTask definitions (Type 11 with nested type)
   - Update type numbers: Trigger→12, GetInstanceData→13, SubProcess→14
   - Remove nested `type` field from configurations
   - Update mapping code to use specific task classes:
     ```csharp
     // Old
     var triggerTask = task as TriggerTransitionTask;
     
     // New
     var startTask = task as StartTask;              // Type 11
     var directTriggerTask = task as DirectTriggerTask;    // Type 12
     var getDataTask = task as GetInstanceDataTask;        // Type 13
     var subProcessTask = task as SubProcessTask;          // Type 14
     ```

4. **Test Existing Workflows:**
   - Verify task executions work correctly
   - Check error handling with new task types
   - Test instance identification with both ID and Key

5. **Explore New Features:**
   - Implement Function data filtering for instance queries
   - Use QueryParameters in Function task mapping handlers
   - Switch to instance Keys where appropriate for better readability

---

## 📚 Documentation Updates

- [Trigger Task Types](../docs/technical/flow/tasks/trigger-task.md) - Complete guide for all four task types
- [Function APIs - Filtering](../docs/technical/flow/function.md#filtering-instance-data) - Data function filtering documentation
- [Mapping Guide - QueryParameters](../docs/technical/flow/mapping.md#queryparameters) - QueryParameters usage in ScriptContext
- [Instance Startup Guide](../docs/technical/how-to/start-instance.md#instance-transition) - Instance Key support in transitions

Turkish documentation:
- [Trigger Task Types](../docs/technical/flow/tasks/trigger-task.md)
- [Function APIs - Filtering](../docs/technical/flow/function.md#filtering-instance-data)
- [Mapping Guide - QueryParameters](../docs/technical/flow/mapping.md#queryparameters)
- [Instance Startup Guide](../docs/technical/how-to/start-instance.md#instance-transition)

---

## 🧠 Summary

With this release:
✅ Four distinct task types for clearer workflow orchestration  
✅ Function data filtering with 12 powerful operators  
✅ QueryParameters support in Function task mapping handlers  
✅ Instance Key support in all transition endpoints  
✅ Improved type safety and error handling  
✅ Better developer experience with dedicated task classes  
✅ Enhanced API readability with business keys  
✅ Backward compatibility maintained for existing workflows

---

**vNext Runtime Platform Team**  
November 27, 2025

