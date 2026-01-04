# vNext Runtime Platform - Release Notes v0.0.29
**Release Date:** January 4, 2026

## 🧭 Overview
This release introduces significant new features including Service Discovery for cross-domain communication, Default Auto Transition support, "latest" version resolution, and a new Transition Schema API endpoint. Additionally, important bug fixes for subflow extensions, sequential task initialization, and publish component timeouts have been implemented.

> ⚠️ **New Feature Alert:** Service Discovery requires additional setup. See the Service Discovery section below for configuration details.

---

## 🚀 New Features

### 1. Service Discovery for Cross-Domain Communication
Implemented service discovery to enable communication between different domains.

**Details:**
- Domains can now discover and communicate with services in other domains
- Requires `vnext-domain-discovery` runtime installation (one per environment recommended)
- Core domain structure for service registration and discovery

**Configuration:**
Add the following to your `appsettings.json`:
```json
{
  "ServiceDiscovery": {
    "Enabled": false,
    "BaseUrl": "http://localhost:3001/api/v1",
    "ValidateSsl": false,
    "Domain": "discovery",
    "RegistryFlow": "domain-registration",
    "TimeoutSeconds": 30,
    "MaxRetryAttempts": 3,
    "RetryDelayMilliseconds": 1000,
    "CircuitBreakerFailureThreshold": 5,
    "CircuitBreakerTimeoutSeconds": 30,
    "DiscoveryCacheSeconds": 300,
    "DiscoveryEndpointTemplate": "/discovery/workflows/domain/instances/{0}/functions/data"
  }
}
```

> ⚠️ **Note:** Keep Service Discovery disabled in development environments. Enable it only in production environments.

