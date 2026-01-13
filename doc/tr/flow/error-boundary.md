# Error Boundary

Error Boundary, çok seviyeli hata çözümleme, öncelik tabanlı yürütme ve kurumsal düzeyde hata kurtarma yetenekleri sunan kapsamlı bir hata yönetim sistemidir.

## Genel Bakış

vNext workflow sistemi, üç seviyede hata politikaları tanımlamanıza olanak tanıyan hiyerarşik bir hata yönetimi mekanizması uygular:

1. **Task Seviyesi** - En spesifik, bireysel task yürütmelerine uygulanır
2. **State Seviyesi** - Task seviyesinde bir boundary hatayı işlemediğinde uygulanır
3. **Global Seviye** - Alt seviyede eşleşme olmadığında workflow geneli fallback

> **Önemli:** ErrorBoundary tanımları **task execution** seviyesinde çalışır. Boundary nerede tanımlanırsa tanımlansın (global, state veya task), aksiyonlar task yürütme hatalarına göre alınır.

---

## Hata Çözümleme Hiyerarşisi

Task yürütme sırasında bir hata oluştuğunda, sistem error boundary'leri aşağıdaki sırayla değerlendirir:

```
Task-level errorBoundary (en spesifik)
   ↓ (eşleşme yoksa, otomatik olarak sonraki seviyeyi kontrol et)
State-level errorBoundary
   ↓ (eşleşme yoksa, otomatik olarak sonraki seviyeyi kontrol et)
Global-level errorBoundary
   ↓ (hala eşleşme yoksa)
Default sistem davranışı (exception throw)
```

Her seviyede, kurallar **priority** değerine göre değerlendirilir (düşük sayı = yüksek öncelik).

---

## ErrorBoundary Yapısı

```json
{
  "errorBoundary": {
    "onError": [
      {
        "action": 0,
        "errorTypes": ["ValidationException"],
        "errorCodes": ["Task:400007"],
        "transition": "error-state",
        "priority": 10,
        "retryPolicy": {
          "maxRetries": 3,
          "initialDelay": "PT5S",
          "backoffType": 1,
          "backoffMultiplier": 2.0,
          "maxDelay": "PT1M",
          "useJitter": true
        },
        "logOnly": false
      }
    ]
  }
}
```

### Özellikler

| Özellik | Tip | Açıklama |
|---------|-----|----------|
| `onError` | array | Öncelik sırasına göre değerlendirilen hata yönetim kuralları |

> **Not:** `onTimeout` özelliği şemada mevcut ancak **henüz implemente edilmemiştir**.

---

## Error Handler Rule

`onError` dizisindeki her kural, belirli hataların nasıl yönetileceğini tanımlar.

### Özellikler

