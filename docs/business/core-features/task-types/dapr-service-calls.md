# Dapr Service Calls

## At a glance

- **What it is**: Calling microservices using Dapr service invocation patterns (service-to-service calls).
- **Business value**: Faster integration in microservice ecosystems with standardized connectivity, resiliency, and observability.
- **Where it fits**: Domain services, shared capabilities, and internal platform services.

## Why it matters

In distributed systems, hard-coding service URLs and handling failures inconsistently creates operational risk. Dapr-style service invocation supports:

- stable service identifiers (reducing environment-specific rewiring),
- built-in resiliency patterns (timeouts/retries/circuit breaking),
- better cross-service tracing.

## Typical business scenarios

- Call an internal “customer” service for profile and status
- Invoke an “limits” service to evaluate threshold policies
- Call “notifications” to enqueue a message (when using service invocation rather than pub/sub)

## Minimal example (conceptual)

Most service calls can be described as:

- target **service identifier** (appId)
- method/endpoint
- request payload

For complete implementation examples, refer to [vnext-example](https://github.com/burgan-tech/vnext-example).

## Where to go deeper (developer reference)

- `docs/technical/flow/tasks/dapr-service.md`
- `docs/technical/services/init-service.md` (service discovery/registration behavior in some environments)

