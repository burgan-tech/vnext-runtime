# Mapping (Haritalama) Rehberi

Platformda **Haritalama**, görevlere geçilecek verileri ve görev sonucunda oluşan verileri, görevin kullanıldığı bağlamla ilişkilendirmek için kullanılır. Bu döküman, mapping interface'lerinin nasıl kullanılacağı, ScriptContext ve ScriptResponse sınıflarının nasıl tüketileceği konusunda kapsamlı bir rehber sunar.

## İçindekiler
1. [Mapping Türleri](#mapping-türleri)
2. [Interface'ler](#interfaceler)
3. [ScriptContext Sınıfı](#scriptcontext-sınıfı)
4. [ScriptResponse Sınıfı](#scriptresponse-sınıfı)
5. [ScriptBase Kullanımı](#scriptbase-kullanımı)
6. [Implementasyon Örnekleri](#implementasyon-örnekleri)
7. [Timer Mapping Örnekleri](#timer-mapping-örnekleri)
8. [Best Practices](#best-practices)
9. [Hata Yönetimi](#hata-yönetimi)

## Mapping Türleri

Haritalama temelde iki alanda yapılmaktadır:

### InputMapping
Göreve geçilecek verileri hazırlamak için kullanılır. Task çalıştırılmadan önce:
- Input verilerini dönüştürme
- Task konfigürasyonunu ayarlama
- Headers ve authentication bilgilerini ekleme
- Validation işlemleri

### OutputMapping
Görevden dönen verileri işlemek için kullanılır. Task çalıştırıldıktan sonra:
- Response verilerini dönüştürme
- Instance data'ya merge etme
- Error handling
- Audit logging

## Interface'ler

Platform üzerinde tanımlara betik tabanlı kod yazımında kullanılan temel arabirimlerdir. Geliştirilecek betikler için kullanılan teknoloji [Roslyn](https://github.com/dotnet/roslyn) ürünüdür.

### Kaynak Kod Referansları

Interface tanımları ve model sınıfları aşağıdaki dosyalarda bulunmaktadır:
- **Ana Interface'ler**: [`../../src/`](../../src/) klasörü
- **Dokümantasyon Kopyaları**: [`../../src/`](../../src/) klasörü  
- **ScriptContext ve ScriptResponse**: [`../../src/Models.cs`](../../src/Models.cs)

> **Not**: Bu dokümandaki kod örnekleri referans amaçlıdır. Güncel tanımlar için yukarıdaki kaynak dosyaları kontrol edilmelidir.

### IMapping
Genel mapping arayüzüdür. Task'ların input ve output binding'leri için kullanılır.

> **Kaynak**: [`../../src/IMapping.cs`](../../src/IMapping.cs)

**Kullanım Alanları:**
- Task execution öncesi input data hazırlama ve dönüştürme
- Task execution sonrası output data işleme
- Data validation ve transformation
- Audit logging ve metadata yönetimi

**Metodlar:**
```csharp
Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context);
Task<ScriptResponse> OutputHandler(ScriptContext context);
```

**Metod Açıklamaları:**
- `InputHandler`: Task çalıştırılmadan önce input verilerini hazırlar, WorkflowTask objesini konfigüre eder
- `OutputHandler`: Task çalıştırıldıktan sonra output verilerini işler ve workflow instance'a merge eder

### ITimerMapping
Schedule mapping için kullanılır. Timer tabanlı iş akışları ve zamanlama işlemleri için özel arayüz.

> **Kaynak**: [`../../src/ITimerMapping.cs`](../../src/ITimerMapping.cs)

**Kullanım Alanları:**
- DateTime tabanlı zamanlama
- Cron expression ile periyodik işlemler
- Duration tabanlı geciktirme
- Immediate execution (anında çalıştırma)
- Business logic tabanlı zamanlama hesaplamaları

**Metod:**
```csharp
Task<TimerSchedule> Handler(ScriptContext context);
```

**Metod Açıklaması:**
- `Handler`: Script context'e göre timer schedule hesaplar ve TimerSchedule objesi döner

### ISubProcessMapping
Sub process için input binding'i için kullanılır. Bağımsız çalışan alt süreçlerin başlatılması için.

> **Kaynak**: [`../../src/ISubProcessMapping.cs`](../../src/ISubProcessMapping.cs)

**Kullanım Alanları:**
- Background data processing
- Audit log generation
- External system notifications
- Data synchronization
- Fire-and-forget operations

**Metod:**
```csharp
Task<ScriptResponse> InputHandler(ScriptContext context);
```

**Metod Açıklaması:**
- `InputHandler`: Subprocess başlatmak için gerekli input verilerini hazırlar ve ScriptResponse döner

**Not:** Subprocess'ler bağımsız çalıştığı için sadece input binding gerekir, output binding yoktur.

### ISubFlowMapping
Sub flow için input binding'i ve tamamlandığında bir üst flow datası aktarmak için output binding için kullanılır.

> **Kaynak**: [`../../src/ISubFlowMapping.cs`](../../src/ISubFlowMapping.cs)

**Kullanım Alanları:**
- Approval workflows
- Data processing workflows
- Validation processes
- Computed value generation
- Parent-child workflow integration

**Metodlar:**
```csharp
Task<ScriptResponse> InputHandler(ScriptContext context);
Task<ScriptResponse> OutputHandler(ScriptContext context);
```

**Metod Açıklamaları:**
- `InputHandler`: Subflow başlatmak için input verilerini hazırlar ve ScriptResponse döner
- `OutputHandler`: Subflow tamamlandığında sonuçları parent workflow'a aktarır ve ScriptResponse döner

**Not:** Subflow'lar parent workflow ile entegre çalıştığı için hem input hem output binding gerekir.

### IConditionMapping
Auto transition gibi karar kısımları için kullanılır. Otomatik geçişlerde koşul kontrolü sağlar.

> **Kaynak**: [`../../src/IConditionMapping.cs`](../../src/IConditionMapping.cs)

**Kullanım Alanları:**
- Data validation checks
- Business rule validation
- Time-based conditions
- External system status checks
- User role/permission verification
- Auto-transition decision logic

**Metod:**
```csharp
Task<bool> Handler(ScriptContext context);
```

**Metod Açıklaması:**
- `Handler`: Verilen context'e göre boolean değer döner (true: geçişe izin ver, false: geçişi engelle)

## ScriptContext Sınıfı

`ScriptContext`, script execution sırasında mevcut olan tüm bağlamsal bilgileri içerir. Bu sınıf, workflow instance'ı, transition bilgileri, request data'sı ve diğer metadata'yı script'lere sunar.

> **Kaynak**: [`../../src/Models.cs`](../../src/Models.cs) - `ScriptContext` sınıfı (satır 112-599)

### Temel Özellikler

```csharp
public sealed class ScriptContext
{
    // Request bilgileri (camelCase formatında otomatik dönüştürülür)
    public dynamic? Body { get; private set; }
    public dynamic? Headers { get; private set; }
    public dynamic? QueryParameters { get; private set; }
    public dynamic? RouteValues { get; private set; }
    
    // Workflow instance bilgileri
    public Instance Instance { get; private set; }
    public Definitions.Workflow Workflow { get; private set; }
    public Transition Transition { get; private set; }
    
    // Runtime bilgileri
    public IRuntimeInfoProvider Runtime { get; private set; }
    public Dictionary<string, dynamic> Definitions { get; private set; }
    public Dictionary<string, dynamic?> TaskResponse { get; private set; }
    public Dictionary<string, dynamic> MetaData { get; private set; }
    
    // Builder pattern desteği
    public sealed class Builder { /* ... */ }
}
```

### Önemli Özellikler

#### Body
Request payload data'sını içerir. Transition request'lerinden gelen data veya task execution sonuçları burada bulunur.

```csharp
// Body'den veri okuma
var userId = context.Body?.userId;
var amount = context.Body?.amount;
```

#### Headers
Request header bilgilerini içerir. Tutarlılık sağlamak için tüm header isimleri **lower-case** formatına dönüştürülür.

> **Önemli**: Request Header'lar tutarlılık sağlamak için her zaman **lower-case** olarak dönüştürülüp tutulur.

```csharp
// Header'lardan veri okuma (lower-case formatında)
var authorization = context.Headers?.authorization;
var contentType = context.Headers?.["content-type"];
var userAgent = context.Headers?.["user-agent"];
```

#### QueryParameters
Function task'larından gelen query string parametrelerini içerir. Bu özellik **sadece Function task'ları kullanıldığında** kullanılabilir ve function endpoint'ine geçilen query parametrelerine erişim sağlar.

> **Önemli**: QueryParameters, Function task'larına özgüdür ve indeksleyici söz dizimi ile erişilir.

```csharp
// Query parametrelerinden veri okuma (sadece Function task'larında)
var userId = context.QueryParameters?.["userId"];
var cityId = context.QueryParameters?.["cityId"];
var page = context.QueryParameters?.["page"];
var pageSize = context.QueryParameters?.["pageSize"];
var filter = context.QueryParameters?.["filter"];
```

**Yaygın Kullanım Alanları:**
- Function task handler'larında özel query parametrelerine erişim
- Function'lara geçilen filtre parametrelerini okuma
- Function çağrılarından pagination parametrelerini alma
- Query string'lerinden kullanıcıya özel tanımlayıcıları çıkarma

**Function Task Mapping'inde Örnek:**
```csharp
public class FunctionTaskMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Function çağrısından query parametrelerine erişim
        var userId = context.QueryParameters?.["userId"];
        var cityId = context.QueryParameters?.["cityId"];
        
        // Function mantığında kullanım
        LogInformation("Function işleniyor - userId: {0}, cityId: {1}", 
            args: new object?[] { userId, cityId });
        
        return Task.FromResult(new ScriptResponse());
    }
}
```

#### Instance.Data
Workflow instance'ının mevcut data'sını içerir. Workflow boyunca biriken tüm bilgiler burada saklanır.

> **Önemli**: `Instance.Data` her zaman **camelCase** formatında veri tutar. Tüm property isimleri camelCase'e dönüştürülür.

```csharp
// Instance data'dan veri okuma (camelCase formatında)
var userInfo = context.Instance.Data.userInfo;
var paymentSchedule = context.Instance.Data.paymentSchedule;
var currentLogin = context.Instance.Data.login.currentLogin;
```

#### TaskResponse
Tamamlanan task'ların sonuçlarını içerir. Output handler'larda task sonuçlarını işlemek için kullanılır.

```csharp
// Task response'larından veri okuma
var httpTaskResult = context.TaskResponse["httpTask"];
var scriptTaskResult = context.TaskResponse["scriptTask"];
```

## ScriptResponse Sınıfı

`ScriptResponse`, mapping interface'lerinden dönen standart response modelidir. Task audit data'sı, instance merge data'sı ve metadata bilgilerini taşır.

> **Kaynak**: [`../../src/Models.cs`](../../src/Models.cs) - `ScriptResponse` sınıfı (satır 23-63)

### Yapısı

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

### Özellik Açıklamaları

- **Key**: Response'u tanımlayan benzersiz anahtar
- **Data**: Ana veri payload'ı (instance'a merge edilecek data)
- **Headers**: HTTP headers veya metadata headers
- **RouteValues**: Routing parametreleri
- **Tags**: Kategorilendirme ve filtreleme için etiketler

### ⚠️ Önemli Uyarılar - ScriptResponse.Data

> **Kritik**: `ScriptResponse.Data` property'si **dynamic** tipinde olduğu için runtime'da compile hataları sık karşılaşılan bir durumdur.

**Dikkat Edilmesi Gereken Durumlar:**

1. **Type Safety**: Dynamic objeler compile-time'da kontrol edilmez
   ```csharp
   // ❌ Hatalı - runtime'da hata verebilir
   response.Data = new { invalidProperty = someComplexObject };
   
   // ✅ Doğru - basit tipler kullan
   response.Data = new { success = true, userId = 123 };
   ```

2. **Null Reference Kontrolü**: Dynamic property'lerde null kontrolü yapın
   ```csharp
   // ❌ Hatalı - null reference exception riski
   var result = context.Body.data.result;
   
   // ✅ Doğru - null-safe erişim
   var result = context.Body?.data?.result;
   ```

3. **Serialization Sorunları**: Karmaşık objeler serialize edilemeyebilir
   ```csharp
   // ❌ Hatalı - serialize edilemeyebilir
   response.Data = new { complexObject = new SomeComplexClass() };
   
   // ✅ Doğru - serializable objeler kullan
   response.Data = new { 
       id = obj.Id, 
       name = obj.Name, 
       timestamp = DateTime.UtcNow 
   };
   ```

4. **Property İsimlendirme**: camelCase kullanın
   ```csharp
   // ❌ Hatalı - PascalCase
   response.Data = new { UserId = 123, PaymentStatus = "success" };
   
   // ✅ Doğru - camelCase
   response.Data = new { userId = 123, paymentStatus = "success" };
   ```

### Kullanım Örnekleri

```csharp
// Basit data response
return new ScriptResponse
{
    Data = new { success = true, userId = 123 }
};

// Headers ile response
return new ScriptResponse
{
    Data = requestData,
    Headers = new { Authorization = "Bearer " + token },
    Tags = new[] { "authentication", "success" }
};
```

## ScriptBase Kullanımı

`ScriptBase` sınıfı, script'lerde sık kullanılan fonksiyonlara kolay erişim sağlar. Özellikle DAPR secret store entegrasyonu için kullanışlıdır.

### Temel Fonksiyonlar

```csharp
public abstract class ScriptBase
{
    // Secret store fonksiyonları
    protected string GetSecret(string storeName, string secretStore, string secretKey);
    protected async Task<string> GetSecretAsync(string storeName, string secretStore, string secretKey);
    protected Dictionary<string, string> GetSecrets(string storeName, string secretStore);
    
    // Property kontrol fonksiyonları (yeni eklenen)
    protected static bool HasProperty(object obj, string propertyName);
    protected static object? GetPropertyValue(object obj, string propertyName);
    
    // Tipli property değeri alma (yeni eklenen)
    protected T? GetPropertyValue<T>(object obj, string propertyName);
}
```

> **Yeni Eklenen Metodlar**: `HasProperty`, `GetPropertyValue` ve generic `GetPropertyValue<T>` metodları ScriptBase'e eklenmiştir. Bu metodlar dynamic objeler üzerinde güvenli property erişimi ve tip dönüşümü sağlar.

### Kullanım Örneği

```csharp
public class MyMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Secret store'dan API key alma
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

### HasProperty ve GetPropertyValue Kullanımı

```csharp
public class SafePropertyAccessMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Güvenli property kontrolü
        if (HasProperty(context.Instance.Data, "card"))
        {
            var cardData = GetPropertyValue(context.Instance.Data, "card");
            
            if (HasProperty(cardData, "products"))
            {
                var productsData = GetPropertyValue(cardData, "products");
                // Products ile işlem yap
            }
        }
        
        // Generic tip dönüşümü ile güvenli erişim
        var userId = GetPropertyValue<int>(context.Instance.Data, "userId");
        var amount = GetPropertyValue<decimal>(context.Body, "amount");
        var isActive = GetPropertyValue<bool>(context.Instance.Data, "isActive");
        
        if (userId.HasValue && amount.HasValue)
        {
            LogInformation("Kullanıcı {0} için {1} tutarında işlem", 
                args: new object?[] { userId.Value, amount.Value });
        }
        
        return Task.FromResult(new ScriptResponse());
    }
}
```

### Loglama Fonksiyonları

ScriptBase, otomatik çağıran bilgisi yakalama ile kapsamlı loglama fonksiyonları sağlar. Tüm loglama metodları `CallerFilePath`, `CallerMemberName` ve `CallerLineNumber` attribute'larını kullanarak log'lara otomatik olarak kaynak konum bilgisini ekler.

**Mevcut Log Seviyeleri:**

```csharp
public abstract class ScriptBase
{
    // Trace seviyesi - çok detaylı tanı bilgileri
    protected static void LogTrace(object message, 
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? method = null,
        [CallerLineNumber] int line = 0,
        params object[] args);

    // Debug seviyesi - dahili sistem olayları
    protected static void LogDebug(object message,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? method = null,
        [CallerLineNumber] int line = 0,
        params object[] args);

    // Information seviyesi - genel bilgilendirme mesajları
    protected static void LogInformation(object message,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? method = null,
        [CallerLineNumber] int line = 0,
        params object[] args);

    // Warning seviyesi - anormal veya beklenmeyen olaylar
    protected static void LogWarning(object message,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? method = null,
        [CallerLineNumber] int line = 0,
        params object[] args);

    // Error seviyesi - hata olayları
    protected static void LogError(object message,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? method = null,
        [CallerLineNumber] int line = 0,
        params object[] args);

    // Critical seviyesi - kritik hatalar
    protected static void LogCritical(object message,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? method = null,
        [CallerLineNumber] int line = 0,
        params object[] args);
}
```

**Kullanım Örnekleri:**

> **Önemli Not**: Log metodlarında parametreli mesajlar için `args` parametresini kullanın. Örnek: `LogInformation("Mesaj: {0}", args: new object?[] { değer })`

```csharp
public class PaymentProcessingMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        LogInformation("Kullanıcı için ödeme işlemi başlatılıyor: {0}", args: new object?[] { context.Instance.Data.userId });
        
        try
        {
            var amount = context.Body?.amount;
            if (amount == null || amount <= 0)
            {
                LogWarning("Geçersiz ödeme tutarı alındı: {0}", args: new object?[] { amount });
                return Task.FromResult(new ScriptResponse
                {
                    Data = new { error = "Geçersiz tutar" }
                });
            }
            
            LogDebug("Ödeme tutarı işleniyor: {0}", args: new object?[] { amount });
            
            // Ödeme işlemi...
            
            LogInformation("Ödeme başarıyla işlendi");
            return Task.FromResult(new ScriptResponse { Data = new { success = true } });
        }
        catch (Exception ex)
        {
            LogError("Ödeme işlemi başarısız oldu: {0}", args: new object?[] { ex.Message });
            throw;
        }
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        LogTrace("OutputHandler çağrıldı, durum kodu: {0}", args: new object?[] { context.Body?.statusCode });
        return Task.FromResult(new ScriptResponse());
    }
}
```

### Konfigürasyon Fonksiyonları

ScriptBase, uygulama konfigürasyon değerlerine ve connection string'lere erişim için metodlar sağlar. Bu metodlar `:` ayırıcısı kullanarak hiyerarşik konfigürasyon key'lerini destekler.

**Mevcut Konfigürasyon Metodları:**

```csharp
public abstract class ScriptBase
{
    // String olarak konfigürasyon değeri al (bulunamazsa null döner)
    protected static string? GetConfigValue(string key);
    
    // Varsayılan değer ile konfigürasyon değeri al
    protected static string GetConfigValue(string key, string defaultValue);
    
    // Belirli tip olarak konfigürasyon değeri al
    protected static T? GetConfigValue<T>(string key);
    
    // Varsayılan değer ile belirli tip olarak konfigürasyon değeri al
    protected static T GetConfigValue<T>(string key, T defaultValue);
    
    // İsme göre connection string al
    protected static string? GetConnectionString(string name);
    
    // Konfigürasyon key'inin var olup olmadığını kontrol et
    protected static bool ConfigExists(string key);
}
```

**Kullanım Örnekleri:**

```csharp
public class ConfigAwareMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Basit konfigürasyon değeri alma
        var apiUrl = GetConfigValue("ExternalApi:BaseUrl");
        
        // Varsayılan değer ile alma
        var timeout = GetConfigValue("ExternalApi:Timeout", "30");
        
        // Tipli konfigürasyon alma
        var maxRetries = GetConfigValue<int>("ExternalApi:MaxRetries", 3);
        var enableLogging = GetConfigValue<bool>("Features:EnableDetailedLogging", false);
        
        // Konfigürasyonun var olup olmadığını kontrol et
        if (ConfigExists("ExternalApi:SecondaryUrl"))
        {
            var secondaryUrl = GetConfigValue("ExternalApi:SecondaryUrl");
            LogInformation("Secondary URL yapılandırıldı: {0}", args: new object?[] { secondaryUrl });
        }
        
        // Connection string al
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

**Konfigürasyon Key Örnekleri:**

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

İç içe key'lere `:` ayırıcısı ile erişin: `ExternalApi:BaseUrl`, `Features:EnableDetailedLogging`

## ITransitionMapping Implementasyonu

### Genel Bakış

`ITransitionMapping`, transition payload'larının workflow instance data'sına merge edilmeden önce dönüştürülmesi için özelleştirilmiş bir interface'dir. Bu, transition request verilerinin nasıl işlenip saklanacağı üzerinde hassas kontrol sağlar.

> **Kaynak**: [`../../src/ITransitionMapping.cs`](../../src/ITransitionMapping.cs)

### Interface Tanımı

```csharp
public interface ITransitionMapping 
{
    Task<dynamic> Handler(ScriptContext context);
}
```

### Temel Kavramlar

**Varsayılan Davranış (Mapping Yok):**
- Mapping tanımı olmadığında, transition payload doğrudan instance data'ya olduğu gibi merge edilir
- Request body'deki tüm property'ler `Instance.Data`'ya eklenir

**Mapping ile:**
- Payload'a özel dönüştürme mantığı uygulanır
- Instance data'ya neyin merge edileceğini tam olarak kontrol edersiniz
- Validasyon, filtreleme, zenginleştirme ve yeniden yapılandırma sağlar

### Kullanım Örnekleri

#### 1. Temel Payload Dönüşümü

```csharp
public class OrderApprovalTransitionMapping : ScriptBase, ITransitionMapping
{
    public async Task<dynamic> Handler(ScriptContext context)
    {
        LogInformation("Sipariş onay transition'ı işleniyor");
        
        // Payload yapısını dönüştür
        return new
        {
            approval = new
            {
                approvedBy = context.Body?.userId,
                approvedAt = DateTime.UtcNow,
                comments = context.Body?.comments ?? "Yorum yok",
                status = "approved"
            }
        };
    }
}
```

#### 2. Veri Validasyonu ve Filtreleme

```csharp
public class PaymentTransitionMapping : ScriptBase, ITransitionMapping
{
    public async Task<dynamic> Handler(ScriptContext context)
    {
        // Zorunlu alanları doğrula
        var amount = context.Body?.amount;
        var currency = context.Body?.currency;
        
        if (amount == null || amount <= 0)
        {
            LogWarning("Geçersiz ödeme tutarı: {0}", args: new object?[] { amount });
            throw new ArgumentException("Geçerli tutar gereklidir");
        }
        
        if (string.IsNullOrEmpty(currency))
        {
            currency = "TRY"; // Varsayılan para birimi
            LogDebug("Para birimi belirtilmedi, TRY varsayılan olarak ayarlandı");
        }
        
        // Temizlenmiş ve doğrulanmış veriyi döndür
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

#### 3. Veri Zenginleştirme

```csharp
public class UserActionTransitionMapping : ScriptBase, ITransitionMapping
{
    public async Task<dynamic> Handler(ScriptContext context)
    {
        // Zenginleştirme için konfigürasyon al
        var environment = GetConfigValue("Environment", "production");
        var region = GetConfigValue("Deployment:Region", "eu-west-1");
        
        // Payload'ı ek context ile zenginleştir
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

#### 4. Koşullu İşleme

```csharp
public class ConditionalTransitionMapping : ScriptBase, ITransitionMapping
{
    public async Task<dynamic> Handler(ScriptContext context)
    {
        var actionType = context.Body?.actionType?.ToString();
        
        LogDebug("İşlem tipi işleniyor: {0}", args: new object?[] { actionType });
        
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
                message = "Bilinmeyen işlem tipi"
            }
        };
    }
}
```

### Mapping ile Transition Tanımı

```json
{
  "key": "approve-order",
  "source": "pending-approval",
  "target": "approved",
  "triggerType": 0,
  "labels": [
    {
      "language": "tr-TR",
      "label": "Siparişi Onayla"
    }
  ],
  "mapping": {
    "location": "./src/OrderApprovalTransitionMapping.csx",
    "code": "dXNpbmcgU3lzdGVtLlRocmVhZGluZy5UYXNrczsKdXNpbmc..."
  }
}
```

### En İyi Uygulamalar

1. **Her Zaman Input'u Doğrulayın**: Null değerleri kontrol edin ve veri tiplerini doğrulayın
2. **Loglama Kullanın**: Debug için önemli işlemleri ve hataları loglayın
3. **Exception'ları Yönetin**: Riskli işlemleri try-catch bloklarına sarın
4. **camelCase Property'ler Döndürün**: Platform konvansiyonları ile tutarlılığı koruyun
5. **Basit Tutun**: Ağır işlemler yapmayın; veri dönüşümüne odaklanın
6. **Beklenen Payload'ı Dokümante Edin**: Beklenen input yapısını açıklayan yorumlar ekleyin

### Yaygın Kalıplar

**Mevcut Veri ile Merge:**
```csharp
public async Task<dynamic> Handler(ScriptContext context)
{
    // Mevcut veriyi koru ve yeni alanlar ekle
    var existingData = context.Instance.Data;
    
    return new
    {
        preservedField = existingData?.preservedField,
        newField = context.Body?.newField,
        updatedAt = DateTime.UtcNow
    };
}
```

**Dizi İşleme:**
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

## Implementasyon Örnekleri

### 1. HTTP Task Mapping Örneği

E-ticaret sepet işlemleri için mapping örneği:

```csharp
public class AddToCartMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        if (httpTask == null)
            throw new ArgumentException("Task must be of type HttpTask");
            
        // Authentication header ekleme
        var headers = new Dictionary<string, string?>
        {
            ["Authorization"] = $"Bearer {context.Instance.Data?.login?.currentLogin?.accessToken}"
        };
        httpTask.SetHeaders(headers);
        
        // Mevcut sepet kontrolü ve ürün ekleme logic'i
        // ... (detaylı implementasyon)
        
        return Task.FromResult(new ScriptResponse
        {
            Data = new { cart = cartData }
        });
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var statusCode = context.Body?.statusCode ?? 500;
        
        if (statusCode >= 200 && statusCode <= 300)
        {
            return Task.FromResult(new ScriptResponse
            {
                Data = new
                {
                    success = true,
                    cart = new
                    {
                        success = true,
                        id = context.Body?.data?.id,
                        products = context.Body?.data?.products
                    }
                }
            });
        }
        
        // Error handling...
    }
}
```

### 2. Payment Processing Mapping Örneği

Ödeme işlemleri için kapsamlı mapping örneği:

```csharp
public class ProcessPaymentMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        
        // Payment request data hazırlama
        var paymentRequest = new
        {
            scheduleId = context.Instance?.Data?.paymentSchedule?.scheduleId,
            userId = context.Instance?.Data?.userId,
            amount = context.Instance?.Data?.amount,
            currency = context.Instance?.Data?.currency,
            processedAt = DateTime.UtcNow
        };

        httpTask.SetBody(paymentRequest);
        
        // Headers ayarlama
        var headers = new Dictionary<string, string?>
        {
            ["Content-Type"] = "application/json",
            ["X-Payment-Request-Id"] = Guid.NewGuid().ToString()
        };
        httpTask.SetHeaders(headers);

        return Task.FromResult(new ScriptResponse());
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var statusCode = context.Body?.statusCode ?? 500;
        
        if (statusCode >= 200 && statusCode < 300)
        {
            // Başarılı ödeme işlemi
            return new ScriptResponse
            {
                Key = "payment-success",
                Data = new
                {
                    paymentResult = new
                    {
                        status = "success",
                        transactionId = context.Body?.data?.transactionId,
                        processedAt = DateTime.UtcNow
                    }
                },
                Tags = new[] { "payments", "success" }
            };
        }
        
        // Hata durumu işleme
        return new ScriptResponse
        {
            Key = "payment-failure",
            Data = new { error = "Payment failed" },
            Tags = new[] { "payments", "failure" }
        };
    }
}
```

### 3. Condition Mapping Örneği

```csharp
public class AuthorizationSuccessRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        // Authentication başarı durumunu kontrol et
        return context.Instance.Data.authentication?.success == true;
    }
}
```

## Timer Mapping Örnekleri

Timer mapping, workflow'larda zamanlama işlemleri için kullanılır. Dapr-compatible scheduling desteği sunar.

### 1. Temel Timer Örneği

```csharp
public class PaymentDueTimerRule : ITimerMapping
{
    public async Task<TimerSchedule> Handler(ScriptContext context)
    {
        try
        {
            var paymentSchedule = context.Instance.Data.paymentSchedule;
            if (paymentSchedule == null)
                return TimerSchedule.FromDuration(TimeSpan.FromDays(1));

            // Frequency'e göre zamanlama
            var frequency = paymentSchedule.frequency?.ToString().ToLower() ?? "monthly";
            
            return frequency switch
            {
                "daily" => TimerSchedule.FromCronExpression("0 9 * * *"), // Daily at 9 AM
                "weekly" => TimerSchedule.FromCronExpression("0 9 * * 1"), // Weekly on Monday
                "monthly" => TimerSchedule.FromCronExpression("0 9 1 * *"), // Monthly on 1st
                "immediate" => TimerSchedule.Immediate(),
                _ => TimerSchedule.FromDuration(TimeSpan.FromDays(30))
            };
        }
        catch (Exception)
        {
            return TimerSchedule.FromDuration(TimeSpan.FromDays(1)); // Fallback
        }
    }
}
```

### 2. Business Logic Timer Örneği

```csharp
public class BusinessHoursTimerRule : ITimerMapping
{
    public async Task<TimerSchedule> Handler(ScriptContext context)
    {
        var now = DateTime.UtcNow;
        var isWeekend = now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday;
        var hour = now.Hour;
        
        // Business hours dışında ise, bir sonraki iş günü saat 9'da
        if (isWeekend || hour < 9 || hour >= 17)
        {
            return TimerSchedule.FromCronExpression("0 9 * * 1-5"); // Next weekday at 9 AM
        }
        
        // İş saatleri içinde ise 1 saat sonra
        return TimerSchedule.FromDuration(TimeSpan.FromHours(1));
    }
}
```

### 3. Conditional Timer Örneği

```csharp
public class ConditionalTimerRule : ITimerMapping
{
    public async Task<TimerSchedule> Handler(ScriptContext context)
    {
        var amount = context.Instance.Data.amount != null 
            ? Convert.ToDecimal(context.Instance.Data.amount) 
            : 0m;
        
        if (amount > 10000) // Yüksek tutarlı işlemler
        {
            return TimerSchedule.Immediate(); // Hemen işle
        }
        else if (amount > 1000) // Orta tutarlı işlemler
        {
            return TimerSchedule.FromDuration(TimeSpan.FromMinutes(30)); // 30 dakika içinde
        }
        else // Düşük tutarlı işlemler
        {
            return TimerSchedule.FromCronExpression("0 0 * * *"); // Gece yarısı batch işlem
        }
    }
}
```

### Timer Schedule Türleri

```csharp
// DateTime tabanlı
TimerSchedule.FromDateTime(DateTime.UtcNow.AddHours(2))

// Cron expression
TimerSchedule.FromCronExpression("0 9 * * *") // Her gün saat 9

// Duration tabanlı
TimerSchedule.FromDuration(TimeSpan.FromMinutes(30))

// Immediate execution
TimerSchedule.Immediate()

// Predefined schedules
TimerSchedule.Daily
TimerSchedule.Weekly
TimerSchedule.Monthly
TimerSchedule.Yearly
TimerSchedule.Hourly
TimerSchedule.Midnight

// Dapr @ expressions
TimerSchedule.FromExpression("@every 10m")
TimerSchedule.FromExpression("@daily")
TimerSchedule.FromExpression("@hourly")
```

## Best Practices

### 1. Error Handling
```csharp
public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    try
    {
        // Ana logic
        return Task.FromResult(new ScriptResponse { Data = result });
    }
    catch (Exception ex)
    {
        return Task.FromResult(new ScriptResponse
        {
            Key = "error",
            Data = new { error = ex.Message, timestamp = DateTime.UtcNow },
            Tags = new[] { "error", "exception" }
        });
    }
}
```

### 2. Null Safety
```csharp
// Güvenli property erişimi
var userId = context.Instance?.Data?.userId ?? 0;
var amount = context.Body?.amount != null ? Convert.ToDecimal(context.Body.amount) : 0m;

// Null kontrolü ile
if (context.Instance?.Data?.paymentSchedule != null)
{
    var schedule = context.Instance.Data.paymentSchedule;
    // İşlemler...
}
```

### 3. Type Safety
```csharp
// Type casting ile güvenlik
var httpTask = task as HttpTask;
if (httpTask == null)
    throw new ArgumentException("Task must be of type HttpTask");

// Safe conversion
if (int.TryParse(context.Body?.id?.ToString(), out int id))
{
    // id kullan
}
```

### 4. Performance
```csharp
// Async/await kullanımı
public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var apiKey = await GetSecretAsync("store", "secrets", "api_key");
    // Diğer async işlemler...
}
```

### 5. Logging ve Debugging
```csharp
return new ScriptResponse
{
    Key = "operation-result",
    Data = result,
    Tags = new[] { "operation", "success", "user-" + userId }
};
```

## Hata Yönetimi

### 1. HTTP Status Code Handling
```csharp
public Task<ScriptResponse> OutputHandler(ScriptContext context)
{
    var statusCode = context.Body?.statusCode ?? 500;
    
    return statusCode switch
    {
        >= 200 and < 300 => HandleSuccess(context),
        400 => HandleBadRequest(context),
        401 => HandleUnauthorized(context),
        404 => HandleNotFound(context),
        >= 500 => HandleServerError(context),
        _ => HandleUnknownError(context)
    };
}
```

### 2. Retry Logic
```csharp
var retryCount = context.Instance?.Data?.retryCount ?? 0;
var maxRetries = context.Instance?.Data?.maxRetries ?? 3;

if (retryCount < maxRetries)
{
    return new ScriptResponse
    {
        Data = new 
        { 
            shouldRetry = true, 
            retryCount = retryCount + 1,
            retryAfter = TimeSpan.FromMinutes(Math.Pow(2, retryCount)) // Exponential backoff
        }
    };
}
```

### 3. Validation
```csharp
public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var errors = new List<string>();
    
    if (context.Body?.userId == null)
        errors.Add("UserId is required");
        
    if (context.Body?.amount == null || Convert.ToDecimal(context.Body.amount) <= 0)
        errors.Add("Valid amount is required");
    
    if (errors.Any())
    {
        return Task.FromResult(new ScriptResponse
        {
            Key = "validation-error",
            Data = new { errors = errors },
            Tags = new[] { "validation", "error" }
        });
    }
    
    // Ana logic...
}
```

Bu kapsamlı rehber, mapping interface'lerinin nasıl kullanılacağı, ScriptContext ve ScriptResponse sınıflarının nasıl tüketileceği konusunda detaylı bilgi sağlar. Örnekler gerçek kullanım senaryolarından alınmış olup, best practices ve hata yönetimi konularında da rehberlik eder.
