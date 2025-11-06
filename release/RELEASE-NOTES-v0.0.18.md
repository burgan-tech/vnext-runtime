# vNext Runtime Platform - Release Notes v0.0.18
**Release Date:** November 6, 2025

## 🧭 Overview
This release brings functional extensions for the **vNext Runtime Platform**, flexible structure in view management, new functions for the script engine, and changes that ensure consistency in schema structure.  
Additionally, `subFlow` view override support, new system tasks, and configuration updates are also provided with this release.

---

## 🚀 Major Updates

### 1. Schema Enhancements
Extensive tightening has been implemented on schemas in this release:
- `target` has been removed from the **View schema**.
- Adjustments have been made to common fields such as `timer`, `rule`, `view` in **Workflow** schemas.
- **New View Schema:**
```json
{
  "view": {
    "extensions": ["ex_1", "ex_2"],
    "loadData": true,
    "view": {
      "key": "",
      "version": "",
      "domain": "",
      "flow": "sys-views"
    }
  }
}
```
> For complete schema details, you can refer to the [`release-v0.0` branch in the `vnext-schema` repository](https://github.com/burgan-tech/vnext-schema).

---

### 2. SubFlow View Override Support
Custom view definitions can now be made in subflow calls. This allows customized screens to be used in different subflows.

**Example Usage:**
```json
"subFlow": { 
  "type": "S", 
  "process": { 
      "key": "password-subflow", 
      "domain": "core", 
      "version": "1.0.0", 
      "flow": "sys-flows" 
  }, 
  "mapping": { 
    "location": "./src/PasswordSubflowMapping.csx"
  }, 
  "viewOverides": [ 
    { 
      "account-selection-view": { 
        "key": "account-selection-new-view",
        "domain": "core",
        "version": "1.0.0",
        "flow": "sys-views"
      } 
    }
  ] 
}
```

---

### 3. ScriptBase Extensions
New functions have been added. ScriptBase now provides comprehensive helper methods for both **Dapr Secret Store** and **Configuration** operations.

**New Featured Functions:**
- `GetSecret`, `GetSecretAsync`, `GetSecrets`, `GetSecretsAsync`
- `LogTrace`, `LogDebug`, `LogInformation`, `LogWarning`, `LogError`, `LogCritical`
- `GetConfigValue`, `GetConnectionString`, `ConfigExists`

These functions are located in `BBT.Workflow.Scripting.Functions.ScriptBase`.

---

### 4. System Tasks
Newly added **reusable system tasks**:
#### Script Task
```json
{
  "key": "script-task",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-tasks"
}
```
#### Notification Task
Enables sending notifications through the Hub:
```json
{
  "key": "notification-task",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-tasks"
}
```

---

## 🧩 Bug Fixes
- **Transition Bug:**  
  Fixed the issue where `Instance Data` could not be written when the payload was empty during transition.

---

## ⚠️ Breaking Changes
- **View schema** structure has been changed (new `extensions` and `loadData` fields).
- Old extension system components (`data`, `view`, `available-transition`) have been removed.
- **View endpoint response** structure has been changed:
```json
// Old:
<returned content directly>

// New:
{
  "type": "json",
  "content": "....",
  "key": "account-creation-view"
}
```

---

## 🔧 Configuration Updates
New `vnext.config.json` example:
```json
{
  "runtimeVersion": "0.0.18",
  "schemaVersion": "0.0.17"
}
```

---

## 🧱 Issues Referenced
- [View Schema Extensions #58](https://github.com/burgan-tech/vnext/issues/58)
- [Extension and loadData property for View #75](https://github.com/burgan-tech/vnext/issues/75)
- [Implementation into existing SignalR Hub #76](https://github.com/burgan-tech/vnext/issues/76)
- [Improvement of viewOverides property in SubItems #84](https://github.com/burgan-tech/vnext/issues/84)
- [Versioning and History Management Strategies #97](https://github.com/burgan-tech/vnext/issues/97)

---

## 📘 Developer Notes
- Before using the new functions, make sure the `@burgan-tech/vnext-core-runtime` package is updated to **v0.0.18**.
- Update your `vnext.config.json` and schemas to be compatible with the new version.

---

## 🧠 Summary
With this release:
✅ View structure has been modernized.  
✅ SubFlow view override support has been added.  
✅ ScriptBase has become more powerful.  
✅ Notification and Script system tasks have been added.  
✅ Schema consistency has been improved.

---

**vNext Runtime Platform Team**  
November 6, 2025
