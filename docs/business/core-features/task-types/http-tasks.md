# HTTP Tasks

## At a glance

- **What it is**: A standardized way to call external systems via HTTP as part of workflow execution.
- **Business value**: Integrate with existing enterprise systems (CRM/ERP/core systems) without embedding integration logic in every service.
- **Where it fits**: REST integrations, webhooks, partner APIs, internal platform APIs.

## Typical business scenarios

- Fetch customer profile or eligibility data
- Initiate payments or downstream processing
- Call KYC/AML providers
- Trigger notifications via third-party services

## Operational considerations

- Treat HTTP calls as **integration points** that can fail; design for retries, timeouts, and fallbacks.
- Keep payloads aligned with your data governance and PII rules.
- Avoid placing business policy logic inside the integration call—use scripts for policy and tasks for connectivity.

## Minimal example (conceptual)

An HTTP task generally defines:

- **method** (GET/POST/etc.)
- **url**
- **headers/body** (if needed)
- **timeout**

For complete, working examples with mappings and real endpoints, refer to [vnext-example](https://github.com/burgan-tech/vnext-example).

## Where to go deeper (developer reference)

- `docs/technical/flow/tasks/http-task.md`

