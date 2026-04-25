Bir akış tanımında, state'ler arasındaki geçişleri yöneten bileşenlere **Geçiş** (Transition) denir. Transition'lar state geçişlerini yönetmekle yükümlüdür ve farklı tetikleme türlerine göre çalışabilir.

## Transition Özellikleri

### Temel Özellikler
- **Key**: Transition'ın benzersiz anahtarı
- **From**: Hangi state'den geçiş yapılacağını belirtir (null olabilir)
- **Target**: Hangi state'e geçiş yapılacağını belirler
- **TriggerType**: Transition'ın tetikleme türü
- **VersionStrategy**: Data versiyonu değişiklik stratejisi (Major/Minor)

### İsteğe Bağlı Özellikler
- **Timer**: Zamanlanmış transition'lar için kullanılan mapping kodu
- **Rule**: Otomatik transition'lar için koşul mapping kodu (veya **`location`** = **`dynamicExpresso`** iken **Dynamic Expresso** — v0.0.43+)
- **Schema**: Transition'da iletilen payload'ı validate etmek için kullanılan şema referansı
- **AvailableIn**: Shared transition'lar için hangi state'lerde çalıştırılabilir olduğunu belirtir
- **Labels**: Çoklu dil desteği için etiketler
- **View**: Transition'ın görünüm referansı
- **OnExecutionTasks**: Transition çalıştırıldığında yürütülecek görevler

## Trigger Türleri (TriggerType)

### Manual (0)
Client tarafından çağrılan transition'dır. Kullanıcı etkileşimi ile tetiklenir.

**Kullanım Alanları:**
- Kullanıcı buton tıklamaları
- Form gönderimleri  
- Manuel onay süreçleri

### Automatic (1)
Sistem tarafından otomatik olarak çalıştırılan koşullu transition'dır. 

**Özellikler:**
- `Rule` alanında mapping ile tanımlanır
- `IConditionMapping` arayüzünü runtime'da compile edip execute eder
- Belirli koşullar sağlandığında otomatik tetiklenir

**Kullanım Alanları:**
- İş kurallarına dayalı otomatik geçişler
- Durum kontrolü sonrası otomatik ilerlemeler
- Veri validasyonu sonrası geçişler

#### Kural ifadeleri ve Dynamic Expresso (v0.0.43+)

**Dynamic Expresso ifade kuralları (isteğe bağlı).** **Roslyn** ile derlenen **`IConditionMapping`** betiklerine ek olarak, otomatik transition **Rule** tanımı, **Dynamic Expresso** ile değerlendirilen düz metin **boolean** ifadeleri kullanabilir; bu mod uygun olduğunda ayrı bir derlenmiş koşul betiği gerekmez.

**Seçim**

- **`rule.location`** değerini **`dynamicExpresso`** yapın ve ifadeyi **`rule.code`** içine **native** kodlama ile koyun: JSON’da **`"encoding": "NAT"`**, kod tarafında **`ScriptCode.FromNative`**.
- Başka bir **`location`** değeri, Roslyn koşul betiği yolunu kullanmaya devam eder (**RoutingConditionEvaluator** → **ScriptConditionEvaluator**).

**Kök bağlama**

İfadeler, **`ScriptContext`** üzerinden yalnızca izin verilen üyelerle oluşturulan **`ExpressoRuleContext`** tipinde tek parametre **`context`** alır:

| `context` üyesi | Kullanılabilir yüzey |
|-----------------|----------------------|
| **`Body`** | İstek gövdesi |
| **`CurrentTransition`** | **`Data`**, **`Header`**; kalıcı transition isteği dışında **null** olabilir |
| **`MetaData`** | Meta veri |
| **`Workflow`** | **`key`**, **`domain`**, **`flow`**, **`version`**, **`StateKeys`** |
| **`Instance`** | **`Id`**, **`Key`**, **`Flow`**, state alanları, JSON olarak **`Data`** |
| **`Headers`** | HTTP başlıkları |
| **`QueryParameters`** | Sorgu parametreleri |
| **`RouteValues`** | Route verisinden JSON nesne |
| **`Transition`** | **`Key`**, **`From`**, **`Target`**, **`TriggerType`**, **`TriggerKind`**; betik bağlamında yoksa **null**; gömülü rule/timer/task payload’ları **yok** |
| **`Runtime`** | **`Domain`**, **`Version`**; set edilmemişse **null** olabilir |

