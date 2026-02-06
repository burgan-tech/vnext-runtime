# Real-time and Asynchronous Execution

## At a glance

- **Audience**: Developers and analysts deciding execution patterns and user experience expectations.
- **Goal**: Explain when to use synchronous (real-time) execution vs asynchronous execution and how it relates to transitions, timers, and events.
- **Key idea**: Use **sync** when users need immediate confirmation; use **async** for long-running, event-driven, scheduled, or resilient operations.

## Real-time (synchronous) execution

vNext endpoints support a `sync=true` query parameter for operations like starting an instance and triggering a transition.

Business perspective:

- Suitable for interactive channels (customer/employee UI) where the user expects an immediate outcome.
- Useful when the step is fast and deterministic (e.g., lightweight validation and routing).

Operational considerations:

- Keep synchronous steps bounded in time.
- Avoid heavy integrations in the “critical path” when possible; prefer async continuation.

## Asynchronous execution

Asynchronous execution is essential for workflows that include:

- external dependencies (third-party systems),
- long-running checks (verification, settlement),
- scheduled actions (reminders, timeouts),
- event-driven progress (pub/sub events).

Platform building blocks that naturally support async:

- **Scheduled transitions (triggerType=2)** for timers and timeouts
- **Event transitions (triggerType=3)** for event-driven orchestration
- **Automatic transitions (triggerType=1)** for rule-based progression (system-driven)

## Choosing sync vs async (practical guidance)

Use **sync** when:

- a UI must confirm a decision immediately (approve/reject, capture inputs),
- the operation does not wait on external systems,
- you want a single request/response interaction.

Use **async** when:

- you may need retries and resilience against partial failures,
- you rely on eventual consistency or callbacks/events,
- processing can outlive a user session.

## Idempotency and business keys (reliability in integration)

When integrating external systems, idempotency matters:

- Starts can be retried safely when using keys (platform supports idempotent start behavior in recent versions).
- Using business keys (order numbers, customer IDs) improves traceability and reduces UUID plumbing.

## Where to go deeper (developer reference)

- `docs/technical/how-to/start-instance.md` (start/transition endpoints, sync parameter, key behavior)
- `docs/technical/flow/transition.md` (manual/automatic/scheduled/event trigger types)

