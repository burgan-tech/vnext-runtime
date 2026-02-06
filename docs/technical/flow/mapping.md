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
- **Main Interfaces**: [`../../src/`](../../src/) folder
- **Documentation Copies**: [`../../src/`](../../src/) folder  
- **ScriptContext and ScriptResponse**: [`../../src/Models.cs`](../src/Models.cs)

> **Note**: The code examples in this document are for reference purposes. Please check the source files above for current definitions.

### IMapping
General mapping interface. Used for input and output bindings of tasks.

> **Source**: [`../../src/IMapping.cs`](../../src/IMapping.cs)

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

> **Source**: [`../../src/ITimerMapping.cs`](../../src/ITimerMapping.cs)

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

> **Source**: [`../../src/ISubProcessMapping.cs`](../../src/ISubProcessMapping.cs)

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

> **Source**: [`../../src/ISubFlowMapping.cs`](../../src/ISubFlowMapping.cs)

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

> **Source**: [`../../src/IConditionMapping.cs`](../../src/IConditionMapping.cs)

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

> **Source**: [`../../src/Models.cs`](../../src/Models.cs) - `ScriptContext` class (lines 112-599)

### Basic Properties

```csharp
public sealed class ScriptContext
{
    // Request information (automatically converted to camelCase format)
    public dynamic? Body { get; private set; }
    public dynamic? Headers { get; private set; }
    public dynamic? QueryParameters { get; private set; }
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

#### QueryParameters
Contains query string parameters from Function tasks. This property is **only available when using Function tasks** and provides access to query parameters passed to the function endpoint.

> **Important**: QueryParameters is specific to Function tasks and is accessed using indexer syntax.

```csharp
// Reading data from query parameters (Function tasks only)
var userId = context.QueryParameters?.["userId"];
var cityId = context.QueryParameters?.["cityId"];
var page = context.QueryParameters?.["page"];
var pageSize = context.QueryParameters?.["pageSize"];
var filter = context.QueryParameters?.["filter"];
```

**Common Use Cases:**
- Accessing custom query parameters in Function task handlers
- Reading filter parameters passed to functions
- Getting pagination parameters from function calls
- Extracting user-specific identifiers from query strings

**Example in Function Task Mapping:**
```csharp
public class FunctionTaskMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Access query parameters from function call
        var userId = context.QueryParameters?.["userId"];
        var cityId = context.QueryParameters?.["cityId"];
        
        // Use in function logic
        LogInformation("Processing function for userId: {0}, cityId: {1}", 
            args: new object?[] { userId, cityId });
        
        return Task.FromResult(new ScriptResponse());
    }
}
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

> **Source**: [`../../src/Models.cs`](../../src/Models.cs) - `ScriptResponse` class (lines 23-63)

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
    
    // Typed property value retrieval (newly added)
    protected T? GetPropertyValue<T>(object obj, string propertyName);
}
```

> **Newly Added Methods**: `HasProperty`, `GetPropertyValue`, and generic `GetPropertyValue<T>` methods have been added to ScriptBase. These methods provide safe property access and type conversion on dynamic objects.

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
        
        // Safe access with generic type conversion
        var userId = GetPropertyValue<int>(context.Instance.Data, "userId");
        var amount = GetPropertyValue<decimal>(context.Body, "amount");
        var isActive = GetPropertyValue<bool>(context.Instance.Data, "isActive");
        
        if (userId.HasValue && amount.HasValue)
        {
            LogInformation("Processing transaction for user {0} with amount {1}", 
                args: new object?[] { userId.Value, amount.Value });
        }
        
        return Task.FromResult(new ScriptResponse());
    }
}
```

### Logging Functions

ScriptBase provides comprehensive logging functions with automatic caller information capture. All logging methods use `CallerFilePath`, `CallerMemberName`, and `CallerLineNumber` attributes to automatically include source location in logs.

**Available Log Levels:**

