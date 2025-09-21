
# Versiyon Yönetimi ve ETag Kullanımı

Amorphie platformunda tüm iş kaydı örnekleri versiyonlar halinde yönetilebilir. Versiyonlamada [semantik versiyonlama](https://semver.org/) yaklaşımı benimsenmiştir. Versiyon standart olarak `minor.major.patch.revision` formatındadır.

Kaydın versiyon güncellemesi akış tanımlarında belirlenir. `VersionStrategy` adında özellikle:
- Her transition geçişinde versiyon değişimi belirlenebilir.
- Her state girişi ve çıkışı için versiyon değişimi belirlenebilir.

<DataSchema id="5391667" />

Versiyonlama dışında kayıt değişiklikleri [ETag](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/ETag) yaklaşımı ile de yönetilir. ETag değeri her zaman `ULID` olarak üretilir.

ETag değeri aynı zamanda istemci tarafında önbellekleme için de kullanılabilir.