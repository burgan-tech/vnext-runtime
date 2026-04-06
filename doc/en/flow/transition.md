Components that manage transitions between states in a workflow definition are called **Transitions**. Transitions are responsible for managing state transitions and can work according to different trigger types.

## Transition Properties

### Basic Properties
- **Key**: Unique key of the transition
- **From**: Specifies which state the transition will be made from (can be null)
- **Target**: Determines which state the transition will be made to
- **TriggerType**: Trigger type of the transition
- **VersionStrategy**: Data version change strategy (Major/Minor)

### Optional Properties
- **Timer**: Mapping code used for scheduled transitions
- **Rule**: Condition mapping code used for automatic transitions (or **Dynamic Expresso** when **`location`** is **`dynamicExpresso`** — v0.0.43+)
- **Schema**: Schema reference used to validate the payload transmitted in the transition
- **AvailableIn**: Specifies in which states shared transitions can be executed
- **Labels**: Labels for multi-language support
- **View**: View reference of the transition
- **OnExecutionTasks**: Tasks to be executed when the transition is run

## Trigger Types (TriggerType)

### Manual (0)
Transition called by the client. Triggered by user interaction.

**Usage Areas:**
- User button clicks
- Form submissions  
- Manual approval processes

### Automatic (1)
Conditional transition automatically executed by the system. 

**Properties:**
- Defined with mapping in the `Rule` field
- Compiles and executes the `IConditionMapping` interface at runtime
- Automatically triggered when certain conditions are met

**Usage Areas:**
- Automatic transitions based on business rules
- Automatic progressions after status checks
- Transitions after data validation

#### Rule expressions with Dynamic Expresso (v0.0.43+)

**Dynamic Expresso expression rules (optional).** Besides **Roslyn** **`IConditionMapping`** scripts, an automatic transition **Rule** may use plain-text **boolean** expressions evaluated with **Dynamic Expresso**, so you can avoid a separate compiled condition script when this mode fits.

**Selection**

- Set **`rule.location`** to **`dynamicExpresso`** and put the expression in **`rule.code`** using **native** encoding: **`"encoding": "NAT"`** in JSON, or **`ScriptCode.FromNative`** in code.
- Any other **`location`** continues to use the Roslyn condition script path (**RoutingConditionEvaluator** → **ScriptConditionEvaluator**).

**Root binding**

Expressions receive a single parameter **`context`** of type **`ExpressoRuleContext`**, built from **`ScriptContext`** with an allowlist only:

| `context` member | Available surface |
|------------------|-------------------|
| **`Body`** | Request body payload |
| **`CurrentTransition`** | **`Data`**, **`Header`**; may be **null** outside persisted transition requests |
| **`MetaData`** | Metadata bag |
| **`Workflow`** | **`key`**, **`domain`**, **`flow`**, **`version`**, **`StateKeys`** |
| **`Instance`** | **`Id`**, **`Key`**, **`Flow`**, state fields, **`Data`** as JSON |
| **`Headers`** | HTTP headers |
| **`QueryParameters`** | Query string parameters |
| **`RouteValues`** | JSON object from route data |
| **`Transition`** | **`Key`**, **`From`**, **`Target`**, **`TriggerType`**, **`TriggerKind`**; may be **null** if not set on script context; **no** embedded rule/timer/task payloads |
| **`Runtime`** | **`Domain`**, **`Version`**; may be **null** if not set |

**JSON under `Instance.Data` / `Body` / etc.**

JSON is exposed as **`RuleJsonDynamic`**: dynamic member access, string indexers, array **`Count`**, array **`Contains`**. Missing object keys or missing dot-properties resolve to **null** (no runtime binder failure), so you can use **`?.`** and **`??`** in expressions where Dynamic Expresso supports them.

Use **`AsDouble()`** / **`AsInt32()`** for numeric JSON values, **`AsBoolean()`** for booleans, and **`AsArrayLength()`** for array lengths; **`Contains`** is supported on JSON arrays.

**Expression examples**

```text
context.Instance.Data["amount"].AsDouble() > 100000
context.Instance.Data["documents"].AsArrayLength() == 0
context.Instance.Data["flags"]["manualReviewRequired"].AsBoolean() == false
context.Instance.Data["approvers"].Contains("u1")
context.Body["score"].AsDouble() >= 80
context.RouteValues["entityId"].ToString() == context.Instance.Data["externalId"].ToString()
context.Transition != null && context.Transition.Key == "approve-auto"
context.Runtime != null && context.Runtime.Domain.ToString() == "my-domain"
```

**JSON rule examples** (automatic transition definition fragment; **`encoding: NAT`** = native / plain expression)

```json
"rule": {
  "location": "dynamicExpresso",
  "encoding": "NAT",
  "code": "context.Instance.Data.absenceType.ToString() == \"personal-leave\""
}
```

