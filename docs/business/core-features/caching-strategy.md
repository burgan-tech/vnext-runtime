# Caching Strategy (Performance and Consistency)

## At a glance

- **What it is**: A set of caching patterns that reduce latency and prevent repeated reads of stable definitions and frequently accessed data.
- **Business value**: Faster user experiences, lower infrastructure load, and more predictable performance under peak demand.
- **Where it fits**: Reading definitions (views), frequent instance-data reads, and operational synchronization patterns.

## Why it matters

Workflow platforms tend to read the same artifacts repeatedly:

- definitions (flows, views, tasks),
- the “current state” of active instances,
- and data snapshots that power UI and integrations.

Caching reduces repeated work while preserving correctness through invalidation and versioning patterns.

## What is cached (examples)

### View definitions (definition cache)

View definitions can be cached using a predictable key pattern:

- `View:{Domain}:{Flow}:{Key}:{Version}`

This reduces database load and speeds up UI rendering.

### Data reads with ETag (client-friendly caching)

When clients query instance-related data, ETag-based caching allows:

- returning **304 Not Modified** when nothing changed,
- avoiding repeated payload transfer and processing.

## Keeping caches correct (invalidation and refresh)

Caching only works if freshness is managed. In vNext Runtime, the ecosystem includes patterns such as:

- **re-initialize after component deployment** to clear stale definition caches,
- **invalidate-cache** operations/events to refresh cached artifacts after updates.

This reduces “why am I seeing the old definition?” problems after deploys.

## Distributed cache building blocks

The runtime environment includes distributed state/cache components (commonly backed by Redis via Dapr state API). This supports:

- consistent behavior across horizontally scaled service instances,
- faster access to frequently needed state/metadata.

## Where to go deeper (developer reference)

- `docs/technical/flow/view.md` (view caching pattern and key shape)
- `docs/technical/flow/function.md` (ETag caching behavior)
- `docs/technical/services/init-service.md` (cache clearing after deploy / re-initialize)

