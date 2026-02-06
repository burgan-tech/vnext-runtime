# SubProcess Tasks

## At a glance

- **What it is**: A task-based way to start independent subprocess instances during workflow execution.
- **Business value**: Enable parallelization and decomposition while keeping cross-process coordination governed.
- **Where it fits**: Fan-out processing, independent audits, parallel checks, asynchronous side processes.

## When to use it

Use subprocess tasks when:

- you need to launch a separate workflow instance as part of a larger process,
- the child execution can run independently (often in parallel),
- and you want an explicit correlation link for monitoring and lifecycle operations.

Typical examples:

- Start a “notification” workflow while the main workflow continues
- Launch background validation processes
- Trigger separate “audit” workflows for traceability

## Relationship to hierarchical workflows

SubProcess tasks complement hierarchical patterns:

- **SubFlow state**: blocks the parent while the child runs
- **SubProcess**: runs in parallel and does not block by default

## Where to go deeper (developer reference)

- `docs/technical/flow/tasks/trigger-task.md` (SubProcessTask type and usage)
- `docs/technical/flow/state.md` (SubFlow vs SubProcess conceptual differences)

