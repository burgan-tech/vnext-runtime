# vNext Runtime Platform - Release Notes v0.0.33
**Release Date:** January 26, 2026

## Overview
This release introduces multi-domain support for local development environments, upgrades to .NET 10 LTS, and enhances state tracking with HumanTask support. Additionally, important improvements to Service Discovery validation, TriggerTask SSL configuration, and instance filtering capabilities have been implemented.

---

## New Features

### 1. Multi-Domain Support in vNext Runtime
Added comprehensive multi-domain support for local development environments, allowing teams to run isolated domain environments simultaneously.

**Details:**
- Run multiple domains (e.g., `core`, `sales`, `hr`) on the same infrastructure
- Shared PostgreSQL, Redis, Vault, and Dapr services
- Domain-specific port allocation with configurable offsets
- Template-based domain creation from `vnext/docker/templates/`
- Automatic database name generation (e.g., `core` → `vNext_Core`)

**Features:**
- Pre-configured `core` (offset 0) and `discovery` (offset 5) domains included
- Create custom domains with `make create-domain DOMAIN=mydom PORT_OFFSET=10`
- Manage individual domains independently
- View all running domains with `make status-all-domains`
- Domain-specific environment files and configurations
- Reserved offsets (0 and 5) prevent conflicts with system domains

**Example Workflow:**
```bash
# Start shared infrastructure
make up-infra

# Pre-configured domains (core and discovery) are ready to use
make up-vnext DOMAIN=core
make up-vnext DOMAIN=discovery

# Create and start your custom domain (use offset 10 or higher)
make create-domain DOMAIN=sales PORT_OFFSET=10
make db-create DOMAIN=sales
make up-vnext DOMAIN=sales
```

**Port Allocation:**

| Offset | Domain (Example) | App Port | Execution | Inbox | Outbox | Init |
|--------|------------------|----------|-----------|-------|--------|------|
| 0      | core (reserved)  | 4201     | 4202      | 4203  | 4204   | 3005 |
| 5      | discovery (reserved) | 4206 | 4207      | 4208  | 4209   | 3010 |
| 10     | sales            | 4211     | 4212      | 4213  | 4214   | 3015 |
| 20     | hr               | 4221     | 4222      | 4223  | 4224   | 3025 |
| 30     | finance          | 4231     | 4232      | 4233  | 4234   | 3035 |

> **⚠️ Reserved Offsets:** The `core` and `discovery` domains are provided as pre-configured domains with offsets **0** and **5** respectively. **Do not use these offsets** for your custom domains. Start with offset 10 or higher for new domains.

> **Note:** For complete documentation, see the Multi-Domain Support section in README.md

---

## Enhancements

### 1. Upgrade to .NET 10 (LTS)
Upgraded the entire platform to .NET 10 LTS for improved performance, security, and access to latest framework features.

**Benefits:**
- Enhanced performance and memory management
- Latest language features and APIs
- Long-term support for production deployments
- Improved diagnostics and tooling

