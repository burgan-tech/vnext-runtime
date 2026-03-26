# vNext Runtime Platform - Release Notes v0.0.42
**Release Date:** March 25, 2026

## Overview

This release shifts **flow database schema** creation and migration to **deploy time** via a **DB-Migrator job** instead of running migration checks on every start and transition request. It improves **observability** with consistent structured logging and **TaskCoordinator** span tracing (Aether). For **synchronous** start and transition calls (`sync=true`), responses now include **mapped instance data** (attributes, ETags, extensions) in addition to `id` and `status`. **User-defined functions** support **multi-task execution** with ordered `onExecutionTasks` and optional **output mapping** via `IOutputHandler`. **SubFlow** definitions can use an **`overrides`** object to replace transition roles, state query roles, timeouts, and related behavior while keeping legacy **view override** compatibility. **ScriptBase** adds **collection and dynamic-object helpers** (`CreateObject`, `GetList`, `ListFilter`, `ListSelect`, and related APIs) for safer in-script shaping of `Instance.Data`. **AutoMapper** has been replaced by **Mapperly** in the Aether SDK, and AutoMapper is no longer a vNext dependency.

---

## Bug Fixes

### Instance state during subflow completion propagation (#461)

While a subflow completion was still propagating, the parent instance could incorrectly report **Completed** status. The **State** function (long polling) was adjusted so parent status reflects the correct phase during that window.

