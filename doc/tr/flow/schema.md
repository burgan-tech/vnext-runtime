# Schema Yönetimi

Schema'lar, workflow sisteminde veri doğrulama ve normalizasyon için kullanılan JSON Schema tabanlı tanımlardır. İstek yapılan payload'ların belirlenen kurallara uygunluğunu kontrol ederek veri bütünlüğü ve tutarlılığı sağlarlar.

:::highlight green 💡
Schema'lar JSON formatında tanımlanır ve sistem tarafından otomatik olarak yüklenir. Her bir schema domain'e özgü bir anahtar ile tanımlanır ve sürüm yönetimi desteklenir.
:::

## İçindekiler

1. [Schema Tanımı](#schema-tanımı)
2. [Schema Özellikleri](#schema-özellikleri)
3. [Kullanım Alanları](#kullanım-alanları)
4. [Type Değerleri](#type-değerleri)
5. [JSON Schema Yapısı](#json-schema-yapısı)
6. [Kullanım Örnekleri](#kullanım-örnekleri)
7. [En İyi Uygulamalar](#en-iyi-uygulamalar)

## Schema Tanımı

Schema, workflow bileşenleri için giriş ve çıkış verilerinin yapısını tanımlayan bir domain entity'sidir. Schema'lar versiyonlanır ve workflow'lar, transition'lar ve task'lar tarafından referans edilebilir.

### Temel Yapı

```json
{
  "key": "account-confirmation",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-schemas",
  "flowVersion": "1.0.0",
  "tags": [
    "banking",
    "account-confirmation",
    "input-schema",
    "approval"
  ],
  "attributes": {
    "type": "workflow",
    "schema": {
      "$schema": "https://json-schema.org/draft/2020-12/schema",
      "$id": "https://schemas.vnext.com/banking/account-confirmation.json",
      "title": "Account Confirmation Schema",
      "description": "Schema for account opening confirmation",
      "type": "object",
      "required": [
        "confirmed",
        "termsAccepted"
      ],
      "properties": {
        "confirmed": {
          "type": "boolean",
          "description": "Kullanıcının onay durumu"
        },
        "termsAccepted": {
          "type": "boolean",
          "description": "Şartlar ve koşulların kabul durumu"
        }
      }
    }
  }
}
```

## Schema Özellikleri

### Temel Özellikler

| Özellik | Tip | Açıklama |
|---------|-----|----------|
| `key` | `string` | Schema için benzersiz tanımlayıcı |
| `version` | `string` | Versiyon bilgisi (semantic versioning) |
| `domain` | `string` | Schema'nın ait olduğu domain |
| `flow` | `string` | Flow stream bilgisi (varsayılan: `sys-schemas`) |
| `flowVersion` | `string` | Flow versiyon bilgisi |
| `tags` | `string[]` | Kategorilendirme ve arama için etiketler |
| `attributes` | `object` | Schema içeriği ve metadata |

### Tags Özelliği

Etiketler, schema'ların kategorilendirmesi ve aranması için kullanılır:

```json
{
  "tags": [
    "banking",
    "account-confirmation",
    "input-schema",
    "approval"
  ]
}
```

**Etiket Kullanım Önerileri:**
- Domain alanını belirtin (örn: `banking`, `payments`, `user-management`)
- Schema tipini belirtin (örn: `input-schema`, `output-schema`, `validation`)
- İş sürecini belirtin (örn: `approval`, `registration`, `transfer`)
- Bileşen türünü belirtin (örn: `workflow`, `task`, `view`)

### Attributes Özelliği

`attributes` objesi schema'nın içeriğini ve metadata bilgilerini barındırır:

| Özellik | Tip | Açıklama |
|---------|-----|----------|
| `type` | `string` | Schema'nın hangi bileşen türü için kullanıldığı |
| `schema` | `object` | Gerçek JSON Schema tanımı |

## Kullanım Alanları

Schema'lar aşağıdaki senaryolarda veri doğrulama için kullanılır:

### 1. Workflow Başlatma

Workflow başlatılırken gelen veri payload'ının doğrulanması:

```json
{
  "startTransition": {
    "key": "start",
    "target": "initial-state",
    "schema": {
      "ref": "Schemas/start-input-schema.json"
    }
  }
}
```

### 2. Transition Doğrulama

Geçiş sırasında gönderilen verinin doğrulanması:

```json
{
  "key": "approve",
  "target": "approved",
  "triggerType": 0,
  "schema": {
    "ref": "Schemas/approval-schema.json"
  }
}
```

### 3. Task Giriş/Çıkış Kontrolü

Task'lara gönderilen ve dönen verilerin doğrulanması:

```json
{
  "task": {
    "ref": "Tasks/validate-user.json"
  },
  "inputSchema": {
    "ref": "Schemas/user-input-schema.json"
  },
  "outputSchema": {
    "ref": "Schemas/user-output-schema.json"
  }
}
```

### 4. View Form Doğrulama

Kullanıcı arayüzünden gelen form verilerinin doğrulanması.

## Type Değerleri

`attributes.type` özelliği, schema'nın hangi bileşen türü için tasarlandığını belirtir:

| Type Değeri | Açıklama | Kullanım Alanı |
|-------------|----------|----------------|
| `workflow` | Workflow seviyesinde kullanılan schema'lar | Workflow başlatma, genel veri yapıları |
| `task` | Task giriş/çıkış doğrulaması | HTTP task'ları, script task'ları |
| `function` | Platform fonksiyonları için schema'lar | Fonksiyon parametreleri ve dönüş değerleri |
| `view` | View form doğrulaması | Kullanıcı formu girişleri |
| `schema` | Meta-schema tanımları | Schema'ların doğrulanması |
| `extension` | Extension veri yapıları | Extension giriş/çıkış verileri |
| `headers` | HTTP header doğrulaması | API çağrıları için header yapıları |

### Type Seçimi Rehberi

```
┌─────────────────────────────────────────────────────────────┐
│                    Type Seçim Rehberi                        │
├─────────────────────────────────────────────────────────────┤
│ Workflow başlatma verisi mi?          → type: "workflow"    │
│ Task için giriş/çıkış mı?             → type: "task"        │
│ Platform fonksiyonu mu?               → type: "function"    │
│ Kullanıcı form girişi mi?             → type: "view"        │
│ Başka schema'yı doğrulama mı?         → type: "schema"      │
│ Extension verisi mi?                  → type: "extension"   │
│ HTTP header yapısı mı?                → type: "headers"     │
└─────────────────────────────────────────────────────────────┘
```

## JSON Schema Yapısı

Schema tanımları [JSON Schema Draft 2020-12](https://json-schema.org/draft/2020-12/schema) standardını kullanır.

### Temel JSON Schema Elemanları

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.vnext.com/banking/example.json",
  "title": "Örnek Schema",
  "description": "Bu bir örnek schema tanımıdır",
  "type": "object",
  "required": ["field1", "field2"],
  "properties": {
    "field1": {
      "type": "string",
      "description": "Zorunlu metin alanı",
      "minLength": 1,
      "maxLength": 100
    },
    "field2": {
      "type": "integer",
      "description": "Zorunlu sayı alanı",
      "minimum": 0,
      "maximum": 1000
    }
  }
}
```

### Desteklenen JSON Schema Özellikleri

#### Tip Doğrulamaları

| Tip | Açıklama | Örnek |
|-----|----------|-------|
| `string` | Metin değerleri | `"type": "string"` |
| `number` | Ondalıklı sayılar | `"type": "number"` |
| `integer` | Tam sayılar | `"type": "integer"` |
| `boolean` | True/False değerleri | `"type": "boolean"` |
| `array` | Dizi değerleri | `"type": "array"` |
| `object` | Nesne değerleri | `"type": "object"` |
| `null` | Null değer | `"type": "null"` |

#### String Doğrulamaları

```json
{
  "type": "string",
  "minLength": 1,
  "maxLength": 255,
  "pattern": "^[A-Z][a-z]+$",
  "format": "email"
}
```

**Desteklenen Format Değerleri:**
- `email`: E-posta adresi
- `uri`: URI formatı
- `date`: ISO 8601 tarih (YYYY-MM-DD)
- `date-time`: ISO 8601 tarih-saat
- `time`: ISO 8601 saat
- `uuid`: UUID formatı
- `ipv4`: IPv4 adresi
- `ipv6`: IPv6 adresi

#### Number/Integer Doğrulamaları

```json
{
  "type": "number",
  "minimum": 0,
  "maximum": 1000000,
  "exclusiveMinimum": 0,
  "exclusiveMaximum": 1000000,
  "multipleOf": 0.01
}
```

#### Array Doğrulamaları

```json
{
  "type": "array",
  "items": {
    "type": "string"
  },
  "minItems": 1,
  "maxItems": 10,
  "uniqueItems": true
}
```

#### Object Doğrulamaları

```json
{
  "type": "object",
  "properties": {
    "name": { "type": "string" },
    "age": { "type": "integer" }
  },
  "required": ["name"],
  "additionalProperties": false
}
```

### Koşullu Doğrulamalar

#### If-Then-Else

```json
{
  "type": "object",
  "properties": {
    "accountType": {
      "type": "string",
      "enum": ["individual", "corporate"]
    }
  },
  "if": {
    "properties": {
      "accountType": { "const": "corporate" }
    }
  },
  "then": {
    "required": ["taxNumber", "companyName"]
  },
  "else": {
    "required": ["identityNumber"]
  }
}
```

#### OneOf / AnyOf / AllOf

```json
{
  "oneOf": [
    {
      "type": "object",
      "properties": {
        "paymentType": { "const": "credit-card" },
        "cardNumber": { "type": "string" }
      },
      "required": ["paymentType", "cardNumber"]
    },
    {
      "type": "object",
      "properties": {
        "paymentType": { "const": "bank-transfer" },
        "iban": { "type": "string" }
      },
      "required": ["paymentType", "iban"]
    }
  ]
}
```

## Kullanım Örnekleri

### Örnek 1: Hesap Açılış Schema'sı

```json
{
  "key": "account-opening-input",
  "version": "1.0.0",
  "domain": "banking",
  "flow": "sys-schemas",
  "flowVersion": "1.0.0",
  "tags": [
    "banking",
    "account-opening",
    "input-schema"
  ],
  "attributes": {
    "type": "workflow",
    "schema": {
      "$schema": "https://json-schema.org/draft/2020-12/schema",
      "$id": "https://schemas.vnext.com/banking/account-opening-input.json",
      "title": "Account Opening Input Schema",
      "description": "Hesap açılış başvurusu için gerekli veriler",
      "type": "object",
      "required": [
        "customerType",
        "accountType",
        "currency"
      ],
      "properties": {
        "customerType": {
          "type": "string",
          "enum": ["individual", "corporate"],
          "description": "Müşteri tipi"
        },
        "accountType": {
          "type": "string",
          "enum": ["checking", "savings", "investment"],
          "description": "Hesap tipi"
        },
        "currency": {
          "type": "string",
          "enum": ["TRY", "USD", "EUR", "GBP"],
          "description": "Para birimi"
        },
        "initialDeposit": {
          "type": "number",
          "minimum": 0,
          "description": "Açılış tutarı"
        }
      }
    }
  }
}
```

### Örnek 2: Onay Schema'sı

```json
{
  "key": "approval-confirmation",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-schemas",
  "flowVersion": "1.0.0",
  "tags": [
    "core",
    "approval",
    "confirmation"
  ],
  "attributes": {
    "type": "task",
    "schema": {
      "$schema": "https://json-schema.org/draft/2020-12/schema",
      "$id": "https://schemas.vnext.com/core/approval-confirmation.json",
      "title": "Approval Confirmation Schema",
      "description": "Onay işlemi için gerekli veriler",
      "type": "object",
      "required": [
        "approved",
        "approverNote"
      ],
      "properties": {
        "approved": {
          "type": "boolean",
          "description": "Onay durumu"
        },
        "approverNote": {
          "type": "string",
          "minLength": 10,
          "maxLength": 500,
          "description": "Onaylayan notu"
        },
        "approvedAt": {
          "type": "string",
          "format": "date-time",
          "description": "Onay zamanı"
        }
      }
    }
  }
}
```

### Örnek 3: Para Transferi Schema'sı

```json
{
  "key": "money-transfer-input",
  "version": "1.0.0",
  "domain": "payments",
  "flow": "sys-schemas",
  "flowVersion": "1.0.0",
  "tags": [
    "payments",
    "transfer",
    "input-schema"
  ],
  "attributes": {
    "type": "workflow",
    "schema": {
      "$schema": "https://json-schema.org/draft/2020-12/schema",
      "$id": "https://schemas.vnext.com/payments/money-transfer-input.json",
      "title": "Money Transfer Input Schema",
      "description": "Para transferi için gerekli veriler",
      "type": "object",
      "required": [
        "sourceAccount",
        "targetAccount",
        "amount",
        "currency"
      ],
      "properties": {
        "sourceAccount": {
          "type": "string",
          "pattern": "^[0-9]{16}$",
          "description": "Kaynak hesap numarası"
        },
        "targetAccount": {
          "type": "string",
          "description": "Hedef hesap numarası veya IBAN"
        },
        "amount": {
          "type": "number",
          "exclusiveMinimum": 0,
          "maximum": 1000000,
          "description": "Transfer tutarı"
        },
        "currency": {
          "type": "string",
          "enum": ["TRY", "USD", "EUR"],
          "description": "Para birimi"
        },
        "description": {
          "type": "string",
          "maxLength": 140,
          "description": "Transfer açıklaması"
        },
        "scheduled": {
          "type": "boolean",
          "default": false,
          "description": "İleri tarihli transfer mi?"
        },
        "scheduledDate": {
          "type": "string",
          "format": "date",
          "description": "Planlanan transfer tarihi"
        }
      },
      "if": {
        "properties": {
          "scheduled": { "const": true }
        }
      },
      "then": {
        "required": ["scheduledDate"]
      }
    }
  }
}
```

### Örnek 4: HTTP Header Schema'sı

```json
{
  "key": "api-request-headers",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-schemas",
  "flowVersion": "1.0.0",
  "tags": [
    "core",
    "headers",
    "api"
  ],
  "attributes": {
    "type": "headers",
    "schema": {
      "$schema": "https://json-schema.org/draft/2020-12/schema",
      "$id": "https://schemas.vnext.com/core/api-request-headers.json",
      "title": "API Request Headers Schema",
      "description": "API istekleri için zorunlu header'lar",
      "type": "object",
      "required": [
        "X-Request-Id",
        "X-Correlation-Id"
      ],
      "properties": {
        "X-Request-Id": {
          "type": "string",
          "format": "uuid",
          "description": "Benzersiz istek tanımlayıcısı"
        },
        "X-Correlation-Id": {
          "type": "string",
          "format": "uuid",
          "description": "Korelasyon tanımlayıcısı"
        },
        "X-Client-Id": {
          "type": "string",
          "description": "İstemci tanımlayıcısı"
        },
        "Accept-Language": {
          "type": "string",
          "pattern": "^[a-z]{2}-[A-Z]{2}$",
          "description": "Dil tercihi (örn: tr-TR)"
        }
      }
    }
  }
}
```

## En İyi Uygulamalar

### 1. Açıklayıcı Metadata Kullanın

Her schema için anlamlı `title` ve `description` ekleyin:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "Kullanıcı Kayıt Schema'sı",
  "description": "Yeni kullanıcı kaydı için gerekli tüm alanları tanımlar"
}
```

### 2. Alan Açıklamalarını Ekleyin

Her property için `description` tanımlayın:

```json
{
  "properties": {
    "email": {
      "type": "string",
      "format": "email",
      "description": "Kullanıcının e-posta adresi. Doğrulama için kullanılacaktır."
    }
  }
}
```

### 3. Uygun Validasyon Kuralları Kullanın

İş kurallarına uygun kısıtlamalar ekleyin:

```json
{
  "amount": {
    "type": "number",
    "minimum": 1,
    "maximum": 100000,
    "multipleOf": 0.01,
    "description": "Transfer tutarı (1-100.000 arası, kuruş hassasiyetinde)"
  }
}
```

### 4. Enum Değerlerini Tercih Edin

Belirli değer kümesi olan alanlar için enum kullanın:

```json
{
  "status": {
    "type": "string",
    "enum": ["pending", "approved", "rejected", "cancelled"],
    "description": "İşlem durumu"
  }
}
```

### 5. Required Alanları Doğru Tanımlayın

Sadece gerçekten zorunlu alanları `required` listesine ekleyin:

```json
{
  "required": ["email", "password"],
  "properties": {
    "email": { "type": "string" },
    "password": { "type": "string" },
    "nickname": { "type": "string" }
  }
}
```

### 6. additionalProperties Kullanımı

Beklenmeyen alanları engellemek için `additionalProperties: false` kullanın:

```json
{
  "type": "object",
  "properties": {
    "name": { "type": "string" }
  },
  "additionalProperties": false
}
```

:::warning Dikkat
`additionalProperties: false` kullanıldığında, schema'da tanımlanmayan tüm alanlar reddedilir. Bu, veri bütünlüğü için önemlidir ancak geriye dönük uyumluluğu etkileyebilir.
:::

### 7. Schema Versiyonlamayı Takip Edin

- **Major** versiyon: Kırıcı değişiklikler (zorunlu alan ekleme, alan kaldırma)
- **Minor** versiyon: Geriye uyumlu eklemeler (opsiyonel alan ekleme)
- **Patch** versiyon: Hata düzeltmeleri (description güncellemeleri)

```json
{
  "key": "user-registration",
  "version": "2.1.0"
}
```

### 8. Anlamlı Etiketler Kullanın

Schema'ları kategorilendirmek için tutarlı etiketleme stratejisi izleyin:

```json
{
  "tags": [
    "domain:banking",
    "type:input-schema",
    "workflow:account-opening",
    "version:v2"
  ]
}
```

## Sık Karşılaşılan Hatalar

### 1. Schema Doğrulama Hatası

```
Error: JSON schema validation failed for property 'email'
```

**Çözüm**: Gönderilen verinin schema'da tanımlanan tipe ve kurallara uyduğundan emin olun.

### 2. Required Alan Eksik

```
Error: Required property 'amount' is missing
```

**Çözüm**: `required` listesindeki tüm alanların payload'da bulunduğundan emin olun.

### 3. Enum Değeri Geçersiz

```
Error: Value 'invalid' is not one of the allowed values: ['pending', 'approved', 'rejected']
```

**Çözüm**: Sadece enum'da tanımlı değerleri kullanın.

### 4. Format Hatası

```
Error: String 'not-an-email' does not match format 'email'
```

**Çözüm**: Format kurallarına uygun değerler gönderin.

## İlgili Dokümantasyon

- [📄 Workflow Tanımlaması](./flow.md) - Workflow yapısı ve bileşenleri
- [📄 Transition Yönetimi](./transition.md) - Geçişlerde schema kullanımı
- [📄 Task Yönetimi](./task.md) - Task giriş/çıkış schema'ları
- [📄 View Yönetimi](./view.md) - Form doğrulama schema'ları

Bu dokümantasyon, schema tanımlaması ve kullanımı için kapsamlı bir rehber sunmaktadır. Geliştiriciler bu rehberi takip ederek veri bütünlüğü sağlayan schema'lar oluşturabilirler.

