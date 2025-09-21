# OAuth2 Kimlik Doğrulama İş Akışı

## Genel Bakış

Bu belge, VNext Runtime platformu kullanılarak geliştirilen kapsamlı OAuth2 kimlik doğrulama iş akışı implementasyonunu açıklamaktadır. İş akışı, push bildirimleri ve OTP doğrulaması dahil olmak üzere gelişmiş Çok Faktörlü Kimlik Doğrulama (MFA) yetenekleri ile birden fazla OAuth2 grant türünü desteklemektedir.

## Mimari

[![](https://mermaid.ink/img/pako:eNqVVctu00AU_ZXRILFKg5PacWOhojRp0jZtkjZpKiCocu3rxqpjR2Ob0jqR2LDjKdhQqBCIPWLH9_AD8AnMI44dkyKRxTiec8-Zc--dGUfY8EzAGj4l-niIerWBi-iv8nCAf3-6eoralTAYFvMSYk9wA9vQA9tz756QO-tHHjmzHO8cdQOdBAP8CK2srKMNTn33AlUdmxJQX3dsk5NohFAX4wYPr0bpuHtTgVUZNrkP_gTVqN7Pq_e_frxCDaLTwN7FGFAXHDAWRAWl5U3QJmNcP89YRnXddsDMmKhxE_UokZ45qHM5g3s7NgiYTEl3qKGGSPB1nGA1AXlZuuEJq8p8IaE01n3_3CPmBG1x_puvqDOb-gdLpyl4xL7kGRyzVk3QNue_veb5zUFUpeBSpYErnls81WbEaKgbGgb4flzvbY7tLsXE2Eg6sscNvPyGet4ZuKgBLpDFBjeSXoiJnf9h72TZzYTd4uwPn9l22KtXUHUIxtmc2cwyd1PM9EwqRIwtXoB2VIPHtgHoAE5tPwDa2bgK7VgJ3Uad0GdVGo89EkxQR_Tju5hueYFtpU8Jc5ltSXtuYl9spi-o3euwhG7oIBs73OJBJAQXWrTPoe4ySIwHqfqnZ1J16CYhvTijhWpkutRdXsget3IYpTlZT4d_uTlcLrbHxfoR3yp-vFeSpvQToSN26D8-yx762cpz1_3lC_nBhQOogizbcbRbULAUC9LI0Qwx1qBklNPI5gyxLMM0i2mkMUesVZDSyNaNyPaNSCdGVkGxlDSyv4DgHL3KbRNrAQkhh0dARjp7xRHjDDCtzggGWKN_TZ3QozNwp5Qz1t0HnjeKacQLT4dYs-idRt_CMb3AoWbr9CORhIBrAql6oRtgbY0rYC3CT7BWLCh5dbWsFNRioVAuyFIphy_otCTnFVmVpbKkqCUKTnP4kq8p5WVpraSWZLkoy4qqlgul6R_HxRkW?type=png)](https://mermaid.live/edit#pako:eNqVVctu00AU_ZXRILFKg5PacWOhojRp0jZtkjZpKiCocu3rxqpjR2Ob0jqR2LDjKdhQqBCIPWLH9_AD8AnMI44dkyKRxTiec8-Zc--dGUfY8EzAGj4l-niIerWBi-iv8nCAf3-6eoralTAYFvMSYk9wA9vQA9tz756QO-tHHjmzHO8cdQOdBAP8CK2srKMNTn33AlUdmxJQX3dsk5NohFAX4wYPr0bpuHtTgVUZNrkP_gTVqN7Pq_e_frxCDaLTwN7FGFAXHDAWRAWl5U3QJmNcP89YRnXddsDMmKhxE_UokZ45qHM5g3s7NgiYTEl3qKGGSPB1nGA1AXlZuuEJq8p8IaE01n3_3CPmBG1x_puvqDOb-gdLpyl4xL7kGRyzVk3QNue_veb5zUFUpeBSpYErnls81WbEaKgbGgb4flzvbY7tLsXE2Eg6sscNvPyGet4ZuKgBLpDFBjeSXoiJnf9h72TZzYTd4uwPn9l22KtXUHUIxtmc2cwyd1PM9EwqRIwtXoB2VIPHtgHoAE5tPwDa2bgK7VgJ3Uad0GdVGo89EkxQR_Tju5hueYFtpU8Jc5ltSXtuYl9spi-o3euwhG7oIBs73OJBJAQXWrTPoe4ySIwHqfqnZ1J16CYhvTijhWpkutRdXsget3IYpTlZT4d_uTlcLrbHxfoR3yp-vFeSpvQToSN26D8-yx762cpz1_3lC_nBhQOogizbcbRbULAUC9LI0Qwx1qBklNPI5gyxLMM0i2mkMUesVZDSyNaNyPaNSCdGVkGxlDSyv4DgHL3KbRNrAQkhh0dARjp7xRHjDDCtzggGWKN_TZ3QozNwp5Qz1t0HnjeKacQLT4dYs-idRt_CMb3AoWbr9CORhIBrAql6oRtgbY0rYC3CT7BWLCh5dbWsFNRioVAuyFIphy_otCTnFVmVpbKkqCUKTnP4kq8p5WVpraSWZLkoy4qqlgul6R_HxRkW)

OAuth2 iş akışı, aşağıdaki temel bileşenlerle bir durum makinesi olarak implementasyonu yapılmıştır:

### Ana İş Akışı Durumları

1. **İstemci Doğrulama** - OAuth2 istemci kimlik bilgilerini doğrular
2. **Grant Type Seçimi** - Uygun kimlik doğrulama akışına yönlendirir
3. **Kimlik Doğrulama Akışları** - Password, Authorization Code, Client Credentials
4. **MFA Kontrolü** - Cihaz kaydına göre MFA gereksinimlerini belirler
5. **MFA Akışları** - Push bildirimi veya OTP doğrulaması
6. **Cihaz Kaydı** - Başarılı OTP MFA sonrası yeni cihazları kaydeder
7. **Token Oluşturma** - Access ve refresh token'ları oluşturur
8. **Son Durumlar** - Başarı veya başarısızlık uç noktaları

### Desteklenen Grant Türleri

- **Client Credentials Flow** - Servis-servis kimlik doğrulaması için
- **Password Grant Flow** - Kullanıcı kimlik bilgileri ile güvenilir uygulamalar için
- **Authorization Code Flow** - Yönlendirme tabanlı kimlik doğrulaması ile web uygulamaları için

### Çok Faktörlü Kimlik Doğrulama (MFA)

- **Push Bildirimi MFA** - Push desteği olan kayıtlı cihazlar için
- **OTP MFA** - Kayıtlı olmayan cihazlar için SMS/Email tabanlı tek kullanımlık şifreler
- **Cihaz Kaydı** - Başarılı OTP doğrulaması sonrası otomatik cihaz kaydı

## İş Akışı Bileşenleri

### Görevler

İş akışı, harici servislerle etkileşim kurmak için aşağıdaki HTTP görevlerini kullanır:

| Görev | Endpoint | Amaç |
|-------|----------|------|
| `validate-client` | `/api/oauth2/client/validate` | OAuth2 istemci kimlik bilgilerini doğrular |
| `validate-user-credentials` | `/api/oauth2/user/authenticate` | Kullanıcı kimlik bilgilerini doğrular |
| `validate-authorization-code` | `/api/oauth2/code/validate` | Authorization code'ları doğrular |
| `check-device-registration` | `/api/oauth2/device/check` | Cihaz kayıt durumunu kontrol eder |
| `send-push-notification` | `/api/oauth2/push/send` | MFA için push bildirimleri gönderir |
| `check-push-response` | `/api/oauth2/push/check` | Push bildirimi yanıtlarını kontrol eder |
| `send-otp-notification` | `/api/oauth2/otp/send` | SMS/email ile OTP kodları gönderir |
| `verify-otp-code` | `/api/oauth2/otp/verify` | OTP kodlarını doğrular |
| `register-device` | `/api/oauth2/device/register` | Yeni cihazları kaydeder |
| `generate-tokens` | `/api/oauth2/tokens/generate` | Access ve refresh token'ları oluşturur |

### Alt Akışlar

İş akışı, farklı kimlik doğrulama yöntemleri için özelleştirilmiş alt akışlar içerir:

- **Password Alt Akışı** (`password-subflow.json`) - Password tabanlı kimlik doğrulamayı yönetir
- **Authorization Code Alt Akışı** (`authorization-code-subflow.json`) - Authorization code doğrulamasını yönetir
- **OTP MFA Alt Akışı** (`otp-mfa-subflow.json`) - Timeout yönetimi ile OTP tabanlı MFA'yı yönetir
- **Push Bildirimi MFA Alt Akışı** (`push-notification-mfa-subflow.json`) - Timeout yönetimi ile push tabanlı MFA'yı yönetir

## Kimlik Doğrulama Akış Detayları

### 1. İstemci Doğrulama

Her kimlik doğrulama isteği istemci doğrulaması ile başlar:

```json
{
  "client_id": "your-client-id",
  "client_secret": "your-client-secret",
  "grant_type": "password|authorization_code|client_credentials",
  "scope": "requested-scopes"
}
```

**Başarı Yanıtı:**
```json
{
  "clientValidation": {
    "success": true,
    "validatedAt": "2024-01-01T12:00:00Z",
    "clientId": "your-client-id",
    "grantTypes": ["password", "authorization_code"],
    "redirectUris": ["https://example.com/callback"],
    "scopes": ["read", "write"],
    "isActive": true
  }
}
```

### 2. Grant Type Akışları

#### Password Grant Akışı

Kullanıcı kimlik bilgilerini güvenli bir şekilde işleyebilen güvenilir uygulamalar için:

```json
{
  "username": "user@example.com",
  "password": "user-password"
}
```

**Başarı Yanıtı:**
```json
{
  "authentication": {
    "success": true,
    "userId": "12345",
    "username": "user@example.com",
    "email": "user@example.com",
    "roles": ["user", "premium"],
    "mfaRequired": true
  }
}
```

#### Authorization Code Akışı

Yönlendirme tabanlı kimlik doğrulaması kullanan web uygulamaları için:

```json
{
  "code": "authorization-code",
  "client_id": "your-client-id",
  "client_secret": "your-client-secret",
  "redirect_uri": "https://example.com/callback",
  "code_verifier": "pkce-code-verifier"
}
```

#### Client Credentials Akışı

Servis-servis kimlik doğrulaması için (MFA'yı atlar):

```json
{
  "client_id": "service-client-id",
  "client_secret": "service-client-secret",
  "scope": "service-scope"
}
```

### 3. Çok Faktörlü Kimlik Doğrulama

#### Cihaz Kayıt Kontrolü

Başarılı birincil kimlik doğrulaması sonrası, sistem cihaz kaydını kontrol eder:

```json
{
  "user_id": "12345",
  "device_id": "device-fingerprint",
  "client_id": "your-client-id"
}
```

**Kayıtlı Cihaz Yanıtı:**
```json
{
  "deviceRegistration": {
    "checkedAt": "2024-01-01T12:00:00Z",
    "isRegistered": true,
    "deviceId": "device-fingerprint",
    "deviceName": "iPhone 12",
    "deviceType": "mobile",
    "supportsPush": true,
    "registeredAt": "2024-01-01T10:00:00Z",
    "lastSeenAt": "2024-01-01T11:30:00Z"
  }
}
```

#### Push Bildirimi MFA

Push desteği olan kayıtlı cihazlar için:

1. **Push Bildirimi Gönder:**
```json
{
  "userId": "12345",
  "deviceId": "device-fingerprint",
  "title": "Kimlik Doğrulama İsteği",
  "message": "Bu giriş denemesini cihazınızdan onaylayın",
  "actionType": "mfa_authentication",
  "expiresIn": 120,
  "metadata": {
    "requestId": "req-12345",
    "clientId": "your-client-id",
    "ipAddress": "192.168.1.1",
    "userAgent": "Mozilla/5.0..."
  }
}
```

2. **Push Yanıtını Kontrol Et:**
```json
{
  "notificationId": "notif-12345",
  "userId": "12345",
  "deviceId": "device-fingerprint"
}
```

**Onaylanmış Yanıt:**
```json
{
  "mfa": {
    "success": true,
    "pushApproved": true,
    "method": "push",
    "approvedAt": "2024-01-01T12:01:00Z"
  }
}
```

#### OTP MFA

Kayıtlı olmayan cihazlar veya push mevcut olmadığında:

1. **OTP Gönder:**
```json
{
  "userId": "12345",
  "email": "user@example.com",
  "phone": "+1234567890",
  "method": "sms",
  "deviceId": "device-fingerprint",
  "language": "tr-TR"
}
```

**OTP Gönderildi Yanıtı:**
```json
{
  "otp": {
    "sent": true,
    "otpId": "otp-12345",
    "expiresAt": "2024-01-01T12:05:00Z",
    "method": "sms",
    "attemptsRemaining": 3
  }
}
```

2. **OTP Doğrula:**
```json
{
  "otpId": "otp-12345",
  "otpCode": "123456",
  "userId": "12345",
  "deviceId": "device-fingerprint"
}
```

**OTP Doğrulandı Yanıtı:**
```json
{
  "mfa": {
    "success": true,
    "otpVerified": true,
    "method": "otp",
    "verifiedAt": "2024-01-01T12:02:00Z"
  }
}
```

### 4. Cihaz Kaydı

Başarılı OTP MFA sonrası, kayıtlı olmayan cihazlar otomatik olarak kaydedilir:

```json
{
  "userId": "12345",
  "deviceId": "device-fingerprint",
  "deviceName": "Chrome Tarayıcısı",
  "deviceType": "desktop",
  "supportsPush": false
}
```

**Kayıt Yanıtı:**
```json
{
  "deviceRegistration": {
    "success": true,
    "deviceId": "device-fingerprint",
    "isRegistered": true,
    "supportsPush": false
  }
}
```

### 5. Token Oluşturma

Başarılı kimlik doğrulama ve MFA sonrası, token'lar oluşturulur:

```json
{
  "client_id": "your-client-id",
  "client_secret": "your-client-secret",
  "grant_type": "password",
  "user_id": "12345",
  "username": "user@example.com",
  "email": "user@example.com",
  "scope": "read write",
  "user_roles": ["user", "premium"],
  "device_id": "device-fingerprint",
  "device_registered": true,
  "mfa_completed": true,
  "mfa_method": "push",
  "ip_address": "192.168.1.1",
  "user_agent": "Mozilla/5.0..."
}
```

**Token Yanıtı:**
```json
{
  "tokens": {
    "success": true,
    "generated_at": "2024-01-01T12:03:00Z",
    "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "token_type": "Bearer",
    "expires_in": 3600,
    "scope": "read write"
  }
}
```

## Hata Yönetimi

İş akışı çeşitli senaryolar için kapsamlı hata yönetimi içerir:

### İstemci Doğrulama Hataları

```json
{
  "clientValidation": {
    "success": false,
    "validatedAt": "2024-01-01T12:00:00Z",
    "error": "İstemci doğrulaması başarısız",
    "errorCode": "invalid_client",
    "errorDescription": "Sağlanan istemci kimlik bilgileri geçersiz",
    "statusCode": 401
  }
}
```

### Kimlik Doğrulama Hataları

```json
{
  "authentication": {
    "success": false,
    "error": "Geçersiz kullanıcı adı veya şifre",
    "errorCode": "invalid_credentials"
  }
}
```

### MFA Hataları

```json
{
  "mfa": {
    "success": false,
    "error": "Geçersiz OTP kodu",
    "errorCode": "invalid_otp",
    "attemptsRemaining": 2
  }
}
```

### Token Oluşturma Hataları

```json
{
  "tokens": {
    "success": false,
    "generated_at": "2024-01-01T12:03:00Z",
    "error": "Token oluşturma başarısız",
    "errorCode": "token_generation_error",
    "errorDescription": "Access token'ları oluşturulamadı",
    "statusCode": 500
  }
}
```

## Sonuç

Bu OAuth2 kimlik doğrulama iş akışı, modern kimlik doğrulama ihtiyaçları için kapsamlı, güvenli ve ölçeklenebilir bir çözüm sağlar. Birden fazla grant türünü, gelişmiş MFA yeteneklerini destekler ve kapsamlı izleme ve hata yönetimi özellikleri içerir.

Modüler tasarım kolay özelleştirme ve genişletmeye olanak tanırken, sağlam güvenlik özellikleri modern kimlik doğrulama standartları ve en iyi uygulamalarla uyumluluğu sağlar.
