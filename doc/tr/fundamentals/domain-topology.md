# Domain Topolojisi ve Mimari

## Platform Domain Kavramı

vNext Runtime platformu, **Domain** kavramını temel alır. Domain, bir iş alanını, ürün grubunu veya ekip sorumluluğunu temsil eden izole edilmiş bir runtime ortamıdır.

### Domain = Runtime Prensibi

**Her domain, kendi bağımsız runtime'ına sahiptir.** Bu prensip, platformun temel mimarisini oluşturur:

- Bir domain = Bir vNext Runtime instance
- Her domain tekil ve bağımsızdır
- Domainler arası izolasyon tam olarak sağlanır

## Domain Örnekleri

Bir kurumda domainler şu şekilde organize edilebilir:

### Ürün Grubu Bazlı Domain
```
Onboarding Domain
├── vNext Runtime (onboarding)
├── Database (onboarding_db)
├── PubSub (onboarding_events)
└── State Store (onboarding_state)
```

**Örnek:** Müşteri kabul süreçlerini yöneten onboarding ekibinin kendi domain'i.

### Ekip Sorumluluğu Bazlı Domainler
```
Entegrasyon Ekibi
├── IDM Domain (Kimlik yönetimi)
│   ├── vNext Runtime (idm)
│   └── Infrastructure
└── Notification Domain (Bildirim servisleri)
    ├── vNext Runtime (notification)
    └── Infrastructure
```

**Örnek:** Entegrasyon ekibi, sorumluluğundaki IDM ve Notification sistemlerini ayrı domainler olarak yönetir.

## Domain İzolasyonunun Faydaları

### 1. Altyapı İzolasyonu
Her domain kendi altyapı bileşenlerine sahiptir:
- **Database**: Domain'e özel veritabanı
- **PubSub**: Domain'e özel mesajlaşma kanalları
- **State Store**: Domain'e özel durum yönetimi
- **Secrets**: Domain'e özel güvenlik yapılandırması

### 2. Bağımsız Geliştirme
- Her domain ekibi kendi hızında gelişebilir
- Domain'ler arası bağımlılık minimum seviyededir
- Versiyon yönetimi domain bazlı yapılır
- Deployment bağımsız gerçekleştirilir

### 3. Ölçeklenebilirlik
- Her domain ihtiyacına göre ölçeklendirilir
- Yüksek yük alan domain'ler daha fazla kaynak alabilir
- Düşük yük alan domain'ler minimum kaynakla çalışır
- Kaynak kullanımı optimize edilir

### 4. Hata İzolasyonu
- Bir domain'deki sorun diğerlerini etkilemez
- Yedekleme ve geri yükleme domain bazlı yapılır
- Bakım ve güncelleme bağımsız planlanır

## Domainler Arası İletişim

Domainler birbirinden izole olmasına rağmen, iş gereksinimleri doğrultusunda iletişim kurabilirler:

### 1. API Gateway Üzerinden
```
┌─────────────┐      API Gateway      ┌─────────────┐
│  Onboarding │◄──────────────────────►│     IDM     │
│   Domain    │    REST/HTTP Calls     │   Domain    │
└─────────────┘                        └─────────────┘
```

- Senkron iletişim
- REST API çağrıları
- HTTP Task kullanımı

### 2. Event-Driven Yapılar
```
┌─────────────┐                        ┌─────────────┐
│  Payments   │──┐                  ┌──│Notification │
│   Domain    │  │  Event Bus       │  │   Domain    │
└─────────────┘  │  (PubSub)        │  └─────────────┘
                 │                  │
                 ├─────────┬────────┤
                 │         │        │
                 └────────Event────┘
```

- Asenkron iletişim
- Event-based entegrasyon
- Gevşek bağlılık (Loose coupling)
- DaprPubSub Task kullanımı

## C4 Context Diagram - Multi-Domain Architecture

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

LAYOUT_WITH_LEGEND()

title vNext Platform - Multi-Domain Architecture (Context Level)

Person(customer, "Müşteri", "Mobil/Web kullanıcısı")
Person(employee, "Çalışan", "Backoffice kullanıcısı")
System_Ext(external_api, "Dış Sistemler", "Banka, Ödeme, KYC sistemleri")

System_Boundary(vnext_platform, "vNext Platform") {
    System(onboarding_domain, "Onboarding Domain", "Müşteri kabul süreçleri\nvNext Runtime")
    System(idm_domain, "IDM Domain", "Kimlik ve yetkilendirme\nvNext Runtime")
    System(notification_domain, "Notification Domain", "Bildirim servisleri\nvNext Runtime")
    System(payment_domain, "Payment Domain", "Ödeme süreçleri\nvNext Runtime")
}

System_Ext(api_gateway, "API Gateway", "Domain'lere erişim katmanı")
System_Ext(event_bus, "Event Bus", "Domain'ler arası event iletişimi")