| Özellik | Tip | Zorunlu | Varsayılan | Açıklama |
|---------|-----|---------|------------|----------|
| `action` | int | Evet | - | Alınacak aksiyon (Hata Aksiyonları'na bakın) |
| `errorTypes` | string[] | Hayır | `["*"]` | Eşleştirilecek exception tip isimleri |
| `errorCodes` | string[] | Hayır | `["*"]` | Eşleştirilecek hata kodları |
| `transition` | string | Hayır | - | Tetiklenecek transition key'i |
| `priority` | int | Hayır | 100 | Kural önceliği (düşük = yüksek öncelik) |
| `retryPolicy` | object | Hayır | - | Yeniden deneme yapılandırması |
| `logOnly` | boolean | Hayır | false | Sadece logla, akışı etkileme |

### Hata Eşleştirme

- **errorTypes**: Exception sınıf isimleri (örn. `ValidationException`, `TimeoutException`)
- **errorCodes**: `Kategori:Kod` veya sadece `Kod` formatında hata kodları (örn. `Task:400007`, `500`)
- Boş dizi veya `["*"]` tüm hataları eşleştirir

### Öncelik Sistemi

- Düşük değerler önce değerlendirilir
- Varsayılan öncelik: `100`
- Wildcard kuralları için: `999`
- Önerilen aralıklar:
  - Kritik handler'lar: `1-10`
  - Spesifik handler'lar: `10-50`
  - Genel handler'lar: `50-100`
  - Fallback handler'lar: `100-999`

---

## Hata Aksiyonları

| Kod | Aksiyon | Açıklama |
|-----|---------|----------|
| `0` | **Abort** | Yürütmeyi durdur, isteğe bağlı olarak error transition tetikle |
| `1` | **Retry** | Yapılandırılmış retry policy ile task'ı yeniden dene |
| `2` | **Rollback** | Compensation state'e geri al |
| `3` | **Ignore** | Hatayı yoksay ve sonraki task'a devam et |
| `4` | **Notify** | Bildirim gönder ve isteğe bağlı olarak transition yap |
| `5` | **Log** | Sadece logla, akışı etkilemez |

---

## Retry Policy

`Retry` aksiyonu için yeniden deneme davranışını yapılandırın.

```json
{
  "retryPolicy": {
    "maxRetries": 3,
    "initialDelay": "PT5S",
    "backoffType": 1,
    "backoffMultiplier": 2.0,
    "maxDelay": "PT1M",
    "useJitter": true
  }
}
```

### Özellikler

| Özellik | Tip | Varsayılan | Açıklama |
|---------|-----|------------|----------|
| `maxRetries` | int | 3 | Maksimum yeniden deneme sayısı |
| `initialDelay` | string | - | İlk gecikme (ISO 8601 süre) |
| `backoffType` | int | 1 | 0: Sabit, 1: Üstel |
| `backoffMultiplier` | number | 2.0 | Üstel backoff için çarpan |
| `maxDelay` | string | - | Yeniden denemeler arası maksimum gecikme (ISO 8601 süre) |
| `useJitter` | boolean | true | Thundering herd'ü önlemek için rastgele jitter ekle |

### Backoff Tipleri

| Kod | Tip | Açıklama |
|-----|-----|----------|
| `0` | Sabit (Fixed) | Her yeniden denemede aynı gecikme |
| `1` | Üstel (Exponential) | Her yeniden denemede gecikme ikiye katlanır (veya çarpılır) |

### Süre Formatı (ISO 8601)

- `PT5S` - 5 saniye
- `PT30S` - 30 saniye
- `PT1M` - 1 dakika
- `PT5M` - 5 dakika
- `PT1H` - 1 saat

---

## Örnekler

### 1. Global Seviye ErrorBoundary

Workflow geneli hata yönetimi için workflow `attributes` seviyesinde tanımlayın:

```json
{
  "key": "payment-workflow",
  "domain": "banking",
  "version": "1.0.0",
  "attributes": {
    "type": "F",
    "errorBoundary": {
      "onError": [
        {
          "action": 0,
          "errorCodes": ["*"],
          "transition": "error-state",
          "priority": 999
        }
      ]
    },
    "states": [...]
  }
}
```

### 2. State Seviyesi ErrorBoundary

State'e özgü hata yönetimi için state seviyesinde tanımlayın:

```json
{
  "key": "processing",
  "stateType": 2,
  "versionStrategy": "Minor",
  "labels": [...],
  "errorBoundary": {
    "onError": [
      {
        "action": 1,
        "errorTypes": ["TransientException"],
        "priority": 10,
        "retryPolicy": {
          "maxRetries": 5,
          "initialDelay": "PT10S",
          "backoffType": 1,
          "backoffMultiplier": 2.0,
          "maxDelay": "PT5M",
          "useJitter": true
        }
      },
      {
        "action": 0,
        "errorCodes": ["*"],
        "transition": "failed",
        "priority": 100
      }
    ]
  },
  "transitions": [...]
}
```

### 3. Task Seviyesi ErrorBoundary

Task'a özgü hata yönetimi için task execution seviyesinde tanımlayın:

```json
{
  "onExecutionTasks": [
    {
      "order": 1,
      "task": {
        "key": "call-external-api",
        "domain": "core",
        "version": "1.0.0",
        "flow": "sys-tasks"
      },
      "mapping": {
        "ref": "Mappings/api-call-mapping.json"
      },
      "errorBoundary": {
        "onError": [
          {
            "action": 1,
            "errorCodes": ["Task:503", "Task:504"],
            "priority": 1,
            "retryPolicy": {
              "maxRetries": 3,
              "initialDelay": "PT5S",
              "backoffType": 1,
              "backoffMultiplier": 2.0
            }
          },
          {
            "action": 3,
            "errorCodes": ["Task:404"],
            "priority": 2,
            "logOnly": true
          }
        ]
      }
    }
  ]
}
```

### 4. Öncelikli Çoklu Kurallar

```json
{
  "errorBoundary": {
    "onError": [
      {
        "_comment": "Validation hatalarını yönet - hemen durdur",
        "action": 0,
        "errorTypes": ["ValidationException"],
        "transition": "validation-failed",
        "priority": 1
      },
      {
        "_comment": "Geçici hataları yeniden dene",
        "action": 1,
        "errorCodes": ["Task:503", "Task:504", "Task:429"],
        "priority": 10,
        "retryPolicy": {
          "maxRetries": 5,
          "initialDelay": "PT5S",
          "backoffType": 1
        }
      },
      {
        "_comment": "Kritik olmayan hataları logla ve yoksay",
        "action": 5,
        "errorCodes": ["Task:204"],
        "priority": 20,
        "logOnly": true
      },
      {
        "_comment": "Fallback - error transition ile durdur",
        "action": 0,
        "errorCodes": ["*"],
        "transition": "error-state",
        "priority": 999
      }
    ]
  }
}
```

### 5. Jitter ile Üstel Backoff

```json
{
  "errorBoundary": {
    "onError": [
      {
        "action": 1,
        "errorTypes": ["*"],
        "priority": 100,
        "retryPolicy": {
          "maxRetries": 5,
          "initialDelay": "PT1S",
          "backoffType": 1,
          "backoffMultiplier": 2.0,
          "maxDelay": "PT30S",
          "useJitter": true
        }
      }
    ]
  }
}
```

Jitter ile yeniden deneme gecikmeleri (yaklaşık):
1. ~1s (+ rastgele 0-500ms)
2. ~2s (+ rastgele 0-1000ms)
3. ~4s (+ rastgele 0-2000ms)
4. ~8s (+ rastgele 0-4000ms)
5. ~16s (+ rastgele 0-8000ms, 30s'de sınırlanır)

---

## En İyi Uygulamalar

### 1. Önceliği Akıllıca Kullanın

```json
{
  "onError": [
    { "action": 0, "errorTypes": ["ValidationException"], "priority": 1 },
    { "action": 1, "errorCodes": ["Task:503"], "priority": 10 },
    { "action": 0, "errorCodes": ["*"], "priority": 999 }
  ]
}
```

### 2. Her Zaman Fallback Ekleyin

Yüksek öncelik numaralı wildcard kuralını fallback olarak ekleyin:

```json
{
  "action": 0,
  "errorCodes": ["*"],
  "transition": "error-state",
  "priority": 999
}
```

### 3. Uygun Retry Policy Kullanın

- **Geçici hatalar** (503, 504, 429): Üstel backoff ile yeniden dene
- **Validation hataları**: Hemen durdur, yeniden deneme yok
- **İş hataları**: Hata yönetimi state'ine yönlendir

### 4. Hiyerarşiden Yararlanın

- **Task seviyesi**: Harici API çağrıları için spesifik retry policy'ler
- **State seviyesi**: State operasyonları için ortak hata yönetimi
- **Global seviye**: İşlenmeyen hatalar için fallback ve bildirim

### 5. Debugging için logOnly Kullanın

```json
{
  "action": 5,
  "errorCodes": ["*"],
  "priority": 1,
  "logOnly": true
}
```

---

## İlgili Dökümanlar

- [Workflow Tanımı](./flow.md) - Workflow yapısı ve bileşenleri
- [State Yönetimi](./state.md) - State tanımları ve tipleri
- [Task Yönetimi](./task.md) - Task tipleri ve yürütme
- [Transition Yönetimi](./transition.md) - Transition'lar ve tetikleyiciler
