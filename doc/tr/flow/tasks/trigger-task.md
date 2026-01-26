# Trigger Task Türleri

İş akışı instance yönetimi için dört ayrı task türü mevcuttur. Her task türü, farklı bir iş akışı etkileşim senaryosunu destekler ve kendi type numarasına sahiptir.

## Task Türleri

- **StartTask** (Type: "11") - Yeni iş akışı instance'ları başlatır
- **DirectTriggerTask** (Type: "12") - Mevcut instance'larda transition tetikler
- **GetInstanceDataTask** (Type: "13") - Instance verilerini alır
- **SubProcessTask** (Type: "14") - Bağımsız subprocess instance'ları başlatır

## Özellikler

- ✅ Programatik olarak yeni iş akışı instance'ları başlatma
- ✅ Mevcut instance'larda transition tetikleme (doğrudan veya key tabanlı)
- ✅ Bağımsız subprocess instance'ları başlatma
- ✅ Extension desteği ile instance verilerini alma
- ✅ Çapraz iş akışı orkestrasyon
- ✅ Dinamik iş akışı kompozisyonu
- ✅ Detaylı yanıt takibi

## 1. StartTask (Type: "11")

Yeni bir iş akışı instance'ı oluşturur. İş akışı yürütmesi sırasında programatik olarak yeni iş akışı instance'ları başlatmak için kullanılır.

### Görev Tanımı

```json
{
  "key": "start-approval-workflow",
  "flow": "sys-tasks",
  "domain": "core",
  "version": "1.0.0",
  "tags": ["workflow", "instance", "start"],
  "attributes": {
    "type": "11",
    "config": {
      "domain": "approvals",
      "flow": "approval-flow",
      "body": {
        "documentId": "doc-12345",
        "requestedBy": "user-123"
      }
    }
  }
}
```

### Konfigürasyon Alanları

| Alan | Tür | Gerekli | Varsayılan | Açıklama |
|------|-----|---------|------------|----------|
| `domain` | string | Evet | - | Hedef iş akışı domain'i |
| `flow` | string | Evet | - | Hedef iş akışı flow adı |
| `body` | object | Hayır | - | İstekle gönderilecek veri |
| `validateSsl` | boolean | Hayır | true | SSL sertifika doğrulaması (v0.0.33+) |

### SSL Yapılandırması

**SSL Doğrulama Etkin (Varsayılan):**
```json
{
  "type": "11",
  "config": {
    "domain": "approvals",
    "flow": "approval-flow",
    "validateSsl": true
  }
}
```

**SSL Doğrulama Devre Dışı:**
```json
{
  "type": "11",
  "config": {
    "domain": "approvals",
    "flow": "approval-flow",
    "validateSsl": false
  }
}
```

:::warning Güvenlik Uyarısı
SSL doğrulamasını yalnızca geliştirme ortamında veya güvenilir dahili servislerde devre dışı bırakın.
:::

### Kullanım Alanları

- Ana iş süreçlerinden onay iş akışları başlatma
- Transaction loglama için audit iş akışları oluşturma
- Bildirim iş akışları başlatma
- Paralel işleme iş akışları başlatma

