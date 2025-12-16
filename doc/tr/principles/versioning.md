# Versiyon Yönetimi ve Deployment Stratejisi

Bu dokümantasyon, vNext platformunda versiyonlama, paket yönetimi ve deployment süreçlerini kapsamlı bir şekilde açıklar.

## İçindekiler

1. [Versiyon Yönetimi](#versiyon-yönetimi)
2. [ETag Kullanımı](#etag-kullanımı)
3. [Geliştirme Ortamı](#geliştirme-ortamı)
4. [Paket Yönetimi](#paket-yönetimi)
5. [Runtime Deployment](#runtime-deployment)
6. [Versiyon Formatı](#versiyon-formatı)
7. [Test ve Rollback Stratejisi](#test-ve-rollback-stratejisi)

---

## Versiyon Yönetimi

vNext platformunda tüm iş kaydı örnekleri versiyonlar halinde yönetilebilir. Versiyonlamada [semantik versiyonlama](https://semver.org/) yaklaşımı benimsenmiştir.

### Versiyon Formatı

Versiyon standart olarak `MAJOR.MINOR.PATCH` formatındadır:

| Bileşen | Açıklama | Örnek |
|---------|----------|-------|
| **MAJOR** | Geriye uyumsuz API değişiklikleri | `2.0.0` |
| **MINOR** | Geriye uyumlu yeni özellikler | `1.1.0` |
| **PATCH** | Geriye uyumlu hata düzeltmeleri | `1.0.1` |

### VersionStrategy

Kaydın versiyon güncellemesi akış tanımlarında belirlenir. `VersionStrategy` özelliği ile:

- Her **transition** geçişinde versiyon değişimi belirlenebilir
- Her **state** girişi ve çıkışı için versiyon değişimi belirlenebilir

```json
{
  "key": "approve",
  "target": "approved",
  "versionStrategy": "Minor"
}
```

**Desteklenen Stratejiler:**
- `Major`: Major versiyon artırılır (1.0.0 → 2.0.0)
- `Minor`: Minor versiyon artırılır (1.0.0 → 1.1.0)
- `Patch`: Patch versiyon artırılır (1.0.0 → 1.0.1)

---

## ETag Kullanımı

Versiyonlama dışında kayıt değişiklikleri [ETag](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/ETag) yaklaşımı ile de yönetilir.

### ETag Özellikleri

- ETag değeri her zaman **ULID** olarak üretilir
- Her kayıt değişikliğinde yeni bir ETag oluşturulur
- Concurrent update kontrolü için kullanılır
- İstemci tarafında önbellekleme için kullanılabilir

### Kullanım Örneği

**İstek:**
```http
GET /:domain/workflows/:flow/instances/:instanceId
If-None-Match: "01ARZ3NDEKTSV4RRFFQ69G5FAV"
```

**Yanıt (değişiklik varsa):**
```http
HTTP/1.1 200 OK
ETag: "01ARZ3NDEKTSV4RRFFQ69G5FAV"
Content-Type: application/json

{ ... }
```

**Yanıt (değişiklik yoksa):**
```http
HTTP/1.1 304 Not Modified
```

---

## Geliştirme Ortamı

### Ön Kabuller

| Kural | Açıklama |
|-------|----------|
| Versiyon serbestliği | Geliştirici geliştirme ortamında istediği seviyede versiyon numarası verebilir (Major.Minor.Patch) |
| İçerik güncelleme | Versiyon güncellemesi yapmadan içerik güncellenebilir |
| Çoklu versiyon | Bir artifact runtime üstünde iki farklı versiyon hizmet verecekse **major** arttırılmış bir kopyası daha yaratılır |
| Paket dağıtımı | Local ya da remote runtime üzerine dağıtım paket ile yapılır |
| Bağımlılık yönetimi | Geliştirme sürecinde ihtiyaç duyulan referans paketler npm ile yönetilir |

### Test Kısıtlaması (vNext Runtime)

:::warning Önemli
vNext Runtime'da geliştirilen paketler **sadece runtime üzerinde test edilebilir**.
:::

- Developer, paket çıkıp local dağıtımını yapmadan test yapamaz
- Test süreci mutlaka runtime'a deployment sonrasında gerçekleşir
- Her test için paket build edilmeli, GitHub Packages'e publish edilmeli ve runtime'a deploy edilmelidir

---

## Paket Yönetimi

### Paket Deposu

Paket dağıtımları için **GitHub Packages** kullanılır.

**Paket İsimlendirme Formatı:** `vNext.<Domain Name>`

| Örnek | Açıklama |
|-------|----------|
| `vNext.Account` | Hesap domain paketi |
| `vNext.Customer` | Müşteri domain paketi |
| `vNext.Contract` | Sözleşme domain paketi |

### Paket Bağımlılık Yönetimi

- Paket bağımlılıkları `package.json` dosyasında tanımlanır
- Bağımlılık çözümlemesi npm tarafından otomatik yapılır
- Versiyon çakışmaları durumunda en yüksek uyumlu versiyon seçilir
- Paket referansları projeye çekilerek referans bütünlüğü korunur

### Paket Yayınlama Süreci

```
1. Paket Geliştirme (Local Kod Yazımı)
   ↓
2. package.json Versiyon Güncelleme
   ↓
3. Paket Build
   ↓
4. GitHub Packages'e Publish
   ↓
5. Runtime'a Dağıtım (publish/package endpoint)
   ↓
6. Runtime Paket Yükleme ve Doğrulama
   ↓
7. Runtime Üzerinde Test (Zorunlu)
   ↓
8. Test Başarılı mı?
   ├─ Hayır → Düzeltme ve Adım 2'den Devam
   └─ Evet → Paket Yayınlandı
```

---

## Runtime Deployment

### Ön Kabuller

- Runtime üzerine akışlar ve bağımlılıkları `package.json` temelli npm paket yöneticisi ile dağıtılır
- Paket dağıtımı paket parametresi alan `publish` endpoint servisi ile yapılır

### Environment Parametreleri

Runtime başlatılırken aşağıdaki environment parametreleri sağlanmalıdır:

| Parametre | Açıklama |
|-----------|----------|
| `NPM_REGISTRY_URL` | GitHub Packages registry URL'i |
| `NPM_AUTH_TOKEN` | GitHub Packages authentication token |

### Dağıtım Servisi

Dağıtım `publish/package` endpoint'i üzerinden yapılır:

```http
POST /publish/package
Content-Type: application/json

{
  "package": "vNext.Account",
  "version": "1.17.0"
}
```

---

## Versiyon Formatı

### Genişletilmiş Format

Runtime'da dağıtılan artifact'lar için genişletilmiş versiyon formatı kullanılır:

**Format:** `MAJOR.MINOR.PATCH-pkg.PKG_VERSION+PKG_NAME`

### Format Bileşenleri

| Bileşen | Açıklama | Örnek |
|---------|----------|-------|
| `MAJOR.MINOR.PATCH` | Artifact versiyon | `1.0.0` |
| `-pkg.PKG_VERSION` | Paket sürümü (SemVer sıralamasını etkiler) | `-pkg.1.17.0` |
| `+PKG_NAME` | Build metadata / Paket ismi (sıralamayı etkilemez) | `+account` |

### Versiyon Örnekleri

| Versiyon | Açıklama |
|----------|----------|
| `1.0.0-pkg.1.17.0+account` | Account paketi, core 1.0.0, paket 1.17.0 |
| `2.1.3-pkg.2.5.1+customer` | Customer paketi, core 2.1.3, paket 2.5.1 |
| `1.0.0-alpha.1-pkg.1.17.0+account` | Alpha pre-release versiyonu |
| `1.0.0-pkg.1.17.0+account+build.123` | Build metadata ile |

### Versiyon Karşılaştırma

SemVer kurallarına göre:
- `1.0.0-pkg.1.18.0 > 1.0.0-pkg.1.17.0` ✓
- `2.0.0-pkg.1.0.0 > 1.0.0-pkg.2.0.0` ✓
- Build metadata (`+`) sıralamayı etkilemez

---

## Test ve Rollback Stratejisi

### Test Stratejisi

| Kural | Açıklama |
|-------|----------|
| Runtime testi | vNext Runtime mimarisi nedeniyle tüm testler runtime üzerinde yapılır |
| Deployment gereksinimi | Her paket değişikliği için runtime'a deployment gerekir |
| Test ortamı | Test ortamı için ayrı bir runtime instance'ı kullanılması önerilir |
| Başarısız test | Test başarısız olursa rollback mekanizması ile önceki versiyona dönülebilir |
| Başarılı test | Test başarılı olursa paket production'a deploy edilebilir |

### Rollback Mekanizması

- Önceki versiyon bilgisi runtime tarafından saklanır
- Hata durumunda önceki versiyona geri dönüş yapılabilir
- Rollback işlemi `publish/package` endpoint'i üzerinden versiyon parametresi ile yapılır

```http
POST /publish/package
Content-Type: application/json

{
  "package": "vNext.Account",
  "version": "1.16.0",
  "rollback": true
}
```

### Paket Güvenlik Politikaları

| Politika | Açıklama |
|----------|----------|
| İmza doğrulama | Paket imzaları doğrulanır |
| Güvenlik taraması | Bağımlılık güvenlik açıkları kontrol edilir |
| Kaynak kontrolü | Sadece yetkili kaynaklardan paket yüklenir |

---

## İlgili Dokümantasyon

- [📄 Referans Şeması](./reference.md) - Bileşenler arası referans yönetimi
- [📄 Persistance](./persistance.md) - Veri saklama ve Dual-Write Pattern
- [📄 Workflow Tanımlaması](../flow/flow.md) - İş akışı tanımlaması ve geliştirme rehberi
