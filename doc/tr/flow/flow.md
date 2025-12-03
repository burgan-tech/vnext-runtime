# İş Akışı (Workflow) Tanımlaması

İş akışları (Workflows), iş süreçlerini modelleyen ve çalışma zamanında çalıştırılan temel bileşenlerdir. Her iş akışı belirli bir amaca yönelik durum makinesi olarak tasarlanır ve sistemde farklı türlerde tanımlanabilir.

:::highlight green 💡
İş akışları JSON formatında tanımlanır ve sistem tarafından otomatik olarak yüklenir. Her bir iş akışı domain'e özgü bir anahtar ile tanımlanır ve sürüm yönetimi desteklenir.
:::

## İş Akışı Türleri

Sistem şu anda 4 farklı iş akışı türünü desteklemektedir:

| İş Akışı Türü | Kod | Açıklama | Kullanım Alanları |
|---------------|-----|----------|-------------------|
| **Core** | C | Platform çekirdek iş akışları | Sistem işlemleri, platform servisleri |
| **Flow** | F | Ana iş akışları | İşletme ana süreçleri, kullanıcı etkileşimi |
| **SubFlow** | S | Alt iş akışları | Tekrar kullanılabilir süreç parçaları |
| **SubProcess** | P | Alt süreçler | Paralel ve bağımsız işlemler |

## İş Akışı Bileşenleri

### Temel Özellikler

#### Key (Anahtar)
- İş akışının benzersiz tanımlayıcısı
- Domain içinde benzersiz olmalıdır
- Dosya adı ile uyumlu olması önerilir

#### Domain (Etki Alanı)
- İş akışının hangi etki alanına ait olduğunu belirtir
- Mikroservis mimarisi için namespace sağlar
- Çoklu tenant desteği sunar

#### Version (Sürüm)
- Semantic versioning (SemVer) standardını kullanır
- Örnek: `"1.0.0"`, `"2.1.3"`
- Geriye uyumluluk kontrolü için kullanılır

#### Type (Tür)
- İş akışının hangi türde olduğunu belirler
- Çalışma zamanı davranışını etkiler
- Şemada `workflowType` olarak tanımlanır

### İsteğe Bağlı Bileşenler

#### Labels (Etiketler)
Çoklu dil desteği için kullanılır:
```json
"labels": [
  {
    "label": "Kullanıcı Kayıt Süreci",
    "language": "tr"
  },
  {
    "label": "User Registration Process",
    "language": "en"
  }
]
```

#### Timeout (Zaman Aşımı)
İş akışının otomatik sonlandırılması için:
```json
"timeout": {
  "key": "registration-timeout",
  "target": "timeout-state",
  "versionStrategy": "Minor",
  "timer": {
    "reset": "workflow-start",
    "duration": "PT24H"
  }
}
```

#### Functions (Fonksiyonlar)
Platform servisleri için fonksiyon referansları:
```json
"functions": [
  {
    "ref": "Functions/user-validation.json"
  }
]
```

#### Features (Özellikler)
Ortak kullanılan bileşen referansları:
```json
"features": [
  {
    "ref": "Features/document-upload.json"
  }
]
```

#### Extensions (Uzantılar)
İş akışı kayıt örneği zenginleştirme görevleri:
```json
"extensions": [
  {
    "ref": "Extensions/audit-logger.json"
  }
]
```

#### SharedTransitions (Paylaşımlı Geçişler)
Ortak geçişler (örnek: Cancel, Approve):
```json
"sharedTransitions": [
  {
    "key": "cancel",
    "target": "cancelled",
    "triggerType": "Manual",
    "labels": [...]
  }
]
```

#### Cancel (İptal)
İş akışının iptal davranışını tanımlar:
```json
"cancel": {
  "key": "cancel-account-opening",
  "target": "cancelled",
  "triggerType": 0,
  "versionStrategy": "Minor",
  "labels": [
    {
      "language": "en-US",
      "label": "Cancel Account Opening"
    }
  ],
  "onExecutionTasks": [],
  "availableIn": []
}
```

:::warning Kritik: Kademeli İptal (Cascading Cancel)
Alt korelasyonların iptal edilebilmesi için, alt akış tanımlarında da **cancel tanımının bulunması gerekmektedir**. Eğer alt akışta cancel tanımı yoksa, üst sistem isteği bildirir ancak yanıt alamazsa **bypass** eder.
:::

