# Kural Tabanlı View Seçimi

## Genel Bakış

Kural tabanlı view seçimi, çalışma zamanı koşullarına göre farklı view'ların dinamik olarak seçilmesini sağlar. Bu özellik, iş akışı mantığını değiştirmeden platforma özel arayüzler, role dayalı view'lar ve koşullu UI render'ı sunar.

## Kullanım Alanları

- **Platforma özel view'lar**: iOS, Android ve Web istemcileri için farklı view'lar gösterme
- **Role dayalı view'lar**: Kullanıcı rollerine göre farklı arayüzler sunma
- **Koşullu UI**: Instance verisi veya state'e göre view render etme
- **A/B Testi**: Deney koşullarına göre farklı view'lar sunma

## JSON Şeması

`views` özelliği view girişlerinden oluşan bir dizi kabul eder. Her girişte, ne zaman seçileceğini belirleyen isteğe bağlı bir `rule` bulunabilir.

```json
{
  "views": [
    {
      "rule": {
        "location": "inline",
        "code": "using System.Threading.Tasks;\nusing BBT.Workflow.Scripting;\npublic class ViewIosRule : IConditionMapping\n{\n    public async Task<bool> Handler(ScriptContext context)\n    {\n        try\n        {\n            if (context.Headers?[\"x-platform\"] == \"ios\")\n            {\n                return true;\n            }\n            return false;\n        }\n        catch (Exception ex)\n        {\n            return false;\n        }\n    }\n}",
        "encoding": "NAT"
      },
      "view": {
        "domain": "my-domain",
        "key": "ios-view",
        "version": "1.0.0",
        "flow": "sys-views"
      },
      "loadData": true,
      "extensions": ["ext1", "ext2"]
    },
    {
      "view": {
        "domain": "my-domain",
        "key": "default-view",
        "version": "1.0.0",
        "flow": "sys-views"
      },
      "loadData": true
    }
  ]
}
```

## ViewEntry Özellikleri

| Özellik | Tip | Zorunlu | Açıklama |
|---------|-----|---------|----------|
| `rule` | ScriptCode | Hayır | Koşullu değerlendirme için IConditionMapping uygulayan C# script. Yoksa varsayılan olarak kullanılır. |
| `view` | Reference | Evet | Yüklenecek view bileşenine referans. |
| `loadData` | boolean | Hayır | View ile birlikte instance verisi yüklensin mi. Varsayılan: false. |
| `extensions` | string[] | Hayır | Bu view seçildiğinde çalıştırılacak extension listesi. |

### Rule (ScriptCode) Özellikleri

| Özellik | Tip | Zorunlu | Açıklama |
|---------|-----|---------|----------|
| `location` | string | Hayır | Script konumu. Gömülü kod için `"inline"` kullanın. |
| `code` | string | Evet | IConditionMapping arayüzünü uygulayan C# kodu. |
| `encoding` | string | Hayır | Düz metin için `"NAT"`, Base64 için `"B64"`. |

### View (Reference) Özellikleri

| Özellik | Tip | Zorunlu | Açıklama |
|---------|-----|---------|----------|
| `domain` | string | Evet | View'ın kayıtlı olduğu domain. |
| `key` | string | Evet | View'ın benzersiz anahtarı. |
| `version` | string | Evet | View sürümü (semver formatı). |
| `flow` | string | Evet | Flow tipi, genelde `"sys-views"`. |

## Kural Değerlendirmesi

### Değerlendirme Sırası

1. View'lar **dizi sırasına** göre değerlendirilir (baştan sona)
2. **İlk eşleşen kural** kazanır ve o view döner
3. **Kuralı olmayan** bir giriş varsayılan/fallback olarak kullanılır
4. Varsayılan view'ı her zaman dizinin **sonuna** koyun

### Kullanılabilir ScriptContext Özellikleri

`Handler` metodu içinde `ScriptContext` ile şunlara erişebilirsiniz:

