# vnext-runtime — Schema / Config-Domain Contract

> Bu repo kod değil **config/manifest** ağırlıklıdır. Bu sözleşme: (1) config katman şeması,
> (2) dosya → amaç haritası, (3) "X ayarı NEREDE" çözüm algoritması, (4) templating/adlandırma
> desenleri. `code-structure-contract.md` yerine bu seçildi çünkü izlenecek sınıf/DI/katman yok;
> izlenecek olan config-üretim zinciri ve çok-domain yapısıdır.

## 1. Dizin / dosya haritası (üst 2 seviye)

| Yol | Sorumluluk (tek cümle) | Kilit dosya |
|---|---|---|
| `Makefile` | Tüm operatör komutları (kurulum/başlat/DB/publish/domain) | `Makefile:66-684` |
| `README.md` / `README.tr.md` | EN/TR kullanım kılavuzu (ikisi ayna) | `README.md:1` |
| `context7.json` | Context7 doküman indeksleme meta'sı (yalnızca URL + public key) | `context7.json:1-3` |
| `vnext/docker/docker-compose.yml` | Tüm servis tanımı (infra + vnext profilleri) | `docker-compose.yml:1-421` |
| `vnext/docker/templates/` | Yeni domain için `{{PLACEHOLDER}}`'lı **kaynak** şablonlar | `templates/.env:1` |
| `vnext/docker/domains/<d>/` | `create-domain.sh`'in ürettiği domain-özel config kopyaları | `domains/core/.env` |
| `vnext/docker/config/` | Paylaşılan altyapı config'i (vault/otel + kullanılmayan: clickhouse/grafana/prometheus/mockoon) | `config/otel/otel-config.yaml` |
| `vnext/docker/create-domain.sh` | Şablon→domain üretimi + port/DB hesaplama | `create-domain.sh:80-125` |
| `vnext/docker/publish-component.sh` | Bileşen yayınlama (vnext-init'e POST) | `publish-component.sh:149-153` |
| `vnext/<servis>/dapr/components/` | Servis-başına Dapr bileşen tanımları (sidecar'a mount) | `orchestration/dapr/components/state.yaml` |
| `vnext/<servis>/dapr/config.yaml` | Dapr Configuration (tracing + secret scope) | `orchestration/dapr/config.yaml:1-14` |
| `vnext/db-migrator/dapr/components/` | Migrator sidecar (yalnızca config/lock/secretstore) | `db-migrator/dapr/components/secretstore.yaml` |

`<servis>` = `orchestration`, `execution`, `worker-inbox`, `worker-outbox`, `db-migrator`.

## 2. Config katman şeması (öncelik/akış)
Bir servisin efektif ayarı şu katmanlardan derlenir:

1. **Altyapı `.env`** — `vnext/docker/.env` (imaj sürümleri; `up-infra` bunu okur). `create-env-files`
   yoksa üretir (`Makefile:104-128`). İçerik: `VNEXT_VERSION`, `DAPR_*_VERSION`, `POSTGRES_VERSION`,
   `VAULT_VERSION=1.13.3` … hepsi büyük ölçüde `latest` (`.env:5-16`).
2. **Domain `.env`** — `domains/<d>/.env` — `DOMAIN_NAME`, `APP_DOMAIN`, `PORT_OFFSET`, portlar,
   `VNEXT_COMPONENT_VERSION`. `up-vnext` bunu `--env-file` ile yükler (`Makefile:209-210`).
3. **Servis `.env`** — `domains/<d>/.env.<orchestration|execution|worker-inbox|worker-outbox|db-migrator>`
   — `DATABASE_*`, `REDIS_*`, `VAULT_*`, `DAPR_*_STORE_NAME`, `OTEL_*` (`.env.orchestration:9-45`).
   docker-compose `env_file` olarak ilgili servise verir.
4. **.NET appsettings** — `domains/<d>/appsettings.<Service>.Development.json` — uygulama-içi
   detay config; `/app/appsettings.Development.json:ro` olarak mount edilir (`docker-compose.yml:63`).
5. **Dapr Configuration + bileşenler** — `vnext/<servis>/dapr/config.yaml` (tracing/secret scope) +
   `components/*.yaml` (state/pubsub/lock/secret/binding).

> **Kaynak vs kopya kuralı:** `templates/` = editlenecek KAYNAK; `domains/<d>/` = üretilmiş kopya.
> Tüm domain'ler için varsayılanı değiştir → `templates/`'i düzenle. Tek domain'i değiştir →
> `domains/<d>/`'yi düzenle (README.md:28). Kopyayı düzenleyip sebebini `templates/`'te aramak hata olur.

## 3. "X ayarı NEREDE" çözüm algoritması

| Aranan | Nereye bak | Kanıt |
|---|---|---|
| Motor imaj sürümü | `.env`/`domains/<d>/.env` → `VNEXT_VERSION` | `.env:6` |
| Bileşen paket sürümü | `domains/<d>/.env` → `VNEXT_COMPONENT_VERSION` | `templates/.env:15` |
| DB bağlantısı | `appsettings.*.json` `ConnectionStrings:Default` + `.env.<svc>` `DATABASE_*` | `appsettings.Development.json:4`, `.env.orchestration:10-15` |
| DB adı | `vNext_<Normalize(domain)>`; üretim `create-domain.sh:44-45`, `db-create` `Makefile:356-357` | README.md:78-82 |
| Redis | `.env.<svc>` `REDIS_*` + appsettings `Redis` + Dapr `state.yaml`/`pubsub.yaml` | `appsettings:42-58`, `state.yaml:9` |
| Vault / secret | `.env.<svc>` `VAULT_*` + Dapr `secretstore.yaml` + appsettings `Vault.Enabled` | `.env.orchestration:22-23`, `secretstore.yaml:9-12`, `appsettings:218` |
| Dapr store adları | `.env.<svc>` `DAPR_*_STORE_NAME` (bileşen `metadata.name`'iyle eşleşir) | `.env.orchestration:38-42`, `state.yaml:4` |
| Host portu | `domains/<d>/.env` `VNEXT_*_PORT` (offset'li) → compose `${VNEXT_*_PORT:-default}` | `domains/discovery/.env:30-34`, `docker-compose.yml:71` |
| Dapr sidecar portu | `domains/<d>/.env` `DAPR_*_PORT` (offset*100) → compose `command` | `create-domain.sh:30-41`, `docker-compose.yml:154-155` |
| OTEL/telemetry | `.env.<svc>` `OTEL_*` + appsettings `Telemetry` | `.env.orchestration:43-45`, `appsettings:59-135` |
| Notification hedefi | `orchestration/dapr/components/vnext-notification-*.yaml` `url` | `vnext-notification-email.yaml:10` |
| Bir servisin var mı/portu | `docker-compose.yml` servis bloğu (`container_name`, `image`, `ports`) | `docker-compose.yml:53-72` |

## 4. Templating / adlandırma desenleri
- **Placeholder:** `{{DOMAIN_NAME}}`, `{{DB_NAME}}`, `{{PORT_OFFSET}}`, `{{VNEXT_*_PORT}}`,
  `{{DAPR_*_PORT}}` — `create-domain.sh:87-104` sed ile doldurur. Ek olarak `vNext_[a-zA-Z0-9_]*`
  regex'i tüm DB adlarını `${DB_NAME}`'e çevirir (`:105`).
- **Port şeması:** app portları `4201..4204 + PORT_OFFSET`, init `3005 + offset`; Dapr portları
  `421xx..461xx + offset*100` (`create-domain.sh:24-41`). Rezerve: `core`=offset 0, `discovery`=offset 5;
  özel domain'ler ≥10 (README.md:87-97). Her app container-içi **5000** dinler.
- **Compose proje adı:** her domain `-p vnext-<domain>` (`Makefile:210`); container adları
  `<servis>-${DOMAIN_NAME:-core}` (`docker-compose.yml:22,54`).
- **DB adı normalizasyonu:** `core`→`vNext_Core`, `user-management`→`vNext_User_Management`
  (README.md:78-82). ⚠️ İki farklı impl: `create-domain.sh:44` (tek-kelime, ilk harf büyük) vs
  `Makefile change-domain:551` (kelime-başı büyük); `db-create:356` awk sadece ilk harfi büyütür.
- **Dapr bileşen adı ↔ store adı:** `metadata.name: vnext-state` (`state.yaml:4`) ≡ env
  `DAPR_STATE_STORE_NAME=vnext-state` (`.env.orchestration:39`); ikisi eşleşmezse motor bileşeni
  bulamaz.
- **Dapr sidecar app-id:** `vnext-<servis>-${DOMAIN_NAME}` (`docker-compose.yml:41,151,171,213,257`);
  `ExecutionApi.AppId` appsettings'te buna referans verir (`appsettings:182-184`).

## 5. Servis-başına Dapr bileşen envanteri (fark önemli)
Her `daprd` sidecar YALNIZCA kendi servisinin `components/` dizinini mount eder:
- **db-migrator:** `config`, `lock`, `secretstore` (state/pubsub YOK — sadece migration).
- **execution / worker-inbox / worker-outbox:** `config`, `lock`, `pubsub`, `pubsub-broadcast`,
  `secretstore`, `state` (6 bileşen).
- **orchestration:** yukarıdaki 6 + `vnext-notification-email/sms/state` (bindings.http) — notification
  binding'leri SADECE orchestration'da (doğrulandı — dizin listesi, 2026-07-02).

## 6. Test / build / CI
- **Test YOK, build YOK:** repo derlenmez; imajlar dışarıda (burgan-tech/vnext CI) üretilir. Bu repoda
  `.csproj`/`package.json`/test klasörü yoktur (doğrulandı — `git ls-files`, 2026-07-02).
- **CI YOK:** `.github/workflows` yok. Doğrulama = `make dev` sonrası `make health` (canlı smoke).
- **"Build" kavramı** burada Docker imaj çekme/başlatmadır (`make build` = `compose build`,
  `Makefile:164-167`; pratikte imajlar `ghcr.io`'dan `pull` edilir, `make update` `Makefile:447-451`).
