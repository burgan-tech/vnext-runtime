# Timer Task

Timer Task, iş akışında zamanlayıcı işlemleri gerçekleştiren sistem görevidir. Bu görev türü sadece sistem tarafından kullanılır ve manuel olarak tanımlanamaz. Belirli süre bekleme, schedule işlemleri ve timeout yönetimi için kullanılır.

:::warning Sistem Görevi
Bu görev türü sadece sistem tarafından dahili olarak kullanılır. Manual tanımlanamaz.
:::

## Workflow Timeout Pipeline (v0.0.50+)

Workflow seviyesinde bir timeout tetiklendiğinde, sistem artık timeout hedef state'i için tam **TransitionPipeline**'ı çalıştırır. Bu, `onExit` task'ları, state değişimi, `onEntry` task'ları, finish işleme, zamanlanmış transition kaydı ve otomatik transition'ları kapsar. Daha önce `FlowTimeoutJobHandler` doğrudan state mutasyonu yapıyor ve hiçbir pipeline adımını çalıştırmadan instance'ı tamamlanmış olarak işaretliyordu.

> **Referans:** [#514](https://github.com/burgan-tech/vnext/issues/514)

## Dinamik Timeout Mapping (v0.0.50+)

Workflow seviyesinde timeout, `ITimerMapping` scriptleri ile dinamik timeout süresi hesaplama için isteğe bağlı bir `mapping` alanını destekler. Mapping script'i runtime'da `ScriptContext` kullanılarak değerlendirilir (instance data, workflow metadata, headers erişilebilir). Script başarılı olursa döndürülen `TimerSchedule` (DateTime veya Duration) kullanılır. Başarısız olursa statik `timer.duration` yedek olarak kullanılır. `mapping` tanımlandığında statik `timer` **zorunludur**.

**Tanım örneği:**

```json
"timeout": {
  "key": "$timeout",
  "target": "timed-out",
  "versionStrategy": "None",
  "timer": { "reset": "false", "duration": "PT1H" },
  "mapping": {
    "location": "./src/TimeoutMapping.csx",
    "code": "<BASE64>"
  }
}
```

**Loglama:**

| Olay | Seviye | Açıklama |
|------|--------|----------|
| `TimeoutMappingResolved` | Info | Mapping script'i başarılı; dinamik schedule kullanıldı |
| `TimeoutMappingFallback` | Warning | Mapping script'i başarısız; statik timer.duration yedek olarak kullanıldı |

> **Referans:** [#524](https://github.com/burgan-tech/vnext/issues/524)