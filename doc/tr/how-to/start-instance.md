# Instance Başlatma ve Bileşen Yönetimi

Bu dokümantasyon, vNext Runtime sisteminde bir instance'ın nasıl başlatılacağını ve sistem bileşenlerinin nasıl register edilerek aktif edileceğini açıklamaktadır.

## Instance Yaşam Döngüsü

### 1. Instance Başlatma

Bir iş akışı başlatmak için her zaman `start` endpoint'i kullanılır. Sistem size bir `id` değerini response olarak döner. Bu id değeri, oluşturulan instance id'dir ve bundan sonraki transition süreci bu id ile ilerletilir.

**Endpoint:**
```
POST /:domain/workflows/:flow/instances/start?sync=true
```

**Örnek İstek:**
```http
POST /ecommerce/workflows/scheduled-payments/instances/start?sync=true
Content-Type: application/json

{
    "key": "99999999999",
    "tags": [
        "test",
        "oauth",
        "authentication"
    ],
    "attributes": {
        "userId": 454,
        "amount": 12000,
        "currency": "TL",
        "frequency": "monthly",
        "startDate": "2025-10-01T09:02:38.201Z",
        "endDate": "2026-10-01T09:02:38.201Z",
        "paymentMethodId": "1",
        "description": "Tayfun ödeme",
        "recipientId": "324324",
        "isAutoRetry": false,
        "maxRetries": 3
    }
}
```

**Örnek Response:**
```json
{
  "id": "18075ad5-e5b2-4437-b884-21d733339113",
  "status": "A"
}
```

### 2. Instance Transition

Başlatılan instance'ı ilerletmek için transition endpoint'i kullanılır.

**Endpoint:**
```
PATCH /:domain/workflows/:flow/instances/:instanceId/transitions/activate?sync=true
```

**Örnek İstek:**
```http
PATCH /ecommerce/workflows/scheduled-payments/instances/inst_abc123def456/transitions/activate?sync=true
Content-Type: application/json

{
    "approvedBy": "admin",
    "approvalDate": "2025-09-20T10:30:00Z"
}
```

## Sistem Bileşenleri ve Register İşlemi

### Bileşen Türleri

Sistemde aşağıdaki bileşen türleri bulunmaktadır:

- **sys-flows**: İş akışı tanımları
- **sys-tasks**: Görev tanımları
- **sys-views**: Görünüm tanımları
- **sys-schemas**: Şema tanımları
- **sys-extensions**: Uzantı tanımları
- **sys-functions**: Fonksiyon tanımları

### Bileşen Register İşlemi

Her bileşen için sistemde varsayılan iş akışları sunulur ve tüm bileşenler bu özel iş akışları üzerinden register edilerek aktif edilir ve versiyonu yönetilir.

#### sys-flows Bileşeni Yaşam Döngüsü

Aşağıdaki Mermaid diyagramı, sys-flows bileşeninin state ve transition'larını göstermektedir:

![Sys-Flows](https://kroki.io/mermaid/svg/eNqNkbFqwzAQhvc-xc0FLx07FAKBdCykSykdrtYlOSzLQZY99GU8dvaeLfZ79SQ5jp2kEA3G3P3677tfpUNHS8atxTypnx5AzufjFyTJCyiLGwfPkFoSUWiFT6x7BaaOaxJJ-DmJzv2TgyJNU4fhnpfssSyjh6KZy0Qzjqn2akZyujwT2bnPVBM5lAe26U6qk6WG1oVVWtRkzypTOALL252DYhPXC3V_3rHUmMGyslVejdUEPsgwFLrqGyctjQa-WVPfkNGDddS9kjkefgAzxxsgxTqnnPsmKMioMPs_kkg8mi2CxxXJmktHOTgU7q41SlCySgtR12r0UGYiXh0PJiUtRZIW27s4hrBHmzcsb4Gs6PjLqU9FUDLJvrYEqmv7pmsvklNkYiQ-MsdWKO_HGR51tFzLTeNDjUiTUQvbN1zLsqF7-4FWZBlQS3J4jfAHp34Cig)

#### Bileşen Register Örneği

**1. Yeni bir workflow oluşturma (draft state):**
```http
POST /core/workflows/sys-flows/instances/start?sync=true
Content-Type: application/json

{
  "key": "my-custom-workflow",
  "version": "1.0.0",
  "domain": "ecommerce",
  "flow": "sys-flows",
  "flowVersion": "1.0.0",
  "attributes": {
    "type": "F",
    "labels": [
      {
        "language": "tr-TR",
        "label": "Ödeme İşlemi"
      }
    ],
    "startTransition": {
      "key": "start-payment",
      "target": "pending",
      "triggerType": 0
    },
    "states": [
      {
        "key": "pending",
        "stateType": 1,
        "transitions": []
      }
    ]
  }
}
```

**2. Workflow'u aktifleştirme:**
```http
PATCH /core/workflows/sys-flows/instances/{instanceId}/transitions/activate?sync=true
Content-Type: application/json

{
}
```

**3. Aktif workflow'u güncelleme:**
```http
PATCH /core/workflows/sys-flows/instances/{instanceId}/transitions/update?sync=true
Content-Type: application/json

{
  "type": "F",
    "labels": [
      {
        "language": "tr-TR",
        "label": "Ödeme İşlemi"
      }
    ],
    "startTransition": {
      "key": "start-payment",
      "target": "pending",
      "triggerType": 0
    },
    "states": [
      {
        "key": "pending",
        "stateType": 1,
        "transitions": []
      }
    ]
}
```

### Diğer Bileşenler

Diğer sistem bileşenleri (sys-tasks, sys-views, sys-schemas, sys-extensions, sys-functions) de aynı yaşam döngüsü mantığına sahiptir:

- **draft** → **active** → **passive** → **deleted**
- Her geçiş için uygun transition endpoint'leri kullanılır
- Bileşen türüne göre farklı şema validasyonları uygulanır

## Best Practices

### Instance Yönetimi
1. **Sync Parameter**: Sync işlemler için `sync=true` parametresini kullanın
2. **ETag Kullanımı**: Concurrent update'leri önlemek için ETag pattern'ini kullanın
3. **Error Handling**: HTTP status kodlarını kontrol edin ve uygun error handling yapın

### Bileşen Yönetimi
1. **Versiyon Stratejisi**: Semantic versioning kullanın (Major.Minor.Patch)
2. **Test Ortamı**: Önce draft state'te test edin, sonra aktifleştirin
3. **Rollback Planı**: Passive state'i rollback için kullanın
4. **Monitoring**: Bileşen durumlarını düzenli olarak kontrol edin

## Hata Durumları

### Yaygın Hatalar
- **404 Not Found**: Instance veya workflow bulunamadı
- **409 Conflict**: Concurrent update çakışması (ETag mismatch)
- **400 Bad Request**: Geçersiz transition veya veri formatı
- **422 Unprocessable Entity**: Şema validasyon hatası

### Hata Çözümleri
1. Instance ID'yi kontrol edin
2. ETag değerini güncelleyin
3. Request payload'ını şema ile karşılaştırın
4. Transition kurallarını kontrol edin