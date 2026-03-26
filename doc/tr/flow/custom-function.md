# Özel Fonksiyonlar (Custom Functions)

Özel fonksiyonlar, vNext platformunda BFF (Backend for Frontend) API kullanımını azaltmak için tasarlanmış bileşenlerdir. Instance verileri üzerinde çalışarak diğer domain'lere veya entegre servislere uç nokta sağlarlar.

## İçindekiler

1. [Genel Bakış](#genel-bakış)
2. [Function Tanımı](#function-tanımı)
3. [Function Özellikleri](#function-özellikleri)
4. [Tüketim Noktaları](#tüketim-noktaları)
5. [Sistem Fonksiyonları](#sistem-fonksiyonları)
6. [Kullanım Örnekleri](#kullanım-örnekleri)
7. [En İyi Uygulamalar](#en-iyi-uygulamalar)

---

## Genel Bakış

Özel fonksiyonlar şu amaçlarla kullanılır:

- **BFF API Azaltma**: Doğrudan veri erişimi sağlayarak aracı API katmanlarını azaltır
- **Veri Dönüşümü**: Instance verilerini mapping ile istenilen formatta sunar
- **Task Çalıştırma**: Fonksiyon çağrıldığında tanımlı task'ı çalıştırır
- **Servis Entegrasyonu**: Diğer domain'lere veya harici servislere uç nokta sağlar

:::highlight green 💡
Her fonksiyon bir task çalıştırabilir ve task sonucundaki veri mapping ile istenilen formatta döndürülebilir.
:::

---

## Function Tanımı

### Temel Yapı

```json
{
  "key": "function-get-user-info",
  "flow": "sys-functions",
  "domain": "core",
  "version": "1.0.0",
  "flowVersion": "1.0.0",
  "tags": [
    "system",
    "core",
    "users",
    "lookup"
  ],
  "attributes": {
    "scope": "I",
    "task": {
      "order": 1,
      "task": {
        "key": "get-user-info",
        "domain": "core",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "location": "./src/GetUserInfoMapping.csx",
        "code": "<BASE64_ENCODED_MAPPING_CODE>"
      }
    }
  }
}
```

---

## Function Özellikleri

### Temel Özellikler

| Özellik | Tip | Açıklama |
|---------|-----|----------|
| `key` | `string` | Fonksiyon için benzersiz tanımlayıcı |
| `flow` | `string` | Flow stream bilgisi (varsayılan: `sys-functions`) |
| `domain` | `string` | Fonksiyonun ait olduğu domain |
| `version` | `string` | Versiyon bilgisi (semantic versioning) |
| `flowVersion` | `string` | Flow versiyon bilgisi |
| `tags` | `string[]` | Kategorilendirme ve arama için etiketler |
| `attributes` | `object` | Fonksiyon konfigürasyonu |

### Attributes Özellikleri

| Özellik | Tip | Açıklama |
|---------|-----|----------|
| `scope` | `string` | Fonksiyon kapsamı (`I` = Instance, `F` = Workflow, `D` = Domain) |
| `task` | `object` | Tek task (eski şekil; tek görev için) |
| `onExecutionTasks` | `array` | Sıralı çalıştırılacak task'lar (v0.0.42+); aşağıdaki **Çoklu task** bölümüne bakın |
| `output` | `object` | İsteğe bağlı çıktı mapping betiği: `location` / `code` (v0.0.42+); **`IOutputHandler`** uygular |

### Scope Değerleri

| Değer | Açıklama | Erişim Seviyesi |
|-------|----------|-----------------|
| `I` | Instance | Belirli bir instance için çalışır |
| `F` | Workflow | Workflow seviyesinde çalışır |
| `D` | Domain | Domain seviyesinde çalışır |

### Task Yapısı

```json
{
  "task": {
    "order": 1,
    "task": {
      "key": "task-key",
      "domain": "core",
      "version": "1.0.0",
      "flow": "sys-tasks"
    },
    "mapping": {
      "location": "./src/MappingFile.csx",
      "code": "<BASE64_ENCODED_CODE>"
    }
  }
}
```

| Özellik | Tip | Açıklama |
|---------|-----|----------|
| `order` | `number` | Task çalışma sırası |
| `task` | `object` | Task referansı |
| `mapping` | `object` | Input/Output dönüşüm mapping'i |

### Çoklu task çalıştırma ve output mapping (v0.0.42+)

Tek bir **`task`** yerine **`attributes.onExecutionTasks`** ile **sırayla** birden fazla task çalıştırılabilir. Her öğede **`order`**, **`task`** referansı ve isteğe bağlı **`mapping`** bulunur. Sonraki task'lar, aynı fonksiyon yürütmesinde önceki task çıktılarını kullanabilir.

İsteğe bağlı **`attributes.output`**, **`IOutputHandler`** uygulayan bir betiğe işaret eder. **`OutputHandler`** içinde sonuçlar **`context.OutputResponse`** üzerinden okunur (anahtarlar çalıştırılan task anahtarlarına göre, tipik olarak **camelCase**).

```json
"attributes": {
  "scope": "I",
  "onExecutionTasks": [
    {
      "order": 1,
      "task": {
        "key": "validate-account-policies",
        "domain": "core",
        "flow": "sys-tasks",
        "version": "1.0.0"
      },
      "mapping": {
        "location": "./src/FunctionValidatePoliciesMapping.csx",
        "code": ""
      }
    },
    {
      "order": 2,
      "task": {
        "key": "get-data-from-workflow",
        "domain": "core",
        "flow": "sys-tasks",
        "version": "1.0.0"
      },
      "mapping": {
        "location": "./src/FunctionGetInstanceDataMapping.csx",
        "code": ""
      }
    }
  ],
  "output": {
    "location": "./src/FunctionOutputMapping.csx",
    "code": ""
  }
}
```

```csharp
using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class FunctionOutputMapping : IOutputHandler
{
    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var policies = context.OutputResponse["validateAccountPolicies"].data;
        var instanceData = context.OutputResponse?["getDataFromWorkflow"].data;
        return Task.FromResult(new ScriptResponse
        {
            Key = "multi-task-function-output",
            Data = new { policyValidation = policies, instanceSnapshot = instanceData }
        });
    }
}
```

---

## Tüketim Noktaları

### Domain Seviyesi Fonksiyonlar

**Tüm domain instance ve verilerini döndürür:**

```http
GET /api/v1/{domain}/functions
```

**Belirli bir fonksiyonun sonucunu döndürür:**

```http
GET /api/v1/{domain}/functions/{function}
```

### Workflow seviyesi özel fonksiyon URL'i (v0.0.42'de kaldırıldı)

**`GET /api/v1/{domain}/workflows/{workflow}/functions/{function}`** kalıbı (instance id olmadan kayıtlı fonksiyon adıyla çağrı) **v0.0.42** itibarıyla **kaldırılmıştır**. Bunun yerine aşağıdaki **instance kapsamlı** fonksiyon uçları, **workflow instance listeleme** (`GET .../workflows/{workflow}/instances`) veya dokümante edilen diğer API'ler kullanılmalıdır.

### Instance Seviyesi Fonksiyonlar

**Belirli bir instance için fonksiyonu çalıştırır:**

```http
GET /api/v1/{domain}/workflows/{workflow}/instances/{instance}/functions/{function}
```

---

## Sistem Fonksiyonları

vNext platformu, her workflow instance'ı için hazır sistem fonksiyonları sağlar:

### State Function

Instance'ın mevcut durum bilgisini döndürür.

**Endpoint:**
```http
GET /api/v1/{domain}/workflows/{workflow}/instances/{instance}/functions/state
```

**Response:**
```json
{
  "data": {
    "href": "/core/workflows/account-opening/instances/d4b161a8-7705-4bfb-9ba4-d76461bb35eb/functions/data?extensions=extension-user-session"
  },
  "view": {
    "loadData": true,
    "href": "/core/workflows/account-opening/instances/d4b161a8-7705-4bfb-9ba4-d76461bb35eb/functions/view"
  },
  "state": "account-type-selection",
  "status": "A",
  "activeCorrelations": [],
  "transitions": [
    {
      "name": "select-demand-deposit",
      "href": "/core/workflows/account-opening/instances/d4b161a8-7705-4bfb-9ba4-d76461bb35eb/transitions/select-demand-deposit"
    },
    {
      "name": "execute-sub",
      "href": "/core/workflows/account-opening/instances/d4b161a8-7705-4bfb-9ba4-d76461bb35eb/transitions/execute-sub"
    }
  ],
  "eTag": "01KCHWT3QQFM6J9QQD9G4T0VRP"
}
```

**Response Alanları:**

| Alan | Tip | Açıklama |
|------|-----|----------|
| `data.href` | `string` | Data fonksiyon endpoint'i |
| `view.loadData` | `boolean` | View'ın data yüklemesi gerekip gerekmediği |
| `view.href` | `string` | View fonksiyon endpoint'i |
| `state` | `string` | Mevcut state adı |
| `status` | `string` | Instance durumu (A=Active, C=Completed) |
| `activeCorrelations` | `array` | Aktif alt korelasyonlar |
| `transitions` | `array` | Kullanılabilir transition'lar |
| `eTag` | `string` | Cache kontrolü için ETag değeri |

### View Function

Instance'ın mevcut state veya transition için view verisini döndürür.

**Endpoint:**
```http
GET /api/v1/{domain}/workflows/{workflow}/instances/{instance}/functions/view?transitionKey={transition}&platform={platform}
```

**Query Parametreleri:**

| Parametre | Tip | Açıklama |
|-----------|-----|----------|
| `transitionKey` | `string` | Belirli transition için view (opsiyonel) |
| `platform` | `string` | Hedef platform: `web`, `ios`, `android` |

**Response:**
```json
{
  "key": "account-type-selection-view",
  "content": "{\"type\":\"form\",\"title\":{\"en-US\":\"Choose Your Account Type\",\"tr-TR\":\"Hesap Türünüzü Seçin\"},\"fields\":[...]}",
  "type": "Json",
  "display": "full-page",
  "label": ""
}
```

**Response Alanları:**

| Alan | Tip | Açıklama |
|------|-----|----------|
| `key` | `string` | View tanımlayıcısı |
| `content` | `string` | View içeriği (JSON formatında) |
| `type` | `string` | İçerik tipi (Json, Html, vb.) |
| `display` | `string` | Gösterim modu (full-page, popup, bottom-sheet, vb.) |
| `label` | `string` | Lokalize edilmiş etiket |

### Schema Function

Instance'ın mevcut state veya transition için schema verisini döndürür.

**Endpoint:**
```http
GET /api/v1/{domain}/workflows/{workflow}/instances/{instance}/functions/schema?transitionKey={transition}
```

**Response:**
```json
{
  "key": "account-type-selection",
  "type": "workflow",
  "schema": {
    "$id": "https://schemas.vnext.com/banking/account-type-selection.json",
    "type": "object",
    "title": "Account Type Selection Schema",
    "$schema": "https://json-schema.org/draft/2020-12/schema",
    "required": ["accountType"],
    "properties": {
      "accountType": {
        "type": "string",
        "oneOf": [
          {
            "const": "demand-deposit",
            "description": "Vadesiz Hesap - Demand Deposit Account"
          },
          {
            "const": "time-deposit",
            "description": "Vadeli Hesap - Time Deposit Account"
          },
          {
            "const": "investment-account",
            "description": "Fonlu Hesap - Investment Account"
          },
          {
            "const": "savings-account",
            "description": "Tasarruf Hesabı - Savings Account"
          }
        ],
        "title": "Account Type",
        "description": "Type of account to be opened"
      }
    },
    "description": "Schema for account type selection input",
    "additionalProperties": false
  }
}
```

---

## Kullanım Örnekleri

### Örnek 1: Kullanıcı Bilgisi Fonksiyonu

```json
{
  "key": "function-get-user-info",
  "flow": "sys-functions",
  "domain": "core",
  "version": "1.0.0",
  "flowVersion": "1.0.0",
  "tags": ["system", "core", "users", "lookup"],
  "attributes": {
    "scope": "I",
    "task": {
      "order": 1,
      "task": {
        "key": "get-user-info",
        "domain": "core",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "location": "./src/GetUserInfoMapping.csx",
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

public class GetUserInfoMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        try
        {
            var httpTask = task as HttpTask;
            if (httpTask == null)
                throw new InvalidOperationException("Task must be an HttpTask");

            var userId = context.Body?.userId;

            // URL'yi userId ile güncelle
            httpTask.SetUrl(httpTask.Url.Replace("{userId}", userId?.ToString() ?? ""));

            // Header'ları ayarla
            var headers = new Dictionary<string, string?>
            {
                ["Content-Type"] = "application/json",
                ["Accept"] = "application/json",
                ["X-Request-Id"] = Guid.NewGuid().ToString()
            };

            httpTask.SetHeaders(headers);

            return Task.FromResult(new ScriptResponse());
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ScriptResponse
            {
                Key = "user-info-error",
                Data = new { error = ex.Message }
            });
        }
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            var statusCode = context.Body?.statusCode ?? 500;
            var responseData = context.Body?.data;

            if (statusCode >= 200 && statusCode < 300)
            {
                return new ScriptResponse
                {
                    Key = "user-info-success",
                    Data = new
                    {
                        user = responseData,
                        phoneNumber = responseData?.phoneNumber,
                        hasRegisteredDevices = ((object[])responseData?.registeredDevices).Length > 0,
                        language = responseData?.language ?? "tr-TR"
                    },
                    Tags = new[] { "users", "lookup", "success" }
                };
            }
            else
            {
                return new ScriptResponse
                {
                    Key = "user-info-failure",
                    Data = new
                    {
                        error = "Failed to get user information",
                        errorCode = "user_info_failed",
                        statusCode = statusCode,
                        hasRegisteredDevices = false
                    },
                    Tags = new[] { "users", "lookup", "failure" }
                };
            }
        }
        catch (Exception ex)
        {
            return new ScriptResponse
            {
                Key = "user-info-exception",
                Data = new
                {
                    error = "Internal processing error",
                    errorCode = "processing_error",
                    errorDescription = ex.Message,
                    hasRegisteredDevices = false
                },
                Tags = new[] { "users", "lookup", "error" }
            };
        }
    }
}
```

### Örnek 2: Hesap Bakiyesi Fonksiyonu

```json
{
  "key": "function-get-account-balance",
  "flow": "sys-functions",
  "domain": "banking",
  "version": "1.0.0",
  "flowVersion": "1.0.0",
  "tags": ["banking", "accounts", "balance"],
  "attributes": {
    "scope": "I",
    "task": {
      "order": 1,
      "task": {
        "key": "get-balance",
        "domain": "banking",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "location": "./src/GetBalanceMapping.csx",
        "code": "<BASE64>"
      }
    }
  }
}
```

---

## En İyi Uygulamalar

### 1. Fonksiyon Tasarımı

| Uygulama | Açıklama |
|----------|----------|
| Tek sorumluluk | Her fonksiyon tek bir iş yapmalı |
| Anlamlı isimlendirme | `function-` prefix'i ile başlayan açıklayıcı isimler |
| Uygun scope | İhtiyaca göre doğru scope seçimi (I, W, D) |
| Versiyon yönetimi | Semantic versioning kullanımı |

### 2. Mapping Yazımı

| Uygulama | Açıklama |
|----------|----------|
| Hata yönetimi | Try-catch blokları ile hata yakalama |
| Null kontrolü | Null-safe kod yazımı (`?.` operatörü) |
| Loglama | Uygun log mesajları ekleme |
| Performans | Gereksiz işlemlerden kaçınma |

### 3. Güvenlik

| Uygulama | Açıklama |
|----------|----------|
| Yetkilendirme | Uygun authorization kontrolleri |
| Veri doğrulama | Input validation yapılması |
| Hassas veri | Hassas verilerin maskelenmesi |
| Rate limiting | İstek limitleri uygulanması |

### 4. Performans

| Uygulama | Açıklama |
|----------|----------|
| Caching | Uygun cache stratejisi kullanımı |
| Async işlemler | Asenkron operasyonlar için async/await |
| Timeout | Uygun timeout değerleri belirleme |
| Resource yönetimi | Kaynakların düzgün serbest bırakılması |

---

## İlgili Dökümanlar

- [Function API'leri](./function.md) - Yerleşik sistem fonksiyonları (State, Data, View)
- [Instance Filtreleme](./instance-filtering.md) - GraphQL-stil filtreleme kılavuzu
- [Extension Yönetimi](./extension.md) - Veri zenginleştirme bileşenleri
- [Task Yönetimi](./task.md) - Görev türleri ve kullanımı
- [Mapping Rehberi](./mapping.md) - Kapsamlı haritalama rehberi