**`Instance.Data` / `Body` vb. altındaki JSON**

JSON, **`RuleJsonDynamic`** olarak sunulur: dynamic üye erişimi, string indeksleyiciler, dizi **`Count`**, dizi **`Contains`**. Eksik nesne anahtarları veya eksik nokta-üzerinden özellikler **null** çözümlenir (çalışma zamanı binder hatası yok); Dynamic Expresso’nun desteklediği yerlerde **`?.`** ve **`??`** kullanılabilir.

Sayısal JSON için **`AsDouble()`** / **`AsInt32()`**, boolean için **`AsBoolean()`**, dizi uzunluğu için **`AsArrayLength()`** kullanın; JSON dizilerinde **`Contains`** desteklenir.

**İfade örnekleri**

```text
context.Instance.Data["amount"].AsDouble() > 100000
context.Instance.Data["documents"].AsArrayLength() == 0
context.Instance.Data["flags"]["manualReviewRequired"].AsBoolean() == false
context.Instance.Data["approvers"].Contains("u1")
context.Body["score"].AsDouble() >= 80
context.RouteValues["entityId"].ToString() == context.Instance.Data["externalId"].ToString()
context.Transition != null && context.Transition.Key == "approve-auto"
context.Runtime != null && context.Runtime.Domain.ToString() == "my-domain"
```

**JSON kural örnekleri** (otomatik transition tanımı parçası; **`encoding: NAT`** = native / düz ifade)

```json
"rule": {
  "location": "dynamicExpresso",
  "encoding": "NAT",
  "code": "context.Instance.Data.absenceType.ToString() == \"personal-leave\""
}
```

```json
"rule": {
  "location": "dynamicExpresso",
  "encoding": "NAT",
  "code": "context.Headers.sub.ToString() == context.Instance.Data.customerId.ToString()"
}
```

