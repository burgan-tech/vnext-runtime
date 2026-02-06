# Service Discovery (Domain Registration and Microservice Invocation)

## At a glance

- **What it is**: Capabilities that reduce manual wiring between services by enabling discovery/registration patterns at runtime.
- **Business value**: Faster integration, fewer environment-specific configuration errors, and improved resiliency in distributed systems.
- **Where it fits**: Multi-domain setups, microservice invocation, and standardized connectivity across environments.

## Two related “discovery” concerns

In practice, service discovery shows up in two complementary ways:

1) **Domain/service registration** (platform-level): ensuring a domain runtime can register itself to a discovery endpoint for the environment.

2) **Microservice discovery for calls** (execution-level): enabling services to call each other using stable identifiers rather than hard-coded URLs.

## Platform-level domain registration (high level)

In environments where Service Discovery is enabled, the runtime may enforce successful registration at startup to avoid running in a partially discoverable state.

Business implications:

- **safer operations** (fail-fast when discovery is misconfigured)
- **predictable routing** in multi-domain scenarios
- clearer environment governance (dev/test/prod registration endpoints)

## Microservice invocation discovery (Dapr Service Task)

For service-to-service calls, vNext supports Dapr service invocation patterns where:

- callers use an `appId` (stable service identifier),
- the platform handles discovery, load balancing, resiliency, and tracing behaviors provided by the runtime.

### Minimal example (readable service call shape)

```json
{
  "attributes": {
    "type": "3",
    "config": {
      "appId": "user-service",
      "methodName": "users/123",
      "httpVerb": "GET",
      "timeoutSeconds": 30
    }
  }
}
```

For complete integration examples, reference [vnext-example](https://github.com/burgan-tech/vnext-example).

## Where to go deeper (developer reference)

- `docs/technical/services/init-service.md` (service discovery endpoint and startup behavior)
- `docs/technical/flow/tasks/dapr-service.md` (Dapr service task and `appId` usage)
- `docs/technical/fundamentals/domain-topology.md` (multi-domain architecture and inter-domain patterns)

