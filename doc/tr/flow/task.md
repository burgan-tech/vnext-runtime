# Task (Görev) Belgesi

Görevler (Tasks), iş akışı çalışma zamanında belirli işlemleri gerçekleştiren bağımsız bileşenlerdir. Her görev kendi özel amacına göre farklı türlerde tanımlanabilir ve iş akışının farklı noktalarında çalıştırılabilir.

:::highlight green 💡
Görevler bağımsız bir iş akışı olarak saklanır. Kullanılacakları noktaya referans olarak tanımlanırlar.
Her bir etki alanı dağıtımında `tasks` adında bir iş akışı oluşur. Domain içerisinde kullanılan tüm görevler bu iş akışında birer kayıt örneğidir.
:::

## Görev Türleri

Sistem şu anda 11 farklı görev türünü desteklemektedir:

| Görev Türü | Açıklama | Detay Belge |
|-------------|----------|-------------|
| **DaprService** | Dapr service invocation çağrıları | [📄 DaprService README](./tasks/dapr-service.md) |
| **DaprPubSub** | Dapr pub/sub mesajlaşma | [📄 DaprPubSub README](./tasks/dapr-pubsub.md) |
| **Http** | HTTP web servis çağrıları | [📄 Http README](./tasks/http-task.md) |
| **Script** | C# Roslyn script çalıştırma | [📄 Script README](./tasks/script-task.md) |
| **Condition** | Koşul kontrolü (sadece sistem) | [📄 Condition README](./tasks/condition-task.md) |
| **Timer** | Zamanlayıcı görevleri (sadece sistem) | [📄 Timer README](./tasks/timer-task.md) |
| **Trigger** | İş akışı instance yönetimi ve orkestrasyon | [📄 Trigger README](./tasks/trigger-task.md) |
| **GetInstances** | Başka bir workflow'dan instance'ları çek | [📄 GetInstances README](./tasks/get-instances-task.md) |

## Görev Kullanımı

Görevler diğer modüller tarafından referans verilerek kullanılır. Her görev kullanımında `order`, `task` referansı ve `mapping` bilgileri tanımlanır.

### Örnek Görev Tanımı

```json
"onExecutionTasks": [
  {
    "order": 1,
    "task": {
      "ref": "Tasks/invalidate-cache.json"
    },
    "mapping": {
      "location": "./src/InvalideCacheMapping.csx",
      "code": "<BASE64>"
    }
  }
]
```

### Çalıştırma Sırası
- `order` değerleri kendi aralarında gruplanır
- Aynı sırada olanlar **paralel** çalıştırılır
- Farklı sıradakiler **sıralı** çalıştırılır

### Veri Yönetimi
- Task'ların çalışma sonucunda output data'sı varsa master data'yı patch version olarak yükseltir
- `mapping` alanı ile input ve output binding'i yapılır

### Görev Çalıştırma Noktaları

**İş akışı içinde:**
- `Transition.OnExecutionTasks`: Geçiş tetiklendiğinde çalışır
- `State.OnEntries`: Bir aşamaya ilk girişte çalışır
- `State.OnExits`: Bir aşamadan ilk çıkışta çalışır

**İş akışı dışında:**
- `Functions.OnExecutionTasks`: Platform servisleri içinde çalışır
- `Extensions.OnExecutionTasks`: İş akışı kayıt örneği görevleri

## Standart Görev Yanıtı

Tüm görev türleri aynı standart yanıt yapısını kullanır:

```csharp
public sealed class StandardTaskResponse
{
    /// <summary>
    /// Görev çalıştırmasından dönen veri
    /// </summary>
    public dynamic? Data { get; set; }

    /// <summary>
    /// HTTP tabanlı görevler için status kodu
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Görev çalıştırmasının başarılı olup olmadığı
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Hata durumunda hata mesajı
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// HTTP tabanlı görevler için response header'ları
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Görev çalıştırması hakkında ek metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Görev çalıştırma süresi (milisaniye)
    /// </summary>
    public long? ExecutionDurationMs { get; set; }

    /// <summary>
    /// Görev türü identifier'ı
    /// </summary>
    public string? TaskType { get; set; }
}
```