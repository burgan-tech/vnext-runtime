# vnext-runtime — Kritik Akışlar

Bu dosya `vnext-runtime`'ın en sık sorulan uçtan-uca operasyon akışlarını, giriş noktasından
(Makefile/script) dış uca kadar `dosya:satır` kanıtıyla verir. Her akış kendi başına bağımsızdır.
Not: vNext motorunun kendisi `ghcr.io/burgan-tech/vnext/*` imajlarındadır; buradaki akışlar o motoru
**ayağa kaldırma/konfigüre etme/besleme** akışlarıdır.

---

## 1. `make dev` — sıfırdan lokal ortam
1. `make dev` → `setup` + `up-build` + `health` (`Makefile:393-397`).
2. `setup` → `create-env-files` (yoksa `vnext/docker/.env`'i imaj sürümleriyle üretir,
   `Makefile:104-128`) + `create-network` (`vnext-development` docker ağı, `:135-139`).
3. `up-build` → `--profile infra --profile vnext up -d --build` (`Makefile:216-221`); Compose
   external ağ `vnext-development`'i kullanır (`docker-compose.yml:415-417`).
4. Başlatma sırası (Compose `depends_on` + healthcheck ile): postgres healthy → vnext-db-migrator
   `completed_successfully` → vnext-app healthy → vnext-init healthy → vnext-component-publisher
   (`docker-compose.yml:73-75,95-98,117-119`).
5. `health` → domain `.env`'inden portları okuyup `curl .../health` atar (`Makefile:324-348`).
- **DIŞ UÇ:** app/health davranışı motordadır (imajlar).

## 2. Yeni domain oluşturma (`create-domain`)
1. `make create-domain DOMAIN=sales PORT_OFFSET=10` → `./create-domain.sh sales 10`
   (`Makefile:486-488`).
2. Script portları offset'le hesaplar (app `4201+offset`, Dapr `421xx+offset*100`,
   `create-domain.sh:24-41`) ve DB adını türetir (`vNext_Sales`, `:44-45`).
3. `templates/` altındaki her `.env*` ve `appsettings.*.json`'ı `process_template` ile okur,
   `{{PLACEHOLDER}}`'ları ve `vNext_*` regex'ini sed'le doldurup `domains/sales/`'e yazar
   (`:80-125`).
4. Sonuç: `domains/sales/` altında 6 `.env` + 5 `appsettings` dosyası. Ardından
   `make db-create DOMAIN=sales` + `make up-vnext DOMAIN=sales` (README.md:109-112).
- **NOT:** `templates/` KAYNAK, `domains/<d>/` ÜRETİLMİŞ kopyadır — kopyayı elle düzenlemek bir
  sonraki `create-domain`'de kaybolur (yeniden üretilirse).

## 3. Bir domain'i başlatma (`up-vnext`)
1. `make up-vnext DOMAIN=core` domain dizinini kontrol eder; yoksa `create-domain` ister
   (`Makefile:196-201`).
2. Altyapı çalışmıyorsa `vnext-postgres` container'ını kontrol edip `up-infra` çağırır (`:202-206`).
3. `domains/core/.env`'i `set -a; . .env; set +a` ile source'lar, sonra
   `$(COMPOSE_CMD) -p vnext-core --env-file ./domains/core/.env --profile vnext up -d` (`:209-210`).
   `-p vnext-core` proje izolasyonu → aynı compose ile birden çok domain yan yana çalışır.
4. Her app servisi kendi Dapr sidecar'ıyla ağ namespace'i paylaşır
   (`network_mode: service:<app>`, `docker-compose.yml:161,181,224,267`).

## 4. Bileşen yayınlama (init + publisher)
1. **vnext-init** (imaj `ghcr.io/burgan-tech/vnext/init`) vnext-app healthy olunca çalışır
   (`docker-compose.yml:76-98`): `@burgan-tech/vnext-core-runtime` npm paketini indirir,
   core klasöründen Extensions/Functions/Schemas/Tasks/Views/Workflows'u okur, her JSON'daki
   `domain` değerini `APP_DOMAIN` ile değiştirir, `vnext-app/api/admin`'e POST eder (README.md:355-370).
2. **vnext-component-publisher** (`alpine/curl`) vnext-init healthy olunca
   `publish-component.sh --skip-health --url http://vnext-init:3000` koşar
   (`docker-compose.yml:99-120`).
3. Script `VNEXT_COMPONENT_VERSION` + `APP_DOMAIN`'i `.env`'den yükler (`publish-component.sh:42-79`),
   vnext-init sağlığını bekler (atlanmadıysa `:82-100`), `/api/package/runtime/publish`'e
   `{"version":..,"appDomain":..}` POST eder (`:20,149-153`), yanıttaki `success`/`failed`'i parse eder
   (`:110-134`).
4. Elle yeniden yayın: `make republish-component DOMAIN=x` (publisher container'ı silip yeniden
   koşar, `Makefile:474-483`).
- **DIŞ UÇ:** hem indirilen bileşen paketi hem `/api/package/runtime/publish` işleyişi bu repoda
  değil (bileşen paketi + motor).

## 5. Veritabanı yaşam döngüsü (domain başına)
1. `make db-create DOMAIN=core` → domain adını normalize edip `vNext_Core` DB'sini `psql` ile
   oluşturur (varsa atlar); infra profilindeki `postgres` container'ına `exec` eder
   (`Makefile:355-362`).
2. `db-drop` (5 sn beklemeli, yıkıcı `:364-371`), `db-reset` (drop+create `:373-375`),
   `db-connect` (`psql -d vNext_Core` `:382-386`), `db-list` (`vNext_%` `:388-390`).
3. Migration'ı **vnext-db-migrator** çalıştırır (imaj içinde); app servisleri migrator
   `completed_successfully` olmadan başlamaz (`docker-compose.yml:73-75`).
- **DIŞ UÇ:** migration SQL'i/şema motorun db-migrator imajındadır.

## 6. Gözlemlenebilirlik (telemetry) akışı
1. Servisler OTLP'yi `http://otel-collector:4317`'e yollar (`.env.orchestration:43`,
   `appsettings.Development.json:66-68`).
2. Dapr sidecar'ları da tracing'i aynı collector'a verir (`orchestration/dapr/config.yaml:7-10`,
   `samplingRate: "1"`).
3. otel-collector → openobserve (UI `http://localhost:5080`, `docker-compose.yml:380-413`;
   erişim README.md:783-786).
- **NOT:** appsettings `Telemetry.Logging` gövde yakalama açık (`appsettings:127-133`,
  `EnableRequestBody:true`) ve `sub`/`role`/`jti` gibi header'ları enrich eder (`:115-124`) — log
  içeriği PII taşıyabilir; maskeleme bu repoda YOK, platform tarafındadır.
