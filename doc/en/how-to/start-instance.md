# Instance Startup and Component Management

This documentation explains how to start an instance in the vNext Runtime system and how to register and activate system components.

## Instance Lifecycle

### 1. Starting an Instance

The `start` endpoint is always used to start a workflow. The system returns an `id` value as a response. This id value is the created instance id, and the subsequent transition process proceeds with this id.

**Endpoint:**
```
POST /:domain/workflows/:flow/instances/start?sync=true
```

**Example Request:**
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

**Example Response:**
```json
{
  "id": "18075ad5-e5b2-4437-b884-21d733339113",
  "status": "A"
}
```

### 2. Instance Transition

The transition endpoint is used to advance the started instance.

**Endpoint:**
```
PATCH /:domain/workflows/:flow/instances/:instanceId/transitions/activate?sync=true
```

**Example Request:**
```http
PATCH /ecommerce/workflows/scheduled-payments/instances/inst_abc123def456/transitions/activate?sync=true
Content-Type: application/json

{
    "approvedBy": "admin",
    "approvalDate": "2025-09-20T10:30:00Z"
}
```

### 3. Querying Instance Status

The GET endpoint is used to query the current status and data of the instance. This endpoint works with the ETag pattern.

**Endpoint:**
```
GET /:domain/workflows/:flow/instances/:instanceId
```

**Example Request:**
```http
GET /ecommerce/workflows/scheduled-payments/instances/18075ad5-e5b2-4437-b884-21d733339113
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
