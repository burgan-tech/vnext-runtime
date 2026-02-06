# Task Types

## At a glance

- **What it is**: Tasks are the executable building blocks that perform work inside a workflow (integrations, calculations, orchestration).
- **Business value**: Standardize how work is performed across workflows while keeping execution observable and governable.
- **Where it fits**: Transition execution, state entry/exit actions, cross-workflow orchestration.

## How tasks are used (high level)

Tasks are typically executed at specific points:

- **Transition execution** (`onExecutionTasks`): work that happens when a decision/action is triggered
- **State entry/exit** (`onEntries`, `onExits`): work that happens when entering/leaving a stage

This makes “what happens when” explicit and easier to operate and audit.

## Task catalog (business-oriented)

- [HTTP Tasks](./http-tasks.md)
- [Dapr Service Calls](./dapr-service-calls.md)
- [Human Tasks](./human-tasks.md)
- [SubProcess Tasks](./subprocess-tasks.md)
- [Other Tasks](./other-tasks.md)

## Where to go deeper (developer reference)

- `docs/technical/flow/task.md` (task catalog, execution points, ordering)
- `docs/technical/flow/tasks/*` (task-specific docs)