### Mapping Örneği

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class StartApprovalMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var startTask = task as StartTask;
        
        // Hedef iş akışını yapılandır
        startTask.SetDomain("approvals");
        startTask.SetFlow("approval-flow");
        
        // Başlangıç verilerini hazırla
        startTask.SetBody(new {
            documentId = context.Instance.Data.documentId,
            documentType = context.Instance.Data.documentType,
            requestedBy = context.Instance.Data.userId,
            approvalLevel = "L1",
            priority = "HIGH",
            requestedAt = DateTime.UtcNow,
            metadata = new {
                sourceInstanceId = context.Instance.Id,
            }
        });
        
        return Task.FromResult(new ScriptResponse
        {
            Data = context.Instance.Data
        });
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var response = new ScriptResponse();
        
        if (context.Body.isSuccess)
        {
            response.Data = new
            {
                approvalInstanceId = context.Body.data.id,
                approvalStarted = true,
                startedAt = DateTime.UtcNow,
                status = "APPROVAL_INITIATED"
            };
        }
        else
        {
            response.Data = new
            {
                approvalStarted = false,
                error = context.Body.errorMessage ?? "Onay iş akışı başlatılamadı",
                shouldRetry = true
            };
        }
        
        return response;
    }
}
```

### Başarılı Yanıt

```json
{
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "status": "A"
}
```

## 2. DirectTriggerTask (Type: "12")

Mevcut bir iş akışı instance'ında belirli bir transition'ı yürütür. Diğer iş akışı instance'larında state transition'larını tetiklemek için kullanılır.

### Görev Tanımı

```json
{
  "key": "trigger-approval-action",
  "flow": "sys-tasks",
  "domain": "core",
  "version": "1.0.0",
  "tags": ["transition", "trigger"],
  "attributes": {
    "type": "12",
    "config": {
      "domain": "approvals",
      "flow": "approval-flow",
      "transitionName": "approve",
      "instanceId": "550e8400-e29b-41d4-a716-446655440000",
      "body": {
        "approvedBy": "manager123",
        "approvalDate": "2024-01-15T10:30:00Z"
      }
    }
  }
}
```

### Konfigürasyon Alanları

| Alan | Tür | Gerekli | Varsayılan | Açıklama |
|------|-----|---------|------------|----------|
| `domain` | string | Evet | - | Hedef iş akışı domain'i |
| `flow` | string | Evet | - | Hedef iş akışı flow adı |
| `transitionName` | string | Evet | - | Çalıştırılacak transition adı |
| `key` | string | Koşullu | - | Hedef instance key'i (instanceId yoksa kullanılır) |
| `instanceId` | string | Koşullu | - | Hedef instance ID'si (öncelikli) |
| `body` | object | Hayır | - | İstekle gönderilecek veri |
| `validateSsl` | boolean | Hayır | true | SSL sertifika doğrulaması (v0.0.33+) |

**Not:** `instanceId` veya `key` alanlarından biri sağlanmalıdır. `instanceId` önceliklidir. İkisi de yoksa mevcut instance ID kullanılır.

### SSL Yapılandırması

**SSL Doğrulama Etkin (Varsayılan):**
```json
{
  "type": "12",
  "config": {
    "domain": "approvals",
    "flow": "approval-flow",
    "transitionName": "approve",
    "validateSsl": true
  }
}
```

**SSL Doğrulama Devre Dışı:**
```json
{
  "type": "12",
  "config": {
    "domain": "approvals",
    "flow": "approval-flow",
    "transitionName": "approve",
    "validateSsl": false
  }
}
```

:::warning Güvenlik Uyarısı
SSL doğrulamasını yalnızca geliştirme ortamında veya güvenilir dahili servislerde devre dışı bırakın.
:::

### Kullanım Alanları

- Harici sistemlerden iş akışlarını onaylama veya reddetme
- Bağımlı iş akışlarında durum güncellemelerini tetikleme
- Çoklu iş akışı süreçlerini koordine etme
- İş akışı callback'lerini uygulama

### Mapping Örneği

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class TriggerApprovalMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var directTriggerTask = task as DirectTriggerTask;
        
        // Hedef instance'ı ayarla
        directTriggerTask.SetInstance(context.Instance.Data.approvalInstanceId);
        directTriggerTask.SetTransitionName("approve");
        
        // Transition payload'ını hazırla
        directTriggerTask.SetBody(new {
            action = "approve",
            approvedBy = context.Instance.Data.currentUser,
            approvalDate = DateTime.UtcNow,
            comments = context.Instance.Data.approvalComments ?? "Onaylandı",
            signature = context.Instance.Data.digitalSignature,
            status = "APPROVED"
        });
        
        return Task.FromResult(new ScriptResponse
        {
            Data = context.Instance.Data
        });
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var response = new ScriptResponse();
        
        if (context.Body.isSuccess)
        {
            response.Data = new
            {
                transitionTriggered = true,
                triggeredAt = DateTime.UtcNow,
                status = context.Body.data?.status,
                status = "TRANSITION_SUCCESS"
            };
        }
        else
        {
            response.Data = new
            {
                transitionTriggered = false,
                error = context.Body.errorMessage,
                status = "TRANSITION_FAILED"
            };
        }
        
        return response;
    }
}
```

