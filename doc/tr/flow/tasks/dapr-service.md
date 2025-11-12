# Dapr Service Task

Dapr Service Task, Dapr service invocation özelliği kullanarak mikroservislere çağrı yapmak için kullanılan görev türüdür. DaprHttpEndpoint Task'a benzer ancak doğrudan Dapr SDK üzerinden service invocation API'sini kullanır.

## Özellikler

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

## Görev Tanımı

### Temel Yapı

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
      "body": {
        "userId": "{{data.userId}}"
      },
      "headers": {
        "Content-Type": "application/json"
      },
      "queryString": "version=v1&include=profile",
      "timeoutSeconds": 30
    }
  }
}
```

### Alanlar

DAPR Service Task'ın config bölümünde aşağıdaki alanlar tanımlanır:

| Alan | Tip | Varsayılan | Açıklama |
|------|-----|------------|----------|
| `appId` | string | - | Hedef service app ID (Zorunlu) |
| `methodName` | string | - | Çağrılacak method/endpoint (Zorunlu) |
| `httpVerb` | string | - | HTTP metodu (Zorunlu) |
| `body` | object | null | Request data |
| `headers` | object | null | HTTP header'ları |
| `queryString` | string | null | Query string parametreleri |
| `timeoutSeconds` | number | 30 | İstek timeout süresi |

## Property Erişimi

DaprServiceTask sınıfında property'ler read-only olarak tanımlanmıştır. Bu property'leri değiştirmek için özel metodlar kullanılmalıdır:

- **AppId**: `SetAppId(string appId)` metoduyla değiştirilir
- **MethodName**: `SetMethodName(string methodName)` metoduyla değiştirilir
- **QueryString**: `SetQueryString(string? queryString)` metoduyla değiştirilir
- **Headers**: `SetHeaders(Dictionary<string, string?> headers)` metoduyla değiştirilir  
- **Body**: `SetBody(dynamic body)` metoduyla değiştirilir
- **HttpVerb**: Read-only (tanım dosyasında ayarlanır)
- **TimeoutSeconds**: Read-only (tanım dosyasında ayarlanır)

## Mapping Örnekleri

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
    
    serviceTask.SetBody(requestData);
    
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

## Standart Yanıt

Dapr Service Task aşağıdaki standart yanıt yapısını döner:

```csharp
{
    "data": {
        "profile": {
            "userId": "123",
            "name": "John Doe",
            "email": "john@example.com"
        },
        "preferences": {
            "language": "tr",
            "timezone": "Europe/Istanbul"
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

## En İyi Uygulamalar

### 1. Service Invocation
```csharp
// ✅ Doğru - SetAppId ve SetMethodName kullanımı
serviceTask.SetAppId("user-service");
serviceTask.SetMethodName("GetUserProfile");

// ❌ Yanlış - Direct assignment mümkün değil
serviceTask.AppId = "user-service";  // Read-only property
```

### 2. Data Preparation  
```csharp
// ✅ Doğru - SetData metoduyla structured object
var requestData = new
{
    userId = context.Instance.Data.userId,
    timestamp = DateTime.UtcNow
};
serviceTask.SetBody(requestData);

// ❌ Yanlış - String serialize etme
serviceTask.SetBody(JsonSerializer.Serialize(data));
```

### 3. Query String Handling
```csharp
// ✅ Doğru - SetQueryString metoduyla
serviceTask.SetQueryString("version=v1&include=profile");

// ✅ Doğru - Conditional query building
var queryParams = new List<string>();
if (context.Instance.Data.includeProfile)
    queryParams.Add("include=profile");
serviceTask.SetQueryString(string.Join("&", queryParams));
```

### 4. Error Handling
```csharp
// ✅ Doğru - Response kontrolü
if (response.isSuccess)
{
    output.Data = new { result = response.data };
}
else
{
    output.Data = new { error = response.errorMessage };
}
```

## Sık Karşılaşılan Sorunlar

### Problem: Service not found
**Çözüm:** App ID ve service discovery configuration'ını kontrol edin

### Problem: Method not allowed
**Çözüm:** HTTP verb ve method name uyumunu kontrol edin

### Problem: Circuit breaker open
**Çözüm:** Downstream service health'ini kontrol edin

### Problem: Authentication failed
**Çözüm:** Token ve RBAC policies'i doğrulayın