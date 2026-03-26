# vNext Runtime - Dokümantasyon

Bu dokümantasyon, vNext Runtime platformu için geliştiricilere yönelik kapsamlı bir rehber sunmaktadır. Platform, low-code, no-code ve full-code desteği sunan bulut tabanlı bir uygulama geliştirme platformudur.

## 📋 İçindekiler

- [🏗️ Platform Yapısı](#️-platform-yapısı)
- [🔄 İş Akışı Bileşenleri](#-iş-akışı-bileşenleri)
- [📚 Dokümantasyon Haritası](#-dokümantasyon-haritası)
- [🚀 Hızlı Başlangıç](#-hızlı-başlangıç)
- [💡 Örnekler ve Kullanım Senaryoları](#-örnekler-ve-kullanım-senaryoları)

---

## 🏗️ Platform Yapısı

vNext Platform, yatayda büyüyen bir servis kümesine sahiptir ve bu servislerle yönetilen önyüz uygulamaları aracılığıyla müşteri, çalışan ve sistemlere arayüz sunarak her türlü iş akışını ve fonksiyonu yüksek güvenlikle gerçekleştirebilir.

### Temel Prensipler

- **Dual-Write Pattern**: Event Sourcing ve Replication desteği
- **Domain-Driven Architecture**: Her domain ayrı runtime ile çalışır
- **Microservice Ready**: Dapr entegrasyonu ile servis mesh desteği
- **Semantic Versioning**: Geriye uyumlu versiyon yönetimi
- **ETag Pattern**: Concurrent update kontrolü

### Mimari Bileşenler

![Components](https://kroki.io/mermaid/svg/eNpVzt0KgkAQBeD7nmJeQHqDwJ_VDIpohS4GL0abbMlWWdef3j5ZvbC5OvAdOFMZal-QBTuYz8ewVqwt-G1bq5KsanSXg-cdIED_mkJClkf65q4dOAhxuPBk4dZrqz68UOgowntj3s-6GUHoSuk_FJhR9wYxcdnbxmwpRmnnITiTpopXihwlGFFrQKoHl7SKWMTleJMTl48oJstGUw2SzaBK7vINp-uWnH_gLZzw2hd72Rf5D2aNUNQ)

---

## 🔄 İş Akışı Bileşenleri

### 1. **Workflow (İş Akışı)**
İş süreçlerini tanımlayan ana bileşen. State'ler ve transition'lar aracılığıyla iş akışını yönetir.

### 2. **State (Durum)**
İş akışının bulunduğu aşamayı temsil eder. Dört farklı türde tanımlanabilir:
- **Initial**: Başlangıç durumu
- **Intermediate**: Ara durumlar
- **Finish**: Bitiş durumu
- **SubFlow**: Alt akış durumu

### 3. **Transition (Geçiş)**
State'ler arasındaki geçişleri yöneten bileşen. Dört farklı tetikleme türü:
- **Manual (0)**: Kullanıcı etkileşimi
- **Automatic (1)**: Koşullu otomatik geçiş
- **Scheduled (2)**: Zamanlanmış geçiş
- **Event (3)**: Olay tabanlı geçiş

### 4. **Task (Görev)**
İş akışı içinde belirli işlemleri gerçekleştiren bağımsız bileşenler:
- **DaprService**: Dapr service invocation
- **DaprPubSub**: Dapr pub/sub mesajlaşma
- **Http**: HTTP web servis çağrıları
- **Script**: C# Roslyn script çalıştırma
- **Condition**: Koşul kontrolü
- **Timer**: Zamanlayıcı görevleri

---

## 📚 Dokümantasyon Haritası

### 🔧 Temel Kavramlar
| Konu | Dosya | Açıklama |
|------|-------|----------|
| **Platform Temelleri** | [`fundamentals/readme.md`](./fundamentals/readme.md) | Platform yapısı ve temel prensipler |
| **Domain Topolojisi** | [`fundamentals/domain-topology.md`](./fundamentals/domain-topology.md) | Domain kavramı, izolasyon ve çoklu-domain mimarisi |
| **Veritabanı Mimarisi** | [`fundamentals/database-architecture.md`](./fundamentals/database-architecture.md) | Multi-schema yapısı, migration sistemi ve DB izolasyonu |
| **Persistance** | [`principles/persistance.md`](./principles/persistance.md) | Veri saklama ve Dual-Write Pattern |
| **Referans Şeması** | [`principles/reference.md`](./principles/reference.md) | Bileşenler arası referans yönetimi |
| **Versiyon Yönetimi** | [`principles/versioning.md`](./principles/versioning.md) | Versiyonlama, paket yönetimi ve deployment stratejisi |

### 🌊 İş Akışı (Flow) Dokümantasyonu
| Konu | Dosya | Açıklama |
|------|-------|----------|
| **İş Akışı Tanımlaması** | [`flow/flow.md`](./flow/flow.md) | Workflow tanımlaması ve geliştirme rehberi |
| **Interface'ler** | [`flow/interface.md`](./flow/interface.md) | Mapping interface'leri ve kullanımı |
| **Mapping Rehberi** | [`flow/mapping.md`](./flow/mapping.md) | Kapsamlı haritalama rehberi ve örnekler |
| **State Yönetimi** | [`flow/state.md`](./flow/state.md) | State türleri ve yaşam döngüsü |
| **Task Tanımları** | [`flow/task.md`](./flow/task.md) | Görev türleri ve kullanım alanları |
| **Transition Yönetimi** | [`flow/transition.md`](./flow/transition.md) | Geçiş türleri ve tetikleme mekanizmaları |
| **View Yönetimi** | [`flow/view.md`](./flow/view.md) | View tanımları, gösterim stratejileri ve platform override'ları |
| **Schema Yönetimi** | [`flow/schema.md`](./flow/schema.md) | Schema tanımları, JSON Schema doğrulaması ve veri bütünlüğü |
| **Function API'leri** | [`flow/function.md`](./flow/function.md) | Sistem function API'leri (State, Data, View) |
| **Özel Fonksiyonlar** | [`flow/custom-function.md`](./flow/custom-function.md) | Task çalıştıran kullanıcı tanımlı fonksiyonlar |
| **Extension Yönetimi** | [`flow/extension.md`](./flow/extension.md) | Instance response'ları için veri zenginleştirme bileşenleri |

### 📋 Görev (Task) Detayları
| Görev Türü | Dosya | Kullanım Alanı |
|------------|-------|----------------|
| **HTTP Task** | [`flow/tasks/http-task.md`](./flow/tasks/http-task.md) | REST API çağrıları ve web servis entegrasyonları |
| **Script Task** | [`flow/tasks/script-task.md`](./flow/tasks/script-task.md) | C# ile iş mantığı ve hesaplama işlemleri |
| **DaprService Task** | [`flow/tasks/dapr-service.md`](./flow/tasks/dapr-service.md) | Microservice çağrıları |
| **DaprPubSub Task** | [`flow/tasks/dapr-pubsub.md`](./flow/tasks/dapr-pubsub.md) | Asenkron mesajlaşma |
| **Condition Task** | [`flow/tasks/condition-task.md`](./flow/tasks/condition-task.md) | Koşul kontrolü ve karar mekanizmaları |
| **Timer Task** | [`flow/tasks/timer-task.md`](./flow/tasks/timer-task.md) | Zamanlama ve periyodik işlemler |
| **Notification Task** | [`flow/tasks/notification-task.md`](./flow/tasks/notification-task.md) | Socket/hub üzerinden gerçek zamanlı durum bildirimleri |

### 🛠️ Nasıl Yapılır (How-To)
| Konu | Dosya | Açıklama |
|------|-------|----------|
| **Instance Başlatma** | [`how-to/start-instance.md`](./how-to/start-instance.md) | Instance yaşam döngüsü ve bileşen yönetimi |

### 🚀 Servisler
| Konu | Dosya | Açıklama |
|------|-------|----------|
| **Init Service** | [`services/init-service.md`](./services/init-service.md) | Paket deployment, versiyonlama ve bileşen yönetimi servisi |

---

## 🚀 Hızlı Başlangıç

### 1. Instance Başlatma

```http
POST /:domain/workflows/:flow/instances/start?sync=true
Content-Type: application/json

{
    "key": "unique-instance-key",
    "tags": ["example", "demo"],
    "attributes": {
        "userId": 123,
        "amount": 1000,
        "currency": "TL"
    }
}
```

### 2. Transition Çalıştırma

```http
PATCH /:domain/workflows/:flow/instances/:instanceId/transitions/:transitionKey?sync=true
Content-Type: application/json

{
    "approvedBy": "admin",
    "approvalDate": "2025-09-20T10:30:00Z"
}
```

### 3. Instance Durumu Sorgulama

```http
GET /:domain/workflows/:flow/instances/:instanceId
If-None-Match: "etag-value"
```

---

## 💡 Örnekler ve Kullanım Senaryoları

### 🛒 E-Ticaret Workflow'u
**Dosya Konumu**: `../../samples/ecommerce/`

Sepet yönetimi, ödeme işlemi ve sipariş takibi içeren kapsamlı e-ticaret örneği.

**Ana Bileşenler**:
- Sepete ürün ekleme (HTTP Task)
- Kullanıcı doğrulama (Script Task)
- Ödeme işlemi (HTTP Task + Condition)

### 🔐 OAuth Authentication Workflow'u
**Dosya Konumu**: `../../samples/oauth/`

OAuth2 authentication flow'u ile MFA (Multi-Factor Authentication) desteği.

**Ana Bileşenler**:
- Client validation
- User credentials validation
- OTP/Push notification MFA
- Token generation

### 💳 Scheduled Payments Workflow'u
**Dosya Konumu**: `../../samples/payments/`

Periyodik ödeme işlemleri ve bildirim sistemi.

**Ana Bileşenler**:
- Payment schedule management
- Timer-based execution
- Notification system (SMS + Push)
- Retry mechanism

---

## 🔍 Interface Kullanımı

### IMapping - Temel Haritalama
```csharp
public class CustomMapping : IMapping
{
    public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Input verilerini hazırla
        return new ScriptResponse { Data = inputData };
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        // Output verilerini işle
        return new ScriptResponse { Data = outputData };
    }
}
```

### ITimerMapping - Zamanlama
```csharp
public class PaymentTimerRule : ITimerMapping
{
    public async Task<TimerSchedule> Handler(ScriptContext context)
    {
        var frequency = context.Instance.Data.frequency;
        return frequency switch
        {
            "daily" => TimerSchedule.FromCronExpression("0 9 * * *"),
            "weekly" => TimerSchedule.FromCronExpression("0 9 * * 1"),
            "monthly" => TimerSchedule.FromCronExpression("0 9 1 * *"),
            _ => TimerSchedule.FromDuration(TimeSpan.FromDays(30))
        };
    }
}
```

### IConditionMapping - Koşul Kontrolü
```csharp
public class AuthSuccessRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        return context.Instance.Data.authentication?.success == true;
    }
}
```

### ITransitionMapping - Transition Payload Mapleme
```csharp
public class ApprovalTransitionMapping : ScriptBase, ITransitionMapping
{
    public async Task<dynamic> Handler(ScriptContext context)
    {
        LogInformation("Onay transition'ı işleniyor");
        
        return new
        {
            approval = new
            {
                approvedBy = context.Body?.userId,
                approvedAt = DateTime.UtcNow,
                status = "approved"
            }
        };
    }
}
```

---

## 🎯 Best Practices

### ✅ Yapılması Gerekenler
- **Semantic versioning** kullanın
- **ETag pattern** ile concurrent update kontrolü yapın
- **Null-safe** kod yazın (`?.` operatörü)
- **Try-catch** ile hata yönetimi yapın
- **camelCase** property isimlendirmesi kullanın

### ❌ Yapılmaması Gerekenler
- Script Task'larda **HTTP çağrıları** yapmayın
- **Circular reference** oluşturmayın
- **SSL doğrulamasını** production'da devre dışı bırakmayın
- **Dynamic property'lerde** null kontrolü yapmadan erişmeyin

---

## 🔧 Geliştirme Araçları

### Referans Kullanımı
```json
{
  "task": {
    "ref": "Tasks/validate-client.json"
  }
}
```

Build sırasında otomatik olarak tam referansa dönüştürülür:
```json
{
  "task": {
    "key": "validate-client",
    "domain": "core", 
    "version": "1.0.0",
    "flow": "sys-tasks"
  }
}
```

### ScriptBase Kullanımı
```csharp
public class SecureMapping : ScriptBase, IMapping
{
    public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        // Secret store'dan güvenli veri alma
        var apiKey = GetSecret("dapr_store", "secret_store", "api_key");
        
        // Güvenli property erişimi
        if (HasProperty(context.Instance.Data, "sensitiveData"))
        {
            var data = GetPropertyValue(context.Instance.Data, "sensitiveData");
            // İşlemler...
        }
        
        return new ScriptResponse();
    }
}
```

**v0.0.42+:** `ScriptBase` ayrıca **koleksiyon ve dynamic nesne** yardımcıları ekler (`CreateObject`, `GetList`, `ListFilter`, `ListSelect` ve ilgili API'ler). Bkz. [Mapping Kılavuzu — Koleksiyon yardımcıları](flow/mapping.md#koleksiyon-ve-dynamic-nesne-yardımcıları) ve [`release/extra/script-base-usage/`](../../release/extra/script-base-usage/) altındaki örnekler.

---

## 📞 Destek ve Katkı

Bu dokümantasyon sürekli güncellenmektedir. Sorularınız veya katkılarınız için:

- **Issues**: GitHub repository üzerinden issue açabilirsiniz
- **Documentation**: Eksik veya hatalı bilgiler için PR gönderebilirsiniz
- **Examples**: Yeni örnek senaryolar için katkı sağlayabilirsiniz

---

## 🔗 İlgili Repolar

### vNext Ekosistemi

- **[vNext Engine](https://github.com/burgan-tech/vnext)** - Ana workflow engine ve runtime
- **[vNext Sys-Flows](https://github.com/burgan-tech/vnext-sys-flow)** - Sistem bileşen iş akışları
- **[vNext Schema](https://github.com/burgan-tech/vnext-schema)** - Sistem bileşen şema yapısı
- **[vNext CLI](https://github.com/burgan-tech/vnext-workflow-cli)** - Komut satırı araçları

---

## 📄 Lisans

Bu dokümantasyon ve örnekler Burgan Technology tarafından geliştirilmiştir.