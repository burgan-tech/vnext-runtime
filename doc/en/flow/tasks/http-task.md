# HTTP Task

HTTP Task is a task type used to send HTTP requests to external web services. With this task type, you can make calls to REST APIs, web services, and other HTTP endpoints.

## Features

- ✅ All HTTP methods are supported (GET, POST, PUT, DELETE, PATCH, etc.)
- ✅ Custom headers can be added
- ✅ Request body support (JSON, XML, etc.)
- ✅ SSL validation can be configured
- ✅ Timeout can be configured
- ✅ Response headers are captured
- ✅ Automatic JSON deserialization
- ✅ Detailed error management
- ✅ Execution duration tracking

## Task Definition

### Basic Structure

```json
{
  "key": "get-user-info",
  "flow": "sys-tasks",
  "domain": "core",
  "version": "1.0.0",
  "tags": [
    "users",
    "lookup",
    "information",
    "devices"
  ],
  "attributes": {
    "type": "6",
    "config": {
      "method": "GET",
      "url": "http://mockoon:3001/api/payments/users/{userId}",
      "headers": {
        "Content-Type": "application/json"
      },
      "timeoutSeconds": 30,
      "validateSsl": true
    }
  }
}
```

### Fields

The following fields are defined in the config section of HTTP Task:

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `url` | string | - | Target URL (Required) |
| `method` | string | "GET" | HTTP method (GET, POST, PUT, DELETE, etc.) |
| `headers` | object | null | HTTP headers |
| `body` | object | null | Request body (except GET) |
| `timeoutSeconds` | number | 30 | Request timeout duration |
| `validateSsl` | boolean | true | SSL certificate validation |

## SSL Configuration

### SSL Validation Enabled (Default)
```json
{
  "validateSsl": true
}
```

### SSL Validation Disabled
```json
{
  "validateSsl": false
}
```

:::warning Security Warning
Disable SSL validation only in development environment or trusted internal services.
:::

## Timeout Configuration

```json
{
  "timeoutSeconds": 60
}
```

- Default: 30 seconds

## Property Access

Some properties in the HttpTask class are defined as read-only. Special methods must be used to change these properties:

- **Url**: Changed with `SetUrl(string url)` method
- **Headers**: Changed with `SetHeaders(Dictionary<string, string?> headers)` method  
- **Body**: Changed with `SetBody(dynamic body)` method
- **Method**: Direct assignment can be made
- **TimeoutSeconds**: Read-only (also set in definition file)
- **ValidateSSL**: Read-only (also set in definition file)

## Mapping Examples

### Input Mapping

```csharp
public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var httpTask = task as HttpTask;
    
    // Change URL parameters
    httpTask.SetUrl(httpTask.Url.Replace("{userId}", context.Instance.Data.userId.ToString()));
    
    // Add authorization header
    var headers = new Dictionary<string, string?>
    {
        ["Content-Type"] = "application/json"
    };
    
    httpTask.SetHeaders(headers);
    
    // Prepare request body
    if (httpTask.Method != "GET")
    {
        var requestBody = new
        {
            name = context.Instance.Data.name,
            email = context.Instance.Data.email,
            timestamp = DateTime.UtcNow
        };
        
        httpTask.SetBody(requestBody);
    }
    
    return new ScriptResponse()
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
        // Successful response processing
        var userData = response.data;
        
        output.Data = new
        {
            userId = userData.id,
            created = userData.createdAt,
            status = "created",
            responseHeaders = response.headers,
            executionTime = response.executionDurationMs
        };
    }
    else
    {
        // Error condition processing
        output.Data = new
        {
            error = response.errorMessage,
            statusCode = response.statusCode,
            failed = true
        };
    }
    
    return output;
}
```

## Standard Response

HTTP Task returns the following standard response structure:

```csharp
{
    "Data": { /* API response data */ },
    "StatusCode": 200,
    "IsSuccess": true,
    "ErrorMessage": null,
    "Headers": {
        "content-type": "application/json",
        "content-length": "156"
    },
    "Metadata": {
        "Url": "https://api.example.com/users",
        "Method": "POST",
        "ReasonPhrase": "OK"
    },
    "ExecutionDurationMs": 245,
    "TaskType": "Http"
}
```

### Successful Response
- `IsSuccess`: true
- `StatusCode`: 2xx HTTP status code
- `Data`: Deserialized response data
- `Headers`: Response headers
- `ExecutionDurationMs`: Request duration

### Error Response
- `IsSuccess`: false
- `StatusCode`: HTTP error code (4xx, 5xx)
- `ErrorMessage`: Error description
- `Metadata`: Additional error information

## Error Scenarios

