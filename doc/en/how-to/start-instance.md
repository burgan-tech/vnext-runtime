# Instance Startup and Component Management

This documentation explains how to start an instance in the vNext Runtime system and how to register and activate system components.

## Instance Lifecycle

### 1. Starting an Instance

The `start` endpoint is always used to start a workflow. The system returns an `id` value as a response. This id value is the created instance id, and the subsequent transition process proceeds with this id.

> **v0.0.23 Change**: The `key` field is no longer mandatory. It can be left empty at start and assigned during subsequent transitions.

**Endpoint:**
```
POST /:domain/workflows/:flow/instances/start?sync=true
```

**Payload Schema:**
```json
{
    "key": "",
    "tags": [],
    "attributes": {}
}
```

> **Note**: All fields are optional.

**Key Behavior:**
- If `key` value is provided and the current instance key is empty, it will be saved
- If key is not provided at start, it can be assigned during subsequent transitions

> **v0.0.29 Change - Idempotent Start:** When starting a workflow with a key that already exists, the system now returns the existing instance information instead of an error. This enables safe retry scenarios.

**Idempotent Behavior:**
- **Previous behavior (before v0.0.29):** Returns `409 Conflict` error when key already exists
- **New behavior (v0.0.29+):** Returns the existing instance's current status and ID

This change allows:
- Safe retry scenarios for network failures
- Clients to retrieve the original start response on repeated calls
- No need for separate "check if exists" calls before starting

**Example Request (With Key):**
```http
POST /ecommerce/workflows/scheduled-payments/instances/start?sync=true
Content-Type: application/json

{
    "key": "99999999999",
    "tags": [
        "test",
        "oauth",
        "authentication"
    ],
    "attributes": {
        "userId": 454,
        "amount": 12000,
        "currency": "TL",
        "frequency": "monthly",
        "startDate": "2025-10-01T09:02:38.201Z",
        "endDate": "2026-10-01T09:02:38.201Z",
        "paymentMethodId": "1",
        "description": "Tayfun payment",
        "recipientId": "324324",
        "isAutoRetry": false,
        "maxRetries": 3
    }
}
```

**Example Request (Without Key):**
```http
POST /ecommerce/workflows/scheduled-payments/instances/start?sync=true
Content-Type: application/json

{
    "tags": ["priority", "express"],
    "attributes": {
        "userId": 454,
        "amount": 12000
    }
}
```

**Example Response:**
```json
{
  "id": "18075ad5-e5b2-4437-b884-21d733339113",
  "status": "A"
}
```

**Example Response (Idempotent - When Key Already Exists):**

When you call the start endpoint with a key that already has an active instance, you receive the existing instance information:

```http
POST /ecommerce/workflows/scheduled-payments/instances/start?sync=true
Content-Type: application/json

{
    "key": "99999999999"
}
```

```json
{
  "id": "18075ad5-e5b2-4437-b884-21d733339113",
  "status": "A"
}
```

> **Note:** The response returns the existing instance's ID and current status. No new instance is created, and no error is returned.

### 2. Instance Transition

The transition endpoint is used to advance the started instance. You can use either the instance ID (UUID) or the instance Key to reference the instance.

> **v0.0.23 Change**: The transition payload schema has been updated. A structured format with `key`, `tags`, and `attributes` fields is now used.

**Endpoint:**
```
PATCH /:domain/workflows/:flow/instances/:instanceIdOrKey/transitions/:transition?sync=true
```

> **Note**: The `:instanceIdOrKey` parameter accepts either:
> - **Instance ID**: The UUID returned when the instance was created (e.g., `18075ad5-e5b2-4437-b884-21d733339113`)
> - **Instance Key**: The key value provided during instance creation (e.g., `99999999999`)

**New Transition Payload Schema:**
```json
{
    "key": "",
    "tags": [],
    "attributes": {}
}
```

> **Note**: All fields are optional.