```csharp
public abstract class ScriptBase
{
    // Trace level - very detailed diagnostic information
    protected static void LogTrace(object message, 
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? method = null,
        [CallerLineNumber] int line = 0,
        params object[] args);

    // Debug level - internal system events
    protected static void LogDebug(object message,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? method = null,
        [CallerLineNumber] int line = 0,
        params object[] args);

    // Information level - general informational messages
    protected static void LogInformation(object message,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? method = null,
        [CallerLineNumber] int line = 0,
        params object[] args);

    // Warning level - abnormal or unexpected events
    protected static void LogWarning(object message,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? method = null,
        [CallerLineNumber] int line = 0,
        params object[] args);

    // Error level - error events
    protected static void LogError(object message,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? method = null,
        [CallerLineNumber] int line = 0,
        params object[] args);

    // Critical level - critical failures
    protected static void LogCritical(object message,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? method = null,
        [CallerLineNumber] int line = 0,
        params object[] args);
}
```

**Usage Examples:**

> **Important Note**: Use the `args` parameter for parameterized messages in log methods. Example: `LogInformation("Message: {0}", args: new object?[] { value })`

```csharp
public class PaymentProcessingMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        LogInformation("Starting payment processing for user {0}", args: new object?[] { context.Instance.Data.userId });
        
        try
        {
            var amount = context.Body?.amount;
            if (amount == null || amount <= 0)
            {
                LogWarning("Invalid payment amount received: {0}", args: new object?[] { amount });
                return Task.FromResult(new ScriptResponse
                {
                    Data = new { error = "Invalid amount" }
                });
            }
            
            LogDebug("Processing payment amount: {0}", args: new object?[] { amount });
            
            // Process payment...
            
            LogInformation("Payment processed successfully");
            return Task.FromResult(new ScriptResponse { Data = new { success = true } });
        }
        catch (Exception ex)
        {
            LogError("Payment processing failed: {0}", args: new object?[] { ex.Message });
            throw;
        }
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        LogTrace("OutputHandler called with status code: {0}", args: new object?[] { context.Body?.statusCode });
        return Task.FromResult(new ScriptResponse());
    }
}
```

### Configuration Functions

ScriptBase provides methods to access application configuration values and connection strings. These methods support hierarchical configuration keys using the `:` separator.

**Available Configuration Methods:**

```csharp
public abstract class ScriptBase
{
    // Get configuration value as string (returns null if not found)
    protected static string? GetConfigValue(string key);
    
    // Get configuration value with default fallback
    protected static string GetConfigValue(string key, string defaultValue);
    
    // Get configuration value as specific type
    protected static T? GetConfigValue<T>(string key);
    
    // Get configuration value as specific type with default
    protected static T GetConfigValue<T>(string key, T defaultValue);
    
    // Get connection string by name
    protected static string? GetConnectionString(string name);
    
    // Check if configuration key exists
    protected static bool ConfigExists(string key);
}
```

**Usage Examples:**

```csharp
public class ConfigAwareMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Get simple configuration value
        var apiUrl = GetConfigValue("ExternalApi:BaseUrl");
        
        // Get with default value
        var timeout = GetConfigValue("ExternalApi:Timeout", "30");
        
        // Get typed configuration
        var maxRetries = GetConfigValue<int>("ExternalApi:MaxRetries", 3);
        var enableLogging = GetConfigValue<bool>("Features:EnableDetailedLogging", false);
        
        // Check if configuration exists
        if (ConfigExists("ExternalApi:SecondaryUrl"))
        {
            var secondaryUrl = GetConfigValue("ExternalApi:SecondaryUrl");
            LogInformation("Secondary URL configured: {0}", args: new object?[] { secondaryUrl });
        }
        
        // Get connection string
        var dbConnection = GetConnectionString("DefaultConnection");
        
        if (enableLogging)
        {
            LogDebug("API URL: {0}, Timeout: {1}, Max Retries: {2}", 
                args: new object?[] { apiUrl, timeout, maxRetries });
        }
        
        var httpTask = task as HttpTask;
        httpTask.SetUrl(apiUrl);
        httpTask.SetTimeout(int.Parse(timeout));
        
        return Task.FromResult(new ScriptResponse());
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse());
    }
}
```

**Configuration Key Examples:**

