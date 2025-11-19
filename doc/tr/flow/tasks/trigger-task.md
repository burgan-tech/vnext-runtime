# Trigger Task (Tetikleme Görevi)

Trigger Task, kapsamlı iş akışı instance yönetim yetenekleri sağlayan birleşik bir görev türüdür. İş akışlarının yeni instance'lar başlatmasına, transition'ları tetiklemesine, subprocess'ler başlatmasına ve instance verilerini almasına olanak tanır—tümü iş akışı yürütme bağlamı içinde.

## Özellikler

- ✅ Programatik olarak yeni iş akışı instance'ları başlatma
- ✅ Mevcut instance'larda transition tetikleme (doğrudan veya korelasyon tabanlı)
- ✅ Bağımsız subprocess instance'ları başlatma
- ✅ Extension desteği ile instance verilerini alma
- ✅ Çapraz iş akışı orkestrasyon
- ✅ Dinamik iş akışı kompozisyonu
- ✅ Esnek tetikleme türü konfigürasyonu
- ✅ Detaylı yanıt takibi

## Görev Tanımı

### Temel Yapı

```json
{
  "key": "trigger-workflow-action",
  "flow": "sys-tasks",
  "domain": "core",
  "version": "1.0.0",
  "tags": [
    "trigger",
    "workflow",
    "orchestration"
  ],
  "attributes": {
    "type": "11",
    "config": {
      "type": "Start",
      "domain": "target-domain",
      "flow": "target-flow",
      "key": "target-key",
      "version": "1.0.0"
    }
  }
}
```

### Konfigürasyon Alanları

Trigger Task'ın config bölümünde tanımlanan alanlar:

| Alan | Tür | Gerekli | Açıklama |
|------|-----|---------|----------|
| `type` | string | Evet | Tetikleme türü: "Start", "Trigger", "SubProcess", "GetInstanceData" |
| `domain` | string | Evet | Hedef iş akışı domain'i |
| `flow` | string | Evet | Hedef iş akışı flow adı |
| `key` | string | Koşullu | Hedef iş akışı key'i (Start, SubProcess için gerekli) |
| `instanceId` | string | Koşullu | Hedef instance ID'si (Trigger, GetInstanceData için gerekli) |
| `transitionName` | string | Koşullu | Çalıştırılacak transition adı (Trigger türü için gerekli) |
| `version` | string | Hayır | Hedef iş akışı versiyonu (opsiyonel) |
| `extensions` | string[] | Hayır | Alınacak extension'lar (GetInstanceData türü için) |
| `body` | object | Hayır | İstekle gönderilecek veri |

## Tetikleme Türleri

Trigger Task, `TriggerTransitionType` enum'u aracılığıyla dört farklı işlem türünü destekler:

### 1. Instance Başlatma (Type: Start = 1)

Yeni bir iş akışı instance'ı oluşturur. İş akışı yürütmesi sırasında programatik olarak yeni iş akışı instance'ları başlatmak için bu tetikleme türünü kullanın.

**Konfigürasyon Örneği:**
```json
{
  "key": "start-approval-workflow",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-tasks",
  "tags": ["workflow", "instance", "start"],
  "attributes": {
    "type": "11",
    "config": {
      "type": "Start",
      "domain": "approvals",
      "flow": "approval-flow",
      "key": "document-approval",
      "version": "1.0.0"
    }
  }
}
```

**Kullanım Alanları:**
- Ana iş süreçlerinden onay iş akışları başlatma
- Transaction loglama için audit iş akışları oluşturma
- Bildirim iş akışları başlatma
- Paralel işleme iş akışları başlatma

### 2. Transition Tetikleme (Type: Trigger = 2)

Mevcut bir iş akışı instance'ında belirli bir transition'ı yürütür. Diğer iş akışı instance'larında state transition'larını tetiklemek için kullanın.

