# vNext Runtime Platform - Release Notes v0.0.23-26
**Release Date:** December 16, 2025

> 🔴 **Important Notice:** Version 0.0.23 and 0.0.24 contained critical bugs and has been superseded by version 0.0.26. If you are using v0.0.23, please upgrade to v0.0.26 immediately. All references to v0.0.23 should be considered as v0.0.26.

## 🧭 Overview
This release introduces significant architectural changes including **optional key field** for workflow instances, **Native C# code support** in mappings (NAT encoding), **input mapping migration** from Execution to Orchestrator, and a **new versioning strategy** for component publishing. Additionally, enhanced **TriggerTask** capabilities, **Event Hook** feature, and various bug fixes for transition data handling, async transactions, and logging improvements are included.

> ⚠️ **Breaking Changes:** This release contains multiple breaking changes affecting transition payloads, start instance requests, mapping schema format, and component publishing strategy. Please review the Breaking Changes section carefully before upgrading.

---

## 🚀 Major Updates

### 1. Remove Mandatory Constraint on Key Field in Workflow Instance

The `key` field in workflow instances is no longer mandatory. This provides more flexibility in instance creation and allows key assignment during transitions.

**Previous Behavior:**
- `key` field was mandatory in start instance requests
- Transition payload directly received transition data

**New Behavior:**
- `key` field is now optional in start instance requests
- New transition payload schema with structured format
- Key can be set during transition if instance key is empty

**New Transition Payload Schema:**
```json
{
    "key": "",
    "tags": [],
    "attributes": {
        
    }
}
```

All fields are optional.

**Key Assignment Rules:**
- If `key` value is provided and current instance key is empty, it will be saved
- During validation, the operation proceeds only if no active instance exists with that key

**Example - Start Instance (Key Optional):**
```http
POST /ecommerce/workflows/order-processing/instances/start?sync=true
Content-Type: application/json

{
    "tags": ["priority", "express"],
    "attributes": {
        "userId": 123,
        "amount": 5000
    }
}
```

**Example - Set Key During Transition:**
```http
PATCH /ecommerce/workflows/order-processing/instances/{instanceId}/transitions/assign-key?sync=true
Content-Type: application/json

{
    "key": "ORDER-2024-001",
    "attributes": {
        "assignedBy": "system"
    }
}
```