**Cancel Özellikleri:**
- `key`: İptal geçişinin benzersiz tanımlayıcısı
- `target`: İptal edildiğinde geçilecek bitiş durumu
- `triggerType`: İptalin nasıl tetikleneceği (genellikle Manual - 0)
- `versionStrategy`: Versiyon işleme stratejisi (Major/Minor)
- `labels`: İptal aksiyonu için çoklu dil etiketleri
- `onExecutionTasks`: İptal sırasında çalıştırılacak görevler
- `availableIn`: İptalin kullanılabilir olduğu durumlar (boş array tüm durumlar anlamına gelir)

**İptal edilenler:**
- İş akışı instance'ı ile ilişkili aktif job'lar
- İş akışında çalışan aktif task'lar
- Aktif korelasyonlar (SubFlow'lar ve SubProcess'ler)

## Başlangıç Geçişi (Start Transition)

:::warning Zorunlu Bileşen
Tüm iş akışları `startTransition` bileşenine sahip olmalıdır. Bu, iş akışının nasıl başlatılacağını tanımlar.
:::

### Start Transition Özellikleri

```json
"startTransition": {
  "key": "start",
  "target": "initial-state",
  "triggerType": "Manual",
  "versionStrategy": "Major",
  "schema": {
    "ref": "Schemas/start-schema.json"
  },
  "labels": [
    {
      "label": "Süreci Başlat",
      "language": "tr"
    }
  ],
  "onExecutionTasks": [
    {
      "order": 1,
      "task": {
        "ref": "Tasks/initialize-data.json"
      },
      "mapping": {
        "location": "./src/InitializeDataMapping.csx",
        "code": "<BASE64>"
      }
    }
  ]
}
```

**Dikkat Edilmesi Gerekenler:**
- Start transition'ın `from` özelliği yoktur
- İlk state'e yönlendirmelidir (`target`)
- Genellikle `Manual` trigger type kullanılır
- Schema ile başlangıç verilerini validate edebilir

## Durumlar (States)

İş akışının farklı aşamalarını temsil eder. Detaylı bilgi için: [📄 State Dokümantasyonu](./state.md)

### State Türleri
- **Initial**: Başlangıç durumu
- **Intermediate**: Ara işlem durumları  
- **Finish**: Sonlandırma durumları
- **SubFlow**: Alt iş akışı çalıştıran durumlar

### Örnek State Tanımı
```json
"states": [
  {
    "key": "user-verification",
    "stateType": "Intermediate",
    "versionStrategy": "Minor",
    "labels": [...],
    "transitions": [...],
    "onEntries": [...],
    "onExits": [...]
  }
]
```

## İş Akışı Şeması

```json
{
  "key": "string",
  "domain": "string", 
  "version": "string",
  "type": "C (Core)|F (Flow)|S (SubFlow)|P (SubProcess)",
  "timeout": {
    "key": "string",
    "target": "string",
    "versionStrategy": "Major|Minor",
    "timer": {
      "reset": "string",
      "duration": "string (ISO 8601)"
    }
  },
  "cancel": {
    "key": "string",
    "target": "string",
    "triggerType": "0 (Manual)|1 (Automatic)|2 (Scheduled)|3 (Event)",
    "versionStrategy": "Major|Minor",
    "labels": [...],
    "onExecutionTasks": [...],
    "availableIn": ["state1", "state2"]
  },
  "labels": [
    {
      "label": "string",
      "language": "string"
    }
  ],
  "functions": [
    {
      "ref": "string"
    }
  ],
  "features": [
    {
      "ref": "string"
    }
  ],
  "extensions": [
    {
      "ref": "string"
    }
  ],
  "sharedTransitions": [
    {
      "key": "string",
      "target": "string",
      "triggerType": "0 (Manual)|1 (Automatic)| 2 (Scheduled)|3 (Event)",
      "versionStrategy": "Major|Minor",
      "availableIn": ["state1", "state2"],
      "labels": [...],
      "onExecutionTasks": [...]
    }
  ],
  "startTransition": {
    "key": "start",
    "target": "string",
    "triggerType": "Manual",
    "versionStrategy": "Major|Minor",
    "schema": {
      "ref": "string"
    },
    "labels": [...],
    "onExecutionTasks": [...]
  },
  "states": [
    {
      "key": "string",
      "stateType": "1 (Initial)|2 (Intermediate)|3 (Finish)|4 (SubFlow)",
      "versionStrategy": "Major|Minor",
      "labels": [...],
      "transitions": [...],
      "onEntries": [...],
      "onExits": [...],
      "view": {
        "ref": "string"
      },
      "subFlow": {
        "type": "S (SubFlow)|P (SubProcess)",
        "process": {
          "ref": "string"
        },
        "mapping": {
          "location": "string",
          "code": "string (BASE64)"
        }
      }
    }
  ]
}
```

## İş Akışı Geliştirme Rehberi

### 1. İş Akışı Planlaması

