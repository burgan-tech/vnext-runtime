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

## 🚀 vNext Geliştirmeye Başlangıç

vNext Runtime için workflow ve bileşenler geliştirmek amacıyla aşağıdaki araçlara ihtiyacınız olacak:

### 1. vNext CLI

**Repository:** https://github.com/burgan-tech/vnext-cli

vNext CLI, vNext workflow projelerini oluşturmak, doğrulamak ve build etmek için kullanılan komut satırı aracıdır.

**Kurulum & Kullanım:**

```bash
# CLI'ı kurun
npm install -g @burgan-tech/vnext-cli

# Kendi domain'iniz ile yeni bir vNext projesi oluşturun
vnext create DOMAIN_ADINIZ

# Workflow'larınızı doğrulayın
vnext validate

# Workflow paketinizi build edin
vnext build
```

CLI, workflow geliştirme yaşam döngünüzü yönetmenize yardımcı olacak çeşitli komutlar sağlar. Detaylı dokümantasyon için [vnext-cli repository'sini](https://github.com/burgan-tech/vnext-cli) ziyaret edin.

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

## Hızlı Başlangıç

### Makefile ile Kolay Kurulum (Önerilen)

Projede bulunan Makefile, geliştiriciler için en konforlu çalıştırma ortamını sağlar. Sistem environment dosyalarını kontrol eder ve development ortamını tek komutla başlatır:

```bash
# Environment dosyalarını kontrol et ve development ortamını başlat
make dev

# Lightweight development ortamını başlat (monitoring/analytics araçları olmadan)
make dev-lightweight

# Yardım menüsünü görüntüle
make help

# Network kurulumu ve environment kontrolü
make setup
```

### 🪶 Lightweight Modu

Kaynak kısıtlı ortamlar için veya sadece temel işlevselliğe ihtiyacınız olduğunda **lightweight modu** kullanın. Bu mod, ağır monitoring ve analytics araçlarını hariç tutar:

**Hariç Tutulan Servisler:**
- Prometheus (Metrics toplama)
- Grafana (Metrics görselleştirme)
- Metabase (BI Analytics)
- ClickHouse (Analytics veritabanı)
- PgAdmin (PostgreSQL GUI)
- Redis Insight (Redis GUI)

**Dahil Olan Servisler:**
- VNext Orchestration & Execution servisleri
- PostgreSQL, Redis, Vault
- DAPR runtime bileşenleri
- OpenObserve & OpenTelemetry Collector
- Mockoon API mock server

**Kullanım:**

```bash
# Lightweight modda başlat
make dev-lightweight

# Veya servisleri doğrudan başlat
make up-lightweight

# Rebuild ile başlat
make up-build-lightweight

# Lightweight servisleri durdur
make down-lightweight

# Lightweight servisleri yeniden başlat
make restart-lightweight

# Lightweight servis durumunu görüntüle
make status-lightweight

# Lightweight servis loglarını görüntüle
make logs-lightweight
```

**Avantajlar:**
- ⚡ Daha hızlı başlangıç süresi
- 💾 Daha düşük bellek kullanımı (~2GB vs ~4GB)
- 🚀 Daha hafif kaynak ayak izi
- 🎯 Temel workflow geliştirmeye odaklanma

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

## VNext Core Runtime Initialization

`vnext-core-init` servisi, vnext-app servisi healthy olduktan sonra otomatik olarak çalışır ve aşağıdaki işlemleri gerçekleştirir:

1. `@burgan-tech/vnext-core-runtime` npm paketini indirir (versiyon `.env` dosyasından kontrol edilir)
2. Paket içindeki core klasöründen sistem bileşenlerini okur:
   - Extensions (Uzantılar)
   - Functions (Fonksiyonlar)
   - Schemas (Şemalar)
   - Tasks (Görevler)
   - Views (Görünümler)
   - Workflows (İş Akışları)
3. **Custom component'leri birleştirir** (eğer mount edilmiş volume varsa)
4. **🆕 Domain Değiştirme**: JSON dosyalarındaki tüm `"domain"` property değerlerini `APP_DOMAIN` environment variable değeri ile değiştirir
   - Bu sayede her geliştirici kendi domain'inde lokal ortamda çalışabilir
   - Varsayılan domain `"core"`'dur, ancak `.env` dosyasında `APP_DOMAIN=mydomain` ile özelleştirilebilir
   - Hem core sistem bileşenlerine hem de custom component'lere uygulanır
5. Birleştirilmiş ve domain güncellenmiş component'leri `vnext-app/api/admin` endpoint'ine POST request'leri olarak gönderir

Bu şekilde vnext-app uygulaması hem sistem hem de custom component'lerle hazır hale gelir.

## Custom Components

`vnext-core-init` container'ına volume mount ederek kendi custom component'lerinizi ekleyebilirsiniz.

### Kurulum

1. Aşağıdaki yapıda custom components dizini oluşturun:
   ```
   vnext/docker/custom-components/
   ├── Extensions/    # Custom extension tanımları
   ├── Functions/     # Custom function tanımları  
   ├── Schemas/       # Custom JSON schema tanımları
   ├── Tasks/         # Custom task tanımları
   ├── Views/         # Custom view component'leri
   └── Workflows/     # Custom workflow tanımları
   ```

2. `.env` dosyasında `CUSTOM_COMPONENTS_PATH` environment variable'ını ayarlayın:
   ```bash
   CUSTOM_COMPONENTS_PATH=./vnext/docker/custom-components
   ```

3. Eğer set edilmezse, varsayılan olarak `./vnext/docker/custom-components` docker-compose.yml dosyasına göreceli olarak kullanılır.

### Custom Component'ler Nasıl Çalışır

- **Birleştirme**: Custom component ile core component aynı dosya adına sahipse, `data` array'leri birleştirilir
- **Sadece Custom**: Core'da bulunmayan component'ler standalone component olarak yüklenir
- **JSON Schema**: Her component, core component'lerle aynı JSON schema formatını takip etmelidir

Detaylı dokümantasyon ve örnekler için `vnext/docker/custom-components/README.md` dosyasına bakın.

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
| `make up-lightweight` | Servisleri başlatır (lightweight mod) | `make up-lightweight` |
| `make up-build` | Servisleri build ederek başlatır | `make up-build` |
| `make up-build-lightweight` | Servisleri build ederek başlatır (lightweight) | `make up-build-lightweight` |
| `make down` | Servisleri durdurur | `make down` |
| `make down-lightweight` | Servisleri durdurur (lightweight mod) | `make down-lightweight` |
| `make restart` | Servisleri yeniden başlatır | `make restart` |
| `make restart-lightweight` | Servisleri yeniden başlatır (lightweight mod) | `make restart-lightweight` |
| `make build` | Docker image'larını build eder | `make build` |
| `make build-lightweight` | Docker image'larını build eder (lightweight mod) | `make build-lightweight` |

### Service Management

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make status` | Servislerin durumunu gösterir | `make status` |
| `make status-lightweight` | Servislerin durumunu gösterir (lightweight mod) | `make status-lightweight` |
| `make health` | Servislerin sağlık durumunu kontrol eder | `make health` |
| `make logs` | Tüm servislerin loglarını gösterir | `make logs` |
| `make logs-lightweight` | Tüm servislerin loglarını gösterir (lightweight) | `make logs-lightweight` |
| `make logs-orchestration` | Sadece orchestration servis logları | `make logs-orchestration` |
| `make logs-execution` | Sadece execution servis logları | `make logs-execution` |
| `make logs-init` | Core init servis logları | `make logs-init` |
| `make logs-dapr` | DAPR servislerin logları | `make logs-dapr` |
| `make logs-db` | Database servislerin logları | `make logs-db` |
| `make logs-monitoring` | Monitoring servislerin logları | `make logs-monitoring` |
| `make logs-prometheus` | Prometheus servis logları | `make logs-prometheus` |
| `make logs-grafana` | Grafana servis logları | `make logs-grafana` |

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
| `make monitoring-up` | Sadece monitoring servislerini başlatır (Prometheus & Grafana) | `make monitoring-up` |
| `make monitoring-down` | Monitoring servislerini durdurur | `make monitoring-down` |
| `make monitoring-restart` | Monitoring servislerini yeniden başlatır | `make monitoring-restart` |
| `make monitoring-status` | Monitoring servislerinin durumunu gösterir | `make monitoring-status` |
| `make logs-monitoring` | Monitoring servislerinin loglarını gösterir | `make logs-monitoring` |
| `make logs-prometheus` | Prometheus servisinin loglarını gösterir | `make logs-prometheus` |
| `make logs-grafana` | Grafana servisinin loglarını gösterir | `make logs-grafana` |
| `make prometheus-config-reload` | Prometheus konfigürasyonunu yeniden yükler | `make prometheus-config-reload` |
| `make grafana-reset-password` | Grafana admin şifresini 'admin' olarak resetler | `make grafana-reset-password` |

### Custom Components

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make init-custom-components` | Custom components dizin yapısını oluşturur | `make init-custom-components` |
| `make reload-components` | Custom components'leri yeniden yükler | `make reload-components` |

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

# Projeyi lightweight modda çalıştırma (geliştirme için önerilir)
make dev-lightweight

# Sadece logları takip etme
make logs-orchestration
make logs-lightweight  # Lightweight moddaki tüm loglar

# Servis durumunu kontrol etme
make status
make status-lightweight  # Lightweight moddaki durum
make health

# Development sırasında yeniden başlatma
make restart
make restart-lightweight  # Lightweight modda restart

# Custom component ekledikten sonra yeniden yükleme
make reload-components

# Temizlik ve yeniden kurulum
make reset
make dev
# veya lightweight için
make down-lightweight
make dev-lightweight

# Container'lara erişim
make shell-orchestration
make shell-postgres

# Monitoring özel işlemleri (lightweight modda mevcut değil)
make monitoring-up          # Sadece monitoring servislerini başlat
make logs-monitoring        # Prometheus & Grafana loglarını takip et
make monitoring-status      # Monitoring servis durumunu kontrol et
make prometheus-config-reload  # Prometheus config'i yeniden yükle
make grafana-reset-password    # Grafana şifresini resetle
```

## Servisler ve Portlar

| Servis | Açıklama | Port | Erişim URL | Lightweight Mod |
|--------|----------|------|------------|-----------------|
| **vnext-app** | Ana orchestration uygulaması | 4201 | http://localhost:4201 | ✅ Mevcut |
| **vnext-execution-app** | Execution servis uygulaması | 4202 | http://localhost:4202 | ✅ Mevcut |
| **vnext-core-init** | Sistem component'lerini yükleyen init container | - | - | ✅ Mevcut |
| **vnext-orchestration-dapr** | Orchestration servisi için Dapr sidecar | 42110/42111 | - | ✅ Mevcut |
| **vnext-execution-dapr** | Execution servisi için Dapr sidecar | 43110/43111 | - | ✅ Mevcut |
| **dapr-placement** | Dapr placement servisi | 50005 | - | ✅ Mevcut |
| **dapr-scheduler** | Dapr scheduler servisi | 50007 | - | ✅ Mevcut |
| **vnext-redis** | Redis cache | 6379 | - | ✅ Mevcut |
| **vnext-postgres** | PostgreSQL veritabanı | 5432 | - | ✅ Mevcut |
| **vnext-vault** | HashiCorp Vault (opsiyonel) | 8200 | http://localhost:8200 | ✅ Mevcut |
| **openobserve** | Observability dashboard | 5080 | http://localhost:5080 | ✅ Mevcut |
| **otel-collector** | OpenTelemetry Collector | 4317, 4318, 8888 | - | ✅ Mevcut |
| **mockoon** | API Mock Server | 3001 | http://localhost:3001 | ✅ Mevcut |
| **prometheus** | Metrics toplama ve depolama | 9090 | http://localhost:9090 | ❌ Yok |
| **grafana** | Metrics görselleştirme ve dashboard | 3000 | http://localhost:3000 | ❌ Yok |
| **metabase** | BI Analytics Platform | 3002 | http://localhost:3002 | ❌ Yok |
| **clickhouse** | Analytics veritabanı | 8123, 9000 | http://localhost:8123 | ❌ Yok |

## Management Tools

| Tool | URL | Kullanıcı Adı | Şifre | Lightweight Mod |
|------|-----|---------------|-------|-----------------|
| **Redis Insight** | http://localhost:5501 | - | - | ❌ Yok |
| **PgAdmin** | http://localhost:5502 | info@info.com | admin | ❌ Yok |
| **OpenObserve** | http://localhost:5080 | root@example.com | Complexpass#@123 | ✅ Mevcut |
| **Vault UI** | http://localhost:8200 | - | admin (token) | ✅ Mevcut |
| **Prometheus** | http://localhost:9090 | - | - | ❌ Yok |
| **Grafana** | http://localhost:3000 | admin | admin | ❌ Yok |
| **Metabase** | http://localhost:3002 | - | - | ❌ Yok |

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

3. **Volume mount sorunları**: 
   ```bash
   # Custom components dizinini oluştur
   make init-custom-components
   # Path'i kontrol et ve düzelt
   ```

4. **Environment dosyaları eksik**:
   ```bash
   # Environment kontrolü
   make check-env
   # Dosyaların vnext/docker/ dizininde mevcut olduğundan emin olun
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

## 📊 Monitoring ve Metrics

VNext Runtime, gerçek zamanlı sistem gözlemlenebilirliği için Prometheus ve Grafana entegrasyonu ile kapsamlı monitoring yetenekleri içerir.

### 🚀 Monitoring için Hızlı Başlangıç

```bash
# Ana uygulama ile birlikte monitoring servislerini başlat
make dev

# Veya sadece monitoring servislerini başlat
cd vnext/docker
docker-compose up -d prometheus grafana
```

### 📈 Metrics Dashboard Erişimi

- **Grafana Dashboard**: http://localhost:3000 (admin/admin)
- **Prometheus**: http://localhost:9090

### 🎯 Kullanılabilir Metrics

#### Counter Metrics
- `workflow_state_transitions_total` - State geçişleri
- `workflow_errors_total` - Tip/önem derecesine göre toplam hatalar  
- `workflow_exceptions_total` - İşlenmemiş exception'lar
- `workflow_validation_failures_total` - Validation hataları
- `http_requests_total` - HTTP istekleri
- `workflow_db_queries_total` - Veritabanı sorguları
- `script_executions_total` - Script çalıştırmaları
- `background_jobs_scheduled_total` - Arka plan işleri
- `external_service_calls_total` - Harici servis çağrıları
- `dapr_service_invocations_total` - DAPR çağrıları

#### Gauge Metrics
- `workflow_health_status` - Sistem sağlığı (0=sağlıksız, 1=sağlıklı)
- `workflow_error_rate` - Mevcut hata oranı %
- `workflow_instances_by_status` - Duruma göre instance sayısı
- `task_factory_pool_size` - Object pool metrikleri
- `workflow_cache_size_bytes` - Cache boyutu
- `background_jobs_pending` - Bekleyen iş sayısı

#### Histogram Metrics
- `workflow_state_duration_seconds` - Her state'te geçen süre
- `workflow_db_query_duration_seconds` - Veritabanı sorgu süresi
- `http_request_duration_seconds` - HTTP istek süresi
- `background_job_duration_seconds` - İş çalıştırma süresi
- `script_execution_duration_seconds` - Script çalıştırma süresi
- `external_service_duration_seconds` - Harici çağrı süresi

### 📊 Dashboard Özellikleri

#### Sistem Sağlık Genel Bakış
- Genel Sistem Sağlık Durumu (Sağlıklı/Sağlıksız)
- Genel Hata Oranı (%)
- Tip/Önem Derecesine Göre Gerçek Zamanlı Hata Oranı

#### Workflow State Metrics
- State Geçişleri (dakika başına)
- Instance Durum Dağılımı (pasta grafiği)
- State Süresi P95 (saniye)

#### Veritabanı Metrikleri
- Tip/Tablo Bazında Veritabanı Sorguları (dakika başına)
- Sorgu Süresi P95/P50

#### HTTP API Metrikleri
- Endpoint/Durum Bazında HTTP İstekleri (dakika başına)
- İstek Süresi P95
- Tip Bazında HTTP Hataları

#### Arka Plan İşleri & Script Engine
- Arka Plan İşleri Durumu (Bekleyen/Çalışan)
- Tip/Dil Bazında Script Çalıştırmaları

#### Cache & Harici Servisler
- Cache Hit/Miss Oranları
- Durum Bazında Harici Servis Çağrıları
- DAPR Entegrasyon Metrikleri

### 📈 Metrics Endpoint'leri

Workflow uygulamaları şu endpoint'lerden metrics sağlar:
- **Orchestration API**: http://vnext-app:5000/metrics
- **Execution API**: http://vnext-execution-app:5000/metrics

### 🔧 Konfigürasyon Dosyaları

#### Prometheus Konfigürasyonu
- `vnext/docker/config/prometheus/prometheus.yml` - Prometheus scraping konfigürasyonu

#### Grafana Konfigürasyonu
- `vnext/docker/config/grafana/provisioning/datasources/` - Otomatik yapılandırılmış Prometheus datasource
- `vnext/docker/config/grafana/provisioning/dashboards/` - Dashboard provisioning
- `vnext/docker/config/grafana/dashboards/workflow-metrics.json` - Ana workflow dashboard

### 🛠 Monitoring Sorun Giderme

#### Grafana Dashboard Görünmüyor?
1. Container'ların çalışıp çalışmadığını kontrol edin:
   ```bash
   docker ps | grep -E "(grafana|prometheus)"
   ```

2. Prometheus targets'ı kontrol edin:
   - http://localhost:9090/targets adresini ziyaret edin

#### Metrics Gelmiyor?
1. Workflow uygulamasının `/metrics` endpoint'ini kontrol edin
2. Prometheus konfigürasyonunda target'ların doğru olduğunu doğrulayın
3. Network bağlantısını kontrol edin

### 📝 Dashboard'ları Özelleştirme

Dashboard'u özelleştirmek için:
1. Grafana UI'da düzenleyin
2. JSON formatında export edin
3. `vnext/docker/config/grafana/dashboards/workflow-metrics.json` dosyasını güncelleyin