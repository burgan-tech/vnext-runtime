# vNext Runtime Platform - Release Notes v0.0.40
**Release Date:** March 10, 2026

## Overview
This release improves **schema consistency** by making **flowVersion** required in definition schemas (vnext-schema) and **config** required in task definitions, and introduces an **authorization-aware ETag strategy** for data endpoints: response body exposes **etag** (request/response change) and **entityETag** (entity data change), with corresponding **ETag** and **X-Entity-ETag** response headers. It also fixes View function handling of deep JSON **content** (max depth increased to 256, `ReferenceHandler.IgnoreCycles`) and removes an unnecessary retry block in the cache invalidation handler.

---

## Bug Fixes

### 1. Make flowVersion a Required Field in Definition Schemas

**flowVersion** is now **required** in definition schemas in vnext-schema. This aligns the schema with runtime behavior (flowVersion was already required in v0.0.39) and ensures consistency across tooling and validation.

> **Reference:** [#88 - Make flowVersion a required field in definition schemas](https://github.com/burgan-tech/vnext-schema/issues/88)

---

### 2. Make config a Required Field in Task Definitions

Task definitions now **require** a **config** section. The task-definition schema (task-definition.schema.json) has been updated so that `config` is mandatory, ensuring all task definitions carry explicit configuration and improving consistency.

> **Reference:** [#89 - Make config a required field in task-definition.schema.json](https://github.com/burgan-tech/vnext-schema/issues/89)

---

## Tasks / Improvements

### Redesign ETag Strategy for Authorization-Aware Responses

After adding the authorization layer to data endpoints, response content could change per caller, and the previous ETag logic became inconsistent. The ETag strategy has been **redesigned** so that:

- **etag** in the response body reflects **request/response** change (e.g. authorization or response shape).
- **entityETag** in the response body reflects **entity data** change.

Clients can use **entityETag** to track when the underlying data has changed and **etag** for general response caching. The same values are exposed in response headers:

- **ETag** — matches the `etag` value (without surrounding quotes in the header).
- **X-Entity-ETag** — matches the `entityETag` value.

**Example response body:**
```json
{
  "etag": "\"Hko5JI4fDcAOOnf-KGFNA7Xo_MpuxcLl1_hg5j2Sua8\"",
  "entityEtag": "\"01KK8Q8N5T6H49T8AENYT6Z6ZQ\""
}
```

**Example response headers:**
```http
ETag: "Hko5JI4fDcAOOnf-KGFNA7Xo_MpuxcLl1_hg5j2Sua8"
X-Entity-ETag: "01KK8Q8N5T6H49T8AENYT6Z6ZQ"
```

> **Reference:** [#448 - Redesign ETag Strategy for Authorization-Aware Responses](https://github.com/burgan-tech/vnext/issues/448)

---

## Other

### View Function: Deep JSON Content and ReferenceHandler

An error occurred when the **content** field of a view contained JSON deeper than the default maximum depth (32). The maximum depth has been increased to **256**, and **ReferenceHandler** has been set to **IgnoreCycles** so that deeply nested or cyclic structures in view content are handled correctly.

---

### Cache Invalidation Handler: Retry Behavior

An unnecessary retry block in the cache invalidation handler was removed, so retries are no longer incorrectly prevented when appropriate.

---

## Configuration Updates

Configuration for v0.0.40:

```json
{
  "runtimeVersion": "0.0.40",
  "schemaVersion": "0.0.38"
}
```

> **Note:** Schema version **0.0.38** is used with this runtime version. Ensure your configuration and tooling reference the updated schema.

---

## Issues Referenced

- [#88 - Make flowVersion a required field in definition schemas](https://github.com/burgan-tech/vnext-schema/issues/88)
- [#89 - Make config a required field in task-definition.schema.json](https://github.com/burgan-tech/vnext-schema/issues/89)
- [#448 - Redesign ETag Strategy for Authorization-Aware Responses](https://github.com/burgan-tech/vnext/issues/448)

---

## Summary

With this release:

- **Definition schemas** (vnext-schema) require **flowVersion**; **task definitions** require **config**.
- **Data endpoints** use an **authorization-aware ETag** strategy: **etag** and **entityETag** in the response body, with **ETag** and **X-Entity-ETag** response headers for consistent caching and change detection.
- **View function** supports deeper JSON **content** (max depth 256) with **ReferenceHandler.IgnoreCycles**.
- **Cache invalidation handler** retry behavior is corrected.

---

**vNext Runtime Platform Team**  
March 10, 2026
