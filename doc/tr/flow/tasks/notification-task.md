# Notification Task

Notification Task, socket/hub bağlantıları üzerinden client'lara workflow durum bilgisini long-polling pattern kullanarak gönderen, sistem tarafından tanımlanmış bir task'tır. Bu task, bağlı client'lara workflow durum değişiklikleri hakkında gerçek zamanlı bildirimler göndermeyi sağlar.

## İçindekiler

1. [Genel Bakış](#genel-bakış)
2. [Task Tanımı](#task-tanımı)
3. [Nasıl Çalışır](#nasıl-çalışır)
4. [Konfigürasyon](#konfigürasyon)
5. [Dapr Binding Kurulumu](#dapr-binding-kurulumu)
6. [Kullanım Örnekleri](#kullanım-örnekleri)
7. [En İyi Uygulamalar](#en-iyi-uygulamalar)
8. [Sorun Giderme](#sorun-giderme)

## Genel Bakış

Notification Task, özelleşmiş bir task tipidir ve şu özelliklere sahiptir:
- Bağlı client'lara workflow durum bilgisi gönderir
- Socket/hub iletişim kanallarını kullanır
- Gerçek zamanlı güncellemeler için long-polling pattern uygular
- Sistem tarafından önceden tanımlanmıştır (task tanımı oluşturmaya gerek yoktur)
- İletişim için Dapr binding kullanır

### Temel Karakteristikler

| Özellik | Değer |
|---------|-------|
| **Task Tipi** | Notification (Tip: "G") |
| **Sistem Tanımlı** | Evet |
| **Task Key** | `notification-task` |
| **Domain** | `core` |
| **Flow** | `sys-tasks` |
| **Özel Tanım Gerektirir** | Hayır |
| **İletişim Yöntemi** | Dapr HTTP Binding |

## Task Tanımı

### Referans Şeması

Workflow'unuzda Notification Task kullanırken şu şekilde referans verin:

```json
{
  "order": 1,
  "task": {
    "key": "notification-task",
    "domain": "core",
    "version": "1.0.0",
    "flow": "sys-tasks"
  },
  "mapping": {
    "type": "G"
  }
}
```

### Şema Özellikleri

| Özellik | Gerekli | Açıklama |
|---------|---------|----------|
| `order` | Evet | State içindeki çalıştırma sırası |
| `task.key` | Evet | Her zaman `notification-task` (sistem tanımlı) |
| `task.domain` | Evet | Her zaman `core` |
| `task.version` | Evet | Task versiyonu (örn: `1.0.0`) |
| `task.flow` | Evet | Her zaman `sys-tasks` |
| `mapping.type` | Evet | Notification Task için her zaman `"G"` |

### Önemli Notlar

- **Sistem Tanımlı**: `notification-task` sistem tarafından önceden tanımlanmıştır. Geliştiricilerin ayrı bir task tanım dosyası oluşturmasına gerek yoktur.
- **Özel Mapping Yok**: Diğer task'lardan farklı olarak, notification task'ları belirli bir mapping tipi (`"G"`) kullanır ve özel mapping script'lerine ihtiyaç duymaz.
- **Otomatik Çalıştırma**: Task çalıştırıldığında otomatik olarak durum bilgisini gönderir.

## Nasıl Çalışır

### Çalıştırma Akışı

```
1. Workflow, NotificationTask içeren state'e ulaşır
   ↓
2. NotificationTask sırasıyla çalıştırılır
   ↓
3. Task, mevcut workflow durum bilgisini alır
   ↓
4. Durum verisi Dapr HTTP binding üzerinden gönderilir
   ↓
5. Dapr, yapılandırılmış notification hub/socket'e iletir
   ↓
6. Bağlı client'lar durum güncellemesini alır
```

### Client'lara Gönderilen Veri

Notification task, State Function API ile aynı bilgiyi gönderir:

```json
{
  "data": {
    "href": "/domain/workflows/workflow-key/instances/instance-id/functions/data"
  },
  "view": {
    "href": "/domain/workflows/workflow-key/instances/instance-id/functions/view",
    "loadData": true
  },
  "state": "current-state",
  "status": "A",
  "activeCorrelations": [...],
  "transitions": [
    {
      "href": "/domain/workflows/workflow-key/instances/instance-id/transitions/transition-key",
      "name": "transition-key"
    }
  ],
  "eTag": "etag-value"
}
```

## Konfigürasyon

### 1. Dapr Binding Component

Dapr HTTP binding component dosyası oluşturun: `notification-http-binding.yaml`

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: notification-http-binding
spec:
  type: bindings.http
  version: v1
  metadata:
  - name: url
    value: "http://your-notification-hub:port/api/notifications"
  - name: method
    value: "POST"
  - name: headers
    value: "Content-Type: application/json"
```

**Component Özellikleri:**

| Özellik | Açıklama | Örnek |
|---------|----------|-------|
| `metadata.name` | Binding component adı | `notification-http-binding` |
| `spec.type` | Binding tipi | `bindings.http` |
| `metadata.url` | Hedef notification hub URL'i | `http://notification-hub:8080/api/notify` |
| `metadata.method` | HTTP metodu | `POST` |
| `metadata.headers` | HTTP header'ları | `Content-Type: application/json` |

### 2. Execution API Konfigürasyonu

Execution API `appsettings.json` dosyanızda (veya `appsettings.Execution.Development.json` gibi ortama özel dosyada), binding adını yapılandırın:

```json
{
  "Dapr": {
    "NotificationBinding": {
      "Name": "notification-http-binding"
    }
  }
}
```

**Konfigürasyon Konumu:**
- Dosya: `appsettings.Execution.Development.json`
- Satırlar: 123-125 (yaklaşık)
- Özellik: `Dapr.NotificationBinding.Name`

**Örnek Tam Konfigürasyon:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "Dapr": {
    "NotificationBinding": {
      "Name": "notification-http-binding"
    },
    "SecretStore": "secret-store",
    "StateStore": "state-store"
  }
}
```

## Kullanım Örnekleri

### Örnek 1: Workflow'da Temel Bildirim

```json
{
  "key": "user-registration-workflow",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-flows",
  "attributes": {
    "type": "F",
    "states": [
      {
        "key": "email-verification",
        "stateType": 1,
        "onEntries": [
          {
            "order": 1,
            "task": {
              "key": "send-verification-email",
              "domain": "core",
              "version": "1.0.0",
              "flow": "sys-tasks"
            }
          },
          {
            "order": 2,
            "task": {
              "key": "notification-task",
              "domain": "core",
              "version": "1.0.0",
              "flow": "sys-tasks"
            },
            "mapping": {
              "type": "G"
            }
          }
        ]
      }
    ]
  }
}
```

Bu örnekte:
1. E-posta doğrulama e-postası gönderilir
2. Notification task, bağlı client'lara durum değişikliğini bildirir
3. Client'lar güncellemeyi alır ve uygun UI'ı gösterebilir

### Örnek 2: Çoklu State Bildirimleri

```json
{
  "key": "order-processing-workflow",
  "states": [
    {
      "key": "order-created",
      "onEntries": [
        {
          "order": 1,
          "task": {
            "key": "create-order-record",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          }
        },
        {
          "order": 2,
          "task": {
            "key": "notification-task",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          },
          "mapping": {
            "type": "G"
          }
        }
      ]
    },
    {
      "key": "payment-processing",
      "onEntries": [
        {
          "order": 1,
          "task": {
            "key": "process-payment",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          }
        },
        {
          "order": 2,
          "task": {
            "key": "notification-task",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          },
          "mapping": {
            "type": "G"
          }
        }
      ]
    },
    {
      "key": "order-completed",
      "onEntries": [
        {
          "order": 1,
          "task": {
            "key": "notification-task",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          },
          "mapping": {
            "type": "G"
          }
        }
      ]
    }
  ]
}
```

Her kritik state'de bildirim gönderilir:
- Sipariş oluşturulduğunda
- Ödeme işlenirken
- Sipariş tamamlandığında

### Örnek 3: Koşullu Bildirimler

Sadece gerektiğinde bildirim göndermek için state transition'larını stratejik olarak kullanın:

```json
{
  "key": "approval-workflow",
  "states": [
    {
      "key": "pending-approval",
      "onEntries": [
        {
          "order": 1,
          "task": {
            "key": "notification-task",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          },
          "mapping": {
            "type": "G"
          }
        }
      ]
    },
    {
      "key": "approved",
      "onEntries": [
        {
          "order": 1,
          "task": {
            "key": "update-approval-status",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          }
        },
        {
          "order": 2,
          "task": {
            "key": "notification-task",
            "domain": "core",
            "version": "1.0.0",
            "flow": "sys-tasks"
          },
          "mapping": {
            "type": "G"
          }
        }
      ]
    }
  ]
}
```

## En İyi Uygulamalar

### 1. Stratejik Yerleştirme

Client'ların anında güncellemeye ihtiyaç duyduğu state'lere notification task'ları yerleştirin:
- Kritik iş operasyonlarından sonra
- Kullanıcı etkileşimi gerektiren karar noktalarında
- Bekleme durumlarına girerken
- Workflow tamamlandığında

### 2. Sıra Yönetimi

Notification task'ları, veri değiştiren task'lardan sonra çalıştırın:

```json
{
  "onEntries": [
    {
      "order": 1,
      "task": { /* Veri değiştiren task */ }
    },
    {
      "order": 2,
      "task": { /* notification-task */ },
      "mapping": { "type": "G" }
    }
  ]
}
```

Bu, client'ların en güncel durum bilgisini almasını sağlar.

### 3. Notification Hub Tasarımı

Notification hub'ınızı ölçeklenebilir şekilde tasarlayın:
- WebSocket bağlantıları için SignalR veya benzeri kullanın
- Instance ID'ye göre bağlantı grupları uygulayın
- Client tarafında yeniden bağlanma mantığını handle edin
- Heartbeat/keep-alive mekanizması uygulayın

### 4. Güvenlik

Bildirim kanalınızı güvenli hale getirin:
- Hub bağlantısına izin vermeden önce client'ları doğrulayın
- Instance'a özel bildirimler için yetkilendirmeyi doğrulayın
- Şifreli bağlantılar kullanın (WSS/HTTPS)
- Dapr binding header'larına authentication token'ları ekleyin

### 5. Hata Yönetimi

Bildirim başarısızlıklarını düzgün şekilde handle edin:
- Notification task başarısızlıkları workflow ilerlemesini bloke etmemelidir
- Notification hub'ınızda retry mantığı uygulayın
- İzleme için bildirim başarısızlıklarını loglayın
- Gerçek zamanlı bildirimler başarısız olursa polling'e fallback sağlayın

### 6. Performans Değerlendirmeleri

Bildirim performansını optimize edin:
- Notification hub'da connection pooling kullanın
- Yüksek hacimli senaryolar için mesaj batching uygulayın
- Sık erişilen durum bilgilerini önbellekleyin
- Bildirim gecikmesini izleyin

### 7. Test

Bildirim fonksiyonalitesini test edin:
- Notification hub endpoint'lerini unit test edin
- Dapr binding ile integration test yapın
- Birden fazla eşzamanlı client ile load test yapın
- Yeniden bağlanma senaryolarını test edin

## İlgili Dokümantasyon

- [State Function API](../function.md#state-fonksiyonu) - State bilgi yapısı
- [HTTP Task](./http-task.md) - HTTP task konfigürasyonu
- [Dapr Bindings](https://docs.dapr.io/reference/components-reference/supported-bindings/) - Dapr binding dokümantasyonu
- [Task Türleri](../task.md) - Tüm task türlerine genel bakış
- [Workflow Tanımı](../flow.md) - Workflow yapısı ve konfigürasyonu

## Özet

Notification Task, bağlı client'lara gerçek zamanlı workflow durum güncellemeleri sağlayan güçlü, sistem tanımlı bir task'tır. Hatırlanması gereken temel noktalar:

- ✅ Sistem tanımlı, özel task tanımı gerekmez
- ✅ İletişim için Dapr HTTP binding kullanır
- ✅ `notification-http-binding.yaml` component gerektirir
- ✅ `appsettings.json`'da binding adını yapılandırın
- ✅ Her zaman mapping type `"G"` kullanın
- ✅ Workflow state'lerinde stratejik olarak yerleştirin
- ✅ Veri değiştiren task'lardan sonra çalıştırın

Bu rehberi takip ederek, workflow'larınızda gerçek zamanlı bildirimler uygulayabilir, kullanıcılara workflow durum değişiklikleri hakkında anında geri bildirim sağlayabilirsiniz.