İş akışı tasarlamadan önce aşağıdaki sorulara yanıt verin:

**Temel Sorular:**
- İş süreci hangi aşamalardan oluşuyor?
- Hangi durumlar arasında geçişler var?
- Otomatik ve manuel geçişler nelerdir?
- Zaman aşımı durumları var mı?
- Alt süreçler gerekli mi?

**Teknik Sorular:**
- Başlangıç verisi şeması nedir?
- Hangi görevlerin çalıştırılması gerekir?
- Harici sistem entegrasyonları var mı?
- Veri dönüşümleri gerekli mi?

### 2. İş Akışı Yapısı

#### Domain ve Key Belirleme
```json
{
  "key": "user-registration",
  "domain": "authentication"
}
```

**Dikkat Edilmesi Gerekenler:**
- Key değeri domain içinde benzersiz olmalı
- Dosya adı ile uyumlu olmalı
- Kebab-case kullanımı önerilir

#### Type Seçimi
- **Flow**: Ana kullanıcı süreçleri için
- **SubFlow**: Tekrar kullanılabilir alt süreçler için  
- **SubProcess**: Paralel işlemler için
- **Core**: Platform işlemleri için (sadece sistem)

#### Version Stratejisi
```json
{
  "version": "1.0.0",
  "versionStrategy": "Major"
}
```

### 3. State Tasarımı

#### Initial State
Her iş akışında sadece bir adet initial state olmalıdır:
```json
{
  "key": "start",
  "stateType": "Initial",
  "transitions": [
    {
      "key": "begin-verification",
      "target": "verification",
      "triggerType": 1
    }
  ]
}
```

#### Intermediate States
İş mantığının yürütüldüğü durumlar:
```json
{
  "key": "verification",
  "stateType": 2,
  "onEntries": [
    {
      "order": 1,
      "task": {
        "ref": "Tasks/send-verification-email.json"
      }
    }
  ],
  "transitions": [
    {
      "key": "verify-email",
      "target": "verified", 
      "triggerType": 0
    },
    {
      "key": "timeout",
      "target": "failed",
      "triggerType": 3,
      "timer": {...}
    }
  ]
}
```

#### Finish States
Süreç sonlandırma durumları:
```json
{
  "key": "completed",
  "stateType": "Finish",
  "onEntries": [
    {
      "order": 1,
      "task": {
        "ref": "Tasks/send-welcome-email.json"
      }
    }
  ]
}
```

### 4. Geçiş (Transition) Tasarımı

Detaylı bilgi için: [📄 Transition Dokümantasyonu](./transition.md)

#### Manual Transitions
Kullanıcı etkileşimi gerektiren geçişler:
```json
{
  "key": "approve",
  "target": "approved",
  "triggerType": 0,
  "schema": {
    "ref": "Schemas/approval-schema.json"
  }
}
```

#### Automatic Transitions  
Koşullu otomatik geçişler:
```json
{
  "key": "auto-approve",
  "target": "approved", 
  "triggerType": 1,
  "rule": {
    "location": "./src/AutoApprovalRule.csx",
    "code": "<BASE64>"
  }
}
```

#### Scheduled Transitions
Zaman tabanlı geçişler:
```json
{
  "key": "daily-check",
  "target": "checking",
  "triggerType": 2, 
  "timer": {
    "location": "./src/DailyCheckTimer.csx",
    "code": "<BASE64>"
  }
}
```

### 5. Görev (Task) Entegrasyonu

Detaylı bilgi için: [📄 Task Dokümantasyonu](./task.md)

#### OnExecutionTasks
Geçiş sırasında çalışan görevler:
```json
"onExecutionTasks": [
  {
    "order": 1,
    "task": {
      "ref": "Tasks/validate-user.json"
    },
    "mapping": {
      "location": "./src/ValidateUserMapping.csx", 
      "code": "<BASE64>"
    }
  },
  {
    "order": 2,
    "task": {
      "ref": "Tasks/send-notification.json"
    },
    "mapping": {
      "location": "./src/NotificationMapping.csx",
      "code": "<BASE64>"
    }
  }
]
```

**Görev Sıralaması:**
- Aynı `order` değerine sahip görevler paralel çalışır
- Farklı `order` değerleri sıralı çalışır
- Order değerleri artan sırada işlenir

### 6. Mapping ve Interface Kullanımı

Detaylı bilgi için: [📄 Mapping Dokümantasyonu](./mapping.md) ve [📄 Interface Dokümantasyonu](./interface.md)

