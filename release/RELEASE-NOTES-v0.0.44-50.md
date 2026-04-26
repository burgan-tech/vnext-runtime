# vNext Runtime Platform - Release Notes v0.0.44-50
**Release Date:** April 25, 2026
**Stable Release:** v0.0.50
**Interim Versions:** v0.0.44 – v0.0.49

## Overview

This release consolidates seven interim versions (v0.0.44 through v0.0.49) into the stable **v0.0.50** release. The primary themes are **pipeline correctness**, **infrastructure resilience**, **performance optimization**, and **observability**.

On the correctness front, **workflow timeout** now executes the full **TransitionPipeline** (onExit, onEntry, automatic transitions, finish handling) instead of a direct state mutation, and the **async execution race condition** that prematurely set instances to `Active` before post-commit jobs completed has been resolved. **Extension** error handling is reclassified so that infrastructure failures abort the request while application-level errors degrade gracefully. **Schema validation** is now enforced on `sync=false` requests, preventing invalid payloads from being accepted and later faulting.

Infrastructure improvements include **UTC DateTime normalization** via an EF Core interceptor and global ValueConverter (independent of PostgreSQL server timezone), a **PgBouncer-safe schema command interceptor** replacing the connection-level interceptor, and a **timestamp with time zone migration** for `SubFlowStateChangedAt`. On the CI/CD side, **software supply chain security** (Cosign signing, provenance attestation) and a **cross-repo dispatch token fix** strengthen the delivery pipeline.

Performance gains come from **distributed cache tracing** with short-TTL local vidx caching (eliminating redundant Redis roundtrips), **broadcast cache invalidation** that reads from distributed cache instead of performing a full DB reload on non-initiating pods, and **hot-path index adjustments** that fix a `GetInstanceHistory` regression and remove a redundant partial PK index.

New capabilities include **dynamic workflow timeout mapping** via `ITimerMapping` scripts (with static duration fallback), **ErrorBoundary enrichment** with `errorMessage`, `errorState`, and `errorTask` parameters in instance data, **`acceptedStatusCodes`** on HTTP-based task types to suppress ErrorBoundary for expected non-2xx responses, and **local/remote metadata parity** for `GetInstances` and `GetInstanceData` task executors.

---

## Bug Fixes

### Timeout target state pipeline not executed (#514)

When a workflow instance timed out, `FlowTimeoutJobHandler` changed the instance state and marked it complete but **never ran the TransitionPipeline**. All `onExit` tasks, `onEntry` tasks, automatic transitions, scheduled transitions, and finish-state handling were silently skipped. The handler now invokes the full `TransitionPipeline` with a synthetic timeout transition context, ensuring consistent state lifecycle execution regardless of how a transition is triggered.

