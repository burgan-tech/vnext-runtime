# Script Execution

## At a glance

- **What it is**: Executing script-based logic as part of workflow execution (typically via Script Tasks and mappings).
- **Business value**: Apply domain rules and transformations exactly where they belong in the process, with clear auditability and versioning.
- **Where it fits**: Validations, calculations, routing preparation, and response shaping.

## How script execution shows up in a workflow

Script-based logic is commonly used in three places:

- **Task execution**: a Script Task runs domain logic and returns data to the workflow.
- **Transition mapping**: prepare inputs/outputs when a transition is triggered (e.g., reshape payload before calling a service).
- **Rule/timer mappings**: support automatic and scheduled transitions (business conditions and schedules).

From a business perspective, this means you can keep “policy logic” close to the process stage where it matters.

## Guardrails (important expectations)

Script execution should remain:

- **fast** (keep it bounded),
- **reliable** (avoid external dependencies inside scripts),
- **governed** (version, review, test, and roll out like any policy change).

For integrations, prefer dedicated task types (HTTP/Dapr) and use scripts for transformation and decision logic around them.

## Minimal example (conceptual)

Example: compute `riskTier` and `isEligible` based on instance attributes, then use that output for routing in the next step.

For complete working examples with tasks + mappings, refer to [vnext-example](https://github.com/burgan-tech/vnext-example).

## Where to go deeper (developer reference)

- `docs/technical/flow/tasks/script-task.md` (script task behavior and limitations)
- `docs/technical/flow/mapping.md` and `docs/technical/flow/interface.md` (mapping context and interfaces)

