# Dapr Service Task

Dapr Service Task is a task type used to make calls to microservices using the Dapr service invocation feature. It is similar to DaprHttpEndpoint Task but uses the service invocation API directly through the Dapr SDK.

## Features

- ✅ Service-to-service invocation
- ✅ Automatic service discovery
- ✅ Load balancing
- ✅ Circuit breaker
- ✅ Retry policies
- ✅ Distributed tracing
- ✅ Security (mTLS, RBAC)
- ✅ API versioning
- ✅ Request/response interception
- ✅ Middleware support

## Task Definition

### Basic Structure

```json
{
  "key": "invoke-user-service",
  "flow": "sys-tasks",
  "domain": "core", 
  "version": "1.0.0",
  "tags": [
    "service",
    "invocation",
    "user"
  ],
  "attributes": {
    "type": "3",
    "config": {
      "appId": "user-service",
      "methodName": "GetUserProfile", 
      "httpVerb": "GET",
      "data": {
        "userId": "{{data.userId}}"
      },
      "queryString": "version=v1&include=profile",
      "timeoutSeconds": 30
    }
  }
}
```

### Fields

The following fields are defined in the config section of DAPR Service Task:

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `appId` | string | - | Target service app ID (Required) |
| `methodName` | string | - | Method/endpoint to call (Required) |
| `httpVerb` | string | - | HTTP method (Required) |
| `data` | object | null | Request data |
| `queryString` | string | null | Query string parameters |
| `timeoutSeconds` | number | 30 | Request timeout duration |

## Property Access

Properties in the DaprServiceTask class are defined as read-only. Special methods must be used to change these properties:

- **AppId**: Changed with `SetAppId(string appId)` method
- **MethodName**: Changed with `SetMethodName(string methodName)` method
- **QueryString**: Changed with `SetQueryString(string? queryString)` method
- **Data**: Changed with `SetData(dynamic data)` method
- **HttpVerb**: Read-only (set in definition file)
- **TimeoutSeconds**: Read-only (set in definition file)

## Mapping Examples

### Input Mapping

```csharp
public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var serviceTask = task as DaprServiceTask;
    
    // Dynamic service selection
    if (context.Instance.Data.userType == "premium")
    {
        serviceTask.SetAppId("premium-user-service");
    }
    else
    {
        serviceTask.SetAppId("standard-user-service");
    }
    
    // Environment-based method selection
    var environment = context.GetConfiguration("Environment");
    if (environment == "development")
    {
        serviceTask.SetMethodName($"debug/users/{context.Instance.Data.userId}");
    }
    else
    {
        serviceTask.SetMethodName($"users/{context.Instance.Data.userId}");
    }
    
    // Query string setup
    var queryParams = new List<string>();
    if (context.Instance.Data.includeProfile)
        queryParams.Add("include=profile");
    if (context.Instance.Data.version != null)
        queryParams.Add($"version={context.Instance.Data.version}");
    
    if (queryParams.Any())
        serviceTask.SetQueryString(string.Join("&", queryParams));
    
    // Request data preparation
    var requestData = new
    {
        userId = context.Instance.Data.userId,
        includeProfile = true,
        includePreferences = context.Instance.Data.includePreferences ?? false,
        timestamp = DateTime.UtcNow,
        correlationId = context.Instance.Data.correlationId
    };
    
    serviceTask.SetData(requestData);
    
    return new ScriptResponse();
}
```

### Output Mapping

```csharp
public async Task<ScriptResponse> OutputHandler(ScriptContext context)
{
    var output = new ScriptResponse();
    var response = context.Body;
    
    if (response.isSuccess)
    {
        var serviceResponse = response.data;
        
        output.Data = new
        {
            userProfile = serviceResponse.profile,
            preferences = serviceResponse.preferences,
            lastUpdated = serviceResponse.lastUpdated,
            
            // Service metadata
            serviceInfo = new
            {
                appId = response.metadata?.appId,
                methodName = response.metadata?.methodName,
                httpVerb = response.metadata?.httpVerb,
                responseTime = response.executionDurationMs,
                taskType = response.taskType
            },
            
            // Processing timestamp
            processedAt = DateTime.UtcNow
        };
    }
    else
    {
        // Error handling
        output.Data = new
        {
            serviceCallFailed = true,
            error = response.errorMessage,
            
            // Error classification
            errorType = ClassifyServiceError(response.errorMessage),
            retryable = IsRetryableServiceError(response.errorMessage),
            
            // Service info for debugging
            serviceInfo = new
            {
                appId = response.metadata?.appId,
                methodName = response.metadata?.methodName,
                httpVerb = response.metadata?.httpVerb
            },
            
            // Processing timestamp
            failedAt = DateTime.UtcNow
        };
    }
    
    return output;
}

private string ClassifyServiceError(string errorMessage)
{
    if (errorMessage.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        return "timeout";
    if (errorMessage.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
        return "authentication";
    if (errorMessage.Contains("forbidden", StringComparison.OrdinalIgnoreCase))
        return "authorization";
    if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        return "not-found";
    if (errorMessage.Contains("service unavailable", StringComparison.OrdinalIgnoreCase))
        return "service-unavailable";
    
    return "general-error";
}

private bool IsRetryableServiceError(string errorMessage)
{
    return errorMessage.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
           errorMessage.Contains("service unavailable", StringComparison.OrdinalIgnoreCase) ||
           errorMessage.Contains("connection", StringComparison.OrdinalIgnoreCase);
}
```

## Standard Response

Dapr Service Task returns the following standard response structure:

```csharp
{
    "data": {
        "profile": {
            "userId": "123",
            "name": "John Doe",
            "email": "john@example.com"
        },
        "preferences": {
            "language": "en",
            "timezone": "America/New_York"
        }
    },
    "isSuccess": true,
    "errorMessage": null,
    "metadata": {
        "appId": "user-service",
        "methodName": "GetUserProfile",
        "httpVerb": "GET"
    },
    "executionDurationMs": 120,
    "taskType": "DaprServiceTask"
}
```

## Best Practices

### 1. Service Invocation
```csharp
// ✅ Correct - Using SetAppId and SetMethodName
serviceTask.SetAppId("user-service");
serviceTask.SetMethodName("GetUserProfile");

// ❌ Wrong - Direct assignment not possible
serviceTask.AppId = "user-service";  // Read-only property
```

### 2. Data Preparation  
```csharp
// ✅ Correct - Structured object with SetData method
var requestData = new
{
    userId = context.Instance.Data.userId,
    timestamp = DateTime.UtcNow
};
serviceTask.SetData(requestData);

// ❌ Wrong - String serialization
serviceTask.SetData(JsonSerializer.Serialize(data));
```

### 3. Query String Handling
```csharp
// ✅ Correct - With SetQueryString method
serviceTask.SetQueryString("version=v1&include=profile");

// ✅ Correct - Conditional query building
var queryParams = new List<string>();
if (context.Instance.Data.includeProfile)
    queryParams.Add("include=profile");
serviceTask.SetQueryString(string.Join("&", queryParams));
```

### 4. Error Handling
```csharp
// ✅ Correct - Response check
if (response.isSuccess)
{
    output.Data = new { result = response.data };
}
else
{
    output.Data = new { error = response.errorMessage };
}
```

## Common Problems

### Problem: Service not found
**Solution:** Check App ID and service discovery configuration

### Problem: Method not allowed
**Solution:** Check HTTP verb and method name compatibility

### Problem: Circuit breaker open
**Solution:** Check downstream service health

### Problem: Authentication failed
**Solution:** Verify token and RBAC policies