**Key Assignment Behavior:**
- If a `key` value is sent during transition and the current instance key is empty, it will be saved
- This allows assigning keys to instances that were created without a key

**Example Request (using Instance ID):**
```http
PATCH /ecommerce/workflows/scheduled-payments/instances/18075ad5-e5b2-4437-b884-21d733339113/transitions/activate?sync=true
Content-Type: application/json

{
    "attributes": {
        "approvedBy": "admin",
        "approvalDate": "2025-09-20T10:30:00Z"
    }
}
```

**Example Request (using Instance Key):**
```http
PATCH /ecommerce/workflows/scheduled-payments/instances/99999999999/transitions/activate?sync=true
Content-Type: application/json

{
    "attributes": {
        "approvedBy": "admin",
        "approvalDate": "2025-09-20T10:30:00Z"
    }
}
```

**Example Request (Assigning Key During Transition):**
```http
PATCH /ecommerce/workflows/scheduled-payments/instances/18075ad5-e5b2-4437-b884-21d733339113/transitions/assign-key?sync=true
Content-Type: application/json

{
    "key": "ORDER-2024-001",
    "tags": ["assigned"],
    "attributes": {
        "assignedBy": "system"
    }
}
```

> **Benefits of Using Instance Key:**
> - More readable and meaningful identifiers (e.g., order numbers, customer IDs)
> - Easier integration with external systems that use business keys
> - No need to store and manage UUIDs separately

### 3. Querying Instance Status

The GET endpoint is used to query the current status and data of the instance. This endpoint works with the ETag pattern. You can use either the instance ID or instance Key.

**Endpoint:**
```
GET /:domain/workflows/:flow/instances/:instanceIdOrKey
```

**Example Request (using Instance ID):**
```http
GET /ecommerce/workflows/scheduled-payments/instances/18075ad5-e5b2-4437-b884-21d733339113
If-None-Match: "18075ad5-e5b2-4437-b884-21d733339113"
```

**Example Request (using Instance Key):**
```http
GET /ecommerce/workflows/scheduled-payments/instances/99999999999
If-None-Match: "18075ad5-e5b2-4437-b884-21d733339113"
```

**Example Response:**
```json
{
  "id": "18075ad5-e5b2-4437-b884-21d733339113",
  "key": "99999999999",
  "flow": "scheduled-payments",
  "domain": "core",
  "flowVersion": "1.0.1",
  "eTag": "18075ad5-e5b2-4437-b884-21d733339113",
  "tags": [],
  "attributes": {},
  "extensions": {},
  "sortValue": ""
}
```

### 4. Filtering Instances

Use the filtering capability to query instances based on various criteria.

**Endpoint:**
```http
GET /{domain}/workflows/{workflow}/functions/data?filter={...}
```

**Example - Filter by Status:**
```http
GET /ecommerce/workflows/scheduled-payments/functions/data?filter={"status":{"eq":"Active"}}
```

**Example - Filter by JSON Data Field:**
```http
GET /ecommerce/workflows/scheduled-payments/functions/data?filter={"attributes":{"amount":{"gt":"1000"}}}
```

**Example - Combined Filter with Logical OR:**
```http
GET /ecommerce/workflows/scheduled-payments/functions/data?filter={"or":[{"status":{"eq":"Active"}},{"status":{"eq":"Busy"}}]}
```

> **Detailed Documentation:** For complete filtering syntax, operators, and aggregations, see [Instance Filtering Guide](../flow/instance-filtering.md).

## System Components and Registration Process

### Component Types

The following component types exist in the system:

- **sys-flows**: Workflow definitions
- **sys-tasks**: Task definitions
- **sys-views**: View definitions
- **sys-schemas**: Schema definitions
- **sys-extensions**: Extension definitions
- **sys-functions**: Function definitions

### Component Registration Process

Default workflows are provided in the system for each component, and all components are registered and activated through these special workflows, and their versions are managed.