> **Referans:** [#470](https://github.com/burgan-tech/vnext/issues/470)

#### Varsayılan Otomatik Geçiş (Default Auto Transition) (v0.0.29+)

Birden fazla otomatik transition tanımlandığında ve hiçbirinin koşulu sağlanmadığında, yedek olarak bir **Varsayılan Otomatik Geçiş** tetiklenebilir.

**Yapılandırma:**
```json
{
  "triggerKind": 10
}
```

**TriggerKind Değerleri:**
| Değer | Açıklama |
|-------|----------|
| 0 | Uygulanamaz (standart auto transition) |
| 10 | Varsayılan Otomatik Geçiş (Default Auto Transition) |

**Örnek Kullanım:**
```json
{
  "transitions": [
    {
      "key": "approve",
      "target": "approved",
      "triggerType": 1,
      "rule": { "ref": "Mappings/check-approval.cs" }
    },
    {
      "key": "reject",
      "target": "rejected",
      "triggerType": 1,
      "rule": { "ref": "Mappings/check-rejection.cs" }
    },
    {
      "key": "pending-review",
      "target": "pending",
      "triggerType": 1,
      "triggerKind": 10
    }
  ]
}
```

Bu örnekte, `approve` veya `reject` koşullarından hiçbiri sağlanmazsa, `pending-review` geçişi varsayılan yedek olarak çalıştırılır.

> **Not:** Her state için sadece bir adet `triggerKind: 10` tanımlanmalıdır. Hiçbir koşul eşleşmezse ve varsayılan tanımlı değilse, otomatik geçiş gerçekleşmez.

### Scheduled (2)
Zamanlanmış transition'dır. Belirli bir zamanda veya cron benzeri periyodik olarak çalışması istendiğinde kullanılır. Sadece sistem tarafından çalıştırılır.

**Özellikler:**
- `Timer` alanında mapping ile tanımlanır
- `ITimerMapping` arayüzünü runtime'da compile edip execute eder
- Zaman tabanlı tetikleme

**Kullanım Alanları:**
- Periyodik raporlama
- Otomatik yedekleme işlemleri
- Zamanlanmış bildirimlerin gönderimi
- Timeout durumları

### Event (3)
Pub/Sub sistemleri tarafından çağrılan transition'dır. Olay tabanlı tetikleme sağlar.

**Kullanım Alanları:**
- Microservice'ler arası iletişim
- Harici sistem entegrasyonları
- Asenkron işlem tetiklemeleri
- Event-driven architecture implementasyonları

## Update Parent Data Transition (v0.0.31+)

SubFlow state'lerinde bulunan parent workflow instance'ının data değerlerini güncellemek için kullanılan özel bir transition türüdür. Bu transition, normal state değişimi yapmaz; sadece parent instance'ın data'sını günceller.

### Temel Özellikler

- **Target**: Her zaman `$self` - State değişikliği yapılmaz
- **Kullanım Alanı**: Sadece `subflow-state` tipindeki state'lerde kullanılabilir
- **Davranış**: SubFlow'a ilerlemez, sadece data güncellemesi yapar
- **Well-Known Key**: `update-parent-data`

### Konfigürasyon

Workflow tanımında `updateData` konfigürasyonu olarak tanımlanır:

```json
{
  "updateData": {
    "key": "update-parent",
    "target": "$self",
    "triggerType": 0,
    "versionStrategy": "None",
    "labels": [
      { "language": "en", "label": "Update Parent Data" },
      { "language": "tr", "label": "Parent Veri Güncelle" }
    ]
  }
}
```

### Parametreler

| Parametre | Açıklama |
|-----------|----------|
| `key` | Transition'ın benzersiz anahtarı (örn: `"update-parent"`) |
| `target` | Mutlaka `"$self"` olmalıdır - State değişikliği yapılmaz |
| `triggerType` | `0` (Manual) - Kullanıcı tarafından tetiklenir |
| `versionStrategy` | Data versiyonlama stratejisi |
| `labels` | Çoklu dil desteği için etiketler |


### Yapılan İşlemler

| İşlem | Yapılır mı? |
|-------|-------------|
| Data mapping (transition data mapping kurallarına göre) | Evet |
| Instance data güncelleme | Evet |
| Instance key validasyonu ve set etme | Evet |
| Transition record oluşturma | Evet |
| Tag ekleme (varsa) | Evet |

### Yapılmayan İşlemler

| İşlem | Yapılır mı? |
|-------|-------------|
| State değişikliği (target `$self` olduğu için) | Hayır |
| SubFlow'a ilerleme | Hayır |
| Auto transition çağırımı | Hayır |
| OnExit/OnEntry task'larının çalıştırılması | Hayır |
| State change event'lerinin yayınlanması | Hayır |

### Önemli Notlar

1. **State Tipi Kısıtlaması**: Sadece SubFlow state'lerinde kullanılabilir. Diğer state tiplerinde normal transition pipeline'ı çalışır.

2. **Target Kısıtlaması**: Target mutlaka `"$self"` olmalıdır. Farklı bir target belirtilirse, normal transition davranışı sergilenir.

3. **Instance Durumu**: Tamamlanmış (completed) instance'lar üzerinde çalıştırılamaz. Bu durumda hata döner.

---

## Shared Transitions
`AvailableIn` özelliği sayesinde, bir transition birden fazla state'de kullanılabilir hale getirilebilir. Bu durumda transition hangi state'lerde çalıştırılabileceği bu liste ile belirlenir.

**Subflow iken ana flow (v0.0.39+):** Instance **subflow içindeyken** artık **ana flow**'un shared transition'ı da çalıştırılabilir (önceden yalnızca subflow'un shared transition'ı kullanılabiliyordu). Shared transition, instance subflow'dayken kullanıma açıksa **target** tanımı **$self** olmak zorundadır; böylece subflow'dan tetiklendiğinde geçiş doğru (parent) bağlama uygulanır.

## Versiyon Yönetimi
`VersionStrategy` özelliği ile transition'ların data versiyonu Major ve Minor olarak değiştirilebilir. Bu, geriye uyumluluk ve veri migrasyonu açısından önemlidir.

## Payload Validasyonu
`Schema` referansı kullanılarak transition'da iletilen veriler validate edilebilir. Bu sayede veri bütünlüğü korunur ve hatalı veri girişleri önlenir.

**Asenkron path validasyonu (v0.0.50+):** `sync=false` modunda artık şema validasyonu, istek kabul edilmeden ve arka plan job'u kuyruğa alınmadan **önce** yapılır. Daha önce asenkron path'te validasyon atlanıyordu ve geçersiz payload'lar kabul edilip çalıştırma sırasında hata veriyordu. Geçersiz payload'lar artık senkron path ile aynı şekilde `400 Bad Request` ve alan düzeyinde validasyon hataları döndürür.

> **Referans:** [#556](https://github.com/burgan-tech/vnext/issues/556)

## Workflow Timeout (v0.0.50+)

Bir workflow'da `timeout` yapılandırması varsa ve instance zaman aşımına uğrarsa, sistem artık timeout hedef state'i için **tam TransitionPipeline**'ı çalıştırır. Daha önce `FlowTimeoutJobHandler`, instance state'ini doğrudan değiştirip tamamlanmış olarak işaretliyor ve tüm pipeline adımlarını atlıyordu.

**Timeout'ta artık çalıştırılan pipeline adımları:**

| Adım | Açıklama |
|------|----------|
| `RunOnExitTasksStep` | Mevcut state'in `onExit` task'larını çalıştırır |
| `ChangeStateStep` | Instance'ı hedef state'e geçirir |
| `RunOnEntryTasksStep` | Hedef state'in `onEntry` task'larını çalıştırır |
| `HandleFinishStep` | Hedef `Finish` state ise tamamlama işlemlerini yapar |
| `ScheduleTransitionsStep` | Zamanlanmış transition'ları kaydeder |
| `RunAutomaticTransitionsStep` | Otomatik transition'ları zincirler |

> **Referans:** [#514](https://github.com/burgan-tech/vnext/issues/514)

### Dinamik Timeout Mapping (v0.0.50+)

Workflow seviyesinde timeout, `ITimerMapping` scriptleri ile dinamik timeout süresi hesaplama için isteğe bağlı bir `mapping` alanını destekler. Mapping tanımlandığında, yedek (fallback) olarak statik `timer` yapılandırması da **zorunludur**.

**Çözümleme sırası:**
1. Mapping script'i runtime'da `ScriptContext` kullanılarak değerlendirilir (instance data, workflow, headers vb. erişilebilir)
2. Mapping başarılı olursa, döndürülen `TimerSchedule` (DateTime veya Duration) kullanılır
3. Mapping başarısız olursa (derleme veya çalışma zamanı hatası), statik `timer.duration` yedek olarak kullanılır

**Tanım örneği:**

```json
"timeout": {
  "key": "$timeout",
  "target": "timed-out",
  "versionStrategy": "None",
  "timer": { "reset": "false", "duration": "PT1H" },
  "mapping": {
    "location": "./src/TimeoutMapping.csx",
    "code": "<BASE64>"
  }
}
```

**Validasyon kuralları:**
- `mapping` tanımlandığında `timer` zorunludur. `timer` olmadan `mapping` tanımlamak şu validasyon hatasını üretir: `"When timeout mapping is defined, static timer configuration is also required as fallback."`
- `timer` olmadan `mapping` yayınlama zamanında reddedilir
- `mapping` olmadan `timer` daha önce olduğu gibi çalışmaya devam eder (sadece statik süre)

**Kullanım senaryosu:** Bir parent workflow, subprocess'i body'de `timeoutMinutes: 30` göndererek başlatır. Subprocess'in timeout mapping'i bu değeri `context.Instance.Data` üzerinden okur ve 30 dakikalık timeout zamanlar. Mapping başarısız olursa, statik 1 saatlik yedek (`PT1H`) uygulanır.

> **Referans:** [#524](https://github.com/burgan-tech/vnext/issues/524)