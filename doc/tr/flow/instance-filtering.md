# Instance Filtreleme Kılavuzu

## Genel Bakış

vNext workflow sistemi, instance'ları sorgulamak için güçlü filtreleme yetenekleri sağlar. Hem **Instance tablo kolonları** hem de **JSON veri alanları** üzerinde legacy format veya GraphQL-stil JSON format kullanarak filtreleme yapabilirsiniz.

## Desteklenen Route'lar

### 1. Function/Data Route

```http
GET /{domain}/workflows/{workflow}/functions/data?filter={...}
```

### 2. Workflow Instances Route

```http
GET /{domain}/workflows/{workflow}/instances?filter={...}
```

Her iki route da `filter` query parametresi ile aynı filtreleme yeteneklerini destekler.

---

## Filtre Formatları

### Legacy Format

Basit anahtar-değer formatı: `field=operator:value`

### GraphQL Format (Önerilen)

Mantıksal operatör desteği olan JSON tabanlı format: `{"field":{"operator":"value"}}`

---

## Filtrelenebilir Alanlar

### Instance Tablo Kolonları

Doğrudan veritabanı kolonları:

| Kolon | Tip | Açıklama | Desteklenen Operatörler |
|-------|-----|----------|-------------------------|
| `key` | string | Instance anahtarı | eq, ne, like, startswith, endswith, in, nin |
| `flow` | string | Workflow adı | eq, ne, like, startswith, endswith, in, nin |
| `status` | string | Instance durumu | eq, ne, in, nin |
| `currentState` (veya `state`) | string | Mevcut state | eq, ne, like, startswith, endswith, in, nin |
| `effectiveState` | string | Etkin state adı (v0.0.33+) | eq, ne, like, startswith, endswith, in, nin |
| `effectiveStateType` | int | Etkin state tipi kodu (v0.0.33+) | eq, ne, gt, ge, lt, le, in, nin |
| `effectiveStateSubType` | int | Etkin state alt tipi kodu (v0.0.33+) | eq, ne, gt, ge, lt, le, in, nin |
| `createdAt` | DateTime | Oluşturulma zamanı | eq, ne, gt, ge, lt, le, between |
| `modifiedAt` | DateTime | Değiştirilme zamanı | eq, ne, gt, ge, lt, le, between |
| `completedAt` | DateTime | Tamamlanma zamanı | eq, ne, gt, ge, lt, le, between |
| `isTransient` | boolean | Geçici işaret | eq, ne |

### JSON Veri Alanları (attributes)

Instance'ın JSON verisinde saklanan herhangi bir alan `attributes` prefix'i kullanılarak filtrelenebilir.

---

## Desteklenen Operatörler

| Operatör | Açıklama | Örnek Değer |
|----------|----------|-------------|
| `eq` | Eşittir | `"1111"` |
| `ne` | Eşit değildir | `"test"` |
| `gt` | Büyüktür | `"100"` |
| `ge` | Büyük veya eşittir | `"100"` |
| `lt` | Küçüktür | `"100"` |
| `le` | Küçük veya eşittir | `"100"` |
| `between` | Arasında (dahil) | `["2024-01-01", "2024-12-31"]` |
| `like` | İçerir (büyük/küçük harf duyarsız) | `"workflow"` |
| `startswith` | İle başlar | `"payment"` |
| `endswith` | İle biter | `"flow"` |
| `in` | Listede | `["Active", "Busy"]` |
| `nin` | Listede değil | `["Completed", "Faulted"]` |
| `isnull` | Null veya null değil | `true` veya `false` |

---

## Status Değerleri

`status` alanı hem kod hem de isim kabul eder:

| Status İsmi | Kod | Açıklama |
|-------------|-----|----------|
| `Active` | `A` | Instance aktif |
| `Busy` | `B` | Instance işlem yapıyor |
| `Completed` | `C` | Instance başarıyla tamamlandı |
| `Faulted` | `F` | Instance hata aldı |
| `Passive` | `P` | Instance pasif |

