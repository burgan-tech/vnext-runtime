# Extension Management

Extensions are components used for enriching instance data in the vNext platform. Like functions, they can execute tasks, but extensions are reflected in instance data responses and do not provide external endpoints.

## Table of Contents

1. [Overview](#overview)
2. [Extension Definition](#extension-definition)
3. [Extension Types](#extension-types)
4. [Extension Scopes](#extension-scopes)
5. [Usage Examples](#usage-examples)
6. [Best Practices](#best-practices)

---

## Overview

Extensions are used for:

- **Data Enrichment**: Adds additional context information to instance data
- **Task Execution**: Can execute tasks like functions
- **Response Enrichment**: Adds data under the `extensions` object in instance data responses
- **Dynamic Data**: Provides real-time calculated or externally sourced data

:::highlight green 💡
Unlike functions, extensions do not provide external endpoints. They only enrich instance data responses.
:::

### Extension vs Function Comparison

| Feature | Extension | Function |
|---------|-----------|----------|
| Task execution | ✅ Yes | ✅ Yes |
| External endpoint | ❌ No | ✅ Yes |
| Response reflection | ✅ Under `extensions` object | ❌ Separate endpoint |
| Purpose | Data enrichment | API endpoint |

---

## Extension Definition

### Basic Structure

```json
{
  "key": "extension-user-session",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-extensions",
  "flowVersion": "1.0.0",
  "tags": [
    "system",
    "core",
    "sys-extensions",
    "components"
  ],
  "attributes": {
    "type": 2,
    "scope": 1,
    "task": {
      "order": 1,
      "task": {
        "key": "user-session",
        "domain": "core",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "location": "./src/UserSessionMapping.csx",
        "code": "<BASE64_ENCODED_MAPPING_CODE>"
      }
    }
  }
}
```

### Core Properties

| Property | Type | Description |
|----------|------|-------------|
| `key` | `string` | Unique identifier for the extension |
| `version` | `string` | Version information (semantic versioning) |
| `domain` | `string` | Domain the extension belongs to |
| `flow` | `string` | Flow stream information (default: `sys-extensions`) |
| `flowVersion` | `string` | Flow version information |
| `tags` | `string[]` | Tags for categorization and searching |
| `attributes` | `object` | Extension configuration |

### Attributes Properties

| Property | Type | Description |
|----------|------|-------------|
| `type` | `number` | Extension type (1-4) |
| `scope` | `number` | Extension scope (1-3) |
| `task` | `object` | Task definition to execute |

---

## Extension Types

Determines when and in which flows the extensions will work.

### ExtensionType Enum

```csharp
public enum ExtensionType
{
    /// <summary>
    /// Extension that works while record instances are rotating in all flows
    /// </summary>
    Global = 1,

    /// <summary>
    /// Extension that works on all flows and when record instances are requested
    /// </summary>
    GlobalAndRequested = 2,

    /// <summary>
    /// Extension that only works on the flows for which it is defined
    /// </summary>
    DefinedFlows = 3,
    
    /// <summary>
    /// Extension that only works on defined flows and when requested
    /// </summary>
    DefinedFlowAndRequested = 4
}
```

### Type Comparison

| Type | Value | Auto Execute | On Request | Scope |
|------|-------|--------------|------------|-------|
| **Global** | 1 | ✅ Yes | ❌ No | All flows |
| **GlobalAndRequested** | 2 | ✅ Yes | ✅ Yes | All flows |
| **DefinedFlows** | 3 | ✅ Yes | ❌ No | Defined flows |
| **DefinedFlowAndRequested** | 4 | ✅ Yes | ✅ Yes | Defined flows |

### Type Usage Scenarios

| Type | Usage Scenario |
|------|----------------|
| `Global` | Data always required in all instances (e.g., system info) |
| `GlobalAndRequested` | Usually required but sometimes triggered by request (e.g., user session) |
| `DefinedFlows` | Data always required in specific workflows (e.g., account details) |
| `DefinedFlowAndRequested` | Data required in specific workflows and on demand |

---

## Extension Scopes

Determines on which endpoints the extensions will work.

### ExtensionScope Enum

```csharp
public enum ExtensionScope
{
    /// <summary>
    /// Works on {domain}/workflows/{workflow}/instances/{instance} endpoint
    /// </summary>
    GetInstance = 1,

    /// <summary>
    /// Works on {domain}/workflows/{workflow}/instances endpoint
    /// </summary>
    GetAllInstances = 2,
    
    /// <summary>
    /// Works on {domain}/workflows/{workflow}/instances/{instance}/transitions endpoint
    /// </summary>
    GetHistoryTransition = 2,

    /// <summary>
    /// Works on all GET endpoints
    /// </summary>
    Everywhere = 3
}
```

### Scope Comparison

| Scope | Value | Endpoints |
|-------|-------|-----------|
| **GetInstance** | 1 | Single instance query |
| **GetAllInstances** | 2 | Instance list query |
| **GetHistoryTransition** | 2 | Transition history query |
| **Everywhere** | 3 | All GET endpoints |

---

## Usage Examples

### Example 1: User Session Extension

```json
{
  "key": "extension-user-session",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-extensions",
  "flowVersion": "1.0.0",
  "tags": ["system", "core", "session"],
  "attributes": {
    "type": 2,
    "scope": 1,
    "task": {
      "order": 1,
      "task": {
        "key": "user-session",
        "domain": "core",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "location": "./src/UserSessionMapping.csx",
        "code": "<BASE64>"
      }
    }
  }
}
```

**Mapping Example:**

```csharp
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class UserSessionMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse());
    }

    /// <summary>
    /// Populate the user session data into the workflow instance
    /// </summary>
    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        return new ScriptResponse
        {
            Key = "user-session-output",
            Data = new
            {
                userSession = new
                {
                    userId = context.Headers?["user_reference"],
                    deviceId = context.Headers?["x-device-id"],
                    userAgent = context.Headers?["user-agent"],
                    ipAddress = context.Headers?["x-forwarded-for"] ?? context.Headers?["x-real-ip"]
                }
            }
        };
    }
}
```

### Example 2: Account Limits Extension

```json
{
  "key": "extension-account-limits",
  "version": "1.0.0",
  "domain": "banking",
  "flow": "sys-extensions",
  "flowVersion": "1.0.0",
  "tags": ["banking", "accounts", "limits"],
  "attributes": {
    "type": 3,
    "scope": 1,
    "task": {
      "order": 1,
      "task": {
        "key": "get-account-limits",
        "domain": "banking",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "location": "./src/AccountLimitsMapping.csx",
        "code": "<BASE64>"
      }
    }
  }
}
```

### Example 3: Customer Profile Extension

```json
{
  "key": "extension-customer-profile",
  "version": "1.0.0",
  "domain": "customer",
  "flow": "sys-extensions",
  "flowVersion": "1.0.0",
  "tags": ["customer", "profile", "lookup"],
  "attributes": {
    "type": 4,
    "scope": 3,
    "task": {
      "order": 1,
      "task": {
        "key": "get-customer-profile",
        "domain": "customer",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "location": "./src/CustomerProfileMapping.csx",
        "code": "<BASE64>"
      }
    }
  }
}
```

---

## Extension Execution Mechanism

### Adding to Response

When the Get Instance Data endpoint is called, if an extension definition exists, extensions are executed according to scope and type, and placed in the response under the `extensions` object.

**Example Response:**

```json
{
  "data": {
    "userId": "user123",
    "amount": 1000,
    "currency": "TRY"
  },
  "eTag": "W/\"xyz789abc123\"",
  "extensions": {
    "userSession": {
      "userId": "user123",
      "deviceId": "device-abc",
      "userAgent": "Mozilla/5.0...",
      "ipAddress": "192.168.1.1"
    },
    "accountLimits": {
      "dailyLimit": 50000,
      "remainingLimit": 45000,
      "monthlyLimit": 500000
    }
  }
}
```

### Calling Extension with Request

Extensions of type `GlobalAndRequested` or `DefinedFlowAndRequested` can be called with a query parameter:

```http
GET /api/v1/{domain}/workflows/{workflow}/instances/{instance}/functions/data?extensions=extension-user-session,extension-account-limits
```

---

## Best Practices

### 1. Extension Design

| Practice | Description |
|----------|-------------|
| Single responsibility | Each extension should enrich a single data source |
| Meaningful naming | Descriptive names with `extension-` prefix |
| Appropriate type selection | Correct ExtensionType selection based on need |
| Appropriate scope selection | Correct ExtensionScope selection based on need |

### 2. Performance

| Practice | Description |
|----------|-------------|
| Lightweight design | Extensions should run quickly |
| Caching | Use cache for infrequently changing data |
| Lazy loading | Load data only when needed |
| Timeout | Set appropriate timeout values |

### 3. Type and Scope Selection

| Scenario | Recommended Type | Recommended Scope |
|----------|------------------|-------------------|
| Always required system data | `Global (1)` | `Everywhere (3)` |
| User session info | `GlobalAndRequested (2)` | `GetInstance (1)` |
| Workflow-specific account info | `DefinedFlows (3)` | `GetInstance (1)` |
| On-demand detail info | `DefinedFlowAndRequested (4)` | `GetInstance (1)` |

### 4. Error Handling

| Practice | Description |
|----------|-------------|
| Graceful degradation | Extension error should not block main response |
| Timeout handling | Timeout for long-running extensions |
| Error logging | Proper logging of errors |
| Fallback values | Default values in case of error |

### 5. Security

| Practice | Description |
|----------|-------------|
| Data filtering | Filter sensitive data |
| Authorization | Limit data based on user permissions |
| Audit logging | Log data accesses |

---

## Common Errors

### 1. Extension Not Working

```
Extension 'extension-xyz' not found in response
```

**Possible Causes:**
- Extension type or scope mismatch
- Extension definition not loaded
- Request parameter missing (for Requested types)

**Solution:** Check type and scope settings.

### 2. Extension Timeout

```
Extension 'extension-xyz' timed out
```

**Solution:** Increase task timeout value or optimize extension logic.

### 3. Extension Data Error

```
Extension 'extension-xyz' returned invalid data
```

**Solution:** Check the mapping OutputHandler method and return correct data format.

---

## Related Documentation

- [📄 Custom Functions](./custom-function.md) - Custom function definitions
- [📄 Function APIs](./function.md) - Built-in system functions
- [📄 Task Management](./task.md) - Task types and usage
- [📄 Mapping Guide](./mapping.md) - Comprehensive mapping guide
- [📄 View Management](./view.md) - View usage with extensions