Rel(customer, api_gateway, "Kullanır", "HTTPS")
Rel(employee, api_gateway, "Kullanır", "HTTPS")
Rel(api_gateway, onboarding_domain, "Yönlendirir", "HTTP/REST")
Rel(api_gateway, idm_domain, "Yönlendirir", "HTTP/REST")
Rel(api_gateway, notification_domain, "Yönlendirir", "HTTP/REST")
Rel(api_gateway, payment_domain, "Yönlendirir", "HTTP/REST")

Rel(onboarding_domain, idm_domain, "Kimlik doğrulama", "HTTP/REST")
Rel(onboarding_domain, notification_domain, "Bildirim gönder", "Event")
Rel(payment_domain, notification_domain, "SMS/Push bildirim", "Event")
Rel(onboarding_domain, external_api, "KYC sorgulaması", "HTTPS")
Rel(payment_domain, external_api, "Ödeme işlemi", "HTTPS")

Rel(onboarding_domain, event_bus, "Event yayınlar", "PubSub")
Rel(payment_domain, event_bus, "Event yayınlar", "PubSub")
Rel(event_bus, notification_domain, "Event tüketir", "PubSub")

@enduml
```

## C4 Container Diagram - Domain İçi Yapı

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

LAYOUT_WITH_LEGEND()

title Single Domain Internal Structure (Container Level)

Person(user, "Kullanıcı", "Domain kullanıcısı")

System_Boundary(domain, "vNext Domain (örn: Onboarding)") {
    Container(orchestration, "vnext-app", "Orchestration Service", "Flow yönetimi, state machine, transition kontrolü")
    Container(execution, "vnext-execution-app", "Execution Service", "Task çalıştırma, serverless worker")
    Container(init, "vnext-init", "Seed Service", "İlk kurulum, system bileşenleri")
    
    ContainerDb(database, "Domain Database", "PostgreSQL", "Flow instance'ları, state, data")
    ContainerDb(state_store, "State Store", "Redis/Dapr", "Distributed state, cache")
    Container(pubsub, "PubSub", "RabbitMQ/Dapr", "Event messaging")
}

System_Ext(external_service, "Dış Servisler", "API'ler, webhooks")

Rel(user, orchestration, "Workflow yönetimi", "HTTPS/REST")
Rel(orchestration, database, "Instance CRUD", "SQL")
Rel(orchestration, execution, "Task çalıştır", "Dapr Service Invocation")
Rel(orchestration, state_store, "State okur/yazar", "Dapr State API")
Rel(orchestration, pubsub, "Event publish/subscribe", "Dapr PubSub API")

Rel(execution, external_service, "HTTP Task", "HTTPS")
Rel(execution, database, "Data okur", "SQL")
Rel(execution, state_store, "Cache kullanır", "Dapr State API")

Rel(init, database, "Schema oluştur, seed data", "SQL")
Rel(init, orchestration, "System flow'ları deploy", "Internal API")

@enduml
```

## Domain Yönetimi Best Practices

### Domain Sınırlarını Doğru Belirleyin
- **İş alanına göre**: Her domain belirli bir iş fonksiyonunu temsil etmeli
- **Ekip sorumluluğuna göre**: Domain sahipliği net olmalı
- **Ölçek ihtiyacına göre**: Farklı yük karakteristiklerine sahip alanlar ayrı domain'ler olmalı

### Domain İzolasyonunu Koruyun
- Domain'ler arası doğrudan database erişimi yasaktır
- Tüm iletişim API veya Event üzerinden olmalı
- Shared infrastructure minimize edilmeli

### Monitoring ve Observability
- Her domain için ayrı monitoring dashboard'ları
- Domain bazlı metrik toplama
- Distributed tracing ile domain'ler arası çağrı takibi

### Versiyon Yönetimi
- Domain'ler bağımsız versiyonlanır
- API contract'ları semantic versioning ile yönetilir
- Breaking change'ler koordine edilir ancak deployment bağımsızdır

## Domain Lifecycle

### 1. Domain Oluşturma
```bash
# Infrastructure provisioning
- Domain database oluşturma
- Domain state store yapılandırması
- Domain PubSub konfigürasyonu

# vNext Runtime deployment
- vnext-init ile system kurulum
- vnext-app deployment
- vnext-execution-app deployment
```

### 2. Domain İşletimi
- Flow deployment ve yönetim
- Monitoring ve alerting
- Scaling ve optimization
- Backup ve disaster recovery

### 3. Domain Retirement
- Migration planlaması
- Dependency analizi
- Graceful shutdown
- Data archiving

## Sonuç

Domain topolojisi, vNext Runtime platformunun ölçeklenebilir, esnek ve yönetilebilir olmasını sağlayan temel mimari karardır. Her domain'in bağımsız runtime'a sahip olması, ekiplerin hızlı hareket etmesini, sistemin dayanıklı olmasını ve kaynakların verimli kullanılmasını mümkün kılar.

## İlgili Dökümanlar

- [Database Architecture](./database-architecture.md) - Domain seviyesinde veritabanı yapısı
- [Persistence](../principles/persistance.md) - Veri saklama stratejileri

