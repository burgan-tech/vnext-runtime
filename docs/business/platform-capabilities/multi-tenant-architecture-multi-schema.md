# Multi-tenant Architecture (Multi-Schema)

## At a glance

- **Audience**: Developers and analysts designing domain isolation, data governance, and operational boundaries.
- **Goal**: Explain domain-level isolation and the multi-schema approach used to organize system vs flow data.
- **Key idea**: vNext isolates **domains** at the database level, and isolates **flows** inside a domain via **schemas**.

## Domain isolation (tenant boundary)

In vNext Runtime, each **domain** has its own independent database.

Business outcomes of this design:

- **Strong isolation** between domains/tenants (security, compliance)
- **Independent scaling and governance** per domain
- **Safer change management** (domain-specific data policies and lifecycle)

Data sharing between domains is expected to occur via:

- APIs, or
- events (pub/sub),

not by direct database access.

## Multi-schema inside a domain database (flow boundary)

Within each domain database, vNext uses a multi-schema strategy to separate:

1) **System metadata** (platform-managed), and  
2) **Flow-specific operational data** (created as flows are executed).

### System schemas (platform-managed)

When the platform starts, core system schemas are created (examples):

- `sys_flows` (workflow definitions and version information)
- `sys_tasks` (task definitions)
- `sys_functions` (function APIs)
- `sys_views` (view definitions)
- `sys_extensions` (extensions/plugins)
- `sys_schemas` (schema registry and migration history)

### Flow schemas (created on demand)

When a flow is deployed and run for the first time, the platform creates a schema for that flow (example: `customer_onboarding`) and applies migrations automatically.

This yields practical benefits:

- flow-specific data and history stay grouped
- migrations can be tracked and applied per schema
- operational queries and cleanup can be scoped per flow

## Multi-schema rules & mental model (for analysts and developers)

When deciding how to structure a solution:

- **Domain** answers: “Which business area owns this data and lifecycle?”
- **Flow** answers: “Which process needs its own instance history and operational footprint?”

Good heuristics:

- Put different compliance boundaries into separate **domains** (separate databases).
- Keep different process lifecycles as different **flows** (separate flow schemas).

## Automatic migration (why it matters)

vNext Runtime manages schema changes automatically:

- On startup it checks migration history
- Applies missing migrations
- Updates migration tracking per schema

This supports controlled evolution across multiple flows without manual DB scripting for each deployment.

## Where to go deeper (developer reference)

- `docs/technical/fundamentals/database-architecture.md`
- `docs/technical/fundamentals/domain-topology.md`

