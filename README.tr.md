# VNext Runtime - Lokal Development Ortamı

[![TR](https://img.shields.io/badge/🇹🇷-Türkçe-red)](README.tr.md) [![EN](https://img.shields.io/badge/🇺🇸-English-blue)](README.md)

Bu proje, geliştiricilerin lokal ortamlarında VNext Runtime sistemini ayağa kaldırıp development yapmalarına olanak sağlamak için oluşturulmuştur. Docker tabanlı bu kurulum, tüm bağımlılıkları içerir ve hızlı bir şekilde geliştirme ortamını hazır hale getirir.

> **⚠️ Önemli Not:** Deploy versiyon yöntemi netleşene kadar her versiyon geçişinde, sistem bileşenleri lokalde varsa sıfırlanarak yeniden kurulmalıdır.

> **Languages:** This README is available in [English](README.en.md) | [Türkçe](README.md)

> **📚 vNext Platform Dokümantasyonu:** Kapsamlı teknik dokümantasyon, mimari rehberler, API referansları ve kullanım kılavuzları için yeni dokümantasyon portalımızı ziyaret edin:
> **[vNext Docs Portal](https://burgan-tech.github.io/vnext-docs/)**
>
> Portal içerisinde Technical, Architecture, Business ve Product dokümantasyon alanlarına ulaşabilirsiniz.

## Environment Konfigürasyonu

Repo, domain'e özgü konfigürasyonlar oluşturmak için kullanılan şablon dosyalarını `vnext/docker/templates/` dizininde içerir. `make create-domain` komutu ile bir domain oluşturduğunuzda, bu şablonlar işlenir ve sonuçta oluşan konfigürasyon dosyaları `vnext/docker/domains/<domain_adi>/` dizinine yerleştirilir.

**Şablon Dosyaları:**
- `.env` - Versiyonlar ve portları içeren ana environment dosyası
- `.env.orchestration` - Orchestration servis konfigürasyonu
- `.env.execution` - Execution servis konfigürasyonu
- `.env.worker-inbox` - Worker inbox servis konfigürasyonu
- `.env.worker-outbox` - Worker outbox servis konfigürasyonu
- `appsettings.*.Development.json` - Uygulama ayarları

**Amaç:** Tüm yeni domain'ler için varsayılan değerleri değiştirmek amacıyla `vnext/docker/templates/` dizinindeki şablon dosyalarını özelleştirebilir, veya bireysel domain özelleştirmesi için `vnext/docker/domains/<domain_adi>/` dizinindeki domain'e özgü dosyaları düzenleyebilirsiniz.

## 🎯 Çoklu Domain Desteği (Yeni!)

**VNext Runtime artık aynı altyapı üzerinde birden fazla domain'i eş zamanlı çalıştırmayı destekliyor.** Bu özellik, takımların aynı PostgreSQL, Redis, Vault ve Dapr servislerini paylaşarak izole domain ortamları (örn. `core`, `sales`, `hr`) çalıştırmasına olanak tanır.

### Klasör Yapısı

```
vnext/docker/
├── templates/                              # Yeni domain'ler için şablon dosyalar
│   ├── .env                                # {{PLACEHOLDER}} içeren ana env şablonu
│   ├── .env.orchestration                  # Orchestration şablonu
│   ├── .env.execution                      # Execution şablonu
│   ├── .env.worker-inbox                   # Inbox worker şablonu
│   ├── .env.worker-outbox                  # Outbox worker şablonu
│   └── appsettings.*.Development.json      # App settings şablonları
├── domains/                                # Domain konfigürasyonları (her domain için)
│   ├── core/                               # Domain: core
│   ├── sales/                              # Domain: sales
│   └── <domain_adi>/                       # Sizin domain'iniz
│       ├── .env
│       ├── .env.orchestration
│       ├── .env.execution
│       ├── .env.worker-inbox
│       ├── .env.worker-outbox
│       └── appsettings.*.Development.json
├── config/                                 # Paylaşılan altyapı konfigürasyonu
├── docker-compose.yml
└── create-domain.sh                        # Şablonlardan domain oluşturma scripti
```

### Yeni Domain Oluşturma

Şablonlardan domain konfigürasyonu oluşturmak için `create-domain` komutunu kullanın:

```bash
# Port çakışmasını önlemek için port offset ile domain oluştur
# Not: core (offset 0) ve discovery (offset 5) önceden yapılandırılmıştır
# Özel domain'ler için offset 10 veya üstünü kullanın
make create-domain DOMAIN=sales PORT_OFFSET=10
make create-domain DOMAIN=hr PORT_OFFSET=20
make create-domain DOMAIN=finance PORT_OFFSET=30
```

Bu komut şunları oluşturur:
- Domain'e özgü DAPR store adları, portlar ve app ID'leri içeren environment dosyaları
- Domain'e özgü veritabanı connection string'leri içeren appsettings dosyaları
- Observability için uygun OTEL servis adları

Veritabanı adı, domain adınızdan otomatik olarak oluşturulur:
- `core` → `vNext_Core`
- `sales` → `vNext_Sales`
- `kullanici-yonetimi` → `vNext_Kullanici_Yonetimi`

### Port Tahsisi

Her domain, `PORT_OFFSET` değerine göre benzersiz portlar kullanır:

| Offset | Domain (Örnek) | App Port | Execution | Inbox | Outbox | Init |
|--------|----------------|----------|-----------|-------|--------|------|
| 0      | core (rezerve) | 4201     | 4202      | 4203  | 4204   | 3005 |
| 5      | discovery (rezerve) | 4206 | 4207      | 4208  | 4209   | 3010 |
| 10     | sales          | 4211     | 4212      | 4213  | 4214   | 3015 |
| 20     | hr             | 4221     | 4222      | 4223  | 4224   | 3025 |
| 30     | finance        | 4231     | 4232      | 4233  | 4234   | 3035 |

Dapr portları çakışmayı önlemek için `offset * 100` kullanır.

> **⚠️ Rezerve Edilmiş Offset'ler:** `core` ve `discovery` domain'leri, sırasıyla **0** ve **5** offset'leri ile önceden yapılandırılmış domain'ler olarak sunulmaktadır. Özel domain'leriniz için **bu offset'leri kullanmayınız**. Yeni domain'ler için 10 veya daha yüksek offset değerleri ile başlayınız.

### Birden Fazla Domain Çalıştırma

```bash
# 1. Paylaşılan altyapıyı başlat
make up-infra

# 2. Önceden yapılandırılmış domain'ler (core ve discovery) kullanıma hazır
make up-vnext DOMAIN=core
make up-vnext DOMAIN=discovery

# 3. Kendi domain'inizi oluşturun ve başlatın (offset 10 veya üstü kullanın)
make create-domain DOMAIN=sales PORT_OFFSET=10
make db-create DOMAIN=sales
make up-vnext DOMAIN=sales

# 4. Tüm çalışan servisleri görüntüle
make status-all-domains

# 5. Belirli bir domain'in sağlık durumunu kontrol et
make health DOMAIN=core
make health DOMAIN=sales
```

### Domain Yönetimi

```bash
# Tüm yapılandırılmış domain'leri listele
make list-domains

# Belirli bir domain'i durdur
make down-vnext DOMAIN=sales

# Belirli bir domain'i yeniden başlat
make restart-vnext DOMAIN=sales

# Tüm domain'leri durdur ama altyapıyı çalışır tut
make down-all-vnext

# Belirli bir domain'in loglarını görüntüle
make logs-vnext DOMAIN=core
```

### Şablonları Özelleştirme

Şablonlar `vnext/docker/templates/` dizininde bulunur. Varsayılan değerleri değiştirmek için bunları özelleştirebilirsiniz. Şablonlar `{{PLACEHOLDER}}` sözdizimini kullanır:

| Placeholder | Açıklama |
|-------------|----------|
| `{{DOMAIN_NAME}}` | Domain adı (örn. `core`, `sales`) |
| `{{PORT_OFFSET}}` | Port offset değeri |
| `{{DB_NAME}}` | Veritabanı adı (örn. `vNext_Core`) - appsettings connection string'lerinde kullanılır |
| `{{VNEXT_APP_PORT}}` | Orchestration portu |
| `{{DAPR_*_PORT}}` | Dapr sidecar portları |

**Önemli Environment Değişkenleri (otomatik oluşturulur):**

| Değişken | Açıklama |
|----------|----------|
| `DAPR_STATE_STORE_NAME` | Dapr state store adı (zorunlu) |
| `DAPR_SECRET_STORE_NAME` | Dapr secret store adı |
| `DAPR_PUBSUB_STORE_NAME` | Dapr pubsub store adı |
| `DAPR_APP_ID` | Her servis için benzersiz Dapr app tanımlayıcısı |
| `OTEL_SERVICE_NAME` | OpenTelemetry servis adı |

### Eski Tekli Domain Modu

Geriye uyumluluk için eski `change-domain` komutu hala kullanılabilir:

```bash
make change-domain DOMAIN=sirketim
```

Bu komut tüm domain ile ilgili ayarları günceller ancak birden fazla domain'i eş zamanlı çalıştırmayı desteklemez.

## Hızlı Başlangıç

### Makefile ile Kolay Kurulum (Önerilen)

Projede bulunan Makefile, geliştiriciler için en konforlu çalıştırma ortamını sağlar. Sistem environment dosyalarını kontrol eder ve development ortamını tek komutla başlatır:

```bash
# Environment dosyalarını kontrol et ve development ortamını başlat
make dev

# Yardım menüsünü görüntüle
make help

# Network kurulumu ve environment kontrolü
make setup
```

### `make dev` Ne Yapar?

`make dev` çalıştırdığınızda otomatik olarak şunlar gerçekleşir:

1. ✅ **Environment Kurulumu** - `.env` dosyaları ve Docker network oluşturulur
2. ✅ **PostgreSQL** başlar → `vNext_WorkflowDb` veritabanı otomatik oluşturulur
3. ✅ **vnext-app** başlar → postgres healthy olduktan sonra
4. ✅ **vnext-init** başlar → vnext-app healthy olduktan sonra
5. ✅ **vnext-component-publisher** çalışır → vnext-init healthy olduktan sonra component'leri otomatik publish eder
6. ✅ Diğer tüm servisler başlar

Bu sayede tek bir komutla:
- Veritabanı şema ile hazır
- Component'ler yüklü
- Tüm altyapı çalışır durumda

### Manuel Kurulum

Eğer Makefile kullanmak istemiyorsanız, manual olarak kurabilirsiniz:

#### 1. Environment Dosyalarını Kontrol Edin

`.env`, `.env.orchestration` ve `.env.execution` dosyalarının `vnext/docker/` dizininde mevcut olduğundan emin olun ve gerektiğinde özelleştirin.

#### 2. Docker Network Oluşturun

```bash
docker network create vnext-development
```

#### 3. Servisleri Başlatın

```bash
# vnext/docker dizinine geç
cd vnext/docker

# Tüm servisleri arka planda başlat
docker-compose up -d

# Logları takip etmek için
docker-compose logs -f vnext-app

# Belirli bir servisi yeniden başlatmak için
docker-compose restart vnext-app
```

#### 4. Sistem Durumunu Kontrol Edin

```bash
# Çalışan servislerin durumunu görüntüle
docker-compose ps

# vnext-app sağlık kontrolü
curl http://localhost:4201/health
```

## 🚀 vNext Geliştirmeye Başlangıç

vNext Runtime için workflow ve bileşenler geliştirmek amacıyla aşağıdaki araçlara ihtiyacınız olacak:

> Detaylı başlangıç rehberi ve platform dokümantasyonu için [vNext Docs Portal](https://burgan-tech.github.io/vnext-docs/) adresini ziyaret edin.

### 1. vNext Template

**Repository:** https://github.com/burgan-tech/vnext-template

Domain tabanlı mimariye sahip vNext workflow bileşenleri için yapılandırılmış bir şablon paketi. Bu şablon, yerleşik doğrulama ve build yetenekleriyle eksiksiz bir proje yapısı oluşturur.

**Kurulum & Kullanım:**

```bash
# Domain adınızla yeni bir vNext projesi oluşturun
npx @burgan-tech/vnext-template DOMAIN_ADINIZ

# Örnek
npx @burgan-tech/vnext-template kullanici-yonetimi
```

Bu komut, aşağıdaki yapıyı içeren domain adınızla yeni bir dizin oluşturacaktır:

```
DOMAIN_ADINIZ/
├── Extensions/    # Custom extension tanımları
├── Functions/     # Custom function tanımları
├── Schemas/       # JSON schema tanımları
├── Tasks/         # Task tanımları
├── Views/         # View bileşenleri
└── Workflows/     # Workflow tanımları
```

**Kullanılabilir Script'ler:**

| Script | Açıklama |
|--------|----------|
| `npm run validate` | Proje yapısını ve şemaları doğrula |
| `npm run build` | Runtime paketini dist/ dizinine build et |
| `npm run build:runtime` | Runtime paketini açıkça build et |
| `npm run build:reference` | Sadece export'larla referans paketi build et |

**Belirli Versiyon Kurulumu:**

```bash
npx @burgan-tech/vnext-template@<versiyon> DOMAIN_ADINIZ
```

Detaylı dokümantasyon için [vnext-template repository'sini](https://github.com/burgan-tech/vnext-template) ziyaret edin.

### 2. vNext Flow Studio

**Repository:** https://github.com/burgan-tech/vnext-flow-studio

Görsel workflow tasarımı ve yönetimi için güçlü bir Visual Studio Code uzantısı.

**Özellikler:**
- 🎨 Görsel workflow tasarım arayüzü
- 📦 Workflow'ları ve bileşenleri görsel olarak yönetin
- 🚀 VS Code'dan doğrudan deploy edin
- 🔍 IntelliSense ve doğrulama desteği

**Kurulum:**
1. VS Code'u açın
2. Extensions'da "vNext Flow Studio" araması yapın
3. Kurun ve workflow'larınızı görsel olarak tasarlamaya başlayın

Detaylı kullanım talimatları için [vnext-flow-studio repository'sini](https://github.com/burgan-tech/vnext-flow-studio) ziyaret edin.

### 3. vNext Schema

**Repository:** https://github.com/burgan-tech/vnext-schema

Tüm desteklenen vNext bileşenleri (workflow'lar, görevler, fonksiyonlar, vb.) için JSON şemalarını içerir.

**Amaç:**
- 📚 Mevcut bileşenler ve özellikleri hakkında bilgi edinin
- 🤖 Şema doğrulama için AI araçları ile entegre edin
- ✅ Workflow'larınızın platform standartlarına uygun olduğundan emin olun

Bileşen yapılarını ve doğrulama kurallarını anlamak için [vnext-schema repository'sine](https://github.com/burgan-tech/vnext-schema) başvurun.

### 4. vNext Example

**Repository:** https://github.com/burgan-tech/vnext-example

vNext Platformu için referans workflow'lar ve bileşenler içeren örnek proje. Önerilen pattern'ları, bileşen kompozisyon tekniklerini ve uçtan uca akışları göstererek geliştiricilere gerçek dünya implementasyonları için rehberlik eder.

**İçerik:**
- Örnek workflow tanımları ve iş akışları
- Schema, Task, View, Function ve Extension bileşen örnekleri
- API test koleksiyonları (Postman)
- JMeter ile yük testleri
- Domain yapılandırma örnekleri (`vnext.config.json`)

**Hızlı Başlangıç:**

```bash
git clone https://github.com/burgan-tech/vnext-example.git
cd vnext-example
npm install
npm run validate
```

Detaylı bilgi için [vnext-example repository'sini](https://github.com/burgan-tech/vnext-example) ziyaret edin.

---

## VNext Core Runtime Initialization

`vnext-init` servisi, vnext-app servisi healthy olduktan sonra otomatik olarak çalışır ve aşağıdaki işlemleri gerçekleştirir:

1. `@burgan-tech/vnext-core-runtime` npm paketini indirir (versiyon `.env` dosyasından kontrol edilir)
2. Paket içindeki core klasöründen sistem bileşenlerini okur:
   - Extensions (Uzantılar)
   - Functions (Fonksiyonlar)
   - Schemas (Şemalar)
   - Tasks (Görevler)
   - Views (Görünümler)
   - Workflows (İş Akışları)
3. **🆕 Domain Değiştirme**: JSON dosyalarındaki tüm `"domain"` property değerlerini `APP_DOMAIN` environment variable değeri ile değiştirir
   - Bu sayede her geliştirici kendi domain'inde lokal ortamda çalışabilir
   - Varsayılan domain `"core"`'dur, ancak `.env` dosyasında `APP_DOMAIN=mydomain` ile özelleştirilebilir

## Veritabanı Yönetimi (Domain-Spesifik)

Her domain kendi veritabanını gerektirir. Veritabanı adları domain adından otomatik olarak oluşturulur:
- `core` → `vNext_Core`
- `sales` → `vNext_Sales`
- `morph-idm` → `vNext_Morph_idm`

### Veritabanı Komutları

```bash
# Veritabanı durumunu kontrol et (tüm veritabanlarını listeler)
make db-status

# Sadece vNext veritabanlarını listele
make db-list

# Domain için veritabanı oluştur
make db-create DOMAIN=core
make db-create DOMAIN=sales

# Domain veritabanını sil (DİKKAT: yıkıcı!)
make db-drop DOMAIN=core

# Veritabanını sıfırla (sil ve yeniden oluştur)
make db-reset DOMAIN=core

# psql ile domain veritabanına bağlan
make db-connect DOMAIN=core
```

## Otomatik Component Publishing

`vnext-component-publisher` servisi, `vnext-init` healthy olduktan sonra otomatik olarak çalışır:

1. vnext-init'in hazır olmasını bekler
2. Yapılandırılmış versiyon ve domain ile component'leri publish eder
3. Tamamlar ve çıkar

Component'leri manuel olarak yeniden publish etmek için:

```bash
# Component publisher'ı yeniden çalıştır
make republish-component

# Veya doğrudan script'i kullan
make publish-component
```

## Instance Filtreleme

VNext Runtime, workflow instance'larını JSON attribute'larına göre sorgulama için güçlü filtreleme yetenekleri sağlar. Bu özellik, basit API çağrıları ile çeşitli operatörler kullanarak instance'ları arama ve filtreleme yapmanıza olanak tanır.

### Temel Kullanım

HTTP isteklerinizde query parametreleri kullanarak instance'ları filtreleyin:

```bash
# clientId "122" ye eşit olan instance'ları bul
curl -X GET "http://localhost:4201/api/v1.0/{domain}/workflows/{workflow}/instances?filter=attributes=clientId=eq:122"

# testValue 2'den büyük olan instance'ları bul
curl -X GET "http://localhost:4201/api/v1.0/{domain}/workflows/{workflow}/instances?filter=attributes=testValue=gt:2"

# status "completed" olmayan instance'ları bul
curl -X GET "http://localhost:4201/api/v1.0/{domain}/workflows/{workflow}/instances?filter=attributes=status=ne:completed"
```

### Filtre Syntax'ı

Filtreleme şu formatı kullanır: `filter=attributes={field}={operator}:{value}`

#### Kullanılabilir Operatörler

| Operatör | Açıklama | Örnek |
|----------|----------|-------|
| `eq` | Eşittir | `filter=attributes=clientId=eq:122` |
| `ne` | Eşit değildir | `filter=attributes=status=ne:inactive` |
| `gt` | Büyüktür | `filter=attributes=amount=gt:100` |
| `ge` | Büyük eşittir | `filter=attributes=score=ge:80` |
| `lt` | Küçüktür | `filter=attributes=count=lt:10` |
| `le` | Küçük eşittir | `filter=attributes=age=le:65` |
| `between` | İki değer arasında | `filter=attributes=amount=between:50,200` |
| `like` | Alt string içerir | `filter=attributes=name=like:ahmet` |
| `startswith` | İle başlar | `filter=attributes=email=startswith:test` |
| `endswith` | İle biter | `filter=attributes=email=endswith:.com` |
| `in` | Liste içinde | `filter=attributes=status=in:active,pending` |
| `nin` | Liste içinde değil | `filter=attributes=type=nin:test,debug` |

### Pratik Örnekler

#### Tek Filtre Örnekleri

```bash
# Tüm aktif siparişleri bul
curl "http://localhost:4201/api/v1.0/ecommerce/workflows/order-processing/instances?filter=attributes=status=eq:active"

# Yüksek değerli işlemleri bul
curl "http://localhost:4201/api/v1.0/finance/workflows/payment/instances?filter=attributes=amount=gt:1000"

# Son siparişleri bul (timestamp field olduğu varsayılarak)
curl "http://localhost:4201/api/v1.0/ecommerce/workflows/order-processing/instances?filter=attributes=createdDate=ge:2024-01-01"

# Müşteri email domain'ine göre ara
curl "http://localhost:4201/api/v1.0/ecommerce/workflows/customer/instances?filter=attributes=email=endswith:@company.com"
```

#### Çoklu Filtre Örnekleri

```bash
# Birden fazla filtreyi birleştir (VE mantığı)
curl "http://localhost:4201/api/v1.0/ecommerce/workflows/order-processing/instances?filter=attributes=status=eq:pending&filter=attributes=priority=eq:high"

# Fiyat aralığında siparişleri bul
curl "http://localhost:4201/api/v1.0/ecommerce/workflows/order-processing/instances?filter=attributes=totalAmount=between:100,500"

# Belirli müşteri tiplerini bul
curl "http://localhost:4201/api/v1.0/crm/workflows/customer/instances?filter=attributes=customerType=in:premium,vip"
```

### Örnek Instance Verisi

Workflow instance'ları ile çalışırken şuna benzer JSON verileriniz olabilir:

```json
{
  "clientId": "122",
  "testValue": 4,
  "status": "active",
  "email": "musteri@example.com",
  "amount": 150.50,
  "priority": "high",
  "tags": ["vip", "premium"]
}
```

### cURL ile Filtre Testi

```bash
# Temel eşitlik filtresini test et
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=clientId=eq:122"

# Sayısal karşılaştırmayı test et
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=amount=gt:100"

# String operasyonlarını test et
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=email=endswith:.com"

# Çoklu filtreleri test et
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=status=eq:active&filter=attributes=priority=eq:high"
```

### Filtreler ile Sayfalama

```bash
# Sayfalama ile filtreleme
curl "http://localhost:4201/api/v1.0/ecommerce/workflows/order-processing/instances?filter=attributes=status=eq:active&page=1&pageSize=10"

# Büyük veri setlerini sayfalama ile filtreleme
curl "http://localhost:4201/api/v1.0/analytics/workflows/events/instances?filter=attributes=eventType=eq:purchase&page=1&pageSize=50"
```

### Response Formatı

Filtrelenmiş sonuçlar standart formatta döner:

```json
{
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "flow": "order-processing",
      "flowVersion": "1.0.0",
      "domain": "ecommerce",
      "key": "ORDER-2024-001",
      "attributes": {
        "clientId": "122",
        "amount": 150.50,
        "status": "active"
      },
      "etag": "abc123def456"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3
  }
}
```

### Yaygın Kullanım Senaryoları

1. **Müşteri Hizmetleri**: Belirli bir müşterinin tüm siparişlerini bulma
2. **Finansal Raporlama**: İşlemleri tutar aralıklarına göre filtreleme
3. **Sipariş Yönetimi**: Bekleyen veya başarısız siparişleri bulma
4. **Kullanıcı Analitiği**: Kullanıcıları kayıt tarihi veya aktiviteye göre filtreleme
5. **Hata Takibi**: Hata durumundaki instance'ları bulma

### Test için cURL Örnekleri

Filtreleme yeteneklerini test etmek için bu cURL komutlarını kullanabilirsiniz:

```bash
# Temel eşitlik filtresini test et
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=clientId=eq:122"

# Sayısal karşılaştırmayı test et
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=testValue=gt:2"

# String operasyonlarını test et
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=status=startswith:act"

# Çoklu filtreleri test et
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=status=eq:active&filter=attributes=priority=ne:low"

# Aralık filtrelemesini test et
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=amount=between:100,500"
```

Bu filtreleme sistemi, production iş yüklerine optimize edilmiş yüksek performanslı sorgulama yetenekleri sağlar ve iş verilerine dayalı spesifik workflow instance'larını bulmayı kolaylaştırır.

## Makefile Komutları

Proje kök dizininde bulunan Makefile, development sürecini kolaylaştıran birçok komut içerir. Tüm komutları görmek için:

```bash
make help
```

### Temel Komutlar

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make help` | Tüm kullanılabilir komutları listeler | `make help` |
| `make dev` | Development ortamını kurar ve başlatır | `make dev` |
| `make setup` | Environment dosyalarını kontrol eder ve network'ü oluşturur | `make setup` |
| `make info` | Proje bilgilerini ve erişim URL'lerini gösterir | `make info` |

### Çoklu Domain Yönetimi

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make create-domain` | Şablonlardan domain oluştur | `make create-domain DOMAIN=sirketim PORT_OFFSET=10` |
| `make list-domains` | Tüm yapılandırılmış domain'leri listele | `make list-domains` |
| `make up-vnext` | Bir domain için vnext servislerini başlat | `make up-vnext DOMAIN=sirketim` |
| `make down-vnext` | Bir domain için vnext servislerini durdur | `make down-vnext DOMAIN=sirketim` |
| `make restart-vnext` | Bir domain için vnext servislerini yeniden başlat | `make restart-vnext DOMAIN=sirketim` |
| `make status-vnext` | Bir domain'in durumunu göster | `make status-vnext DOMAIN=sirketim` |
| `make logs-vnext` | Bir domain'in loglarını göster | `make logs-vnext DOMAIN=sirketim` |
| `make status-all-domains` | Tüm çalışan vnext servislerini göster | `make status-all-domains` |
| `make down-all-vnext` | Tüm domain servislerini durdur (altyapıyı tut) | `make down-all-vnext` |
| `make health` | Sağlık kontrolü (opsiyonel DOMAIN ile) | `make health DOMAIN=sirketim` |

### Altyapı Yönetimi

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make up-infra` | Sadece altyapı servislerini başlat | `make up-infra` |
| `make down-infra` | Sadece altyapı servislerini durdur | `make down-infra` |
| `make status-infra` | Altyapı durumunu göster | `make status-infra` |
| `make logs-infra` | Altyapı loglarını göster | `make logs-infra` |

### Eski Domain Konfigürasyonu

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make change-domain` | Domain değiştir (eski tekli-domain modu) | `make change-domain DOMAIN=sirketim` |

### Environment Setup

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make check-env` | Environment dosyalarının varlığını kontrol eder | `make check-env` |
| `make create-network` | Docker network'ünü oluşturur | `make create-network` |

### Docker Operations

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make up` | Servisleri başlatır | `make up` |
| `make up-build` | Servisleri build ederek başlatır | `make up-build` |
| `make down` | Servisleri durdurur | `make down` |
| `make restart` | Servisleri yeniden başlatır | `make restart` |
| `make build` | Docker image'larını build eder | `make build` |

### Service Management

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make status` | Servislerin durumunu gösterir | `make status` |
| `make health` | Servislerin sağlık durumunu kontrol eder | `make health` |
| `make logs` | Tüm servislerin loglarını gösterir | `make logs` |
| `make logs-orchestration` | Sadece orchestration servis logları | `make logs-orchestration` |
| `make logs-execution` | Sadece execution servis logları | `make logs-execution` |
| `make logs-init` | Init servis logları | `make logs-init` |
| `make logs-dapr` | DAPR servislerin logları | `make logs-dapr` |
| `make logs-db` | Database servislerin logları | `make logs-db` |

### Veritabanı İşlemleri (Domain-Spesifik)

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make db-status` | Veritabanı durumunu ve tüm veritabanlarını listeler | `make db-status` |
| `make db-list` | Sadece vNext veritabanlarını listeler | `make db-list` |
| `make db-create` | Domain için veritabanı oluşturur | `make db-create DOMAIN=core` |
| `make db-drop` | Domain veritabanını siler (yıkıcı!) | `make db-drop DOMAIN=core` |
| `make db-reset` | Domain veritabanını silip yeniden oluşturur | `make db-reset DOMAIN=core` |
| `make db-connect` | psql ile domain veritabanına bağlanır | `make db-connect DOMAIN=core` |

### Development Tools

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make shell-orchestration` | Orchestration container'ına shell açar | `make shell-orchestration` |
| `make shell-execution` | Execution container'ına shell açar | `make shell-execution` |
| `make shell-postgres` | PostgreSQL shell açar | `make shell-postgres` |
| `make shell-redis` | Redis CLI açar | `make shell-redis` |

### Monitoring

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make ps` | Çalışan container'ları listeler | `make ps` |
| `make top` | Container resource kullanımını gösterir | `make top` |
| `make stats` | Container istatistiklerini gösterir | `make stats` |

### Custom Components

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make publish-component` | Component paketi publish eder | `make publish-component` |
| `make republish-component` | Component publisher container'ını yeniden çalıştırır | `make republish-component` |

### Maintenance

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make clean` | Durdurulmuş container'ları ve kullanılmayan network'leri temizler | `make clean` |
| `make clean-all` | ⚠️ TÜM domain'leri, altyapıyı, image ve volume'leri siler | `make clean-all` |
| `make reset` | Environment'ı resetler (stop, clean, setup) | `make reset` |
| `make update` | Latest image'ları çeker ve servisleri yeniden başlatır | `make update` |

### Yaygın Kullanım Senaryoları

```bash
# İlk kez projeyi çalıştırma
make dev

# Sadece logları takip etme
make logs-orchestration

# Servis durumunu kontrol etme
make status
make health

# Veritabanı işlemleri
make db-status
make db-reset

# Development sırasında yeniden başlatma
make restart

# Custom component ekledikten sonra yeniden yükleme
make reload-components

# Component'leri yeniden publish etme
make republish-component

# Temizlik ve yeniden kurulum
make reset
make dev

# Container'lara erişim
make shell-orchestration
make shell-postgres
```

## Servisler ve Portlar

### Altyapı Servisleri (Paylaşılan)

| Servis | Açıklama | Port | Erişim URL |
|--------|----------|------|------------|
| **dapr-placement** | Dapr placement servisi | 50005 | - |
| **dapr-scheduler** | Dapr scheduler servisi | 50007 | - |
| **vnext-redis** | Redis cache | 6379 | - |
| **vnext-postgres** | PostgreSQL veritabanı | 5432 | - |
| **vnext-vault** | HashiCorp Vault | 8200 | http://localhost:8200 |
| **openobserve** | Observability dashboard | 5080 | http://localhost:5080 |
| **otel-collector** | OpenTelemetry Collector | 4317, 4318, 8888 | - |
| **mockoon** | API Mock Server | 3001 | http://localhost:3001 |

### VNext Domain Servisleri (Her Domain İçin)

Portlar `PORT_OFFSET` değerine göre değişir. Varsayılan (offset 0):

| Servis | Açıklama | Port | Container Adı |
|--------|----------|------|---------------|
| **vnext-app** | Orchestration uygulaması | 4201 | vnext-app-{domain} |
| **vnext-execution-app** | Execution servisi | 4202 | vnext-execution-app-{domain} |
| **vnext-worker-inbox** | Worker inbox servisi | 4203 | vnext-worker-inbox-{domain} |
| **vnext-worker-outbox** | Worker outbox servisi | 4204 | vnext-worker-outbox-{domain} |
| **vnext-init** | Init container | 3005 | vnext-init-{domain} |
| **vnext-orchestration-dapr** | Orchestration için Dapr sidecar | 42110/42111 | vnext-orchestration-dapr-{domain} |
| **vnext-execution-dapr** | Execution için Dapr sidecar | 43110/43111 | vnext-execution-dapr-{domain} |

`PORT_OFFSET=10` olan domain'ler için portlar 4211, 4212, 4213, 4214, 3015 vb. olur.

## Management Tools

| Tool | URL | Kullanıcı Adı | Şifre |
|------|-----|---------------|-------|
| **OpenObserve** | http://localhost:5080 | root@example.com | Complexpass#@123 |
| **Vault UI** | http://localhost:8200 | - | admin (token) |

## Development İpuçları

### Environment Variable'ları Customize Etme

Environment dosyalarını özelleştirmek için:

```bash
# Mevcut environment dosyalarını kontrol et
make check-env

# vnext/docker/ dizinindeki .env dosyalarını gerektiğinde düzenleyin
```

Önemli konfigürasyonlar:

1. **Veritabanı bağlantısını değiştirmek**:
   ```bash
   # vnext/docker/.env.orchestration dosyasında
   ConnectionStrings__Default=Host=my-postgres;Port=5432;Database=MyWorkflowDb;Username=myuser;Password=mypass;
   ```

2. **Redis ayarlarını değiştirmek**:
   ```bash
   # vnext/docker/.env.orchestration dosyasında
   Redis__Standalone__EndPoints__0=my-redis:6379
   Redis__Password=myredispassword
   ```

3. **Log seviyesini değiştirmek**:
   ```bash
   # vnext/docker/.env.orchestration dosyasında
   Logging__LogLevel__Default=Debug
   Telemetry__Logging__MinimumLevel=Debug
   ```

### Debugging

Makefile komutları ile:

```bash
# Tüm servislerin loglarını görüntüle
make logs

# Specific servis logları
make logs-orchestration
make logs-execution
make logs-init

# Servis durumlarını kontrol et
make status
make health

# Container'lara erişim
make shell-orchestration
make shell-postgres
make shell-redis
```

Manuel komutlar:

```bash
# vnext/docker dizininden
cd vnext/docker

# Docker compose komutları
docker-compose logs -f vnext-app
docker-compose exec vnext-app sh
docker-compose ps
```

### Yaygın Sorunlar ve Çözümleri

1. **Port çakışması**: 
   ```bash
   # Makefile ile reset
   make reset
   # .env dosyalarında port numaralarını değiştirin
   ```

2. **Memory yetersizliği**: 
   - Docker Desktop'ta memory limitini artırın (min 4GB önerilir)
   - Container resource kullanımını kontrol edin: `make stats`

3. **Environment dosyaları eksik**:
   ```bash
   # Environment kontrolü
   make check-env
   # Dosyaların vnext/docker/ dizininde mevcut olduğundan emin olun
   ```

4. **Veritabanı oluşturulmadı**:
   ```bash
   # Veritabanı durumunu kontrol et
   make db-status
   # Domain için veritabanı oluştur
   make db-create DOMAIN=core
   ```

### Performance Tuning

```bash
# .env.orchestration dosyasında
TaskFactory__UseObjectPooling=true
TaskFactory__MaxPoolSize=100
Redis__ConnectionTimeout=3000
```

Development workflow önerileri:

```bash
# Günlük development rutini
make dev              # İlk başlatma
make logs-orchestration  # Log takibi
make restart          # Değişiklik sonrası restart
make health          # Sağlık kontrolü

# Haftalık temizlik
make clean           # Hafif temizlik
make reset           # Derin reset (gerekirse)
```

## 📚 Dokümantasyon

vNext platformu için kapsamlı dokümantasyon artık **vNext Docs Portal** üzerinden sunulmaktadır:

**🌐 [vNext Docs Portal](https://burgan-tech.github.io/vnext-docs/)**

Portal üzerinde aşağıdaki dokümantasyon alanlarına ulaşabilirsiniz:

- **Technical** -- Lokal geliştirme ortamı kurulumu, çekirdek kavramlar, bileşenler, servisler ve API referansı
- **Architecture** -- Domain modeli, runtime mimarisi, veri katmanı, altyapı ve mimari karar kayıtları (ADR)
- **Business** -- Manifesto, yetenekler, kullanım senaryoları ve değer önerisi
- **Product** -- Ürün vizyonu, özellik kataloğu, yol haritası, persona ve sürüm stratejisi