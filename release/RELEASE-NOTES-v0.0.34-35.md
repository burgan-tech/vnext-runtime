# vNext Runtime Platform - Release Notes v0.0.34-35
**Release Date:** February 3, 2026

## Overview
This combined release document covers **v0.0.34** and **v0.0.35**. It includes cache invalidation synchronization improvements across pods via a broadcast pub/sub pattern, deterministic handling for non-blocking task failures across the transition pipeline, and a new `sync` configuration flag for `SubProcessTask`.

---

## v0.0.34

## Bug Fixes

### 1. Prevent Out-of-Flow State Transition Triggering
Fixed a bug where a state transition could be triggered independently from any point, allowing the flow state to be moved to an unintended stage.

> **Reference:** [#342 - The state transition can be triggered independently from any point, allowing the flow state to be moved to a different stage](https://github.com/burgan-tech/vnext/issues/342)

---

## v0.0.35

## Hotfixes

### 1. Cache Invalidation Broadcast (Cross-Pod Synchronization)
Implemented a broadcast-based cache invalidation approach so cache refresh events can be received by all pods (instead of only the pod that received the request).

**Runtime configuration (Dapr):**
- `vnext-pubsub-broadcast` component is available on orchestration/execution/workers.
- Orchestration subscribes to the invalidation topic and routes it to the utility endpoint.

**Dapr component:**
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: vnext-pubsub-broadcast
spec:
  type: pubsub.redis
  metadata:
  - name: redisHost
    value: vnext-redis:6379
```

**Dapr subscription (orchestration):**
```yaml
apiVersion: dapr.io/v1alpha1
kind: Subscription
metadata:
  name: vnext-invalidate-cache-subscription
spec:
  topic: development.vnext.invalidate-cache
  route: /api/v1/utilities/invalidate
  pubsubname: vnext-pubsub-broadcast
```

> **Reference:** [#356 - Cache Invalidation Broadcast System Implementation](https://github.com/burgan-tech/vnext/issues/356)

---

## Enhancements

### 1. Deterministic Handling for Non-Blocking Task Failures (Transition Pipeline)
Enhanced the transition pipeline to consistently capture business-level, non-blocking task failures (i.e., failures not handled by ErrorBoundary) and ensure they are either:
- routed/handled via AutoTransitions, or
- faulted with a clear consolidated error when unhandled (e.g., epilogue skipped, no auto transitions available, or no winner).

> **Reference:** [#369 - Handle non-blocking task failures across transition pipeline + add SubProcess sync flag](https://github.com/burgan-tech/vnext/issues/369)

---

### 2. `SubProcessTask` `sync` Configuration (v0.0.35+)
`SubProcessTask` now supports a **`sync`** flag in its config. This value is propagated end-to-end and sent explicitly as the `sync=true|false` query parameter in the subprocess start request.

**Example:**
```json
{
  "type": "14",
  "config": {
    "domain": "audit",
    "key": "transaction-audit",
    "version": "1.0.0",
    "sync": false
  }
}
```

**Documentation updated:**
- `doc/en/flow/tasks/trigger-task.md`
- `doc/tr/flow/tasks/trigger-task.md`

> **Reference:** [#369 - Handle non-blocking task failures across transition pipeline + add SubProcess sync flag](https://github.com/burgan-tech/vnext/issues/369)

---

## Configuration Updates

Configuration for v0.0.35:
```json
{
  "runtimeVersion": "0.0.35",
  "schemaVersion": "0.0.34"
}
```

> **Note:** Schema version remains unchanged from v0.0.34 (v0.0.35 runtime is backward compatible with schema v0.0.34).

---

## Issues Referenced

- [#342 - The state transition can be triggered independently from any point, allowing the flow state to be moved to a different stage](https://github.com/burgan-tech/vnext/issues/342)
- [#356 - Cache Invalidation Broadcast System Implementation](https://github.com/burgan-tech/vnext/issues/356)
- [#369 - Handle non-blocking task failures across transition pipeline + add SubProcess sync flag](https://github.com/burgan-tech/vnext/issues/369)

---

## Summary

With this release:
- Fixed unintended out-of-flow state transition triggering
- Added a broadcast pub/sub pattern for cache invalidation across pods
- Improved deterministic propagation/handling of non-blocking task failures in transition pipeline
- Added `SubProcessTask` `sync` flag and updated documentation accordingly

---

**vNext Runtime Platform Team**  
February 3, 2026

