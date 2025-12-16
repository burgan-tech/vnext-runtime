# vNext Runtime - Documentation

This documentation provides a comprehensive guide for developers on the vNext Runtime platform. The platform is a cloud-based application development platform that supports low-code, no-code, and full-code development.

## 📋 Table of Contents

- [🏗️ Platform Architecture](#️-platform-architecture)
- [🔄 Workflow Components](#-workflow-components)
- [📚 Documentation Map](#-documentation-map)
- [🚀 Quick Start](#-quick-start)
- [💡 Examples and Use Cases](#-examples-and-use-cases)

---

## 🏗️ Platform Architecture

The vNext Platform has a horizontally scalable service cluster and can perform all kinds of workflows and functions with high security by providing interfaces to customers, employees, and systems through frontend applications managed by these services.

### Core Principles

- **Dual-Write Pattern**: Event Sourcing and Replication support
- **Domain-Driven Architecture**: Each domain runs with a separate runtime
- **Microservice Ready**: Service mesh support with Dapr integration
- **Semantic Versioning**: Backward-compatible version management
- **ETag Pattern**: Concurrent update control

### Architectural Components

![Components](https://kroki.io/mermaid/svg/eNpVzt0KgkAQBeD7nmJeQHqDwJ_VDIpohS4GL0abbMlWWdef3j5ZvbC5OvAdOFMZal-QBTuYz8ewVqwt-G1bq5KsanSXg-cdIED_mkJClkf65q4dOAhxuPBk4dZrqz68UOgowntj3s-6GUHoSuk_FJhR9wYxcdnbxmwpRmnnITiTpopXihwlGFFrQKoHl7SKWMTleJMTl48oJstGUw2SzaBK7vINp-uWnH_gLZzw2hd72Rf5D2aNUNQ)

---

## 🔄 Workflow Components

### 1. **Workflow**
The main component that defines business processes. Manages workflows through states and transitions.

### 2. **State**
Represents the stage where the workflow is located. Can be defined in four different types:
- **Initial**: Starting state
- **Intermediate**: Intermediate states
- **Finish**: End state
- **SubFlow**: Sub-flow state

### 3. **Transition**
Component that manages transitions between states. Four different trigger types:
- **Manual (0)**: User interaction
- **Automatic (1)**: Conditional automatic transition
- **Scheduled (2)**: Scheduled transition
- **Event (3)**: Event-based transition

### 4. **Task**
Independent components that perform specific operations within the workflow:
- **DaprService**: Dapr service invocation
- **DaprPubSub**: Dapr pub/sub messaging
- **Http**: HTTP web service calls
- **Script**: C# Roslyn script execution
- **Condition**: Condition checking
- **Timer**: Timer tasks

---

## 📚 Documentation Map

### 🔧 Core Concepts
| Topic | File | Description |
|-------|------|-------------|
| **Platform Fundamentals** | [`fundamentals/readme.md`](./fundamentals/readme.md) | Platform structure and core principles |
| **Domain Topology** | [`fundamentals/domain-topology.md`](./fundamentals/domain-topology.md) | Domain concept, isolation, and multi-domain architecture |
| **Database Architecture** | [`fundamentals/database-architecture.md`](./fundamentals/database-architecture.md) | Multi-schema structure, migration system, and DB isolation |
| **Persistence** | [`principles/persistance.md`](./principles/persistance.md) | Data storage and Dual-Write Pattern |
| **Reference Schema** | [`principles/reference.md`](./principles/reference.md) | Inter-component reference management |
| **Version Management** | [`principles/versioning.md`](./principles/versioning.md) | Versioning, package management, and deployment strategy |

### 🌊 Workflow (Flow) Documentation
| Topic | File | Description |
|-------|------|-------------|
| **Workflow Definition** | [`flow/flow.md`](./flow/flow.md) | Workflow definition and development guide |
| **Interfaces** | [`flow/interface.md`](./flow/interface.md) | Mapping interfaces and usage |
| **Mapping Guide** | [`flow/mapping.md`](./flow/mapping.md) | Comprehensive mapping guide and examples |
| **State Management** | [`flow/state.md`](./flow/state.md) | State types and lifecycle |
| **Task Definitions** | [`flow/task.md`](./flow/task.md) | Task types and usage areas |
| **Transition Management** | [`flow/transition.md`](./flow/transition.md) | Transition types and trigger mechanisms |
| **View Management** | [`flow/view.md`](./flow/view.md) | View definitions, display strategies, and platform overrides |
| **Schema Management** | [`flow/schema.md`](./flow/schema.md) | Schema definitions, JSON Schema validation, and data integrity |
| **Function APIs** | [`flow/function.md`](./flow/function.md) | System function APIs (State, Data, View) |
| **Custom Functions** | [`flow/custom-function.md`](./flow/custom-function.md) | User-defined functions with task execution |
| **Extension Management** | [`flow/extension.md`](./flow/extension.md) | Data enrichment components for instance responses |

### 📋 Task Details
| Task Type | File | Usage Area |
|-----------|------|------------|
| **HTTP Task** | [`flow/tasks/http-task.md`](./flow/tasks/http-task.md) | REST API calls and web service integrations |
| **Script Task** | [`flow/tasks/script-task.md`](./flow/tasks/script-task.md) | Business logic and calculation operations with C# |
| **DaprService Task** | [`flow/tasks/dapr-service.md`](./flow/tasks/dapr-service.md) | Microservice calls |
| **DaprPubSub Task** | [`flow/tasks/dapr-pubsub.md`](./flow/tasks/dapr-pubsub.md) | Asynchronous messaging |
| **Condition Task** | [`flow/tasks/condition-task.md`](./flow/tasks/condition-task.md) | Condition checking and decision mechanisms |
| **Timer Task** | [`flow/tasks/timer-task.md`](./flow/tasks/timer-task.md) | Scheduling and periodic operations |
| **Notification Task** | [`flow/tasks/notification-task.md`](./flow/tasks/notification-task.md) | Real-time state notifications via socket/hub |

### 🛠️ How-To
| Topic | File | Description |
|-------|------|-------------|
| **Instance Startup** | [`how-to/start-instance.md`](./how-to/start-instance.md) | Instance lifecycle and component management |

---

## 🚀 Quick Start

### 1. Starting an Instance

```http
POST /:domain/workflows/:flow/instances/start?sync=true
Content-Type: application/json

{
    "key": "unique-instance-key",
    "tags": ["example", "demo"],
    "attributes": {
        "userId": 123,
        "amount": 1000,
        "currency": "TL"
    }
}
```

### 2. Running a Transition

```http
PATCH /:domain/workflows/:flow/instances/:instanceId/transitions/:transitionKey?sync=true
Content-Type: application/json

{
    "approvedBy": "admin",
    "approvalDate": "2025-09-20T10:30:00Z"
}
```

### 3. Querying Instance Status

```http
GET /:domain/workflows/:flow/instances/:instanceId
If-None-Match: "etag-value"
```

---

## 💡 Examples and Use Cases

### 🛒 E-commerce Workflow
**File Location**: `../../samples/ecommerce/`

Comprehensive e-commerce example including cart management, payment processing, and order tracking.

**Main Components**:
- Add product to cart (HTTP Task)
- User authentication (Script Task)
- Payment processing (HTTP Task + Condition)

### 🔐 OAuth Authentication Workflow
**File Location**: `../../samples/oauth/`

OAuth2 authentication flow with MFA (Multi-Factor Authentication) support.

**Main Components**:
- Client validation
- User credentials validation
- OTP/Push notification MFA
- Token generation

### 💳 Scheduled Payments Workflow
**File Location**: `../../samples/payments/`

Periodic payment processing and notification system.

**Main Components**:
- Payment schedule management
- Timer-based execution
- Notification system (SMS + Push)
- Retry mechanism

---

## 🔍 Interface Usage

### IMapping - Basic Mapping
```csharp
public class CustomMapping : IMapping
{
    public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Prepare input data
        return new ScriptResponse { Data = inputData };
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        // Process output data
        return new ScriptResponse { Data = outputData };
    }
}
```

### ITimerMapping - Scheduling
```csharp
public class PaymentTimerRule : ITimerMapping
{
    public async Task<TimerSchedule> Handler(ScriptContext context)
    {
        var frequency = context.Instance.Data.frequency;
        return frequency switch
        {
            "daily" => TimerSchedule.FromCronExpression("0 9 * * *"),
            "weekly" => TimerSchedule.FromCronExpression("0 9 * * 1"),
            "monthly" => TimerSchedule.FromCronExpression("0 9 1 * *"),
            _ => TimerSchedule.FromDuration(TimeSpan.FromDays(30))
        };
    }
}
```

### IConditionMapping - Condition Checking
```csharp
public class AuthSuccessRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        return context.Instance.Data.authentication?.success == true;
    }
}
```

### ITransitionMapping - Transition Payload Mapping
```csharp
public class ApprovalTransitionMapping : ScriptBase, ITransitionMapping
{
    public async Task<dynamic> Handler(ScriptContext context)
    {
        LogInformation("Processing approval transition");
        
        return new
        {
            approval = new
            {
                approvedBy = context.Body?.userId,
                approvedAt = DateTime.UtcNow,
                status = "approved"
            }
        };
    }
}
```

---

## 🎯 Best Practices

### ✅ Do's
- Use **semantic versioning**
- Implement concurrent update control with **ETag pattern**
- Write **null-safe** code (`?.` operator)
- Implement error management with **try-catch**
- Use **camelCase** property naming

### ❌ Don'ts
- Don't make **HTTP calls** in Script Tasks
- Don't create **circular references**
- Don't disable **SSL validation** in production
- Don't access **dynamic properties** without null checking

---

## 🔧 Development Tools

### Reference Usage
```json
{
  "task": {
    "ref": "Tasks/validate-client.json"
  }
}
```

Automatically converted to full reference during build:
```json
{
  "task": {
    "key": "validate-client",
    "domain": "core", 
    "version": "1.0.0",
    "flow": "sys-tasks"
  }
}
```

### ScriptBase Usage
```csharp
public class SecureMapping : ScriptBase, IMapping
{
    public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Get secure data from secret store
        var apiKey = GetSecret("dapr_store", "secret_store", "api_key");
        
        // Safe property access
        if (HasProperty(context.Instance.Data, "sensitiveData"))
        {
            var data = GetPropertyValue(context.Instance.Data, "sensitiveData");
            // Operations...
        }
        
        return new ScriptResponse();
    }
}
```

---

## 📞 Support and Contribution

This documentation is continuously updated. For questions or contributions:

- **Issues**: You can open issues on the GitHub repository
- **Documentation**: You can send PRs for missing or incorrect information
- **Examples**: You can contribute new example scenarios

---

## 🔗 Related Repositories

### vNext Ecosystem

- **[vNext Engine](https://github.com/burgan-tech/vnext)** - Main workflow engine and runtime
- **[vNext Sys-Flows](https://github.com/burgan-tech/vnext-sys-flow)** - System component workflows
- **[vNext Schema](https://github.com/burgan-tech/vnext-schema)** - System component schema structure
- **[vNext CLI](https://github.com/burgan-tech/vnext-workflow-cli)** - Command line tools

---

## 📄 License

This documentation and examples are developed by Burgan Technology.
