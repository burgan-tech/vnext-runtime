# Function API'leri

Function API'leri, workflow instance'ları için sistem seviyesi operasyonlar sağlar. Bu yerleşik fonksiyonlar, client'ların workflow engine dahili yapılarına doğrudan erişmeden instance durumunu sorgulamasına, veri almasına ve view bilgisi elde etmesine olanak tanır.

## İçindekiler

1. [Genel Bakış](#genel-bakış)
2. [State Fonksiyonu](#state-fonksiyonu)
3. [Data Fonksiyonu](#data-fonksiyonu)
4. [View Fonksiyonu](#view-fonksiyonu)
5. [Yetkilendirme (Authorization)](#yetkilendirme-authorization)
6. [En İyi Uygulamalar](#en-iyi-uygulamalar)
7. [İlgili Dökümanlar](#ilgili-dökümanlar)

## Genel Bakış

vNext Runtime platformu, her workflow instance'ı için otomatik olarak kullanılabilir olan üç temel function API'si sağlar:

| Fonksiyon | Amaç | Endpoint Deseni |
|-----------|------|-----------------|
| **State** | Instance durumu için long-polling | `GET /{domain}/workflows/{workflow}/instances/{instance}/functions/state` |
| **Data** | Instance verisini alma | `GET /{domain}/workflows/{workflow}/instances/{instance}/functions/data` |
| **View** | View içeriğini alma | `GET /{domain}/workflows/{workflow}/instances/{instance}/functions/view` |

> **Not:** Schema Fonksiyonu ve kullanıcı tanımlı fonksiyonlar için bkz. [Custom Functions](./custom-function.md).

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
| `instance` | Path | string | Evet | Instance ID or Key |

### Response

```json
{
  "data": {
    "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/data"
  },
  "view": {
    "hasView": true,
    "loadData": true,
    "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/view"
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
      "name": "approve",
      "view": {
        "hasView": false,
        "loadData": true,
        "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/view?transitionKey=approve"
      },
      "schema": {
        "hasSchema": true,
        "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/schema?transitionKey=approve"
      }
    },
    {
      "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/transitions/reject",
      "name": "reject",
      "view": {
        "hasView": false,
        "loadData": true,
        "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/view?transitionKey=reject"
      },
      "schema": {
        "hasSchema": true,
        "href": "/core/workflows/oauth-flow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/schema?transitionKey=reject"
      }
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
| `view.hasView` | `boolean` | Mevcut state için view olup olmadığı (v0.0.39+) |
| `view.href` | `string` | View fonksiyon endpoint URL'i |
| `view.loadData` | `boolean` | View'ın instance data'ya ihtiyaç duyup duymadığı |
| `state` | `string` | Instance'ın mevcut durumu |
| `status` | `string` | Instance durum kodu (A=Active, C=Completed, vb.) |
| `activeCorrelations` | `array` | Aktif sub-flow'lar ve correlation'lar |
| `transitions` | `array` | Mevcut durumdan kullanılabilir transition'lar (v0.0.39+ role grant'a göre filtrelenir) |
| `transitions[].view` | `object` | Transition için view bilgisi (v0.0.39+) |
| `transitions[].view.hasView` | `boolean` | Bu transition için view olup olmadığı (v0.0.39+) |
| `transitions[].schema` | `object` | Transition için şema linki (tanımlıysa) |
| `transitions[].schema.hasSchema` | `boolean` | Bu transition için şema olup olmadığı (v0.0.39+) |
| `transitions[].schema.href` | `string` | transitionKey ile Schema fonksiyon endpoint URL'i |
| `eTag` | `string` | Cache doğrulama için ETag |

### Transition'ların role grant'a göre filtrelenmesi (v0.0.39+)

State fonksiyonunun döndürdüğü `transitions` dizisi **transition role grant**'larına göre filtrelenir. Yalnızca çağıranın izinli rolü olduğu transition'lar dahil edilir. Roller statik (örn. kimlik sağlayıcınızdan) veya ön tanımlı sistem rolleri **$InstanceStarter** (instance'ı başlatan actor) ve **$PreviousUser** (bir önceki transition'ı tetikleyen actor) olabilir. Gereksiz view veya schema istekleri ve 404'leri önlemek için `view.hasView` ve `schema.hasSchema` kullanın.

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

### Trace ve logda ParentInstanceId (v0.0.38+)

Trace ve loglarda **ParentInstanceId** alanı kullanılır; böylece parent instance ile başlatılan veya tetiklenen (subflow, cross-domain) child instance'ların log ve trace'leri parent instance id ile ilişkilendirilerek takip edilebilir.

### Örnek İstek

```http
GET /core/workflows/oauth-authentication-workflow/instances/f410f37d-dc4b-4442-af84-e3a4707bd949/functions/state HTTP/1.1
Host: api.example.com
Accept: application/json
```

## Data Fonksiyonu

Data fonksiyonu, bir workflow instance'ının mevcut verisini alır. Verimli veri senkronizasyonu için ETag tabanlı önbellekleme destekler.

> **Filtreleme:** GraphQL-stil sorgular, mantıksal operatörler ve aggregation'lar dahil gelişmiş filtreleme yetenekleri için bkz. [Instance Filtreleme Kılavuzu](./instance-filtering.md).

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
| `data` | `object` | Mevcut instance verisi (camelCase özellikler); Master şemada roleGrant kullanıldığında [Alan bazlı görünürlük](#master-şema-alan-bazlı-görünürlük-v039) bölümüne bakın |
| `eTag` | `string` | Cache doğrulama için ETag |
| `extensions` | `object` | Kayıtlı extension'lardan ek veriler |

### Instance zarfı ve metadata (v0.0.39+)

GetInstance ve GetInstances yanıtları, **instance zarfı** bilgisini içerebilir: `id`, `key`, `flow`, `domain`, `flowVersion`, `etag`, `tags`, **metadata** (örn. `currentState`, `effectiveState`, `status`, `createdAt`, `modifiedAt`, `createdBy`, `modifiedBy`, `createdByBehalfOf`, `modifiedByBehalfOf`), `attributes` ve `extensions`. Böylece veri yüküyle birlikte instance kimliği ve audit bilgisi sunulur.

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

#### Authorization-aware ETag (v0.0.40+)

Data endpoint'leri yetkilendirme kullandığında yanıt çağırana göre değişebilir. v0.0.40 itibarıyla ETag stratejisi şöyle ayrılır:

- **etag** — **Yanıt** (veya istek bağlamı, örn. yetkilendirme) değiştiğinde değişir. Genel yanıt önbelleği ve `If-None-Match` koşullu istekler için kullanın.
- **entityETag** — **Entity verisi** değiştiğinde değişir. Gerçek veri değişimini (örn. senkronizasyon veya invalidation) tespit etmek için kullanın.

Her iki değer hem response body'de hem header'larda yer alır:

| Body alanı     | Response header   | Anlamı                    |
|----------------|-------------------|---------------------------|
| `etag`         | `ETag`            | Yanıt/istek değişimi      |
| `entityEtag`   | `X-Entity-ETag`   | Entity veri değişimi      |

**Örnek response body:**
```json
{
  "etag": "\"Hko5JI4fDcAOOnf-KGFNA7Xo_MpuxcLl1_hg5j2Sua8\"",
  "entityEtag": "\"01KK8Q8N5T6H49T8AENYT6Z6ZQ\""
}
```

**Örnek response header'ları:**
```http
ETag: "Hko5JI4fDcAOOnf-KGFNA7Xo_MpuxcLl1_hg5j2Sua8"
X-Entity-ETag: "01KK8Q8N5T6H49T8AENYT6Z6ZQ"
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

View fonksiyonu, mevcut workflow durumu için uygun view tanımını alır. Platforma özel içerik, transition'a özel view'lar ve **uzak (cross-domain) view'lar** (v0.0.39+) desteklenir: başka bir domain'de host edilen view'lar referans verilerek kullanılabilir; böylece ortak view'ların yeniden kullanımı, versiyonlama ve dağıtımı tek merkezden yönetilebilir.

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

**İçerik tipi (v0.0.39+):** `content`, view `type`'ına göre tiplenir: `type` **Json** ise `content` bir object veya array; `type` **Html** (ve benzeri) ise `content` bir string'dir.

**Json view örneği:**
```json
{
  "key": "account-type-selection-view",
  "content": {
    "type": "form",
    "title": { "en-US": "Choose Your Account Type", "tr-TR": "Hesap Türünüzü Seçin" },
    "fields": [...]
  },
  "type": "Json",
  "display": "full-page",
  "label": "Hesap Tipi Seç"
}
```

**Html view örneği:** `content` bir string'dir (örn. `"<div>...</div>"`).

### Response Alanları

| Alan | Tip | Açıklama |
|------|-----|----------|
| `key` | `string` | View key tanımlayıcısı |
| `content` | `string` veya `object`/`array` | View içeriği: **Json** tipi → object/array (v0.0.39+); **Html** ve benzeri → string |
| `type` | `string` | İçerik tipi (Json, Html, vb.) |
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

1. **Kural tabanlı view'lar**: State veya transition bir `views` dizisi (kural tabanlı view seçimi) tanımlıyorsa, ilk eşleşen kural view'ı belirler. Detay için [Kural Tabanlı View Seçimi](./rule-based-view-selection.md) dokümanına bakın.
2. **Tek view / eski kullanım**: `transitionKey` sağlandıysa transition view'ı (tanımlıysa), değilse state view kullanılır; sağlanmadıysa state view kullanılır.
3. **Platform**: `platform` sağlandıysa ve view'da platform override varsa o içerik ve display kullanılır; yoksa varsayılan.
4. **Dil**: Accept-Language uygulanır ve view'ın labels dizisinden uygun etiket döndürülür.

```
1. State/transition'da "views" dizisi (kural tabanlı) var mı?
   ├─ Evet: Kuralları sırayla değerlendir; ilk eşleşen view'ı döndür (veya kuralı olmayan varsayılan giriş)
   └─ Hayır: Aşağıdaki tek view mantığına geç

2. transitionKey sağlandı mı?
   ├─ Evet: Transition'ın tanımlı bir view'ı var mı kontrol et
   │   ├─ Evet: Transition view'ını kullan
   │   └─ Hayır: State view'ını kullan (veya state view yoksa boş döndür)
   └─ Hayır: State view'ını kullan

3. platform sağlandı mı?
   ├─ Evet: View'ın bu platform için platform override'ı var mı kontrol et
   │   ├─ Evet: Override içeriğini ve display ayarlarını kullan
   │   └─ Hayır: Orijinal içeriği ve display ayarlarını kullan
   └─ Hayır: Orijinal içeriği ve display ayarlarını kullan

4. Accept-Language header'ına göre dil seçimi uygula
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

## Yetkilendirme (Authorization)

Workflow'larda fonksiyon, flow, state ve transition seviyesinde **roles** ve **queryRoles** tanımlanabilir. Aşağıdaki sistem fonksiyon endpoint'leri yetki bilgilerini ve yetkilendirme kontrolünü sunar (v0.0.37+).

### Ön tanımlı sistem rolleri (v0.0.39+)

Instance yetkilendirmesi için (örn. transition roles, state/flow queryRoles veya Master şema alan görünürlüğünde) iki statik sistem rolü kullanılabilir:

| Rol | Açıklama |
|-----|----------|
| **$InstanceStarter** | Instance'ı başlatan actor |
| **$PreviousUser** | Bir önceki transition'ı tetikleyen actor |

Transition veya roleGrant örneği:

```json
{
  "roles": [
    { "role": "$InstanceStarter", "grant": "allow" },
    { "role": "$PreviousUser", "grant": "allow" }
  ]
}
```

### Master şema alan bazlı görünürlük (v0.0.39+)

Flow **Master şeması**, şema property'lerinde **roleGrant** (`roles`) özelliği ile **alan bazlı görünürlük** tanımlayabilir. Data fonksiyonu ve veri dönen endpoint'ler (Get Instance, GetInstances vb.) authorize katmanını çalıştırır ve yalnızca çağıranın görmesine izinli olduğu alanları döndürür. `roles` tanımı olmayan property'ler tüm yetkili çağıranlara görünür. Vocabulary ve araç uyumluluğu için [roles-vocab.json](https://unpkg.com/@burgan-tech/vnext-schema@0.0.37/vocabularies/roles-vocab.json) kullanılabilir.

### Get Flow Permissions

Flow için tanımlı roles ve queryRoles bilgisini döner.

```http
GET /api/v1/{domain}/workflows/{workflow}/functions/permissions
```

**Örnek yanıt:**

```json
{
  "workflow": "account-opening",
  "queryRoles": [{ "role": "morph-idm.viewer", "grant": "allow" }],
  "states": [
    { "key": "account-type-selection", "queryRoles": [] },
    {
      "key": "account-details-input",
      "queryRoles": [
        { "role": "morph-idm.maker", "grant": "allow" },
        { "role": "morph-idm.viewer", "grant": "deny" }
      ]
    }
  ],
  "transitions": [
    { "key": "initiate-account-opening", "target": "account-type-selection", "roles": [] },
    {
      "key": "select-demand-deposit",
      "target": "account-details-input",
      "roles": [
        { "role": "morph-idm.maker", "grant": "allow" },
        { "role": "morph-idm.initiator", "grant": "allow" }
      ]
    }
  ],
  "functions": []
}
```

### Get Instance Permissions

Get Flow Permissions ile aynı yanıt yapısı. Instance bir subflow içindeyse **subflow**'un yetkileri döner (subflow'lar ana flow üzerinden yönetilir).

```http
GET /api/v1/{domain}/workflows/{workflow}/instances/{instanceId}/functions/permissions
```

### Flow Authorize

Verilen rolün, flow üzerinde verilen transition (veya function) için izinli olup olmadığını kontrol eder. İsteğe bağlı `version`; verilmezse latest kullanılır.

```http
GET /api/v1/{domain}/workflows/{workflow}/functions/authorize?transitionKey=submit-account-details&role=morph-idm.maker&version=1.0.0
```

**Yanıt 200:** `{ "allowed": true }`  
**Yanıt 403:** `{ "allowed": false }`

### Instance Authorize

Flow Authorize ile aynı yanıt. `queryRoles=true` ile yetki, instance'ın mevcut state'ine (flow ve state queryRoles) göre değerlendirilir. Subflow içindeki instance'larda aktif subflow instance kullanılır.

```http
GET /api/v1/{domain}/workflows/{workflow}/instances/{instanceId}/functions/authorize?queryRoles=true&role=morph-idm.viewer
```

### Authorize Query Parametreleri

| Parametre | Açıklama |
|-----------|----------|
| `role` | Kontrol edilecek rol. |
| `version` | İsteğe bağlı. Flow versiyonu; varsayılan latest. |
| `transitionKey` | Kontrol edilecek transition (transition seviye roles). |
| `functionKey` | Kontrol edilecek fonksiyon (function seviye roles). |
| `queryRoles` | true ise instance'ın mevcut state'i için flow ve state queryRoles kontrol edilir (subflow bağlamı varsa ona göre). |

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

## İlgili Dökümanlar

- [Custom Functions](./custom-function.md) - Kullanıcı tanımlı fonksiyonlar ve Schema Fonksiyonu
- [Instance Filtreleme](./instance-filtering.md) - GraphQL-stil filtreleme kılavuzu
- [View Yönetimi](./view.md) - View tanımları ve gösterim stratejileri
- [Kural Tabanlı View Seçimi](./rule-based-view-selection.md) - Kurallara göre dinamik view seçimi
- [State Yönetimi](./state.md) - Workflow state'lerini anlama
- [Transition Yönetimi](./transition.md) - Transition'ları yürütme
- [Versiyonlama ve ETag](../principles/versioning.md) - ETag pattern detayları