> **Reference:** [#514](https://github.com/burgan-tech/vnext/issues/514)

### Extension failures abort the entire request (#515)

On all InstanceData read endpoints, any extension task failure caused the entire HTTP response to be aborted — even when the extension's `outputMapping` had already handled the downstream error at the application level. Error classification is now split into two categories:

- **Infrastructure errors** (network timeout, DNS failure, connection refused) — fail-fast, abort request.
- **Application-level / output-mapping-handled errors** — degrade gracefully, continue with remaining extensions.

> **Reference:** [#515](https://github.com/burgan-tech/vnext/issues/515)

### UTC DateTime normalization (#520)

PostgreSQL server configured with `TimeZone = 'Europe/Istanbul'` caused silent +3h offset corruption on all `DateTime` values stored in `timestamp` (without time zone) columns. A two-layer defense-in-depth fix was implemented:

1. **Write path:** `UtcDateTimeInterceptor` (`SaveChangesInterceptor`) normalizes all `DateTime` properties to UTC before `SaveChanges`.
2. **Read path:** Global `UtcDateTimeConverter` via `ConfigureConventions` ensures all `DateTime` values read from the database are tagged as `DateTimeKind.Utc`.

Behavior is now independent of the PostgreSQL server timezone configuration.

> **Reference:** [#520](https://github.com/burgan-tech/vnext/issues/520)

### SubFlowStateChangedAt timestamp migration (#525)

The `SubFlowStateChangedAt` column in `InstancesCorrelations` was `timestamp without time zone`. A migration converts it to `timestamp with time zone` using `AT TIME ZONE 'UTC'`, aligning it with the rest of the schema.

> **Reference:** [#525](https://github.com/burgan-tech/vnext/issues/525)

### PgBouncer-safe schema interceptor (#534)

`NpgsqlSchemaConnectionInterceptor` set `search_path` at connection open time, which PgBouncer transaction-mode pooling could reassign to a different session. The new `PgBouncerSafeSchemaCommandInterceptor` prepends `SET search_path` inline to every SQL command, eliminating the extra round-trip and ensuring schema consistency under pooled connections.

> **Reference:** [#534](https://github.com/burgan-tech/vnext/issues/534)

### Async mode race condition: premature Active status (#535)

When `sync=false`, `ResolveAvailableStep` and `ClearBusyOnResumeStep` wrote `Active` status to the database mid-pipeline, before post-commit jobs (SubFlow start, remote task invocations) had completed. Clients polling during this window would see `Active` while the pipeline was still executing. The status update is now **deferred** via `PipelineDirectives.SetResolvedStatus` and applied in `TransitionPipeline.ApplyResolvedStatusAsync` only after all post-commit jobs complete.

> **Reference:** [#535](https://github.com/burgan-tech/vnext/issues/535)

### Cross-repo dispatch token scope (#541)

The `actions/create-github-app-token` step generated a token scoped only to `vnext`. `peter-evans/repository-dispatch` targeting `vnext-helm-charts` failed with HTTP 403. The token scope now includes both repositories.

> **Reference:** [#541](https://github.com/burgan-tech/vnext/issues/541)

### Schema validation skipped on sync=false (#556)

When `sync=false`, the async strategy accepted any payload without validating against the transition's JSON schema. Invalid data entered the system and faulted during background execution. Schema validation is now performed **before** accepting the request, regardless of the `sync` flag. Invalid payloads return `400 Bad Request` with field-level errors.

> **Reference:** [#556](https://github.com/burgan-tech/vnext/issues/556)

### Local execution metadata parity for GetInstances / GetInstanceData (#563)

`GetInstanceDataTaskExecutor.ExecuteLocalAsync` returned no `metadata` on the happy path, and `GetInstancesTaskExecutor` had inconsistent metadata between local and remote paths. Both executors now build and return the same `metadata` dictionary as the remote/grouped paths, ensuring downstream consumers (scripting, mapping, logging) see identical shapes regardless of execution mode.

> **Reference:** [#563](https://github.com/burgan-tech/vnext/issues/563)

---

## Features & Enhancements

### ErrorBoundary enrichment: error message, state, task parameters (#496)

ErrorBoundary now populates `errorMessage`, `errorState`, and `errorTask` fields in instance data when an error is caught. This enables downstream consumers (backoffice UIs, mapping scripts) to display meaningful error context and handle errors programmatically.

> **Reference:** [#496](https://github.com/burgan-tech/vnext/issues/496)

### Dynamic timeout mapping for subprocesses (#524)

Workflow-level timeout now supports an optional `mapping` field (a `ScriptCode` object implementing `ITimerMapping`) for dynamic timeout duration calculation at runtime. When the mapping script succeeds, its `TimerSchedule` result is used. On failure, the static `timer.duration` is used as fallback. A static `timer` configuration is **required** when `mapping` is defined (validation enforced).

**Definition example:**

```json
"timeout": {
  "key": "$timeout",
  "target": "timed-out",
  "versionStrategy": "None",
  "timer": { "reset": "false", "duration": "PT1H" },
  "mapping": {
    "location": "./src/TimeoutMapping.csx",
    "code": "<BASE64>"
  }
}
```

**Use case:** A parent workflow starts a subprocess with `timeoutMinutes: 30` in the body. The subprocess timeout mapping reads this value and schedules a 30-minute timeout. If the mapping fails, the static 1-hour fallback applies.

**Documentation:** [Transitions](../doc/en/flow/transition.md), [Workflow Definition](../doc/en/flow/flow.md)

> **Reference:** [#524](https://github.com/burgan-tech/vnext/issues/524)

### Task `acceptedStatusCodes` configuration (v0.0.50+)

HTTP-based task types now support an optional `acceptedStatusCodes` array in their `config` section. Status codes listed here are treated as successful even when they are error codes (4xx, 5xx), preventing the ErrorBoundary from being triggered. Supports exact codes (`"404"`), wildcard patterns (`"4xx"`), and partial wildcards (`"40x"`).

**Supported task types:** Dapr Service (3), HTTP (6), StartTask (11), DirectTriggerTask (12), GetInstanceDataTask (13), SubProcessTask (14), GetInstances (15).

```json
"config": {
  "method": "GET",
  "url": "http://user-service/api/users/{userId}",
  "acceptedStatusCodes": ["404"]
}
```

**Documentation:** [HTTP Task](../doc/en/flow/tasks/http-task.md), [Dapr Service Task](../doc/en/flow/tasks/dapr-service.md), [Trigger Tasks](../doc/en/flow/tasks/trigger-task.md)

### Software supply chain security (#545)

The CI/CD pipeline now produces verifiable, signed, and traceable container images:

- **Digest-based** image handling for immutable references
- **Cosign keyless signing** with GitHub OIDC for artifact authenticity
- **Build provenance attestation** via GitHub attestation actions
- Ready for downstream **admission-policy verification**

> **Reference:** [#545](https://github.com/burgan-tech/vnext/issues/545)

---

## Performance Improvements

### Broadcast cache invalidation: distributed-cache-first reload (#557)

Non-initiating pods receiving a `DefinitionCacheInvalidationEvent` broadcast no longer perform a full paginated DB read of all active instance data. Instead, they read instance metadata (keys/versions), check the **distributed cache** first (which the initiating pod has already populated), and upsert only into the local in-memory cache. A per-key DB fallback handles distributed cache misses. This eliminates the heavy, redundant DB round-trip that stalled the first pod receiving the broadcast.

> **Reference:** [#557](https://github.com/burgan-tech/vnext/issues/557)

### Distributed cache tracing and Redis vidx optimization (#568)

- **Cache tracing:** A new `CacheActivityHelper` (`ActivitySource("BBT.Workflow.Cache")`) wraps distributed cache operations (`Cache.Get`, `Cache.Set`, `Cache.Remove`, `Cache.Warmup`, `Cache.VersionIndex`) with proper parent spans so Dapr sidecar spans nest correctly in trace tools.
- **vidx short-TTL local cache:** `CacheSet` now caches Redis version-index (`vidx`) query results in a `ConcurrentDictionary` with a short TTL, eliminating 3–5+ redundant Redis roundtrips per transition hop. The local cache is invalidated on `SetAsync` and `InvalidateAsync`.

> **Reference:** [#568](https://github.com/burgan-tech/vnext/issues/568)

### Hot-path index adjustments and GetInstanceHistory fix (#561)

- **Dropped** redundant `IX_Instances_Active_Id` partial PK index (never used by the planner, only write overhead).
- **Added** non-partial `IX_Instances_Key` B-tree for `FindByIdentifierAsync` key-based lookups across all statuses.
- **Fixed** `GetInstanceHistory` regression: a new `FindByIdentifierWithFullHistoryAsync` repository method loads the full `DataList` without the `IsLatest` filter, restoring complete revision history on the history endpoint.

> **Reference:** [#561](https://github.com/burgan-tech/vnext/issues/561)

---

## Breaking Changes

None for this release.

---

## Configuration Updates

Configuration for v0.0.50:

```json
{
  "runtimeVersion": "0.0.50",
  "schemaVersion": "0.0.41"
}
```

> **Note:** Schema package **0.0.50** is the documented pairing with this runtime. The `task-definition.schema.json` and `workflow-definition.schema.json` include new fields (`acceptedStatusCodes`, `timeout.mapping`). Update domain **`validate.js`** / **Ajv2019** usage per [Schema Management](../doc/en/flow/schema.md).

---

## Issues Referenced

- [vnext #496](https://github.com/burgan-tech/vnext/issues/496) — ErrorBoundary enrichment (error message, state, task)
- [vnext #514](https://github.com/burgan-tech/vnext/issues/514) — FlowTimeoutJobHandler pipeline execution
- [vnext #515](https://github.com/burgan-tech/vnext/issues/515) — Extension error classification / graceful degradation
- [vnext #520](https://github.com/burgan-tech/vnext/issues/520) — UTC DateTime normalization
- [vnext #521](https://github.com/burgan-tech/vnext/issues/521) — Rejecting out-of-order SubFlow state (duplicate of #520/#525)
- [vnext #524](https://github.com/burgan-tech/vnext/issues/524) — Dynamic timeout mapping for subprocesses
- [vnext #525](https://github.com/burgan-tech/vnext/issues/525) — SubFlowStateChangedAt timestamptz migration
- [vnext #534](https://github.com/burgan-tech/vnext/issues/534) — PgBouncer-safe schema interceptor
- [vnext #535](https://github.com/burgan-tech/vnext/issues/535) — Async mode race condition (premature Active status)
- [vnext #541](https://github.com/burgan-tech/vnext/issues/541) — Cross-repo dispatch token scope
- [vnext #545](https://github.com/burgan-tech/vnext/issues/545) — Software supply chain security
- [vnext #556](https://github.com/burgan-tech/vnext/issues/556) — Schema validation on sync=false
- [vnext #557](https://github.com/burgan-tech/vnext/issues/557) — Broadcast cache invalidation optimization
- [vnext #561](https://github.com/burgan-tech/vnext/issues/561) — Hot-path indexes + GetInstanceHistory fix
- [vnext #563](https://github.com/burgan-tech/vnext/issues/563) — Local execution metadata parity
- [vnext #568](https://github.com/burgan-tech/vnext/issues/568) — Distributed cache tracing + vidx optimization

---

## Summary

- **Timeout pipeline** now executes full `TransitionPipeline` (onExit, onEntry, automatic transitions, finish handling) instead of direct state mutation.
- **Extension** error handling reclassified: infrastructure errors fail-fast, application-level errors degrade gracefully.
- **UTC DateTime** normalization via EF Core interceptor + global ValueConverter, independent of server timezone.
- **PgBouncer-safe** schema command interceptor replaces connection-level interceptor.
- **Async race condition** fixed: `Active` status deferred until all post-commit jobs complete.
- **Schema validation** enforced on `sync=false` requests; invalid payloads rejected with `400`.
- **Dynamic timeout mapping** via `ITimerMapping` scripts with static duration fallback.
- **ErrorBoundary** enrichment with `errorMessage`, `errorState`, `errorTask` in instance data.
- **`acceptedStatusCodes`** on HTTP-based tasks to suppress ErrorBoundary for expected non-2xx responses.
- **Supply chain security**: Cosign signing, provenance attestation, digest-based image handling.
- **Cache performance**: distributed-cache-first broadcast reload, vidx short-TTL local cache, cache operation tracing.
- **Index optimization**: redundant partial PK dropped, Key index added, GetInstanceHistory regression fixed.
- **Metadata parity** between local and remote execution paths for GetInstances/GetInstanceData task executors.

---

**vNext Runtime Platform Team**
April 25, 2026