> **Reference:** [#168 - Upgrade to .NET 10 (LTS)](https://github.com/burgan-tech/vnext/issues/168)

---

### 2. HumanTask State Tracking for Pending Instance Queries
Implemented comprehensive state tracking for human task workflows with new state subtypes and instance-level fields.

**Details:**
- New State SubTypes: `Busy` (code: 5) and `Human` (code: 6)
- New Instance fields: `EffectiveStateType` and `EffectiveStateSubType`
- Enhanced filtering capabilities for pending instance queries
- Automatic database migration on startup

**State SubType Mapping:**

| SubType | Code | Description |
|---------|------|-------------|
| None | 0 | No specific subtype |
| Success | 1 | Successful completion |
| Error | 2 | Error condition |
| Terminated | 3 | Manually terminated |
| Suspended | 4 | Temporarily suspended |
| **Busy** | **5** | Processing in progress (NEW) |
| **Human** | **6** | Human interaction required (NEW) |

**Instance Fields:**
- `EffectiveStateType`: Current effective state type
- `EffectiveStateSubType`: Current effective state subtype

**Use Cases:**
- Query all instances awaiting human interaction
- Filter workflows in processing state
- Track human task queues across workflows
- Generate workload reports by state type

**Filter Example:**
```http
GET /core/workflows/approval-flow/instances?filter={"effectiveStateSubType":{"eq":"6"}}
```

> **Migration:** The database migration for new columns is applied automatically on startup.

> **Reference:** [#328 - Add HumanTask state tracking for pending instance queries](https://github.com/burgan-tech/vnext/issues/328)

---

### 3. Persist InputHandler Result to InstanceTask.Request
InputHandler results are now persisted to the `InstanceTask.Request` field for complete task audit trail.

**Details:**
- Input handler transformation results are stored
- Enables full task execution audit trail
- Supports debugging and compliance requirements
- Request data available for historical analysis

**Benefits:**
- Complete visibility into task input data
- Track data transformations across task execution
- Support for compliance and audit requirements
- Enhanced debugging capabilities

> **Reference:** [#309 - Persist InputHandler result to InstanceTask.Request](https://github.com/burgan-tech/vnext/issues/309)

---

## Bug Fixes

### 1. Cannot Select $self as Target Value in Cancel Transitions
Fixed issue where `$self` could not be used as a target in cancel transitions.

**Problem:**
- Cancel transitions couldn't use `$self` as target
- Schema validation prevented valid use cases

**Solution:**
- Added `$self` target support in vnext-schema for cancel transitions
- Enables workflows to cancel and remain in current state

> **Reference:** [#307 - Cannot select $self as target value in cancel transitions](https://github.com/burgan-tech/vnext/issues/307)

---

### 2. EffectiveState Display Issue from Nested Subflows
Fixed EffectiveState calculation error when displaying state from deeply nested subflow structures.

**Problem:**
- EffectiveState showed incorrect values for nested subflows
- Bottom-up state propagation had calculation errors

**Solution:**
- Corrected bottom-up effective state display algorithm
- Ensures accurate state representation across nested workflows

> **Reference:** [#316 - EffectiveState aktif son state gösterme sorunu](https://github.com/burgan-tech/vnext/issues/316)

---

### 3. Align TriggerInvokers HttpClient Configuration with HttpTaskInvoker
Centralized and standardized HttpClient Factory configuration across all TriggerTask types.

**Details:**
- Unified HttpClient Factory behavior for StartTask, DirectTriggerTask, GetInstanceDataTask, and SubProcessTask
- Added `validateSsl` attribute to TriggerTask configuration
- Consistent SSL validation across all task types

**New Configuration:**
```json
{
  "type": "11",
  "config": {
    "domain": "core",
    "flow": "my-flow",
    "validateSsl": true
  }
}
```

**validateSsl Attribute:**

| Value | Behavior |
|-------|----------|
| `true` (default) | SSL certificate validation enabled |
| `false` | SSL certificate validation disabled |

> **Warning:** Disable SSL validation only in development or trusted internal environments.

> **Reference:** [#320 - Align TriggerInvokers HttpClient Configuration with HttpTaskInvoker](https://github.com/burgan-tech/vnext/issues/320)

---

## Hotfixes

### 1. NotificationMapping Does Not Handle Subflow Data Returned from State Endpoint
Fixed notification mapping to correctly handle subflow data from state endpoints.

**Problem:**
- NotificationMapping didn't align with current state behavior for subflow data
- Subflow state information missing in notifications

**Solution:**
- Updated notification mapping to match current state behavior
- Ensures consistent subflow data handling across all endpoints

> **Reference:** [#283 - NotificationMapping.csx does not handle subflow data returned from state endpoint](https://github.com/burgan-tech/vnext/issues/283)

---

### 2. Fail Fast When vNextApi:BaseUrl Points to Localhost in Production
Implemented validation to prevent localhost addresses in production Service Discovery configuration.

**Details:**
- Application validates `vNextApi:BaseUrl` setting on startup
- Localhost addresses (`localhost`, `127.0.0.1`) are blocked in production environments
- Fail-fast behavior prevents deployment with incorrect configuration

**Validation Rules:**
- Development environment: Localhost allowed
- Production environment: Localhost blocked and application fails to start

**Error Example:**
```
FATAL: Service Discovery configuration error - 
vNextApi:BaseUrl cannot point to localhost in production environment.
Current value: http://localhost:4201
```

**Configuration:**
```json
{
  "vNextApi": {
    "BaseUrl": "https://api.production.com"  // Must be resolvable address
  }
}
```

> **Reference:** [#313 - Fail fast when vNextApi:BaseUrl points to localhost in production](https://github.com/burgan-tech/vnext/issues/313)

---

### 3. Service Discovery Registration Failure Crash
Application now crashes immediately when Service Discovery is enabled but registration fails.

**Details:**
- Fail-fast behavior when Service Discovery registration fails
- Prevents silent failures in service mesh environments
- Ensures deployment issues are detected immediately

**Behavior:**
- Service Discovery enabled + Registration successful: Normal operation
- Service Discovery enabled + Registration failed: Application crash
- Service Discovery disabled: No validation

**Error Handling:**
```
FATAL: Service Discovery registration failed.
Cannot proceed without successful service registration.
Check network connectivity and Service Discovery endpoint configuration.
```

> **Note:** This ensures that microservice deployments with Service Discovery don't run in a partially configured state.

> **Reference:** [#325 - Refactor: Move service discovery enable check into RegisterDomainAsync and add failure handling](https://github.com/burgan-tech/vnext/issues/325)

---

### 4. Fail-Fast Extension Execution with Error Target Details
Extension tasks now fail immediately with detailed error information when errors occur.

**Details:**
- Extension errors terminate request immediately
- Error target details logged for troubleshooting
- Detailed error response returned to client

**Error Response:**
```json
{
  "type": "https://httpstatuses.com/500/extension-error",
  "title": "Extension Execution Failed",
  "status": 500,
  "detail": "Extension 'validate-customer' failed during execution",
  "errorTarget": "CustomerValidationExtension",
  "targetEndpoint": "https://extensions.api.com/validate",
  "traceId": "00-abc123-def456-01"
}
```

**Benefits:**
- Fast failure detection
- Detailed error context for debugging
- Prevents cascade failures

> **Reference:** [#306 - Fail-fast extension execution with error target details](https://github.com/burgan-tech/vnext/issues/306)

---

### 5. Add EffectiveState Column and Update Alias Handling
Enhanced instance filtering with EffectiveState field support.

**Details:**
- EffectiveState information added to instance filtering queries
- Improved alias handling for state-related fields
- Better query performance for state-based searches

**Filterable Fields:**
- `effectiveState`: Current effective state
- `effectiveStateType`: Current effective state type
- `effectiveStateSubType`: Current effective state subtype

> **Reference:** [#317 - Add EffectiveState column and update alias handling](https://github.com/burgan-tech/vnext/pull/317)

---

## Other Changes

### 1. Documentation Reorganization
Reorganized and improved vNext technical documentation structure for better discoverability and maintainability.

**Improvements:**
- Enhanced documentation structure
- Improved navigation and organization
- Better categorization of topics
- Updated examples and guides

> **Reference:** [#310 - Reorganize documentation structure for improved discoverability and maintainability](https://github.com/burgan-tech/vnext/issues/310)

---

## Configuration Updates

Configuration for v0.0.33:
```json
{
  "runtimeVersion": "0.0.32",
  "schemaVersion": "0.0.33",
  "componentVersion": "0.0.18"
}
```

---

## Issues Referenced

**New Features:**
- [#168 - Upgrade to .NET 10 (LTS)](https://github.com/burgan-tech/vnext/issues/168)
- vnext-runtime multi-domain support (README.md documentation)

**Enhancements:**
- [#309 - Persist InputHandler result to InstanceTask.Request](https://github.com/burgan-tech/vnext/issues/309)
- [#328 - Add HumanTask state tracking for pending instance queries](https://github.com/burgan-tech/vnext/issues/328)

**Bug Fixes:**
- [#307 - Cannot select $self as target value in cancel transitions](https://github.com/burgan-tech/vnext/issues/307)
- [#316 - EffectiveState aktif son state gösterme sorunu](https://github.com/burgan-tech/vnext/issues/316)
- [#320 - Align TriggerInvokers HttpClient Configuration with HttpTaskInvoker](https://github.com/burgan-tech/vnext/issues/320)

**Hotfixes:**
- [#283 - NotificationMapping.csx does not handle subflow data returned from state endpoint](https://github.com/burgan-tech/vnext/issues/283)
- [#306 - Fail-fast extension execution with error target details](https://github.com/burgan-tech/vnext/issues/306)
- [#313 - Fail fast when vNextApi:BaseUrl points to localhost in production](https://github.com/burgan-tech/vnext/issues/313)
- [#317 - Add EffectiveState column and update alias handling](https://github.com/burgan-tech/vnext/pull/317)
- [#325 - Refactor: Move service discovery enable check into RegisterDomainAsync and add failure handling](https://github.com/burgan-tech/vnext/issues/325)

**Other:**
- [#310 - Reorganize documentation structure for improved discoverability and maintainability](https://github.com/burgan-tech/vnext/issues/310)

---

## Summary

With this release:
- Multi-domain support for local development environments added
- .NET 10 LTS upgrade completed
- HumanTask state tracking with EffectiveStateType and EffectiveStateSubType fields implemented
- InputHandler results now persisted to InstanceTask.Request
- Cancel transitions support $self target
- EffectiveState display issue for nested subflows fixed
- TriggerTask types now support validateSsl attribute
- NotificationMapping aligned with current state behavior
- Service Discovery localhost validation in production implemented
- Service Discovery registration failure now causes immediate crash
- Extension tasks fail-fast with detailed error information
- EffectiveState column added to instance filtering
- Documentation structure reorganized

---

## Upgrade Path

### From v0.0.31 to v0.0.33:

1. **Update Runtime:**
   ```bash
   git pull origin master
   ```

2. **Update Configuration:**
   ```json
   {
     "runtimeVersion": "0.0.32",
     "schemaVersion": "0.0.33",
     "componentVersion": "0.0.18"
   }
   ```

3. **Database Migration:**
   The `EffectiveStateType` and `EffectiveStateSubType` field migrations are applied automatically on startup.

4. **Review Service Discovery Configuration:**
   If using Service Discovery, ensure `vNextApi:BaseUrl` does not point to localhost in production environments.

5. **Review TriggerTask Configurations:**
   If you have custom TriggerTask configurations, you can now optionally add the `validateSsl` attribute (defaults to `true`).

6. **(Optional) Explore Multi-Domain Support:**
   For local development, explore the new multi-domain capabilities:
   ```bash
   # Start infrastructure and pre-configured domains
   make up-infra
   make up-vnext DOMAIN=core
   make up-vnext DOMAIN=discovery
   
   # Create custom domains (use offset 10 or higher)
   make create-domain DOMAIN=mydom PORT_OFFSET=10
   make db-create DOMAIN=mydom
   make up-vnext DOMAIN=mydom
   ```
   
   > **Note:** Offsets 0 and 5 are reserved for `core` and `discovery` domains.

---

**vNext Runtime Platform Team**  
January 26, 2026
