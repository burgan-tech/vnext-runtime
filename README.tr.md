# VNext Runtime - Lokal Development Ortamı

[![TR](https://img.shields.io/badge/🇹🇷-Türkçe-red)](README.tr.md) [![EN](https://img.shields.io/badge/🇺🇸-English-blue)](README.md)

Bu proje, geliştiricilerin lokal ortamlarında VNext Runtime sistemini ayağa kaldırıp development yapmalarına olanak sağlamak için oluşturulmuştur. Docker tabanlı bu kurulum, tüm bağımlılıkları içerir ve hızlı bir şekilde geliştirme ortamını hazır hale getirir.

> **⚠️ Önemli Not:** Deploy versiyon yöntemi netleşene kadar her versiyon geçişinde, sistem bileşenleri lokalde varsa sıfırlanarak yeniden kurulmalıdır.

> **Languages:** This README is available in [English](README.en.md) | [Türkçe](README.md)

## Environment Konfigürasyonu

Repo, `vnext/docker/` dizininde hazır environment dosyaları (`.env`, `.env.orchestration`, `.env.execution`) içerir. Bu dosyalar sistem versiyonlarını, veritabanı bağlantılarını, Redis konfigürasyonunu, telemetry ayarlarını ve diğer runtime parametrelerini kontrol eder.

**Amaç:** Bu environment dosyalarını altyapınıza ve geliştirme ihtiyaçlarınıza göre özelleştirebilirsiniz. Tüm kullanılabilir environment variable'ları ve varsayılan değerlerini repository içindeki ilgili dosyalardan inceleyebilirsiniz.

## 🎯 Domain Konfigürasyonu (Önemli!)

**Domain konfigürasyonu, vNext Runtime'da kritik bir kavramdır.** Her geliştiricinin platform ile çalışabilmesi için kendi domain'ini yapılandırması gerekir. Domain'inizi ayarlamak için aşağıdaki dosyalardaki `APP_DOMAIN` değerini güncellemelisiniz:

1. **`vnext/docker/.env`** - Runtime domain konfigürasyonu
2. **`vnext/docker/.env.orchestration`** - Orchestration servis domain'i
3. **`vnext/docker/.env.execution`** - Execution servis domain'i
4. **`vnext.config.json`** - Proje domain konfigürasyonu (kendi workflow repository'nizde)

```bash
# Örnek: Varsayılan "core" değerini kendi domain'inize değiştirin
APP_DOMAIN=sirketim
```

Bu, tüm workflow bileşenlerinin, görevlerin ve sistem kaynaklarının doğru şekilde kendi domain namespace'inize atanmasını sağlar.

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

## Otomatik Veritabanı Başlatma

Docker Compose başladığında, PostgreSQL init script kullanarak `vNext_WorkflowDb` veritabanını otomatik olarak oluşturur. Bu sayede:

- Herhangi bir servis bağlanmaya çalışmadan önce veritabanı hazır olur
- Postgres'e bağımlı servisler, veritabanı healthy olana kadar bekler
- Manuel veritabanı oluşturma gerekmez

### Veritabanı Komutları

```bash
# Veritabanı durumunu kontrol et
make db-status

# Manuel olarak veritabanı oluştur (gerekirse)
make db-create

# Veritabanını sil ve yeniden oluştur
make db-reset

# psql ile veritabanına bağlan
make db-connect
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

### Database Operations

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make db-status` | Veritabanı durumunu ve listesini gösterir | `make db-status` |
| `make db-create` | vNext veritabanını oluşturur | `make db-create` |
| `make db-drop` | vNext veritabanını siler (yıkıcı!) | `make db-drop` |
| `make db-reset` | Veritabanını silip yeniden oluşturur | `make db-reset` |
| `make db-connect` | psql ile veritabanına bağlanır | `make db-connect` |

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
| `make clean-all` | ⚠️ TÜM container, image ve volume'leri siler | `make clean-all` |
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

| Servis | Açıklama | Port | Erişim URL |
|--------|----------|------|------------|
| **vnext-app** | Ana orchestration uygulaması | 4201 | http://localhost:4201 |
| **vnext-execution-app** | Execution servis uygulaması | 4202 | http://localhost:4202 |
| **vnext-init** | Sistem component'lerini yükleyen init container | - | - |
| **vnext-component-publisher** | Init sonrası component'leri publish eder | - | - |
| **vnext-orchestration-dapr** | Orchestration servisi için Dapr sidecar | 42110/42111 | - |
| **vnext-execution-dapr** | Execution servisi için Dapr sidecar | 43110/43111 | - |
| **dapr-placement** | Dapr placement servisi | 50005 | - |
| **dapr-scheduler** | Dapr scheduler servisi | 50007 | - |
| **vnext-redis** | Redis cache | 6379 | - |
| **vnext-postgres** | PostgreSQL veritabanı | 5432 | - |
| **vnext-vault** | HashiCorp Vault | 8200 | http://localhost:8200 |
| **openobserve** | Observability dashboard | 5080 | http://localhost:5080 |
| **otel-collector** | OpenTelemetry Collector | 4317, 4318, 8888 | - |
| **mockoon** | API Mock Server | 3001 | http://localhost:3001 |

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
   # Gerekirse manuel oluştur
   make db-create
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

VNext Runtime platformu, iş akışları ve geliştirme rehberleri hakkında kapsamlı dokümantasyon için lütfen şu kaynaklara başvurun:

- **📖 [Kapsamlı Dokümantasyon (Türkçe)](doc/tr/README.md)** - Platform mimarisi, iş akışı bileşenleri ve detaylı API referansları içeren kapsamlı geliştirici rehberi
- **🇺🇸 [English Documentation](doc/en/README.md)** - Comprehensive developer guide covering platform architecture, workflow components, and detailed API references

### Hızlı Dokümantasyon Linkleri

| Konu | Türkçe | İngilizce |
|------|--------|-----------|
| **Platform Temelleri** | [fundamentals/readme.md](doc/tr/fundamentals/readme.md) | [fundamentals/readme.md](doc/en/fundamentals/readme.md) |
| **İş Akışı Durumları** | [flow/state.md](doc/tr/flow/state.md) | [flow/state.md](doc/en/flow/state.md) |
| **Görev Türleri** | [flow/task.md](doc/tr/flow/task.md) | [flow/task.md](doc/en/flow/task.md) |
| **Haritalama Rehberi** | [flow/mapping.md](doc/tr/flow/mapping.md) | [flow/mapping.md](doc/en/flow/mapping.md) |
| **Instance Nasıl Başlatılır** | [how-to/start-instance.md](doc/tr/how-to/start-instance.md) | [how-to/start-instance.md](doc/en/how-to/start-instance.md) |