### Başarılı Yanıt

```json
{
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "status": "A"
}
```

## 3. GetInstanceDataTask (Type: "13")

Başka bir iş akışı instance'ından instance verilerini alır. Ek ilgili verileri almak için opsiyonel extension'ları destekler.

### Görev Tanımı

```json
{
  "key": "get-user-profile-data",
  "flow": "sys-tasks",
  "domain": "core",
  "version": "1.0.0",
  "tags": ["instance", "data", "fetch"],
  "attributes": {
    "type": "13",
    "config": {
      "domain": "users",
      "flow": "user-profile",
      "instanceId": "660e8400-e29b-41d4-a716-446655440001",
      "extensions": ["profile", "preferences", "security"]
    }
  }
}
```

### Konfigürasyon Alanları

| Alan | Tür | Gerekli | Varsayılan | Açıklama |
|------|-----|---------|------------|----------|
| `domain` | string | Evet | - | Hedef iş akışı domain'i |
| `flow` | string | Evet | - | Hedef iş akışı flow adı |
| `key` | string | Koşullu | - | Hedef instance key'i (instanceId yoksa kullanılır, doğrudan key olarak kullanılır) |
| `instanceId` | string | Koşullu | - | Hedef instance ID'si (öncelikli) |
| `extensions` | string[] | Hayır | - | Alınacak extension'lar |
| `validateSsl` | boolean | Hayır | true | SSL sertifika doğrulaması (v0.0.33+) |

**Not:** `instanceId` veya `key` alanlarından biri sağlanmalıdır. `instanceId` önceliklidir. İkisi de yoksa mevcut instance ID kullanılır.

### SSL Yapılandırması

**SSL Doğrulama Etkin (Varsayılan):**
```json
{
  "type": "13",
  "config": {
    "domain": "users",
    "flow": "user-profile",
    "instanceId": "660e8400-e29b-41d4-a716-446655440001",
    "validateSsl": true
  }
}
```

**SSL Doğrulama Devre Dışı:**
```json
{
  "type": "13",
  "config": {
    "domain": "users",
    "flow": "user-profile",
    "instanceId": "660e8400-e29b-41d4-a716-446655440001",
    "validateSsl": false
  }
}
```

:::warning Güvenlik Uyarısı
SSL doğrulamasını yalnızca geliştirme ortamında veya güvenilir dahili servislerde devre dışı bırakın.
:::

### Kullanım Alanları

- Kişiselleştirme için kullanıcı profil verilerini alma
- Merkezi iş akışlarından konfigürasyon alma
- Master iş akışlarından referans verileri yükleme
- Birden fazla iş akışı instance'ından veri toplama
- Çapraz iş akışı veri federasyonu

