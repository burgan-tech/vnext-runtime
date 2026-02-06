# Other Tasks

## At a glance

Beyond HTTP and service invocation, the platform supports additional task types that help build complete end-to-end processes.

## Event-driven messaging (Pub/Sub)

When you need decoupled communication (publish an event and let consumers react), pub/sub tasks support event-driven architectures.

Business scenarios:

- notify downstream domains about a completed step
- emit “case created/approved/failed” events
- integrate with analytics and monitoring pipelines

Developer reference:

- `docs/technical/flow/tasks/dapr-pubsub.md`

## Cross-workflow orchestration (Trigger task family)

When a workflow must coordinate other workflows (start, trigger, fetch data, start subprocesses), trigger tasks provide controlled orchestration.

Business scenarios:

- start an approval workflow from an onboarding workflow
- trigger a transition in a dependent workflow when a condition is met
- retrieve data to make a routing decision

Developer reference:

- `docs/technical/flow/tasks/trigger-task.md`

## Fetching instances (GetInstances)

When reporting or coordination requires reading instance lists from another workflow, “get instances” style tasks support that.

Developer reference:

- `docs/technical/flow/tasks/get-instances-task.md`

## System-only tasks (Condition, Timer)

Some task types are primarily internal/system-driven (e.g., timer mechanics, condition evaluation plumbing). From a business perspective, these enable:

- rule-based routing (auto transitions)
- time-based behavior (timeouts, reminders)

Developer reference:

- `docs/technical/flow/tasks/condition-task.md`
- `docs/technical/flow/tasks/timer-task.md`

