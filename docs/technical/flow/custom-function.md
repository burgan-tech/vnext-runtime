# Custom Functions

Custom functions are components designed to reduce BFF (Backend for Frontend) API usage in the vNext platform. They work on instance data to provide endpoints to other domains or integrated services.

## Table of Contents

1. [Overview](#overview)
2. [Function Definition](#function-definition)
3. [Function Properties](#function-properties)
4. [Consumption Endpoints](#consumption-endpoints)
5. [System Functions](#system-functions)
6. [Usage Examples](#usage-examples)
7. [Best Practices](#best-practices)

---

## Overview

Custom functions are used for:

- **BFF API Reduction**: Reduces intermediate API layers by providing direct data access
- **Data Transformation**: Presents instance data in desired format via mapping
- **Task Execution**: Runs the defined task when function is called
- **Service Integration**: Provides endpoints to other domains or external services

:::highlight green 💡
Each function can execute a task and the task result data can be returned in the desired format via mapping.
:::

---

## Function Definition

### Basic Structure

```json
{
  "key": "function-get-user-info",
  "flow": "sys-functions",
  "domain": "core",
  "version": "1.0.0",
  "flowVersion": "1.0.0",
  "tags": [
    "system",
    "core",
    "users",
    "lookup"
  ],
  "attributes": {
    "scope": "I",
    "task": {
      "order": 1,
      "task": {
        "key": "get-user-info",
        "domain": "core",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "location": "./src/GetUserInfoMapping.csx",
        "code": "<BASE64_ENCODED_MAPPING_CODE>"
      }
    }
  }
}
```

---

## Function Properties

### Core Properties

| Property | Type | Description |
|----------|------|-------------|
| `key` | `string` | Unique identifier for the function |
| `flow` | `string` | Flow stream information (default: `sys-functions`) |
| `domain` | `string` | Domain the function belongs to |
| `version` | `string` | Version information (semantic versioning) |
| `flowVersion` | `string` | Flow version information |
| `tags` | `string[]` | Tags for categorization and searching |
| `attributes` | `object` | Function configuration |

### Attributes Properties

| Property | Type | Description |
|----------|------|-------------|
| `scope` | `string` | Function scope (`I` = Instance, `F` = Workflow, `D` = Domain) |
| `task` | `object` | Task definition to execute |

### Scope Values

| Value | Description | Access Level |
|-------|-------------|--------------|
| `I` | Instance | Works for a specific instance |
| `F` | Workflow | Works at workflow level |
| `D` | Domain | Works at domain level |

### Task Structure

```json
{
  "task": {
    "order": 1,
    "task": {
      "key": "task-key",
      "domain": "core",
      "version": "1.0.0",
      "flow": "sys-tasks"
    },
    "mapping": {
      "location": "./src/MappingFile.csx",
      "code": "<BASE64_ENCODED_CODE>"
    }
  }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `order` | `number` | Task execution order |
| `task` | `object` | Task reference |
| `mapping` | `object` | Input/Output transformation mapping |

---

## Consumption Endpoints

### Domain Level Functions

**Returns all domain instances and data:**

```http
GET /api/v1/{domain}/functions
```

**Returns result of a specific function:**

```http
GET /api/v1/{domain}/functions/{function}
```

### Workflow Level Functions

**Executes a function within a workflow:**

```http
GET /api/v1/{domain}/workflows/{workflow}/functions/{function}
```

### Instance Level Functions

**Executes a function for a specific instance:**

```http
GET /api/v1/{domain}/workflows/{workflow}/instances/{instance}/functions/{function}
```

---

## System Functions

The vNext platform provides ready-to-use system functions for every workflow instance:

### State Function

Returns the current state information of an instance.

**Endpoint:**
```http
GET /api/v1/{domain}/workflows/{workflow}/instances/{instance}/functions/state
```

**Response:**
```json
{
  "data": {
    "href": "/core/workflows/account-opening/instances/d4b161a8-7705-4bfb-9ba4-d76461bb35eb/functions/data?extensions=extension-user-session"
  },
  "view": {
    "loadData": true,
    "href": "/core/workflows/account-opening/instances/d4b161a8-7705-4bfb-9ba4-d76461bb35eb/functions/view"
  },
  "state": "account-type-selection",
  "status": "A",
  "activeCorrelations": [],
  "transitions": [
    {
      "name": "select-demand-deposit",
      "href": "/core/workflows/account-opening/instances/d4b161a8-7705-4bfb-9ba4-d76461bb35eb/transitions/select-demand-deposit"
    },
    {
      "name": "execute-sub",
      "href": "/core/workflows/account-opening/instances/d4b161a8-7705-4bfb-9ba4-d76461bb35eb/transitions/execute-sub"
    }
  ],
  "eTag": "01KCHWT3QQFM6J9QQD9G4T0VRP"
}
```

**Response Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `data.href` | `string` | Data function endpoint |
| `view.loadData` | `boolean` | Whether view requires data loading |
| `view.href` | `string` | View function endpoint |
| `state` | `string` | Current state name |
| `status` | `string` | Instance status (A=Active, C=Completed) |
| `activeCorrelations` | `array` | Active sub-correlations |
| `transitions` | `array` | Available transitions |
| `eTag` | `string` | ETag value for cache control |

### View Function

Returns the view data for the current state or transition of an instance.

**Endpoint:**
```http
GET /api/v1/{domain}/workflows/{workflow}/instances/{instance}/functions/view?transitionKey={transition}&platform={platform}
```

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `transitionKey` | `string` | View for specific transition (optional) |
| `platform` | `string` | Target platform: `web`, `ios`, `android` |

**Response:**
```json
{
  "key": "account-type-selection-view",
  "content": "{\"type\":\"form\",\"title\":{\"en-US\":\"Choose Your Account Type\",\"tr-TR\":\"Hesap Türünüzü Seçin\"},\"fields\":[...]}",
  "type": "Json",
  "display": "full-page",
  "label": ""
}
```

**Response Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `key` | `string` | View identifier |
| `content` | `string` | View content (in JSON format) |
| `type` | `string` | Content type (Json, Html, etc.) |
| `display` | `string` | Display mode (full-page, popup, bottom-sheet, etc.) |
| `label` | `string` | Localized label |

### Schema Function

Returns the schema data for the current state or transition of an instance.

**Endpoint:**
```http
GET /api/v1/{domain}/workflows/{workflow}/instances/{instance}/functions/schema?transitionKey={transition}
```

**Response:**
```json
{
  "key": "account-type-selection",
  "type": "workflow",
  "schema": {
    "$id": "https://schemas.vnext.com/banking/account-type-selection.json",
    "type": "object",
    "title": "Account Type Selection Schema",
    "$schema": "https://json-schema.org/draft/2020-12/schema",
    "required": ["accountType"],
    "properties": {
      "accountType": {
        "type": "string",
        "oneOf": [
          {
            "const": "demand-deposit",
            "description": "Vadesiz Hesap - Demand Deposit Account"
          },
          {
            "const": "time-deposit",
            "description": "Vadeli Hesap - Time Deposit Account"
          },
          {
            "const": "investment-account",
            "description": "Fonlu Hesap - Investment Account"
          },
          {
            "const": "savings-account",
            "description": "Tasarruf Hesabı - Savings Account"
          }
        ],
        "title": "Account Type",
        "description": "Type of account to be opened"
      }
    },
    "description": "Schema for account type selection input",
    "additionalProperties": false
  }
}
```

---

## Usage Examples

### Example 1: User Info Function

```json
{
  "key": "function-get-user-info",
  "flow": "sys-functions",
  "domain": "core",
  "version": "1.0.0",
  "flowVersion": "1.0.0",
  "tags": ["system", "core", "users", "lookup"],
  "attributes": {
    "scope": "I",
    "task": {
      "order": 1,
      "task": {
        "key": "get-user-info",
        "domain": "core",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "location": "./src/GetUserInfoMapping.csx",
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

public class GetUserInfoMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        try
        {
            var httpTask = task as HttpTask;
            if (httpTask == null)
                throw new InvalidOperationException("Task must be an HttpTask");

            var userId = context.Body?.userId;

            // Update URL with userId
            httpTask.SetUrl(httpTask.Url.Replace("{userId}", userId?.ToString() ?? ""));

            // Set Headers
            var headers = new Dictionary<string, string?>
            {
                ["Content-Type"] = "application/json",
                ["Accept"] = "application/json",
                ["X-Request-Id"] = Guid.NewGuid().ToString()
            };

            httpTask.SetHeaders(headers);

            return Task.FromResult(new ScriptResponse());
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ScriptResponse
            {
                Key = "user-info-error",
                Data = new { error = ex.Message }
            });
        }
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            var statusCode = context.Body?.statusCode ?? 500;
            var responseData = context.Body?.data;

            if (statusCode >= 200 && statusCode < 300)
            {
                return new ScriptResponse
                {
                    Key = "user-info-success",
                    Data = new
                    {
                        user = responseData,
                        phoneNumber = responseData?.phoneNumber,
                        hasRegisteredDevices = ((object[])responseData?.registeredDevices).Length > 0,
                        language = responseData?.language ?? "tr-TR"
                    },
                    Tags = new[] { "users", "lookup", "success" }
                };
            }
            else
            {
                return new ScriptResponse
                {
                    Key = "user-info-failure",
                    Data = new
                    {
                        error = "Failed to get user information",
                        errorCode = "user_info_failed",
                        statusCode = statusCode,
                        hasRegisteredDevices = false
                    },
                    Tags = new[] { "users", "lookup", "failure" }
                };
            }
        }
        catch (Exception ex)
        {
            return new ScriptResponse
            {
                Key = "user-info-exception",
                Data = new
                {
                    error = "Internal processing error",
                    errorCode = "processing_error",
                    errorDescription = ex.Message,
                    hasRegisteredDevices = false
                },
                Tags = new[] { "users", "lookup", "error" }
            };
        }
    }
}
```

### Example 2: Account Balance Function

```json
{
  "key": "function-get-account-balance",
  "flow": "sys-functions",
  "domain": "banking",
  "version": "1.0.0",
  "flowVersion": "1.0.0",
  "tags": ["banking", "accounts", "balance"],
  "attributes": {
    "scope": "I",
    "task": {
      "order": 1,
      "task": {
        "key": "get-balance",
        "domain": "banking",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "location": "./src/GetBalanceMapping.csx",
        "code": "<BASE64>"
      }
    }
  }
}
```

---

## Best Practices

### 1. Function Design

| Practice | Description |
|----------|-------------|
| Single responsibility | Each function should do one thing |
| Meaningful naming | Descriptive names with `function-` prefix |
| Appropriate scope | Correct scope selection based on need (I, W, D) |
| Version management | Use semantic versioning |

### 2. Mapping Development

| Practice | Description |
|----------|-------------|
| Error handling | Catch errors with try-catch blocks |
| Null checking | Write null-safe code (`?.` operator) |
| Logging | Add appropriate log messages |
| Performance | Avoid unnecessary operations |

### 3. Security

| Practice | Description |
|----------|-------------|
| Authorization | Proper authorization checks |
| Data validation | Perform input validation |
| Sensitive data | Mask sensitive data |
| Rate limiting | Apply request limits |

### 4. Performance

| Practice | Description |
|----------|-------------|
| Caching | Use appropriate cache strategy |
| Async operations | Use async/await for asynchronous operations |
| Timeout | Set appropriate timeout values |
| Resource management | Properly release resources |

---

## Related Documentation

- [Function APIs](./function.md) - Built-in system functions (State, Data, View)
- [Instance Filtering](./instance-filtering.md) - GraphQL-style filtering guide
- [Extension Management](./extension.md) - Data enrichment components
- [Task Management](./task.md) - Task types and usage
- [Mapping Guide](./mapping.md) - Comprehensive mapping guide

