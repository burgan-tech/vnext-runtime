# vNext Runtime Platform - Release Notes v0.0.39
**Release Date:** March 9, 2026

## Overview
This release introduces **transition filtering by role grants** in the State function (static roles plus **$InstanceStarter** / **$PreviousUser**), **field-level visibility** on the Flow Master schema via roleGrant (x-roles) for Data function and data-returning endpoints, **remote view** support (cross-domain view references), **instance metadata** in GetInstance/GetInstances responses, **parent flow shared-transition** execution when the instance is in a subflow (with **$self** target rule), and **ErrorBoundary** retry resolved by rule-based matching. It also adds **flow version on instance** (with automatic migration), **View function content** typed by view type (JSON vs string), **hasView** and **hasSchema** in the State function response to avoid unnecessary requests, and **type-dependent content** in the view definition schema.

---

## Features

### 1. Filter State-Function Transitions by Transition Role Grants

The State function now returns **available transitions** filtered by the roles defined in the transition’s authorize configuration. Only transitions for which the caller has an allowed role (static roles or **$InstanceStarter** / **$PreviousUser**) are included in the response.

**Behavior:**
- Transitions without role restrictions remain available to all callers.
- Transitions with `roles` (roleGrant) are returned only when the caller holds an allowed role.
- Enables clients to show only transitions the user is authorized to execute.

