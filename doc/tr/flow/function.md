# Function API'leri

Function API'leri, workflow instance'ları için sistem seviyesi operasyonlar sağlar. Bu yerleşik fonksiyonlar, client'ların workflow engine dahili yapılarına doğrudan erişmeden instance durumunu sorgulamasına, veri almasına ve view bilgisi elde etmesine olanak tanır.

## İçindekiler

1. [Genel Bakış](#genel-bakış)
2. [State Fonksiyonu](#state-fonksiyonu)
3. [Data Fonksiyonu](#data-fonksiyonu)
4. [View Fonksiyonu](#view-fonksiyonu)
5. [Kullanım Örnekleri](#kullanım-örnekleri)
6. [En İyi Uygulamalar](#en-iyi-uygulamalar)

## Genel Bakış

vNext Runtime platformu, her workflow instance'ı için otomatik olarak kullanılabilir olan üç temel function API'si sağlar:

| Fonksiyon | Amaç | Endpoint Deseni |
|-----------|------|-----------------|
| **State** | Instance durumu için long-polling | `GET /{domain}/workflows/{workflow}/instances/{instance}/functions/state` |
| **Data** | Instance verisini alma | `GET /{domain}/workflows/{workflow}/instances/{instance}/functions/data` |
| **View** | View içeriğini alma | `GET /{domain}/workflows/{workflow}/instances/{instance}/functions/view` |

Bu fonksiyonlar şunları sağlar:
- Gerçek zamanlı durum izleme (long-polling)
- ETag desteği ile verimli veri alma
- Platforma özel içerik ile dinamik view render etme

## State Fonksiyonu

State fonksiyonu, bir workflow instance'ı hakkında gerçek zamanlı durum bilgisi sağlar. Client'ların instance durum değişikliklerini izlemesi gereken long-polling senaryoları için tasarlanmıştır.

### Endpoint

```http
GET /{domain}/workflows/{workflow}/instances/{instance}/functions/state
```

### Parametreler

| Parametre | Konum | Tip | Gerekli | Açıklama |
|-----------|-------|-----|---------|----------|
| `domain` | Path | string | Evet | Domain adı |
| `workflow` | Path | string | Evet | Workflow key |
| `instance` | Path | string | Evet | Instance ID |

### Response

```json
{
  "data": {
    "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/data"
  },
  "view": {
    "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/view",
    "loadData": true
  },
  "state": "active",
  "status": "A",
  "activeCorrelations": [
    {
      "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/data",
      "correlationId": "corr-123",
      "parentState": "parent-state",
      "subFlowInstanceId": "sub-instance-456",
      "subFlowType": "SubFlow",
      "subFlowDomain": "core",
      "subFlowName": "approval-subflow",
      "subFlowVersion": "1.0.0",
      "isCompleted": false,
      "status": "Running",
      "currentState": "pending-approval"
    }
  ],
  "transitions": [
    {
      "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/transitions/approve",
      "name": "approve"
    },
    {
      "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/transitions/reject",
      "name": "reject"
    }
  ],
  "eTag": "W/\"abc123def456\""
}
```

### Response Alanları

| Alan | Tip | Açıklama |
|------|-----|----------|
| `data` | `object` | Instance verisini almak için link |
| `data.href` | `string` | Data fonksiyon endpoint URL'i |
| `view` | `object` | Mevcut state için view bilgisi |
| `view.href` | `string` | View fonksiyon endpoint URL'i |
| `view.loadData` | `boolean` | View'ın instance data'ya ihtiyaç duyup duymadığı |
| `state` | `string` | Instance'ın mevcut durumu |
| `status` | `string` | Instance durum kodu (A=Active, C=Completed, vb.) |
| `activeCorrelations` | `array` | Aktif sub-flow'lar ve correlation'lar |
| `transitions` | `array` | Mevcut durumdan kullanılabilir transition'lar |
| `eTag` | `string` | Cache doğrulama için ETag |

### Aktif Correlation'lar

Bir workflow aktif sub-flow'lara veya correlation'lara sahip olduğunda, bunlar response'a dahil edilir:

| Alan | Açıklama |
|------|----------|
| `correlationId` | Benzersiz correlation tanımlayıcısı |
| `parentState` | Parent workflow durumu |
| `subFlowInstanceId` | Sub-flow instance ID |
| `subFlowType` | Sub-flow tipi (SubFlow, SubProcess) |
| `subFlowDomain` | Sub-flow'un domain'i |
| `subFlowName` | Sub-flow workflow'un adı |
| `subFlowVersion` | Sub-flow'un versiyonu |
| `isCompleted` | Sub-flow'un tamamlanıp tamamlanmadığı |
| `status` | Sub-flow'un mevcut durumu |
| `currentState` | Sub-flow'un mevcut state'i |

### Kullanım Alanları

1. **Long-Polling**: Client'lar durum değişikliklerini tespit etmek için bu endpoint'i poll edebilir
2. **Durum İzleme**: Workflow ilerlemesini izleyen dashboard uygulamaları
3. **Transition Keşfi**: Kullanılabilir kullanıcı eylemlerini dinamik olarak keşfetme
4. **Sub-Flow Takibi**: Paralel sub-workflow'ların ilerlemesini izleme

### Örnek İstek

```http
GET /core/workflows/oauth-authentication-workflow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/state HTTP/1.1
Host: api.example.com
Accept: application/json
```

## Data Fonksiyonu

Data fonksiyonu, bir workflow instance'ının mevcut verisini alır. Verimli veri senkronizasyonu için ETag tabanlı önbellekleme destekler.

### Endpoint

```http
GET /{domain}/workflows/{workflow}/instances/{instance}/functions/data
```

### Parametreler

| Parametre | Konum | Tip | Gerekli | Açıklama |
|-----------|-------|-----|---------|----------|
| `domain` | Path | string | Evet | Domain adı |
| `workflow` | Path | string | Evet | Workflow key |
| `instance` | Path | string | Evet | Instance ID |

### Header'lar

| Header | Tip | Gerekli | Açıklama |
|--------|-----|---------|----------|
| `If-None-Match` | string | Hayır | Koşullu istek için ETag değeri |

### Response (200 OK)

```json
{
  "data": {
    "userId": "user123",
    "amount": 1000,
    "currency": "TRY",
    "authentication": {
      "success": true,
      "method": "otp",
      "timestamp": "2025-11-11T10:30:00Z"
    },
    "approval": {
      "status": "pending",
      "requestedAt": "2025-11-11T10:35:00Z"
    }
  },
  "eTag": "W/\"xyz789abc123\"",
  "extensions": {
    "userProfile": {
      "name": "Ahmet Yılmaz",
      "email": "ahmet.yilmaz@example.com"
    },
    "accountLimits": {
      "dailyLimit": 5000,
      "remainingLimit": 4000
    }
  }
}
```

### Response (304 Not Modified)

Eğer `If-None-Match` header'ı mevcut ETag ile eşleşirse, sunucu şunu döndürür:

```http
HTTP/1.1 304 Not Modified
ETag: W/"xyz789abc123"
```

Body döndürülmez, bu da bant genişliği ve işlem süresinden tasarruf sağlar.

### Response Alanları

| Alan | Tip | Açıklama |
|------|-----|----------|
| `data` | `object` | Mevcut instance verisi (camelCase özellikler) |
| `eTag` | `string` | Cache doğrulama için ETag |
| `extensions` | `object` | Kayıtlı extension'lardan ek veriler |

### ETag Desteği

Data fonksiyonu, verimli önbellekleme için ETag pattern'i uygular:

**İlk İstek:**
```http
GET /core/workflows/payment-flow/instances/123/functions/data
```

**Response:**
```http
HTTP/1.1 200 OK
ETag: "W/\"v1\""
Content-Type: application/json

{
  "data": { ... },
  "eTag": "W/\"v1\""
}
```

**Sonraki İstek:**
```http
GET /core/workflows/payment-flow/instances/123/functions/data
If-None-Match: "W/\"v1\""
```

**Response (Veri Değişmedi):**
```http
HTTP/1.1 304 Not Modified
ETag: "W/\"v1\""
```

**Response (Veri Değişti):**
```http
HTTP/1.1 200 OK
ETag: "W/\"v2\""
Content-Type: application/json

{
  "data": { ...güncellenmiş veri... },
  "eTag": "W/\"v2\""
}
```

### Extension'lar

Extension'lar, instance verisini zenginleştiren ek bağlam verisi sağlar:

- Extension'lar view referans konfigürasyonunda tanımlanır
- Her extension harici kaynaklardan veri alır
- Extension verisi `extensions` objesine merge edilir
- Extension'lar şunlar için kullanışlıdır:
  - Kullanıcı profil bilgisi
  - Referans veri lookup'ları
  - Gerçek zamanlı hesaplanan değerler
  - Harici sistem verileri

### Instance Verilerini Filtreleme

Data fonksiyonu, attribute'lara göre instance verilerini sorgulamak için güçlü filtreleme yetenekleri destekler. Bu, kriterlerinize uyan belirli instance'ları filtrelemenize ve almanıza olanak tanır.

#### Filtre Söz Dizimi

Filtreler şu formatı kullanır: `filter=attributes={alan}={operatör}:{değer}`

#### Kullanılabilir Operatörler

| Operatör | Açıklama | Örnek |
|----------|----------|-------|
| `eq` | Eşittir | `filter=attributes=clientId=eq:122` |
| `ne` | Eşit değildir | `filter=attributes=status=ne:inactive` |
| `gt` | Büyüktür | `filter=attributes=amount=gt:100` |
| `ge` | Büyük veya eşittir | `filter=attributes=score=ge:80` |
| `lt` | Küçüktür | `filter=attributes=count=lt:10` |
| `le` | Küçük veya eşittir | `filter=attributes=age=le:65` |
| `between` | İki değer arasında | `filter=attributes=amount=between:50,200` |
| `like` | Alt dize içerir | `filter=attributes=name=like:john` |
| `startswith` | İle başlar | `filter=attributes=email=startswith:test` |
| `endswith` | İle biter | `filter=attributes=email=endswith:.com` |
| `in` | Liste içinde değer | `filter=attributes=status=in:active,pending` |
| `nin` | Liste dışında değer | `filter=attributes=type=nin:test,debug` |

#### Filtre Örnekleri

**Tek Filtre:**
```http
GET /core/workflows/payment-flow/instances/abc-123/functions/data?filter=attributes=amount=gt:1000 HTTP/1.1
Host: api.example.com
Accept: application/json
```

**Çoklu Filtre (VE mantığı):**
```http
GET /core/workflows/order-processing/instances/123/functions/data?filter=attributes=status=eq:active&filter=attributes=priority=eq:high HTTP/1.1
Host: api.example.com
Accept: application/json
```

**Aralık Filtresi:**
```http
GET /core/workflows/payment-flow/instances/abc-123/functions/data?filter=attributes=amount=between:100,500 HTTP/1.1
Host: api.example.com
Accept: application/json
```

**Dize İşlemleri:**
```http
GET /core/workflows/customer/instances/123/functions/data?filter=attributes=email=endswith:@company.com HTTP/1.1
Host: api.example.com
Accept: application/json
```

> **Not:** Filtreleme, instance attribute'ları üzerinde çalışır ve büyük veri kümeleri için pagination ile birleştirildiğinde özellikle kullanışlıdır. Instance filtreleme hakkında daha fazla bilgi için [Instance Filtreleme](../../../README.md#instance-filtering) bölümüne bakın.

### Kullanım Alanları

1. **Veri Senkronizasyonu**: Client-side veriyi sunucu ile senkronize tutma
2. **Verimli Polling**: Gereksiz veri transferlerinden kaçınmak için ETag kullanma
3. **View Veri Binding**: View'ları mevcut instance verisi ile doldurma
4. **Audit ve Loglama**: Audit için tam instance durumunu alma
5. **Filtrelenmiş Veri Alma**: Attribute değerlerine göre belirli instance'ları sorgulama

### Örnek İstek

```http
GET /core/workflows/payment-flow/instances/abc-123/functions/data HTTP/1.1
Host: api.example.com
Accept: application/json
If-None-Match: "W/\"previous-etag\""
```

## View Fonksiyonu

View fonksiyonu, mevcut workflow durumu için uygun view tanımını alır. Platforma özel içerik ve transition'a özel view'ları destekler.

### Endpoint

```http
GET /{domain}/workflows/{workflow}/instances/{instance}/functions/view
```

### Parametreler

| Parametre | Konum | Tip | Gerekli | Açıklama |
|-----------|-------|-----|---------|----------|
| `domain` | Path | string | Evet | Domain adı |
| `workflow` | Path | string | Evet | Workflow key |
| `instance` | Path | string | Evet | Instance ID |
| `transitionKey` | Query | string | Hayır | View alınacak belirli transition |
| `platform` | Query | string | Hayır | Hedef platform (mobile, web, tablet, vb.) |

### Response

```json
{
  "key": "account-type-selection-view",
  "content": "{\"type\":\"form\",\"fields\":[...]}",
  "type": "json",
  "display": "full-page",
  "label": "Hesap Tipi Seç"
}
```

### Response Alanları

| Alan | Tip | Açıklama |
|------|-----|----------|
| `key` | `string` | View key tanımlayıcısı |
| `content` | `string` | View içeriği (format tipe bağlı) |
| `type` | `string` | İçerik tipi (json, html, vb.) |
| `display` | `string` | Gösterim modu (full-page, popup, vb.) |
| `label` | `string` | View için lokalize edilmiş etiket |

### Query Parametreleri

#### transitionKey

Hangi transition'ın view'ının alınacağını belirtir:

- **Sağlandı**: O belirli transition için tanımlanan view'ı döndürür (varsa)
- **Sağlanmadı**: State view'ını döndürür

**Örnek:**
```http
GET /core/workflows/account-opening/instances/123/functions/view?transitionKey=confirm-creation
```

Bu, "confirm-creation" transition'ı için onay view'ını döndürür.

#### platform

View içeriği için hedef platformu belirtir. Desteklenen değerler: `web`, `ios`, `android`

Sistem, platforma özel içerik seçimini otomatik olarak yönetir:
- İstenen platform için bir platform override varsa → override içeriğini döndürür
- Override yoksa → orijinal view içeriğini döndürür
- Client platform seçim mantığını uygulamak zorunda değildir

**Örnek:**
```http
GET /core/workflows/account-opening/instances/123/functions/view?platform=ios
```

Sistem, view tanımına göre iOS'a özel içerik mi yoksa varsayılan içerik mi döndüreceğini otomatik olarak belirler.

### View Seçim Mantığı

Fonksiyon, hangi view'ın döndürüleceğini belirlemek için bu mantığı izler:

```
1. transitionKey sağlandı mı?
   ├─ Evet: Transition'ın tanımlı bir view'ı var mı kontrol et
   │   ├─ Evet: Transition view'ını kullan
   │   └─ Hayır: State view'ını kullan (veya state view yoksa boş döndür)
   └─ Hayır: State view'ını kullan

2. platform sağlandı mı?
   ├─ Evet: View'ın bu platform için platform override'ı var mı kontrol et
   │   ├─ Evet: Override içeriğini ve display ayarlarını kullan
   │   └─ Hayır: Orijinal içeriği ve display ayarlarını kullan
   └─ Hayır: Orijinal içeriği ve display ayarlarını kullan

3. Accept-Language header'ına göre dil seçimi uygula
   └─ View'ın labels dizisinden uygun etiketi döndür
```

### Kullanım Alanları

1. **State View Render Etme**: Mevcut workflow durumu için view alma (sistem platform seçimini yönetir)
2. **Transition Onayı**: Client, transition submit öncesi view var mı diye `transitionKey` ile sorgular
3. **Platforma Özel UI**: Sistem, web, iOS veya Android için optimize edilmiş view'ları otomatik olarak sunar
4. **Çoklu Dil Desteği**: View'ları kullanıcının tercih ettiği dilde gösterme
5. **Wizard Flow'ları**: Adım adım input formları alma

**Önemli**: Sistem, tüm platform ve transition tabanlı view seçim mantığını yönetir. Client'ın yapması gerekenler:
- Transition submit öncesi transition view kontrolü için `transitionKey` parametresini sağlamak
- Platforma özel içerik için isteğe bağlı olarak `platform` parametresini (web/ios/android) sağlamak
- Sistem, hangi view içeriğinin döndürüleceğini otomatik olarak belirler

### Örnek İstekler

**State View Al:**
```http
GET /core/workflows/account-opening/instances/123/functions/view HTTP/1.1
Host: api.example.com
Accept: application/json
Accept-Language: tr-TR
```

**Transition View Al:**
```http
GET /core/workflows/account-opening/instances/123/functions/view?transitionKey=final-confirmation HTTP/1.1
Host: api.example.com
Accept: application/json
```

**Mobil'e Özel View Al:**
```http
GET /core/workflows/account-opening/instances/123/functions/view?platform=mobile HTTP/1.1
Host: api.example.com
Accept: application/json
Accept-Language: tr-TR
```

**Mobil Transition View Al:**
```http
GET /core/workflows/account-opening/instances/123/functions/view?transitionKey=submit&platform=mobile HTTP/1.1
Host: api.example.com
Accept: application/json
```

## En İyi Uygulamalar

### 1. Long-Polling'i Verimli Kullanın

- Başarısız istekler için exponential backoff uygulayın
- Makul polling aralıkları belirleyin (3-10 saniye)
- Workflow final state'e ulaştığında polling'i durdurun
- Gereksiz veri transferlerinden kaçınmak için ETag kullanın

### 2. ETag Önbelleklemesinden Yararlanın

- Her zaman saklanan ETag ile `If-None-Match` header'ı gönderin
- 304 response'larını düzgün şekilde handle edin
- Her başarılı 200 response'unda ETag'leri güncelleyin
- Transition'larda optimistic locking için ETag'leri kullanın

### 3. Platform Parametresini Doğru Kullanın

- Client tipine göre platform parametresi (web/ios/android) gönderin
- Sistem platforma özel içerik seçimini otomatik olarak yönetir
- Fallback mantığı uygulamaya gerek yok - sistem handle eder
- Platforma özel view'ları uygun şekilde önbellekleyin

### 4. View Render Etmeyi Optimize Edin

- Muhtemel sonraki state'ler için view'ları önceden alın
- View tanımlarını yerel olarak önbellekleyin
- Mümkün olduğunda view içeriğini lazy-load edin
- Tekrarlanan view'lar için view render pooling uygulayın

### 5. Performansı İzleyin

Şu metrikler için takip yapın:
- Fonksiyon başına ortalama response süresi
- ETag istekleri için cache hit oranı
- Polling aralığı verimliliği
- View render etme performansı

### 6. Güvenlik Değerlendirmeleri

- Function çağrıları için her zaman HTTPS kullanın
- Header'larda authentication token'ları dahil edin
- Render etmeden önce view içeriğini doğrulayın (XSS önleme)
- Client tarafında rate limiting uygulayın
- İstek takibi için correlation ID'leri kullanın

## İlgili Dokümantasyon

- [View Yönetimi](./view.md) - View tanımları ve gösterim stratejileri
- [State Yönetimi](./state.md) - Workflow state'lerini anlama
- [Transition Yönetimi](./transition.md) - Transition'ları yürütme
- [Versiyonlama ve ETag](../principles/versioning.md) - ETag pattern detayları

