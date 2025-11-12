# View Yönetimi

View'lar, workflow state'leri ve transition'ları için görsel arayüzü temsil eden UI bileşen tanımlarıdır. Bir workflow'un farklı aşamalarında kullanıcıların ne göreceğini tanımlamak için deklaratif bir yol sağlarlar.

## İçindekiler

1. [View Tanımı](#view-tanımı)
2. [View Özellikleri](#view-özellikleri)
3. [View Referans Şeması](#view-referans-şeması)
4. [Gösterim Stratejisi](#gösterim-stratejisi)
5. [Gösterim Modları](#gösterim-modları)
6. [Platform Override'ları](#platform-overrideları)
7. [Kullanım Örnekleri](#kullanım-örnekleri)
8. [En İyi Uygulamalar](#en-iyi-uygulamalar)

## View Tanımı

View, workflow state'leri veya transition'ları göstermek için UI tanımını içeren bir domain entity'sidir. View'lar versiyonlanır ve workflow'lar tarafından referans edilebilir.

### Sınıf Tanımı

```csharp
public sealed class View : IDomainEntity, IViewReference, IReferenceSetter
{
    public string Key { get; private set; }
    public string Flow { get; init; }
    public string Domain { get; private set; }
    public string Version { get; private set; }
    public ViewType Type { get; private set; }
    public string Content { get; private set; }
    public string Display { get; private set; }
    public LanguageLabel[]? Labels { get; private set; }
    public PlatformOverrides? PlatformOverrides { get; private set; }
}
```

## View Özellikleri

### Temel Özellikler

| Özellik | Tip | Açıklama |
|---------|-----|----------|
| `Key` | `string` | View için benzersiz tanımlayıcı |
| `Flow` | `string` | Flow stream bilgisi (varsayılan: `sys-views`) |
| `Domain` | `string` | View'ın ait olduğu domain |
| `Version` | `string` | Versiyon bilgisi (semantic versioning) |
| `Type` | `ViewType` | View içerik tipi (örn: JSON, HTML, vb.) |
| `Content` | `string` | Gerçek view içeriği/tanımı |
| `Display` | `string` | View'ı render etmek için gösterim modu |
| `Labels` | `LanguageLabel[]` | View için çoklu dil etiketleri |
| `PlatformOverrides` | `PlatformOverrides` | Platforma özel view override'ları |

### Content Özelliği

`Content` özelliği gerçek UI tanımını içerir. Bu şunlar olabilir:
- Form tanımları için JSON şeması
- HTML şablonları
- Bileşen referansları
- Özel UI tanımları

### Display Özelliği

View'ın nasıl render edileceğini belirtir. Mevcut seçenekler için [Gösterim Modları](#gösterim-modları) bölümüne bakın.

### Labels Özelliği

View başlıkları ve açıklamaları için çoklu dil desteği:

```json
{
  "labels": [
    {
      "language": "en-US",
      "label": "Account Type Selection"
    },
    {
      "language": "tr-TR",
      "label": "Hesap Tipi Seçimi"
    }
  ]
}
```

## View Referans Şeması

Bir workflow state veya transition'dan view'a referans verirken aşağıdaki şemayı kullanın:

### Tam View Referansı

```json
{
  "view": {
    "view": {
      "key": "account-type-selection-view",
      "domain": "core",
      "version": "1.0.0",
      "flow": "sys-views"
    },
    "loadData": true,
    "extensions": ["user-profile-ext", "account-limits-ext"]
  }
}
```

### Referans Özellikleri

| Özellik | Tip | Gerekli | Açıklama |
|---------|-----|---------|----------|
| `view` | `IViewReference` | Evet | View tanımına referans |
| `loadData` | `boolean` | Hayır | View'ın instance data'ya ihtiyaç duyup duymadığı (varsayılan: false) |
| `extensions` | `string[]` | Hayır | View zenginleştirme için ek veri extension key'leri |

#### loadData Flag'i

- **true**: View, workflow instance data'sına erişim gerektirir. Platform, view'ı render ederken instance data'yı dahil eder.
- **false**: View kendi kendine yeterlidir ve instance data'ya ihtiyaç duymaz.

#### extensions Dizisi

View'ı zenginleştirmek için ek veri kaynaklarını belirtir:
- Extension key'leri harici veri sağlayıcılarına referans verir
- Extension'lar yüklenir ve instance data ile merge edilir
- Ek bilgiler için kullanışlıdır (kullanıcı profilleri, referans verileri, vb.)

## Gösterim Stratejisi

Platform, hangi view'ın render edileceğini belirlemek için belirli stratejiler izler:

### State View Render Etme

**Kural**: Bir state'de birden fazla transition varsa state view render edilir.

```json
{
  "key": "account-selection",
  "stateType": 1,
  "view": {
    "view": {
      "key": "account-type-selection-view",
      "domain": "core",
      "version": "1.0.0",
      "flow": "sys-views"
    },
    "loadData": true
  },
  "transitions": [
    {
      "key": "select-savings",
      "target": "savings-account-details"
    },
    {
      "key": "select-checking",
      "target": "checking-account-details"
    }
  ]
}
```

Bu örnekte, state birden fazla transition'a sahip olduğu için kullanıcının seçim yapmasına izin vermek için `account-type-selection-view` render edilir.

### Transition View Render Etme

**Kural**: Transition view'ları client tarafından handle edilir ve transition submit öncesi kontrol edilir.

Transition view'ları genellikle şunlar için kullanılır:
- Onay diyalogları
- Ek veri girişi
- Uyarı mesajları
- Şartlar ve koşulları kabul etme

```json
{
  "key": "confirm-account-creation",
  "source": "account-details",
  "target": "account-created",
  "view": {
    "view": {
      "key": "account-confirmation-popup-view",
      "domain": "core",
      "version": "1.0.0",
      "flow": "sys-views"
    },
    "loadData": true
  }
}
```

**Client Davranışı**:
1. Transition'ı submit etmeden önce, client bir view'ın var olup olmadığını sorgular
2. Bir view varsa, kullanıcıya render edilir
3. Kullanıcı onayı/girişi beklenir
4. Transition herhangi bir ek veri ile submit edilir

### Wizard State View Render Etme

**Kural**: Wizard tipindeki state'lerde transition view gösterilir.

Wizard state'leri yalnızca **bir transition** kabul eder ve kullanıcıları adım adım bir süreçte yönlendirir:

```json
{
  "key": "wizard-step-1",
  "stateType": 1,
  "attributes": {
    "wizard": true
  },
  "transitions": [
    {
      "key": "next-step",
      "target": "wizard-step-2",
      "view": {
        "view": {
          "key": "wizard-step-1-input-view",
          "domain": "core",
          "version": "1.0.0",
          "flow": "sys-views"
        },
        "loadData": true
      }
    }
  ]
}
```

Wizard modunda:
- Transition view hemen render edilir
- Kullanıcı gerekli girişi sağlar
- Geçerli submit üzerine transition otomatik olarak ilerler

## Gösterim Modları

`display` özelliği view'ın kullanıcıya nasıl sunulacağını belirler. Mevcut modlar:

### 1. full-page

View'ı tüm ekranı kaplayan tam sayfa bileşeni olarak render eder.

**Kullanım Alanları:**
- Ana workflow ekranları
- Karmaşık formlar
- Dashboard görünümleri

```json
{
  "display": "full-page"
}
```

### 2. popup

View'ı mevcut ekranın üzerine gelen modal/popup diyalog olarak render eder.

**Kullanım Alanları:**
- Onay diyalogları
- Uyarı mesajları
- Kısa formlar

```json
{
  "display": "popup"
}
```

### 3. bottom-sheet

View'ı ekranın altından yukarı kayan bottom sheet olarak render eder.

**Kullanım Alanları:**
- Mobil dostu seçimler
- Hızlı işlemler
- Filtre seçenekleri

```json
{
  "display": "bottom-sheet"
}
```

### 4. top-sheet

View'ı ekranın üstünden aşağı kayan top sheet olarak render eder.

**Kullanım Alanları:**
- Bildirimler
- Başarı mesajları
- Hızlı bilgi gösterimi

```json
{
  "display": "top-sheet"
}
```

### 5. drawer

View'ı yandan kayan yan drawer/menü olarak render eder.

**Kullanım Alanları:**
- Navigasyon menüleri
- Ayarlar panelleri
- Yan filtreler

```json
{
  "display": "drawer"
}
```

### 6. inline

View'ı mevcut sayfa içeriğinde inline olarak render eder.

**Kullanım Alanları:**
- Gömülü formlar
- Inline editörler
- Bağlamsal bilgi

```json
{
  "display": "inline"
}
```

## Platform Override'ları

Platform override'ları, farklı platformlar (web, iOS, Android) için farklı view içerikleri sağlamanıza olanak tanır.

### Tanım

```csharp
public class PlatformOverrides
{
    public PlatformOverride? Android { get; private set; }
    public PlatformOverride? Ios { get; private set; }
    public PlatformOverride? Web { get; private set; }
}

public class PlatformOverride : ValueObject
{
    public string Content { get; private set; }
    public string Display { get; private set; }
    public ViewType? Type { get; private set; } = ViewType.Json;
}

public static class PlatformConst
{
    public const string Web = "web";
    public const string Ios = "ios";
    public const string Android = "android";
}
```

### Desteklenen Platformlar

Sistem üç platform tipini destekler:
- **web**: Web tarayıcıları
- **ios**: iOS mobil cihazlar
- **android**: Android mobil cihazlar

### Kullanım

Sistem, `platform` query parametresine göre platforma özel içerik seçimini otomatik olarak yönetir:

**Platform ile İstek:**
```http
GET /core/workflows/account-opening/instances/123/functions/view?platform=ios
```

**Override ile View Tanımı:**
```json
{
  "key": "account-selection-view",
  "content": "{...varsayılan içerik...}",
  "display": "full-page",
  "type": "json",
  "platformOverrides": {
    "ios": {
      "content": "{...iOS için optimize edilmiş içerik...}",
      "display": "bottom-sheet",
      "type": "json"
    },
    "android": {
      "content": "{...Android için optimize edilmiş içerik...}",
      "display": "bottom-sheet",
      "type": "json"
    },
    "web": {
      "content": "{...web için optimize edilmiş içerik...}",
      "display": "full-page",
      "type": "json"
    }
  }
}
```

**Sistem Davranışı:**
- Sistem, platform parametresine göre hangi içeriği döndüreceğini otomatik olarak belirler
- İstenen platform için bir platform override varsa → override içeriğini döndürür
- Override yoksa → orijinal view içeriğini döndürür
- Client platform seçim mantığını handle etmek zorunda değildir

## Kullanım Örnekleri

### Örnek 1: Basit State View

```json
{
  "key": "account-type-selection-view",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-views",
  "type": "json",
  "content": "{\"type\":\"form\",\"fields\":[{\"name\":\"accountType\",\"type\":\"select\",\"options\":[\"savings\",\"checking\"]}]}",
  "display": "full-page",
  "labels": [
    {
      "language": "tr-TR",
      "label": "Hesap Tipi Seç"
    }
  ]
}
```

### Örnek 2: Onay Popup'ı

```json
{
  "key": "final-confirmation-popup-view",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-views",
  "type": "json",
  "content": "{\"type\":\"confirmation\",\"message\":\"Devam etmek istediğinizden emin misiniz?\",\"actions\":[\"confirm\",\"cancel\"]}",
  "display": "popup",
  "labels": [
    {
      "language": "tr-TR",
      "label": "İşlemi Onayla"
    }
  ]
}
```

### Örnek 3: Platforma Özel View

```json
{
  "key": "product-catalog-view",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-views",
  "type": "json",
  "content": "{\"layout\":\"grid\",\"columns\":4}",
  "display": "full-page",
  "platformOverrides": {
    "ios": {
      "content": "{\"layout\":\"list\",\"columns\":1}",
      "display": "full-page",
      "type": "json"
    },
    "android": {
      "content": "{\"layout\":\"list\",\"columns\":1}",
      "display": "full-page",
      "type": "json"
    },
    "web": {
      "content": "{\"layout\":\"grid\",\"columns\":4}",
      "display": "full-page",
      "type": "json"
    }
  },
  "labels": [
    {
      "language": "tr-TR",
      "label": "Ürün Kataloğu"
    }
  ]
}
```

### Örnek 4: Extension'lı View

```json
{
  "view": {
    "view": {
      "key": "user-dashboard-view",
      "domain": "core",
      "version": "1.0.0",
      "flow": "sys-views"
    },
    "loadData": true,
    "extensions": [
      "user-profile-extension",
      "recent-transactions-extension",
      "account-summary-extension"
    ]
  }
}
```

Bu örnekte:
- `loadData: true` instance data'nın dahil edilmesini sağlar
- Extension'lar ek veri sağlar:
  - Kullanıcı profil bilgisi
  - Son işlem geçmişi
  - Hesap özet detayları

## En İyi Uygulamalar

### 1. İçeriği Taşınabilir Tutun

View içeriğini mümkün olduğunca platforma bağımsız tasarlayın. Platform override'larını yalnızca belirli platformlarda (web, iOS, Android) optimal kullanıcı deneyimi için gerekli olduğunda kullanın.

### 2. Uygun Gösterim Modlarını Kullanın

Gösterim modlarını şunlara göre seçin:
- İçerik miktarı
- Gerekli kullanıcı etkileşimi
- Platform konvansiyonları (mobil vs. web)
- Workflow bağlamı (birincil vs. destekleyici işlemler)

### 3. Extension'ları Akıllıca Kullanın

Extension'ları şunlar için kullanın:
- Workflow'dan bağımsız değişen veriler
- Referans verileri (lookup'lar, konfigürasyonlar)
- Kullanıcıya özel bilgiler
- Gerçek zamanlı veri zenginleştirme

Extension'lardan kaçının:
- Temel workflow verileri (bunun yerine instance data kullanın)
- Workflow ile versiyonlanması gereken veriler

### 4. Çoklu Dil Desteğini Uygulayın

Her zaman desteklenen tüm dillerde etiketler sağlayın:

```json
{
  "labels": [
    {
      "language": "en-US",
      "label": "English Label"
    },
    {
      "language": "tr-TR",
      "label": "Türkçe Etiket"
    },
    {
      "language": "es-ES",
      "label": "Etiqueta en Español"
    }
  ]
}
```

### 5. View'ları Uygun Şekilde Versiyonlayın

- Semantic versioning kullanın
- Kırıcı değişiklikler için yeni versiyonlar oluşturun
- Mümkün olduğunda geriye uyumluluğu koruyun
- Değişiklikleri versiyon notlarında belgeleyin

### 6. Mobil Platformlar için Optimize Edin

iOS ve Android için platform override'ları oluştururken:
- Seçimler ve hızlı işlemler için bottom-sheet kullanın
- Layout'ları basitleştirin (sütunları azaltın, daha büyük dokunma hedefleri)
- Metin girişi gereksinimlerini minimize edin
- Başparmakla erişilebilir bölgeleri göz önünde bulundurun
- Hem iOS hem Android cihazlarda test edin
- Platforma özel tasarım kılavuzlarına uyun (Android için Material Design, iOS için Human Interface Guidelines)

### 7. Gösterim Stratejilerini Test Edin

View render etmeyi şunlarla test edin:
- Tek transition'lı state'ler
- Çoklu transition'lı state'ler
- Wizard flow'ları
- Farklı gösterim modları
- Çeşitli ekran boyutları

### 8. View Tanımlarını Önbellekleyin

View'lar şu pattern kullanılarak önbelleklenir:
```
View:{Domain}:{Flow}:{Key}:{Version}
```

Bu, hızlı erişim sağlar ve veritabanı yükünü azaltır.

## İlgili Dokümantasyon

- [State Yönetimi](./state.md) - Workflow state'lerini anlama
- [Transition Yönetimi](./transition.md) - Transition'larla çalışma
- [Function API'leri](./function.md) - View function endpoint detayları
- [Interface Dokümantasyonu](./interface.md) - Mapping interface'leri

