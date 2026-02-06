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
- **Rule**: Condition mapping code used for automatic transitions
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

## Version Management
With the `VersionStrategy` property, the data version of transitions can be changed as Major and Minor. This is important in terms of backward compatibility and data migration.

## Payload Validation
Data transmitted in the transition can be validated using the `Schema` reference. This way, data integrity is maintained and erroneous data entries are prevented.
