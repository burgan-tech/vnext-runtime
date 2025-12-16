# Extension Yönetimi

Extension'lar, vNext platformunda instance verilerinin zenginleştirilmesi için kullanılan bileşenlerdir. Function'lar gibi task çalıştırabilirler, ancak extension'lar instance data response'larına yansır ve dış katmanlara uç nokta sağlamazlar.

## İçindekiler

1. [Genel Bakış](#genel-bakış)
2. [Extension Tanımı](#extension-tanımı)
3. [Extension Tipleri](#extension-tipleri)
4. [Extension Kapsamları](#extension-kapsamları)
5. [Kullanım Örnekleri](#kullanım-örnekleri)
6. [En İyi Uygulamalar](#en-iyi-uygulamalar)

---

## Genel Bakış

Extension'lar şu amaçlarla kullanılır:

- **Veri Zenginleştirme**: Instance verilerine ek bağlam bilgisi ekler
- **Task Çalıştırma**: Function'lar gibi task çalıştırabilir
- **Response Zenginleştirme**: Instance data response'larına `extensions` objesi altında veri ekler
- **Dinamik Veri**: Gerçek zamanlı hesaplanan veya harici kaynaklardan alınan verileri sağlar

:::highlight green 💡
Extension'lar function'lardan farklı olarak dış katmanlara uç nokta sağlamaz. Sadece instance veri response'larını zenginleştirir.
:::

### Extension vs Function Karşılaştırması

| Özellik | Extension | Function |
|---------|-----------|----------|
| Task çalıştırma | ✅ Evet | ✅ Evet |
| Dış uç nokta sağlama | ❌ Hayır | ✅ Evet |
| Response'a yansıma | ✅ `extensions` objesi altında | ❌ Ayrı endpoint |
| Kullanım amacı | Veri zenginleştirme | API uç noktası |

---

## Extension Tanımı

### Temel Yapı

```json
{
  "key": "extension-user-session",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-extensions",
  "flowVersion": "1.0.0",
  "tags": [
    "system",
    "core",
    "sys-extensions",
    "components"
  ],
  "attributes": {
    "type": 2,
    "scope": 1,
    "task": {
      "order": 1,
      "task": {
        "key": "user-session",
        "domain": "core",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "location": "./src/UserSessionMapping.csx",
        "code": "<BASE64_ENCODED_MAPPING_CODE>"
      }
    }
  }
}
```

### Temel Özellikler

| Özellik | Tip | Açıklama |
|---------|-----|----------|
| `key` | `string` | Extension için benzersiz tanımlayıcı |
| `version` | `string` | Versiyon bilgisi (semantic versioning) |
| `domain` | `string` | Extension'ın ait olduğu domain |
| `flow` | `string` | Flow stream bilgisi (varsayılan: `sys-extensions`) |
| `flowVersion` | `string` | Flow versiyon bilgisi |
| `tags` | `string[]` | Kategorilendirme ve arama için etiketler |
| `attributes` | `object` | Extension konfigürasyonu |

### Attributes Özellikleri

| Özellik | Tip | Açıklama |
|---------|-----|----------|
| `type` | `number` | Extension tipi (1-4 arası) |
| `scope` | `number` | Extension kapsamı (1-3 arası) |
| `task` | `object` | Çalıştırılacak task tanımı |

---

## Extension Tipleri

Extension'ların ne zaman ve hangi akışlarda çalışacağını belirler.

### ExtensionType Enum

```csharp
public enum ExtensionType
{
    /// <summary>
    /// Kayıt örnekleri tüm akışlarda dönerken çalışacak extension
    /// </summary>
    Global = 1,

    /// <summary>
    /// Tüm akışlarda ve kayıt örnekleri istendiğinde çalışacak extension
    /// </summary>
    GlobalAndRequested = 2,

    /// <summary>
    /// Sadece tanımlandığı akışlarda çalışacak extension
    /// </summary>
    DefinedFlows = 3,
    
    /// <summary>
    /// Sadece tanımlandığı akışlarda ve istendiğinde çalışacak extension
    /// </summary>
    DefinedFlowAndRequested = 4
}
```

### Tip Karşılaştırması

| Tip | Değer | Otomatik Çalışma | İstek ile Çalışma | Kapsam |
|-----|-------|------------------|-------------------|--------|
| **Global** | 1 | ✅ Evet | ❌ Hayır | Tüm akışlar |
| **GlobalAndRequested** | 2 | ✅ Evet | ✅ Evet | Tüm akışlar |
| **DefinedFlows** | 3 | ✅ Evet | ❌ Hayır | Tanımlı akışlar |
| **DefinedFlowAndRequested** | 4 | ✅ Evet | ✅ Evet | Tanımlı akışlar |

### Tip Kullanım Senaryoları

| Tip | Kullanım Senaryosu |
|-----|-------------------|
| `Global` | Tüm instance'larda her zaman gerekli olan veriler (örn: sistem bilgileri) |
| `GlobalAndRequested` | Genellikle gerekli ama bazen istek ile tetiklenen veriler (örn: kullanıcı oturumu) |
| `DefinedFlows` | Belirli workflow'larda her zaman gerekli veriler (örn: hesap detayları) |
| `DefinedFlowAndRequested` | Belirli workflow'larda ve talep edildiğinde gerekli veriler |

---

## Extension Kapsamları

Extension'ların hangi endpoint'lerde çalışacağını belirler.

### ExtensionScope Enum

```csharp
public enum ExtensionScope
{
    /// <summary>
    /// {domain}/workflows/{workflow}/instances/{instance} endpoint'inde çalışır
    /// </summary>
    GetInstance = 1,

    /// <summary>
    /// {domain}/workflows/{workflow}/instances endpoint'inde çalışır
    /// </summary>
    GetAllInstances = 2,
    
    /// <summary>
    /// {domain}/workflows/{workflow}/instances/{instance}/transitions endpoint'inde çalışır
    /// </summary>
    GetHistoryTransition = 2,

    /// <summary>
    /// Tüm GET endpoint'lerinde çalışır
    /// </summary>
    Everywhere = 3
}
```

### Kapsam Karşılaştırması

| Kapsam | Değer | Çalıştığı Endpoint'ler |
|--------|-------|------------------------|
| **GetInstance** | 1 | Tek instance sorgulama |
| **GetAllInstances** | 2 | Instance listesi sorgulama |
| **GetHistoryTransition** | 2 | Transition geçmişi sorgulama |
| **Everywhere** | 3 | Tüm GET endpoint'leri |

---

## Kullanım Örnekleri

### Örnek 1: Kullanıcı Oturumu Extension'ı

```json
{
  "key": "extension-user-session",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-extensions",
  "flowVersion": "1.0.0",
  "tags": ["system", "core", "session"],
  "attributes": {
    "type": 2,
    "scope": 1,
    "task": {
      "order": 1,
      "task": {
        "key": "user-session",
        "domain": "core",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "location": "./src/UserSessionMapping.csx",
        "code": "<BASE64>"
      }
    }
  }
}
```

**Mapping Örneği:**

```csharp
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class UserSessionMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse());
    }

    /// <summary>
    /// Kullanıcı oturum verisini workflow instance'ına yerleştir
    /// </summary>
    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        return new ScriptResponse
        {
            Key = "user-session-output",
            Data = new
            {
                userSession = new
                {
                    userId = context.Headers?["user_reference"],
                    deviceId = context.Headers?["x-device-id"],
                    userAgent = context.Headers?["user-agent"],
                    ipAddress = context.Headers?["x-forwarded-for"] ?? context.Headers?["x-real-ip"]
                }
            }
        };
    }
}
```

### Örnek 2: Hesap Limitleri Extension'ı

```json
{
  "key": "extension-account-limits",
  "version": "1.0.0",
  "domain": "banking",
  "flow": "sys-extensions",
  "flowVersion": "1.0.0",
  "tags": ["banking", "accounts", "limits"],
  "attributes": {
    "type": 3,
    "scope": 1,
    "task": {
      "order": 1,
      "task": {
        "key": "get-account-limits",
        "domain": "banking",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "location": "./src/AccountLimitsMapping.csx",
        "code": "<BASE64>"
      }
    }
  }
}
```

### Örnek 3: Müşteri Profili Extension'ı

```json
{
  "key": "extension-customer-profile",
  "version": "1.0.0",
  "domain": "customer",
  "flow": "sys-extensions",
  "flowVersion": "1.0.0",
  "tags": ["customer", "profile", "lookup"],
  "attributes": {
    "type": 4,
    "scope": 3,
    "task": {
      "order": 1,
      "task": {
        "key": "get-customer-profile",
        "domain": "customer",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "location": "./src/CustomerProfileMapping.csx",
        "code": "<BASE64>"
      }
    }
  }
}
```

---

## Extension Çalışma Mekanizması

### Response'a Ekleme

Get Instance Data endpoint'i çağrıldığında, extension tanımı varsa scope ve type'a göre extension'lar çalıştırılır ve response'a `extensions` objesi altına yerleştirilir.

**Örnek Response:**

```json
{
  "data": {
    "userId": "user123",
    "amount": 1000,
    "currency": "TRY"
  },
  "eTag": "W/\"xyz789abc123\"",
  "extensions": {
    "userSession": {
      "userId": "user123",
      "deviceId": "device-abc",
      "userAgent": "Mozilla/5.0...",
      "ipAddress": "192.168.1.1"
    },
    "accountLimits": {
      "dailyLimit": 50000,
      "remainingLimit": 45000,
      "monthlyLimit": 500000
    }
  }
}
```

### İstek ile Extension Çağırma

`GlobalAndRequested` veya `DefinedFlowAndRequested` tipindeki extension'lar query parametresi ile çağrılabilir:

```http
GET /api/v1/{domain}/workflows/{workflow}/instances/{instance}/functions/data?extensions=extension-user-session,extension-account-limits
```

---

## En İyi Uygulamalar

### 1. Extension Tasarımı

| Uygulama | Açıklama |
|----------|----------|
| Tek sorumluluk | Her extension tek bir veri kaynağını zenginleştirmeli |
| Anlamlı isimlendirme | `extension-` prefix'i ile başlayan açıklayıcı isimler |
| Uygun tip seçimi | İhtiyaca göre doğru ExtensionType seçimi |
| Uygun kapsam seçimi | İhtiyaca göre doğru ExtensionScope seçimi |

### 2. Performans

| Uygulama | Açıklama |
|----------|----------|
| Lightweight tasarım | Extension'lar hızlı çalışmalı |
| Caching | Sık değişmeyen veriler için cache kullanımı |
| Lazy loading | Sadece gerektiğinde veri yükleme |
| Timeout | Uygun timeout değerleri belirleme |

### 3. Tip ve Kapsam Seçimi

| Senaryo | Önerilen Tip | Önerilen Kapsam |
|---------|--------------|-----------------|
| Her zaman gerekli sistem verisi | `Global (1)` | `Everywhere (3)` |
| Kullanıcı oturum bilgisi | `GlobalAndRequested (2)` | `GetInstance (1)` |
| Workflow'a özel hesap bilgisi | `DefinedFlows (3)` | `GetInstance (1)` |
| İsteğe bağlı detay bilgisi | `DefinedFlowAndRequested (4)` | `GetInstance (1)` |

### 4. Hata Yönetimi

| Uygulama | Açıklama |
|----------|----------|
| Graceful degradation | Extension hatası ana response'u engellemeli |
| Timeout handling | Uzun süren extension'lar için timeout |
| Error logging | Hataların uygun şekilde loglanması |
| Fallback değerler | Hata durumunda varsayılan değerler |

### 5. Güvenlik

| Uygulama | Açıklama |
|----------|----------|
| Veri filtreleme | Hassas verilerin filtrelenmesi |
| Yetkilendirme | Kullanıcı yetkilerine göre veri sınırlama |
| Audit logging | Veri erişimlerinin loglanması |

---

## Sık Karşılaşılan Hatalar

### 1. Extension Çalışmıyor

```
Extension 'extension-xyz' not found in response
```

**Olası Nedenler:**
- Extension tipi veya kapsamı uyuşmuyor
- Extension tanımı yüklenmemiş
- İstek parametresi eksik (Requested tipler için)

**Çözüm:** Tip ve kapsam ayarlarını kontrol edin.

### 2. Extension Timeout

```
Extension 'extension-xyz' timed out
```

**Çözüm:** Task timeout değerini artırın veya extension mantığını optimize edin.

### 3. Extension Veri Hatası

```
Extension 'extension-xyz' returned invalid data
```

**Çözüm:** Mapping OutputHandler metodunu kontrol edin ve doğru veri formatı döndürün.

---

## İlgili Dokümantasyon

- [📄 Özel Fonksiyonlar](./custom-function.md) - Özel fonksiyon tanımları
- [📄 Function API'leri](./function.md) - Yerleşik sistem fonksiyonları
- [📄 Task Yönetimi](./task.md) - Görev türleri ve kullanımı
- [📄 Mapping Rehberi](./mapping.md) - Kapsamlı haritalama rehberi
- [📄 View Yönetimi](./view.md) - Extension'lı view kullanımı

