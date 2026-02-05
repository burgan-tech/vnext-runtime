# Instance Başlatma ve Bileşen Yönetimi

Bu dokümantasyon, vNext Runtime sisteminde bir instance'ın nasıl başlatılacağını ve sistem bileşenlerinin nasıl register edilerek aktif edileceğini açıklamaktadır.

## Instance Yaşam Döngüsü

### 1. Instance Başlatma

Bir iş akışı başlatmak için her zaman `start` endpoint'i kullanılır. Sistem size bir `id` değerini response olarak döner. Bu id değeri, oluşturulan instance id'dir ve bundan sonraki transition süreci bu id ile ilerletilir.

> **v0.0.23 Değişikliği**: `key` alanı artık zorunlu değildir. Başlangıçta boş bırakılabilir ve sonraki transition'larda atanabilir.

**Endpoint:**
```
POST /:domain/workflows/:flow/instances/start?sync=true
```

**Payload Şeması:**
```json
{
    "key": "",
    "tags": [],
    "attributes": {}
}
```

> **Not**: Tüm alanlar opsiyoneldir.

**Key Davranışı:**
- `key` değeri dolu gelirse ve mevcut instance'da key boş ise kaydedilir
- Key değeri başlangıçta verilmezse, sonraki transition'larda atanabilir

> **v0.0.29 Değişikliği - Idempotent Başlatma:** Aynı key ile bir workflow başlatıldığında, sistem artık hata yerine mevcut instance bilgilerini döner. Bu, güvenli yeniden deneme senaryolarını mümkün kılar.

**Idempotent Davranış:**
- **Eski davranış (v0.0.29 öncesi):** Key zaten varsa `409 Conflict` hatası döner
- **Yeni davranış (v0.0.29+):** Mevcut instance'ın güncel durumu ve ID bilgisi döner

Bu değişiklik şunları sağlar:
- Ağ hataları için güvenli yeniden deneme senaryoları
- İstemcilerin tekrarlanan çağrılarda orijinal başlatma yanıtını alabilmesi
- Başlatmadan önce ayrı "var mı kontrol et" çağrılarına gerek kalmaması

**Örnek İstek (Key ile):**
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

**Örnek İstek (Key Olmadan):**
```http
POST /ecommerce/workflows/scheduled-payments/instances/start?sync=true
Content-Type: application/json

{
    "tags": ["priority", "express"],
    "attributes": {
        "userId": 454,
        "amount": 12000
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

**Örnek Response (Idempotent - Key Zaten Varsa):**

Zaten aktif bir instance'ı olan bir key ile start endpoint'ini çağırdığınızda, mevcut instance bilgilerini alırsınız:

```http
POST /ecommerce/workflows/scheduled-payments/instances/start?sync=true
Content-Type: application/json

{
    "key": "99999999999"
}
```

```json
{
  "id": "18075ad5-e5b2-4437-b884-21d733339113",
  "status": "A"
}
```

> **Not:** Yanıt, mevcut instance'ın ID ve güncel durumunu döner. Yeni instance oluşturulmaz ve hata döndürülmez.

### 2. Instance Transition

Başlatılan instance'ı ilerletmek için transition endpoint'i kullanılır. Instance'a referans vermek için instance ID (UUID) veya instance Key kullanabilirsiniz.

> **v0.0.23 Değişikliği**: Transition payload şeması güncellenmiştir. Artık `key`, `tags` ve `attributes` alanları ile yapılandırılmış format kullanılmaktadır.

**Endpoint:**
```
PATCH /:domain/workflows/:flow/instances/:instanceIdOrKey/transitions/:transition?sync=true
```

> **Not**: `:instanceIdOrKey` parametresi şunları kabul eder:
> - **Instance ID**: Instance oluşturulduğunda dönen UUID (örn. `18075ad5-e5b2-4437-b884-21d733339113`)
> - **Instance Key**: Instance oluşturulurken sağlanan key değeri (örn. `99999999999`)

**Yeni Transition Payload Şeması:**
```json
{
    "key": "",
    "tags": [],
    "attributes": {}
}
```

> **Not**: Tüm alanlar opsiyoneldir.

**Key Atama Davranışı:**
- Transition sırasında `key` değeri gönderilirse ve mevcut instance'da key boş ise kaydedilir
- Bu sayede başlangıçta key olmadan oluşturulan instance'lara sonradan key atanabilir

**Örnek İstek (Instance ID kullanarak):**
```http
PATCH /ecommerce/workflows/scheduled-payments/instances/18075ad5-e5b2-4437-b884-21d733339113/transitions/activate?sync=true
Content-Type: application/json

