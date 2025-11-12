# Notification Task

The Notification Task is a system-defined task that sends workflow state information to clients through socket/hub connections using long-polling pattern. This task enables real-time notifications to connected clients about workflow state changes.

## Table of Contents

1. [Overview](#overview)
2. [Task Definition](#task-definition)
3. [How It Works](#how-it-works)
4. [Configuration](#configuration)
5. [Dapr Binding Setup](#dapr-binding-setup)
6. [Usage Examples](#usage-examples)
7. [Best Practices](#best-practices)
8. [Troubleshooting](#troubleshooting)

## Overview

The Notification Task is a specialized task type that:
- Sends workflow state information to connected clients
- Uses socket/hub communication channels
- Implements long-polling pattern for real-time updates
- Is pre-defined by the system (no need to create task definition)
- Utilizes Dapr binding for communication

### Key Characteristics

| Property | Value |
|----------|-------|
| **Task Type** | Notification (Type: "G") |
| **System Defined** | Yes |
| **Task Key** | `notification-task` |
| **Domain** | `core` |
| **Flow** | `sys-tasks` |
| **Requires Custom Definition** | No |
| **Communication Method** | Dapr HTTP Binding |

## Task Definition

### Reference Schema

When using the Notification Task in your workflow, reference it as follows:

```json
{
  "order": 1,
  "task": {
    "key": "notification-task",
    "domain": "core",
    "version": "1.0.0",
    "flow": "sys-tasks"
  },
  "mapping": {
    "type": "G"
  }
}
```

### Schema Properties

| Property | Required | Description |
|----------|----------|-------------|
| `order` | Yes | Execution order within the state |
| `task.key` | Yes | Always `notification-task` (system-defined) |
| `task.domain` | Yes | Always `core` |
| `task.version` | Yes | Task version (e.g., `1.0.0`) |
| `task.flow` | Yes | Always `sys-tasks` |
| `mapping.type` | Yes | Always `"G"` for Notification Task |

### Important Notes

- **System-Defined**: The `notification-task` is predefined by the system. Developers do not need to create a separate task definition file.
- **No Custom Mapping**: Unlike other tasks, notification tasks use a specific mapping type (`"G"`) and do not require custom mapping scripts.
- **Automatic Execution**: The task automatically sends state information when executed.

## How It Works

### Execution Flow

```
1. Workflow reaches state with NotificationTask
   ↓
2. NotificationTask executes in order
   ↓
3. Task retrieves current workflow state information
   ↓
4. State data sent via Dapr HTTP binding
   ↓
5. Dapr forwards to configured notification hub/socket
   ↓
6. Connected clients receive state update
```

### Data Sent to Clients

The notification task sends the same information as the State Function API:

```json
{
  "data": {
    "href": "/domain/workflows/workflow-key/instances/instance-id/functions/data"
  },
  "view": {
    "href": "/domain/workflows/workflow-key/instances/instance-id/functions/view",
    "loadData": true
  },
  "state": "current-state",
  "status": "A",
  "activeCorrelations": [...],
  "transitions": [
    {
      "href": "/domain/workflows/workflow-key/instances/instance-id/transitions/transition-key",
      "name": "transition-key"
    }
  ],
  "eTag": "etag-value"
}
```

## Configuration

### 1. Dapr Binding Component

Create a Dapr HTTP binding component file: `notification-http-binding.yaml`

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: notification-http-binding
spec:
  type: bindings.http
  version: v1
  metadata:
  - name: url
    value: "http://your-notification-hub:port/api/notifications"
  - name: method
    value: "POST"
  - name: headers
    value: "Content-Type: application/json"
```

**Component Properties:**

| Property | Description | Example |
|----------|-------------|---------|
| `metadata.name` | Binding component name | `notification-http-binding` |
| `spec.type` | Binding type | `bindings.http` |
| `metadata.url` | Target notification hub URL | `http://notification-hub:8080/api/notify` |
| `metadata.method` | HTTP method | `POST` |
| `metadata.headers` | HTTP headers | `Content-Type: application/json` |

### 2. Execution API Configuration

In your Execution API `appsettings.json` (or environment-specific file like `appsettings.Execution.Development.json`), configure the binding name:

```json
{
  "Dapr": {
    "NotificationBinding": {
      "Name": "notification-http-binding"
    }
  }
}
```

**Configuration Location:**
- File: `appsettings.Execution.Development.json`
- Lines: 123-125 (approximately)
- Property: `Dapr.NotificationBinding.Name`

## Usage Examples

### Example 1: Basic Notification in Workflow

```json
{
  "key": "user-registration-workflow",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-flows",
  "attributes": {
    "type": "F",
    "states": [
      {
        "key": "email-verification",
        "stateType": 1,
        "onEntries": [
          {
            "order": 1,
            "task": {
              "key": "send-verification-email",
              "domain": "core",
              "version": "1.0.0",
              "flow": "sys-tasks"
            }
          },
          {
            "order": 2,
            "task": {
              "key": "notification-task",
              "domain": "core",
              "version": "1.0.0",
              "flow": "sys-tasks"
            },
            "mapping": {
              "type": "G"
            }
          }
        ]
      }
    ]
  }
}
```

In this example:
1. Email verification email is sent
2. Notification task notifies connected clients about the state change
3. Clients receive update and can display appropriate UI

### Example 2: Multiple State Notifications

```json
{
  "key": "order-processing-workflow",
  "states": [
    {
      "key": "order-created",
      "onEntries": [
        {
          "order": 1,
          "task": {
            "key": "create-order-record",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          }
        },
        {
          "order": 2,
          "task": {
            "key": "notification-task",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          },
          "mapping": {
            "type": "G"
          }
        }
      ]
    },
    {
      "key": "payment-processing",
      "onEntries": [
        {
          "order": 1,
          "task": {
            "key": "process-payment",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          }
        },
        {
          "order": 2,
          "task": {
            "key": "notification-task",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          },
          "mapping": {
            "type": "G"
          }
        }
      ]
    },
    {
      "key": "order-completed",
      "onEntries": [
        {
          "order": 1,
          "task": {
            "key": "notification-task",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          },
          "mapping": {
            "type": "G"
          }
        }
      ]
    }
  ]
}
```

Notifications are sent at each critical state:
- When order is created
- During payment processing
- When order is completed

### Example 3: Conditional Notifications

Use state transitions strategically to notify only when needed:

```json
{
  "key": "approval-workflow",
  "states": [
    {
      "key": "pending-approval",
      "onEntries": [
        {
          "order": 1,
          "task": {
            "key": "notification-task",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          },
          "mapping": {
            "type": "G"
          }
        }
      ]
    },
    {
      "key": "approved",
      "onEntries": [
        {
          "order": 1,
          "task": {
            "key": "update-approval-status",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          }
        },
        {
          "order": 2,
          "task": {
            "key": "notification-task",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          },
          "mapping": {
            "type": "G"
          }
        }
      ]
    }
  ]
}
```

## Best Practices

### 1. Strategic Placement

Place notification tasks at states where clients need immediate updates:
- After critical business operations
- At decision points requiring user interaction
- When entering waiting states
- At workflow completion

### 2. Order Management

Execute notification tasks after data-modifying tasks:

```json
{
  "onEntries": [
    {
      "order": 1,
      "task": { /* Data modification task */ }
    },
    {
      "order": 2,
      "task": { /* notification-task */ },
      "mapping": { "type": "G" }
    }
  ]
}
```

This ensures clients receive the most up-to-date state information.

### 3. Notification Hub Design

Design your notification hub for scalability:
- Use SignalR or similar for WebSocket connections
- Implement connection groups by instance ID
- Handle reconnection logic on client side
- Implement heartbeat/keep-alive mechanism

### 4. Security

Secure your notification channel:
- Authenticate clients before allowing hub connection
- Validate authorization for instance-specific notifications
- Use encrypted connections (WSS/HTTPS)
- Include authentication tokens in Dapr binding headers

### 5. Error Handling

Handle notification failures gracefully:
- Notification task failures should not block workflow progression
- Implement retry logic in your notification hub
- Log notification failures for monitoring
- Provide fallback to polling if real-time notifications fail

### 6. Performance Considerations

Optimize notification performance:
- Use connection pooling in notification hub
- Implement message batching for high-volume scenarios
- Cache frequently accessed state information
- Monitor notification latency

### 7. Testing

Test notification functionality:
- Unit test notification hub endpoints
- Integration test with Dapr binding
- Load test with multiple concurrent clients
- Test reconnection scenarios

## Related Documentation

- [State Function API](../function.md#state-function) - State information structure
- [HTTP Task](./http-task.md) - HTTP task configuration
- [Dapr Bindings](https://docs.dapr.io/reference/components-reference/supported-bindings/) - Dapr binding documentation
- [Task Types](../task.md) - Overview of all task types
- [Workflow Definition](../flow.md) - Workflow structure and configuration

## Summary

The Notification Task is a powerful system-defined task that enables real-time workflow state updates to connected clients. Key points to remember:

- ✅ System-defined, no custom task definition needed
- ✅ Uses Dapr HTTP binding for communication
- ✅ Requires `notification-http-binding.yaml` component
- ✅ Configure binding name in `appsettings.json`
- ✅ Always use mapping type `"G"`
- ✅ Place strategically in workflow states
- ✅ Execute after data-modifying tasks

By following this guide, you can implement real-time notifications in your workflows, providing users with immediate feedback on workflow state changes.

