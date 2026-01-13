# GetInstances Task

GetInstances Task, sayfalama, sıralama ve filtreleme desteğiyle diğer workflow'lardan instance verilerini çekmeyi sağlar. Bu task, workflow'lar arası veri sorguları ve toplu görünümler oluşturmak için kullanışlıdır.

## Genel Bakış

| Özellik | Değer |
|---------|-------|
| **Task Tipi** | `15` |
| **Amaç** | Başka bir workflow'dan instance'ları çek |
| **API Endpoint** | `GET /api/v1/{domain}/workflows/{workflow}/functions/data` |

## Konfigürasyon

### Task Tanımı

```json
{
  "key": "fetch-customer-orders",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-tasks",
  "flowVersion": "1.0.0",
  "tags": ["data-fetch", "workflow-communication", "pagination"],
  "attributes": {
    "type": "15",
    "config": {
      "domain": "sales",
      "flow": "order-workflow",
      "page": 1,
      "pageSize": 10,
      "sort": "-CreatedAt",
      "filter": ["status eq 'active'"]
    }
  }
}
```

### Config Parametreleri

| Parametre | Tip | Zorunlu | Varsayılan | Açıklama |
|-----------|-----|---------|------------|----------|
| `domain` | string | Evet | - | Hedef workflow domain'i |
| `flow` | string | Evet | - | Hedef workflow adı |
| `page` | int | Hayır | `1` | Sayfa numarası (1 tabanlı indeks) |
| `pageSize` | int | Hayır | `10` | Sayfa başına öğe sayısı |
| `sort` | string | Hayır | - | İsteğe bağlı yön önekiyle sıralama alanı |
| `filter` | string[] | Hayır | - | Filtre ifadeleri dizisi |
| `useDapr` | bool | Hayır | `false` | Doğrudan HTTP yerine Dapr servis çağrısı kullan |

### Sort Parametresi

`sort` parametresi, isteğe bağlı bir yön önekiyle sıralama alanını belirtir:

| Format | Açıklama | Örnek |
|--------|----------|-------|
| `FieldName` | Artan sıralama | `CreatedAt` |
| `-FieldName` | Azalan sıralama | `-CreatedAt` |

**Yaygın Sıralama Alanları:**
- `CreatedAt` - Instance oluşturma tarihi
- `UpdatedAt` - Son güncelleme tarihi
- `Key` - Instance anahtarı

### Filter Parametresi

`filter` parametresi, sorguya uygulanan filtre ifadelerinden oluşan bir dizi kabul eder:

```json
{
  "filter": [
    "status eq 'active'",
    "amount gt 1000"
  ]
}
```

## Kullanım Örnekleri

### Temel Kullanım

Bir workflow'dan ilk 10 instance'ı çek:

```json
{
  "attributes": {
    "type": "15",
    "config": {
      "domain": "core",
      "flow": "customer-workflow"
    }
  }
}
```

### Sayfalama ile

Sayfa başına 25 öğe ile 3. sayfayı çek:

```json
{
  "attributes": {
    "type": "15",
    "config": {
      "domain": "core",
      "flow": "customer-workflow",
      "page": 3,
      "pageSize": 25
    }
  }
}
```

### Sıralama ile

Oluşturma tarihine göre sıralanmış instance'ları çek (en yeniler önce):

```json
{
  "attributes": {
    "type": "15",
    "config": {
      "domain": "core",
      "flow": "customer-workflow",
      "sort": "-CreatedAt"
    }
  }
}
```

### Filtreleme ile

Belirli kriterlere sahip aktif instance'ları çek:

```json
{
  "attributes": {
    "type": "15",
    "config": {
      "domain": "core",
      "flow": "order-workflow",
      "filter": [
        "status eq 'active'",
        "total gt 500"
      ]
    }
  }
}
```

### Tam Örnek

Tüm parametreleri içeren tam konfigürasyon:

```json
{
  "key": "get-pending-orders",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-tasks",
  "flowVersion": "1.0.0",
  "tags": ["task-test", "instances", "data-fetch", "filter-test", "pagination"],
  "attributes": {
    "type": "15",
    "config": {
      "domain": "sales",
      "flow": "order-workflow",
      "page": 1,
      "pageSize": 50,
      "sort": "-CreatedAt",
      "filter": [
        "state eq 'pending'",
        "priority eq 'high'"
      ],
      "useDapr": false
    }
  }
}
```

## Response Mapping

Task, instance verilerini içeren sayfalanmış bir yanıt döndürür. Verileri çıkarmak ve dönüştürmek için mapping kullanın:

### Yanıt Yapısı

```json
{
  "links": {
    "self": "/api/v1/core/workflows/order-workflow/functions/data?page=1&pageSize=10",
    "first": "/api/v1/core/workflows/order-workflow/functions/data?page=1&pageSize=10",
    "next": "/api/v1/core/workflows/order-workflow/functions/data?page=2&pageSize=10",
    "prev": ""
  },
  "items": [
    {
      "data": {
        "orderId": "ORDER-001",
        "status": "pending",
        "amount": 1500
      },
      "etag": "01ARZ3NDEKTSV4RRFFQ69G5FAV",
      "extensions": {}
    },
    {
      "data": {
        "orderId": "ORDER-002",
        "status": "active",
        "amount": 2300
      },
      "etag": "01ARZ3NDEKTSV4RRFFQ69G5FAW",
      "extensions": {}
    }
  ]
}
```

### Yanıt Alanları

| Alan | Açıklama |
|------|----------|
| `links.self` | Mevcut sayfa URL'i |
| `links.first` | İlk sayfa URL'i |
| `links.next` | Sonraki sayfa URL'i (son sayfadaysa boş) |
| `links.prev` | Önceki sayfa URL'i (ilk sayfadaysa boş) |
| `items` | Instance verileri dizisi |
| `items[].data` | Instance veri nesnesi |
| `items[].etag` | Eşzamanlılık kontrolü için ETag |
| `items[].extensions` | Extension verileri (varsa) |

### Mapping Örneği

```csharp
public async Task<ScriptResponse> OutputHandler(ScriptContext context)
{
    var response = context.Body;
    
    var items = response?.items;
    var links = response?.links;
    
    // Her bir öğeyi işle
    var orders = new List<dynamic>();
    foreach (var item in items)
    {
        var data = item?.data;
        var etag = item?.etag;
        orders.Add(new { data, etag });
    }
    
    return new ScriptResponse
    {
        Data = new
        {
            orders = orders,
            hasNextPage = !string.IsNullOrEmpty(links?.next?.ToString())
        }
    };
}
```

## Kullanım Senaryoları

### 1. Workflow'lar Arası Veri Toplama

Toplu görünümler veya raporlar oluşturmak için birden fazla workflow'dan veri çekin.

### 2. Referans Veri Arama

İşleme sırasında master data workflow'larından referans verilerini arayın.

### 3. Mevcut Kayıtlara Karşı Doğrulama

Yeni instance'lar oluşturmadan önce mevcut kayıtları kontrol edin.

### 4. Dashboard Veri Toplama

Dashboard görüntüleri için çeşitli workflow'lardan instance verilerini toplayın.

## En İyi Uygulamalar

1. **Sayfalama Kullanın**: Performans sorunlarından kaçınmak için büyük veri setleri için her zaman sayfalama kullanın.

2. **Sayfa Boyutunu Sınırlayın**: Optimum performans için `pageSize` değerini makul tutun (10-50).

3. **Filtre Kullanın**: Aktarılan veri miktarını azaltmak için filtreler uygulayın.

4. **Sonuçları Önbelleğe Alın**: API çağrılarını azaltmak için sık erişilen verileri önbelleğe almayı düşünün.

5. **Boş Sonuçları Yönetin**: Kriterlere uyan instance olmadığı durumları her zaman ele alın.

## İlgili Dökümanlar

- [Task Genel Bakış](../task.md) - Task tipleri ve yürütme
- [Mapping](../mapping.md) - Task'lar arası veri eşleme
- [Trigger Task](./trigger-task.md) - Workflow instance yönetimi