> **Reference:** [#461](https://github.com/burgan-tech/vnext/issues/461)

### Schema: `flowVersion` and `config` required (#88, #89)

**flowVersion** is required across definition schemas, and **config** is required on task definitions in vnext-schema, aligning validation with runtime and avoiding serialization issues.

> **References:** [#88](https://github.com/burgan-tech/vnext-schema/issues/88), [#89](https://github.com/burgan-tech/vnext-schema/issues/89)

### Schema: `errorBoundary` abort action aligned with backend (#93)

The workflow definition schema for **errorBoundary** (abort action / transition) was aligned with backend validators.

> **Reference:** [#93](https://github.com/burgan-tech/vnext-schema/issues/93)

### Workflow CLI: correct `.csx` file when names collide (#10)

When the same **.csx** file name existed in multiple component directories, the CLI could update the wrong file. Selection now respects the configured **location** (vnext-workflow-cli **v1.0.6**).

> **Reference:** [#10](https://github.com/burgan-tech/vnext-workflow-cli/issues/10)

### Init service: timestamps in logs and clearer errors (#475)

Init service logs include **timestamps**. Helm chart updates increase **nginx ingress** timeouts where init was timing out.

> **Reference:** [#475](https://github.com/burgan-tech/vnext/issues/475)

### Remote view / roles: headers, ViewType, init domain replacement, StateSubType (#482)

HTTP handling for **remote view** and **remote role** calls was hardened (null-safety, **ViewType** enum, **content-header** forwarding). Remote view definitions receive corrected **domain replacement** in init. **StateSubType** adds **Cancelled = 7** and **Timeout = 8**.

> **Reference:** [#482](https://github.com/burgan-tech/vnext/issues/482)

---

## Features

### Flow DB schema at deploy time (#449)

Per-request migration checks on start and transition have been removed. A **DB-Migrator job** applies schema creation and migrations at **deploy** time.

> **Reference:** [#449](https://github.com/burgan-tech/vnext/issues/449)

### Telemetry: Aether logging and TaskCoordinator spans (#451)

Logging is more consistent end-to-end, and **task executions** are easier to trace with **span** instrumentation on the task coordinator path.

> **Reference:** [#451](https://github.com/burgan-tech/vnext/issues/451)

### Synchronous start/transition: mapped instance payload (#393)

When **`sync=true`**, start and transition responses can include **`key`**, **`attributes`**, **`eTag`**, **`entityEtag`**, **`extensions`**, and related instance envelope fields—not only `id` and `status`.

> **Reference:** [#393](https://github.com/burgan-tech/vnext/issues/393)

### SubFlow `overrides` for roles and timeouts (#405)

Beyond legacy **view** overrides, **`subFlow.overrides`** supports **transitions** (e.g. role grants), **states** (e.g. **queryRoles**), **timeout** definitions, and other replace-mode overrides for authorize and transition list behavior.

> **Reference:** [#405](https://github.com/burgan-tech/vnext/issues/405)

### Mapperly replaces AutoMapper in Aether / vNext (#52)

**Mapperly** is used in the Aether SDK; **AutoMapper** (commercial license) is no longer required for vNext.

> **Reference:** [#52](https://github.com/burgan-tech/aether/issues/52)

### Multi-task functions and output mapping (#478)

Function definitions may declare multiple **`onExecutionTasks`** (ordered). Each task can supply **mapping**; later tasks can consume earlier task outputs. Optional **`output`** mapping uses **`IOutputHandler`** to build the function’s final script response.

> **Reference:** [#478](https://github.com/burgan-tech/vnext/issues/478)

### ScriptBase: collection and dynamic-object helpers

**`ScriptBase`** now includes helpers to work with **dynamic objects** and **lists** in mapping scripts—e.g. **`CreateObject`**, **`CreateList`**, **`SetProperty`**, **`GetList`**, **`AsList`**, **`ListAdd`** / **`ListRemove`**, **`ListFilter`**, **`ListCount`**, **`ListAny`**, **`ListFirst`** / **`ListLast`**, **`ListSelect<TResult>`**, **`RemoveProperty`**, and **`ToDictionary`**. These reduce fragile casts when reading or building structures under **`Instance.Data`**.

**Documentation:** [Mapping Guide — ScriptBase](../doc/en/flow/mapping.md#scriptbase-usage) (see **Collection and dynamic object helpers**). **Runnable sample mappings:** [`release/extra/script-base-usage/`](./extra/script-base-usage/).

### ETag strategy (authorization-aware) follow-up (#448)

Work continues to keep **etag** / **entityETag** and response headers consistent when authorization changes the response shape (see also [Release Notes v0.0.40](./RELEASE-NOTES-v0.0.40.md)).

> **Reference:** [#448](https://github.com/burgan-tech/vnext/issues/448)

---

## Breaking Changes

1. **`GET /api/v1/{domain}/workflows/{workflow}/functions/{function}`** is **removed**. Integrations that called a workflow-level function by name must migrate to supported APIs (e.g. instance-scoped **State**, **Data**, **View**, **Schema**, or other documented endpoints).

2. **GetInstancesTask** (task type `15`) now calls **`GET /api/v1/{domain}/workflows/{workflow}/instances`** (with query parameters for filter, paging, sort) instead of the previous workflow **functions/data**-style URL. **Validate mapping scripts** and consumers of task output against the new response shape.

3. **Domain package validation (`validate.js`):** Repositories using the shared validator must adopt **`Ajv2019`** (JSON Schema draft **2019-09**) so workflow definitions validate against updated schemas (including **errorBoundary** fixes). See internal **`release/extra/validate.js`** or your template’s migration notes for the full script changes.

---

## Configuration Updates

Configuration for v0.0.42:

```json
{
  "runtimeVersion": "0.0.42",
  "schemaVersion": "0.0.39"
}
```

> **Note:** Use schema package **0.0.39** with this runtime. Update **`validate.js`** and domain tooling accordingly.

---

## Issues Referenced

- [vnext #461](https://github.com/burgan-tech/vnext/issues/461) — Subflow propagation / instance state
- [vnext-schema #88](https://github.com/burgan-tech/vnext-schema/issues/88) — `flowVersion` required
- [vnext-schema #89](https://github.com/burgan-tech/vnext-schema/issues/89) — Task `config` required
- [vnext-schema #93](https://github.com/burgan-tech/vnext-schema/issues/93) — `errorBoundary` schema alignment
- [vnext-workflow-cli #10](https://github.com/burgan-tech/vnext-workflow-cli/issues/10) — CSX selection by location
- [vnext #475](https://github.com/burgan-tech/vnext/issues/475) — Init logs / ingress timeout
- [vnext #482](https://github.com/burgan-tech/vnext/issues/482) — Remote view/roles, StateSubType 7–8
- [vnext #449](https://github.com/burgan-tech/vnext/issues/449) — DB-Migrator / deploy-time schema
- [vnext #451](https://github.com/burgan-tech/vnext/issues/451) — Telemetry / spans
- [vnext #393](https://github.com/burgan-tech/vnext/issues/393) — Sync start/transition payload
- [vnext #405](https://github.com/burgan-tech/vnext/issues/405) — SubFlow overrides
- [aether #52](https://github.com/burgan-tech/aether/issues/52) — Mapperly
- [vnext #448](https://github.com/burgan-tech/vnext/issues/448) — ETag strategy
- [vnext #478](https://github.com/burgan-tech/vnext/issues/478) — Multi-task functions / output mapping

---

## Summary

- **Deploy-time** database schema lifecycle via **DB-Migrator**; no per-request migration on start/transition.
- **Better telemetry**: structured logs and **TaskCoordinator** spans.
- **`sync=true`** start/transition returns **full mapped instance** fields where applicable.
- **SubFlow `overrides`** for roles, states, timeouts; **multi-task functions** with **`IOutputHandler`** output mapping.
- **ScriptBase** **collection / dynamic-object** helpers and samples under **`release/extra/script-base-usage/`**.
- **Breaking:** removed **`.../functions/{function}`**; **GetInstancesTask** uses **`.../instances`**; update **`validate.js`** for **Ajv2019**.

---

**vNext Runtime Platform Team**  
March 25, 2026
