# Instance Filtering (Advanced Querying)

## At a glance

- **What it is**: Advanced filtering to query workflow instances by status, state, timestamps, and business attributes.
- **Business value**: Power dashboards, operational queues, and SLA reporting without building bespoke query endpoints per use case.
- **Where it fits**: “show me all stuck cases”, “all items waiting for approval”, “high-value active payments”.

## Why it matters

Operational excellence depends on answering questions quickly:

- Where is work accumulating?
- Which cases need human attention?
- Which integrations are failing and how often?
- How do we slice the backlog by product/customer/tier?

Instance filtering enables these answers using a consistent query language.

## What you can filter (conceptually)

- **Instance metadata**: key, flow name, status, created/modified/completed times
- **Current/effective state**: useful for nested flows and “what’s really blocking progress”
- **Business attributes**: values stored on the instance (customerId, amount, priority, etc.)

## Minimal examples (business-focused)

### Find active instances above a value threshold

```http
GET /banking/workflows/payment-workflow/instances?filter={"status":{"eq":"Active"},"attributes":{"amount":{"gt":"5000"}}}
```

### Find items waiting for human interaction

```http
GET /approvals/workflows/approval-flow/instances?filter={"effectiveStateSubType":{"eq":"6"}}
```

These examples are intended to show the *shape* of the query; for the full operator list and formats, use the developer reference below.

## Where to go deeper (developer reference)

- `docs/technical/flow/instance-filtering.md` (filter formats, operators, effective state filters)
- `docs/technical/flow/function.md` (ETag and data function behavior)