### Mapping Örneği

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class GetUserProfileDataMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var getDataTask = task as GetInstanceDataTask;
        
        // Veri alınacak hedef instance'ı ayarla
        getDataTask.SetInstance(context.Instance.Data.userProfileInstanceId);
        
        // Extension'ları ayarla (opsiyonel)
        getDataTask.SetExtensions(new[] { "profile", "preferences", "security" });
        
        return Task.FromResult(new ScriptResponse
        {
            Data = context.Instance.Data
        });
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var response = new ScriptResponse();
        
        if (context.Body.isSuccess)
        {
            var instanceData = context.Body.data;
            
            response.Data = new
            {
                userProfile = new
                {
                    userId = instanceData.userId,
                    name = instanceData.profile?.name,
                    email = instanceData.profile?.email,
                    phone = instanceData.profile?.phone,
                    preferences = instanceData.preferences,
                    securitySettings = instanceData.security,
                    accountType = instanceData.profile?.accountType
                },
                dataFetchedAt = DateTime.UtcNow,
                status = "PROFILE_DATA_LOADED"
            };
        }
        else
        {
            response.Data = new
            {
                error = "Kullanıcı profil verisi alınamadı",
                errorMessage = context.Body.errorMessage,
                shouldRetry = false,
                status = "PROFILE_DATA_LOAD_FAILED"
            };
        }
        
        return response;
    }
}
```

### Başarılı Yanıt

```json
{
  "isSuccess": true,
  "data": {
    "userId": "user123",
    "profile": {
      "name": "John Doe",
      "email": "john.doe@example.com"
    },
    "preferences": {
      "language": "tr-TR",
      "theme": "dark"
    }
  }
}
```

## 4. SubProcessTask (Type: "14")

Ana iş akışı ile paralel çalışan bağımsız bir subprocess instance'ı başlatır. Subprocess'ler, ana iş akışını bloke etmeyen fire-and-forget işlemlerdir.

### Görev Tanımı

```json
{
  "key": "start-audit-subprocess",
  "flow": "sys-tasks",
  "domain": "core",
  "version": "1.0.0",
  "tags": ["subprocess", "audit"],
  "attributes": {
    "type": "14",
    "config": {
      "domain": "audit",
      "flow": "transaction-audit",
      "version": "1.0.0",
      "body": {
        "transactionId": "txn-12345",
        "userId": "user-123"
      }
    }
  }
}
```

### Konfigürasyon Alanları

| Alan | Tür | Gerekli | Varsayılan | Açıklama |
|------|-----|---------|------------|----------|
| `domain` | string | Evet | - | Hedef iş akışı domain'i |
| `flow` | string | Evet | - | Hedef iş akışı key'i |
| `version` | string | Hayır | - | SubFlow versiyonu |
| `key` | string | Hayır | - | SubFlow key değeri |
| `tags` | Array<string> | Hayır | - | Etiket değerleri |
| `body` | object | Hayır | - | İstekle gönderilecek veri |
| `validateSsl` | boolean | Hayır | true | SSL sertifika doğrulaması (v0.0.33+) |

### SSL Yapılandırması

**SSL Doğrulama Etkin (Varsayılan):**
```json
{
  "type": "14",
  "config": {
    "domain": "audit",
    "flow": "transaction-audit",
    "version": "1.0.0",
    "validateSsl": true
  }
}
```

**SSL Doğrulama Devre Dışı:**
```json
{
  "type": "14",
  "config": {
    "domain": "audit",
    "flow": "transaction-audit",
    "version": "1.0.0",
    "validateSsl": false
  }
}
```

:::warning Güvenlik Uyarısı
SSL doğrulamasını yalnızca geliştirme ortamında veya güvenilir dahili servislerde devre dışı bırakın.
:::

### Kullanım Alanları

- Arka planda audit loglama
- Asenkron bildirim gönderme
- Paralel veri işleme
- Bağımsız raporlama iş akışları
- Event-driven yan etkiler

**SubProcess vs SubFlow:**
- **SubProcess**: Fire-and-forget, bağımsız çalışır, ana iş akışını bloke etmez
- **SubFlow**: Ana iş akışını bloke eder, veri döner, sıkı entegrasyon

### Mapping Örneği

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class StartAuditSubProcessMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var subProcessTask = task as SubProcessTask;
        
        // Subprocess'i yapılandır
        subProcessTask.SetDomain("audit");
        subProcessTask.SetFlow("transaction-audit");
        subProcessTask.SetVersion("1.0.0");
        
        // Subprocess başlangıç verilerini hazırla
        subProcessTask.SetBody(new {
            transactionId = context.Instance.Data.transactionId,
            transactionType = context.Instance.Data.transactionType,
            amount = context.Instance.Data.amount,
            currency = context.Instance.Data.currency,
            userId = context.Instance.Data.userId,
            action = context.Instance.Data.action,
            timestamp = DateTime.UtcNow,
            parentInstanceId = context.Instance.Id,
            auditDetails = new {
                ipAddress = context.Headers["x-forwarded-for"],
                userAgent = context.Headers["user-agent"],
                sessionId = context.Instance.Data.sessionId
            }
        });
        
        return Task.FromResult(new ScriptResponse
        {
            Data = context.Instance.Data
        });
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var response = new ScriptResponse();
        
        // SubProcess fire-and-forget'tir
        // Sadece başlatıldığını onayla
        response.Data = new
        {
            auditSubProcessId = context.Body.data?.id,
            auditSubProcessStatus = context.Body.data?.status,
            auditInitiated = true,
            initiatedAt = DateTime.UtcNow,
            status = "AUDIT_SUBPROCESS_LAUNCHED"
        };
        
        return response;
    }
}
```


