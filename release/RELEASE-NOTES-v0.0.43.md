# vNext Runtime Platform - Release Notes v0.0.43
**Release Date:** April 6, 2026

## Overview

This release improves **reliability and consistency** across flows, tasks, and domain events: **cancel** transitions on single-flow definitions validate correctly, **TriggerTask** family tasks map **HTTP status codes** on local execution for **errorBoundary** matching, and **AmbientServiceProvider** scopes restore **domain event** delivery (including effective-state and subflow completion paths). **GetInstance** / **GetInstanceData** honor the **`version`** query parameter, **GroupBy** and aggregation work when filters reference **Instance** table columns, **DaprServiceTask** runs without invalid trace context for **non-flow** functions, and **definition cache** warm-up no longer loads the full active-instance set into memory (**OutOfMemoryException** fix). New capabilities include **auto-transition rules** evaluated with **Dynamic Expresso** over **instance JSONB**, and **instance-data–based authorization** using **`$user`**, **`$role`**, and **`$userBehalfOf`** JSONPath-style selectors plus **`$InstanceBehalfOfStarter`** and **`$PreviousBehalfOfUser`**.

---

## Bug Fixes

### Flow-level cancel transition validation (#509)

The well-known **cancel** transition key resolved correctly but failed **StateTransitionListSpecification** validation (**Transition:100020**) on single-flow workflows. Validation now accepts the cancel definition as intended.