> **Reference:** [#397 - Filter state-function transitions by transition role grants](https://github.com/burgan-tech/vnext/issues/397)

---

### 2. Master Schema Field-Level Visibility (roleGrant / x-roles)

The Flow Master schema supports **field-level visibility** via a **roleGrant** (roles) property on schema fields. Data functions and data-returning endpoints (Get Instance, GetInstances, etc.) run the authorize layer and expose only fields the caller is allowed to see.

**Example (Master schema properties with roles):**
```json
{
  "properties": {
    "amount": {
      "type": "number",
      "roles": [
        { "role": "morph-idm.maker", "grant": "allow" },
        { "role": "morph-idm.approver", "grant": "allow" },
        { "role": "morph-idm.viewer", "grant": "allow" }
      ]
    },
    "internalNotes": {
      "type": "string",
      "roles": [
        { "role": "morph-idm.approver", "grant": "allow" },
        { "role": "morph-idm.manager", "grant": "allow" }
      ]
    },
    "publicStatus": {
      "type": "string"
    }
  }
}
```

Properties without `roles` are visible to all authorized callers. For third-party and tooling compatibility, a **roles vocabulary** is published:

- [roles-vocab.json](https://unpkg.com/@burgan-tech/vnext-schema@0.0.37/vocabularies/roles-vocab.json)

> **Reference:** [#402 - Implement master schema field-level visibility with x-roles for function responses](https://github.com/burgan-tech/vnext/issues/402)

---

### 3. Predefined System Roles ($InstanceStarter, $PreviousUser)

Two **static system roles** are available in the roleGrant schema for instance authorization:

| Role | Description |
|------|-------------|
| **$InstanceStarter** | The actor who started the instance |
| **$PreviousUser** | The actor who triggered the previous transition |

**Example:**
```json
{
  "roles": [
    { "role": "$InstanceStarter", "grant": "allow" },
    { "role": "$PreviousUser", "grant": "allow" }
  ]
}
```

They can be used in transition roles, state/flow queryRoles, and Master schema field-level visibility.

> **Reference:** [#396 - Implement predefined system roles for instance authorization](https://github.com/burgan-tech/vnext/issues/396)

---

### 4. Support Remote View in View Function

Views can now be **referenced and used across domains**. Cross-domain view references allow shared views to be reused, versioned, and distributed in a single place instead of duplicating them per domain.

**Behavior:**
- View function can resolve and return view content from another domain.
- Enables central view libraries and consistent UI across domains.

> **Reference:** [#431 - Support remote view in View function](https://github.com/burgan-tech/vnext/issues/431)

---

### 5. Instance Metadata in GetInstance / GetInstances Response

GetInstance and GetInstances responses now include **instance metadata and audit information** in addition to the instance data. Clients receive a full instance envelope: identity, key, flow, domain, flow version, ETag, tags, metadata (state, status, timestamps, createdBy, modifiedBy, etc.), attributes, and extensions.

**Example response shape (v0.0.39+):**
```json
{
  "id": "f226a728-9fa7-49ee-ab4e-3e8b84184df6",
  "key": "449874961045",
  "flow": "account-opening",
  "domain": "core",
  "flowVersion": "1.0.0",
  "etag": "\"01KJZXDY3N8ZKYB4RDHDWWWS7P\"",
  "tags": ["test", "banking", "account-openning", "key-test"],
  "metadata": {
    "currentState": "account-details-input",
    "effectiveState": "account-details-input",
    "status": "A",
    "effectiveStateType": "intermediate",
    "effectiveStateSubType": "none",
    "createdAt": "2026-03-05T21:10:56.857956Z",
    "modifiedAt": "2026-03-05T21:10:58.697809Z",
    "createdBy": "46467491018",
    "createdByBehalfOf": "34987491018",
    "modifiedBy": "46467491018",
    "modifiedByBehalfOf": "34987491018"
  },
  "attributes": { "initial": { "session": "16" }, "accountType": "demand-deposit", "userSession": {} },
  "extensions": {}
}
```

> **Reference:** [#432 - Include instance metadata in GetInstances list response](https://github.com/burgan-tech/vnext/issues/432)

---

### 6. Trigger Parent Flow Transition from Subflow (Shared-Transition)

When an instance is **in a subflow**, the **parent flow’s** shared transition can now be executed. Previously, only the subflow’s shared transition was available.

**Important rule:** If a shared transition is available while the instance is in a subflow, its **target** must be **$self** (so the transition applies to the current—parent—context when invoked from the subflow).

> **Reference:** [#425 - Trigger parent flow transition from subflow (shared-transition policy)](https://github.com/burgan-tech/vnext/issues/425)

---

## Bug Fixes

### ErrorBoundary: Resolve Retry Policy by Matching Rule (Error-Aware Retry)

ErrorBoundary execution was updated so that **retry is resolved by matching the error to the correct rule**. Infrastructure-level errors were previously incorrectly included in the boundary; this is fixed and retry behavior is now rule-based (error-aware).

> **Reference:** [#421 - ErrorBoundary: Resolve retry policy by matching rule (error-aware retry)](https://github.com/burgan-tech/vnext/issues/421)

---

## Tasks / Improvements

### Flow Version on Instance

The **flow version** with which an instance was started is now **stored on the instance** and used for transitions. This ensures the instance continues on the same flow version it was started with. Runtime runs an **automatic migration** per flow schema; no manual migration step is required.

> **Reference:** [#428 - Hotfix: Store flow version on instance and use it for transitions](https://github.com/burgan-tech/vnext/issues/428)

---

### View Function Content Typed by View Type

The View function **content** is now returned with a type that matches the view type:

- **type === "Json"** → `content` is an **object or array** (JSON).
- **type === "Html"** (and similar) → `content` is a **string** (in JSON).

Clients can parse and use the response without re-parsing JSON strings for Json views.

> **Reference:** [#429 - View function should return content typed by view type (JSON vs string)](https://github.com/burgan-tech/vnext/issues/429)

---

### hasView and hasSchema in State Function Response

The State function response now includes **hasView** and **hasSchema** where applicable (e.g. on the root `view` object and on each transition’s `view` and `schema`). Clients can avoid unnecessary view or schema requests and reduce 404s by checking these flags before calling the view or schema endpoints.

**Example (excerpt):**
```json
{
  "view": { "hasView": true, "loadData": true, "href": "..." },
  "transitions": [
    {
      "name": "select-demand-deposit",
      "view": { "hasView": false, "loadData": true, "href": "..." },
      "schema": { "hasSchema": true, "href": "..." }
    }
  ]
}
```

> **Reference:** [#430 - Add hasView to state function response to avoid unnecessary view requests](https://github.com/burgan-tech/vnext/issues/430)

---

### View Definition: Type-Dependent Content Field (Schema)

The view definition schema now allows **content** to be **string or JSON** depending on view type (e.g. Json → object/array, Html → string). This improves conflict handling in version control and design tools while remaining backward compatible.

> **Reference:** [vnext-schema #85 - View definition: type-dependent content field (string | JSON) with backward compatibility](https://github.com/burgan-tech/vnext-schema/issues/85)

---

## Configuration Updates

Configuration for v0.0.39:

```json
{
  "runtimeVersion": "0.0.39",
  "schemaVersion": "0.0.37"
}
```

> **Note:** This version includes a **database migration**. The runtime runs it automatically for each flow schema. Schema version is **0.0.37**; ensure your configuration and tooling use the updated schema.

---

## Issues Referenced

- [#396 - Implement predefined system roles for instance authorization ($InstanceStarter, $PreviousUser)](https://github.com/burgan-tech/vnext/issues/396)
- [#397 - Filter state-function transitions by transition role grants](https://github.com/burgan-tech/vnext/issues/397)
- [#402 - Implement master schema field-level visibility with x-roles for function responses](https://github.com/burgan-tech/vnext/issues/402)
- [#421 - ErrorBoundary: Resolve retry policy by matching rule (error-aware retry)](https://github.com/burgan-tech/vnext/issues/421)
- [#425 - Trigger parent flow transition from subflow (shared-transition policy)](https://github.com/burgan-tech/vnext/issues/425)
- [#428 - Hotfix: Store flow version on instance and use it for transitions](https://github.com/burgan-tech/vnext/issues/428)
- [#429 - View function should return content typed by view type (JSON vs string)](https://github.com/burgan-tech/vnext/issues/429)
- [#430 - Add hasView to state function response to avoid unnecessary view requests](https://github.com/burgan-tech/vnext/issues/430)
- [#431 - Support remote view in View function](https://github.com/burgan-tech/vnext/issues/431)
- [#432 - Include instance metadata in GetInstances list response](https://github.com/burgan-tech/vnext/issues/432)
- [vnext-schema #85 - View definition: type-dependent content field](https://github.com/burgan-tech/vnext-schema/issues/85)

---

## Summary

With this release:

- **State function** returns only transitions allowed by role grants (including $InstanceStarter and $PreviousUser) and exposes **hasView** / **hasSchema** to avoid unnecessary requests.
- **Master schema** supports **field-level visibility** via roleGrant (x-roles); Data function and data endpoints return only authorized fields.
- **System roles** **$InstanceStarter** and **$PreviousUser** are available for authorization and visibility.
- **View function** supports **remote (cross-domain) views** and returns **content** typed by view type (JSON vs string).
- **GetInstance / GetInstances** include **instance metadata** and audit fields in the response.
- **Shared transitions** can run on the **parent flow** when the instance is in a subflow; **target** must be **$self** in that case.
- **ErrorBoundary** retry is **rule-based** (error-aware); infrastructure errors are no longer incorrectly handled by the boundary.
- **Flow version** is stored on the instance and used for transitions; runtime performs automatic migration.
- **View definition** schema allows type-dependent **content** (string | JSON) with backward compatibility.

---

**vNext Runtime Platform Team**  
March 9, 2026
