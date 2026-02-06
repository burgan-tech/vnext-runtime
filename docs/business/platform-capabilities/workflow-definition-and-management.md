# Workflow Definition and Management

## At a glance

- **Audience**: Developers and analysts defining, reviewing, and evolving business processes.
- **Goal**: Explain how a flow is defined, how it is started, and how governance (domain, versioning, reuse) is applied.
- **Key idea**: A workflow is a **versioned state machine** composed of **states**, **transitions**, and **tasks**.

## What is a workflow (flow)?

In vNext, a workflow models a business process as a **state machine**. A workflow definition is stored as a structured document (JSON) and is versioned so it can evolve safely over time.

From a platform perspective, a workflow definition typically includes:

- **Identity & governance**: `domain`, `key`, `version`, `type`
- **Entry point**: `startTransition` (required)
- **Process graph**: `states[]` (each state can contain transitions and tasks)
- **Reusable/shared behaviors**: `sharedTransitions`, `features`
- **Extensibility**: `functions`, `extensions`
- **Operational policies**: `timeout`, `cancel`, `errorBoundary`

## Workflow types (why they matter)

Workflow type determines the intent and runtime behavior:

- **Flow (F)**: Main business workflows (primary customer/employee journeys).
- **SubFlow (S)**: Reusable sub-processes invoked from a parent workflow (blocks parent while running).
- **SubProcess (P)**: Parallel, independent operations (does not block parent; used for fan-in/fan-out patterns).
- **Core (C)**: Platform/core workflows (system operations).

## Defining a flow: the minimum viable definition

A flow definition should be understandable to both analysts (process clarity) and developers (execution clarity). The core building blocks are:

### 1) `domain`, `key`, `version`

- **domain**: The organizational namespace (often maps to a business domain and also to runtime isolation patterns).
- **key**: A unique identifier within a domain (recommended kebab-case).
- **version**: Semantic versioning (SemVer) enables controlled change.

### 2) `startTransition` (required)

Every workflow has a `startTransition` that defines:

- how a new instance enters the process,
- which initial state is targeted,
- optional validation and tasks to run at start.

### 3) `states[]` (the process graph)

Each state represents a stage in the process, with:

- transitions that move to the next stage,
- entry/exit tasks for side effects and integrations,
- optional view references for UI representation,
- and (for hierarchical flows) optional subflow configuration.

## Minimum example (starter flow)

This is a minimal, readable example you can use as a starting point. It shows:

- a `startTransition` that enters an initial state,
- a manual approval path,
- an automatic route (rule-based),
- and clear finish outcomes.

```json
{
  "type": "F",
  "key": "account-opening",
  "domain": "onboarding",
  "version": "1.0.0",
  "startTransition": {
    "key": "start",
    "target": "collect-info",
    "triggerType": 0,
    "versionStrategy": "Major",
    "schema": { "ref": "Schemas/start.json" }
  },
  "states": [
    {
      "key": "collect-info",
      "stateType": 1,
      "versionStrategy": "Minor",
      "onEntries": [
        { "order": 1, "task": { "ref": "Tasks/validate-input.json" } }
      ],
      "transitions": [
        { "key": "submit", "target": "risk-check", "triggerType": 0, "versionStrategy": "Minor" }
      ]
    },
    {
      "key": "risk-check",
      "stateType": 2,
      "versionStrategy": "Minor",
      "transitions": [
        {
          "key": "auto-approve",
          "target": "approved",
          "triggerType": 1,
          "versionStrategy": "Minor",
          "rule": { "ref": "Mappings/auto-approval-rule.csx" }
        },
        { "key": "route-to-human", "target": "human-review", "triggerType": 1, "versionStrategy": "Minor" }
      ]
    },
    {
      "key": "human-review",
      "stateType": 2,
      "versionStrategy": "Minor",
      "transitions": [
        {
          "key": "approve",
          "target": "approved",
          "triggerType": 0,
          "versionStrategy": "Minor",
          "schema": { "ref": "Schemas/approve.json" }
        },
        { "key": "reject", "target": "rejected", "triggerType": 0, "versionStrategy": "Minor" }
      ]
    },
    { "key": "approved", "stateType": 3, "subType": 1, "versionStrategy": "Minor" },
    { "key": "rejected", "stateType": 3, "subType": 2, "versionStrategy": "Minor" }
  ]
}
```

### How it runs (high level)

- **Start** creates an instance and enters `collect-info`.
- A user/client triggers `submit` to move into `risk-check`.
- The system evaluates automatic transitions:
  - if rules match, it transitions to `approved` (auto path),
  - otherwise it routes to `human-review` (manual decision).

### Where to find full examples

- For complete, production-grade examples (tasks, mappings, views, and real integrations), reference [vnext-example](https://github.com/burgan-tech/vnext-example).

## Management concerns (what to decide up-front)

### Versioning and change control

Use versioning to manage evolution:

- **Minor**: backward-compatible or incremental changes
- **Major**: breaking behavior changes that require coordination/migration

### Naming and readability

Make the definition readable as a business process:

- states named by **business stage** (e.g., `verification`, `risk-review`, `approved`)
- transitions named by **decision/action** (e.g., `approve`, `reject`, `request-more-info`)

### Standardizing common transitions

Use shared transitions for common actions like:

- `cancel`
- `approve/reject`
- `timeout`

This improves consistency across workflows and reduces duplication.

## Where to go deeper (developer reference)

If you need full schema and examples, see the archived developer docs:

- `docs/technical/flow/flow.md`
- `docs/technical/flow/task.md`
- `docs/technical/flow/mapping.md`