```json
"rule": {
  "location": "dynamicExpresso",
  "encoding": "NAT",
  "code": "context.Headers.sub.ToString() == context.Instance.Data.customerId.ToString()"
}
```

> **Reference:** [#470](https://github.com/burgan-tech/vnext/issues/470)

#### Default Auto Transition (v0.0.29+)

When multiple automatic transitions are defined and none of their conditions are met, a **Default Auto Transition** can be triggered as a fallback.

**Configuration:**
```json
{
  "triggerKind": 10
}
```

**TriggerKind Values:**
| Value | Description |
|-------|-------------|
| 0 | Not applicable (standard auto transition) |
| 10 | Default Auto Transition |

**Example Usage:**
```json
{
  "transitions": [
    {
      "key": "approve",
      "target": "approved",
      "triggerType": 1,
      "rule": { "ref": "Mappings/check-approval.cs" }
    },
    {
      "key": "reject",
      "target": "rejected",
      "triggerType": 1,
      "rule": { "ref": "Mappings/check-rejection.cs" }
    },
    {
      "key": "pending-review",
      "target": "pending",
      "triggerType": 1,
      "triggerKind": 10
    }
  ]
}
```

In this example, if neither `approve` nor `reject` conditions are met, the `pending-review` transition will be executed as the default fallback.

> **Note:** Only one transition with `triggerKind: 10` should be defined per state. If no conditions match and no default is defined, no automatic transition occurs.

### Scheduled (2)
Scheduled transition. Used when it is desired to run at a specific time or periodically like cron. Only executed by the system.

**Properties:**
- Defined with mapping in the `Timer` field
- Compiles and executes the `ITimerMapping` interface at runtime
- Time-based triggering

**Usage Areas:**
- Periodic reporting
- Automatic backup operations
- Sending scheduled notifications
- Timeout situations

### Event (3)
Transition called by Pub/Sub systems. Provides event-based triggering.

**Usage Areas:**
- Inter-microservice communication
- External system integrations
- Asynchronous operation triggers
- Event-driven architecture implementations

## Update Parent Data Transition (v0.0.31+)

A special transition type for updating parent workflow instance data from SubFlow states. This transition does not change the state; it only updates the parent instance's data.

### Key Features

- **Target**: Always `$self` - No state change occurs
- **Usage**: Only available in `subflow-state` type states
- **Behavior**: Does not advance to SubFlow, only performs data update
- **Well-Known Key**: `update-parent-data`

### Configuration

The transition is defined as `updateData` configuration in the workflow definition:

```json
{
  "updateData": {
    "key": "update-parent",
    "target": "$self",
    "triggerType": 0,
    "versionStrategy": "None",
    "labels": [
      { "language": "en", "label": "Update Parent Data" },
      { "language": "tr", "label": "Parent Veri Güncelle" }
    ]
  }
}
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| `key` | Unique key for the transition (e.g., `"update-parent"`) |
| `target` | Must be `"$self"` - No state change |
| `triggerType` | `0` (Manual) - Triggered by user |
| `versionStrategy` | Data versioning strategy |
| `labels` | Labels for multi-language support |

### What It Does

| Operation | Performed? |
|-----------|------------|
| Data mapping (according to transition rules) | Yes |
| Instance data update | Yes |
| Instance key validation and setting | Yes |
| Transition record creation | Yes |
| Tag addition (if any) | Yes |

### What It Does NOT Do

| Operation | Performed? |
|-----------|------------|
| State change (target is `$self`) | No |
| SubFlow advancement | No |
| Auto transition invocation | No |
| OnExit/OnEntry task execution | No |
| State change event publishing | No |

### Important Notes

1. **State Type Restriction**: Only usable in SubFlow states. In other state types, the normal transition pipeline runs.

2. **Target Restriction**: Target must be `"$self"`. If a different target is specified, normal transition behavior is exhibited.

3. **Instance Status**: Cannot be executed on completed instances. Returns an error in this case.

---

## Shared Transitions
Thanks to the `AvailableIn` property, a transition can be made available in multiple states. In this case, this list determines in which states the transition can be executed.

**Parent flow from subflow (v0.0.39+):** When an instance is **in a subflow**, the **parent flow’s** shared transition can now be executed (previously only the subflow’s shared transition was available). If a shared transition is available while the instance is in a subflow, its **target** must be **$self** so that the transition applies to the correct (parent) context when invoked from the subflow.

## Version Management
With the `VersionStrategy` property, the data version of transitions can be changed as Major and Minor. This is important in terms of backward compatibility and data migration.

## Payload Validation
Data transmitted in the transition can be validated using the `Schema` reference. This way, data integrity is maintained and erroneous data entries are prevented.
