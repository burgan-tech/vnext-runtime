# Timer Execution (Scheduled and Delayed Work)

## At a glance

- **What it is**: Time-based execution that enables delayed, scheduled, or periodic workflow behavior.
- **Business value**: Model SLAs, reminders, timeouts, and periodic checks in a governed process.
- **Where it fits**: “wait 24h”, “retry tomorrow”, “remind every week”, “expire after deadline”.

## Why it matters

Time is a first-class dimension of real processes:

- customers don’t respond instantly,
- external systems settle later,
- approvals have deadlines,
- and compliance often mandates timed actions and expirations.

Timer execution makes these behaviors explicit and auditable.

## What it enables (common scenarios)

- **SLA timeouts**: escalate if no action within a window
- **Reminders**: notify customer/employee at scheduled intervals
- **Delayed retries**: postpone retry after transient failures
- **Periodic checks**: re-validate status until completion (within limits)

## How it works (high level)

Two related building blocks are typically involved:

- **Scheduled transitions** (time-driven movement between states)
- **Timer tasks/mappings** (how the schedule is expressed)

Timer behavior is executed by the system (not manually by clients), which supports consistent timing and operational control.

## Minimal example (scheduled transition shape)

```json
{
  "key": "timeout",
  "target": "expired",
  "triggerType": 2,
  "versionStrategy": "Minor",
  "timer": { "ref": "Mappings/timeout-schedule.csx" }
}
```

For end-to-end examples (including timer mapping scripts), reference [vnext-example](https://github.com/burgan-tech/vnext-example).

## Where to go deeper (developer reference)

- `docs/technical/flow/transition.md` (scheduled trigger type and timer mappings)
- `docs/technical/flow/tasks/timer-task.md` (system timer task behavior and constraints)