> **Reference:** [#509](https://github.com/burgan-tech/vnext/issues/509)

### DaprServiceTask: trace context without flow instance (#507)

**DaprServiceTask** could produce an invalid trace context when **function** executions ran **without** a workflow instance. Execution is corrected for this scenario.

> **Reference:** [#507](https://github.com/burgan-tech/vnext/issues/507)

### AmbientServiceProvider in Gateway and PostCommit scopes (#504)

**AmbientServiceProvider.Current** is restored consistently across **local Gateway** and **PostCommit** scopes, preventing **domain event** loss and related hook gaps. **Effective state** and **subflow completion** events are stabilized on this path.

> **Reference:** [#504](https://github.com/burgan-tech/vnext/issues/504)

### GroupBy / aggregation with Instance column filters (#488)

Aggregations using **GroupBy** failed when the filter referenced **Instance** metadata columns alongside instance data filters. Query generation is fixed for this combination.

> **Reference:** [#488](https://github.com/burgan-tech/vnext/issues/488)

### TriggerTask family: StatusCode on local execution (#493)

**TriggerTask**-family tasks did not map **HTTP StatusCode** on **local** execution, so **errorBoundary** matching against standard codes (e.g. **409**, **404**) was unreliable. Local execution now aligns with the **result pattern** and error-boundary expectations.

> **Reference:** [#493](https://github.com/burgan-tech/vnext/issues/493)

### GetInstance / GetInstanceData: `version` query parameter (#490)

The **`version`** query parameter was ignored on **GetInstance** and **GetInstanceData**; operations now respect instance version as intended for consistency.

> **Reference:** [#490](https://github.com/burgan-tech/vnext/issues/490)

### Definition cache invalidation: memory use (#487)

Full **active-instance** loads during definition cache warm-up could cause **OutOfMemoryException**. Initialization uses a revised approach that avoids loading the entire active-instance table into memory.

> **Reference:** [#487](https://github.com/burgan-tech/vnext/issues/487)

---

## Features

### Auto-transition rules with Dynamic Expresso over instance data (#470)

**Automatic** transition **Rule** evaluation can use **Dynamic Expresso** for plain-text **boolean** expressions over **instance JSON** and **ScriptContext**, as an **optional** alternative to **Roslyn** **`IConditionMapping`** scripts.

**How to select Expresso vs Roslyn**

- Set **`rule.location`** to **`dynamicExpresso`**, put the expression in **`rule.code`**, and use **native** encoding: **`"encoding": "NAT"`** in JSON (or **`ScriptCode.FromNative`** in code).
- Any other **`location`** keeps the Roslyn path (**RoutingConditionEvaluator** → **ScriptConditionEvaluator**).

**`context` parameter (`ExpressoRuleContext`)**

Expressions take a single **`context`** argument built from **`ScriptContext`** with an allowlist, including: **`Body`**, **`CurrentTransition`** (e.g. **Data** / **Header**), **`MetaData`**, **`Workflow`**, **`Instance`** (including **Data** as JSON), **`Headers`**, **`QueryParameters`**, **`RouteValues`**, **`Transition`** (key/from/target/trigger fields; no embedded rule/timer/task payloads), and **`Runtime`**. Some members may be **null** when not present on the context.

**JSON access**

**`Instance.Data`**, **`Body`**, and related JSON are surfaced as **`RuleJsonDynamic`** (dynamic members, indexers, array **Count** / **Contains**). Missing keys resolve to **null**. Prefer **`AsDouble()`** / **`AsInt32()`**, **`AsBoolean()`**, **`AsArrayLength()`**, and array **`Contains`** as needed.

**Expression samples**

```text
context.Instance.Data["amount"].AsDouble() > 100000
context.Instance.Data["documents"].AsArrayLength() == 0
context.Body["score"].AsDouble() >= 80
context.Transition != null && context.Transition.Key == "approve-auto"
```

**Definition fragment (NAT)**

```json
"rule": {
  "location": "dynamicExpresso",
  "encoding": "NAT",
  "code": "context.Instance.Data.absenceType.ToString() == \"personal-leave\""
}
```

Full detail, **`ExpressoRuleContext`** member table, and additional examples: [Transitions](../doc/en/flow/transition.md) — subsection **Rule expressions with Dynamic Expresso (v0.0.43+)**.

> **Reference:** [#470](https://github.com/burgan-tech/vnext/issues/470)

### Instance-data authorization: `$user` / `$role` / `$userBehalfOf` JSONPath (#469)

Authorization is extended to compare token values to data resolved from **ScriptContext** (including **instance data**) via JSONPath-style expressions:

| Prefix | Role in comparison |
|--------|-------------------|
| **`$user.<path>`** | **Actor** token vs. value at `<path>` |
| **`$role.<path>`** | **Role** token vs. value at `<path>` |
| **`$userBehalfOf.<path>`** | **Subject** (behalf-of) token vs. value at `<path>` |

Additional **system roles**:

| Role | Meaning |
|------|---------|
| **`$InstanceBehalfOfStarter`** | Subject (behalf-of) for the instance starter |
| **`$PreviousBehalfOfUser`** | Subject (behalf-of) for the actor of the **previous** transition |

This applies across **available transition** and **data** authorization (including **Master** schema field visibility where supported).

**Documentation:** [Function APIs — Authorization](../doc/en/flow/function.md) (subsection **Instance-data JSONPath authorization (v0.0.43+)**).

> **Reference:** [#469](https://github.com/burgan-tech/vnext/issues/469)

---

## Breaking Changes

None for this release.

---

## Configuration Updates

Configuration for v0.0.43:

```json
{
  "runtimeVersion": "0.0.43",
  "schemaVersion": "0.0.39"
}
```

> **Note:** **Schema package 0.0.39** remains the documented pairing with this runtime (same as v0.0.42). Update domain **`validate.js`** / **Ajv2019** usage per [Schema Management](../doc/en/flow/schema.md) if you have not already.

---

## Issues Referenced

- [vnext #509](https://github.com/burgan-tech/vnext/issues/509) — Cancel transition validation (single-flow)
- [vnext #507](https://github.com/burgan-tech/vnext/issues/507) — DaprServiceTask trace context without instance
- [vnext #504](https://github.com/burgan-tech/vnext/issues/504) — AmbientServiceProvider / domain events
- [vnext #488](https://github.com/burgan-tech/vnext/issues/488) — GroupBy + Instance filters
- [vnext #493](https://github.com/burgan-tech/vnext/issues/493) — TriggerTask StatusCode / errorBoundary
- [vnext #490](https://github.com/burgan-tech/vnext/issues/490) — GetInstance(GetInstanceData) version query
- [vnext #487](https://github.com/burgan-tech/vnext/issues/487) — Definition cache OOM
- [vnext #470](https://github.com/burgan-tech/vnext/issues/470) — Dynamic Expresso auto-transition rules
- [vnext #469](https://github.com/burgan-tech/vnext/issues/469) — Instance JSONPath authorization

---

## Summary

- **Cancel** transition validation fixed for single-flow definitions.
- **Stable domain events** and hooks via **AmbientServiceProvider** scope fixes; **effective state** / **subflow completion** paths improved.
- **GetInstance** / **GetInstanceData** respect **`version`**; **GroupBy** with **Instance** filters fixed; **DaprServiceTask** without instance fixed; **TriggerTask** **StatusCode** for **errorBoundary** on local runs.
- **Memory-safe** definition cache warm-up (no full active-instance table load).
- **Dynamic Expresso** for **auto-transition** rules over instance data; **JSONPath-style** **`$user` / `$role` / `$userBehalfOf`** authorization and new behalf-of system roles.

---

**vNext Runtime Platform Team**  
April 6, 2026