```json
{
  "ExternalApi": {
    "BaseUrl": "https://api.example.com",
    "Timeout": "30",
    "MaxRetries": 3,
    "SecondaryUrl": "https://backup-api.example.com"
  },
  "Features": {
    "EnableDetailedLogging": true
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=mydb;User=sa;Password=xxx"
  }
}
```

Access nested keys using `:` separator: `ExternalApi:BaseUrl`, `Features:EnableDetailedLogging`

## ITransitionMapping Implementation

### Overview

`ITransitionMapping` is a specialized interface for transforming transition payloads before they are merged into workflow instance data. This provides fine-grained control over how transition request data is processed and stored.

> **Source**: [`../../src/ITransitionMapping.cs`](../../src/ITransitionMapping.cs)

### Interface Definition

```csharp
public interface ITransitionMapping 
{
    Task<dynamic> Handler(ScriptContext context);
}
```

### Key Concepts

**Default Behavior (No Mapping):**
- When no mapping is defined, the transition payload is merged directly into instance data as-is
- All properties from the request body are added to `Instance.Data`

**With Mapping:**
- Custom transformation logic is applied to the payload
- You control exactly what gets merged into instance data
- Enables validation, filtering, enrichment, and restructuring

### Usage Examples

#### 1. Basic Payload Transformation

```csharp
public class OrderApprovalTransitionMapping : ScriptBase, ITransitionMapping
{
    public async Task<dynamic> Handler(ScriptContext context)
    {
        LogInformation("Processing order approval transition");
        
        // Transform payload structure
        return new
        {
            approval = new
            {
                approvedBy = context.Body?.userId,
                approvedAt = DateTime.UtcNow,
                comments = context.Body?.comments ?? "No comments",
                status = "approved"
            }
        };
    }
}
```

#### 2. Data Validation and Filtering

```csharp
public class PaymentTransitionMapping : ScriptBase, ITransitionMapping
{
    public async Task<dynamic> Handler(ScriptContext context)
    {
        // Validate required fields
        var amount = context.Body?.amount;
        var currency = context.Body?.currency;
        
        if (amount == null || amount <= 0)
        {
            LogWarning("Invalid payment amount: {0}", args: new object?[] { amount });
            throw new ArgumentException("Valid amount is required");
        }
        
        if (string.IsNullOrEmpty(currency))
        {
            currency = "USD"; // Default currency
            LogDebug("Currency not provided, defaulting to USD");
        }
        
        // Return sanitized and validated data
        return new
        {
            payment = new
            {
                amount = decimal.Parse(amount.ToString()),
                currency = currency.ToString().ToUpper(),
                requestedAt = DateTime.UtcNow,
                status = "pending"
            }
        };
    }
}
```

#### 3. Data Enrichment

```csharp
public class UserActionTransitionMapping : ScriptBase, ITransitionMapping
{
    public async Task<dynamic> Handler(ScriptContext context)
    {
        // Get configuration for enrichment
        var environment = GetConfigValue("Environment", "production");
        var region = GetConfigValue("Deployment:Region", "us-east-1");
        
        // Enrich payload with additional context
        return new
        {
            userAction = new
            {
                action = context.Body?.action,
                userId = context.Body?.userId,
                timestamp = DateTime.UtcNow,
                metadata = new
                {
                    environment = environment,
                    region = region,
                    requestId = context.Headers?.["x-request-id"],
                    userAgent = context.Headers?.["user-agent"]
                }
            }
        };
    }
}
```

#### 4. Conditional Processing

```csharp
public class ConditionalTransitionMapping : ScriptBase, ITransitionMapping
{
    public async Task<dynamic> Handler(ScriptContext context)
    {
        var actionType = context.Body?.actionType?.ToString();
        
        LogDebug("Processing action type: {0}", args: new object?[] { actionType });
        
        return actionType switch
        {
            "approve" => new
            {
                status = "approved",
                approvedBy = context.Body?.userId,
                approvedAt = DateTime.UtcNow
            },
            "reject" => new
            {
                status = "rejected",
                rejectedBy = context.Body?.userId,
                rejectedAt = DateTime.UtcNow,
                reason = context.Body?.reason
            },
            "defer" => new
            {
                status = "deferred",
                deferredBy = context.Body?.userId,
                deferredUntil = context.Body?.deferUntil ?? DateTime.UtcNow.AddDays(1)
            },
            _ => new
            {
                status = "pending",
                message = "Unknown action type"
            }
        };
    }
}
```

