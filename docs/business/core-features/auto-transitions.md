# Auto Transitions (Condition-Based Routing)

## At a glance

- **What it is**: The ability for the system to move a workflow forward automatically when **conditions/rules** are met.
- **Business value**: Reduce manual handling, speed up decisions, and standardize routing.
- **Where it fits**: Eligibility checks, risk routing, straight-through-processing (STP), exception handling.

## Why it matters

Many processes are “mostly predictable”:

- a large portion can be auto-approved/auto-routed,
- a smaller portion needs review, escalation, or exception handling.

Auto transitions allow you to express this split explicitly, improving throughput while keeping governance intact.

## How it works (high level)

- Auto transitions are **system-executed** (not triggered directly by clients).
- Each auto transition has a **target state** and (optionally) a **rule** that determines whether it should fire.
- When a state is entered, the platform can evaluate automatic transitions as part of the lifecycle pipeline.

## Typical business patterns

- **Threshold routing**: amount < limit → STP; otherwise → review
- **Risk tier routing**: low → approve; medium → additional checks; high → reject/escalate
- **Document completeness**: complete → proceed; missing → request documents

## Governance considerations

- Treat routing rules as **policy artifacts**: version them, review them, and test them.
- Ensure outcomes are **explainable** (capture the reason or key decision inputs).
- Prefer deterministic rules that are stable under retry scenarios.

## Minimal example (readable transition shape)

```json
{
  "key": "auto-approve",
  "target": "approved",
  "triggerType": 1,
  "versionStrategy": "Minor",
  "rule": { "ref": "Mappings/auto-approval-rule.csx" }
}
```

For complete workflow examples (including rule scripts and mappings), reference [vnext-example](https://github.com/burgan-tech/vnext-example).

## Where to go deeper (developer reference)

- `docs/technical/flow/transition.md` (automatic trigger type, default auto transition behavior)
- `docs/technical/flow/state.md` (where auto transitions run in the lifecycle)

