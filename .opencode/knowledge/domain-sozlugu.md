# vnext-runtime — Domain Sözlüğü

Yalnızca bu repo config'ini okurken gerçekten gereken terimler. Her madde kanıtlı.

## Platform / motor
- **vNext:** Burgan'ın workflow/orkestrasyon platformu. Bu repo onun **motorunu çalıştıran** lokal
  dağıtım ortamıdır; motor kodu burada değil, `ghcr.io/burgan-tech/vnext/*` imajlarındadır
  (`docker-compose.yml:23,55,123,186,229`).
- **burgan-tech/vnext:** motorun asıl (server-side .NET) kaynak reposu; imajları GHCR'a bu ad altında
  yayınlanır (`ghcr.io/burgan-tech/vnext/...`). Bu repo o imajları TÜKETİR.
- **orchestrator / vnext-app:** workflow orkestrasyon API servisi (host 4201→container 5000,
  `docker-compose.yml:53-72`). Namespace `BBT.Workflow.Orchestration` (`appsettings:61`).
- **execution / vnext-execution-app:** task yürütme servisi (4202→5000, `:121-140`);
  `ExecutionApi.AppId` ile orchestration'dan çağrılır (`appsettings:182-184`).
- **worker-inbox / worker-outbox:** mesaj/kuyruk işleyen worker servisleri (4203/4204→5000,
  `:184-249`); outbox şeması `sys_queues` (`appsettings:145-147`).
- **db-migrator:** DB migration'larını koşan tek-seferlik servis; `restart:no`, diğerlerini
  `completed_successfully` ile geçitler (`docker-compose.yml:21-33,73-75`).
- **vnext-init:** bileşen paketini indirip `domain` alanını değiştirip `/api/admin`'e yükleyen init
  container'ı (`docker-compose.yml:76-98`, README.md:355-370).
- **vnext-component-publisher:** `publish-component.sh`'i koşan `alpine/curl` container'ı
  (`docker-compose.yml:99-120`).

## Domain kavramı
- **domain (APP_DOMAIN):** izole bir vNext çalışma alanı (`core`, `discovery`, `sales`…). Her domain
  ayrı DB + ayrı portlar + ayrı Compose projesi (`-p vnext-<domain>`) kullanır ama altyapıyı paylaşır
  (README.md:30-33). `DOMAIN_NAME`/`APP_DOMAIN` `.env`'de (`templates/.env:6-7`).
- **PORT_OFFSET:** port çakışmasını önleyen sayısal ofset; app portları `4201+offset`, Dapr portları
  `offset*100` (`create-domain.sh:24-41`). `core`=0, `discovery`=5 rezerve; özel ≥10 (README.md:87-97).
- **templates/ vs domains/:** `templates/` düzenlenecek KAYNAK şablonlar (`{{PLACEHOLDER}}`'lı);
  `domains/<d>/` `create-domain.sh`'in ürettiği kopyalar (README.md:28, `create-domain.sh:113-125`).
- **DB adı:** `vNext_<Normalize(domain)>`; `core`→`vNext_Core`, `user-management`→`vNext_User_Management`
  (README.md:78-82).

## Dapr terimleri
- **daprd / sidecar:** her app servisinin yanında koşan Dapr runtime container'ı; app'in ağ
  namespace'ini paylaşır (`network_mode: service:<app>`, `docker-compose.yml:161`).
- **placement / scheduler:** paylaşılan Dapr altyapı servisleri (50005 / 50007,
  `docker-compose.yml:274-304`).
- **component (bileşen):** `vnext/<servis>/dapr/components/*.yaml` — state/pubsub/lock/secret/binding
  tanımları; sidecar `--resources-path ./components` ile yükler (`:42,49`).
- **store name:** ortam değişkeni (`DAPR_STATE_STORE_NAME=vnext-state`) bileşen `metadata.name`'iyle
  eşleşmeli (`.env.orchestration:39` ↔ `state.yaml:4`).
- **binding (notification):** `bindings.http` tipli çıkış bağlaması; `vnext-notification-{email,sms,state}`
  yalnızca orchestration'da, hedef `http://mocklab:5000/...` (`vnext-notification-email.yaml:6-10`).

## Altyapı / config
- **infra vs vnext profili:** Compose profilleri; `infra` bir kez paylaşılan servisler, `vnext`
  domain başına app servisleri (`docker-compose.yml:16-24,270-273`).
- **Vault:** Dapr secret store arka ucu (HashiCorp Vault, `vnext-vault:8200`); dev modu,
  `secretstore.yaml:6`. appsettings'te `Vault.Enabled:false` (uygulama-içi Vault kapalı,
  `appsettings:218-220`).
- **OpenObserve:** log/trace görselleştirme UI'si (`http://localhost:5080`, `docker-compose.yml:380-394`).
- **OTEL collector:** telemetry toplayıcı; OTLP `:4317` alır, openobserve'e verir (`:395-413`).
- **ClickHouse:** analitik DB; appsettings'te `Enabled:false` ve compose'da servisi YOK — config
  dizini (`config/clickhouse/`) mevcut ama bağlı değil (`appsettings:221-233`; bkz. bilinen-tuzaklar).
- **mockoon / mocklab:** mock API sunucusu; README `Example.ApiBaseUrl=http://localhost:3001`
  (`appsettings:234-236`) ve notification binding'leri `mocklab:5000`'e işaret eder ama güncel
  compose'da mock servisi YOK (bkz. bilinen-tuzaklar).

## Sürüm değişkenleri
- **VNEXT_VERSION:** motor imaj etiketi (`ghcr.io/burgan-tech/vnext/*:${VNEXT_VERSION}`),
  varsayılan `latest` (`.env:6`).
- **VNEXT_COMPONENT_VERSION / VNEXT_CORE_VERSION:** yayınlanan bileşen paketi sürümü
  (`templates/.env:11-15`); `publish-component.sh`'in zorunlu değişkeni (`:62-63`).
- **`latest` her yerde:** `VAULT_VERSION=1.13.3` hariç tüm imaj etiketleri `latest`
  (`.env:6-16`) — sürüm sabitlenmemiştir.