**Konfigürasyon Örneği:**
```json
{
  "key": "trigger-approval-action",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-tasks",
  "tags": ["transition", "trigger"],
  "attributes": {
    "type": "11",
    "config": {
      "type": "Trigger",
      "domain": "approvals",
      "flow": "approval-flow",
      "transitionName": "approve"
    }
  }
}
```

**Kullanım Alanları:**
- Harici sistemlerden iş akışlarını onaylama veya reddetme
- Bağımlı iş akışlarında durum güncellemelerini tetikleme
- Çoklu iş akışı süreçlerini koordine etme
- İş akışı callback'lerini uygulama

### 3. SubProcess Başlatma (Type: SubProcess = 3)

Ana iş akışı ile paralel çalışan bağımsız bir subprocess instance'ı başlatır. Subprocess'ler, ana iş akışını bloke etmeyen fire-and-forget işlemlerdir.

**Konfigürasyon Örneği:**
```json
{
  "key": "start-audit-subprocess",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-tasks",
  "tags": ["subprocess", "audit"],
  "attributes": {
    "type": "11",
    "config": {
      "type": "SubProcess",
      "domain": "audit",
      "flow": "audit-flow",
      "key": "transaction-audit",
      "version": "1.0.0"
    }
  }
}
```

**Kullanım Alanları:**
- Arka planda audit loglama
- Asenkron bildirim gönderme
- Paralel veri işleme
- Bağımsız raporlama iş akışları
- Event-driven yan etkiler

**SubProcess vs SubFlow:**
- **SubProcess**: Fire-and-forget, bağımsız çalışır, ana iş akışını bloke etmez
- **SubFlow**: Ana iş akışını bloke eder, veri döner, sıkı entegrasyon

### 4. Instance Verisi Alma (Type: GetInstanceData = 4)

Başka bir iş akışı instance'ından instance verilerini alır. Ek ilgili verileri almak için opsiyonel extension'ları destekler.

**Konfigürasyon Örneği:**
```json
{
  "key": "get-user-profile-data",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-tasks",
  "tags": ["instance", "data", "fetch"],
  "attributes": {
    "type": "11",
    "config": {
      "type": "GetInstanceData",
      "domain": "users",
      "flow": "user-profile",
      "extensions": ["profile", "preferences", "security"]
    }
  }
}
```

**Kullanım Alanları:**
- Kişiselleştirme için kullanıcı profil verilerini alma
- Merkezi iş akışlarından konfigürasyon alma
- Master iş akışlarından referans verileri yükleme
- Birden fazla iş akışı instance'ından veri toplama
- Çapraz iş akışı veri federasyonu

## Property Erişimi

TriggerTransitionTask sınıfındaki property'lere setter metodları ile erişilir:

- **TriggerDomain**: `SetDomain(string domain)` metodu ile ayarlanır
- **TriggerFlow**: `SetFlow(string flow)` metodu ile ayarlanır
- **TriggerKey**: `SetKey(string key)` metodu ile ayarlanır
- **TriggerInstanceId**: `SetInstance(string instanceId)` metodu ile ayarlanır
- **TriggerType**: `SetTriggerType(string type)` metodu ile ayarlanır
- **Body**: `SetBody(dynamic body)` metodu ile ayarlanır

## Mapping Örnekleri

