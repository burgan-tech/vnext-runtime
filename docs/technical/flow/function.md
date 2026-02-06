# Function APIs

Function APIs provide system-level operations for workflow instances. These built-in functions enable clients to query instance state, retrieve data, and obtain view information without directly accessing the workflow engine internals.

## Table of Contents

1. [Overview](#overview)
2. [State Function](#state-function)
3. [Data Function](#data-function)
4. [View Function](#view-function)
5. [Best Practices](#best-practices)
6. [Related Documentation](#related-documentation)

## Overview

The vNext Runtime platform provides three core function APIs that are automatically available for every workflow instance:

| Function | Purpose | Endpoint Pattern |
|----------|---------|------------------|
| **State** | Long-polling for instance state | `GET /{domain}/workflows/{workflow}/instances/{instance}/functions/state` |
| **Data** | Retrieve instance data | `GET /{domain}/workflows/{workflow}/instances/{instance}/functions/data` |
| **View** | Get view content | `GET /{domain}/workflows/{workflow}/instances/{instance}/functions/view` |

> **Note:** For Schema Function and custom user-defined functions, see [Custom Functions](./custom-function.md).

These functions enable:
- Real-time state monitoring (long-polling)
- Efficient data retrieval with ETag support
- Dynamic view rendering with platform-specific content

## State Function

The State function provides real-time status information about a workflow instance. It is designed for long-polling scenarios where clients need to monitor instance state changes.

### Endpoint

```http
GET /{domain}/workflows/{workflow}/instances/{instance}/functions/state
```

### Parameters

| Parameter | Location | Type | Required | Description |
|-----------|----------|------|----------|-------------|
| `domain` | Path | string | Yes | Domain name |
| `workflow` | Path | string | Yes | Workflow key |
| `instance` | Path | string | Yes | Instance ID |

### Response

```json
{
  "data": {
    "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/data"
  },
  "view": {
    "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/view",
    "loadData": true
  },
  "state": "active",
  "status": "A",
  "activeCorrelations": [
    {
      "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/data",
      "correlationId": "corr-123",
      "parentState": "parent-state",
      "subFlowInstanceId": "sub-instance-456",
      "subFlowType": "SubFlow",
      "subFlowDomain": "core",
      "subFlowName": "approval-subflow",
      "subFlowVersion": "1.0.0",
      "isCompleted": false,
      "status": "Running",
      "currentState": "pending-approval"
    }
  ],
  "transitions": [
    {
      "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/transitions/approve",
      "name": "approve",
      "schema": {
        "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/schema?transitionKey=approve"
      }
    },
    {
      "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/transitions/reject",
      "name": "reject",
      "schema": {
        "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/schema?transitionKey=reject"
      }
    }
  ],
  "eTag": "W/\"abc123def456\""
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `data` | `object` | Link to retrieve instance data |
| `data.href` | `string` | Data function endpoint URL |
| `view` | `object` | View information for current state |
| `view.href` | `string` | View function endpoint URL |
| `view.loadData` | `boolean` | Whether view requires instance data |
| `state` | `string` | Current state of the instance |
| `status` | `string` | Instance status code (A=Active, C=Completed, etc.) |
| `activeCorrelations` | `array` | Active sub-flows and correlations |
| `transitions` | `array` | Available transitions from current state |
| `transitions[].schema` | `object` | Schema link for the transition (if defined) |
| `transitions[].schema.href` | `string` | Schema function endpoint URL with transitionKey |
| `eTag` | `string` | ETag for cache validation |

### Active Correlations

When a workflow has active sub-flows or correlations, they are included in the response:

| Field | Description |
|-------|-------------|
| `correlationId` | Unique correlation identifier |
| `parentState` | Parent workflow state |
| `subFlowInstanceId` | Sub-flow instance ID |
| `subFlowType` | Type of sub-flow (SubFlow, SubProcess) |
| `subFlowDomain` | Domain of the sub-flow |
| `subFlowName` | Name of the sub-flow workflow |
| `subFlowVersion` | Version of the sub-flow |
| `isCompleted` | Whether the sub-flow has completed |
| `status` | Current status of the sub-flow |
| `currentState` | Current state of the sub-flow |

### Use Cases

1. **Long-Polling**: Clients can poll this endpoint to detect state changes
2. **State Monitoring**: Dashboard applications monitoring workflow progress
3. **Transition Discovery**: Dynamically discovering available user actions
4. **Sub-Flow Tracking**: Monitoring progress of parallel sub-workflows

### Example Request

```http
GET /core/workflows/oauth-authentication-workflow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/state HTTP/1.1
Host: api.example.com
Accept: application/json
```

## Data Function

The Data function retrieves the current data of a workflow instance. It supports ETag-based caching for efficient data synchronization.

> **Filtering:** For advanced filtering capabilities including GraphQL-style queries, logical operators, and aggregations, see [Instance Filtering Guide](./instance-filtering.md).

### Endpoint

```http
GET /{domain}/workflows/{workflow}/instances/{instance}/functions/data
```

### Parameters

| Parameter | Location | Type | Required | Description |
|-----------|----------|------|----------|-------------|
| `domain` | Path | string | Yes | Domain name |
| `workflow` | Path | string | Yes | Workflow key |
| `instance` | Path | string | Yes | Instance ID |

### Headers

| Header | Type | Required | Description |
|--------|------|----------|-------------|
| `If-None-Match` | string | No | ETag value for conditional request |

### Response (200 OK)

```json
{
  "data": {
    "userId": "user123",
    "amount": 1000,
    "currency": "USD",
    "authentication": {
      "success": true,
      "method": "otp",
      "timestamp": "2025-11-11T10:30:00Z"
    },
    "approval": {
      "status": "pending",
      "requestedAt": "2025-11-11T10:35:00Z"
    }
  },
  "eTag": "W/\"xyz789abc123\"",
  "extensions": {
    "userProfile": {
      "name": "John Doe",
      "email": "john.doe@example.com"
    },
    "accountLimits": {
      "dailyLimit": 5000,
      "remainingLimit": 4000
    }
  }
}
```

### Response (304 Not Modified)

If the `If-None-Match` header matches the current ETag, the server returns:

```http
HTTP/1.1 304 Not Modified
ETag: W/"xyz789abc123"
```

No body is returned, saving bandwidth and processing time.

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `data` | `object` | Current instance data (camelCase properties) |
| `eTag` | `string` | ETag for cache validation |
| `extensions` | `object` | Additional data from registered extensions |

### ETag Support

The Data function implements ETag pattern for efficient caching:

**First Request:**
```http
GET /core/workflows/payment-flow/instances/123/functions/data
```

**Response:**
```http
HTTP/1.1 200 OK
ETag: "W/\"v1\""
Content-Type: application/json

{
  "data": { ... },
  "eTag": "W/\"v1\""
}
```

**Subsequent Request:**
```http
GET /core/workflows/payment-flow/instances/123/functions/data
If-None-Match: "W/\"v1\""
```

**Response (Data Unchanged):**
```http
HTTP/1.1 304 Not Modified
ETag: "W/\"v1\""
```

**Response (Data Changed):**
```http
HTTP/1.1 200 OK
ETag: "W/\"v2\""
Content-Type: application/json

{
  "data": { ...updated data... },
  "eTag": "W/\"v2\""
}
```

### Extensions

Extensions provide additional context data that enriches the instance data:

- Extensions are defined in the view reference configuration
- Each extension fetches data from external sources
- Extension data is merged into the `extensions` object
- Extensions are useful for:
  - User profile information
  - Reference data lookups
  - Real-time calculated values
  - External system data

### Filtering Instance Data

The Data function supports powerful filtering capabilities to query instance data based on attributes. This allows you to filter and retrieve specific instances that match your criteria.

#### Filter Syntax

Filters use the format: `filter=attributes={field}={operator}:{value}`

#### Available Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `eq` | Equal to | `filter=attributes=clientId=eq:122` |
| `ne` | Not equal to | `filter=attributes=status=ne:inactive` |
| `gt` | Greater than | `filter=attributes=amount=gt:100` |
| `ge` | Greater than or equal | `filter=attributes=score=ge:80` |
| `lt` | Less than | `filter=attributes=count=lt:10` |
| `le` | Less than or equal | `filter=attributes=age=le:65` |
| `between` | Between two values | `filter=attributes=amount=between:50,200` |
| `like` | Contains substring | `filter=attributes=name=like:john` |
| `startswith` | Starts with | `filter=attributes=email=startswith:test` |
| `endswith` | Ends with | `filter=attributes=email=endswith:.com` |
| `in` | Value in list | `filter=attributes=status=in:active,pending` |
| `nin` | Value not in list | `filter=attributes=type=nin:test,debug` |

#### Filter Examples

**Single Filter:**
```http
GET /core/workflows/payment-flow/instances/abc-123/functions/data?filter=attributes=amount=gt:1000 HTTP/1.1
Host: api.example.com
Accept: application/json
```

**Multiple Filters (AND logic):**
```http
GET /core/workflows/order-processing/instances/123/functions/data?filter=attributes=status=eq:active&filter=attributes=priority=eq:high HTTP/1.1
Host: api.example.com
Accept: application/json
```

**Range Filter:**
```http
GET /core/workflows/payment-flow/instances/abc-123/functions/data?filter=attributes=amount=between:100,500 HTTP/1.1
Host: api.example.com
Accept: application/json
```

**String Operations:**
```http
GET /core/workflows/customer/instances/123/functions/data?filter=attributes=email=endswith:@company.com HTTP/1.1
Host: api.example.com
Accept: application/json
```

> **Note:** Filtering works on instance attributes and is particularly useful when combined with pagination for large datasets. For more details on instance filtering, see the [Instance Filtering](../../../README.md#instance-filtering) section.

### Use Cases

1. **Data Synchronization**: Keep client-side data in sync with server
2. **Efficient Polling**: Use ETag to avoid unnecessary data transfers
3. **View Data Binding**: Populate views with current instance data
4. **Audit and Logging**: Retrieve complete instance state for auditing
5. **Filtered Data Retrieval**: Query specific instances based on attribute values

### Example Request

```http
GET /core/workflows/payment-flow/instances/abc-123/functions/data HTTP/1.1
Host: api.example.com
Accept: application/json
If-None-Match: "W/\"previous-etag\""
```

## View Function

The View function retrieves the appropriate view definition for the current workflow state. It supports platform-specific content and transition-specific views.

### Endpoint

```http
GET /{domain}/workflows/{workflow}/instances/{instance}/functions/view
```

### Parameters

| Parameter | Location | Type | Required | Description |
|-----------|----------|------|----------|-------------|
| `domain` | Path | string | Yes | Domain name |
| `workflow` | Path | string | Yes | Workflow key |
| `instance` | Path | string | Yes | Instance ID |
| `transitionKey` | Query | string | No | Specific transition to get view for |
| `platform` | Query | string | No | Target platform (mobile, web, tablet, etc.) |

### Response

```json
{
  "key": "account-type-selection-view",
  "content": "{\"type\":\"form\",\"fields\":[...]}",
  "type": "json",
  "display": "full-page",
  "label": "Select Account Type"
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `key` | `string` | View key identifier |
| `content` | `string` | View content (format depends on type) |
| `type` | `string` | Content type (json, html, etc.) |
| `display` | `string` | Display mode (full-page, popup, etc.) |
| `label` | `string` | Localized label for the view |

### Query Parameters

#### transitionKey

Specifies which transition's view to retrieve:

- **Provided**: Returns the view defined for that specific transition (if exists)
- **Not provided**: Returns the state view

**Example:**
```http
GET /core/workflows/account-opening/instances/123/functions/view?transitionKey=confirm-creation
```

This returns the confirmation view for the "confirm-creation" transition.

#### platform

Specifies the target platform for view content. Supported values: `web`, `ios`, `android`

The system automatically handles platform-specific content selection:
- If a platform override exists for the requested platform → returns the override content
- If no override exists → returns the original view content
- Client doesn't need to implement platform selection logic

**Example:**
```http
GET /core/workflows/account-opening/instances/123/functions/view?platform=ios
```

The system automatically determines whether to return iOS-specific content or the default content based on the view definition.

### View Selection Logic

The function follows this logic to determine which view to return:

```
1. Is transitionKey provided?
   ├─ Yes: Check if transition has a view defined
   │   ├─ Yes: Use transition view
   │   └─ No: Use state view (or return empty if no state view)
   └─ No: Use state view

2. Is platform provided?
   ├─ Yes: Check if view has platform override for this platform
   │   ├─ Yes: Use override content and display settings
   │   └─ No: Use original content and display settings
   └─ No: Use original content and display settings

3. Apply language selection based on Accept-Language header
   └─ Return appropriate label from view's labels array
```

### Use Cases

1. **State View Rendering**: Get the view for the current workflow state (system handles platform selection)
2. **Transition Confirmation**: Client queries with `transitionKey` before transition submission to check if a view exists
3. **Platform-Specific UI**: System automatically serves optimized views for web, iOS, or Android
4. **Multi-Language Support**: Display views in user's preferred language
5. **Wizard Flows**: Retrieve step-by-step input forms

**Important**: The system handles all platform and transition-based view selection logic. The client only needs to:
- Provide the `transitionKey` parameter when checking for transition views before submission
- Optionally provide the `platform` parameter (web/ios/android) for platform-specific content
- The system automatically determines which view content to return

### Example Requests

**Get State View:**
```http
GET /core/workflows/account-opening/instances/123/functions/view HTTP/1.1
Host: api.example.com
Accept: application/json
Accept-Language: en-US
```

**Get Transition View:**
```http
GET /core/workflows/account-opening/instances/123/functions/view?transitionKey=final-confirmation HTTP/1.1
Host: api.example.com
Accept: application/json
```

**Get Mobile-Specific View:**
```http
GET /core/workflows/account-opening/instances/123/functions/view?platform=mobile HTTP/1.1
Host: api.example.com
Accept: application/json
Accept-Language: tr-TR
```

**Get Mobile Transition View:**
```http
GET /core/workflows/account-opening/instances/123/functions/view?transitionKey=submit&platform=mobile HTTP/1.1
Host: api.example.com
Accept: application/json
```

## Best Practices

### 1. Use Long-Polling Efficiently

- Implement exponential backoff for failed requests
- Set reasonable polling intervals (3-10 seconds)
- Stop polling when workflow reaches final state
- Use ETag to avoid unnecessary data transfers

### 2. Leverage ETag Caching

- Always send `If-None-Match` header with stored ETag
- Handle 304 responses gracefully
- Update ETags on every successful 200 response
- Use ETags for optimistic locking in transitions

### 3. Use Platform Parameter Correctly

- Send platform parameter (web/ios/android) based on client type
- The system automatically handles platform-specific content selection
- No need to implement fallback logic - system handles it
- Cache platform-specific views appropriately

### 4. Optimize View Rendering

- Pre-fetch views for likely next states
- Cache view definitions locally
- Lazy-load view content when possible
- Implement view render pooling for repeated views

### 5. Monitor Performance

Track metrics for:
- Average response time per function
- Cache hit rate for ETag requests
- Polling interval efficiency
- View rendering performance

### 6. Security Considerations

- Always use HTTPS for function calls
- Include authentication tokens in headers
- Validate view content before rendering (XSS prevention)
- Implement rate limiting on client side
- Use correlation IDs for request tracking

## Related Documentation

- [Custom Functions](./custom-function.md) - User-defined functions and Schema Function
- [Instance Filtering](./instance-filtering.md) - GraphQL-style filtering guide
- [View Management](./view.md) - View definitions and display strategies
- [State Management](./state.md) - Understanding workflow states
- [Transition Management](./transition.md) - Executing transitions
- [Versioning and ETag](../principles/versioning.md) - ETag pattern details