---

## GraphQL Format Örnekleri

### 1. Basit Instance Kolon Filtresi

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"key":{"eq":"payment-12345"}}
```

### 2. Çoklu Instance Kolon Filtreleri (AND Mantığı)

Aynı seviyedeki birden fazla alan AND mantığı ile birleştirilir:

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"status":{"eq":"Active"},"createdAt":{"gt":"2024-01-01"}}
```

### 3. JSON Veri Alanı Filtresi (attributes)

`attributes` prefix'i kullanarak JSON veri alanlarını filtreleyin:

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"attributes":{"customerId":{"eq":"CUST-123"}}}
```

### 4. Karışık Filtre (Instance + JSON Alanları)

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"key":{"like":"payment"},"status":{"eq":"Active"},"attributes":{"amount":{"gt":"500"}}}
```

### 5. Tarih Aralığı Filtresi

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"createdAt":{"between":["2024-01-01","2024-01-31"]}}
```

### 6. Status IN Filtresi

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"status":{"in":["Active","Busy"]}}
```

### 7. EffectiveState Filtreleri (v0.0.33+)

**Etkin State Adına Göre Filtreleme:**
```http
GET /banking/workflows/payment-workflow/functions/data?filter={"effectiveState":{"eq":"awaiting-approval"}}
```

**Etkin State Alt Tipine Göre Filtreleme (İnsan Görevleri):**
```http
GET /approvals/workflows/approval-flow/functions/data?filter={"effectiveStateSubType":{"eq":"6"}}
```

**Etkin State Alt Tipine Göre Filtreleme (Meşgul Görevler):**
```http
GET /processing/workflows/order-flow/functions/data?filter={"effectiveStateSubType":{"eq":"5"}}
```

**Birleşik Status ve EffectiveState Filtresi:**
```http
GET /core/workflows/payment/functions/data?filter={"status":{"eq":"Active"},"effectiveStateSubType":{"eq":"6"}}
```

**EffectiveState Alt Tip Değerleri:**
- `0` - Yok (None)
- `1` - Başarı (Success)
- `2` - Hata (Error)
- `3` - Sonlandırıldı (Terminated)
- `4` - Askıya Alındı (Suspended)
- `5` - Meşgul (Busy) - işlem devam ediyor
- `6` - İnsan (Human) - insan etkileşimi gerekli

---

## Mantıksal Operatörler

### AND Operatörü

Tüm koşulların doğru olması gereken birden fazla koşulu birleştirir:

```json
{
  "and": [
    {"status": {"eq": "Active"}},
    {"attributes": {"amount": {"gt": "500"}}}
  ]
}
```

### OR Operatörü

Herhangi birinin doğru olabileceği birden fazla koşulu birleştirir:

```json
{
  "or": [
    {"key": {"eq": "payment-12345"}},
    {"key": {"eq": "payment-12346"}}
  ]
}
```

### NOT Operatörü

Bir koşulu tersine çevirir:

```json
{
  "not": {"status": {"in": ["Completed", "Faulted"]}}
}
```

### Karmaşık İç İçe Örnek

```json
{
  "and": [
    {"status": {"eq": "Active"}},
    {
      "or": [
        {"attributes": {"priority": {"eq": "high"}}},
        {"attributes": {"amount": {"gt": "10000"}}}
      ]
    }
  ]
}
```

---

## Group By ve Aggregations

### Group By ile Count

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"groupBy":{"field":"attributes.status","aggregations":{"count":true}}}
```

**Yanıt:**
```json
{
  "groups": [
    {"name": "pending", "count": 45},
    {"name": "approved", "count": 123},
    {"name": "rejected", "count": 12}
  ]
}
```

### Group By ile Çoklu Aggregation

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"groupBy":{"field":"attributes.currency","aggregations":{"count":true,"sum":"attributes.amount","avg":"attributes.amount","min":"attributes.amount","max":"attributes.amount"}}}
```