#### Temel Mapping Örneği
```csharp
public class UserRegistrationMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        
        // Input data hazırlama
        var userData = new
        {
            email = context.Body?.email,
            name = context.Body?.name,
            timestamp = DateTime.UtcNow
        };
        
        httpTask.SetBody(userData);
        
        return Task.FromResult(new ScriptResponse());
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse
        {
            Data = new
            {
                userId = context.Body?.data?.userId,
                registrationDate = DateTime.UtcNow
            }
        });
    }
}
```

#### Timer Mapping Örneği
```csharp
public class RegistrationTimeoutRule : ITimerMapping
{
    public async Task<TimerSchedule> Handler(ScriptContext context)
    {
        // 24 saat sonra timeout
        return TimerSchedule.FromDuration(TimeSpan.FromHours(24));
    }
}
```

#### Condition Mapping Örneği
```csharp
public class AutoApprovalRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        var amount = context.Instance.Data.amount != null
            ? Convert.ToDecimal(context.Instance.Data.amount)
            : 0m;
            
        // 1000 TL altı otomatik onay
        return amount < 1000;
    }
}
```

## Best Practices

### 1. İş Akışı Yapısı
- **Single Responsibility**: Her state tek bir sorumluluğa sahip olmalı
- **Clear Naming**: Anlaşılır state ve transition isimleri kullanın
- **Minimize States**: Gereksiz state'lerden kaçının
- **Error Handling**: Hata durumları için özel state'ler tanımlayın

### 2. Performance
- **Parallel Tasks**: Mümkün olduğunda görevleri paralel çalıştırın
- **Async Operations**: Uzun süren işlemler için async patterns kullanın
- **State Optimization**: Gereksiz state geçişlerinden kaçının
- **Data Size**: Büyük veri setlerini state'de saklamayın

### 3. Güvenlik
- **Input Validation**: Tüm girişleri validate edin
- **Schema Usage**: Veri şemalarını kullanın
- **Secret Management**: Hassas verileri ScriptBase ile yönetin
- **Authorization**: Uygun yetkilendirme kontrolleri ekleyin

### 4. Bakım ve Debug
- **Logging**: Adequate logging ekleyin
- **Error Messages**: Anlaşılır hata mesajları kullanın
- **Documentation**: İş akışını dokümante edin
- **Testing**: Unit test'ler yazın

### 5. Versiyon Yönetimi
- **Semantic Versioning**: SemVer standardını takip edin
- **Breaking Changes**: Major version değişikliklerinde dikkatli olun
- **Backward Compatibility**: Geriye uyumluluğu koruyun
- **Migration Strategy**: Veri migrasyonu planı yapın

## Örnekler

### Basit Onay Süreci
```json
{
  "type": "F",
  "startTransition": {
    "key": "start",
    "target": "pending",
    "triggerType": 0
  },
  "states": [
    {
      "key": "pending",
      "stateType": 1,
      "transitions": [
        {
          "key": "approve",
          "target": "approved",
          "triggerType": 0
        },
        {
          "key": "reject", 
          "target": "rejected",
          "triggerType": 0
        }
      ]
    },
    {
      "key": "approved",
      "stateType": "Finish"
    },
    {
      "key": "rejected",
      "stateType": "Finish"
    }
  ]
}
```

### OAuth Kimlik Doğrulama
Gerçek OAuth örneği için: `samples/oauth/Workflows/oauth-authentication-workflow.json`

### Zamanlanmış Ödeme Süreci  
Gerçek ödeme örneği için: `samples/payments/Workflows/scheduled-payments-workflow.json`

## Sık Karşılaşılan Hatalar

### 1. Missing Initial State
```
Error: Workflow must have exactly one Initial state
```
**Çözüm**: Sadece bir adet `stateType: "1"` tanımlayın

### 2. Invalid Transition Target
```
Error: Transition target 'invalid-state' not found
```
**Çözüm**: Target state'in states array'inde tanımlı olduğundan emin olun

### 3. Missing Start Transition
```
Error: Workflow must have startTransition
```
**Çözüm**: `startTransition` bileşenini tanımlayın

### 4. Invalid JSON Schema
```
Error: JSON schema validation failed
```
**Çözüm**: JSON formatını ve şema uyumluluğunu kontrol edin

## İlgili Dokümantasyon

- [📄 State (Durum) Belgesi](./state.md)
- [📄 Task (Görev) Belgesi](./task.md)  
- [📄 Transition (Geçiş) Belgesi](./transition.md)
- [📄 Interface Belgesi](./interface.md)
- [📄 Mapping (Haritalama) Rehberi](./mapping.md)

Bu dokümantasyon, iş akışı tanımlaması için kapsamlı bir rehber sunmaktadır. Geliştiriciler bu rehberi takip ederek etkili ve verimli iş akışları oluşturabilirler.
