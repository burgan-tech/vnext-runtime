# vnext-runtime — Dependency Trace Contract

> `vnext-runtime` bir **çalıştırma/dağıtım (runtime distribution) reposudur**, kaynak-kod değil
> (0 `.cs/.ts/.dart`). "Giriş noktaları" = Makefile hedefleri + Docker Compose servisleri + shell
> scriptleri. vNext motorunun kendisi bu repoda YOKTUR; `ghcr.io/burgan-tech/vnext/*` imajları
> olarak çekilir. Bu yüzden iz-sürme çoğu zaman "config/servis → çekilen imaj → burgan-tech/vnext"
> sınırında biter (bkz. §5).

## 1. Giriş noktaları

### 1a. Makefile hedefleri (asıl operatör arayüzü)
`Makefile` (kök) tüm yaşam döngüsünü sarar; `$(COMPOSE_CMD)` runtime'da tespit edilir
(OrbStack/Docker/Podman, `Makefile:20-33`). Anlamlı hedefler:

| Hedef | Ne yapar | Kanıt |
|---|---|---|
| `dev` | `setup` + `up-build` + `health` (tam lokal kurulum) | `Makefile:393-397` |
| `setup` | `create-env-files` + `create-network` | `Makefile:98-102` |
| `up` | infra + tüm yapılandırılmış domain'leri başlatır | `Makefile:169-187` |
| `up-infra` | yalnızca `--profile infra` servisleri | `Makefile:189-194` |
| `up-vnext DOMAIN=x` | `-p vnext-x --env-file domains/x/.env --profile vnext up -d` | `Makefile:196-212` |
| `create-domain DOMAIN=x PORT_OFFSET=n` | `./create-domain.sh` çağırır | `Makefile:486-488` |
| `db-create/db-drop/db-reset/db-connect/db-list` | `psql` ile domain DB'si (`vNext_<D>`) | `Makefile:355-390` |
| `publish-component` / `republish-component` | `./publish-component.sh` / publisher container | `Makefile:464-483` |
| `change-domain` | LEGACY tek-domain modu (bkz. tuzaklar — kısmen bozuk) | `Makefile:539-611` |
| `health`, `status*`, `logs*`, `shell-*` | teşhis | `Makefile:284-409` |

Docker Compose iki **profil** kullanır: `infra` (paylaşılan, tek örnek) ve `vnext` (domain başına,
proje adı `vnext-<domain>`) (`docker-compose.yml:16-24,270-273`).

### 1b. Docker Compose servisleri (çekilen imajlar = motor)
`vnext/docker/docker-compose.yml`. **Profil `vnext` (domain başına):**

| Servis | İmaj (motor kaynağı) | Host portu | Kanıt |
|---|---|---|---|
| vnext-db-migrator | `ghcr.io/burgan-tech/vnext/db-migrator` | — (`restart:no`) | `:21-33` |
| vnext-app (orchestrator) | `ghcr.io/burgan-tech/vnext/orchestrator` | 4201→5000 | `:53-72` |
| vnext-init | `ghcr.io/burgan-tech/vnext/init` | 3005→3000 | `:76-98` |
| vnext-component-publisher | `alpine/curl` (script koşucusu) | — | `:99-120` |
| vnext-execution-app | `ghcr.io/burgan-tech/vnext/execution` | 4202→5000 | `:121-140` |
| vnext-worker-inbox | `ghcr.io/burgan-tech/vnext/inbox` | 4203→5000 | `:184-206` |
| vnext-worker-outbox | `ghcr.io/burgan-tech/vnext/outbox` | 4204→5000 | `:227-249` |
| vnext-*-dapr (×5) | `daprio/daprd` | — (sidecar) | `:34-52,144-183,207-269` |

**Profil `infra` (paylaşılan):** dapr-placement (`daprio/dapr`, 50005, `:274-282`),
dapr-scheduler (`daprio/scheduler`, 50007, `:283-304`), redis (`:305-314`), postgres (`:315-335`),
vault + vault-prepopulate (`:336-379`), openobserve (`:380-394`), otel-collector (`:395-413`).

### 1c. Shell scriptleri (Makefile'ın çağırdığı mantık)
- `vnext/docker/create-domain.sh` — `templates/` → `domains/<d>/` üretimi + port hesaplama
  (`:24-45`) + `{{PLACEHOLDER}}` sed replace (`:80-125`).
- `vnext/docker/publish-component.sh` — vnext-init sağlığını bekler, `/api/package/runtime/publish`'e
  `{version, appDomain}` POST eder (`:20,149-153`).
