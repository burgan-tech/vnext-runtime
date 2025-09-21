# HTTP Task

HTTP Task, harici web servislerine HTTP istekleri göndermek için kullanılan görev türüdür. Bu görev türü ile REST API'lere, web servislerine ve diğer HTTP endpoint'lerine çağrı yapabilirsiniz.

## Özellikler

- ✅ Tüm HTTP metodları desteklenir (GET, POST, PUT, DELETE, PATCH, vb.)
- ✅ Özel header'lar eklenebilir
- ✅ Request body desteği (JSON, XML, vb.)
- ✅ SSL doğrulama ayarlanabilir
- ✅ Timeout yapılandırılabilir
- ✅ Response header'ları yakalanır
- ✅ Otomatik JSON deserialization
- ✅ Detaylı hata yönetimi
- ✅ Execution duration tracking

## Görev Tanımı

### Temel Yapı

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

### Alanlar

HTTP Task'ın config bölümünde aşağıdaki alanlar tanımlanır:

| Alan | Tip | Varsayılan | Açıklama |
|------|-----|------------|----------|
| `url` | string | - | Hedef URL (Zorunlu) |
| `method` | string | "GET" | HTTP metodu (GET, POST, PUT, DELETE, vb.) |
| `headers` | object | null | HTTP header'ları |
| `body` | object | null | Request body (GET dışında) |
| `timeoutSeconds` | number | 30 | İstek timeout süresi |
| `validateSsl` | boolean | true | SSL sertifika doğrulaması |

## SSL Yapılandırması

### SSL Doğrulaması Etkin (Varsayılan)
```json
{
  "validateSsl": true
}
```

### SSL Doğrulaması Devre Dışı
```json
{
  "validateSsl": false
}
```

:::warning Güvenlik Uyarısı
SSL doğrulamasını sadece development ortamında veya güvenilir internal servislerde devre dışı bırakın.
:::

## Timeout Yapılandırması

```json
{
  "timeoutSeconds": 60
}
```

- Varsayılan: 30 saniye

## Property Erişimi

HttpTask sınıfında bazı property'ler read-only olarak tanımlanmıştır. Bu property'leri değiştirmek için özel metodlar kullanılmalıdır:

- **Url**: `SetUrl(string url)` metoduyla değiştirilir
- **Headers**: `SetHeaders(Dictionary<string, string?> headers)` metoduyla değiştirilir  
- **Body**: `SetBody(dynamic body)` metoduyla değiştirilir
- **Method**: Doğrudan atama yapılabilir
- **TimeoutSeconds**: Read-only (Tanım dosyasında'da ayarlanır)
- **ValidateSSL**: Read-only (Tanım dosyasında'da ayarlanır)

## Mapping Örnekleri

### Input Mapping

```csharp
public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var httpTask = task as HttpTask;
    
    // URL parametrelerini değiştir
    httpTask.SetUrl(httpTask.Url.Replace("{userId}", context.Instance.Data.userId.ToString()));
    
    // Authorization header ekle
    var headers = new Dictionary<string, string?>
    {
        ["Content-Type"] = "application/json"
    };
    
    httpTask.SetHeaders(headers);
    
    // Request body'yi hazırla
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
        // Başarılı response işleme
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
        // Hata durumu işleme
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

## Standart Yanıt

HTTP Task aşağıdaki standart yanıt yapısını döner:

```csharp
{
    "Data": { /* API yanıt verisi */ },
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

### Başarılı Yanıt
- `IsSuccess`: true
- `StatusCode`: 2xx HTTP status kodu
- `Data`: Deserialize edilmiş response verisi
- `Headers`: Response header'ları
- `ExecutionDurationMs`: İstek süresi

### Hatalı Yanıt
- `IsSuccess`: false
- `StatusCode`: HTTP hata kodu (4xx, 5xx)
- `ErrorMessage`: Hata açıklaması
- `Metadata`: Ek hata bilgileri

## Hata Senaryoları

### HTTP Hatası (4xx, 5xx)
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

### Network Hatası
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

### Timeout Hatası
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

## En İyi Uygulamalar

### 1. URL Parametreleri
```csharp
// ✅ Doğru - Input mapping'de SetUrl metoduyla değiştir
httpTask.SetUrl(httpTask.Url.Replace("{id}", context.Data.id.ToString()));

// ❌ Yanlış - Sabit URL
httpTask.SetUrl("https://api.com/users/123");
```

### 2. Request Body
```csharp
// ✅ Doğru - SetBody metoduyla structured object
var requestData = new
{
    data = context.Instance.Data.payload,
    metadata = new { timestamp = DateTime.UtcNow }
};
httpTask.SetBody(requestData);

// ❌ Yanlış - String serialize etme
httpTask.SetBody(JsonSerializer.Serialize(data));
```

### 4. Hata Kontrolü
```csharp
// ✅ Doğru - Response kontrol et
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
- Timeout değerlerini gerçekçi ayarlayın
- Gereksiz header'lar eklemeyin
- Response data'yı gerektiği kadar işleyin


## Sık Karşılaşılan Sorunlar

### Problem: SSL Certificate Error
**Çözüm:** `validateSsl: false` ayarlayın (sadece development için)

### Problem: Timeout
**Çözüm:** `timeoutSeconds` değerini artırın

### Problem: 401 Unauthorized
**Çözüm:** Authorization header'ını kontrol edin

### Problem: JSON Parse Error
**Çözüm:** Response content-type'ını ve formatını kontrol edin
