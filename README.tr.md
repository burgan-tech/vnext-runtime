# VNext Runtime - Lokal Development Ortamı

[![TR](https://img.shields.io/badge/🇹🇷-Türkçe-red)](README.tr.md) [![EN](https://img.shields.io/badge/🇺🇸-English-blue)](README.md)

Bu proje, geliştiricilerin lokal ortamlarında VNext Runtime sistemini ayağa kaldırıp development yapmalarına olanak sağlamak için oluşturulmuştur. Docker tabanlı bu kurulum, tüm bağımlılıkları içerir ve hızlı bir şekilde geliştirme ortamını hazır hale getirir.

> **Languages:** This README is available in [English](README.en.md) | [Türkçe](README.md)

## Gerekli Dosyalar

Sistemi çalıştırmak için aşağıdaki environment dosyalarını oluşturmanız gerekmektedir:

### .env (Ana Environment Variables)
```bash
# VNext Core Runtime Version
VNEXT_CORE_VERSION=latest

# Uygulama Domain'i (YENİ!)
# Bu domain değeri JSON dosyalarındaki tüm "domain" property'lerini değiştirir
# Her geliştiricinin kendi domain'inde lokal ortamda çalışmasını sağlar
# Varsayılan: core
APP_DOMAIN=core

# Custom Components Path (isteğe bağlı)
CUSTOM_COMPONENTS_PATH=./vnext/docker/custom-components

# Docker Image Versions (isteğe bağlı - varsayılan değerlerini override edebilirsiniz)
VNEXT_ORCHESTRATOR_VERSION=0.0.6
VNEXT_EXECUTION_VERSION=0.0.6
DAPR_RUNTIME_VERSION=latest
```

### .env.orchestration
```bash
# VNext Orchestration Environment Variables
# Bu değerler vnext-app servisinin AppSettings yapılandırmasını override eder

# Application Settings
ApplicationName=vnext
APP_PORT=4201
APP_HOST=0.0.0.0

# Database Configuration (ConnectionStrings:Default)
ConnectionStrings__Default=Host=vnext-postgres;Port=5432;Database=Aether_WorkflowDb;Username=postgres;Password=postgres;

# Redis Configuration
Redis__Standalone__EndPoints__0=vnext-redis:6379
Redis__InstanceName=workflow-api
Redis__ConnectionTimeout=5000
Redis__DefaultDatabase=0
Redis__Password=
Redis__Ssl=false

# vNext API Configuration
vNextApi__BaseUrl=http://localhost:4201
vNextApi__ApiVersion=1
vNextApi__TimeoutSeconds=30
vNextApi__MaxRetryAttempts=3
vNextApi__RetryDelayMilliseconds=1000

# Telemetry Configuration
Telemetry__ServiceName=vNext-orchestration
Telemetry__ServiceVersion=1.0.0
Telemetry__Environment=Development
Telemetry__Otlp__Endpoint=http://otel-collector:4318

# Logging
Logging__LogLevel__Default=Information
Logging__LogLevel__Microsoft.AspNetCore=Warning
Telemetry__Logging__MinimumLevel=Information

# Execution Service
ExecutionService__AppId=vnext-execution-app

# Vault Configuration
Vault__Enabled=false

# Dapr Configuration
DAPR_HTTP_PORT=42110
DAPR_GRPC_PORT=42111
```

### .env.execution
```bash
# VNext Execution Environment Variables
# Not: Şu anda docker-compose.yml'de comment'li durumdadır

# Application Settings
ApplicationName=vnext-execution
APP_PORT=5000
APP_HOST=0.0.0.0

# Database Configuration
ConnectionStrings__Default=Host=vnext-postgres;Port=5432;Database=Aether_ExecutionDb;Username=postgres;Password=postgres;

# Redis Configuration
Redis__Standalone__EndPoints__0=vnext-redis:6379
Redis__InstanceName=execution-api
Redis__ConnectionTimeout=5000
Redis__DefaultDatabase=1

# Telemetry Configuration
Telemetry__ServiceName=vNext-execution
Telemetry__ServiceVersion=1.0.0
Telemetry__Environment=Development
Telemetry__Otlp__Endpoint=http://otel-collector:4318

# Dapr Configuration
DAPR_HTTP_PORT=43110
DAPR_GRPC_PORT=43111
```

## Desteklenen Environment Variables

Aşağıdaki tablo, AppSettings yapılandırmasından türetilen ve `.env.orchestration` / `.env.execution` dosyalarında kullanabileceğiniz environment variable'ları göstermektedir:

### Temel Uygulama Ayarları
| Environment Variable | Açıklama | Varsayılan Değer |
|---------------------|----------|------------------|
| `ApplicationName` | Uygulama adı | `vnext` |
| `APP_HOST` | Uygulamanın dinleyeceği host | `0.0.0.0` |
| `APP_PORT` | Uygulamanın dinleyeceği port | `4201` |

### Veritabanı Yapılandırması
| Environment Variable | Açıklama | Varsayılan Değer |
|---------------------|----------|------------------|
| `ConnectionStrings__Default` | PostgreSQL bağlantı string'i | `Host=localhost;Port=5432;Database=Aether_WorkflowDb;Username=postgres;Password=postgres;` |

### Redis Yapılandırması
| Environment Variable | Açıklama | Varsayılan Değer |
|---------------------|----------|------------------|
| `Redis__Standalone__EndPoints__0` | Redis endpoint | `localhost:6379` |
| `Redis__InstanceName` | Redis instance adı | `workflow-api` |
| `Redis__ConnectionTimeout` | Bağlantı timeout (ms) | `5000` |
| `Redis__DefaultDatabase` | Varsayılan database index | `0` |
| `Redis__Password` | Redis şifresi | `` |
| `Redis__Ssl` | SSL kullanımı | `false` |

### vNext API Yapılandırması
| Environment Variable | Açıklama | Varsayılan Değer |
|---------------------|----------|------------------|
| `vNextApi__BaseUrl` | API base URL | `http://localhost:4201` |
| `vNextApi__ApiVersion` | API versiyonu | `1` |
| `vNextApi__TimeoutSeconds` | İstek timeout | `30` |
| `vNextApi__MaxRetryAttempts` | Maksimum retry sayısı | `3` |
| `vNextApi__RetryDelayMilliseconds` | Retry gecikmesi | `1000` |

### Telemetry ve Logging
| Environment Variable | Açıklama | Varsayılan Değer |
|---------------------|----------|------------------|
| `Telemetry__ServiceName` | Telemetry servis adı | `vNext-orchestration` |
| `Telemetry__ServiceVersion` | Servis versiyonu | `1.0.0` |
| `Telemetry__Environment` | Ortam adı | `Development` |
| `Telemetry__Otlp__Endpoint` | OpenTelemetry endpoint | `http://localhost:4318` |
| `Logging__LogLevel__Default` | Varsayılan log seviyesi | `Information` |
| `Logging__LogLevel__Microsoft.AspNetCore` | ASP.NET Core log seviyesi | `Warning` |
| `Telemetry__Logging__MinimumLevel` | Minimum telemetry log seviyesi | `Information` |

### Task Factory
| Environment Variable | Açıklama | Varsayılan Değer |
|---------------------|----------|------------------|
| `TaskFactory__UseObjectPooling` | Object pooling kullanımı | `false` |
| `TaskFactory__MaxPoolSize` | Maksimum pool boyutu | `50` |
| `TaskFactory__InitialPoolSize` | Başlangıç pool boyutu | `5` |
| `TaskFactory__EnableMetrics` | Metrics aktifleştirme | `true` |

### Diğer Servisler
| Environment Variable | Açıklama | Varsayılan Değer |
|---------------------|----------|------------------|
| `ExecutionService__AppId` | Execution servis app ID | `vnext-execution-app` |
| `Vault__Enabled` | Vault kullanımı | `false` |
| `ResponseCompression__Enable` | Response compression | `true` |

## Hızlı Başlangıç

### Makefile ile Kolay Kurulum (Önerilen)

Projede bulunan Makefile, geliştiriciler için en konforlu çalıştırma ortamını sağlar. Tüm karmaşık işlemleri tek komutla halledebilirsiniz:

```bash
# Development ortamını tek komutla kurup başlat
make dev

# Yardım menüsünü görüntüle
make help

# Sadece environment dosyalarını oluştur
make setup
```

### Manuel Kurulum

Eğer Makefile kullanmak istemiyorsanız, manual olarak kurabilirsiniz:

#### 1. Gerekli Dosyaları Oluşturun

Yukarıda belirtilen `.env`, `.env.orchestration` ve `.env.execution` dosyalarını `vnext/docker/` dizininde oluşturun.

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

### Örnek: E-Commerce Workflow

VNext Runtime sisteminin tüm yeteneklerini gösteren kapsamlı bir e-ticaret workflow örneği sunuyoruz:

- **HTTP Test Dosyası**: `samples/ecommerce/ecommerce-workflow.http` - Test için hazır HTTP istekleri
- **Dokümantasyon**: 
  - 🇺🇸 [İngilizce Rehber](samples/ecommerce/README-ecommerce-workflow-en.md)
  - 🇹🇷 [Türkçe Rehber](samples/ecommerce/README-ecommerce-workflow-tr.md)
- **Gösterilen Özellikler**:
  - State tabanlı workflow yönetimi
  - Kimlik doğrulama akışı
  - Ürün browsing ve seçimi
  - Sepet yönetimi
  - Sipariş işleme
  - Hata yönetimi ve retry mekanizmaları

Bu örnek, VNext Runtime kullanarak karmaşık iş akışlarının nasıl implement edileceğini anlamak için pratik bir başlangıç noktası sağlar.

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
| `make setup` | Environment dosyalarını ve network'ü oluşturur | `make setup` |
| `make info` | Proje bilgilerini ve erişim URL'lerini gösterir | `make info` |

### Environment Setup

| Komut | Açıklama | Kullanım |
|-------|----------|----------|
| `make create-env-files` | Environment dosyalarını oluşturur | `make create-env-files` |
| `make create-network` | Docker network'ünü oluşturur | `make create-network` |
| `make check-env` | Environment dosyalarının varlığını kontrol eder | `make check-env` |

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

# Sadece logları takip etme
make logs-orchestration

# Servis durumunu kontrol etme
make status
make health

# Development sırasında yeniden başlatma
make restart

# Custom component ekledikten sonra yeniden yükleme
make reload-components

# Temizlik ve yeniden kurulum
make reset
make dev

# Container'lara erişim
make shell-orchestration
make shell-postgres

# Monitoring özel işlemleri
make monitoring-up          # Sadece monitoring servislerini başlat
make logs-monitoring        # Prometheus & Grafana loglarını takip et
make monitoring-status      # Monitoring servis durumunu kontrol et
make prometheus-config-reload  # Prometheus config'i yeniden yükle
make grafana-reset-password    # Grafana şifresini resetle
```

## Servisler ve Portlar

| Servis | Açıklama | Port | Erişim URL |
|--------|----------|------|------------|
| **vnext-app** | Ana orchestration uygulaması | 4201 | http://localhost:4201 |
| **vnext-execution-app** | Execution servis uygulaması | 4202 | http://localhost:4202 |
| **vnext-core-init** | Sistem component'lerini yükleyen init container | - | - |
| **vnext-orchestration-dapr** | Orchestration servisi için Dapr sidecar | 42110/42111 | - |
| **vnext-execution-dapr** | Execution servisi için Dapr sidecar | 43110/43111 | - |
| **dapr-placement** | Dapr placement servisi | 50005 | - |
| **dapr-scheduler** | Dapr scheduler servisi | 50007 | - |
| **vnext-redis** | Redis cache | 6379 | - |
| **vnext-postgres** | PostgreSQL veritabanı | 5432 | - |
| **vnext-vault** | HashiCorp Vault (opsiyonel) | 8200 | http://localhost:8200 |
| **openobserve** | Observability dashboard | 5080 | http://localhost:5080 |
| **otel-collector** | OpenTelemetry Collector | 4317, 4318, 8888 | - |
| **prometheus** | Metrics toplama ve depolama | 9090 | http://localhost:9090 |
| **grafana** | Metrics görselleştirme ve dashboard | 3000 | http://localhost:3000 |

## Management Tools

| Tool | URL | Kullanıcı Adı | Şifre |
|------|-----|---------------|-------|
| **Redis Insight** | http://localhost:5501 | - | - |
| **PgAdmin** | http://localhost:5502 | info@info.com | admin |
| **OpenObserve** | http://localhost:5080 | root@example.com | Complexpass#@123 |
| **Vault UI** | http://localhost:8200 | - | admin (token) |
| **Prometheus** | http://localhost:9090 | - | - |
| **Grafana** | http://localhost:3000 | admin | admin |

## Development İpuçları

### Environment Variable'ları Customize Etme

Environment dosyalarını oluşturmak için:

```bash
# Makefile ile otomatik oluşturma (önerilen)
make create-env-files

# Manuel olarak vnext/docker/ dizininde .env dosyalarını oluşturun
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
   # Dosyaları oluştur
   make create-env-files
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