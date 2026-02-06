# Error Boundary

Error Boundary provides a comprehensive error handling system with multi-level error resolution, priority-based execution, and enterprise-grade error recovery capabilities.

## Overview

The vNext workflow system implements a hierarchical error handling mechanism that allows you to define error policies at three levels:

1. **Task Level** - Most specific, applied to individual task executions
2. **State Level** - Applied when no task-level boundary handles the error
3. **Global Level** - Workflow-wide fallback when no lower-level boundary matches

> **Important:** ErrorBoundary definitions work at **task execution** level. Regardless of where the boundary is defined (global, state, or task), actions are taken based on task execution errors.

---

## Error Resolution Hierarchy

When an error occurs during task execution, the system evaluates error boundaries in the following order:

```
Task-level errorBoundary (most specific)
   ↓ (if no match, automatically check next level)
State-level errorBoundary
   ↓ (if no match, automatically check next level)
Global-level errorBoundary
   ↓ (if still no match)
Default system behavior (throw exception)
```

Within each level, rules are evaluated by **priority** (lower number = higher priority).

---

## ErrorBoundary Structure

```json
{
  "errorBoundary": {
    "onError": [
      {
        "action": 0,
        "errorTypes": ["ValidationException"],
        "errorCodes": ["Task:400007"],
        "transition": "error-state",
        "priority": 10,
        "retryPolicy": {
          "maxRetries": 3,
          "initialDelay": "PT5S",
          "backoffType": 1,
          "backoffMultiplier": 2.0,
          "maxDelay": "PT1M",
          "useJitter": true
        },
        "logOnly": false
      }
    ]
  }
}
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `onError` | array | Error handling rules evaluated in priority order |

> **Note:** `onTimeout` property exists in schema but is **not yet implemented**.

---

## Error Handler Rule

Each rule in the `onError` array defines how to handle specific errors.

### Properties

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `action` | int | Yes | - | Action to take (see Error Actions) |
| `errorTypes` | string[] | No | `["*"]` | Exception type names to match |
| `errorCodes` | string[] | No | `["*"]` | Error codes to match |
| `transition` | string | No | - | Transition key to trigger |
| `priority` | int | No | 100 | Rule priority (lower = higher priority) |
| `retryPolicy` | object | No | - | Retry configuration |
| `logOnly` | boolean | No | false | Only log, don't affect flow |

### Error Matching

- **errorTypes**: Exception class names (e.g., `ValidationException`, `TimeoutException`)
- **errorCodes**: Error codes in format `Category:Code` or just `Code` (e.g., `Task:400007`, `500`)
- Empty array or `["*"]` matches all errors

### Priority System

- Lower values are evaluated first
- Default priority: `100`
- Wildcard rules should use: `999`
- Recommended ranges:
  - Critical handlers: `1-10`
  - Specific handlers: `10-50`
  - General handlers: `50-100`
  - Fallback handlers: `100-999`

---

## Error Actions

| Code | Action | Description |
|------|--------|-------------|
| `0` | **Abort** | Abort execution, optionally trigger error transition |
| `1` | **Retry** | Retry the task with configured retry policy |
| `2` | **Rollback** | Rollback to compensation state |
| `3` | **Ignore** | Ignore error and continue to next task |
| `4` | **Notify** | Send notification and optionally transition |
| `5` | **Log** | Log only, does not affect flow |

---

## Retry Policy

Configure retry behavior for the `Retry` action.

```json
{
  "retryPolicy": {
    "maxRetries": 3,
    "initialDelay": "PT5S",
    "backoffType": 1,
    "backoffMultiplier": 2.0,
    "maxDelay": "PT1M",
    "useJitter": true
  }
}
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `maxRetries` | int | 3 | Maximum retry attempts |
| `initialDelay` | string | - | Initial delay (ISO 8601 duration) |
| `backoffType` | int | 1 | 0: Fixed, 1: Exponential |
| `backoffMultiplier` | number | 2.0 | Multiplier for exponential backoff |
| `maxDelay` | string | - | Maximum delay between retries (ISO 8601 duration) |
| `useJitter` | boolean | true | Add random jitter to prevent thundering herd |

### Backoff Types

| Code | Type | Description |
|------|------|-------------|
| `0` | Fixed | Same delay between each retry |
| `1` | Exponential | Delay doubles (or multiplies) with each retry |

### Duration Format (ISO 8601)

- `PT5S` - 5 seconds
- `PT30S` - 30 seconds
- `PT1M` - 1 minute
- `PT5M` - 5 minutes
- `PT1H` - 1 hour

---

## Examples

### 1. Global Level ErrorBoundary

Define at workflow `attributes` level for workflow-wide error handling:

