# vNext Runtime Platform - Release Notes v0.0.27
**Release Date:** December 19, 2025

## 🧭 Overview
This release focuses on bug fixes and stability improvements. Key fixes include Extension response mapping, HttpTask status code handling, memory leak resolution in Roslyn, and init-service response improvements.

> ✅ **No Breaking Changes:** This release is fully backward compatible with v0.0.26. No migration steps required.

---

## 🛠️ Bug Fixes

### 1. Extension Response Mapping Bug
Fixed an issue where response mapping was not working correctly on Extension.

**Problem:**
- Response mapping on Extension was producing incorrect results

**Solution:**
- Fixed the response mapping logic to handle Extension responses correctly

> **Reference:** [#239 - Extension Response Bug](https://github.com/burgan-tech/vnext/issues/239)

---

### 2. HttpTask StatusCode Override Bug
Fixed an issue where status code values were being overwritten incorrectly in HttpTask and Dapr Service Invocations.

**Problem:**
- HttpTask was overriding status code 500 incorrectly
- Dapr Service Invocation status codes were also affected

**Solution:**
- Status code values are now preserved correctly throughout the request pipeline

> **Reference:** [#240 - HttpTask StatusCode 500 override hatası](https://github.com/burgan-tech/vnext/issues/240)

---

## 🔥 Hotfixes

### 1. Memory Leak Fix in Roslyn
Resolved a memory leak issue in the Roslyn script compilation engine.

**Impact:**
- Improved memory management during script execution
- Better resource cleanup for long-running workflows

> **Reference:** [PR #237 - fix memory leak issue](https://github.com/burgan-tech/vnext/pull/237)

---

### 2. Init-Service Response Fix
Fixed issues in init-service response handling for success status and message parts.

**Changes:**
- Corrected success flag handling
- Improved message formatting in responses

> **Reference:** [PR #236 - fix init-service for response](https://github.com/burgan-tech/vnext/pull/236)

---

## 🔧 Configuration Updates

Configuration for v0.0.27:
```json
{
  "runtimeVersion": "0.0.27",
  "schemaVersion": "0.0.28",
  "componentVersion": "0.0.18"
}
```

> **Note:** Schema version remains unchanged from v0.0.26.

---

## 🧱 Issues Referenced

- [#239 - Extension Response Bug](https://github.com/burgan-tech/vnext/issues/239)
- [#240 - HttpTask StatusCode 500 override hatası](https://github.com/burgan-tech/vnext/issues/240)
- [PR #237 - fix memory leak issue](https://github.com/burgan-tech/vnext/pull/237)
- [PR #236 - fix init-service for response](https://github.com/burgan-tech/vnext/pull/236)

---

## 🧠 Summary

With this release:
✅ Extension response mapping bug fixed  
✅ HttpTask and Dapr Service Invocation status code handling fixed  
✅ Memory leak in Roslyn resolved  
✅ Init-service response handling improved  

---

## 🔄 Upgrade Path

### From v0.0.26 to v0.0.27:

1. **Update Runtime:**
   ```bash
   git pull origin master
   ```

2. **Update Configuration:**
   ```json
   {
     "runtimeVersion": "0.0.27",
     "schemaVersion": "0.0.28",
     "componentVersion": "0.0.18"
   }
   ```

No additional migration steps required.

---

**vNext Runtime Platform Team**  
December 19, 2025

