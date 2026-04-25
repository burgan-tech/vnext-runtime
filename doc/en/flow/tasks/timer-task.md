# Timer Task

Timer Task is a system task that performs timer operations in workflows. This task type is only used by the system and cannot be defined manually. It is used for waiting for a certain period, schedule operations, and timeout management.

:::warning System Task
This task type is only used internally by the system. Cannot be defined manually.
:::

## Workflow Timeout Pipeline (v0.0.50+)

When a workflow-level timeout fires, the system now executes the full **TransitionPipeline** for the timeout target state. This includes `onExit` tasks, state change, `onEntry` tasks, finish handling, scheduled transition registration, and automatic transitions. Previously, `FlowTimeoutJobHandler` performed a direct state mutation and marked the instance complete without running any pipeline steps.

> **Reference:** [#514](https://github.com/burgan-tech/vnext/issues/514)

## Dynamic Timeout Mapping (v0.0.50+)

Workflow-level timeout supports an optional `mapping` field for dynamic timeout duration calculation via `ITimerMapping` scripts. The mapping script is evaluated at runtime using `ScriptContext` (with access to instance data, workflow metadata, headers). If the script succeeds, its `TimerSchedule` result (DateTime or Duration) is used. On failure, the static `timer.duration` serves as fallback. A static `timer` is **required** when `mapping` is defined.

**Definition example:**

```json
"timeout": {
  "key": "$timeout",
  "target": "timed-out",
  "versionStrategy": "None",
  "timer": { "reset": "false", "duration": "PT1H" },
  "mapping": {
    "location": "./src/TimeoutMapping.csx",
    "code": "<BASE64>"
  }
}
```

**Logging:**

| Event | Level | Description |
|-------|-------|-------------|
| `TimeoutMappingResolved` | Info | Mapping script succeeded; dynamic schedule used |
| `TimeoutMappingFallback` | Warning | Mapping script failed; static timer.duration used as fallback |

> **Reference:** [#524](https://github.com/burgan-tech/vnext/issues/524)
