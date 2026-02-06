# Hierarchical Workflows (SubFlows, SubProcesses)

## At a glance

- **Audience**: Developers and analysts modelling reusable sub-processes and parallel orchestration patterns.
- **Goal**: Explain SubFlow vs SubProcess, how child executions are triggered, and how correlations behave.
- **Key idea**: Hierarchical workflows enable decomposition: reuse common process parts and orchestrate parallel work safely.

## Why hierarchical workflows exist

Real business processes often contain:

- reusable “modules” (e.g., identity check, document collection),
- parallel work (e.g., run scoring and document verification concurrently),
- and cross-cutting lifecycle actions (e.g., cancel everything consistently).

SubFlows/SubProcesses make these patterns first-class and governable.

## SubFlow vs SubProcess (the key difference)

### SubFlow

- **Blocks the parent**: the parent workflow waits while the SubFlow runs.
- Best when the sub-process is a strict prerequisite (e.g., “complete KYC before approval”).

### SubProcess

- **Does not block the parent**: it runs in parallel in isolation.
- Best for parallel work and fan-in/fan-out patterns (e.g., “start notifications and reporting in parallel”).

## Triggering child flows (how it is represented)

A SubFlow state typically contains a subflow reference configuration (process reference + optional mapping), which defines:

- which child workflow to run,
- how to map parent context into child input,
- how results are correlated back to the parent.

In business terms: a “state” can represent “run another governed process as part of this process”.

## Correlations (how relationships are established)

When a parent starts a SubFlow/SubProcess, the runtime maintains correlation links between:

- the parent instance, and
- the child instance(s),

so the platform can manage lifecycle actions consistently (e.g., cancellation, monitoring “effective state”).

## Cancellation behavior (cascading cancel)

Cancellation is not only a UI action—it’s an operational requirement.

Key behavior observed in the platform docs:

- When a parent is cancelled, **active jobs, tasks, and active correlations (SubFlows/SubProcesses)** are candidates to be cancelled.
- For cascading cancel to apply cleanly, **child flow definitions must also define cancel behavior** (otherwise the system may bypass if it cannot complete child cancellation).

## Practical modelling guidance

- Use **SubFlow** when the parent should not proceed until the child has reached an expected outcome.
- Use **SubProcess** when the parent can continue while the child runs, and later “fan-in” when required.
- Define and standardize cancel across parent and child flows to avoid orphaned work.

## Where to go deeper (developer reference)

- `docs/technical/flow/state.md` (SubFlow/SubProcess note)
- `docs/technical/flow/flow.md` (SubFlow/SubProcess types, cancel + correlations)