### HTTP Error (4xx, 5xx)
```json
{
  "IsSuccess": false,
  "StatusCode": 404,
  "ErrorMessage": "HTTP request failed with status 404: Not Found",
  "Metadata": {
    "Url": "https://api.example.com/users/999",
    "Method": "GET",
    "ReasonPhrase": "Not Found",
    "ResponseContent": "{\"error\": \"User not found\"}"
  }
}
```

### Network Error
```json
{
  "IsSuccess": false,
  "ErrorMessage": "The remote name could not be resolved: 'invalid-domain.com'",
  "Metadata": {
    "Url": "https://invalid-domain.com/api",
    "Method": "POST"
  }
}
```

### Timeout Error
```json
{
  "IsSuccess": false,
  "ErrorMessage": "HTTP request was cancelled",
  "Metadata": {
    "Url": "https://slow-api.com/endpoint",
    "Method": "GET",
    "Cancelled": true
  }
}
```

## Best Practices

### 1. URL Parameters
```csharp
// ✅ Correct - Change with SetUrl method in input mapping
httpTask.SetUrl(httpTask.Url.Replace("{id}", context.Data.id.ToString()));

// ❌ Wrong - Fixed URL
httpTask.SetUrl("https://api.com/users/123");
```

### 2. Request Body
```csharp
// ✅ Correct - Structured object with SetBody method
var requestData = new
{
    data = context.Instance.Data.payload,
    metadata = new { timestamp = DateTime.UtcNow }
};
httpTask.SetBody(requestData);

// ❌ Wrong - String serialization
httpTask.SetBody(JsonSerializer.Serialize(data));
```

### 4. Error Control
```csharp
// ✅ Correct - Check response
 public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            var statusCode = GetStatusCodeFromResponse(context);
            var responseData = GetResponseDataFromContext(context);

            // Successful check
            if (statusCode >= 200 && statusCode < 300)
            {
                var deviceRegistration = new
                {
                    checkedAt = DateTime.UtcNow,
                    isRegistered = responseData?.isRegistered ?? false
                };

                return new ScriptResponse
                {
                    Data = new
                    {
                        deviceRegistration = deviceRegistration
                    }
                };
            }
            else
            {
                var errorInfo = ExtractErrorInformation(context, statusCode);
                
                var deviceRegistration = new
                {
                    checkedAt = DateTime.UtcNow,
                    isRegistered = false,
                    error = errorInfo.message,
                    errorCode = errorInfo.code,
                    errorDescription = errorInfo.description,
                    statusCode = statusCode
                };

                return new ScriptResponse
                {
                    Data = new
                    {
                        deviceRegistration = deviceRegistration
                    }
                };
            }
        }
        catch (Exception ex)
        {
            return new ScriptResponse
            {
                Data = new
                {
                    deviceRegistration = new
                    {
                        checkedAt = DateTime.UtcNow,
                        isRegistered = false,
                        error = "Internal processing error",
                        errorCode = "processing_error",
                        errorDescription = ex.Message
                    }
                }
            };
        }
    }

 #region Helper Methods

    private static int GetStatusCodeFromResponse(ScriptContext context)
    {
        if (context.Body?.statusCode != null)
            return (int)context.Body.statusCode;

        return 200;
    }

    private static dynamic? GetResponseDataFromContext(ScriptContext context)
    {
        if (context.Body?.data != null)
            return context.Body.data;

        if (context.Body != null && context.Body.statusCode == null)
            return context.Body;

        return null;
    }
    
    private static (string message, string code, string description) ExtractErrorInformation(ScriptContext context, int statusCode)
    {
        var errorData = context.Body?.data?.error ?? null;
        
        string message = "Device validation failed";
        string code = "invalid_device";
        string description = "The device credentials provided are invalid";

        if (errorData != null)
        {
            message = errorData.message ?? message;
            code = errorData.code ?? code;
            description = errorData.description ?? description;
        }
        else if (statusCode == 401)
        {
            code = "unauthorized_device";
            description = "Device authentication failed";
        }
        else if (statusCode == 403)
        {
            code = "access_denied";
            description = "Device access denied";
        }
        else if (statusCode >= 500)
        {
            code = "server_error";
            description = "Internal server error during device validation";
        }

        return (message, code, description);
    }
    

    #endregion
```

### 5. Performance
- Set timeout values realistically
- Don't add unnecessary headers
- Process response data as needed


## Common Problems

### Problem: SSL Certificate Error
**Solution:** Set `validateSsl: false` (only for development)

### Problem: Timeout
**Solution:** Increase the `timeoutSeconds` value

### Problem: 401 Unauthorized
**Solution:** Check the Authorization header

### Problem: JSON Parse Error
**Solution:** Check response content-type and format
