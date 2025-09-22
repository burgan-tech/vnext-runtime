# State

Represents the stage or phase where a workflow instance is located. It is called **State**.

## State Types

The State object can be defined in the following types:

### `Initial` (Starting)
- The starting state of the workflow
- Can only be executed with `start instance`
- Intermediate states cannot be initial
- There should be only one in the entire flow

### `Intermediate` (Intermediate)
- Middle states where work is done
- Multiple transitions can be connected
- States where the main business logic of the workflow is executed

### `Finish` (End)
- End states of the workflow
- When this state is reached, the instance status is updated as "Completed"
- Indicates that the workflow has been completed successfully

### `SubFlow` (Sub Flow)
- State that runs another workflow
- Blocks the main workflow and the subflow is organized through the main workflow
- When SubFlow is completed, it continues from auto and schedule transitions
- Increases the reusability capability of workflows

:::info[SubProcess vs SubFlow]
**SubProcess**: Does not block the main workflow and runs in parallel in isolation. It runs parallel to the main workflow and is used to implement flow patterns like fan-in.

**SubFlow**: Blocks the main workflow and the subflow is organized through the main workflow. The main workflow cannot continue until SubFlow is completed.
:::

## State Lifecycle

The state machine follows the following lifecycle:

![State Machine LifeCycle](https://kroki.io/mermaid/svg/eNp9U89v2jAUvu-veNLOqKNVpYrDJJoECC0Bjag9WBzc8JJYDU5lO2xo3v8-xw7Ei1hzsCJ_P973nu28qn9mJRUK0vALmG9KUkG5ZIrVHFLBigIF7ncwGn2HR7JVVCFs6oplJwhKzN7lzsoeLUG_0IrtNQQkaIRAblx7szWPfmHWtP8plQNhzI9OGiXhmERC1GJyrvPC6oq2MqewS2Bl4aWOC9aWYKpzDi0l6jIHJeUFOiSyyIykVBToabkSDKVXZWaJ89-OkZ4-0DX9p6fMXf4Z40yWGhYk5lJRnqG1beQEgvrwUaEyQ_QF2-ZtZmavISZuLEbgtv6hxdwMj1Y3MVcoDrhnJoiGJZk2qvaHa2N5yReu-SS8Ja-1eM-N7zBIbClPpCs7hJ8svOwtly7RsLKZuFRSw_OljwFj56uTeghrWJFtVuK-qfCzjp6twbTfWHWDvKI9Z0rIK2UK8lrAFdrOtzHBrlA0rLv7M80UOyKMoHVkvPCSJc4hZQeEH0hbFw2b_lj_V3kz7GjtjFaUN-bIo6N7QPYJap8n1cnYTSFnVTX5iuP8PkcPmHdAnud3-M0DFmfFQ36PDx7Q3pNPsPHFEN8Q_wJFGEZX)

### Lifecycle Steps

1. **State Policy Checks**
   - Transitions defined in the state are checked
   - Client can only trigger manual and event transitions
   - Auto and schedule transitions are only executed by the system
   - A transition cannot select the same state as target again

2. **Current Transition OnExecutionTasks**
   - OnExecutionTasks of the current transition are executed

3. **Current State OnExits**
   - OnExit tasks of the current state are executed

4. **State Change**
   - Transition is made to the target state of the Current Transition
   - State change only occurs through transitions

5. **State OnEntries**
   - OnEntry tasks of the new state are executed

6. **State Type Check**
   - **Finish**: Instance status is updated as "Completed"
   - **SubFlow**: SubFlow/SubProcess is executed

7. **Auto Transitions**
   - Automatic transitions are executed

8. **Schedule Transitions**
   - Scheduled transitions are executed

## State Components

### VersionStrategy
- Data versioning strategy of the state
- **Major**: For major changes
- **Minor**: For minor changes

### Labels
- State labels for multi-language support
- Separate labels can be defined for each language

### Transitions
- Transitions exiting from the state
- Can be manual, automatic, scheduled, and event-based

### OnEntries
- Tasks executed when entering the state
- Executed during state activation

### OnExits
- Tasks executed when exiting the state
- Executed before leaving the state

### View
- View reference associated with the state
- Used for displaying UI components

### SubFlow
- Sub workflow reference for SubFlow type states
- Contains type, reference, and mapping information

## State Schema

```json
{
  "key": "string",
  "stateType": "Initial|Intermediate|Finish|SubFlow",
  "versionStrategy": {
    "type": "Major|Minor"
  },
  "labels": [
    {
      "label": "string",
      "language": "string"
    }
  ],
  "transitions": [...],
  "onEntries": [...],
  "onExits": [...],
  "subFlow": {
    "type": "string",
    "reference": {...},
    "mapping": {...}
  }
}
```
