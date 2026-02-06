# Scripting Engine (Dynamic C#)

## At a glance

- **What it is**: A governed way to execute **dynamic C# scripts** inside workflows for business logic and data transformation.
- **Business value**: Faster iteration on rules and transformations without rebuilding multiple downstream services.
- **Where it fits**: Calculations, validations, data shaping, and rule evaluation inside workflow execution.

## Why it matters

Most enterprise processes require logic that is:

- specific to the domain (eligibility, thresholds, routing),
- frequently changed (policy updates, product changes),
- and tightly coupled to orchestration steps (before/after integrations).

The scripting engine supports this by allowing workflow packages to include script-based logic in a controlled way.

## What it enables (typical scenarios)

- **Business logic**: compute risk tiers, fees, eligibility, scoring decisions
- **Data transformation**: normalize payloads from different systems; enrich instance data for next steps
- **Validation**: verify required fields and invariants before calling external systems

## Guardrails (important operational expectations)

To keep runtime predictable and safe, script execution is intended to be:

- **fast** (avoid long-running operations),
- **deterministic** (no external dependencies in the critical path),
- **focused on data and logic** (not system integration).

In practice, external calls should be handled via dedicated task types (HTTP/Dapr) rather than inside scripts.

## Minimal example (conceptual)

You can think of scripting as “derive new business facts from existing facts”:

- Input: customer attributes and application data
- Script: compute score and route category
- Output: add `riskTier`, `isEligible`, etc. to instance data for downstream transitions

For production-grade code and task configuration, reference [vnext-example](https://github.com/burgan-tech/vnext-example).

## Where to go deeper (developer reference)

- `docs/technical/flow/tasks/script-task.md` (Script Task behavior and limitations)
- `docs/technical/flow/mapping.md` and `docs/technical/flow/interface.md` (mapping interfaces and script context)