{
    "attributes": {
        "approvedBy": "admin",
        "approvalDate": "2025-09-20T10:30:00Z"
    }
}
```

**Örnek İstek (Instance Key kullanarak):**
```http
PATCH /ecommerce/workflows/scheduled-payments/instances/99999999999/transitions/activate?sync=true
Content-Type: application/json

{
    "attributes": {
        "approvedBy": "admin",
        "approvalDate": "2025-09-20T10:30:00Z"
    }
}
```

**Örnek İstek (Transition'da Key Atama):**
```http
PATCH /ecommerce/workflows/scheduled-payments/instances/18075ad5-e5b2-4437-b884-21d733339113/transitions/assign-key?sync=true
Content-Type: application/json

{
    "key": "ORDER-2024-001",
    "tags": ["assigned"],
    "attributes": {
        "assignedBy": "system"
    }
}
```

> **Instance Key Kullanmanın Avantajları:**
> - Daha okunabilir ve anlamlı tanımlayıcılar (örn. sipariş numaraları, müşteri ID'leri)
> - İş anahtarları kullanan harici sistemlerle daha kolay entegrasyon
> - UUID'leri ayrıca saklamaya ve yönetmeye gerek yok

### 3. Instance Durumunu Sorgulama

Instance'ın mevcut durumunu ve verisini sorgulamak için GET endpoint'i kullanılır. Bu endpoint ETag pattern'i ile çalışır. Instance ID veya instance Key kullanabilirsiniz.

**Endpoint:**
```
GET /:domain/workflows/:flow/instances/:instanceIdOrKey
```

**Örnek İstek (Instance ID kullanarak):**
```http
GET /ecommerce/workflows/scheduled-payments/instances/18075ad5-e5b2-4437-b884-21d733339113
If-None-Match: "18075ad5-e5b2-4437-b884-21d733339113"
```

**Örnek İstek (Instance Key kullanarak):**
```http
GET /ecommerce/workflows/scheduled-payments/instances/99999999999
If-None-Match: "18075ad5-e5b2-4437-b884-21d733339113"
```

**Örnek Response:**
```json
{
  "id": "18075ad5-e5b2-4437-b884-21d733339113",
  "key": "99999999999",
  "flow": "scheduled-payments",
  "domain": "core",
  "flowVersion": "1.0.1",
  "eTag": "18075ad5-e5b2-4437-b884-21d733339113",
  "tags": [],
  "attributes": {},
  "extensions": {},
  "sortValue": ""
}
```

### 5. Instance'ları Filtreleme

Çeşitli kriterlere göre instance'ları sorgulamak için filtreleme özelliğini kullanın.

**Endpoint:**
```http
GET /{domain}/workflows/{workflow}/functions/data?filter={...}
```

**Örnek - Status'e Göre Filtrele:**
```http
GET /ecommerce/workflows/scheduled-payments/functions/data?filter={"status":{"eq":"Active"}}
```

**Örnek - JSON Veri Alanına Göre Filtrele:**
```http
GET /ecommerce/workflows/scheduled-payments/functions/data?filter={"attributes":{"amount":{"gt":"1000"}}}
```

**Örnek - Mantıksal OR ile Kombine Filtre:**
```http
GET /ecommerce/workflows/scheduled-payments/functions/data?filter={"or":[{"status":{"eq":"Active"}},{"status":{"eq":"Busy"}}]}
```

> **Detaylı Dokümantasyon:** Tüm filtreleme sözdizimi, operatörler ve aggregation'lar için bkz. [Instance Filtreleme Kılavuzu](../flow/instance-filtering.md).

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