| Özellik | Tip | Açıklama |
|---------|-----|----------|
| `context.Headers` | Dictionary | HTTP istek başlıkları |
| `context.QueryParameters` | Dictionary | URL query parametreleri |
| `context.Instance.Data` | dynamic | Mevcut instance verisi |
| `context.Instance.Key` | string | Instance anahtarı |
| `context.State` | string | Mevcut state anahtarı |
| `context.Transition` | string | Mevcut transition anahtarı (varsa) |
| `context.CurrentTransition.Data` | dynamic | Orijinal transition istek gövdesi (v0.0.37+) |
| `context.CurrentTransition.Header` | dynamic | Orijinal transition istek başlıkları (v0.0.37+) |

## Örnekler

### Örnek 1: Platforma Göre View Seçimi

`x-platform` başlığına göre farklı view'lar seçme:

```json
{
  "key": "checkout-state",
  "stateType": 1,
  "views": [
    {
      "rule": { "location": "inline", "code": "...", "encoding": "NAT" },
      "view": { "domain": "ecommerce", "key": "checkout-ios", "version": "1.0.0", "flow": "sys-views" },
      "loadData": true
    },
    {
      "rule": { "location": "inline", "code": "...", "encoding": "NAT" },
      "view": { "domain": "ecommerce", "key": "checkout-android", "version": "1.0.0", "flow": "sys-views" },
      "loadData": true
    },
    {
      "view": { "domain": "ecommerce", "key": "checkout-web", "version": "1.0.0", "flow": "sys-views" },
      "loadData": true
    }
  ]
}
```

### Örnek 2: Role Göre View Seçimi

Instance verisindeki kullanıcı rolüne göre farklı view'lar gösterme:

```json
{
  "key": "approval-state",
  "stateType": 2,
  "views": [
    {
      "rule": { "location": "inline", "code": "...", "encoding": "NAT" },
      "view": { "domain": "hr", "key": "approval-admin-view", "version": "1.0.0", "flow": "sys-views" },
      "loadData": true,
      "extensions": ["adminActions"]
    },
    {
      "view": { "domain": "hr", "key": "approval-default-view", "version": "1.0.0", "flow": "sys-views" },
      "loadData": true
    }
  ]
}
```

### Örnek 3: Transition View'ları

Aynı `views` dizi formatı transition'larda da kullanılabilir:

```json
{
  "key": "submit-transition",
  "target": "review-state",
  "triggerType": 0,
  "views": [
    {
      "rule": { "location": "inline", "code": "...", "encoding": "NAT" },
      "view": { "domain": "forms", "key": "submit-mobile-view", "version": "1.0.0", "flow": "sys-views" },
      "loadData": true
    },
    {
      "view": { "domain": "forms", "key": "submit-desktop-view", "version": "1.0.0", "flow": "sys-views" },
      "loadData": true
    }
  ]
}
```

## En İyi Uygulamalar

1. **Her zaman varsayılan view ekleyin**: Kuralı olmayan bir girişi dizinin sonuna fallback olarak koyun.
2. **Kuralları özelden genele sıralayın**: Daha özel kurallar önce gelsin.
3. **Kuralları basit tutun**: Karmaşık mantık extension veya backend servislerinde olmalı.
4. **Anlamlı view anahtarları kullanın**: View'ları açıklayıcı adlandırın (örn. `checkout-ios`, `approval-admin-view`).
5. **Tüm yolları test edin**: Her kural yolunun uygun koşullarla test edildiğinden emin olun.

## Hata Yönetimi

- Hiçbir kural eşleşmez ve varsayılan view yoksa hata döner
- Başarısız kural değerlendirmesi loglanır ve sonraki kurala geçilir
- Geçersiz kural sözdizimi kuralın başarısız olmasına neden olur (sonrakine devam edilir)

## İlgili Dokümantasyon

- [View Yönetimi](./view.md) - View tanımı ve referans şeması
- [Function API'leri](./function.md) - View function endpoint ve seçim mantığı
- [Instance Filtreleme](./instance-filtering.md) - Query parametre kullanımı
- [Mapping Rehberi](./mapping.md) - ScriptContext ve mapping arayüzleri
