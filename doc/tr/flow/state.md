# State (Durum)

Bir workflow instance'ının bulunduğu aşamayı veya evreyi temsil eder. **State** (durum) ismiyle anılır.

## State Türleri

State nesnesi aşağıdaki türlerde tanımlanabilir:

### `Initial` (Başlangıç)
- Workflow'un başlangıç durumudur
- Sadece `start instance` ile çalıştırılabilir
- Ara state'ler initial olamaz
- Tüm akışta yalnızca bir adet olmalıdır

### `Intermediate` (Ara)
- İşin yapıldığı orta durumlar
- Birden fazla transition bağlanabilir
- Workflow'un ana iş mantığının yürütüldüğü durumlardır

### `Finish` (Bitiş)
- Workflow'un bitiş durumları
- Bu duruma gelindiğinde instance durumu "Completed" olarak güncellenir
- Workflow'un başarılı bir şekilde tamamlandığını belirtir

### `SubFlow` (Alt Akış)
- Başka bir workflow'u çalıştıran durum
- Ana workflow'u bloklar ve subflow ana workflow üzerinden organize edilir
- SubFlow tamamlandığında, auto ve schedule transition'lardan devam eder
- Workflow'ların tekrar kullanılabilirlik yetkinliğini artırır

## State Alt Türleri (SubTypes)

State SubType'lar, durumlar için ek sınıflandırma sağlar. Özellikle Finish durumlarının sonuç türünü belirtmek ve işlem durumunu takip etmek için kullanışlıdır.

| Alt Tür | Kod | Açıklama |
|---------|-----|----------|
| **None** | 0 | Belirli bir alt tür yok |
| **Success** | 1 | Başarılı tamamlanma |
| **Error** | 2 | Hata durumu |
| **Terminated** | 3 | Manuel olarak sonlandırıldı |
| **Suspended** | 4 | Geçici olarak askıya alındı |
| **Busy** | 5 | İşlem devam ediyor (v0.0.33+) |
| **Human** | 6 | İnsan etkileşimi gerekli (v0.0.33+) |

### Kullanım Örneği
```json
{
  "key": "completed",
  "stateType": 3,
  "subType": 1,
  "labels": [
    {
      "label": "Başarıyla Tamamlandı",
      "language": "tr"
    }
  ]
}
```

### Alt Tür Kullanım Senaryoları

- **Success (1)**: Başarılı workflow tamamlanmasını temsil eden bitiş durumları için kullanın
- **Error (2)**: Hata/başarısızlık sonuçlarını temsil eden bitiş durumları için kullanın
- **Terminated (3)**: Manuel iptal yoluyla ulaşılan bitiş durumları için kullanın
- **Suspended (4)**: Workflow'un geçici olarak duraklatıldığı durumlar için kullanın
- **Busy (5)**: Workflow'un aktif olarak işlem yaptığı durumlar için kullanın (v0.0.33+)
- **Human (6)**: İnsan etkileşimi veya onayı gereken durumlar için kullanın (v0.0.33+)

:::info[SubProcess vs SubFlow]
**SubProcess**: Ana workflow'u bloklamaz ve paralelde izole şekilde çalışır. Ana workflow'a paralel çalışır ve fan-in gibi akış paternlerini gerçeklemek için kullanılır.

**SubFlow**: Ana workflow'u bloklar ve subflow, ana workflow üzerinden organize edilir. SubFlow tamamlanmadan ana workflow devam edemez.
:::

---

## EffectiveState Takibi (v0.0.33+)

v0.0.33 sürümünden itibaren platform, gelişmiş sorgulama ve izleme yetenekleri için instance seviyesinde etkin durum bilgisini takip eder.

### Instance Alanları

| Alan | Tür | Açıklama |
|------|-----|----------|
| `EffectiveState` | string | Mevcut etkin durum adı |
| `EffectiveStateType` | int | Mevcut etkin durum tipi kodu |
| `EffectiveStateSubType` | int | Mevcut etkin durum alt tipi kodu |

### Amaç

Bu alanlar şunları sağlar:
- Workflow'ları mevcut işlem durumlarına göre sorgulama
- İnsan etkileşimi bekleyen instance'ları filtreleme
- Meşgul ve müsait instance'ları tanımlama
- İş yükü ve kuyruk raporları oluşturma
- İç içe subflow'lar arasında workflow durumunu takip etme

### Davranış

- Aktif subflow'u olmayan instance'lar için, `EffectiveState*` alanları instance'ın kendi durumunu yansıtır
- Aktif subflow'u olan instance'lar için, `EffectiveState*` alanları en derin aktif subflow'un durumunu yansıtır
- Değerler state geçişlerinde otomatik olarak güncellenir
- Veritabanı migration'ı başlangıçta otomatik olarak uygulanır

### Sorgu Örnekleri

**İnsan etkileşimi gereken tüm instance'ları bul:**
```http
GET /core/workflows/approval-flow/instances?filter={"effectiveStateSubType":{"eq":"6"}}
```

**Tüm meşgul instance'ları bul:**
```http
GET /core/workflows/order-processing/instances?filter={"effectiveStateSubType":{"eq":"5"}}
```

**Durum filtreleme ile birleştir:**
```http
GET /core/workflows/payment/instances?filter={"status":{"eq":"Active"},"effectiveStateSubType":{"eq":"6"}}
```