### Script Engine

The platform uses the [Roslyn](https://github.com/dotnet/roslyn) compiler for mapping scripts.

#### C# Version Support

> **v0.0.31+**: The mapping script engine now supports **C# 12** features including:

- Collection expressions (`[1, 2, 3]`)
- Primary constructors
- Inline arrays
- Optional parameters in lambda expressions
- `required` members

#### Default Usings

The following namespaces are automatically available in all mapping scripts:

| Namespace | Description | Since |
|-----------|-------------|-------|
| `System` | Core system types | - |
| `System.Collections.Generic` | Generic collections | - |
| `System.Linq` | LINQ operations | - |
| `System.Threading.Tasks` | Async/await support | - |
| `System.Text.Json` | JSON serialization | v0.0.31+ |

**Example - Using System.Text.Json:**
```csharp
public async Task<ScriptResponse> OutputHandler(ScriptContext context)
{
    var response = context.Body;
    
    // JsonSerializer is available directly (no using required)
    var json = JsonSerializer.Serialize(response);
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    
    return new ScriptResponse
    {
        Data = new { serialized = json }
    };
}
```

---

### Transition Definition with Mapping

> **v0.0.23 Change**: The mapping schema has been updated. The `encoding` field has been added and Native C# code support (NAT) is now available.

**Encoding Options:**
- `B64`: BASE64 encoded code (`location` field is required)
- `NAT`: Native C# code (`location` field is not required - more readable)

**BASE64 Encoding Example (B64):**
```json
{
  "key": "approve-order",
  "source": "pending-approval",
  "target": "approved",
  "triggerType": 0,
  "labels": [
    {
      "language": "en-US",
      "label": "Approve Order"
    }
  ],
  "mapping": {
    "location": "./src/OrderApprovalTransitionMapping.csx",
    "code": "dXNpbmcgU3lzdGVtLlRocmVhZGluZy5UYXNrczsKdXNpbmc...",
    "encoding": "B64"
  }
}
```

**Native Encoding Example (NAT):**
```json
{
  "key": "approve-order",
  "source": "pending-approval",
  "target": "approved",
  "triggerType": 0,
  "labels": [
    {
      "language": "en-US",
      "label": "Approve Order"
    }
  ],
  "mapping": {
    "code": "public class OrderApprovalTransitionMapping : ScriptBase, ITransitionMapping { ... }",
    "encoding": "NAT"
  }
}
```

### Best Practices

1. **Always Validate Input**: Check for null values and validate data types
2. **Use Logging**: Log important operations and errors for debugging
3. **Handle Exceptions**: Wrap risky operations in try-catch blocks
4. **Return camelCase Properties**: Maintain consistency with platform conventions
5. **Keep It Simple**: Don't perform heavy operations; focus on data transformation
6. **Document Expected Payload**: Add comments describing expected input structure

### Common Patterns

**Merge with Existing Data:**
```csharp
public async Task<dynamic> Handler(ScriptContext context)
{
    // Preserve existing data and add new fields
    var existingData = context.Instance.Data;
    
    return new
    {
        preservedField = existingData?.preservedField,
        newField = context.Body?.newField,
        updatedAt = DateTime.UtcNow
    };
}
```

**Array Handling:**
```csharp
public async Task<dynamic> Handler(ScriptContext context)
{
    var items = context.Body?.items ?? new List<object>();
    
    return new
    {
        items = items,
        itemCount = items.Count,
        processedAt = DateTime.UtcNow
    };
}
```

This comprehensive guide provides detailed information on how to use mapping interfaces and how to consume ScriptContext and ScriptResponse classes. The examples are taken from real usage scenarios and also provide guidance on best practices and error management.