**Yanıt:**
```json
{
  "groups": [
    {"name": "USD", "count": 150, "sum": 450000, "avg": 3000, "min": 10, "max": 50000},
    {"name": "EUR", "count": 75, "sum": 180000, "avg": 2400, "min": 50, "max": 25000}
  ]
}
```

### Desteklenen Aggregation'lar

| Aggregation | Açıklama |
|-------------|----------|
| `count` | Gruptaki öğe sayısı |
| `sum` | Sayısal alanın toplamı |
| `avg` | Sayısal alanın ortalaması |
| `min` | Minimum değer |
| `max` | Maksimum değer |

---

## En İyi Uygulamalar

### 1. Kompleks Sorgular için GraphQL Format Kullanın

GraphQL formatı daha okunabilir ve mantıksal operatörleri destekler.

**İyi:**
```json
{
  "and": [
    {"status": {"eq": "Active"}},
    {"attributes": {"amount": {"gt": "500"}}}
  ]
}
```

### 2. Daha İyi Performans için Spesifik Alanlar Kullanın

Mümkün olduğunda indekslenmiş Instance kolonlarını filtreleyin.

**Daha İyi Performans:**
```json
{"key": {"eq": "payment-12345"}}
```

**Daha Yavaş:**
```json
{"attributes": {"indekslenmemişAlan": {"eq": "değer"}}}
```

### 3. Okunabilirlik için Status İsimleri Kullanın

```json
{"status": {"eq": "Active"}}
```
şuna eşittir:
```json
{"status": {"eq": "A"}}
```

### 4. Analitik için Group By Kullanın

İstatistiklere ihtiyacınız olduğunda, tüm kayıtları çekmek yerine group by kullanın.

```json
{
  "groupBy": {
    "field": "attributes.status",
    "aggregations": {"count": true, "sum": "attributes.amount"}
  }
}
```

### 5. Daima Sayfalama Kullanın

Her zaman `page` ve `pageSize` parametrelerini kullanın:

```http
GET /banking/workflows/payment-workflow/functions/data?filter={...}&page=1&pageSize=20
```

---

## Hata Yönetimi

### Geçersiz Filtre Syntax

```json
{
  "error": {
    "code": "invalid_filter",
    "message": "Geçersiz filtre sözdizimi. Geçerli JSON bekleniyor."
  }
}
```

### Desteklenmeyen Operatör

```json
{
  "error": {
    "code": "unsupported_operator",
    "message": "'regex' operatörü desteklenmiyor",
    "supportedOperators": ["eq", "ne", "gt", "ge", "lt", "le", "between", "like", "startswith", "endswith", "in", "nin", "isnull"]
  }
}
```

### Geçersiz Kolon Adı

```json
{
  "error": {
    "code": "invalid_column",
    "message": "'gecersizKolon' geçerli bir Instance kolonu değil. JSON alanları için 'attributes.alanAdi' kullanın.",
    "validColumns": ["key", "flow", "status", "currentState", "createdAt", "modifiedAt", "completedAt", "isTransient"]
  }
}
```

---

## Performans İpuçları

1. **Sayfalama Kullanın**: Daima `page` ve `pageSize` parametrelerini kullanın
2. **İndeksli Kolonlarda Filtreleyin**: Daha iyi performans için `key`, `status`, `createdAt` tercih edin
3. **Group By Alanlarını Sınırlayın**: Optimal performans için maksimum 2-3 alanda group by yapın
4. **Tarih Aralıklarını Akıllıca Kullanın**: Dar tarih aralıkları sorgu performansını artırır
5. **Büyük Veri Setlerinde Wildcard Aramadan Kaçının**: Mümkün olduğunda `like` yerine `startswith` veya `endswith` kullanın

---

## İlgili Dökümanlar

- [Function API'leri](./function.md) - Yerleşik sistem fonksiyonları (State, Data, View)
- [Custom Functions](./custom-function.md) - Kullanıcı tanımlı fonksiyonlar
- [Instance Yaşam Döngüsü](../how-to/start-instance.md) - Instance başlatma ve yönetimi