```json
{
  "key": "payment-workflow",
  "domain": "banking",
  "version": "1.0.0",
  "attributes": {
    "type": "F",
    "errorBoundary": {
      "onError": [
        {
          "action": 0,
          "errorCodes": ["*"],
          "transition": "error-state",
          "priority": 999
        }
      ]
    },
    "states": [...]
  }
}
```

### 2. State Level ErrorBoundary

Define at state level for state-specific error handling:

```json
{
  "key": "processing",
  "stateType": 2,
  "versionStrategy": "Minor",
  "labels": [...],
  "errorBoundary": {
    "onError": [
      {
        "action": 1,
        "errorTypes": ["TransientException"],
        "priority": 10,
        "retryPolicy": {
          "maxRetries": 5,
          "initialDelay": "PT10S",
          "backoffType": 1,
          "backoffMultiplier": 2.0,
          "maxDelay": "PT5M",
          "useJitter": true
        }
      },
      {
        "action": 0,
        "errorCodes": ["*"],
        "transition": "failed",
        "priority": 100
      }
    ]
  },
  "transitions": [...]
}
```

### 3. Task Level ErrorBoundary

Define at task execution level for task-specific error handling:

```json
{
  "onExecutionTasks": [
    {
      "order": 1,
      "task": {
        "key": "call-external-api",
        "domain": "core",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "ref": "Mappings/api-call-mapping.json"
      },
      "errorBoundary": {
        "onError": [
          {
            "action": 1,
            "errorCodes": ["Task:503", "Task:504"],
            "priority": 1,
            "retryPolicy": {
              "maxRetries": 3,
              "initialDelay": "PT5S",
              "backoffType": 1,
              "backoffMultiplier": 2.0
            }
          },
          {
            "action": 3,
            "errorCodes": ["Task:404"],
            "priority": 2,
            "logOnly": true
          }
        ]
      }
    }
  ]
}
```

### 4. Multiple Rules with Priority

```json
{
  "errorBoundary": {
    "onError": [
      {
        "_comment": "Handle validation errors - abort immediately",
        "action": 0,
        "errorTypes": ["ValidationException"],
        "transition": "validation-failed",
        "priority": 1
      },
      {
        "_comment": "Retry transient failures",
        "action": 1,
        "errorCodes": ["Task:503", "Task:504", "Task:429"],
        "priority": 10,
        "retryPolicy": {
          "maxRetries": 5,
          "initialDelay": "PT5S",
          "backoffType": 1
        }
      },
      {
        "_comment": "Log and ignore non-critical errors",
        "action": 5,
        "errorCodes": ["Task:204"],
        "priority": 20,
        "logOnly": true
      },
      {
        "_comment": "Fallback - abort with error transition",
        "action": 0,
        "errorCodes": ["*"],
        "transition": "error-state",
        "priority": 999
      }
    ]
  }
}
```

### 5. Exponential Backoff with Jitter

```json
{
  "errorBoundary": {
    "onError": [
      {
        "action": 1,
        "errorTypes": ["*"],
        "priority": 100,
        "retryPolicy": {
          "maxRetries": 5,
          "initialDelay": "PT1S",
          "backoffType": 1,
          "backoffMultiplier": 2.0,
          "maxDelay": "PT30S",
          "useJitter": true
        }
      }
    ]
  }
}
```

Retry delays with jitter (approximate):
1. ~1s (+ random 0-500ms)
2. ~2s (+ random 0-1000ms)
3. ~4s (+ random 0-2000ms)
4. ~8s (+ random 0-4000ms)
5. ~16s (+ random 0-8000ms, capped at 30s)

---

## Best Practices

### 1. Use Priority Wisely

```json
{
  "onError": [
    { "action": 0, "errorTypes": ["ValidationException"], "priority": 1 },
    { "action": 1, "errorCodes": ["Task:503"], "priority": 10 },
    { "action": 0, "errorCodes": ["*"], "priority": 999 }
  ]
}
```

### 2. Always Have a Fallback

Include a wildcard rule with high priority number as fallback:

```json
{
  "action": 0,
  "errorCodes": ["*"],
  "transition": "error-state",
  "priority": 999
}
```

### 3. Use Appropriate Retry Policies

- **Transient errors** (503, 504, 429): Retry with exponential backoff
- **Validation errors**: Abort immediately, no retry
- **Business errors**: Route to error handling state

### 4. Leverage Hierarchy

- **Task level**: Specific retry policies for external API calls
- **State level**: Common error handling for state operations
- **Global level**: Fallback and notification for unhandled errors

### 5. Use logOnly for Debugging

```json
{
  "action": 5,
  "errorCodes": ["*"],
  "priority": 1,
  "logOnly": true
}
```

---

## Related Documentation

- [Workflow Definition](./flow.md) - Workflow structure and components
- [State Management](./state.md) - State definitions and types
- [Task Management](./task.md) - Task types and execution
- [Transition Management](./transition.md) - Transitions and triggers