## Property Erişimi

Her task türü kendi setter metodlarına sahiptir:

### StartTask Setter Metodları

- **SetDomain(string domain)**: Hedef iş akışı domain'ini ayarlar
- **SetFlow(string flow)**: Hedef iş akışı flow adını ayarlar
- **SetKey(string key)**: Hedef instance key'ini ayarlar (instanceId yoksa kullanılır)
- **SetSync(string key)**: Hedef instance nasıl başlatılacağını ayarlar
- **SetTags(string[] tags)**: Hedef instance tags'ini ayarlar
- **SetVersion(string version)**: SubFlow versiyonunu ayarlar
- **SetBody(dynamic body)**: İstek body'sini ayarlar

### DirectTriggerTask Setter Metodları

- **SetDomain(string domain)**: Hedef iş akışı domain'ini ayarlar
- **SetFlow(string flow)**: Hedef iş akışı flow adını ayarlar
- **SetTransitionName(string transitionName)**: Çalıştırılacak transition adını ayarlar
- **SetInstance(string instanceId)**: Hedef instance ID'sini ayarlar
- **SetKey(string key)**: Hedef instance key'ini ayarlar (instanceId yoksa kullanılır)
- **SetSync(string key)**: Hedef instance nasıl başlatılacağını ayarlar
- **SetTags(string[] tags)**: Hedef instance tags'ini ayarlar
- **SetVersion(string version)**: SubFlow versiyonunu ayarlar
- **SetBody(dynamic body)**: İstek body'sini ayarlar

### GetInstanceDataTask Setter Metodları

- **SetDomain(string domain)**: Hedef iş akışı domain'ini ayarlar
- **SetFlow(string flow)**: Hedef iş akışı flow adını ayarlar
- **SetInstance(string instanceId)**: Hedef instance ID'sini ayarlar
- **SetKey(string key)**: Hedef instance key'ini ayarlar (instanceId yoksa kullanılır, doğrudan key olarak kullanılır)
- **SetExtensions(string[] extensions)**: Alınacak extension'ları ayarlar

### SubProcessTask Setter Metodları

- **SetDomain(string domain)**: Hedef iş akışı domain'ini ayarlar
- **SetFlow(string key)**: Hedef iş akışı key'ini ayarlar
- **SetKey(string key)**: Hedef instance key'ini ayarlar
- **SetTags(string[] tags)**: Hedef instance tags'ini ayarlar
- **SetVersion(string version)**: SubFlow versiyonunu ayarlar
- **SetBody(dynamic body)**: İstek body'sini ayarlar

### Konfigürasyon vs Dinamik Ayarlama

Task'lar için gerekli alanlar **iki şekilde** sağlanabilir:

1. **Statik Konfigürasyon**: Task JSON tanımında config bölümünde belirtilir
2. **Dinamik Ayarlama**: InputHandler içinde setter metodları ile runtime'da ayarlanır

**Öncelik Kuralı:** Hem JSON config'te hem de InputHandler mapping'inde aynı alan tanımlanmışsa, **InputHandler'da set edilen değer önceliğe sahiptir**. Bu sayede runtime'da dinamik değerlerle statik konfigürasyon override edilebilir.

**Kullanım Stratejileri:**