> **Reference:** [#184 - Remove Mandatory Constraint on key Field in Workflow Instance](https://github.com/burgan-tech/vnext/issues/184)

---

### 2. Convert Mapping to Embedded Format with Native Code Support

Mapping definitions now support embedded code format with native C# code support (NAT encoding), eliminating the need for separate files and BASE64 encoding.

**Encoding Options:**
- `B64`: BASE64 encoded code (`location` field required)
- `NAT`: Native C# code (`location` field not required)

**Key Changes:**
- ✅ `encoding` property added to specify code format
- ✅ Native C# code support for better readability and development experience
- ✅ `location` property is optional for NAT encoding (still required for B64)

**Example with BASE64 Encoding (B64):**
```json
{
  "mapping": {
    "location": "./task-mapping.csx",
    "code": "cmV0dXJuIG5ldyB7IHVzZXJJZCA9IGNvbnRleHQuQm9keS5pZCB9Ow==",
    "encoding": "B64"
  }
}
```

**Example with Native Encoding (NAT):**
```json
{
  "key": "process-payment",
  "mapping": {
    "code": "public class PaymentMapping : ScriptBase, IMapping\n{\n    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)\n    {\n        var amount = context.Body?.amount;\n        return Task.FromResult(new ScriptResponse { Data = new { amount } });\n    }\n\n    public Task<ScriptResponse> OutputHandler(ScriptContext context)\n    {\n        return Task.FromResult(new ScriptResponse());\n    }\n}",
    "encoding": "NAT"
  }
}
```

> **Reference:** [#79 - Convert Mapping to Embedded Format](https://github.com/burgan-tech/vnext/issues/79)

---

### 3. Move Input Mappings from Execution to Orchestrator

Input and output mapping operations have been migrated from Execution to Orchestrator for better separation of concerns and performance optimization.

**Architecture Changes:**
- ✅ Execution is now only responsible for task invocation
- ✅ Input mapping operations performed on Orchestrator side
- ✅ Output mapping operations performed on Orchestrator side
- ✅ Resilience and retry mechanisms are consistent between Orchestrator and Execution

**Performance Optimization:**
- TriggerTasks in the same domain now execute locally instead of invocation
- Local execution includes built-in retry mechanism

**Benefits:**
- Clear separation of responsibilities
- Execution focuses purely on task execution
- Orchestrator handles data management and workflow logic
- Improved performance for same-domain trigger tasks
- Better error handling and retry consistency

> **Reference:** [#191 - Move Input Mappings from Execution to Orchestrator and Send Only Required Data](https://github.com/burgan-tech/vnext/issues/191)

---

### 4. New Versioning Strategy with Package Support

Component publishing method has been completely redesigned with a new versioning strategy that includes package version and name information.

**Previous Behavior:**
- System components loaded via `/admin/publish` endpoint with init image
- Other components managed through sys-flows
- Limited versioning and sustainability

**New Behavior:**
- All components managed via `/definitions/publish` endpoint
- New semantic versioning format with package metadata

**Version Format:**
```
MAJOR.MINOR.PATCH-pkg.PKG_VERSION+PKG_NAME
```

**Example:** `1.0.0-pkg.1.17.0+account`

**Format Components:**
- `1.0.0` → Artifact version (version from component file)
- `-pkg.1.17.0` → Package version (affects SemVer sorting: `1.0.0-pkg.1.18.0 > 1.0.0-pkg.1.17.0`)
- `+account` → Build metadata (package name, doesn't affect sorting but carries identity/distribution info)

**Version Examples:**
| Version | Description |
|---------|-------------|
| `1.0.0-pkg.1.17.0+account` | Account package, core 1.0.0, package 1.17.0 |
| `2.1.3-pkg.2.5.1+customer` | Customer package, core 2.1.3, package 2.5.1 |
| `1.0.0-alpha.1-pkg.1.17.0+account` | Alpha pre-release version |
| `1.0.0-pkg.1.17.0+account+build.123` | With build metadata |

**Client Behavior:**
- No changes in version request behavior from client side

> **Reference:** [#208 - Support No-Action Transitions with External Version Management](https://github.com/burgan-tech/vnext/issues/208)

---

### 5. TriggerTask Enhancements

TriggerTask types have been enhanced with new properties and capabilities for better flexibility and control.

#### DirectTrigger
New properties added:
- `domain` (required)
- `flow` (required)
- `transitionName` (required)
- `instanceId` (optional)
- `key` (optional)
- `async` (optional)
- `version` (optional)
- `tags` (optional)
- `body` (optional)

> **Note:** Either `instanceId` or `key` must be provided at runtime.

**Previous Behavior:**
- Only `key` value was used as `flow`

#### Start
New properties:
- `domain` (required)
- `flow` (required)
- `key` (optional)
- `async` (optional)
- `version` (optional)
- `tags` (optional)
- `body` (optional)

#### SubProcess
New properties:
- `domain` (required)
- `flow` (required)
- `key` (optional)
- `version` (optional)
- `tags` (optional)
- `body` (optional)

> **Note:** SubProcess always runs asynchronously.

**Response in Output Mapping:**
All Direct, Start, and SubProcess trigger tasks now include response in the output mapping context.

---

### 6. Transition Schema API Endpoint

A new API endpoint has been added to fetch the schema for a specific transition. This enables clients to dynamically build forms, validate data before submission, and generate UI components based on schema definitions.

**Endpoint:**
```
GET /:domain/workflows/:flow/instances/:instance/functions/schema?transitionKey=:transitionKey
```

**Features:**
- ✅ Returns schema definition for a transition in the active state
- ✅ System-provided `schema` function - no custom implementation needed
- ✅ Enables dynamic form generation
- ✅ Supports client-side validation before submission
- ✅ JSON Schema (draft/2020-12) compatible response

**Example Request:**
```http
GET /banking/workflows/account-opening/instances/18075ad5-e5b2-4437-b884-21d733339113/functions/schema?transitionKey=account-type-selection
```

**Example Response:**
```json
{
    "key": "account-type-selection",
    "type": "workflow",
    "schema": {
        "$id": "https://schemas.vnext.com/banking/account-type-selection.json",
        "type": "object",
        "title": "Account Type Selection Schema",
        "$schema": "https://json-schema.org/draft/2020-12/schema",
        "required": [
            "accountType"
        ],
        "properties": {
            "accountType": {
                "type": "string",
                "oneOf": [
                    {
                        "const": "demand-deposit",
                        "description": "Vadesiz Hesap - Demand Deposit Account"
                    },
                    {
                        "const": "time-deposit",
                        "description": "Vadeli Hesap - Time Deposit Account"
                    },
                    {
                        "const": "investment-account",
                        "description": "Fonlu Hesap - Investment Account"
                    },
                    {
                        "const": "savings-account",
                        "description": "Tasarruf Hesabı - Savings Account"
                    }
                ],
                "title": "Account Type",
                "description": "Type of account to be opened"
            }
        },
        "description": "Schema for account type selection input",
        "additionalProperties": false
    }
}
```

**Use Cases:**
- Dynamic form rendering based on transition requirements
- Client-side validation before submitting transitions
- Understanding required/optional fields for a transition
- Generating UI components automatically from schema

> **Reference:** [#216 - Add API Endpoint to Fetch Transition Schema](https://github.com/burgan-tech/vnext/issues/216)

---

### 7. Event Hook Feature

New Event Hook capability has been added for event publishing with automatic fallback to outbox.

**Behavior:**
1. Before publishing an event, the hook is executed first
2. If hook execution fails, the event is placed in outbox
3. Retry mechanism attempts to deliver from outbox

**Benefits:**
- Improved event delivery reliability
- Automatic fallback mechanism
- Built-in retry support

---

### 8. Inbox and Outbox Services Activation

Inbox and Outbox services have been activated for reliable message processing.

**Features:**
- Message persistence for reliability
- Automatic retry for failed deliveries
- Integration with Event Hook feature

---

## ⚠️ Breaking Changes

### 1. Transition and Start Instance Payload Schema Change

**Impact:** All transition and start instance requests

**Required Action:**
- Update all transition request payloads to use new schema
- `key` field is now optional in start instance requests
- Review existing integrations that depend on old payload structure

**New Schema:**
```json
{
    "key": "",
    "tags": [],
    "attributes": {}
}
```

> **Reference:** [#184](https://github.com/burgan-tech/vnext/issues/184)

---

### 2. Mapping Schema Format Change

**Impact:** All mapping definitions in workflows

**Required Action:**
- Add `encoding` property (`B64` or `NAT`)
- For B64 encoding: keep `location` property
- For NAT encoding: `location` property is not required
- Update build/publish scripts accordingly

**Migration Example (B64 - keep location):**

Before:
```json
{
  "mapping": {
    "location": "./task-mapping.csx",
    "code": "BASE64_ENCODED_CODE"
  }
}
```

After:
```json
{
  "mapping": {
    "location": "./task-mapping.csx",
    "code": "BASE64_ENCODED_CODE",
    "encoding": "B64"
  }
}
```

**New Option (NAT - no location needed):**
```json
{
  "mapping": {
    "code": "public class MyMapping : ScriptBase, IMapping { ... }",
    "encoding": "NAT"
  }
}
```

> **Reference:** [#79](https://github.com/burgan-tech/vnext/issues/79)

---

### 3. DirectTrigger Property Changes

**Impact:** All DirectTrigger task definitions

**Required Action:**
- Update DirectTrigger definitions with new required properties
- Add `domain`, `flow`, and `transitionName` properties
- Provide either `instanceId` or `key` at runtime

> **Reference:** See TriggerTask Enhancements section

---

### 4. Component Publishing Strategy Change

**Impact:** Component deployment and versioning

**Required Action:**
- Use new `/definitions/publish` endpoint
- Update versioning to new format
- Review CI/CD pipelines

> **Reference:** [#208](https://github.com/burgan-tech/vnext/issues/208)

---

## 🛠️ Bug Fixes

### 1. Transition Data Overwriting Fix
Fixed an issue where transition data was being overwritten incorrectly during workflow progression.

### 2. Async Transaction Issues
Resolved transaction issues in async requests that were causing transition pipeline execution problems.

### 3. Mapping and Task Error Handling
Fixed issues where errors in mappings and tasks were not properly stopping the pipeline execution.

### 4. Logging Cleanup
- Cleaned up logging structure to reduce noise
- Fixed deep error logging issues where errors in pipeline were getting lost
- Improved error visibility and traceability

### 5. Tracing Spans
Organized and improved tracing spans for better observability.

---

## 🔧 Configuration Updates

Configuration for v0.0.26:
```json
{
  "runtimeVersion": "0.0.26",
  "schemaVersion": "0.0.28",
  "componentVersion": "0.0.18"
}
```

---

## 🆕 Tool Updates

### vnext-workflow-cli Updates
- Added `vnext.config.json` configuration file for project management
- Project configuration now includes `domain`, `version`, and `paths` for component directories
- Fixed CSX update issue with scanning all reference files
- Updated publish command for new publish endpoint
- Improved logging with more detail

### vnext-template CLI & Template Project
- Added template project creation command
- Added schema validation and build commands
- Commands added to project package.json for CI/CD integration
- Simplified project validation and build processes

### vNext-example Updates
- Added contract flow example with subprocess and direct trigger usage
- Added task-test flow with all task types and mapping examples
- Updated project structure using vnext-template
- Added Context7 MCP integration
- Added AI .mdc file for cursor rules

### General
- vNext-runtime documentation added to Context7 MCP for AI-based documentation search

---

## 🧱 Issues Referenced

- [#184 - Remove Mandatory Constraint on key Field in Workflow Instance](https://github.com/burgan-tech/vnext/issues/184)
- [#79 - Convert Mapping to Embedded Format](https://github.com/burgan-tech/vnext/issues/79)
- [#191 - Move Input Mappings from Execution to Orchestrator](https://github.com/burgan-tech/vnext/issues/191)
- [#208 - Support No-Action Transitions with External Version Management](https://github.com/burgan-tech/vnext/issues/208)
- [#216 - Add API Endpoint to Fetch Transition Schema](https://github.com/burgan-tech/vnext/issues/216)

---

## 📘 Developer Notes

### Migration Checklist

- [ ] **⚠️ Breaking Change:** Update transition payloads to new schema
- [ ] **⚠️ Breaking Change:** Update mapping definitions (add `encoding` field; `location` required for B64, optional for NAT)
- [ ] **⚠️ Breaking Change:** Update DirectTrigger task definitions with new properties
- [ ] **⚠️ Breaking Change:** Update component publishing to use new endpoint and versioning
- [ ] Review start instance calls - `key` is now optional
- [ ] Test TriggerTask enhancements in your workflows
- [ ] Update vnext.config.json in your projects
- [ ] Verify async operations after transaction fixes
- [ ] Update to schema version 0.0.28

### New Capabilities to Explore

- **Native Code Mappings:** Use `encoding: "NAT"` for readable C# code in mappings
- **Flexible Key Assignment:** Assign keys during transitions instead of at start
- **Enhanced TriggerTasks:** Leverage new properties for better control
- **Event Hooks:** Implement reliable event publishing with automatic fallback
- **Context7 Integration:** Use AI-powered documentation search

### Migration Guide

For detailed migration steps, see the Migration section in sprint-25.md:

1. Install vnext-template CLI: `npm i -g @burgan-tech/vnext-template`
2. Create new project: `vnext-template <domain-name>`
3. Copy existing component files to new project structure
4. Configure `vnext.config.json` with domain and paths
5. Set `schemaVersion` and `runtimeVersion` for your vNext platform version
6. Run `npm install` to install schema package
7. Run `npm validate` and `npm build` for validation and build

---

## 🧠 Summary

> 🔴 **Version Note:** v0.0.23 was released with critical bugs and was immediately patched as v0.0.24. Please use v0.0.24 for production deployments.

With this release:
⚠️ **Breaking Change:** Transition and start instance payload schema changed  
⚠️ **Breaking Change:** Mapping schema format changed (encoding field added)  
⚠️ **Breaking Change:** DirectTrigger properties expanded  
⚠️ **Breaking Change:** Component publishing strategy and versioning changed  
✅ Key field now optional in workflow instances  
✅ Native C# code support in mappings (NAT encoding)  
✅ Input mappings moved to Orchestrator for better architecture  
✅ TriggerTask enhancements with new properties  
✅ Transition Schema API endpoint for dynamic form generation  
✅ Event Hook feature for reliable event publishing  
✅ Inbox and Outbox services activated  
✅ Transition data overwriting bug fixed  
✅ Async transaction issues resolved  
✅ Logging and tracing improvements  
✅ vnext-workflow-cli and template updates  
✅ Context7 MCP documentation integration

---

## 🔄 Upgrade Path

### From v0.0.22 to v0.0.26:

1. **Update Runtime:**
   ```bash
   git pull origin master
   ```

2. **Update Configuration:**
   ```json
   {
     "runtimeVersion": "0.0.26",
     "schemaVersion": "0.0.28",
     "componentVersion": "0.0.18"
   }
   ```

3. **Update Transition Payloads:**
   - Review all transition requests
   - Update to new schema format with `key`, `tags`, `attributes`

4. **Update Mapping Definitions:**
   - Add `encoding` property (`B64` or `NAT`)
   - For B64: keep `location` property (required)
   - For NAT: `location` property is optional
   - Consider migrating to NAT encoding for better readability

5. **Update DirectTrigger Tasks:**
   - Add required `domain`, `flow`, `transitionName` properties
   - Ensure `instanceId` or `key` is provided at runtime

6. **Update Component Publishing:**
   - Switch to `/definitions/publish` endpoint
   - Use new versioning format

7. **Run Validation:**
   - Use vnext-template CLI for schema validation
   - Run `npm validate` to check definitions

---

**vNext Runtime Platform Team**  
December 16, 2025

