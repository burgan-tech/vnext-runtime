# Human Tasks

## At a glance

- **What it is**: Human-in-the-loop steps (review, approval, exception handling) represented in the workflow model.
- **Business value**: Make decision points auditable, measurable (SLA), and visible in queues/dashboards.
- **Where it fits**: Approvals, manual verification, exception handling, compliance reviews.

## Two common ways to model human work

### 1) Manual transitions (most common)

A workflow reaches a state like “human-review”, and an authorized user triggers a **manual transition** (approve/reject/request-info).

Benefits:

- explicit decision points
- clear audit trail and timestamps
- easy to map to UI actions

### 2) “Human-needed” state classification (for queues)

Where supported, states can be classified as “human interaction required” so operational queues can filter for them.

Benefits:

- build “work baskets” (all items waiting for review)
- monitor load and bottlenecks

## Operational considerations

- Define SLAs and escalation (often paired with timer execution).
- Ensure authorization and role checks are aligned with policy.
- Capture “decision rationale” as attributes for audit and reporting.

## Where to go deeper (developer reference)

- `docs/technical/flow/transition.md` (manual transitions)
- `docs/technical/flow/state.md` (state subType and effective state concepts)