```csharp
// Senaryo 1: JSON'da domain ve flow tanımlı, mapping'te override edilmez
// Task JSON: "config": { "domain": "approvals", "flow": "approval-flow" }
public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var startTask = task as StartTask;
    // Domain ve flow zaten config'te tanımlı, değiştirmeye gerek yok
    startTask.SetBody(new { /* data */ });
    return Task.FromResult(new ScriptResponse());
}

// Senaryo 2: JSON'da domain yok, mapping'te dinamik olarak ayarlanır
// Task JSON: "config": { "flow": "approval-flow" }
public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var startTask = task as StartTask;
    // Runtime'da dinamik olarak domain belirlenir
    var targetDomain = context.Instance.Data.approvalType == "document" 
        ? "document-approvals" 
        : "standard-approvals";
    startTask.SetDomain(targetDomain);
    startTask.SetBody(new { /* data */ });
    return Task.FromResult(new ScriptResponse());
}

// Senaryo 3: JSON'da instanceId yok, mapping'te context'ten alınır
// Task JSON: "config": { "domain": "approvals", "flow": "approval-flow", "transitionName": "approve" }
public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var directTriggerTask = task as DirectTriggerTask;
    // Instance ID workflow data'sından alınır
    directTriggerTask.SetInstance(context.Instance.Data.approvalInstanceId);
    directTriggerTask.SetBody(new { /* data */ });
    return Task.FromResult(new ScriptResponse());
}

// Senaryo 4: JSON'da instanceId var, mapping'te override edilir (Öncelik mapping'te!)
// Task JSON: "config": { "instanceId": "default-instance-id" }
public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var directTriggerTask = task as DirectTriggerTask;
    // JSON'daki default değer override edilir - mapping değeri kullanılır!
    directTriggerTask.SetInstance(context.Instance.Data.targetInstanceId);
    directTriggerTask.SetBody(new { /* data */ });
    return Task.FromResult(new ScriptResponse());
}
```

## Standart Yanıt

Her task türü kendi yanıt yapısına sahiptir:

### StartTask Yanıtı

```json
{
  "isSuccess": true,
  "data": {
    "instanceId": "550e8400-e29b-41d4-a716-446655440000",
    "state": "initial-state",
    "createdAt": "2025-11-19T10:30:00Z"
  },
  "metadata": {
    "TaskType": "StartTrigger",
    "Domain": "approvals",
    "Flow": "approval-flow"
  }
}
```

### DirectTriggerTask Yanıtı

```json
{
  "isSuccess": true,
  "data": {
    "instanceId": "550e8400-e29b-41d4-a716-446655440000",
    "currentState": "approved",
    "transitionExecuted": "approve",
    "executedAt": "2025-11-19T10:30:00Z"
  },
  "metadata": {
    "TaskType": "DirectTrigger",
    "Domain": "approvals",
    "Flow": "approval-flow",
    "TransitionName": "approve"
  }
}
```

### GetInstanceDataTask Yanıtı

```json
{
  "isSuccess": true,
  "data": {
    "userId": "user123",
    "profile": {
      "name": "John Doe",
      "email": "john.doe@example.com"
    },
    "preferences": {
      "language": "tr-TR",
      "theme": "dark"
    }
  },
  "metadata": {
    "TaskType": "GetInstanceData",
    "Domain": "users",
    "Flow": "user-profile"
  }
}
```

### SubProcessTask Yanıtı

```json
{
  "isSuccess": true,
  "data": {
    "instanceId": "660e8400-e29b-41d4-a716-446655440001",
    "state": "initial-state",
    "launched": true
  },
  "metadata": {
    "TaskType": "SubProcess",
    "Domain": "audit",
    "Key": "transaction-audit"
  }
}
```

### Hata Yanıtı

```json
{
  "isSuccess": false,
  "errorMessage": "Hedef iş akışı bulunamadı",
  "metadata": {
    "errorCode": "WORKFLOW_NOT_FOUND",
    "targetDomain": "approvals",
    "targetFlow": "approval-flow"
  }
}
```

## Hata Senaryoları

### İş Akışı Bulunamadı

```json
{
  "IsSuccess": false,
  "ErrorMessage": "'approvals' domain'inde 'approval-flow' iş akışı bulunamadı",
  "Metadata": {
    "ErrorCode": "WORKFLOW_NOT_FOUND",
    "TargetDomain": "approvals",
    "TargetFlow": "approval-flow"
  }
}
```

### Instance Bulunamadı

```json
{
  "IsSuccess": false,
  "ErrorMessage": "'550e8400-e29b-41d4-a716-446655440000' instance'ı bulunamadı",
  "Metadata": {
    "ErrorCode": "INSTANCE_NOT_FOUND",
    "InstanceId": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

### Transition Kullanılamıyor

```json
{
  "IsSuccess": false,
  "ErrorMessage": "'approve' transition'ı mevcut state'te kullanılamıyor",
  "Metadata": {
    "ErrorCode": "TRANSITION_NOT_AVAILABLE",
    "TransitionName": "approve",
    "CurrentState": "draft"
  }
}
```

## En İyi Pratikler

### 1. Task Türü Seçimi

```csharp
// ✅ Doğru - Yeni instance'lar için StartTask kullan
var startTask = task as StartTask;
startTask.SetDomain("approvals");
startTask.SetFlow("approval-flow");

