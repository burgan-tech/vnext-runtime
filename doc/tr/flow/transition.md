Bir akış tanımında, state'ler arasındaki geçişleri yöneten bileşenlere **Geçiş** (Transition) denir. Transition'lar state geçişlerini yönetmekle yükümlüdür ve farklı tetikleme türlerine göre çalışabilir.

## Transition Özellikleri

### Temel Özellikler
- **Key**: Transition'ın benzersiz anahtarı
- **From**: Hangi state'den geçiş yapılacağını belirtir (null olabilir)
- **Target**: Hangi state'e geçiş yapılacağını belirler
- **TriggerType**: Transition'ın tetikleme türü
- **VersionStrategy**: Data versiyonu değişiklik stratejisi (Major/Minor)

### İsteğe Bağlı Özellikler
- **Timer**: Zamanlanmış transition'lar için kullanılan mapping kodu
- **Rule**: Otomatik transition'lar için kullanılan koşul mapping kodu
- **Schema**: Transition'da iletilen payload'ı validate etmek için kullanılan şema referansı
- **AvailableIn**: Shared transition'lar için hangi state'lerde çalıştırılabilir olduğunu belirtir
- **Labels**: Çoklu dil desteği için etiketler
- **View**: Transition'ın görünüm referansı
- **OnExecutionTasks**: Transition çalıştırıldığında yürütülecek görevler

## Trigger Türleri (TriggerType)

### Manual (0)
Client tarafından çağrılan transition'dır. Kullanıcı etkileşimi ile tetiklenir.

**Kullanım Alanları:**
- Kullanıcı buton tıklamaları
- Form gönderimleri  
- Manuel onay süreçleri

### Automatic (1)
Sistem tarafından otomatik olarak çalıştırılan koşullu transition'dır. 

**Özellikler:**
- `Rule` alanında mapping ile tanımlanır
- `IConditionMapping` arayüzünü runtime'da compile edip execute eder
- Belirli koşullar sağlandığında otomatik tetiklenir

**Kullanım Alanları:**
- İş kurallarına dayalı otomatik geçişler
- Durum kontrolü sonrası otomatik ilerlemeler
- Veri validasyonu sonrası geçişler

### Scheduled (2)
Zamanlanmış transition'dır. Belirli bir zamanda veya cron benzeri periyodik olarak çalışması istendiğinde kullanılır. Sadece sistem tarafından çalıştırılır.

**Özellikler:**
- `Timer` alanında mapping ile tanımlanır
- `ITimerMapping` arayüzünü runtime'da compile edip execute eder
- Zaman tabanlı tetikleme

**Kullanım Alanları:**
- Periyodik raporlama
- Otomatik yedekleme işlemleri
- Zamanlanmış bildirimlerin gönderimi
- Timeout durumları

### Event (3)
Pub/Sub sistemleri tarafından çağrılan transition'dır. Olay tabanlı tetikleme sağlar.

**Kullanım Alanları:**
- Microservice'ler arası iletişim
- Harici sistem entegrasyonları
- Asenkron işlem tetiklemeleri
- Event-driven architecture implementasyonları

## Shared Transitions
`AvailableIn` özelliği sayesinde, bir transition birden fazla state'de kullanılabilir hale getirilebilir. Bu durumda transition hangi state'lerde çalıştırılabileceği bu liste ile belirlenir.

## Versiyon Yönetimi
`VersionStrategy` özelliği ile transition'ların data versiyonu Major ve Minor olarak değiştirilebilir. Bu, geriye uyumluluk ve veri migrasyonu açısından önemlidir.

## Payload Validasyonu
`Schema` referansı kullanılarak transition'da iletilen veriler validate edilebilir. Bu sayede veri bütünlüğü korunur ve hatalı veri girişleri önlenir.