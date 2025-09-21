# Zamanlanmış Ödemeler İş Akışı

## Genel Bakış

Bu belge, VNext Runtime platformu kullanılarak geliştirilen kapsamlı zamanlanmış ödemeler iş akışı implementasyonunu açıklamaktadır. İş akışı, otomatik yeniden deneme mekanizmaları, bildirim sistemleri ve gelişmiş ödeme zamanlama yetenekleri ile birlikte tekrarlayan ödemeleri yönetmek için tasarlanmıştır.

## Mimari

![Payment Architecture](https://kroki.io/mermaid/svg/eNqNVVtP2zAUfu-vsIqYiuRO0IoBfUDaCkx7YKrIpj1EfTCO3VqEpIqdQaX-eE58i5206_pQ2yff-c7Vx6uKbNbo17cBgp-sn1f6PHwkokB_yuqF5-UbGqOErllW5yxDC7J9ZYWSQ63R_BpsokilRqlelmdoPL5F87LgYpVavD3WFVGiLJZe2Yi1wleqxF_mFcyxRZqzod7SnM3XjL609I0IaVlA7nGBgX2ED6IQEiL0dE6w1_wdI7GrTtCinWSfXcetvzXJuy-yUQp_yzONYUWm105JkvpZFyOBcjjDi6qkTAa1sKCwHA4kipWtjBUswBAIfRT23AYR40LV-3dG66aQqRU4j3rKHhmqJzVtltSu_6f0QAT0X2qWnooRh_gnpqptqv97aC3dG1AXaV3UWJvcTrn-6cScFJTljd9-13PGf-kYOdIMG5d56IefpRJcUH23omawoKgfQrDtiO9M_ZasSmFFzQb9KHjZOmo_G__AI80A5YNtxBZUsoUZrcfEwGFzELSo5dqgml0Ae0xcamw8-0oAKn1UP4enpxCcYisziNCiFDDMBuEN_zy-3Q3nJM-lKwYaJWfDXXS5BsFtAztWC_qqrgqJVAnw4NoPutMotuFKOVpYM2HZBnEpDxhryaNQ74gi6AF8HMQzbDf0swsQx2IL8U9M1rnaE15IrrvlE-rbOBZYoxx2FAKkquXhABO1zWE46SPNiZR3jKNXGKr-8eIiz2cnbMonPMNSVeULm51c3Fx9ySb2OH4TmVrPJpv3mEba6hsGfsGv2Y1nmF5fsyk9zuBqa0k4n7JzT8Ivr-j5-RESEbSrU7yhk6vnjuJ08-4EGZFrUlVkO0OX6LJNmOa0by42BcNtYrF7mbB_vsJMBgzxy4C7QxTHwxNH4xGHMxj3ZqBNemDMTh_cTgsMEwHrC98m-AMbOM18)


### Scheduled Payments Main Flow
![Ana Akış](https://kroki.io/mermaid/svg/eNp1VUtrGzEQvvdXCEogBjltUgyhhJRgN9BDi_Emh2J8ULSjWFTeNbvaFGP3v1erx2pWVnyQrdHMN49vZixU_ZdvWaPJ0-IDMZ9Cm8vl2n6RJTvsoNKkAN3tNxMynd6TeV0J-boOT-7aNUzLurp7aT7dF-wNSPZ5Yz3Yw8kRYNFxDm17HBsOgOW3f3m7RyYVlMfzMNyDN0O23lEPcfoN7Yk8cC3fYEjIXS2E_cl0zKbgWyg7BRuE5_xEOCeNXh2ejfgnqzqmFsA87tEJrK8o9SEju2VT9yHP691egTEL4Xi5tQ-P59YPSg2W5ndIJmtmjzTMmJuXxWIFQZLCOwVL8kA1O3AF8y3wP7GvehGxMgv-q9ZSSG75JUX3sndYDhilGEEfZSVb436ADALHbcO3EjXqCnjdlKhDQ2qIuVWWuVXKXMbyeV8mVk7yvsUCLGGjDskTtcoQ5ehHSs5dtkuxv6jwBM1OVsYm1i-K8CAP1IWh1LLqwIqP4Raq7Fquf4rjHNWz4dsjMGddfK_K68u1Ocl0eNhMrF6MMGjeBE0U_CQCX1ygXiI_Kg2vboWMhujKxIXUilPoZCMUZoGucwPp3zZnVUrQlieCezuFxG8Bd9T7Qx76oGTlKOWKte0CBGn7Nd4XQEilvn6EazETMFYJcTgN8QVmYjbWKIHLth87ryKM0ufET8zHe7oVM7iNAVpN9-dCewL742YID6m4xqSu9DQMBo0FpIFzitj2zs9w_LKneFXTdL_RZC9RtE5oOmMUzxPFs0PHzRyqhmIadw3N0I4K-R9iB5AK)

### Payment Process SubFlow

![Sub Flow](https://kroki.io/mermaid/svg/eNp9VG1r2zAQ_r5fIRiFBhS2dgt0Y3SUpoPBNkKTMUbIB9c-paKuPCR7aXD33yef3k5esnxwdKfneU53Op2om115X-iWreYvmP0tdFOCMcvW-k7X-McWxf4RVBv2pNpuJmw6vWQLUJW11hHg7A0qOTnnQfSyvIeqq6Gad9BH48OdfnUZBOzOxz__EL8Wqitqi_nW7HpnBBazLs_AD40xcJ9X8hH0czi6j7P2ZsjMqeU-tirMg8uExkfRn2DGkiTlzO_q5NbLrhx2ehrV-0LWR7k3Wjc6Y6KH5p5HSQf1DnuXLcSr8k53hmFnQzVQOyl8KqQtaS7gfGM-fggcU7jq2uYWWr3vhxXDJb12n8SY5sruiG7NvhvQyFxpud2CNk7sqMDTsC3BWP4T82vk30IxdMoR4o9GPwj7MuYW1crf1l1ZBaniBmqQXXoNMdtUPzTz8qUifFalhuRj102nWtC09Y7KeUxIM0GuC1VCbZOKAaPHCR9I8QA7ZZViYoXQ_NJsZdmnTND2pUgAlL2qd8X-P4-G9qhrewe8UdXZ6dp-2TRgNhNkxDOO4OcBnhKepDgnJ2zZ7ms7WdAs68KYOQhmhkE38ISs6_cv4UzMBOSQX34-OIR4AzMxyxEVlNLIRgWIsKDXozj-cfowF2IGFzlCYC9GCbiD0Tk0NkMM8a58m9JDUDbDOSkkWZ_HjCnPzVyeXxInV-9rQDh04HI6KHk-jjidLDy-EU4anKdG5gfak5OWCpWmB6Ed5MtMtskT56l1XLH_AtJPXbU)

### Notification Sub Process

![Sub Process](https://kroki.io/mermaid/svg/eNp9U1FLwzAQfvdXHMhgww6VuQdFJuJEfFCG3R6k7KFrLy6YtaPJHGPzv5tc2zShxT6kvbvvvvvucmUi3yfruFAwn56BfsLdKlTa7kf0gvdcccaTWPE8g1mRJyjlcgDD4QReUC0kFq8ZyyP9DcYAYxUbgt-visuJF4B5LL-XVIcOh4EYa-MDVcHxB9OjTSUy63_4bUhaSYbq9InyBCFmqduAjIzH60kScfgWwgXMdnLtqGtlk0YNNV2UTLP4sMFMEYc3KY1a_kNiKnWzmIgn0BVUliYGj1OzqGNHV9pdTaou2J3blGiFaJiPYh8f9Dyf8s1WoMKoayuoZI1wVNeucnq71XOW9iN9wNBfLps6aHJ7PQjVQfDsi8xExFJOkYE0u2k4GBfi7hyv2ZihD9mWqioEG-GYjX2EMvOowozdJjd-OMWESyPMQkZ41UgjnP1dgrIxK8yBOEsetLYhsNOp9Lrc5XUH9u6MYCfeWvygfXt1E3-AJkyd)

Zamanlanmış ödemeler iş akışı, aşağıdaki temel bileşenlerle bir durum makinesi olarak implementasyonu yapılmıştır:

### Ana İş Akışı Durumları

1. **Ödeme Konfigürasyonu** - Ödeme planı ayarlarını kaydeder
2. **Aktif Durum** - Subflow (Alt Akış) ile ödeme işleme sürecini başlatır  
3. **Deaktif Durum** - Ödeme planını geçici olarak durdurur
4. **Ödeme Döngüsü Kontrolü** - Subprocess (Alt Süreç) ile bildirim gönderir
5. **Tamamlandı** - Ödeme planı başarıyla tamamlandı
6. **Sonlandırıldı** - Ödeme planı manuel olarak iptal edildi

### Desteklenen Özellikler

- **Zamanlanmış Ödemeler** - Günlük, haftalık, aylık, üç aylık, yıllık
- **Otomatik Yeniden Deneme** - Başarısız ödemeler için konfigüre edilebilir yeniden deneme
- **Bildirim Sistemi** - SMS ve Push bildirimleri
- **Döngüsel İşleme** - Timer tabanlı zamanlanmış tetikleme
- **Manuel Müdahale** - Kullanıcı tarafından yönlendirilen işlemler

## İş Akışı Bileşenleri

### Görevler

İş akışı, harici servislerle etkileşim kurmak için aşağıdaki HTTP görevlerini kullanır:

| Görev | Endpoint | Amaç |
|-------|----------|------|
| `save-payment-configuration` | `/api/payments/schedules` | Ödeme planı konfigürasyonunu kaydeder |
| `activate-payment-schedule` | `/api/payments/schedules/activate` | Ödeme planını aktifleştirir |
| `deactivate-payment-schedule` | `/api/payments/schedules/deactivate` | Ödeme planını deaktifleştirir |
| `process-payment` | `/api/payments/process` | Ödeme işlemini gerçekleştirir |
| `increment-retry-counter` | `/api/payments/retry-counter` | Yeniden deneme sayacını artırır |
| `get-user-info` | `/api/users/{userId}` | Kullanıcı bilgilerini getirir |
| `send-payment-notification-sms` | `/api/payments/notify/sms` | SMS bildirimi gönderir |
| `send-payment-push-notification` | `/api/payments/notify/push` | Push bildirimi gönderir |
| `archive-payment-record` | `/api/payments/schedules/archive` | Ödeme kaydını arşivler |

### Alt Akışlar ve Alt Süreçler

İş akışı, farklı ödeme süreçleri için özelleştirilmiş alt bileşenler içerir:

- **Payment Process Subflow** (`payment-process.json`) - Ana ödeme işleme mantığını yönetir
- **Payment Notification Subprocess** (`payment-notification-subflow.json`) - Ödeme sonrası bildirim gönderimini yönetir

## Ödeme Akış Detayları

### 1. Ödeme Konfigürasyonu

Her zamanlanmış ödeme, konfigürasyon kaydı ile başlar:

```json
{
  "userId": "12345",
  "amount": 100.00,
  "currency": "USD",
  "frequency": "monthly",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "paymentMethodId": "pm_12345",
  "description": "Monthly subscription payment",
  "recipientId": "recipient_789",
  "isAutoRetry": true,
  "maxRetries": 3
}
```

**Başarı Yanıtı:**
```json
{
  "paymentSchedule": {
    "success": true,
    "scheduleId": "sched_12345",
    "status": "inactive",
    "nextPaymentDate": "2024-01-01T00:00:00Z",
    "maxPayments": 12,
    "createdAt": "2024-01-01T12:00:00Z",
    "completedPayments": 0,
    "lastPaymentAt": "2024-01-01T12:00:00Z",
    "isActive": false,
    "activatedAt": "2024-01-01T12:00:00Z",
    "archived": false,
    "finishedAt": "2024-01-01T12:00:00Z"
  }
}
```

### 2. Ödeme İşleme (Subflow)

Ödeme işleme subflow'u aşağıdaki durumları içerir:

#### Ödeme Beklemede
- **Zamanlanmış Tetikleme** - Timer tabanlı ödeme vadesi kontrolü
- **Manuel Tetikleme** - "Hemen Öde" fonksiyonu

#### Ödeme İşlemi

```json
{
  "scheduleId": "sched_12345",
  "userId": "12345",
  "amount": 100.00,
  "currency": "USD",
  "processedAt": "2024-01-01T12:00:00Z"
}
```

**Başarılı Ödeme Yanıtı:**
```json
{
  "paymentResult": {
    "status": "success",
    "success": true,
    "transactionId": "txn_67890",
    "amount": 100.00,
    "currency": "USD",
    "processedAt": "2024-01-01T12:01:00Z",
    "paymentMethod": "credit_card",
    "fees": 2.50
  }
}
```

**Başarısız Ödeme Yanıtı:**
```json
{
  "paymentResult": {
    "status": "failed",
    "success": false,
    "error": "Insufficient funds",
    "errorCode": "insufficient_funds",
    "errorDescription": "The payment method has insufficient funds",
    "statusCode": 402,
    "failedAt": "2024-01-01T12:01:00Z",
    "retryCount": 1
  }
}
```

#### Yeniden Deneme Mekanizması

Başarısız ödemeler için otomatik yeniden deneme:

1. **Otomatik Yeniden Deneme Kontrolü:**
   - `isAutoRetry` aktif olmalı
   - Mevcut deneme sayısı `maxRetries` değerinden az olmalı

2. **Yeniden Deneme Sayacı Artırma:**
```json
{
  "scheduleId": "sched_12345",
  "retryCount": 2,
  "retryAt": "2024-01-01T12:02:00Z"
}
```

3. **Maksimum Deneme Kontrolü:**
   - Maksimum denemeye ulaşılırsa ödeme iptal edilir
   - Ana iş akışı deaktifleştirilirse alt akış sonlandırılır

### 3. Bildirim Sistemi (Subprocess)

Ödeme işlemi tamamlandıktan sonra bildirim alt süreci çalışır:

#### Kullanıcı Bilgilerini Al

```json
{
  "userId": "12345"
}
```

**Kullanıcı Bilgileri Yanıtı:**
```json
{
  "user": {
    "userId": "12345",
    "phoneNumber": "+1234567890",
    "registeredDevices": [
      {
        "deviceId": "device_123",
        "deviceType": "mobile",
        "supportsPush": true
      }
    ],
    "language": "tr-TR"
  }
}
```

#### SMS Bildirimi

**Başarılı Ödeme SMS'i:**
```
Ödeme başarılı! Miktar: 100.00 USD. Referans: txn_67890
```

**Başarısız Ödeme SMS'i:**
```
Ödeme başarısız. Neden: Yetersiz bakiye. Lütfen ödeme bilgilerinizi kontrol edin.
```

#### Push Bildirimi

```json
{
  "title": "Ödeme Başarılı",
  "body": "100.00 USD ödemeniz başarıyla işleme alındı.",
  "userId": "12345",
  "devices": ["device_123"],
  "pushType": "payment-notification",
  "data": {
    "paymentId": "txn_67890",
    "success": true
  }
}
```

### 4. Zamanlama ve Döngü Yönetimi

#### Timer Konfigürasyonu

```csharp
// Günlük ödeme
TimerSchedule.FromDateTime(DateTime.UtcNow.AddDays(1))

// Haftalık ödeme  
TimerSchedule.FromDateTime(DateTime.UtcNow.AddDays(7))

// Aylık ödeme
TimerSchedule.FromDateTime(DateTime.UtcNow.AddMonths(1))

// Üç aylık ödeme
TimerSchedule.FromDateTime(DateTime.UtcNow.AddMonths(3))

// Yıllık ödeme
TimerSchedule.FromDateTime(DateTime.UtcNow.AddYears(1))
```

#### Döngü Kontrolü

Ödeme tamamlandıktan sonra:

1. **Daha Fazla Ödeme Kontrolü:**
   - Maksimum ödeme sayısına ulaşıldı mı?
   - Bitiş tarihi geçildi mi?
   - Plan hala aktif mi?

2. **Döngü Devamı:**
   - Şartlar sağlanıyorsa bir sonraki ödeme için timer ayarlanır
   - Bildirim alt süreci çalışır
   - Ana akış tekrar "Aktif" durumuna geçer

3. **Döngü Bitişi:**
   - Şartlar sağlanmıyorsa "Tamamlandı" durumuna geçilir
   - Ödeme kayıtları arşivlenir

## Hata Yönetimi

### Konfigürasyon Hataları

```json
{
  "paymentSchedule": {
    "success": false,
    "error": "Ödeme konfigürasyonu kaydedilemedi",
    "errorCode": "config_save_failed",
    "statusCode": 400
  }
}
```

### Ödeme İşleme Hataları

```json
{
  "paymentResult": {
    "success": false,
    "error": "Ödeme işleme başarısız",
    "errorCode": "payment_processing_error",
    "errorDescription": "Ödeme işlenemiyor",
    "statusCode": 500
  }
}
```

### Bildirim Hataları

```json
{
  "smsResult": {
    "smsSent": false,
    "error": "SMS bildirimi gönderilemedi",
    "errorCode": "sms_send_failed",
    "smsStatus": "failed"
  },
  "pushResult": {
    "pushSent": false,
    "error": "Push bildirimi gönderilemedi",
    "errorCode": "push_send_failed"
  }
}
```

## Durum Makinesi Geçişleri

### Otomatik Geçişler

- **payment-config-saved** → `payment-active`: Konfigürasyon başarıyla kaydedildi
- **payment-process-complete** → `payment-cycle-check`: Ödeme başarılı ve daha fazla ödeme var
- **payments-all-complete** → `payment-finished`: Tüm ödemeler tamamlandı
- **payment-success** → `payment-success-state`: Ödeme başarılı
- **payment-error** → `payment-failed-state`: Ödeme başarısız
- **auto-retry-payment** → `payment-retry`: Otomatik yeniden deneme
- **max-retries-reached** → `payment-cancelled`: Maksimum deneme sayısına ulaşıldı

### Manuel Geçişler

- **manual-deactivate-payment** → `payment-deactive`: Ödemeyi deaktifleştir
- **manual-reactivate-payment** → `payment-active`: Ödemeyi yeniden aktifleştir
- **manual-update-payment** → `payment-configuration`: Ödeme ayarlarını güncelle
- **manual-delete-payment** → `payment-terminated`: Ödemeyi kalıcı olarak sil
- **manual-pay-now** → `process-payment`: Hemen ödeme yap
- **manual-user-triggers-retry** → `payment-retry`: Kullanıcı yeniden deneme tetikler

### Zamanlanmış Geçişler

- **scheduled-payment-due** → `process-payment`: Ödeme vadesi geldi (Timer)

## Güvenlik ve En İyi Uygulamalar

### Veri Güvenliği

- Ödeme bilgileri şifrelenmiş olarak saklanır
- API çağrıları HTTPS üzerinden yapılır
- Hassas bilgiler loglarda görünmez

### Hata Toleransı

- Yeniden deneme mekanizmaları
- Graceful degradation (bildirim başarısız olsa bile ödeme devam eder)
- Comprehensive error logging

### Performans

- Asenkron görev yürütme
- Optimal timer scheduling
- Efficient database queries

## Sonuç

Bu zamanlanmış ödemeler iş akışı, modern ödeme sistemlerinin ihtiyaçları için kapsamlı, güvenli ve ölçeklenebilir bir çözüm sağlar. Subflow ve subprocess örnekleri, timer tabanlı scheduling, ve gelişmiş hata yönetimi özellikleri ile birlikte güçlü bir ödeme otomasyonu platformu sunar.

Modüler tasarım kolay özelleştirme ve genişletmeye olanak tanırken, sağlam güvenlik özellikleri modern ödeme standartları ve en iyi uygulamalarla uyumluluğu sağlar.