// ✅ Doğru - Mevcut instance'larda transition için DirectTriggerTask kullan
var directTriggerTask = task as DirectTriggerTask;
directTriggerTask.SetInstance(existingInstanceId);
directTriggerTask.SetTransitionName("approve");

// ❌ Yanlış - StartTask'ı instanceId ile kullanma
var startTask = task as StartTask;
startTask.SetInstance(existingInstanceId); // StartTask'ta SetInstance metodu yok
```

### 2. Veri Hazırlama

```csharp
// ✅ Doğru - SubProcessTask için tam veri sağla
var subProcessTask = task as SubProcessTask;
subProcessTask.SetBody(new {
    // Bağımsız çalışma için gerekli tüm veriler
    userId = context.Instance.Data.userId,
    transactionId = context.Instance.Data.transactionId,
    parentInstanceId = context.Instance.Id,
});

// ❌ Yanlış - SubProcessTask için eksik veri
subProcessTask.SetBody(new {
    userId = context.Instance.Data.userId
    // Diğer gerekli alanlar eksik
});
```

### 3. Hata İşleme

```csharp
// ✅ Doğru - Hataları uygun şekilde ele al
public async Task<ScriptResponse> OutputHandler(ScriptContext context)
{
    var response = new ScriptResponse();
    
    if (context.Body.isSuccess)
    {
        response.Data = new {
            success = true,
            instanceId = context.Body.data.instanceId
        };
    }
    else
    {
        response.Data = new {
            success = false,
            error = context.Body.errorMessage,
            shouldRetry = ShouldRetryError(context.Body.errorMessage)
        };
    }
    
    return response;
}

private bool ShouldRetryError(string errorMessage)
{
    return errorMessage?.Contains("timeout") == true ||
           errorMessage?.Contains("unavailable") == true;
}
```

### 4. SubProcessTask vs StartTask

```csharp
// ✅ Fire-and-forget arka plan görevleri için SubProcessTask kullan
// Ana iş akışının tamamlanmasını beklemesi gerekmez
var subProcessTask = task as SubProcessTask;
subProcessTask.SetDomain("audit");
subProcessTask.SetKey("transaction-audit");

// ✅ Gelecekte etkileşim gerekebilecek iş akışları için StartTask kullan
// InstanceId'yi geri alırsınız ve daha sonra DirectTriggerTask ile transition tetikleyebilirsiniz
var startTask = task as StartTask;
startTask.SetDomain("approvals");
startTask.SetFlow("approval-flow");
```

### 5. Extension Kullanımı

```csharp
// ✅ Doğru - Sadece gerekli extension'ları talep et
var getDataTask = task as GetInstanceDataTask;
getDataTask.SetExtensions(new[] { "profile", "preferences" });