### Örnek 1: Yeni Instance Başlatma

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class StartApprovalMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var triggerTask = task as TriggerTransitionTask;
        
        // Hedef iş akışını yapılandır
        triggerTask.SetDomain("approvals");
        triggerTask.SetFlow("approval-flow");
        triggerTask.SetKey("document-approval");
        triggerTask.SetTriggerType("Start");
        
        // Başlangıç verilerini hazırla
        triggerTask.SetBody(new {
            documentId = context.Instance.Data.documentId,
            documentType = context.Instance.Data.documentType,
            requestedBy = context.Instance.Data.userId,
            approvalLevel = "L1",
            priority = "HIGH",
            requestedAt = DateTime.UtcNow,
            metadata = new {
                sourceInstanceId = context.Instance.Id,
                correlationId = context.Instance.CorrelationId
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
                approvalInstanceId = context.Body.data.instanceId,
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

### Örnek 2: Transition Tetikleme

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class TriggerApprovalMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var triggerTask = task as TriggerTransitionTask;
        
        // Hedef instance'ı ayarla
        triggerTask.SetInstance(context.Instance.Data.approvalInstanceId);
        triggerTask.SetTriggerType("Trigger");
        
        // Transition payload'ını hazırla
        triggerTask.SetBody(new {
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
                newState = context.Body.data?.currentState,
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

### Örnek 3: SubProcess Başlatma

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class StartAuditSubProcessMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var triggerTask = task as TriggerTransitionTask;
        
        // Subprocess'i yapılandır
        triggerTask.SetDomain("audit");
        triggerTask.SetFlow("audit-flow");
        triggerTask.SetKey("transaction-audit");
        triggerTask.SetTriggerType("SubProcess");
        
        // Subprocess başlangıç verilerini hazırla
        triggerTask.SetBody(new {
            transactionId = context.Instance.Data.transactionId,
            transactionType = context.Instance.Data.transactionType,
            amount = context.Instance.Data.amount,
            currency = context.Instance.Data.currency,
            userId = context.Instance.Data.userId,
            action = context.Instance.Data.action,
            timestamp = DateTime.UtcNow,
            parentInstanceId = context.Instance.Id,
            correlationId = context.Instance.CorrelationId,
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
            auditSubProcessId = context.Body.data?.instanceId,
            auditInitiated = true,
            initiatedAt = DateTime.UtcNow,
            status = "AUDIT_SUBPROCESS_LAUNCHED"
        };
        
        return response;
    }
}
```

### Örnek 4: Instance Verisi Alma

```csharp
using System;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class GetUserProfileDataMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var triggerTask = task as TriggerTransitionTask;
        
        // Veri alınacak hedef instance'ı ayarla
        triggerTask.SetInstance(context.Instance.Data.userProfileInstanceId);
        triggerTask.SetTriggerType("GetInstanceData");
        
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

## Standart Yanıt

Trigger Task aşağıdaki standart yanıt yapısını döner:

```csharp
{
    "Data": {
        "instanceId": "guid",           // Start ve SubProcess türleri için
        "currentState": "state-name",   // Trigger türü için
        "data": { /* instance data */ } // GetInstanceData türü için
    },
    "IsSuccess": true,
    "ErrorMessage": null,
    "Metadata": {
        "TriggerType": "Start",
        "TargetDomain": "approvals",
        "TargetFlow": "approval-flow"
    },
    "ExecutionDurationMs": 145,
    "TaskType": "TriggerTransition"
}
```

### Türe Göre Başarılı Yanıtlar

#### Instance Başlatma Yanıtı
```json
{
  "isSuccess": true,
  "data": {
    "instanceId": "550e8400-e29b-41d4-a716-446655440000",
    "state": "initial-state",
    "createdAt": "2025-11-19T10:30:00Z"
  }
}
```

#### Transition Tetikleme Yanıtı
```json
{
  "isSuccess": true,
  "data": {
    "instanceId": "550e8400-e29b-41d4-a716-446655440000",
    "currentState": "approved",
    "transitionExecuted": "approve",
    "executedAt": "2025-11-19T10:30:00Z"
  }
}
```

#### SubProcess Yanıtı
```json
{
  "isSuccess": true,
  "data": {
    "instanceId": "660e8400-e29b-41d4-a716-446655440001",
    "state": "initial-state",
    "launched": true
  }
}
```

#### GetInstanceData Yanıtı
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

### 1. Tetikleme Türü Seçimi
```csharp
// ✅ Doğru - Yeni instance'lar için Start kullan
triggerTask.SetTriggerType("Start");
triggerTask.SetKey("new-workflow");

// ✅ Doğru - Mevcut instance'lar için Trigger kullan
triggerTask.SetTriggerType("Trigger");
triggerTask.SetInstance(existingInstanceId);

// ❌ Yanlış - Start'ı instanceId ile kullanma
triggerTask.SetTriggerType("Start");
triggerTask.SetInstance(existingInstanceId); // Bu başarısız olur
```

### 2. Veri Hazırlama
```csharp
// ✅ Doğru - SubProcess için tam veri sağla
triggerTask.SetBody(new {
    // Bağımsız çalışma için gerekli tüm veriler
    userId = context.Instance.Data.userId,
    transactionId = context.Instance.Data.transactionId,
    parentInstanceId = context.Instance.Id,
    correlationId = context.Instance.CorrelationId
});

// ❌ Yanlış - SubProcess için eksik veri
triggerTask.SetBody(new {
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

### 4. SubProcess vs Instance Başlatma
```csharp
// ✅ Fire-and-forget arka plan görevleri için SubProcess kullan
// Ana iş akışının tamamlanmasını beklemesi gerekmez
triggerTask.SetTriggerType("SubProcess");

// ✅ Gelecekte etkileşim gerekebilecek iş akışları için Start kullan
// InstanceId'yi geri alırsınız ve daha sonra transition tetikleyebilirsiniz
triggerTask.SetTriggerType("Start");
```

### 5. Extension Kullanımı
```csharp
// ✅ Doğru - Sadece gerekli extension'ları talep et
var triggerTask = task as TriggerTransitionTask;
// Extension'lar zaten görev tanımında yapılandırılmıştır
// Mapping'te tekrar ayarlamaya gerek yok

// ✅ En İyi Pratik - Extension'ları task config'inde tanımla
// Task JSON:
{
  "config": {
    "type": "GetInstanceData",
    "extensions": ["profile", "preferences"]
  }
}
```

## Yaygın Kullanım Senaryoları

### Kullanım Senaryosu 1: Çok Aşamalı Onay İş Akışı
```csharp
// Doküman gönderildiğinde onay iş akışını başlat
public class StartApprovalWorkflow : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var triggerTask = task as TriggerTransitionTask;
        
        triggerTask.SetTriggerType("Start");
        triggerTask.SetDomain("approvals");
        triggerTask.SetFlow("multi-stage-approval");
        triggerTask.SetKey("document-approval");
        
        triggerTask.SetBody(new {
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

### Kullanım Senaryosu 2: Dağıtık Transaction Koordinasyonu
```csharp
// Birden fazla iş akışı instance'ında transition'ları tetikle
public class CoordinateTransactionMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var triggerTask = task as TriggerTransitionTask;
        
        // Ödeme iş akışında commit'i tetikle
        triggerTask.SetInstance(context.Instance.Data.paymentInstanceId);
        triggerTask.SetTriggerType("Trigger");
        
        triggerTask.SetBody(new {
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

### Kullanım Senaryosu 3: Audit İzi Oluşturma
```csharp
// Audit loglama için subprocess başlat
public class CreateAuditTrail : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var triggerTask = task as TriggerTransitionTask;
        
        triggerTask.SetTriggerType("SubProcess");
        triggerTask.SetDomain("audit");
        triggerTask.SetFlow("audit-trail");
        triggerTask.SetKey("transaction-audit");
        
        triggerTask.SetBody(new {
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

## Yaygın Sorunlar

### Sorun: Tetikleme Türü Uyumsuzluğu
**Çözüm:** Tetikleme türünün işlemle eşleştiğinden emin olun. Yeni instance'lar için "Start", transition'lar için "Trigger" kullanın.

### Sorun: Eksik Gerekli Alanlar
**Çözüm:** Domain ve flow'un her zaman sağlandığını doğrulayın. Key, Start/SubProcess için; instanceId, Trigger/GetInstanceData için gereklidir.

### Sorun: SubProcess Bağımsız Değil
**Çözüm:** Body'de tam veri sağlayın. SubProcess'ler başlatıldıktan sonra ana iş akışı verilerine erişemez.

### Sorun: Extension'lar Yüklenmiyor
**Çözüm:** Extension'ların görev konfigürasyonunda tanımlandığından, mapping kodunda değil, emin olun.

