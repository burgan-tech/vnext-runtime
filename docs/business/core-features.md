# Core Features

This section summarizes the core features of the vNext Workflow Engine in business terms.

## At a glance

- **Purpose**: Summarize workflow, task, scripting, integration, and operational feature sets.
- **Best for**: Readers who want a capability checklist aligned with business outcomes.
- **Key questions answered**: What features exist and what do they enable?

## Workflow engine

- **State management**: Track and persist process progress with a clear current state.
- **Transition pipeline**: Control and validate movement between states, including business rules.
- **Condition evaluation**: Route work based on data and context (e.g., thresholds, eligibility rules).
- **Error handling**: Support recoverable failures and controlled retries/compensation patterns.

## Task types

Tasks are the executable units within workflows that perform business-relevant actions.

- **HTTP tasks**: Integrate with external systems via REST endpoints.
- **Dapr service calls**: Invoke microservices through Dapr-enabled service discovery and resiliency.
- **Script execution**: Apply business logic and transformations using dynamic C# scripting.
- **Human tasks**: If human-in-the-loop steps are required, they can be represented as manual/approval transitions or as a dedicated task pattern depending on runtime support.
- **Timer / scheduled tasks**: Run delayed or scheduled actions (reminders, retries, periodic checks).

## Scripting engine

- **Dynamic C# scripts**: Express business logic without rebuilding services for every small rule change (with appropriate governance).
- **Data transformation**: Normalize, enrich, and reshape payloads between systems.
- **Business logic**: Perform calculations, validations, and rule evaluations as part of orchestration.

## Integration (feature-level overview)

- **REST APIs**: External systems can start workflows, query instances, and trigger transitions.
- **Dapr runtime**: Native fit for microservice ecosystems and sidecar-based integrations.
- **Event-driven**: Supports event-triggered progress for decoupled architectures.
- **Service discovery**: Reduce configuration overhead by relying on platform/service discovery mechanisms.

## Operations (feature-level overview)

- **Caching**: Improve performance and reduce load for frequently accessed data.
- **Metrics**: Measure throughput, latency, error rates, and business KPIs derived from workflow execution.
- **Logging**: Provide operational traceability for execution decisions and failures.
- **Tracing**: Correlate cross-service workflow execution using distributed tracing.

## Structure

This page is an overview. Detailed documents live under `docs/business/core-features/`:

- [Task Types](./core-features/task-types/README.md)
- [Script Execution](./core-features/script-execution.md)
- [Scripting Engine](./core-features/scripting-engine.md)
- [Auto Transitions](./core-features/auto-transitions.md)
- [Timer Execution](./core-features/timer-execution.md)
- [Instance Filtering](./core-features/instance-filtering.md)
- [Caching Strategy](./core-features/caching-strategy.md)
- [Service Discovery](./core-features/service-discovery.md)

## Features explicitly called out (Issue #330)

- **Scripting Engine**: Dynamic C# script execution for business logic. Read more: [Scripting Engine](./core-features/scripting-engine.md)
- **Auto Transitions**: Automatic state transitions based on conditions. Read more: [Auto Transitions](./core-features/auto-transitions.md)
- **Timer Execution**: Scheduled and delayed task execution. Read more: [Timer Execution](./core-features/timer-execution.md)
- **Instance Filtering**: Advanced query capabilities for workflow instances. Read more: [Instance Filtering](./core-features/instance-filtering.md)
- **Caching Strategy**: High-performance distributed caching approach. Read more: [Caching Strategy](./core-features/caching-strategy.md)
- **Service Discovery**: Automatic domain registration and discovery patterns. Read more: [Service Discovery](./core-features/service-discovery.md)