#### sys-flows Component Lifecycle

The following Mermaid diagram shows the states and transitions of the sys-flows component:

![Sys-Flows](https://kroki.io/mermaid/svg/eNqNkbFqwzAQhvc-xc0FLx07FAKBdCykSykdrtYlOSzLQZY99GU8dvaeLfZ79SQ5jp2kEA3G3P3677tfpUNHS8atxTypnx5AzufjFyTJCyiLGwfPkFoSUWiFT6x7BaaOaxJJ-DmJzv2TgyJNU4fhnpfssSyjh6KZy0Qzjqn2akZyujwT2bnPVBM5lAe26U6qk6WG1oVVWtRkzypTOALL252DYhPXC3V_3rHUmMGyslVejdUEPsgwFLrqGyctjQa-WVPfkNGDddS9kjkefgAzxxsgxTqnnPsmKMioMPs_kkg8mi2CxxXJmktHOTgU7q41SlCySgtR12r0UGYiXh0PJiUtRZIW27s4hrBHmzcsb4Gs6PjLqU9FUDLJvrYEqmv7pmsvklNkYiQ-MsdWKO_HGR51tFzLTeNDjUiTUQvbN1zLsqF7-4FWZBlQS3J4jfAHp34Cig)

#### Component Registration Example

**1. Creating a new workflow (draft state):**
```http
POST /core/workflows/sys-flows/instances/start?sync=true
Content-Type: application/json

{
  "key": "my-custom-workflow",
  "version": "1.0.0",
  "domain": "ecommerce",
  "flow": "sys-flows",
  "flowVersion": "1.0.0",
  "attributes": {
    "type": "F",
    "labels": [
      {
        "language": "tr-TR",
        "label": "Payment Process"
      }
    ],
    "startTransition": {
      "key": "start-payment",
      "target": "pending",
      "triggerType": 0
    },
    "states": [
      {
        "key": "pending",
        "stateType": 1,
        "transitions": []
      }
    ]
  }
}
```

**2. Activating the workflow:**
```http
PATCH /core/workflows/sys-flows/instances/{instanceId}/transitions/activate?sync=true
Content-Type: application/json

{
}
```

**3. Updating the active workflow:**
```http
PATCH /core/workflows/sys-flows/instances/{instanceId}/transitions/update?sync=true
Content-Type: application/json

{
  "type": "F",
    "labels": [
      {
        "language": "tr-TR",
        "label": "Payment Process"
      }
    ],
    "startTransition": {
      "key": "start-payment",
      "target": "pending",
      "triggerType": 0
    },
    "states": [
      {
        "key": "pending",
        "stateType": 1,
        "transitions": []
      }
    ]
}
```

### Other Components

Other system components (sys-tasks, sys-views, sys-schemas, sys-extensions, sys-functions) also have the same lifecycle logic:

- **draft** → **active** → **passive** → **deleted**
- Appropriate transition endpoints are used for each transition
- Different schema validations are applied according to the component type

## Best Practices

### Instance Management
1. **Sync Parameter**: Use the `sync=true` parameter for sync operations
2. **ETag Usage**: Use the ETag pattern to prevent concurrent updates
3. **Error Handling**: Check HTTP status codes and implement appropriate error handling

### Component Management
1. **Version Strategy**: Use semantic versioning (Major.Minor.Patch)
2. **Test Environment**: Test in draft state first, then activate
3. **Rollback Plan**: Use passive state for rollback
4. **Monitoring**: Regularly check component statuses

## Error Conditions

### Common Errors
- **404 Not Found**: Instance or workflow not found
- **409 Conflict**: Concurrent update conflict (ETag mismatch)
- **400 Bad Request**: Invalid transition or data format
- **422 Unprocessable Entity**: Schema validation error

### Error Solutions
1. Check the instance ID
2. Update the ETag value
3. Compare the request payload with the schema
4. Check transition rules
