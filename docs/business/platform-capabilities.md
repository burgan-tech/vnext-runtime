# Platform Capabilities

This section describes the platform-level capabilities of the vNext Workflow Engine from a business perspective—what it enables and how it supports enterprise process automation.

## At a glance

- **Purpose**: Describe platform capabilities that determine suitability for enterprise process automation.
- **Best for**: Developers and analysts shaping processes, governance, isolation, and execution patterns.
- **Key questions answered**: How do flows work end-to-end, how do states/transitions behave, how does multi-schema work, and when to use sync vs async?

## Structure

This page is an overview. Detailed documents live under `docs/business/platform-capabilities/`.

- [Workflow Definition and Management](./platform-capabilities/workflow-definition-and-management.md)
- [State Machine with Conditional Transitions](./platform-capabilities/state-machine-with-conditional-transitions.md)
- [Multi-tenant Architecture (Multi-Schema)](./platform-capabilities/multi-tenant-architecture-multi-schema.md)
- [Hierarchical Workflows (SubFlows, SubProcesses)](./platform-capabilities/hierarchical-workflows-subflows-subprocesses.md)
- [Real-time and Asynchronous Execution](./platform-capabilities/real-time-and-asynchronous-execution.md)

## Workflow definition and management

- Model business processes as **workflows** with explicit states, transitions, and tasks
- Apply governance via versioning and controlled evolution of process definitions
- Support multiple process variants across domains and lifecycle stages

Read more: [Workflow Definition and Management](./platform-capabilities/workflow-definition-and-management.md)

## State machine with conditional transitions

- Represent process progression as a **state machine**, enabling clear “where are we?” tracking
- Use **conditions** to determine routing and automation decisions
- Support both user-driven and automated progression, depending on the process need

Read more: [State Machine with Conditional Transitions](./platform-capabilities/state-machine-with-conditional-transitions.md)

## Multi-tenant architecture (multi-schema)

- Isolate tenants/domains while sharing infrastructure where appropriate
- Enable per-domain data separation patterns (e.g., multi-schema) to align with compliance boundaries
- Support organizational scaling by minimizing cross-domain coupling

Read more: [Multi-tenant Architecture (Multi-Schema)](./platform-capabilities/multi-tenant-architecture-multi-schema.md)

## Hierarchical workflows (SubFlows, SubProcesses)

- Decompose complex processes into reusable sub-processes
- Improve maintainability by treating common steps as modular building blocks
- Enable consistent behavior across multiple business workflows

Read more: [Hierarchical Workflows (SubFlows, SubProcesses)](./platform-capabilities/hierarchical-workflows-subflows-subprocesses.md)

## Real-time and asynchronous execution

- Enable immediate (“synchronous”) responses for interactive experiences when needed
- Enable asynchronous orchestration for long-running, event-driven, and scheduled processes
- Support resilient execution patterns across distributed services and integrations

Read more: [Real-time and Asynchronous Execution](./platform-capabilities/real-time-and-asynchronous-execution.md)

