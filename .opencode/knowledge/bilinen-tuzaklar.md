# vnext-runtime — Bilinen Tuzaklar

> Bu dosya en yüksek halüsinasyon riskli dosyadır. Her madde kanıtlı ya da `DOĞRULANMADI:`
> etiketlidir. "X gibi görünür ama Y'dir" durumları ve yanıltıcı yapılar.

## Motor kodu bu repoda YOK — sadece çalıştırma/config
`vnext-runtime` bir kaynak-kod reposu değildir (0 `.cs/.ts/.dart`; doğrulandı — `git ls-files`,
2026-07-02). Workflow yürütme, task tipleri, `/api/admin`, instance filtreleme gibi HER motor
davranışı `ghcr.io/burgan-tech/vnext/*` imajlarının içindedir (`docker-compose.yml:23,55,123,186,229`).
"Bu workflow nasıl çalışıyor / bu task ne yapıyor" → bu repoda cevap YOK → `DURDU: motor kodu
burgan-tech/vnext'te`.

## Domain bileşenleri (workflow/task/view/…) burada tanımlı değil
`vnext-init` bunları `@burgan-tech/vnext-core-runtime` npm paketinden indirir (README.md:355-370).
Repoda hiçbir workflow/task/schema JSON'u yoktur; sadece bunları YÜKLEYEN altyapı vardır. Bir
bileşen tanımını bu repoda arama.

## config/ altında bağlanmamış servis config'leri var (clickhouse/grafana/prometheus/mockoon)
`vnext/docker/config/` altında `clickhouse/`, `grafana/`, `prometheus/`, `mockoon/` config'leri
bulunur ama güncel `docker-compose.yml`'de bu servislerin HİÇBİRİ tanımlı DEĞİLDİR (doğrulandı —
compose'da yalnızca openobserve+otel gözlem servisleri var; grep `mockoon|clickhouse|grafana|prometheus`
yalnızca otel'in prometheus port yorumlarını buldu, 2026-07-02). appsettings `ClickHouse.Enabled:false`
(`appsettings.Development.json:221`). Bu config'ler ölü/başka-branch kalıntısı olabilir; "clickhouse
çalışıyor" varsayma. **DOĞRULANMADI:** bu config'lerin hangi compose varyantında kullanıldığı (başka
branch'te olabilir).

## README "Services and Ports" mockoon'u (3001) listeler ama compose'da yok
README.md:763 mockoon'u infra servisi olarak gösterir; `Example.ApiBaseUrl=http://localhost:3001`
(`appsettings:234-236`). Ancak `docker-compose.yml`'de mockoon servisi YOKTUR. README, compose'un
gerisinde. Port 3001'de bir mock beklenmesi README varsayımıdır, çalışan servis değil.

## Notification binding'leri `mocklab:5000`'e gider ama `mocklab` servisi yok
`vnext-notification-{email,sms,state}.yaml:10` → `http://mocklab:5000/api/notification/*`
(`orchestration/dapr/components/`). Güncel compose'da `mocklab` adlı servis tanımlı değil ve bu
binding'ler yalnızca orchestration sidecar'ına mount edilir. Ayrıca ad karışıklığı: appsettings
`mockoon`/`3001` derken binding `mocklab`/`5000` der — ikisi farklı ad/port. Notification'ı "çalışır"
kabul etme; mock ucu bu ortamda bağlı değil.

## LEGACY `change-domain` var olmayan dosyalara dokunuyor
`Makefile:539-611` (`change-domain`) şu dosyaları editlemeye çalışır: `.env.inbox`, `.env.outbox`
(`:557`) ve `config/postgres/init-db.sql` (`:584`). Bunların HİÇBİRİ repoda yok — güncel şablonlar
`.env.worker-inbox`/`.env.worker-outbox` adını kullanır ve `config/postgres/` dizini yoktur.
`change-domain` eski tek-domain modelinden kalma, kısmen bozuk. Çok-domain için doğru yol
`create-domain` + `up-vnext DOMAIN=`'dir (README.md:163-171 "Legacy" uyarısı). `change-domain`'i
canlı akış sanma.

## Portlar: host ≠ container (hepsi container-içi 5000)
Her app servisi container'da **5000** dinler; host portu domain'e göre değişir
(`${VNEXT_APP_PORT:-4201}:5000`, `docker-compose.yml:71`). Compose healthcheck'leri `localhost:5000`
kullanır (`:65`), Makefile `health` host portlarını kullanır (`Makefile:331-341`). "4201" mutlak
değildir; `PORT_OFFSET`'e göre 4211/4221… olur.

## İki farklı DB-adı normalizasyonu — çok kelimeli domain'de ayrışır
`create-domain.sh:44` tek awk ile sadece ilk harfi büyütüp geri kalanı korur; `Makefile change-domain:551`
her kelimeyi (`_` ayraçlı) büyütür; `db-create:356`/`db-drop:365` awk yalnızca ilk harfi büyütür.
Tek-kelimeli domain'de üçü de aynı (`core`→`vNext_Core`) ama çok-kelimelide README `vNext_User_Management`
derken (`README.md:81`) `create-domain.sh`/`db-create` `vNext_User-management` benzeri farklı sonuç
verebilir. **DOĞRULANMADI:** çok-kelimeli domain adında create-domain ile db-create'in ürettiği DB
adının bire bir eşleştiği (offset'li canlı denenmedi) — çakışırsa app DB'yi bulamaz.

## `orb.local` host adları OrbStack'e özgü
`appsettings.Development.json:22` → `vNextApi.BaseUrl=http://vnext-app.vnext-<domain>.orb.local`.
Bu DNS OrbStack'e özgüdür (Makefile OrbStack'i ayrıca tespit eder, `:16-17`). Docker Desktop/Podman'da
bu host çözülmeyebilir → **DOĞRULANMADI:** OrbStack dışı runtime'da bu URL'nin çalıştığı.

## Tüm imaj etiketleri `latest` (VAULT hariç)
`.env:6-16` ve `templates/.env:11-27`: `VNEXT_VERSION`, `DAPR_*`, `POSTGRES_VERSION` … hepsi `latest`;
tek sabit `VAULT_VERSION=1.13.3`. README uyarısı: sürüm geçişinde ortam sıfırlanıp yeniden kurulmalı
(README.md:7). "Şu sürüm çalışıyor" varsayma; etiket kayan hedeftir.

## Gizli değerler DEV varsayılanı — ama yine de rapora yazma
Postgres kullanıcı/şifresi, Vault token'ı gibi değerler tracked dosyalarda DEV sabiti olarak durur
(`docker-compose.yml`, `.env.orchestration`, `secretstore.yaml`). Bunlar prod sırrı değil ama kural
gereği raporda değerini YAZMA; yalnızca `dosya:satır` referansı ver (örn. "Vault token
`.env.orchestration:23`'te").

## context7.json bir config değil, doküman-indeksleme meta'sı
`context7.json` yalnızca Context7 portal URL'si + public key içerir (`context7.json:1-3`); runtime
davranışını etkilemez. Servis config'i sanma.
