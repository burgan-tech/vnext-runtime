# Mapping Guide

In the platform, **Mapping** is used to associate data to be passed to tasks and data generated as a result of tasks with the context in which the task is used. This document provides a comprehensive guide on how to use mapping interfaces and how to consume ScriptContext and ScriptResponse classes.

## Table of Contents
1. [Mapping Types](#mapping-types)
2. [Interfaces](#interfaces)
3. [ScriptContext Class](#scriptcontext-class)
4. [ScriptResponse Class](#scriptresponse-class)
5. [ScriptBase Usage](#scriptbase-usage)
6. [Implementation Examples](#implementation-examples)
7. [Timer Mapping Examples](#timer-mapping-examples)
8. [Best Practices](#best-practices)
9. [Error Management](#error-management)

## Mapping Types

Mapping is basically done in two areas:

### InputMapping
Used to prepare data to be passed to the task. Before the task is executed:
- Input data transformation
- Task configuration setup
- Adding headers and authentication information
- Validation operations

### OutputMapping
Used to process data returned from the task. After the task is executed:
- Response data transformation
- Merging into instance data
- Error handling
- Audit logging

## Interfaces

These are the basic interfaces used in script-based code writing for definitions on the platform. The technology used for developing scripts is the [Roslyn](https://github.com/dotnet/roslyn) product.

### Source Code References

Interface definitions and model classes can be found in the following files:
- **Main Interfaces**: [`./src/`](./src/) folder
- **Documentation Copies**: [`src/`](src/) folder  
- **ScriptContext and ScriptResponse**: [`../src/Models.cs`](../src/Models.cs)

> **Note**: The code examples in this document are for reference purposes. Please check the source files above for current definitions.

### IMapping
General mapping interface. Used for input and output bindings of tasks.

> **Source**: [`../src/IMapping.cs`](../src/IMapping.cs)

**Usage Areas:**
- Input data preparation and transformation before task execution
- Output data processing after task execution
- Data validation and transformation
- Audit logging and metadata management

**Methods:**
```csharp
Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context);
Task<ScriptResponse> OutputHandler(ScriptContext context);
```

**Method Descriptions:**
- `InputHandler`: Prepares input data before the task is executed, configures the WorkflowTask object
- `OutputHandler`: Processes output data after the task is executed and merges it into the workflow instance

### ITimerMapping
Used for schedule mapping. Special interface for timer-based workflows and scheduling operations.

> **Source**: [`../src/ITimerMapping.cs`](../src/ITimerMapping.cs)

**Usage Areas:**
- DateTime-based scheduling
- Periodic operations with cron expressions
- Duration-based delays
- Immediate execution
- Business logic-based scheduling calculations

**Method:**
```csharp
Task<TimerSchedule> Handler(ScriptContext context);
```

**Method Description:**
- `Handler`: Calculates timer schedule according to script context and returns TimerSchedule object

### ISubProcessMapping
Used for input binding for sub processes. For starting independent sub-processes.

> **Source**: [`../src/ISubProcessMapping.cs`](../src/ISubProcessMapping.cs)

**Usage Areas:**
- Background data processing
- Audit log generation
- External system notifications
- Data synchronization
- Fire-and-forget operations

**Method:**
```csharp
Task<ScriptResponse> InputHandler(ScriptContext context);
```

**Method Description:**
- `InputHandler`: Prepares the necessary input data to start a subprocess and returns ScriptResponse

**Note:** Since subprocesses run independently, only input binding is required, no output binding.

### ISubFlowMapping
Used for input binding for sub flows and output binding to transfer data to the parent flow when completed.

> **Source**: [`../src/ISubFlowMapping.cs`](../src/ISubFlowMapping.cs)

**Usage Areas:**
- Approval workflows
- Data processing workflows
- Validation processes
- Computed value generation
- Parent-child workflow integration

**Methods:**
```csharp
Task<ScriptResponse> InputHandler(ScriptContext context);
Task<ScriptResponse> OutputHandler(ScriptContext context);
```

**Method Descriptions:**
- `InputHandler`: Prepares input data to start subflow and returns ScriptResponse
- `OutputHandler`: Transfers results to parent workflow when subflow is completed and returns ScriptResponse

**Note:** Since subflows run integrated with the parent workflow, both input and output binding are required.

### IConditionMapping
Used for decision parts like auto transitions. Provides condition checking in automatic transitions.

> **Source**: [`../src/IConditionMapping.cs`](../src/IConditionMapping.cs)

**Usage Areas:**
- Data validation checks
- Business rule validation
- Time-based conditions
- External system status checks
- User role/permission verification
- Auto-transition decision logic

**Method:**
```csharp
Task<bool> Handler(ScriptContext context);
```

**Method Description:**
- `Handler`: Returns boolean value according to the given context (true: allow transition, false: block transition)

## ScriptContext Class

`ScriptContext` contains all contextual information available during script execution. This class provides workflow instance, transition information, request data, and other metadata to scripts.

> **Source**: [`../src/Models.cs`](../src/Models.cs) - `ScriptContext` class (lines 112-599)

### Basic Properties

```csharp
public sealed class ScriptContext
{
    // Request information (automatically converted to camelCase format)
    public dynamic? Body { get; private set; }
    public dynamic? Headers { get; private set; }
    public dynamic? RouteValues { get; private set; }
    
    // Workflow instance information
    public Instance Instance { get; private set; }
    public Definitions.Workflow Workflow { get; private set; }
    public Transition Transition { get; private set; }
    
    // Runtime information
    public IRuntimeInfoProvider Runtime { get; private set; }
    public Dictionary<string, dynamic> Definitions { get; private set; }
    public Dictionary<string, dynamic?> TaskResponse { get; private set; }
    public Dictionary<string, dynamic> MetaData { get; private set; }
    
    // Builder pattern support
    public sealed class Builder { /* ... */ }
}
```

### Important Properties

#### Body
Contains request payload data. Data from transition requests or task execution results are found here.

```csharp
// Reading data from Body
var userId = context.Body?.userId;
var amount = context.Body?.amount;
```

#### Headers
Contains request header information. All header names are converted to **lower-case** format for consistency.

> **Important**: Request Headers are always converted and stored as **lower-case** for consistency.

```csharp
// Reading data from headers (in lower-case format)
var authorization = context.Headers?.authorization;
var contentType = context.Headers?.["content-type"];
var userAgent = context.Headers?.["user-agent"];
```

#### Instance.Data
Contains the current data of the workflow instance. All information accumulated throughout the workflow is stored here.

> **Important**: `Instance.Data` always holds data in **camelCase** format. All property names are converted to camelCase.

```csharp
// Reading data from instance data (in camelCase format)
var userInfo = context.Instance.Data.userInfo;
var paymentSchedule = context.Instance.Data.paymentSchedule;
var currentLogin = context.Instance.Data.login.currentLogin;
```

#### TaskResponse
Contains the results of completed tasks. Used to process task results in output handlers.

```csharp
// Reading data from task responses
var httpTaskResult = context.TaskResponse["httpTask"];
var scriptTaskResult = context.TaskResponse["scriptTask"];
```

## ScriptResponse Class

`ScriptResponse` is the standard response model returned from mapping interfaces. It carries task audit data, instance merge data, and metadata information.

> **Source**: [`../src/Models.cs`](../src/Models.cs) - `ScriptResponse` class (lines 23-63)

### Structure

```csharp
public sealed class ScriptResponse
{
    /// <summary>
    /// Unique identifier or key associated with the script response.
    /// </summary>
    public string? Key { get; set; }
    
    /// <summary>
    /// The primary data payload returned by the script execution.
    /// Context-dependent usage based on mapping interface.
    /// </summary>
    public dynamic? Data { get; set; }
    
    /// <summary>
    /// HTTP headers or metadata headers associated with the response.
    /// </summary>
    public dynamic? Headers { get; set; }
    
    /// <summary>
    /// Route values or routing parameters associated with the response.
    /// </summary>
    public dynamic? RouteValues { get; set; }
    
    /// <summary>
    /// Collection of tags for categorizing, filtering, or marking the response.
    /// </summary>
    public string[] Tags { get; set; } = [];
}
```

### Property Descriptions

- **Key**: Unique key identifying the response
- **Data**: Main data payload (data to be merged into instance)
- **Headers**: HTTP headers or metadata headers
- **RouteValues**: Routing parameters
- **Tags**: Tags for categorization and filtering

### ⚠️ Important Warnings - ScriptResponse.Data

> **Critical**: Since the `ScriptResponse.Data` property is of **dynamic** type, runtime compile errors are a frequently encountered situation.

**Situations to Be Careful About:**

1. **Type Safety**: Dynamic objects are not checked at compile-time
   ```csharp
   // ❌ Wrong - may cause runtime error
   response.Data = new { invalidProperty = someComplexObject };
   
   // ✅ Correct - use simple types
   response.Data = new { success = true, userId = 123 };
   ```

2. **Null Reference Check**: Check for null in dynamic properties
   ```csharp
   // ❌ Wrong - null reference exception risk
   var result = context.Body.data.result;
   
   // ✅ Correct - null-safe access
   var result = context.Body?.data?.result;
   ```

3. **Serialization Issues**: Complex objects may not be serializable
   ```csharp
   // ❌ Wrong - may not be serializable
   response.Data = new { complexObject = new SomeComplexClass() };
   
   // ✅ Correct - use serializable objects
   response.Data = new { 
       id = obj.Id, 
       name = obj.Name, 
       timestamp = DateTime.UtcNow 
   };
   ```

4. **Property Naming**: Use camelCase
   ```csharp
   // ❌ Wrong - PascalCase
   response.Data = new { UserId = 123, PaymentStatus = "success" };
   
   // ✅ Correct - camelCase
   response.Data = new { userId = 123, paymentStatus = "success" };
   ```

### Usage Examples

```csharp
// Simple data response
return new ScriptResponse
{
    Data = new { success = true, userId = 123 }
};

// Response with headers
return new ScriptResponse
{
    Data = requestData,
    Headers = new { Authorization = "Bearer " + token },
    Tags = new[] { "authentication", "success" }
};
```

## ScriptBase Usage

The `ScriptBase` class provides easy access to frequently used functions in scripts. It is especially useful for DAPR secret store integration.

### Basic Functions

```csharp
public abstract class ScriptBase
{
    // Secret store functions
    protected string GetSecret(string storeName, string secretStore, string secretKey);
    protected async Task<string> GetSecretAsync(string storeName, string secretStore, string secretKey);
    protected Dictionary<string, string> GetSecrets(string storeName, string secretStore);
    
    // Property check functions (newly added)
    protected static bool HasProperty(object obj, string propertyName);
    protected static object? GetPropertyValue(object obj, string propertyName);
}
```

> **Newly Added Methods**: `HasProperty` and `GetPropertyValue` methods have been added to ScriptBase. These methods provide safe property access on dynamic objects.

### Usage Example

```csharp
public class MyMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Get API key from secret store
        var apiKey = GetSecret("dapr_store", "secret_store", "api_key");
        
        var httpTask = task as HttpTask;
        httpTask.SetHeaders(new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {apiKey}"
        });
        
        return Task.FromResult(new ScriptResponse());
    }
}
```

### HasProperty and GetPropertyValue Usage

```csharp
public class SafePropertyAccessMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Safe property check
        if (HasProperty(context.Instance.Data, "card"))
        {
            var cardData = GetPropertyValue(context.Instance.Data, "card");
            
            if (HasProperty(cardData, "products"))
            {
                var productsData = GetPropertyValue(cardData, "products");
                // Process with products
            }
        }
        
        return Task.FromResult(new ScriptResponse());
    }
}
```

This comprehensive guide provides detailed information on how to use mapping interfaces and how to consume ScriptContext and ScriptResponse classes. The examples are taken from real usage scenarios and also provide guidance on best practices and error management.