// ✅ En İyi Pratik - Extension'ları task config'inde tanımla
// Task JSON:
{
  "config": {
    "domain": "users",
    "flow": "user-profile",
    "extensions": ["profile", "preferences"]
  }
}
```

## Yaygın Kullanım Senaryoları

### Senaryo 1: Çok Aşamalı Onay İş Akışı

```csharp
// Doküman gönderildiğinde onay iş akışını başlat
public class StartApprovalWorkflow : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var startTask = task as StartTask;
        
        startTask.SetDomain("approvals");
        startTask.SetFlow("multi-stage-approval");
        
        startTask.SetBody(new {
            documentId = context.Instance.Data.documentId,
            approvalStages = new[] { "L1", "L2", "L3" },
            currentStage = 0,
            requester = context.Instance.Data.userId
        });
        
        return Task.FromResult(new ScriptResponse());
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        return new ScriptResponse
        {
            Data = new {
                approvalInstanceId = context.Body.data.instanceId,
                status = "APPROVAL_STARTED"
            }
        };
    }
}
```

### Senaryo 2: Dağıtık Transaction Koordinasyonu

```csharp
// Birden fazla iş akışı instance'ında transition'ları tetikle
public class CoordinateTransactionMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var directTriggerTask = task as DirectTriggerTask;
        
        // Ödeme iş akışında commit'i tetikle
        directTriggerTask.SetInstance(context.Instance.Data.paymentInstanceId);
        directTriggerTask.SetTransitionName("commit");
        
        directTriggerTask.SetBody(new {
            action = "commit",
            transactionId = context.Instance.Data.transactionId,
            timestamp = DateTime.UtcNow
        });
        
        return Task.FromResult(new ScriptResponse());
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        return new ScriptResponse
        {
            Data = new {
                commitSuccessful = context.Body.isSuccess,
                coordinationComplete = true
            }
        };
    }
}
```

### Senaryo 3: Audit İzi Oluşturma

```csharp
// Audit loglama için subprocess başlat
public class CreateAuditTrail : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var subProcessTask = task as SubProcessTask;
        
        subProcessTask.SetDomain("audit");
        subProcessTask.SetKey("transaction-audit");
        
        subProcessTask.SetBody(new {
            transactionId = context.Instance.Data.transactionId,
            userId = context.Instance.Data.userId,
            action = context.Instance.Data.action,
            timestamp = DateTime.UtcNow,
            details = context.Instance.Data
        });
        
        return Task.FromResult(new ScriptResponse());
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        // Fire-and-forget - sadece başlatıldığını onayla
        return new ScriptResponse
        {
            Data = new { auditLaunched = true }
        };
    }
}
```

### Senaryo 4: Kullanıcı Profil Verisi Alma

```csharp
// Kullanıcı profil verilerini al
public class GetUserProfileMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var getDataTask = task as GetInstanceDataTask;
        
        getDataTask.SetInstance(context.Instance.Data.userProfileInstanceId);
        getDataTask.SetExtensions(new[] { "profile", "preferences", "security" });
        
        return Task.FromResult(new ScriptResponse());
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        if (context.Body.isSuccess)
        {
            return new ScriptResponse
            {
                Data = new {
                    userProfile = context.Body.data,
                    loadedAt = DateTime.UtcNow
                }
            };
        }
        
        return new ScriptResponse
        {
            Data = new {
                error = "Profil verisi alınamadı",
                errorMessage = context.Body.errorMessage
            }
        };
    }
}
```

## Yaygın Sorunlar

### Sorun: Yanlış Task Türü Kullanımı
**Çözüm:** Her task türünün kendi kullanım amacı vardır. Yeni instance'lar için StartTask, transition'lar için DirectTriggerTask kullanın.

### Sorun: Eksik Gerekli Alanlar
**Çözüm:** Domain ve flow'un her zaman sağlandığını doğrulayın. DirectTriggerTask için transitionName gereklidir. SubProcessTask için key gereklidir.

### Sorun: SubProcessTask Bağımsız Değil
**Çözüm:** Body'de tam veri sağlayın. SubProcessTask'ler başlatıldıktan sonra ana iş akışı verilerine erişemez.

### Sorun: Extension'lar Yüklenmiyor
**Çözüm:** Extension'ların görev konfigürasyonunda veya mapping'te SetExtensions() ile tanımlandığından emin olun.

## Migration Notları

Eğer eski TriggerTransitionTask (type "11" ile nested "type" field) kullanıyorsanız, yeni task türlerine geçiş yapmanız gerekmektedir:

| Eski Type | Eski Nested Type | Yeni Task Type | Yeni Type Numarası |
|-----------|------------------|----------------|-------------------|
| "11" | "Start" | StartTask | "11" |
| "11" | "Trigger" | DirectTriggerTask | "12" |
| "11" | "GetInstanceData" | GetInstanceDataTask | "13" |
| "11" | "SubProcess" | SubProcessTask | "14" |

**Örnek Migration:**

**Eski (TriggerTransitionTask):**
```json
{
  "type": "11",
  "config": {
    "type": "Start",
    "domain": "approvals",
    "flow": "approval-flow"
  }
}
```

**Yeni (StartTask):**
```json
{
  "type": "11",
  "config": {
    "domain": "approvals",
    "flow": "approval-flow"
  }
}
```
