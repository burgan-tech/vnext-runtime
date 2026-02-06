# State Machine with Conditional Transitions

## At a glance

- **Audience**: Developers and analysts who need to reason about process behavior, routing rules, and lifecycle.
- **Goal**: Explain state lifecycle, transition rules, and how conditional (automatic) transitions work.
- **Key idea**: The workflow advances only via **transitions**, and transitions can be **manual**, **automatic**, **scheduled**, or **event-based**.

## State model (how stages are represented)

A workflow instance is always “in” a state that represents its current stage.

### State types

- **Initial**: entry state (only one per flow)
- **Intermediate**: where most business work happens
- **Finish**: terminal states (instance becomes completed)
- **SubFlow**: a state that runs a sub-workflow

### State subtypes (outcome + operational classification)

SubTypes help classify states—especially finish outcomes and operational tracking (e.g., success vs error, human-needed vs busy).

This is useful for:

- reporting (“how many completed successfully?”)
- queues (“which instances need human action?”)
- SLA monitoring (“how long do we stay in ‘Human’ subtype?”)

## Transition model (how the flow moves)

Transitions connect states and define when and how a workflow can progress.

### Trigger types (who/what can execute them)

- **Manual (0)**: triggered by the client/user action (e.g., approve button)
- **Automatic (1)**: system-executed when a rule evaluates to true (business rules)
- **Scheduled (2)**: system-executed based on time (cron/duration; timeouts, reminders)
- **Event (3)**: triggered by events (pub/sub, external system signals)

## State lifecycle (execution order)

When a transition runs, the engine follows a predictable pipeline:

1. **Policy checks** (what is allowed from the current state; what the client may trigger)
2. **Transition `onExecutionTasks`** run
3. **State `onExits`** run
4. **State change** to the target state
5. **State `onEntries`** run
6. **State type check** (finish/subflow behavior)
7. **Automatic transitions** are evaluated/executed
8. **Scheduled transitions** are evaluated/executed

This order matters because it defines where integrations and side effects should live:

- Transition tasks are best for “before moving” validations/mappings
- OnExit/OnEntry tasks are best for “leaving/entering” side effects

## Minimum examples (common transition patterns)

These patterns are intentionally small; they show how routing is expressed without embedding low-level implementation details (like base64-encoded scripts).

### Manual transition (user-driven)

```json
{ "key": "approve", "target": "approved", "triggerType": 0, "versionStrategy": "Minor" }
```

Use when a person (or external channel) makes a decision: approvals, confirmations, “submit”, “cancel”.

### Automatic transition (rule-driven)

```json
{
  "key": "auto-approve",
  "target": "approved",
  "triggerType": 1,
  "versionStrategy": "Minor",
  "rule": { "ref": "Mappings/auto-approval-rule.csx" }
}
```

Use when routing should happen based on business rules (thresholds, eligibility checks). These are executed by the system, not by the client.

### Scheduled transition (time-driven)

```json
{
  "key": "timeout",
  "target": "expired",
  "triggerType": 2,
  "versionStrategy": "Minor",
  "timer": { "ref": "Mappings/timeout-schedule.csx" }
}
```

Use for reminders, SLAs, timeouts, periodic checks.

### Event transition (signal-driven)

```json
{ "key": "payment-received", "target": "settlement", "triggerType": 3, "versionStrategy": "Minor" }
```

Use when workflow progress is driven by events from other services/systems (pub/sub).

## Conditional routing (automatic transitions)

Automatic transitions are a practical way to model business rules without user interaction, for example:

- If amount < threshold → auto-approve
- If fraud suspicion → route to risk review
- If missing documents → request additional info

Key operational expectations:

- Automatic transitions are executed by the system (not by clients).
- Multiple auto transitions can exist; the platform can support a “default” auto transition pattern if none match (see archived reference).

## Where to go deeper (developer reference)

- `docs/technical/flow/state.md` (state types, lifecycle steps)
- `docs/technical/flow/transition.md` (trigger types, rule/timer mappings, validation)
- `docs/technical/flow/flow.md` (how these are composed in a workflow definition)