- `vnext/docker/run-docker.sh` — basit `docker network create` + `docker-compose up --build`
  (Makefile'sız yol; `:3-6`).
- `vnext/docker/config/vault/vault.sh` — vault-prepopulate'in koştuğu secret tohumlama scripti.

## 2. İç bağımlılık grafı
Kod modülü referansı yoktur; "bağımlılık" = Compose başlatma/sağlık sırası ve dosya mount'ları.

```
make dev → setup → up-build
  infra profili:
    postgres (healthcheck) ─┐
    redis ───────────────────┤ paylaşılan
    vault → vault-prepopulate ┤
    dapr-placement, dapr-scheduler
    openobserve → otel-collector
  vnext profili (domain başına):
    vnext-db-migrator (completed_successfully) ──► gate
      ├─ vnext-app (healthy) ──► vnext-init (healthy) ──► vnext-component-publisher
      ├─ vnext-execution-app
      ├─ vnext-worker-inbox
      └─ vnext-worker-outbox
    her app ◄── kendi *-dapr sidecar'ı (network_mode: service:<app>)
```
Kanıt: db-migrator gate `docker-compose.yml:73-75,141-143,204-206,247-249`; app→init→publisher
zinciri `:95-98,117-119`; sidecar network paylaşımı `:50,161,181,224,267`.

Config dosyalarının servislere mount'u:
- `domains/<d>/appsettings.<Service>.Development.json` → `/app/appsettings.Development.json:ro`
  (`docker-compose.yml:31,63,131,194,237`).
- `domains/<d>/.env.<service>` → ilgili servisin `env_file`'ı (`:28,58,126,189,232`).
- `vnext/<servis>/dapr/components/` → sidecar `/components` (`--resources-path ./components`,
  `:42,49,153,160,173,180,216,223,259,266`).

## 3. Dış bağımlılıklar

### 3a. Paketler / imajlar (mimari anlamlı)
- **Motor imajları:** `ghcr.io/burgan-tech/vnext/{db-migrator,orchestrator,init,execution,inbox,outbox}`
  — sürüm `VNEXT_VERSION` (`.env:6`, varsayılan `latest`). Bunlar burgan-tech/vnext repo'sunun
  GHCR paketleridir → asıl kod orada.
- **Dapr:** `daprio/daprd` (sidecar), `daprio/dapr` (placement), `daprio/scheduler`
  (`docker-compose.yml:35,275,284`); sürüm `DAPR_RUNTIME_VERSION` vb.
- **Altyapı imajları:** `redis`, `postgres`, `vault:1.13.3`, `alpine/curl`,
  `public.ecr.aws/zinclabs/openobserve`, `otel/opentelemetry-collector-contrib`
  (`docker-compose.yml:307,317,338,370,383,398`).
- **npm:** `@burgan-tech/vnext-core-runtime` — vnext-init'in indirdiği bileşen paketi
  (README.md:359; sürüm `VNEXT_COMPONENT_VERSION`/`VNEXT_CORE_VERSION`, `templates/.env:11-15`).
  Geliştirici tarafında `@burgan-tech/vnext-template` (README.md:262).

### 3b. Dış servisler (host/URL KAYNAĞI)
Bu repo servis "çağırmaz"; servisleri **konfigüre eder**. Tanımlı uçlar:
- **Dapr state/pubsub/lock → Redis:** `vnext-redis:6379` (`orchestration/dapr/components/state.yaml:9`,
  `pubsub.yaml:8`).
- **Dapr secret → Vault:** `http://vnext-vault:8200`, `secretstores.hashicorp.vault`
  (`orchestration/dapr/components/secretstore.yaml:9-12`).
- **Dapr notification binding'leri → mock:** `vnext-notification-{email,sms,state}` `bindings.http`,
  hedef `http://mocklab:5000/api/notification/*` (`orchestration/dapr/components/vnext-notification-*.yaml:10`).
  ⚠️ SADECE orchestration servisinde tanımlı; `mocklab` host'u güncel compose'da servis olarak YOK.
- **OTEL → collector:** `http://otel-collector:4317` (appsettings `:67`, `.env.orchestration:43`).
- **.NET appsettings dış uçları:** `vNextApi.BaseUrl` OrbStack DNS `…orb.local` (`appsettings.Development.json:22`),
  `Example.ApiBaseUrl=http://localhost:3001` (mockoon, `:234-236`), `ServiceDiscovery` (varsayılan
  `Enabled:false`, `:247-260`), `ClickHouse` (`Enabled:false`, `:221-233`), `Vault.Enabled:false` (`:218`).

### 3c. Veritabanları
- **PostgreSQL** (paylaşılan, `vnext-postgres:5432`) — domain başına ayrı DB `vNext_<Domain>`
  (README.md:78-82; DB adı normalizasyonu `create-domain.sh:44-45`). Bağlantı:
  `appsettings.*.json` `ConnectionStrings:Default` (`:4`, kullanıcı/şifre DEV sabiti — değer yazma)
  + servis `.env`'lerinde `DATABASE_*` (`.env.orchestration:9-15`). Şema/migration mantığı
  **vnext-db-migrator imajının içindedir** (bu repoda migration SQL'i yok) → external.
- **Redis** (`vnext-redis:6379`) — Dapr state/pubsub/lock + .NET `Redis` cache (`appsettings:42-58`).
- **ClickHouse** — appsettings'te `Enabled:false`; config `config/clickhouse/` var ama compose'da
  servis YOK (bkz. tuzaklar).

## 4. İz sürme algoritması (adım adım)
"Bir servis/port/ortam-değişkeni/Dapr-bileşeni değişirse etkisini nasıl bulurum":
1. **Bir servis mi?** `docker-compose.yml`'de servis adını bul → `image` (motor mu, altyapı mı?),
   `env_file`, `volumes` (hangi appsettings/dapr dizini), `ports`, `depends_on`. İmaj
   `ghcr.io/burgan-tech/vnext/*` ise davranış motordadır → external (burgan-tech/vnext).
2. **Bir ortam değişkeni mi?** Önce `templates/.env*` veya `templates/appsettings.*.json` (KAYNAK),
   sonra `domains/<d>/` (üretilmiş kopya). Değişken `{{PLACEHOLDER}}` ise `create-domain.sh`'in
   sed satırından (`:87-105`) nasıl doldurulduğunu izle.
3. **Bir Dapr bileşeni/binding mi?** `vnext/<servis>/dapr/components/*.yaml`; hangi servisin
   sidecar'ına mount edildiğini `docker-compose.yml` volume satırından doğrula. Bileşeni TÜKETEN
   kod motordadır → external.
4. **Bir Makefile hedefi mi?** `^<hedef>:` grep'le; gövdesindeki `$(MAKE)`/`$(COMPOSE_CMD)`/script
   zincirini izle. `create-domain`/`publish-component`/`change-domain` script'e devreder.
5. **Bir port mu?** Host portu `domains/<d>/.env` (`VNEXT_*_PORT`, offset'li) veya
   `docker-compose.yml` `${VNEXT_*_PORT:-default}`; container portu her app'te 5000 (health
   `localhost:5000`). Offset hesabı `create-domain.sh:24-41`.
6. Repo dışına çıkan her ucu (motor imajı, bileşen paketi, Dapr runtime, DB/Redis/Vault/OTEL
   backend davranışı) `external_dependencies`'e yaz.

## 5. DURDU kriterleri
İz aşağıdaki durumlarda bu repoda biter; `DURDU: <somut neden>` yaz:
- **Motor iş mantığı:** workflow yürütme, task tipleri, instance filtreleme, `/api/admin` /
  `/api/v1.0/...` davranışı `ghcr.io/burgan-tech/vnext/*` imajlarının İÇİNDEDİR → DURDU: motor kodu
  burgan-tech/vnext'te.
- **Domain bileşen tanımları:** workflow/task/view/function/schema/extension JSON'ları bu repoda YOK;
  `@burgan-tech/vnext-core-runtime` paketinden veya geliştirici projesinden gelir → external.
- **DB migration / şema:** SQL/migration `vnext-db-migrator` imajının içinde → DURDU.
- **Dapr runtime davranışı:** bileşen tanımları burada; state/pubsub/secret/binding'i TÜKETEN mantık
  motorda ve Dapr sidecar runtime'ında → DURDU.
- **Bağlanılan servis host'ları:** çoğu `vnext-postgres`/`vnext-redis`/`vnext-vault` gibi compose-içi
  DNS; gerçek prod hedefleri bu repoda değil (lokal geliştirme odaklı) → DURDU: lokal-dev config.
- **Gizli değer içeriği:** DEV varsayılanları tracked dosyalarda görünse de rapora değerini yazma
  (yalnızca `dosya:satır` referansı).
