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

:::info[SubProcess vs SubFlow]
**SubProcess**: Ana workflow'u bloklamaz ve paralelde izole şekilde çalışır. Ana workflow'a paralel çalışır ve fan-in gibi akış paternlerini gerçeklemek için kullanılır.

**SubFlow**: Ana workflow'u bloklar ve subflow, ana workflow üzerinden organize edilir. SubFlow tamamlanmadan ana workflow devam edemez.
:::

## State Yaşam Döngüsü

State machine aşağıdaki yaşam döngüsünü takip eder:

![State Machine LifeCycle](https://kroki.io/mermaid/svg/eNp9U89v2jAUvu-veNLOqKNVpYrDJJoECC0Bjag9WBzc8JJYDU5lO2xo3v8-xw7Ei1hzsCJ_P973nu28qn9mJRUK0vALmG9KUkG5ZIrVHFLBigIF7ncwGn2HR7JVVCFs6oplJwhKzN7lzsoeLUG_0IrtNQQkaIRAblx7szWPfmHWtP8plQNhzI9OGiXhmERC1GJyrvPC6oq2MqewS2Bl4aWOC9aWYKpzDi0l6jIHJeUFOiSyyIykVBToabkSDKVXZWaJ89-OkZ4-0DX9p6fMXf4Z40yWGhYk5lJRnqG1beQEgvrwUaEyQ_QF2-ZtZmavISZuLEbgtv6hxdwMj1Y3MVcoDrhnJoiGJZk2qvaHa2N5yReu-SS8Ja-1eM-N7zBIbClPpCs7hJ8svOwtly7RsLKZuFRSw_OljwFj56uTeghrWJFtVuK-qfCzjp6twbTfWHWDvKI9Z0rIK2UK8lrAFdrOtzHBrlA0rLv7M80UOyKMoHVkvPCSJc4hZQeEH0hbFw2b_lj_V3kz7GjtjFaUN-bIo6N7QPYJap8n1cnYTSFnVTX5iuP8PkcPmHdAnud3-M0DFmfFQ36PDx7Q3pNPsPHFEN8Q_wJFGEZX)

### Yaşam Döngüsü Adımları

1. **State Policy Kontrolleri**
   - State'de tanımlı transition'lar kontrol edilir
   - Client sadece manuel ve event transition'ları tetikleyebilir
   - Auto ve schedule transition'lar sadece sistem tarafından çalıştırılır
   - Bir transition aynı state'i tekrar target olarak seçemez

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
   - **SubFlow**: SubFlow/SubProcess çalıştırılır

7. **Auto Transition'lar**
   - Otomatik transition'lar çalıştırılır

8. **Schedule Transition'lar**
   - Zamanlanmış transition'lar çalıştırılır

## State Bileşenleri

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
  "stateType": "Initial|Intermediate|Finish|SubFlow",
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






