# Interface'ler

Platform üzerinde tanımlara betik tabanlı kod yazımında kullanılan temel arabirimlerdir. Geliştirilecek betikler için kullanılan teknoloji [Roslyn](https://github.com/dotnet/roslyn) ürünüdür.

## Kaynak Kod Referansları

Interface tanımları aşağıdaki dosyalarda bulunmaktadır:
- **Ana Interface'ler**: [`/src`](./src/) klasörü
- **Dokümantasyon Kopyaları**: [`src/`](src/) klasörü
- **ScriptContext ve ScriptResponse**: [`../src/Models.cs`](../src/Models.cs)

> **Not**: Bu dokümandaki kod örnekleri referans amaçlıdır. Güncel tanımlar için yukarıdaki kaynak dosyaları kontrol edilmelidir.

## Interface Türleri

### IMapping
Genel mapping arayüzüdür. Task'ların input ve output binding'leri için kullanılır.

> **Kaynak**: [`../src/IMapping.cs`](../src/IMapping.cs)

**Kullanım Alanları:**
- Task execution öncesi input data hazırlama ve dönüştürme
- Task execution sonrası output data işleme
- Data validation ve transformation
- Audit logging ve metadata yönetimi

**Metodlar:**
```csharp
Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context);
Task<ScriptResponse> OutputHandler(ScriptContext context);
```

**Metod Açıklamaları:**
- `InputHandler`: Task çalıştırılmadan önce input verilerini hazırlar, WorkflowTask objesini konfigüre eder
- `OutputHandler`: Task çalıştırıldıktan sonra output verilerini işler ve workflow instance'a merge eder

### ITimerMapping
Schedule mapping için kullanılır. Timer tabanlı iş akışları ve zamanlama işlemleri için özel arayüz.

> **Kaynak**: [`../src/ITimerMapping.cs`](../src/ITimerMapping.cs)

**Kullanım Alanları:**
- DateTime tabanlı zamanlama
- Cron expression ile periyodik işlemler
- Duration tabanlı geciktirme
- Immediate execution (anında çalıştırma)
- Business logic tabanlı zamanlama hesaplamaları

**Metod:**
```csharp
Task<TimerSchedule> Handler(ScriptContext context);
```

**Metod Açıklaması:**
- `Handler`: Script context'e göre timer schedule hesaplar ve TimerSchedule objesi döner

### ISubProcessMapping
Sub process için input binding'i için kullanılır. Bağımsız çalışan alt süreçlerin başlatılması için.

> **Kaynak**: [`../src/ISubProcessMapping.cs`](../src/ISubProcessMapping.cs)

**Kullanım Alanları:**
- Background data processing
- Audit log generation
- External system notifications
- Data synchronization
- Fire-and-forget operations

**Metod:**
```csharp
Task<ScriptResponse> InputHandler(ScriptContext context);
```

**Metod Açıklaması:**
- `InputHandler`: Subprocess başlatmak için gerekli input verilerini hazırlar ve ScriptResponse döner

**Not:** Subprocess'ler bağımsız çalıştığı için sadece input binding gerekir, output binding yoktur.

### ISubFlowMapping
Sub flow için input binding'i ve tamamlandığında bir üst flow datası aktarmak için output binding için kullanılır.

> **Kaynak**: [`../src/ISubFlowMapping.cs`](../src/ISubFlowMapping.cs)

**Kullanım Alanları:**
- Approval workflows
- Data processing workflows
- Validation processes
- Computed value generation
- Parent-child workflow integration

**Metodlar:**
```csharp
Task<ScriptResponse> InputHandler(ScriptContext context);
Task<ScriptResponse> OutputHandler(ScriptContext context);
```

**Metod Açıklamaları:**
- `InputHandler`: Subflow başlatmak için input verilerini hazırlar ve ScriptResponse döner
- `OutputHandler`: Subflow tamamlandığında sonuçları parent workflow'a aktarır ve ScriptResponse döner

**Not:** Subflow'lar parent workflow ile entegre çalıştığı için hem input hem output binding gerekir.

### IConditionMapping
Auto transition gibi karar kısımları için kullanılır. Otomatik geçişlerde koşul kontrolü sağlar.

> **Kaynak**: [`../src/IConditionMapping.cs`](../src/IConditionMapping.cs)

**Kullanım Alanları:**
- Data validation checks
- Business rule validation
- Time-based conditions
- External system status checks
- User role/permission verification
- Auto-transition decision logic

**Metod:**
```csharp
Task<bool> Handler(ScriptContext context);
```

**Metod Açıklaması:**
- `Handler`: Verilen context'e göre boolean değer döner (true: geçişe izin ver, false: geçişi engelle)