### Kullanım Alanları

- **İnsan Görev Kuyrukları**: İnsan onayı bekleyen tüm workflow'ları filtrele ve görüntüle
- **İşlem Takibi**: Şu anda işlem durumunda olan instance'ları takip et
- **İş Yükü Raporları**: Durum türüne göre instance dağılımı raporları oluştur
- **SLA Takibi**: Instance'ların insan etkileşimi durumlarında ne kadar kaldığını izle
- **Kaynak Planlama**: Durum dağılımına göre darboğazları tanımla

> **Not:** Detaylı filtreleme sözdizimi için [Instance Filtreleme Rehberi](instance-filtering.md) bölümüne bakın.

---

## State Yaşam Döngüsü

State machine aşağıdaki yaşam döngüsünü takip eder:

![State Machine LifeCycle](https://kroki.io/mermaid/svg/eNp9U89v2jAUvu-veNLOqKNVpYrDJJoECC0Bjag9WBzc8JJYDU5lO2xo3v8-xw7Ei1hzsCJ_P973nu28qn9mJRUK0vALmG9KUkG5ZIrVHFLBigIF7ncwGn2HR7JVVCFs6oplJwhKzN7lzsoeLUG_0IrtNQQkaIRAblx7szWPfmHWtP8plQNhzI9OGiXhmERC1GJyrvPC6oq2MqewS2Bl4aWOC9aWYKpzDi0l6jIHJeUFOiSyyIykVBToabkSDKVXZWaJ89-OkZ4-0DX9p6fMXf4Z40yWGhYk5lJRnqG1beQEgvrwUaEyQ_QF2-ZtZmavISZuLEbgtv6hxdwMj1Y3MVcoDrhnJoiGJZk2qvaHa2N5yReu-SS8Ja-1eM-N7zBIbClPpCs7hJ8svOwtly7RsLKZuFRSw_OljwFj56uTeghrWJFtVuK-qfCzjp6twbTfWHWDvKI9Z0rIK2UK8lrAFdrOtzHBrlA0rLv7M80UOyKMoHVkvPCSJc4hZQeEH0hbFw2b_lj_V3kz7GjtjFaUN-bIo6N7QPYJap8n1cnYTSFnVTX5iuP8PkcPmHdAnud3-M0DFmfFQ36PDx7Q3pNPsPHFEN8Q_wJFGEZX)

### Yaşam Döngüsü Adımları

1. **State Policy Kontrolleri**
   - State'de tanımlı transition'lar kontrol edilir
   - Client sadece manuel ve event transition'ları tetikleyebilir
   - Auto ve schedule transition'lar sadece sistem tarafından çalıştırılır

2. **Current Transition OnExecutionTasks**
   - Mevcut transition'ın OnExecutionTask'ları çalıştırılır

3. **Current State OnExits**
   - Mevcut state'in OnExit task'ları çalıştırılır

4. **State Değişimi**
   - Current Transition'ın target state'ine geçiş yapılır
   - State değişimi sadece transition'lar üzerinden gerçekleşir

5. **State OnEntries**
   - Yeni state'in OnEntry task'ları çalıştırılır

6. **State Tipi Kontrolü**
   - **Finish**: Instance durumu "Completed" olarak güncellenir
   - **SubFlow**: Sadece SubFlow çalıştırılır

7. **Auto Transition'lar**
   - Otomatik transition'lar çalıştırılır

8. **Schedule Transition'lar**
   - Zamanlanmış transition'lar çalıştırılır

## State Bileşenleri

### SubType (Alt Tür)
- State için ek sınıflandırma sağlar
- Özellikle Finish durumlarında sonuç türünü belirtmek için kullanılır
- Değerler: None (0), Success (1), Error (2), Terminated (3), Suspended (4)

### VersionStrategy
- State'in veri versiyonlama stratejisi
- **Major**: Büyük değişiklikler için
- **Minor**: Küçük değişiklikler için

### Labels
- Çoklu dil desteği için state etiketleri
- Her dil için ayrı label tanımlanabilir

### Transitions
- State'den çıkan geçişler
- Manuel, otomatik, zamanlanmış ve event-based olabilir

### OnEntries
- State'e girildiğinde çalıştırılan task'lar
- State aktivasyonu sırasında yürütülür

### OnExits
- State'den çıkılırken çalıştırılan task'lar
- State'i terk etmeden önce yürütülür

### View
- State ile ilişkilendirilmiş görünüm referansı
- UI bileşenlerinin gösterimi için kullanılır

### SubFlow
- SubFlow türündeki state'ler için alt workflow referansı
- Type, reference ve mapping bilgilerini içerir

## State Şeması

```json
{
  "key": "string",
  "stateType": "1 (Initial)|2 (Intermediate)|3 (Finish)|4 (SubFlow)",
  "subType": "0 (None)|1 (Success)|2 (Error)|3 (Terminated)|4 (Suspended)",
  "versionStrategy": {
    "type": "Major|Minor"
  },
  "labels": [
    {
      "label": "string",
      "language": "string"
    }
  ],
  "transitions": [...],
  "onEntries": [...],
  "onExits": [...],
  "subFlow": {
    "type": "string",
    "reference": {...},
    "mapping": {...}
  }
}
```


