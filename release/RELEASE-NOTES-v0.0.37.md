# vNext Runtime Platform - Release Notes v0.0.37
**Release Date:** February 17, 2026

## Overview
This release adds Master Schema validation for workflow definitions, CurrentTransition in ScriptContext for transition data access in mappings, a full Authorization mechanism (roles and queryRoles) with permissions and authorize endpoints, and OrderBy support for instance queries. It also includes bug fixes for status/state filtering, function execution validation, and GroupBy with filter support (with a breaking change: filter array format removed).

---

## Features

### 1. Master Schema for Workflow Definitions

Flow definitions can reference a **master schema** that validates all instance data throughout the instance lifecycle. When a master schema is defined, every write to instance data is validated against it; if validation fails, the instance is stopped.

**Flow definition example:**

```json
{
    "schema": {
        "key": "token-master",
        "domain": "morph-idm",
        "version": "1.0.0",
        "flow": "sys-schemas"
    }
}
```

**Behavior:**
- If a flow has a master schema reference, all data added to instance data during the instance lifetime is validated against that schema.
- Invalid data causes the instance to be terminated.

> **Reference:** [#110 - Master Schema for Workflow Definitions](https://github.com/burgan-tech/vnext/issues/110)

---

### 2. CurrentTransition Property on ScriptContext

Input and transition mappings can now access the **original transition request** (body and headers) via `ScriptContext.CurrentTransition`. This allows using transition data in mappings without writing it to instance data.

**Usage:**

```csharp
context.CurrentTransition.Data   // dynamic - original transition request body
context.CurrentTransition.Header // dynamic - original transition request headers (lowercase keys)
```

Use transition mapping to process data you do not want in instance data; the original transition payload remains available via `CurrentTransition` for the rest of the mapping pipeline.

> **Reference:** [#385 - Add CurrentTransition property to ScriptContext](https://github.com/burgan-tech/vnext/issues/385)

---

### 3. Authorization Mechanism (Roles and QueryRoles)

Function, flow, state, and transition definitions support **roles** and **queryRoles** for authorization. The following system function endpoints expose permissions and authorization checks.

**Function definition with roles:**

```json
{
  "name": "calculateFee",
  "roles": [
    { "role": "morph-idm.maker", "grant": "allow" },
    { "role": "morph-idm.viewer", "grant": "allow" }
  ]
}
```

**Transition roles:**

```json
{
  "key": "submit",
  "target": "pending",
  "roles": [
    { "role": "morph-idm.maker", "grant": "allow" },
    { "role": "morph-idm.initiator", "grant": "allow" }
  ]
}
```

**Flow and state queryRoles:**

```json
{
  "workflow": "payment-approval",
  "queryRoles": [
    { "role": "morph-idm.viewer", "grant": "allow" }
  ],
  "states": [
    {
      "key": "draft",
      "queryRoles": [
        { "role": "morph-idm.maker", "grant": "allow" },
        { "role": "morph-idm.viewer", "grant": "deny" }
      ]
    },
    {
      "key": "approved",
      "type": "finish",
      "queryRoles": []
    }
  ]
}
```

**Endpoints:**

| Endpoint | Description |
|----------|-------------|
| **Get Flow Permissions** | Returns roles and queryRoles defined for the flow. |
| **Get Instance Permissions** | Same as flow; when the instance is in a subflow, returns the subflow’s permissions. |
| **Flow Authorize** | Checks transition/function roles for the flow (by transition key or function key and role). |
| **Instance Authorize** | Same as flow; with `queryRoles=true`, checks flow and state queryRoles for the instance’s current state (or subflow instance). |

**Get Flow Permissions**

```http
GET /api/v1/core/workflows/account-opening/functions/permissions
```

**Example response:**

```json
{
    "workflow": "account-opening",
    "queryRoles": [
        { "role": "morph-idm.viewer", "grant": "allow" }
    ],
    "states": [
        { "key": "account-type-selection", "queryRoles": [] },
        {
            "key": "account-details-input",
            "queryRoles": [
                { "role": "morph-idm.maker", "grant": "allow" },
                { "role": "morph-idm.viewer", "grant": "deny" }
            ]
        }
    ],
    "transitions": [
        { "key": "initiate-account-opening", "target": "account-type-selection", "roles": [] },
        {
            "key": "select-demand-deposit",
            "target": "account-details-input",
            "roles": [
                { "role": "morph-idm.maker", "grant": "allow" },
                { "role": "morph-idm.initiator", "grant": "allow" }
            ]
        }
    ],
    "functions": []
}
```

**Get Instance Permissions**

```http
GET /api/v1/core/workflows/account-opening/instances/{instanceId}/functions/permissions
```

Returns the same structure as Get Flow Permissions; for instances in a subflow, returns the active subflow’s permissions.

**Flow Authorize**

Checks whether the given role is allowed for the given transition (or function) on the flow. Optional `version`; if omitted, latest is used.

```http
GET /api/v1/core/workflows/account-opening/functions/authorize?transitionKey=submit-account-details&role=morph-idm.maker&version=1.0.0
```

**Response 200:**

```json
{ "allowed": true }
```

**Response 403:**

```json
{ "allowed": false }
```

**Instance Authorize**

```http
GET /api/v1/core/workflows/account-opening/instances/{instanceId}/functions/authorize?queryRoles=true&role=morph-idm.viewer
```

Same response shape as Flow Authorize. With `queryRoles=true`, permission is evaluated against the instance’s current state (and flow/state queryRoles); for subflows, the active subflow instance is used.

**Authorize query parameters:**

| Parameter | Description |
|-----------|-------------|
| `role` | Role to check. |
| `version` | Optional. Flow version; default is latest. |
| `transitionKey` | Transition to check (transition-level roles). |
| `functionKey` | Function to check (function-level roles). |
| `queryRoles` | When true, check flow and state queryRoles for the instance’s current state (and subflow context if applicable). |

> **Reference:** [#382 - Authorization Mechanism](https://github.com/burgan-tech/vnext/issues/382)

---

### 4. OrderBy Support for Instance Queries

Instance list and data endpoints support **sorting** via `sort` or `orderBy` query parameters.

**Single field:**

```
?sort={"field":"createdAt","direction":"desc"}
?orderBy={"field":"status","direction":"asc"}
```

**Multiple fields:**

```
?sort={"fields":[{"field":"status","direction":"asc"},{"field":"createdAt","direction":"desc"}]}
```

- **direction:** `"asc"` or `"desc"` (case-insensitive). Defaults to `"asc"` if omitted.

**Sortable fields:**

| Field | Notes |
|-------|-------|
| `createdAt` | Creation timestamp |
| `modifiedAt` | Modification timestamp |
| `completedAt` | Completion timestamp |
| `status` | Instance status |
| `key` | Instance key |
| `currentState` / `state` | Current state (`state` is alias) |
| `attributes.fieldName` | JSON path into instance data; nested paths supported (e.g. `attributes.nested.path`) |

Instance columns are applied in the database; `attributes.*` ordering uses the latest instance data JSON and is subject to the same schema/security as filtering.

> **Reference:** [#381 - Order By Support Implementation](https://github.com/burgan-tech/vnext/issues/381)

---

## Bug Fixes

### 1. Status and State Filter

Filtering on `status` and `state` (currentState) now works correctly in instance queries.

> **Reference:** [#389 - Filter status property not working](https://github.com/burgan-tech/vnext/issues/389)

---

### 2. Function Execution Validation

The runtime now validates that a function is defined in the workflow’s functions collection before execution. Executing an undefined function returns an error.

> **Reference:** [#180 - Function execution should validate function is defined in workflow's functions collection](https://github.com/burgan-tech/vnext/issues/180)

---

### 3. GroupBy and Filter Together (Breaking Change)

GroupBy and filter can now be used together in the same query. As part of this fix, the filter format has been standardized and the previous array form is no longer supported.

**Breaking change:**

- **Before:** `"filter": ["expr1", "expr2"]` (array) was supported.
- **After:** Only a single expression is supported: `"filter": "expr"` (string). The GraphQL-style filter format is now the single standard.

If you currently pass `filter` as an array, update to a single expression (e.g. combine conditions with `and`/`or` in one expression).

> **Reference:** [#392 - GroupBy and Filtering queries not working together](https://github.com/burgan-tech/vnext/issues/392)

---

## Configuration Updates

Configuration for v0.0.37:

```json
{
  "runtimeVersion": "0.0.37",
  "schemaVersion": "0.0.36"
}
```

> **Note:** Schema version remains at 0.0.36; runtime v0.0.37 is backward compatible with schema v0.0.36.

---

## Issues Referenced

- [#110 - Master Schema for Workflow Definitions](https://github.com/burgan-tech/vnext/issues/110)
- [#385 - Add CurrentTransition property to ScriptContext](https://github.com/burgan-tech/vnext/issues/385)
- [#382 - Authorization Mechanism](https://github.com/burgan-tech/vnext/issues/382)
- [#381 - Order By Support Implementation](https://github.com/burgan-tech/vnext/issues/381)
- [#389 - Filter status property not working](https://github.com/burgan-tech/vnext/issues/389)
- [#180 - Function execution should validate function is defined in workflow's functions collection](https://github.com/burgan-tech/vnext/issues/180)
- [#392 - GroupBy and Filtering queries not working together](https://github.com/burgan-tech/vnext/issues/392)

---

## Summary

With this release:
- Master Schema validates all instance data for flows that reference it; invalid data stops the instance.
- ScriptContext.CurrentTransition provides access to the original transition body and headers in mappings.
- Authorization supports roles and queryRoles on functions, flows, states, and transitions, with permissions and authorize endpoints.
- Instance queries support OrderBy/sort with single or multiple fields and a defined set of sortable fields.
- Status and state filtering works correctly; function execution is validated against the workflow’s functions; GroupBy and filter work together, with filter standardized to a single expression (breaking change for filter array usage).

---

**vNext Runtime Platform Team**  
February 17, 2026