> **Reference:** [#188 - Use Service Discovery Domain to Resolve URLs for Other Domains](https://github.com/burgan-tech/vnext/issues/188)  
> **Repository:** [vnext-domain-discovery](https://github.com/burgan-tech/vnext-domain-discovery)

---

### 2. Default Auto Transition Support
Added support for default transitions when no auto transition conditions are met.

**Details:**
- When no auto transition condition is satisfied, a default transition can now be triggered
- Uses `triggerKind: 10` for default auto transition

**Configuration:**
```json
{
  "triggerKind": 10
}
```

**TriggerKind Values:**
| Value | Description |
|-------|-------------|
| 0 | Not applicable |
| 10 | Default Auto Transition |

> **Reference:** [#199 - Implement Default Auto Transition for Failed Auto Transition Conditions](https://github.com/burgan-tech/vnext/issues/199)

---

### 3. "Latest" Version Resolution in References
Version references now support "latest" string value for automatic latest version resolution.

**Details:**
- Use `"version": "latest"` to automatically resolve to the newest version
- Major and Minor version references also resolve to the latest matching version

**Examples:**
```json
{
  "key": "component-123",
  "version": "latest"
}
```

```json
{
  "key": "component-123",
  "version": "1.1"
}
```

> **Reference:** [#209 - Support "latest" String Value in References for Latest Version Resolution](https://github.com/burgan-tech/vnext/issues/209)

---

### 4. Transition Schema API Endpoint
Added a new API endpoint to fetch transition schema content.

**Details:**
- New function definition to retrieve schema content defined on transitions
- Returns schema content if a transition schema definition exists

**Endpoint:**
```http
GET /api/v1/{domain}/workflows/{workflow}/instances/{instance}/functions/schema?transitionKey={transition}
```

> **Reference:** [#216 - Add API Endpoint to Fetch Transition Schema](https://github.com/burgan-tech/vnext/issues/216)

---

### 5. Idempotent Workflow Start
Implemented idempotent behavior for workflow start requests with the same key.

**Details:**
- When starting a workflow with a key that already exists, the system now returns the existing instance information instead of an error
- Previous behavior: Returns `409 Conflict` (InstanceAlreadyExists) error
- New behavior: Returns the existing instance's current status and ID

**Previous Response (409 Error):**
```json
{
  "error": "InstanceAlreadyExists",
  "message": "An instance with key 'ORDER-123' already exists"
}
```

**New Response (Idempotent):**
```json
{
  "id": "18075ad5-e5b2-4437-b884-21d733339113",
  "status": "A"
}
```

**Benefits:**
- Safe retry scenarios for network failures
- Clients can retrieve original start response on repeated calls
- No need for separate "check if exists" calls before starting

> **Reference:** [#200 - Implement Idempotent Workflow Start - Return Same Response for Repeated Calls with Same Key](https://github.com/burgan-tech/vnext/issues/200)

---

## 🛠️ Bug Fixes

### 1. Subflow Extension Invocation Bug
Fixed an issue where extension calls within subflows were not returning correctly.

**Problem:**
- When a main flow transitioned to a subflow, the data endpoint only returned main flow data
- Extensions from subflows were not included in the response

**Solution:**
- Data endpoint now returns main flow data and appends subflow extensions to its response

> **Reference:** [#250 - Subflow içinde extension çağırımı](https://github.com/burgan-tech/vnext/issues/250)

---

### 2. Logger Initialization in Sequential Tasks
Fixed initialization errors occurring during sequential task execution.

**Problem:**
- Logger, Dapr, or Configuration initialization errors occurred during sequential task execution
- "Logger is not initialized" error in script output handler

**Solution:**
- Logger, Dapr, and Configuration initialization is now performed separately for each task in sequential execution

> **Reference:** [#265 - Script output handler failed – Logger is not initialized](https://github.com/burgan-tech/vnext/issues/265)

---

### 3. Extension Response Mapping Bug
Fixed response mapping issues in Extension HTTP task requests.

**Problem:**
- Response mapping in HTTP task requests within Extensions was not working correctly

**Solution:**
- Corrected the response mapping logic for HTTP task requests in Extensions

> **Reference:** [#239 - Extension Response Bug](https://github.com/burgan-tech/vnext/issues/239)

---

## 🔥 Hotfixes

### 1. Publish Component Timeout Fix
Resolved timeout issues for long-running publish component operations.

**Problem:**
- Timeout inconsistency in init-service caused publish component requests to fail

**Solution:**
- Increased timeout duration to 10 minutes for publish component requests
- Server timeout configurations updated:

```yaml
SERVER_TIMEOUT_MS: 1800000        # 30 minutes
SERVER_KEEP_ALIVE_TIMEOUT_MS: 1800000
SERVER_HEADERS_TIMEOUT_MS: 1810000
```

> **Reference:** [#268 - Publish component request times out for long-running operations](https://github.com/burgan-tech/vnext/issues/268)  
> **Documentation:** [Server Timeout Configuration](https://github.com/burgan-tech/vnext/tree/master/init/VNext.Init.Host#server-timeout-configuration)

---

## 🔄 Improvements

### 1. Transition Pipeline Locking Refactoring
Refactored transition pipeline locking mechanism to resolve deadlock issues.

**Changes:**
- Scope-based lifecycle management for pipeline locking
- Sync dispatcher support added
- Local and remote gateway structure implemented
- Processes within the same domain use local gateway
- Cross-domain processes use remote gateway

> **Reference:** [#256 - Refactor Transition Pipeline Locking to Scope-Based Lifecycle with Sync Dispatcher Support](https://github.com/burgan-tech/vnext/issues/256)

---

### 2. Schema Refactoring
Fixed missing and incorrect schema definitions in vnext-schema repository.

**Changes:**
- Corrected missing schema definitions
- Fixed incorrect schema definitions
- Synchronized schema validator

> **Reference:** [vnext-schema #60 - Schema Refactoring: Fix Missing & Incorrect Definitions, Validator Sync](https://github.com/burgan-tech/vnext-schema/issues/60)

---

### 3. New Makefile Command: change-domain
Added `change-domain` command to Makefile for quick domain configuration.

**Usage:**
```bash
make change-domain
```

This command allows you to quickly configure your development environment infrastructure for a specific domain.

---

## 🔧 Configuration Updates

Configuration for v0.0.29:
```json
{
  "runtimeVersion": "0.0.29",
  "schemaVersion": "0.0.29",
  "componentVersion": "0.0.18"
}
```

---

## 🧱 Issues Referenced

**Features:**
- [#188 - Use Service Discovery Domain to Resolve URLs for Other Domains](https://github.com/burgan-tech/vnext/issues/188)
- [#199 - Implement Default Auto Transition for Failed Auto Transition Conditions](https://github.com/burgan-tech/vnext/issues/199)
- [#200 - Implement Idempotent Workflow Start](https://github.com/burgan-tech/vnext/issues/200)
- [#209 - Support "latest" String Value in References for Latest Version Resolution](https://github.com/burgan-tech/vnext/issues/209)
- [#216 - Add API Endpoint to Fetch Transition Schema](https://github.com/burgan-tech/vnext/issues/216)

**Bug Fixes:**
- [#239 - Extension Response Bug](https://github.com/burgan-tech/vnext/issues/239)
- [#250 - Subflow içinde extension çağırımı](https://github.com/burgan-tech/vnext/issues/250)
- [#265 - Script output handler failed – Logger is not initialized](https://github.com/burgan-tech/vnext/issues/265)

**Hotfixes:**
- [#268 - Publish component request times out for long-running operations](https://github.com/burgan-tech/vnext/issues/268)

**Improvements:**
- [#256 - Refactor Transition Pipeline Locking to Scope-Based Lifecycle with Sync Dispatcher Support](https://github.com/burgan-tech/vnext/issues/256)
- [vnext-schema #60 - Schema Refactoring](https://github.com/burgan-tech/vnext-schema/issues/60)

---

## 🧠 Summary

With this release:
✅ Service Discovery for cross-domain communication implemented  
✅ Default Auto Transition support added  
✅ "Latest" version resolution in references supported  
✅ Transition Schema API endpoint added  
✅ Idempotent workflow start behavior implemented  
✅ Subflow extension invocation bug fixed  
✅ Logger initialization in sequential tasks fixed  
✅ Extension response mapping corrected  
✅ Publish component timeout issues resolved  
✅ Transition pipeline locking refactored with gateway support  
✅ Schema definitions corrected and synchronized  
✅ New `change-domain` Makefile command added  

---

## 🔄 Upgrade Path

### From v0.0.28 to v0.0.29:

1. **Update Runtime:**
   ```bash
   git pull origin master
   ```

2. **Update Configuration:**
   ```json
   {
     "runtimeVersion": "0.0.29",
     "schemaVersion": "0.0.29",
     "componentVersion": "0.0.18"
   }
   ```

3. **(Optional) Configure Service Discovery:**
   If you want to enable cross-domain communication, add the Service Discovery configuration to your `appsettings.json` and set up the `vnext-domain-discovery` runtime.

---

**vNext Runtime Platform Team**  
January 4, 2026

